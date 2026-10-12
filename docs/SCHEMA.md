# CẤU TRÚC CƠ SỞ DỮ LIỆU (DATABASE SCHEMA)

Dự án sử dụng Entity Framework Core Code-First. Dưới đây là các thực thể (Entities) chính và các thuộc tính (properties) thống nhất để AI Agent sử dụng khi sinh code:

- **User**: Id (Guid), Username (string), PasswordHash (string), Role (enum `UserRole` - `Staff` | `Admin`), IsActive (bool).
- **Category**: Id (int), Name (string), Description (string).
- **Product**: Id (int), Name (string), CategoryId (int), Price (decimal), StockQuantity (int), ImageUrl (string). Khóa ngoại trỏ tới `Category`.
- **Customer**: Id (int), FullName (string), PhoneNumber (string), TotalPoints (int).
- **Order**: Id (int), CustomerId (int, nullable), UserId (Guid), OrderDate (DateTime), TotalAmount (decimal).
- **OrderDetail**: Id (int), OrderId (int), ProductId (int), Quantity (int), UnitPrice (decimal).
- **Supplier**: Id (int), Name (string), Phone (string), Address (string).
- **ImportReceipt**: Id (int), SupplierId (int), UserId (Guid), ImportDate (DateTime), TotalAmount (decimal).
- **ImportReceiptDetail**: Id (int), ImportReceiptId (int), ProductId (int), Quantity (int), ImportPrice (decimal).

## CÁC ENUM DÙNG CHUNG

Enum dùng chung được khai báo trong project `MiniMart.Shared`, thư mục `Enums`, và gắn `JsonStringEnumConverter` để JSON trao đổi giữa API và Client vẫn là chuỗi (không đổi contract với Client).

- **UserRole**: `Staff = 0` (mặc định khi tạo tài khoản), `Admin = 1` (toàn quyền, gồm quản lý tài khoản).
  - Dùng cho `User.Role` và trường `Role` của các DTO User (`UserDto`, `CreateUserDto`, `UpdateUserDto`).
  - Trong database, `Role` vẫn được lưu dưới dạng chuỗi (`nvarchar(20)`) nhờ cấu hình `HasConversion<string>()` trong `AppDbContext`, nên dữ liệu hiện có không bị ảnh hưởng.