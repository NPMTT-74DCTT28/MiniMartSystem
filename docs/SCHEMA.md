# CẤU TRÚC CƠ SỞ DỮ LIỆU (DATABASE SCHEMA)

Dự án sử dụng Entity Framework Core Code-First. Dưới đây là các thực thể (Entities) chính và các thuộc tính (properties) thống nhất để AI Agent sử dụng khi sinh code:

- **User**: Id (Guid), Username (string), PasswordHash (string), Role (string - "Admin" | "Staff"), IsActive (bool).
- **Category**: Id (int), Name (string), Description (string).
- **Product**: Id (int), Name (string), CategoryId (int), Price (decimal), StockQuantity (int), ImageUrl (string). Khóa ngoại trỏ tới `Category`.
- **Customer**: Id (int), FullName (string), PhoneNumber (string), TotalPoints (int).
- **Order**: Id (int), CustomerId (int, nullable), UserId (Guid), OrderDate (DateTime), TotalAmount (decimal).
- **OrderDetail**: Id (int), OrderId (int), ProductId (int), Quantity (int), UnitPrice (decimal).
- **Supplier**: Id (int), Name (string), Phone (string), Address (string).
- **ImportReceipt**: Id (int), SupplierId (int), UserId (Guid), ImportDate (DateTime), TotalAmount (decimal).
- **ImportReceiptDetail**: Id (int), ImportReceiptId (int), ProductId (int), Quantity (int), ImportPrice (decimal).