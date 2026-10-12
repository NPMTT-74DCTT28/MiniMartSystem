using System.Text.Json.Serialization;

namespace MiniMart.Shared.Enums
{
    /// <summary>
    /// Vai trò của tài khoản nhân viên trong hệ thống.
    /// Thay thế cho việc hardcode chuỗi "Admin" / "Staff" trước đây.
    /// </summary>
    /// <remarks>
    /// Gắn <see cref="JsonStringEnumConverter"/> để JSON trao đổi giữa API và Client
    /// vẫn là chuỗi ("Admin" | "Staff") như contract cũ, không đổi định dạng dữ liệu.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
    public enum UserRole
    {
        /// <summary>
        /// Nhân viên (quyền hạn giới hạn). Đây là giá trị mặc định khi tạo tài khoản.
        /// </summary>
        Staff = 0,

        /// <summary>
        /// Quản trị viên (toàn quyền quản lý hệ thống, bao gồm quản lý tài khoản).
        /// </summary>
        Admin = 1
    }
}
