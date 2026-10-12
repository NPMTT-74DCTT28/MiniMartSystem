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
- KHÔNG sửa tay các file do EF Core sinh (`Migrations/*.cs`, `*.Designer.cs`, `AppDbContextModelSnapshot.cs`).
  Sau khi merge/rebase phải kiểm tra bằng `dotnet ef migrations has-pending-model-changes`; nếu báo model thay
  đổi ngoài ý muốn thì khôi phục lại đúng model của migration cuối (nguồn chuẩn là file `*.Designer.cs`) rồi
  chạy `dotnet ef database update` để apply các migration còn pending.

## 4. Xử lý Lỗi (Error Handling)
- API luôn trả về `IActionResult` (Ok, BadRequest, NotFound).
- Giao diện WinForms luôn có khối `try-catch` khi gọi API, hiển thị lỗi qua `MessageBox.Show()`.

## 5. Xác thực & Bảo mật
- Endpoint đăng nhập đặt ở `AuthController` (`POST api/auth/login`) và trả về JWT access token; các endpoint nghiệp vụ cần phân quyền thì gắn `[Authorize]` / `[Authorize(Roles = "Admin")]`.
- KHÔNG bao giờ lưu mật khẩu dạng thô: dùng `PasswordHelper.Hash` khi tạo/đổi mật khẩu và `PasswordHelper.Verify` khi đăng nhập (PBKDF2 - HMAC-SHA256, salt ngẫu nhiên cho từng tài khoản).
- Khi đăng nhập thất bại, API trả cùng một thông báo cho cả trường hợp sai tên đăng nhập và sai mật khẩu để tránh dò tài khoản.
- Cấu hình JWT đọc từ section `Jwt` trong `appsettings.json`; không hardcode khóa bí mật trong code C#.

## 6. Quy trình làm việc theo Task (Workflow)
Quy trình dưới đây áp dụng cho MỌI task được giao. Khi task có kèm mã công việc (task key, ví dụ dạng `PROJECT-123` trong Jira) thì phải dùng mã đó ở tên nhánh và commit message; nếu task không cung cấp mã công việc thì bỏ qua phần liên quan tới task key, các bước còn lại vẫn giữ nguyên.

1. **Tạo nhánh:** rẽ nhánh từ `develop` (không rẽ từ `main` hay nhánh của thành viên khác). Tên nhánh mô tả ngắn gọn nội dung task và BẮT BUỘC chứa task key nếu được cung cấp, theo dạng `<loại>/<TASK-KEY>-<mô-tả-ngắn>`, ví dụ `feat/PROJECT-123-login-screen`. Việc không thuộc nghiệp vụ (tài liệu, cấu hình, dọn dẹp) dùng tiền tố `chore/` hoặc `docs/`.
2. **Khảo sát trước khi sinh code:** đọc các tài liệu trong `docs` (`PLAN.md`, `AI_CONVENTIONS.md`, `SCHEMA.md`, `API.md`, `UI_NAVIGATION.md`) và duyệt qua toàn bộ dự án để nắm quy tắc chung, các module đã có và phạm vi của mình; tuyệt đối không vượt ranh giới module của thành viên khác (xem `docs/PLAN.md`).
3. **Chia commit nhỏ:** hoàn thành từng phần nhỏ thì commit ngay, không dồn toàn bộ task vào một commit lớn.
4. **Định dạng commit:** mọi commit dùng Conventional Commits và viết bằng tiếng Việt theo dạng `<type>(<scope>): <mô tả ngắn>`, với `type` thuộc `feat`, `fix`, `refactor`, `docs`, `chore`, `style`, `test`. Phần thân commit mô tả rõ thay đổi; nếu task có task key thì ghi thêm dòng `Refs: <TASK-KEY>` ở cuối. Có thể xem `git log` để lấy mẫu.
5. **Nghiệm thu:** chạy build (`dotnet build MiniMart.slnx`), chạy các bước apply migration/kiểm thử liên quan và dọn sạch dữ liệu hay file tạm trước khi kết thúc.
6. **Bàn giao:** khi task hoàn thành, thông báo cho người dùng để họ tự tạo PR trên GitHub (KHÔNG tự push hay tạo PR nếu không được yêu cầu), viết sẵn commit message chi tiết cho lần merge với **tiêu đề chứa task key**, và liệt kê đầy đủ mọi thay đổi để người dùng kiểm tra.
