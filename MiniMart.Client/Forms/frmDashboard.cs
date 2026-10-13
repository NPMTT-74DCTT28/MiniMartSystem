using System.Globalization;
using MiniMart.Client.HttpClients;
using MiniMart.Shared.DTOs.Dashboard;

namespace MiniMart.Client.Forms;

/// <summary>
/// Màn hình Dashboard thống kê doanh thu (dành cho mọi vai trò đã đăng nhập).
/// </summary>
/// <remarks>
/// <para>
/// Toàn bộ số liệu được lấy qua một lần gọi <c>GET api/dashboard</c> (xem
/// <see cref="DashboardStatisticsDto"/>); màn hình không tự tổng hợp dữ liệu từ nhiều API
/// vì việc tổng hợp cần đọc đồng thời nhiều bảng và Client không được truy cập database.
/// </para>
/// <para>
/// Giao diện gồm 3 phần: bộ lọc khoảng thời gian, các thẻ số liệu tổng hợp, biểu đồ doanh thu
/// theo ngày và các bảng xếp hạng (sản phẩm, danh mục, nhân viên, khách hàng).
/// </para>
/// </remarks>
public partial class frmDashboard : Form
{
    /// <summary>Số ngày mặc định của kỳ thống kê, khớp với mặc định của API.</summary>
    private const int DefaultRangeInDays = 30;

    private static readonly Color HeaderTextColor = Color.FromArgb(17, 24, 39);
    private static readonly Color MutedTextColor = Color.FromArgb(107, 114, 125);
    private static readonly Color AccentColor = Color.FromArgb(37, 99, 235);

    private readonly BaseHttpClient _apiClient = new();

    private readonly DateTimePicker dtpFrom = new()
    {
        Format = DateTimePickerFormat.Short,
        Name = "dtpFrom",
        Width = 140
    };

    private readonly DateTimePicker dtpTo = new()
    {
        Format = DateTimePickerFormat.Short,
        Name = "dtpTo",
        Width = 140
    };

    private readonly Button btnRefresh = new() { Name = "btnRefresh", Text = "Xem thống kê" };
    private readonly Button btnRangeToday = new() { Name = "btnRangeToday", Text = "Hôm nay" };
    private readonly Button btnRangeWeek = new() { Name = "btnRangeWeek", Text = "7 ngày" };
    private readonly Button btnRangeMonth = new() { Name = "btnRangeMonth", Text = "30 ngày" };
    private readonly Button btnRangeThisMonth = new() { Name = "btnRangeThisMonth", Text = "Tháng này" };

