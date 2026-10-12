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

## 2. Quy ước chung

- Cấu hình JWT nằm ở section `Jwt` trong `MiniMart.API/appsettings.json`
  (`Issuer`, `Audience`, `Key`, `ExpiryMinutes`). Khi triển khai thật, `Jwt:Key` nên
  được nạp từ biến môi trường hoặc user-secrets thay vì commit vào repo.
- Mật khẩu **không bao giờ** lưu dạng thô: dùng `PasswordHelper.Hash` khi tạo/đổi mật khẩu
  và `PasswordHelper.Verify` khi đăng nhập.
