# ĐIỀU HƯỚNG MÀN HÌNH (UI NAVIGATION) - MINI MART SYSTEM

Tài liệu này ghi lại cách form chung `frmMain` điều hướng qua lại giữa các màn hình chức năng,
để cả 5 thành viên đăng ký màn hình của mình vào cùng một chỗ.

## 1. Form chung `frmMain`

- Vị trí: `MiniMart.Client/Forms/frmMain.cs`.
- Bố cục: thanh điều hướng bên trái (liệt kê màn hình theo nhóm) + vùng nội dung bên phải
  chỉ hiển thị **một** màn hình tại một thời điểm.
- Đây là form khởi động của ứng dụng (xem `MiniMart.Client/Program.cs`).
- Màn hình con được tạo **lười** ở lần điều hướng đầu tiên và được giữ lại trong bộ nhớ,
  nên quay lại màn hình cũ không phải tải lại dữ liệu.
- Form chung tự cấu hình form con khi nhúng (`TopLevel = false`, `FormBorderStyle = None`,
  `Dock = Fill`) và tự bỏ `MinimumSize`/`MaximumSize`, vì vậy form con **vẫn chạy độc lập được**
  mà không cần viết thêm gì.

## 2. Đăng ký một màn hình mới (3 bước)

1. Tạo form của module trong `MiniMart.Client/Forms`, tên bắt đầu bằng `frm` (VD: `frmPosScreen`).
   Form phải là `Form` thường, dữ liệu lấy qua `BaseHttpClient` như quy định trong
   `docs/AI_CONVENTIONS.md`.
2. Mở `MiniMart.Client/Navigation/NavigationCatalog.cs` và thêm một `NavigationItem` vào `Items`:

   ```csharp
   new NavigationItem(
       Key: "pos",
       Group: "Bán hàng",
       Title: "Bán hàng tại quầy (POS)",
       Description: "Chọn sản phẩm, tính tiền và in hóa đơn.",
       FormFactory: () => new frmPosScreen(),
       Owner: "Member 4"),
   ```

3. Không cần sửa gì thêm: `IsReady` mặc định là `true` nên dấu `*` (đang phát triển) tự mất.

Nếu module mới chỉ dành cho `Admin`, thêm `RequiredRole: UserRole.Admin`.

## 3. Các màn hình đang đăng ký

| Key | Nhóm | Màn hình | Trạng thái | Phụ trách |
|---|---|---|---|---|
| `dashboard` | Tổng quan | Tổng quan & thống kê | **Hoàn thiện** | Member 1 (Leader) |
| `accounts` | Hệ thống | Quản lý tài khoản nhân viên (Admin) | **Hoàn thiện** | Member 1 (Leader) |
| `catalog` | Danh mục | Quản lý danh mục & sản phẩm (Admin) | **Hoàn thiện** | Member 2 |
| `customers` | Khách hàng | Quản lý khách hàng | Đang phát triển | Member 3 |
| `pos` | Bán hàng | Bán hàng tại quầy (POS) | **Hoàn thiện** | Member 4 |
| `inventory` | Nhập kho | Nhập kho & nhà cung cấp (Admin) | **Hoàn thiện** | Member 5 |

Màn hình chưa hoàn thiện dùng `frmPlaceholder` (tên hiển thị kèm dấu `*`) để thanh điều hướng
có đủ chỗ cho cả 5 thành viên ngay từ đầu.

> Màn hình **Đăng nhập** (`frmLogin`) không nằm trên thanh điều hướng: nó chạy trước `frmMain`
> dưới dạng hộp thoại (xem mục 6), nên không có mục nào trong `NavigationCatalog`.

## 4. Phân quyền trên thanh điều hướng

- `frmMain` nhận `UserDto` của tài khoản đang đăng nhập qua constructor:
  `new frmMain(loggedInUser)`. Khi chưa có màn hình đăng nhập, chạy `new frmMain()` (chế độ
  phát triển) sẽ hiển thị **toàn bộ** màn hình.
- Màn hình có `RequiredRole` sẽ bị ẩn với vai trò khác, và `NavigateTo(key)` trả về `false`
  nếu cố điều hướng vào màn hình không được phép.
- Phân quyền ở Client chỉ là lớp trải nghiệm; API vẫn phải kiểm tra bằng `[Authorize]`.

## 5. Điều hướng bằng code

```csharp
var shell = new frmMain(currentUser);
shell.NavigateTo("catalog");   // true nếu màn hình tồn tại và được phép truy cập
```

`frmMain.LogoutRequested` phát sinh khi người dùng bấm nút **Đăng xuất**; màn hình đăng nhập
đăng ký sự kiện này để xử lý việc đổi tài khoản. Nếu chưa có nơi xử lý, form chung sẽ tự đóng.

## 6. Luồng đăng nhập - form chung - đăng xuất

`MiniMart.Client/Program.cs` điều khiển vòng đời của ứng dụng:

```
frmLogin (ShowDialog) --OK--> frmMain --LogoutRequested--> quay lại frmLogin
                        |--đóng bằng nút X--> thoát ứng dụng
```

1. `frmLogin` gọi `POST api/auth/login` qua `BaseHttpClient`. Thành công thì lưu phiên vào
   `AuthSession.SignIn(...)` rồi đóng với `DialogResult.OK`.
2. `Program.cs` mở `frmMain(AuthSession.CurrentUser)` nên thanh điều hướng tự ẩn/hiện theo vai trò.
3. Nút **Đăng xuất** gọi `AuthSession.SignOut()` và đóng form chung; vòng lặp mở lại `frmLogin`
   để đổi tài khoản mà không phải khởi động lại ứng dụng.

**Lưu ý cho màn hình chức năng:** `BaseHttpClient` tự gắn `Authorization: Bearer <token>` từ
`AuthSession`, nên các màn hình chỉ cần gọi API như bình thường. Khi chạy form độc lập (không qua
`frmLogin`) thì chưa có token và các API có `[Authorize]` sẽ trả `401`.
