# DANH SÁCH API ENDPOINTS (MINI MART SYSTEM)

Tài liệu này ghi lại contract của các endpoint Web API để Client (WinForms) và các thành viên khác dùng thống nhất.
Route tuân theo quy ước `[Route("api/[controller]")]` trong `docs/AI_CONVENTIONS.md`.

## 1. Xác thực (Auth)

### POST `/api/auth/login`

Đăng nhập bằng tài khoản nhân viên, trả về JWT access token dùng cho các request tiếp theo.

**Request body** (`LoginRequestDto` — `MiniMart.Shared/DTOs/Auth/LoginRequestDto.cs`)

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|
| `username` | string | Có | Tối đa 50 ký tự |
| `password` | string | Có | Mật khẩu dạng thô, gửi qua HTTPS |

**Response 200** (`LoginResponseDto`)

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-10-12T08:54:49Z",
  "user": {
    "id": "8849ef07-2b98-40e6-926b-99e6eb1a1021",
    "username": "admin",
    "role": "Admin",
    "isActive": true
  }
}
```

- `role` trả về dạng chuỗi `"Admin"` hoặc `"Staff"` (enum `UserRole` gắn `JsonStringEnumConverter`).
- `user` không chứa `PasswordHash`.

**Các mã lỗi**

| HTTP | Khi nào | Body |
|---|---|---|
| `400 Bad Request` | Thiếu `username`/`password` hoặc vượt quá độ dài cho phép | `ValidationProblemDetails` |
| `401 Unauthorized` | Sai tên đăng nhập **hoặc** sai mật khẩu (không phân biệt để tránh dò tài khoản) | `{ "message": "Tên đăng nhập hoặc mật khẩu không đúng." }` |
| `403 Forbidden` | Đúng thông tin nhưng tài khoản có `IsActive = false` | `{ "message": "Tài khoản đã bị vô hiệu hóa." }` |

**Cách Client sử dụng token**

```
Authorization: Bearer <accessToken>
```

Token chứa các claim `sub` (Id), `unique_name` (Username) và `role` (`Admin` | `Staff`),
nên API có thể phân quyền bằng `[Authorize(Roles = "Admin")]`.

### Tài khoản Admin mặc định

Khi API khởi động, `MiniMart.API/Data/DbSeeder.cs` tạo sẵn tài khoản quản trị nếu database chưa có,
nhờ đó có thể đăng nhập ngay sau khi apply migration:

| Trường | Giá trị |
|---|---|
| `username` | `admin` |
| `password` | `Admin@123` |
| `role` | `Admin` |

Mật khẩu vẫn được lưu dưới dạng đã băm (PBKDF2) như mọi tài khoản khác. Đây là tài khoản dùng cho
môi trường phát triển — **phải đổi mật khẩu trước khi triển khai thật**. Hàm seed an toàn khi gọi
lặp lại nên khởi động API nhiều lần không tạo thêm tài khoản.

## 2. Quản lý tài khoản nhân viên (Users)

Toàn bộ endpoint dưới đây gắn `[Authorize(Roles = "Admin")]`: chỉ tài khoản `Admin` gọi được,
các vai trò khác nhận `403 Forbidden`, chưa đăng nhập nhận `401 Unauthorized`.
`PasswordHash` không bao giờ được trả về trong response.

| Method | Route | Mô tả | Mã lỗi |
|---|---|---|---|
| `GET` | `/api/users` | Danh sách tài khoản (sắp xếp theo `username`) | `401`, `403` |
| `GET` | `/api/users/{id}` | Chi tiết một tài khoản theo `Id` (Guid) | `401`, `403`, `404` |
| `POST` | `/api/users` | Tạo tài khoản mới (`CreateUserDto`) | `400`, `401`, `403`, `409` |
| `PUT` | `/api/users/{id}` | Cập nhật tài khoản (`UpdateUserDto`) | `400`, `401`, `403`, `404`, `409` |
| `DELETE` | `/api/users/{id}` | Xóa tài khoản | `400`, `401`, `403`, `404` |

**Request body khi tạo tài khoản** (`CreateUserDto`)

```json
{
  "username": "nhanvien01",
  "password": "Staff@123",
  "role": "Staff",
  "isActive": true
}
```

`role` nhận chuỗi `"Admin"` hoặc `"Staff"` (enum `UserRole` gắn `JsonStringEnumConverter`).
Trong `UpdateUserDto`, bỏ trống `password` (hoặc để `null`) nghĩa là **giữ nguyên mật khẩu cũ**.

**Quy tắc nghiệp vụ được API chặn**

| Trường hợp | HTTP | Thông báo |
|---|---|---|
| `username` đã tồn tại (khi tạo hoặc đổi tên) | `409 Conflict` | `Tên đăng nhập '<username>' đã được sử dụng.` |
| Tự vô hiệu hóa, tự hạ quyền hoặc tự xóa tài khoản đang đăng nhập | `400 Bad Request` | `Không thể ... tài khoản đang đăng nhập.` |
| Vô hiệu hóa, hạ quyền hoặc xóa quản trị viên đang hoạt động cuối cùng | `400 Bad Request` | `Không thể ... quản trị viên cuối cùng của hệ thống.` |

> Các ràng buộc trên giúp không làm mất quyền quản trị của hệ thống, đồng thời việc xóa tài khoản
> đang đăng nhập cũng bị chặn để tránh mất phiên giữa chừng.

## 3. Nhà cung cấp và phiếu nhập kho (Suppliers / ImportReceipts)

Toàn bộ endpoint dưới đây chỉ dành cho `Admin`. Client không gửi `UserId`, `ImportDate` hoặc
`TotalAmount` khi tạo phiếu nhập: API lấy nhân viên từ JWT, đặt thời gian UTC và tính tổng tiền
từ các dòng chi tiết. Tồn kho sản phẩm được cộng trong cùng giao dịch với phiếu nhập.

| Method | Route | Mô tả | Mã lỗi |
|---|---|---|---|
| `GET` | `/api/suppliers` | Danh sách nhà cung cấp (sắp xếp theo tên) | `401`, `403` |
| `GET` | `/api/suppliers/{id}` | Chi tiết nhà cung cấp | `401`, `403`, `404` |
| `POST` | `/api/suppliers` | Tạo nhà cung cấp (`CreateSupplierDto`) | `400`, `401`, `403` |
| `PUT` | `/api/suppliers/{id}` | Cập nhật nhà cung cấp (`UpdateSupplierDto`) | `400`, `401`, `403`, `404` |
| `DELETE` | `/api/suppliers/{id}` | Xóa nhà cung cấp chưa có phiếu nhập | `401`, `403`, `404`, `409` |
| `GET` | `/api/importreceipts/{id}` | Chi tiết phiếu nhập và các dòng sản phẩm | `401`, `403`, `404` |
| `POST` | `/api/importreceipts` | Tạo phiếu nhập (`CreateImportReceiptDto`) | `400`, `401`, `403` |

**Request body khi tạo nhà cung cấp** (`CreateSupplierDto`)

```json
{
  "name": "Nhà cung cấp A",
  "phone": "0901234567",
  "address": "Hà Nội"
}
```

**Request body khi tạo phiếu nhập** (`CreateImportReceiptDto`)

```json
{
  "supplierId": 1,
  "details": [
    {
      "productId": 1,
      "quantity": 10,
      "importPrice": 12000.00
    }
  ]
}
```

Phiếu nhập phải có ít nhất một dòng; `quantity` phải lớn hơn 0 và `importPrice` không được âm.
API trả `409 Conflict` nếu xóa nhà cung cấp đã được dùng trong phiếu nhập để bảo toàn lịch sử.

## 4. Quy ước chung

- Cấu hình JWT nằm ở section `Jwt` trong `MiniMart.API/appsettings.json`
  (`Issuer`, `Audience`, `Key`, `ExpiryMinutes`). Khi triển khai thật, `Jwt:Key` nên
  được nạp từ biến môi trường hoặc user-secrets thay vì commit vào repo.
- Mật khẩu **không bao giờ** lưu dạng thô: dùng `PasswordHelper.Hash` khi tạo/đổi mật khẩu
  và `PasswordHelper.Verify` khi đăng nhập.

## 5. Cổng chạy API & cách Client kết nối

Cổng chuẩn (khai báo trong `MiniMart.API/Properties/launchSettings.json`):

| Profile | Địa chỉ API |
|---|---|
| `http` | `http://localhost:5000` |
| `https` | `https://localhost:5001` và `http://localhost:5000` |

