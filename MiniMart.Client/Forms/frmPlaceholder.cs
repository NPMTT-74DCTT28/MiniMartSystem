namespace MiniMart.Client.Forms;

/// <summary>
/// Màn hình giữ chỗ cho các module chưa hoàn thiện.
/// Nhờ đó thanh điều hướng của <see cref="frmMain"/> hiển thị đủ màn hình của cả 5 thành viên
/// ngay từ đầu; khi module hoàn thiện chỉ cần thay factory trong <c>NavigationCatalog</c>.
/// </summary>
public class frmPlaceholder : Form
{
    /// <summary>Khởi tạo màn hình giữ chỗ.</summary>
    /// <param name="moduleTitle">Tên màn hình, cũng dùng làm tiêu đề khi chạy độc lập.</param>
    /// <param name="owner">Thành viên phụ trách module.</param>
    public frmPlaceholder(string moduleTitle, string? owner)
    {
        Text = moduleTitle;
        BackColor = Color.White;
        FormBorderStyle = FormBorderStyle.None;
        Padding = new Padding(24);

        var note = string.IsNullOrWhiteSpace(owner)
            ? "Màn hình này đang được phát triển."
            : $"Màn hình này đang được phát triển bởi {owner}.";

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Margin = new Padding(0),
            Text = moduleTitle
        }, 0, 1);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(108, 117, 125),
            Margin = new Padding(0, 8, 0, 0),
            Text = note
        }, 0, 2);

        Controls.Add(layout);
    }
}
