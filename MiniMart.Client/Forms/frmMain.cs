using MiniMart.Client.Navigation;
using MiniMart.Shared.DTOs.Users;

namespace MiniMart.Client.Forms;

/// <summary>
/// Form chung (shell) dùng để điều hướng qua lại giữa các màn hình chức năng của toàn bộ thành viên.
/// </summary>
/// <remarks>
/// <para>
/// Bố cục gồm thanh điều hướng bên trái (liệt kê màn hình theo nhóm) và vùng nội dung bên phải
/// chỉ hiển thị MỘT màn hình tại một thời điểm. Danh sách màn hình lấy từ
/// <see cref="NavigationCatalog"/>, nên thành viên muốn thêm màn hình của mình chỉ cần đăng ký
/// một dòng trong danh mục đó.
/// </para>
/// <para>
/// Màn hình con được tạo lười (lazy) ở lần điều hướng đầu tiên và được giữ lại trong bộ nhớ
/// để không phải tải lại dữ liệu khi người dùng quay về màn hình cũ.
/// </para>
/// </remarks>
public class frmMain : Form
{
    private static readonly Color SidebarBackColor = Color.FromArgb(31, 41, 55);
    private static readonly Color SidebarHoverColor = Color.FromArgb(55, 65, 81);
    private static readonly Color SidebarTextColor = Color.FromArgb(209, 213, 219);
    private static readonly Color GroupTextColor = Color.FromArgb(107, 114, 128);
    private static readonly Color ActiveBackColor = Color.FromArgb(37, 99, 235);
    private static readonly Color HeaderTextColor = Color.FromArgb(17, 24, 39);
    private static readonly Color MutedTextColor = Color.FromArgb(107, 114, 125);
    private static readonly Color BorderColor = Color.FromArgb(229, 231, 235);
    private static readonly Font RegularFont = new("Segoe UI", 9F, FontStyle.Regular);
    private static readonly Font BoldFont = new("Segoe UI", 9F, FontStyle.Bold);

    private const int SidebarWidth = 288;
    private const int NavButtonWidth = 254;
    private const int HeaderHeight = 94;

    private readonly FlowLayoutPanel _navPanel = new()
    {
        AutoScroll = true,
        BackColor = SidebarBackColor,
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        Padding = new Padding(10, 6, 10, 6),
        WrapContents = false
    };

    private readonly Panel _contentPanel = new()
    {
        BackColor = Color.White,
        Dock = DockStyle.Fill
    };

