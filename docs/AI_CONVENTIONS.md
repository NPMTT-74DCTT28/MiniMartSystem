# QUY TẮC VIẾT CODE DÀNH CHO AI AGENT (MINI MART SYSTEM)

Bạn là một AI Assistant hỗ trợ lập trình C# .NET 10. Khi sinh code cho dự án này, bạn PHẢI tuân thủ các quy tắc sau:

## 1. Kiến trúc chung
- Không sử dụng Repository Pattern, CQRS, hay MediatR.
- Lớp API chỉ sử dụng mô hình cơ bản: `Controller` tiêm (inject) trực tiếp `AppDbContext`.
- Lớp WinForms Client KHÔNG ĐƯỢC phép gọi trực tiếp `AppDbContext`. Mọi thao tác lấy/ghi dữ liệu phải gọi qua `BaseHttpClient`.

## 2. Quy tắc đặt tên (Naming Conventions)
- **Entities / Models:** PascalCase, danh từ số ít (VD: `Product`, `Customer`). Đặt trong project `MiniMart.Shared`.
- **DTOs:** Hậu tố là `Dto` (VD: `ProductDto`, `CreateProductDto`). Đặt trong project `MiniMart.Shared`.
- **Enums:** PascalCase, danh từ số ít và mô tả rõ ý nghĩa (VD: `UserRole`). Đặt trong thư mục `Enums` của project `MiniMart.Shared`. KHÔNG hardcode chuỗi cho các tập giá trị cố định (VD: vai trò `Admin` / `Staff`) mà phải dùng enum, và gắn `JsonStringEnumConverter` để JSON trao đổi với Client vẫn là chuỗi như contract.
- **API Controllers:** Hậu tố là `Controller`, tiền tố là danh từ số nhiều (VD: `ProductsController`). Route luôn là `[Route("api/[controller]")]`.
- **WinForms UI:** 
  - Tên Form bắt đầu bằng `frm` (VD: `frmProductManager`).
  - Controls: Nút bấm (`btnX`), Textbox (`txtX`), DataGridView (`dgvX`), ComboBox (`cbxX`).
  - Mọi màn hình chức năng phải được đăng ký vào `MiniMart.Client/Navigation/NavigationCatalog.cs`
    để điều hướng được từ form chung `frmMain`; xem `docs/UI_NAVIGATION.md`. KHÔNG tự mở form khác
    bằng `Show()`/`ShowDialog()` trong màn hình con.

## 3. Quy định về Entity Framework Core
- Dùng Data Annotations (như `[Required]`, `[MaxLength]`) trực tiếp trên Entity thay vì dùng Fluent API, trừ phi cấu hình khóa ngoại phức tạp.
- Với property kiểu enum được lưu trong DB dạng chuỗi, cấu hình bằng Fluent API (`HasConversion<string>()` + `HasMaxLength(...)`) đúng theo kiểu cột đã có, nhằm tránh phát sinh migration không cần thiết.
- Luôn kiểm tra `null` khi map dữ liệu từ Entity sang DTO và ngược lại.

## 4. Xử lý Lỗi (Error Handling)
- API luôn trả về `IActionResult` (Ok, BadRequest, NotFound).
- Giao diện WinForms luôn có khối `try-catch` khi gọi API, hiển thị lỗi qua `MessageBox.Show()`.

## 5. Xác thực & Bảo mật
- Endpoint đăng nhập đặt ở `AuthController` (`POST api/auth/login`) và trả về JWT access token; các endpoint nghiệp vụ cần phân quyền thì gắn `[Authorize]` / `[Authorize(Roles = "Admin")]`.
- KHÔNG bao giờ lưu mật khẩu dạng thô: dùng `PasswordHelper.Hash` khi tạo/đổi mật khẩu và `PasswordHelper.Verify` khi đăng nhập (PBKDF2 - HMAC-SHA256, salt ngẫu nhiên cho từng tài khoản).
- Khi đăng nhập thất bại, API trả cùng một thông báo cho cả trường hợp sai tên đăng nhập và sai mật khẩu để tránh dò tài khoản.
- Cấu hình JWT đọc từ section `Jwt` trong `appsettings.json`; không hardcode khóa bí mật trong code C#.