    private readonly Label lblStatus = new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        ForeColor = MutedTextColor,
        Name = "lblStatus",
        Text = "Chưa tải dữ liệu.",
        TextAlign = ContentAlignment.MiddleLeft
    };

    private readonly Dictionary<string, Label> _cardValueLabels = new();
    private readonly Dictionary<string, Label> _cardCaptionLabels = new();

    private bool _isLoading;

    public frmDashboard()
    {
        Name = "frmDashboard";
        Text = "Tổng quan và thống kê";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        Width = 1280;
        Height = 820;

        var today = DateTime.Today;
        dtpFrom.Value = today.AddDays(-(DefaultRangeInDays - 1));
        dtpTo.Value = today;

        InitializeLayout();
        InitializeChartsAndTables();
        InitializeEvents();
    }

    /// <summary>Số liệu thống kê đang hiển thị, hoặc <c>null</c> khi chưa tải được.</summary>
    public DashboardStatisticsDto? Statistics { get; private set; }

    /// <summary>Khoảng thời gian đang chọn trên bộ lọc, dùng để kiểm thử và điều hướng.</summary>
    public (DateTime From, DateTime To) SelectedRange => (dtpFrom.Value.Date, dtpTo.Value.Date);

    /// <summary>
    /// Tải lại số liệu thống kê theo khoảng thời gian đang chọn trên giao diện.
    /// </summary>
    public async Task LoadStatisticsAsync()
    {
        await LoadStatisticsAsync(dtpFrom.Value.Date, dtpTo.Value.Date);
    }

    /// <summary>Tải số liệu thống kê của một khoảng thời gian cụ thể.</summary>
    /// <param name="from">Ngày bắt đầu kỳ thống kê.</param>
    /// <param name="to">Ngày kết thúc kỳ thống kê (tính trọn ngày).</param>
    public async Task LoadStatisticsAsync(DateTime from, DateTime to)
    {
        if (_isLoading)
        {
            return;
        }

        var fromDate = from.Date;
        var toDate = to.Date;

        if (fromDate > toDate)
        {
            MessageBox.Show(
                "Ngày bắt đầu không được sau ngày kết thúc.",
                "Khoảng thời gian không hợp lệ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _isLoading = true;
        SetLoadingState(true);

        try
        {
            var endpoint = string.Create(
                CultureInfo.InvariantCulture,
                $"api/dashboard?from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}");

            var statistics = await _apiClient.GetAsync<DashboardStatisticsDto>(endpoint)
                ?? throw new InvalidOperationException("API không trả về dữ liệu thống kê.");

            Statistics = statistics;
            UpdateCards(statistics);
            UpdateCharts(statistics);
            UpdateTables(statistics);
            lblStatus.Text = $"Kỳ thống kê: {statistics.FromDate:dd/MM/yyyy} - {statistics.ToDate:dd/MM/yyyy}.";
        }
        catch (Exception exception)
        {
            Statistics = null;
            ClearDisplay();
            lblStatus.Text = "Không tải được dữ liệu thống kê.";
            ShowApiError(exception);
        }
        finally
        {
            _isLoading = false;
            SetLoadingState(false);
        }
    }

    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            AutoScroll = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Name = "tblDashboardRoot",
            Padding = new Padding(16),
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 214));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 320));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 620));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildCardsPanel(), 0, 1);
        root.Controls.Add(BuildChartPanel(), 0, 2);
        root.Controls.Add(BuildTablesPanel(), 0, 3);

        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Name = "tblDashboardHeader",
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 640));

        var titles = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Name = "tblDashboardTitles",
            RowCount = 3
        };
        titles.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        titles.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        titles.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        titles.Controls.Add(new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = HeaderTextColor,
            Name = "lblDashboardTitle",
            Text = "Tổng quan và thống kê doanh thu",
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        titles.Controls.Add(new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = MutedTextColor,
            Name = "lblDashboardDescription",
            Text = "Số liệu tổng hợp từ hóa đơn, sản phẩm, khách hàng, nhân viên và phiếu nhập kho.",
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);
        titles.Controls.Add(lblStatus, 0, 2);

        header.Controls.Add(titles, 0, 0);
        header.Controls.Add(BuildRangePanel(), 1, 0);
        return header;
    }

    private Control BuildRangePanel()
    {
        var panel = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Name = "tblDashboardRange",
            Padding = new Padding(0, 18, 0, 0),
            RowCount = 2
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        var pickers = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Name = "flpDashboardRange",
            WrapContents = false
        };
        pickers.Controls.Add(new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 6, 4, 0),
            Name = "lblRangeFrom",
            Text = "Từ ngày"
        });
        pickers.Controls.Add(dtpFrom);
        pickers.Controls.Add(new Label
        {
            AutoSize = true,
            Margin = new Padding(12, 6, 4, 0),
            Name = "lblRangeTo",
            Text = "Đến ngày"
        });
        pickers.Controls.Add(dtpTo);

        var quickButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Name = "flpDashboardQuickRange",
            WrapContents = false
        };
        foreach (var button in new[] { btnRangeToday, btnRangeWeek, btnRangeMonth, btnRangeThisMonth, btnRefresh })
        {
            button.AutoSize = true;
            button.Margin = new Padding(0, 2, 6, 2);
            button.Padding = new Padding(10, 4, 10, 4);
            quickButtons.Controls.Add(button);
        }

        panel.Controls.Add(pickers, 0, 0);
        panel.Controls.Add(quickButtons, 0, 1);
        return panel;
    }

    private Control BuildCardsPanel()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Name = "grpDashboardCards",
            Padding = new Padding(10),
            Text = "Chỉ số tổng hợp"
        };

        var flow = new FlowLayoutPanel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Name = "flpDashboardCards",
            Padding = new Padding(4),
            WrapContents = true
        };

        foreach (var definition in CardDefinitions)
        {
            flow.Controls.Add(CreateCard(definition));
        }

        group.Controls.Add(flow);
        return group;
    }

    private Control CreateCard((string Key, string Title, bool IsCurrency, bool IsHighlight) definition)
    {
        var card = new Panel
        {
            BackColor = definition.IsHighlight ? Color.FromArgb(254, 243, 199) : Color.FromArgb(243, 244, 246),
            Height = 84,
            Margin = new Padding(6),
            Name = $"pnlCard{definition.Key}",
            Padding = new Padding(12, 8, 12, 8),
            Width = 216
        };

        var caption = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            ForeColor = MutedTextColor,
            Height = 22,
            Name = $"lblCard{definition.Key}Caption",
            Text = definition.Title,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var value = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = definition.IsHighlight ? Color.FromArgb(180, 83, 9) : AccentColor,
            Name = $"lblCard{definition.Key}Value",
            Text = "-",
            TextAlign = ContentAlignment.MiddleLeft
        };

        card.Controls.Add(value);
        card.Controls.Add(caption);

        _cardCaptionLabels[definition.Key] = caption;
        _cardValueLabels[definition.Key] = value;
        return card;
    }

    private static void ShowApiError(Exception exception)
    {
        var message = exception is HttpRequestException
            ? "Không kết nối được tới API. Hãy chắc chắn API đang chạy."
            : exception.Message;

        MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void InitializeEvents()
    {
        btnRefresh.Click += async (_, _) => await LoadStatisticsAsync();
        btnRangeToday.Click += async (_, _) => await ApplyQuickRangeAsync(0);
        btnRangeWeek.Click += async (_, _) => await ApplyQuickRangeAsync(6);
        btnRangeMonth.Click += async (_, _) => await ApplyQuickRangeAsync(DefaultRangeInDays - 1);
        btnRangeThisMonth.Click += async (_, _) =>
        {
            var today = DateTime.Today;
            dtpFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtpTo.Value = today;
            await LoadStatisticsAsync();
        };
        Load += async (_, _) => await LoadStatisticsAsync();
    }

    private async Task ApplyQuickRangeAsync(int daysBack)
    {
        var today = DateTime.Today;
        dtpFrom.Value = today.AddDays(-daysBack);
        dtpTo.Value = today;
        await LoadStatisticsAsync();
    }

    private void SetLoadingState(bool isLoading)
    {
        btnRefresh.Enabled = !isLoading;
        btnRefresh.Text = isLoading ? "Đang tải..." : "Xem thống kê";
        Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
    }

    /// <summary>Đưa toàn bộ giao diện về trạng thái rỗng khi không tải được dữ liệu.</summary>
    private void ClearDisplay()
    {
        foreach (var label in _cardValueLabels.Values)
        {
            label.Text = "-";
        }

        ClearChartsAndTables();
    }
}