    private readonly Label _lblModuleTitle = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold),
        ForeColor = HeaderTextColor,
        Margin = new Padding(0),
        Text = "Chọn một màn hình ở thanh điều hướng bên trái"
    };

    private readonly Label _lblModuleDescription = new()
    {
        AutoSize = true,
        ForeColor = MutedTextColor,
        Margin = new Padding(0, 4, 0, 0),
        MaximumSize = new Size(900, 0)
    };

    private readonly Dictionary<string, Form> _formCache = new();
    private readonly Dictionary<string, Button> _navButtons = new();
    private Button? _activeNavButton;

    /// <summary>Khởi tạo form ở chế độ phát triển (chưa có thông tin đăng nhập, hiển thị toàn bộ màn hình).</summary>
    public frmMain()
        : this(null)
    {
    }

    /// <summary>Khởi tạo form với tài khoản đang đăng nhập, dùng để ẩn/hiện màn hình theo vai trò.</summary>
    /// <param name="currentUser">
    /// Tài khoản đang đăng nhập, hoặc <c>null</c> khi chạy ở chế độ phát triển.
    /// </param>
    public frmMain(UserDto? currentUser)
    {
        CurrentUser = currentUser;

        Text = "MiniMart - Hệ thống quản lý cửa hàng tiện lợi";
        BackColor = Color.White;
        Font = RegularFont;
        MinimumSize = new Size(1024, 640);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        BuildLayout();
        BuildNavigation();
    }

    /// <summary>Tài khoản đang đăng nhập, hoặc <c>null</c> khi chạy ở chế độ phát triển.</summary>
    public UserDto? CurrentUser { get; }

    /// <summary>
    /// Phát sinh khi người dùng bấm nút đăng xuất. Nếu không có nơi đăng ký xử lý,
    /// form chung sẽ tự đóng lại.
    /// </summary>
    public event EventHandler? LogoutRequested;

    /// <summary>Điều hướng tới màn hình theo khóa đăng ký trong <see cref="NavigationCatalog"/>.</summary>
    /// <param name="moduleKey">Khóa màn hình (VD: <c>"catalog"</c>).</param>
    /// <returns><c>true</c> khi tìm thấy và hiển thị được màn hình.</returns>
    public bool NavigateTo(string moduleKey)
    {
        var item = NavigationCatalog.Items.FirstOrDefault(
            candidate => string.Equals(candidate.Key, moduleKey, StringComparison.OrdinalIgnoreCase));

        if (item is null || !item.IsVisibleFor(CurrentUser))
        {
            return false;
        }

        NavigateTo(item);
        return true;
    }

    /// <summary>Giải phóng các màn hình con đã tạo trước khi đóng form chung.</summary>
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        foreach (var form in _formCache.Values)
        {
            form.Dispose();
        }

        _formCache.Clear();
        base.OnFormClosed(e);
    }

    private void BuildLayout()
    {
        var header = new Panel
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 14, 24, 10)
        };
        var headerLayout = new FlowLayoutPanel
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        headerLayout.Controls.Add(_lblModuleTitle);
        headerLayout.Controls.Add(_lblModuleDescription);
        header.Controls.Add(headerLayout);
        header.Controls.Add(new Panel
        {
            BackColor = BorderColor,
            Dock = DockStyle.Bottom,
            Height = 1
        });

        // TableLayoutPanel quy định rõ chiều cao phần tiêu đề và phần nội dung,
        // tránh việc hai control cùng Dock=Fill chồng lên nhau.
        var contentHost = new TableLayoutPanel
        {
            BackColor = Color.White,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        contentHost.RowStyles.Add(new RowStyle(SizeType.Absolute, HeaderHeight));
        contentHost.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contentHost.Controls.Add(header, 0, 0);
        contentHost.Controls.Add(_contentPanel, 0, 1);

        var root = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            RowCount = 1
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SidebarWidth));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildSidebar(), 0, 0);
        root.Controls.Add(contentHost, 1, 0);

        Controls.Add(root);
    }

    private Control BuildSidebar()
    {
        var brand = new Panel
        {
            BackColor = SidebarBackColor,
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 14, 14, 8)
        };
        var brandLayout = new FlowLayoutPanel
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        brandLayout.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.White,
            Margin = new Padding(0),
            Text = "MiniMart System"
        });
        brandLayout.Controls.Add(new Label
        {
            AutoSize = true,
            ForeColor = SidebarTextColor,
            Margin = new Padding(0, 6, 0, 0),
            MaximumSize = new Size(SidebarWidth - 30, 0),
            Text = CurrentUser is null
                ? "Chế độ phát triển (chưa đăng nhập)"
                : $"Đăng nhập: {CurrentUser.Username} - {CurrentUser.Role}"
        });
        brand.Controls.Add(brandLayout);

        var footer = new Panel
        {
            BackColor = SidebarBackColor,
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10)
        };
        var btnLogout = new Button
        {
            BackColor = SidebarBackColor,
            Dock = DockStyle.Top,
            FlatStyle = FlatStyle.Flat,
            ForeColor = SidebarTextColor,
            Height = 36,
            Name = "btnLogout",
            Text = CurrentUser is null ? "Thoát" : "Đăng xuất",
            UseMnemonic = false
        };
        btnLogout.FlatAppearance.BorderColor = GroupTextColor;
        btnLogout.FlatAppearance.MouseOverBackColor = SidebarHoverColor;
        btnLogout.Click += (_, _) => RequestLogout();
        footer.Controls.Add(btnLogout);

        var sidebar = new TableLayoutPanel
        {
            BackColor = SidebarBackColor,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 3
        };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        sidebar.Controls.Add(brand, 0, 0);
        sidebar.Controls.Add(_navPanel, 0, 1);
        sidebar.Controls.Add(footer, 0, 2);
        return sidebar;
    }

    private void BuildNavigation()
    {
        _navPanel.SuspendLayout();

        foreach (var group in NavigationCatalog.VisibleGroups(CurrentUser))
        {
            _navPanel.Controls.Add(new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = GroupTextColor,
                Height = 30,
                Margin = new Padding(2, 8, 2, 0),
                Text = group.Key.ToUpperInvariant(),
                TextAlign = ContentAlignment.BottomLeft,
                Width = NavButtonWidth
            });

            foreach (var item in group)
            {
                _navPanel.Controls.Add(CreateNavButton(item));
            }
        }

        _navPanel.ResumeLayout();
    }

    private Button CreateNavButton(NavigationItem item)
    {
        var button = new Button
        {
            AutoEllipsis = true,
            BackColor = SidebarBackColor,
            FlatStyle = FlatStyle.Flat,
            Font = RegularFont,
            ForeColor = SidebarTextColor,
            Height = 42,
            Margin = new Padding(0, 2, 0, 2),
            Name = $"btnNav{item.Key}",
            Text = item.IsReady ? item.Title : $"{item.Title} *",
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false,
            Width = NavButtonWidth
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = SidebarHoverColor;
        button.Click += (_, _) => NavigateTo(item);

        _navButtons[item.Key] = button;
        return button;
    }

    private void NavigateTo(NavigationItem item)
    {
        if (!_formCache.TryGetValue(item.Key, out var form))
        {
            form = CreateChildForm(item);
            _formCache[item.Key] = form;
        }

        _contentPanel.SuspendLayout();
        _contentPanel.Controls.Clear();
        _contentPanel.Controls.Add(form);
        _contentPanel.ResumeLayout();

        form.Visible = true;

        _lblModuleTitle.Text = item.Title;
        _lblModuleDescription.Text = item.Description;
        HighlightActiveButton(item.Key);
    }

    private Form CreateChildForm(NavigationItem item)
    {
        var form = item.FormFactory();

        // Nhúng form con vào vùng nội dung: form con không còn là cửa sổ riêng nên
        // bỏ giới hạn kích thước của chế độ chạy độc lập để không tràn khỏi vùng nội dung.
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        form.MinimumSize = Size.Empty;
        form.MaximumSize = Size.Empty;
        return form;
    }

    private void HighlightActiveButton(string moduleKey)
    {
        if (_activeNavButton is not null)
        {
            _activeNavButton.BackColor = SidebarBackColor;
            _activeNavButton.Font = RegularFont;
            _activeNavButton.ForeColor = SidebarTextColor;
        }

        if (!_navButtons.TryGetValue(moduleKey, out var activeButton))
        {
            _activeNavButton = null;
            return;
        }

        activeButton.BackColor = ActiveBackColor;
        activeButton.Font = BoldFont;
        activeButton.ForeColor = Color.White;
        _activeNavButton = activeButton;
    }

    private void RequestLogout()
    {
        if (LogoutRequested is null)
        {
            MessageBox.Show(
                "Chưa có màn hình đăng nhập được đăng ký. Form chung sẽ đóng lại.",
                "Đăng xuất",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Close();
            return;
        }

        LogoutRequested(this, EventArgs.Empty);
    }
}
