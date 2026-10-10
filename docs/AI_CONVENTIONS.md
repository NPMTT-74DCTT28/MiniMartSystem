# QUY TẮC VIẾT CODE DÀNH CHO AI AGENT (MINI MART SYSTEM)

Bạn là một AI Assistant hỗ trợ lập trình C# .NET 10. Khi sinh code cho dự án này, bạn PHẢI tuân thủ các quy tắc sau:

## 1. Kiến trúc chung
- Không sử dụng Repository Pattern, CQRS, hay MediatR.
- Lớp API chỉ sử dụng mô hình cơ bản: `Controller` tiêm (inject) trực tiếp `AppDbContext`.
- Lớp WinForms Client KHÔNG ĐƯỢC phép gọi trực tiếp `AppDbContext`. Mọi thao tác lấy/ghi dữ liệu phải gọi qua `BaseHttpClient`.

## 2. Quy tắc đặt tên (Naming Conventions)
- **Entities / Models:** PascalCase, danh từ số ít (VD: `Product`, `Customer`). Đặt trong project `MiniMart.Shared`.
- **DTOs:** Hậu tố là `Dto` (VD: `ProductDto`, `CreateProductDto`). Đặt trong project `MiniMart.Shared`.
- **API Controllers:** Hậu tố là `Controller`, tiền tố là danh từ số nhiều (VD: `ProductsController`). Route luôn là `[Route("api/[controller]")]`.
- **WinForms UI:** 
  - Tên Form bắt đầu bằng `frm` (VD: `frmProductManager`).
  - Controls: Nút bấm (`btnX`), Textbox (`txtX`), DataGridView (`dgvX`), ComboBox (`cbxX`).

## 3. Quy định về Entity Framework Core
- Dùng Data Annotations (như `[Required]`, `[MaxLength]`) trực tiếp trên Entity thay vì dùng Fluent API, trừ phi cấu hình khóa ngoại phức tạp.
- Luôn kiểm tra `null` khi map dữ liệu từ Entity sang DTO và ngược lại.

## 4. Xử lý Lỗi (Error Handling)
- API luôn trả về `IActionResult` (Ok, BadRequest, NotFound).
- Giao diện WinForms luôn có khối `try-catch` khi gọi API, hiển thị lỗi qua `MessageBox.Show()`.