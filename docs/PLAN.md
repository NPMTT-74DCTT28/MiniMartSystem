# KẾ HOẠCH PHÁT TRIỂN ỨNG DỤNG - HỆ THỐNG QUẢN LÝ CỬA HÀNG TIỆN LỢI (MINI MART SYSTEM)

## 1. Tổng quan Dự án
- **Số thành viên:** 5 (Leader cũng tham gia xây dựng 1 chức năng cụ thể).
- **Loại ứng dụng:** Giao diện Windows Forms (Client) kết hợp với ASP.NET Core Web API (Server).
- **Nền tảng:** C# / .NET 10.
- **Mô hình kiến trúc:** Client–Server đặt trong cùng một Monorepo. Kiến trúc thiết kế đơn giản, phù hợp với trình độ sinh viên, không áp dụng các pattern quá phức tạp của doanh nghiệp lớn (như CQRS, Microservices).
- **Luồng dữ liệu:** Giao diện WinForms không kết nối trực tiếp với Database mà phải gọi qua các endpoint của Web API.

## 2. Chiến lược Cơ sở dữ liệu (Database)
- **Hệ quản trị CSDL:** SQL Server.
- **ORM:** Entity Framework Core (Tiếp cận theo hướng Code-First).
- **Môi trường triển khai (2 Options song song):**
  1. **Development (Cá nhân):** Trong quá trình code tính năng riêng, mỗi thành viên chạy EF Core Migrations (`Update-Database`) để tạo Local DB trên máy cá nhân.
  2. **Integration (Tập trung):** Dùng SQL Server trên máy của Leader làm DB tập trung. Các thành viên kết nối vào DB này thông qua mạng LAN ảo Tailscale khi cần test tích hợp chéo các module.

## 3. Phân chia công việc
Dự án được chia theo module chức năng dọc, mỗi thành viên phụ trách toàn bộ từ Database -> API -> UI cho một thực thể (Entity) nhất định.

- **Member 1 (Leader) - Setup & Hệ thống & Thống kê:**
  - Khởi tạo Base Project, cấu hình DbContext, xử lý lớp HttpClient dùng chung ở WinForms.
  - Quản lý bảng `Users` (Tài khoản nhân viên, phân quyền cơ bản).
  - API & UI cho chức năng Đăng nhập.
  - UI Dashboard thống kê doanh thu cơ bản (gọi API tổng hợp dữ liệu từ các bảng khác).
- **Member 2 - Quản lý Sản phẩm (Catalog):**
  - Quản lý các bảng: `Products`, `Categories`.
  - Làm API & UI thực hiện CRUD danh mục và sản phẩm.
- **Member 3 - Quản lý Khách hàng (Customers):**
  - Quản lý bảng: `Customers`.
  - Làm API & UI thực hiện CRUD khách hàng, theo dõi điểm tích lũy/hạng thành viên.
- **Member 4 - Bán hàng (POS / Orders):**
  - Quản lý các bảng: `Orders`, `OrderDetails`.
  - Làm API tạo Hóa đơn bán lẻ (trừ số lượng tồn kho bên bảng Products).
  - Làm UI màn hình Bán hàng tại quầy (chọn sản phẩm, tính tiền, in hóa đơn).
- **Member 5 - Nhập kho (Inventory / Suppliers):**
  - Quản lý các bảng: `Suppliers`, `ImportReceipts`, `ImportReceiptDetails`.
  - Làm API tạo Phiếu nhập kho (cộng dồn số lượng tồn kho bên bảng Products).
  - Làm UI màn hình Quản lý nhà cung cấp và Tạo phiếu nhập.

## 4. Trạng thái Hiện tại & Nhiệm vụ của AI Agent
- **Trạng thái dự án:** Khung xương Solution (gồm 3 project `Shared`, `API`, `Client`), `AppDbContext`, `BaseHttpClient`, và chuỗi kết nối đã được Leader thiết lập hoàn chỉnh.
- **Form chung điều hướng:** `frmMain` (`MiniMart.Client/Forms/frmMain.cs`) là form khởi động, hiển thị thanh điều hướng tới màn hình của cả 5 thành viên. Danh sách màn hình tập trung tại `MiniMart.Client/Navigation/NavigationCatalog.cs`; chi tiết xem `docs/UI_NAVIGATION.md`.
- **Màn hình đăng nhập:** `frmLogin` (`MiniMart.Client/Forms/frmLogin.cs`) chạy trước `frmMain`, dùng `POST api/auth/login` và lưu JWT vào `MiniMart.Client/Security/AuthSession.cs`. `BaseHttpClient` tự gắn Bearer token nên mọi màn hình chức năng gọi API đã xác thực được ngay.
- **Quản lý tài khoản:** API `api/users` (CRUD, chỉ Admin) và màn hình `frmAccountManager` đã hoàn thiện; API tự tạo tài khoản `admin` mặc định khi database trống (xem `docs/API.md`).
- **Dashboard thống kê:** API `api/dashboard` tổng hợp số liệu từ nhiều bảng (hóa đơn, sản phẩm, khách hàng, nhân viên, phiếu nhập kho) và màn hình `frmDashboard` hiển thị thẻ số liệu, biểu đồ doanh thu cùng các bảng xếp hạng (xem `docs/API.md`).
- **Nhiệm vụ của AI:** 
  - Đóng vai trò là trợ lý lập trình, hỗ trợ từng thành viên hoàn thiện chức năng dọc (từ DB đến UI) dựa trên phân công ở Mục 3.
  - Khi được yêu cầu viết code cho một module, chỉ tập trung sinh Models, DTOs, API Controllers và Code-behind (WinForms) cho riêng module đó.
- **Ranh giới (Cấm kỵ):** 
  - KHÔNG tự ý khởi tạo lại project, KHÔNG ghi đè các cấu hình hệ thống (`Program.cs`, `appsettings.json`) trừ khi được yêu cầu rõ ràng.
  - KHÔNG can thiệp vào logic hoặc database schema thuộc phạm vi quản lý của thành viên khác.