Profile `https` bind **cả hai** cổng, nên Client mặc định gọi `http://localhost:5000/`
vẫn hoạt động với cả hai profile: khi API chạy bằng HTTPS, request vào cổng 5000 sẽ được
`UseHttpsRedirection()` chuyển sang `https://localhost:5001`.

`BaseHttpClient` (`MiniMart.Client/HttpClients/BaseHttpClient.cs`) xác định địa chỉ API theo
thứ tự ưu tiên sau:

1. Tham số truyền vào constructor: `new BaseHttpClient("https://localhost:5001/")`.
2. Biến môi trường `MINIMART_API_BASE_URL` (dùng khi muốn gọi thẳng HTTPS hoặc trỏ vào API
   tập trung qua Tailscale mà không phải build lại Client).
3. Mặc định `http://localhost:5000/`.

Ví dụ chạy Client gọi thẳng HTTPS:

```powershell
$env:MINIMART_API_BASE_URL = "https://localhost:5001/"
dotnet run --project MiniMart.Client
```

> Nếu API chỉ chạy profile `http` thì không có cổng HTTPS nào được mở, vì vậy đừng đặt
> mặc định của Client là `https://localhost:5001/`; hãy dùng biến môi trường khi cần.

## 6. Xác thực token ở phía Client

- `MiniMart.Client/Security/AuthSession.cs` là nơi duy nhất giữ access token và tài khoản đang
  đăng nhập trong bộ nhớ; `frmLogin` gọi `AuthSession.SignIn(...)` sau khi đăng nhập thành công và
  nút **Đăng xuất** của `frmMain` gọi `AuthSession.SignOut()`.
- `BaseHttpClient` tự gắn header `Authorization: Bearer <token>` vào mọi request thông qua
  `AuthTokenHandler`, nên màn hình chức năng **không cần** truyền token thủ công.
- Token hết hạn (mặc định 120 phút, xem `Jwt:ExpiryMinutes`) thì API trả `401`; khi đó cần đăng nhập
  lại để lấy token mới.
