using System.Windows.Forms.DataVisualization.Charting;
using MiniMart.Shared.DTOs.Dashboard;

namespace MiniMart.Client.Forms;

/// <summary>
/// Phần biểu đồ và bảng xếp hạng của màn hình Dashboard thống kê doanh thu.
/// </summary>
/// <remarks>
/// Biểu đồ dùng <c>System.Windows.Forms.DataVisualization.Charting.Chart</c> từ gói
/// <c>WinForms.DataVisualization</c> (xem <c>MiniMart.Client.csproj</c>) thay vì tự vẽ tay,
/// nhờ vậy có sẵn trục, chú thích và tooltip.
/// </remarks>
public partial class frmDashboard
{
    /// <summary>Danh sách thẻ số liệu, khai báo một chỗ để phần bố cục và phần cập nhật dữ liệu dùng chung.</summary>
    private static readonly (string Key, string Title, bool IsCurrency, bool IsHighlight)[] CardDefinitions =
    {
        ("Revenue", "Doanh thu kỳ", true, true),
        ("Orders", "Số hóa đơn", false, false),
        ("AverageOrder", "Trung bình/hóa đơn", true, false),
        ("QuantitySold", "Sản phẩm đã bán", false, false),
        ("ImportAmount", "Giá trị nhập kho", true, false),
        ("ImportCount", "Số phiếu nhập", false, false),
        ("Products", "Tổng sản phẩm", false, false),
        ("LowStock", "Sản phẩm sắp hết hàng", false, true),
        ("Customers", "Khách hàng", false, false),
        ("Users", "Nhân viên", false, false)
    };

    private readonly Chart chtRevenueByDay = new()
    {
        Dock = DockStyle.Fill,
        Name = "chtRevenueByDay"
    };

    private readonly Chart chtRevenueByCategory = new()
    {
        Dock = DockStyle.Fill,
        Name = "chtRevenueByCategory"
    };

    private readonly DataGridView dgvTopProducts = CreateReadOnlyGrid("dgvTopProducts");
    private readonly DataGridView dgvRevenueByCategory = CreateReadOnlyGrid("dgvRevenueByCategory");
    private readonly DataGridView dgvRevenueByUser = CreateReadOnlyGrid("dgvRevenueByUser");
    private readonly DataGridView dgvTopCustomers = CreateReadOnlyGrid("dgvTopCustomers");

    private void InitializeChartsAndTables()
    {
        ConfigureRevenueByDayChart();
        ConfigureRevenueByCategoryChart();
        ConfigureTopProductsGrid();
        ConfigureRevenueByCategoryGrid();
        ConfigureRevenueByUserGrid();
        ConfigureTopCustomersGrid();
    }

    private Control BuildChartPanel()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Name = "grpDashboardCharts",
            Padding = new Padding(10),
            Text = "Biểu đồ doanh thu"
        };

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Name = "tblDashboardCharts",
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

        var dayGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Name = "grpChartRevenueByDay",
            Padding = new Padding(6),
            Text = "Doanh thu theo ngày"
        };
        dayGroup.Controls.Add(chtRevenueByDay);

        var categoryGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Name = "grpChartRevenueByCategory",
            Padding = new Padding(6),
            Text = "Tỷ trọng doanh thu theo danh mục"
        };
        categoryGroup.Controls.Add(chtRevenueByCategory);

        layout.Controls.Add(dayGroup, 0, 0);
        layout.Controls.Add(categoryGroup, 1, 0);
        group.Controls.Add(layout);
        return group;
    }

    private Control BuildTablesPanel()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Name = "grpDashboardTables",
            Padding = new Padding(10),
            Text = "Bảng xếp hạng trong kỳ"
        };

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Name = "tblDashboardTables",
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        layout.Controls.Add(WrapGrid("Sản phẩm bán chạy", "grpTopProducts", dgvTopProducts), 0, 0);
        layout.Controls.Add(WrapGrid("Doanh thu theo danh mục", "grpRevenueByCategory", dgvRevenueByCategory), 1, 0);
        layout.Controls.Add(WrapGrid("Doanh thu theo nhân viên", "grpRevenueByUser", dgvRevenueByUser), 0, 1);
        layout.Controls.Add(WrapGrid("Khách hàng chi tiêu nhiều nhất", "grpTopCustomers", dgvTopCustomers), 1, 1);

        group.Controls.Add(layout);
        return group;
    }

    private static Control WrapGrid(string title, string name, DataGridView grid)
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Name = name,
            Padding = new Padding(6),
            Text = title
        };
        group.Controls.Add(grid);
        return group;
    }

    private static DataGridView CreateReadOnlyGrid(string name)
    {
        return new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            Dock = DockStyle.Fill,
            Name = name,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
    }

    private void ConfigureRevenueByDayChart()
    {
        var area = new ChartArea("areaRevenueByDay")
        {
            AxisX =
            {
                Title = "Ngày",
                IntervalAutoMode = IntervalAutoMode.VariableCount,
                LabelStyle = { Format = "dd/MM" }
            },
            AxisY =
            {
                Title = "Doanh thu",
                LabelStyle = { Format = "N0" }
            }
        };
        chtRevenueByDay.ChartAreas.Add(area);
        chtRevenueByDay.Legends.Clear();

        var series = new Series("Doanh thu")
        {
            ChartArea = area.Name,
            ChartType = SeriesChartType.Column,
            IsValueShownAsLabel = false,
            ToolTip = "#VALX{dd/MM/yyyy}: #VAL{N0}"
        };
        series["PointWidth"] = "0.6";
        chtRevenueByDay.Series.Add(series);
    }

    private void ConfigureRevenueByCategoryChart()
    {
        var area = new ChartArea("areaRevenueByCategory");
        chtRevenueByCategory.ChartAreas.Add(area);
        chtRevenueByCategory.Legends.Clear();

        var legend = new Legend("legendRevenueByCategory")
        {
            Docking = Docking.Bottom,
            Alignment = StringAlignment.Center
        };
        chtRevenueByCategory.Legends.Add(legend);

        var series = new Series("Doanh thu")
        {
            ChartArea = area.Name,
            ChartType = SeriesChartType.Doughnut,
            Legend = legend.Name,
            ToolTip = "#AXISLABEL: #VAL{N0}"
        };
        series["PieLabelStyle"] = "Disabled";
        series["DoughnutPercentage"] = "45";
        chtRevenueByCategory.Series.Add(series);
    }

    private void ConfigureTopProductsGrid()
    {
        dgvTopProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopProductDto.ProductName),
            HeaderText = "Sản phẩm",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 140
        });
        dgvTopProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopProductDto.QuantitySold),
            HeaderText = "SL bán",
            Width = 80
        });
        dgvTopProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopProductDto.Revenue),
            HeaderText = "Doanh thu",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
        });
    }

    private void ConfigureRevenueByCategoryGrid()
    {
        dgvRevenueByCategory.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(RevenueByCategoryDto.CategoryName),
            HeaderText = "Danh mục",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 140
        });
        dgvRevenueByCategory.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(RevenueByCategoryDto.QuantitySold),
            HeaderText = "SL bán",
            Width = 80
        });
        dgvRevenueByCategory.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(RevenueByCategoryDto.Revenue),
            HeaderText = "Doanh thu",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
        });
    }

    private void ConfigureRevenueByUserGrid()
    {
        dgvRevenueByUser.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(RevenueByUserDto.Username),
            HeaderText = "Nhân viên",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 120
        });
        dgvRevenueByUser.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(RevenueByUserDto.OrderCount),
            HeaderText = "Hóa đơn",
            Width = 80
        });
        dgvRevenueByUser.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(RevenueByUserDto.Revenue),
            HeaderText = "Doanh thu",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
        });
    }

    private void ConfigureTopCustomersGrid()
    {
        dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopCustomerDto.FullName),
            HeaderText = "Khách hàng",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 130
        });
        dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopCustomerDto.PhoneNumber),
            HeaderText = "Điện thoại",
            Width = 110
        });
        dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopCustomerDto.OrderCount),
            HeaderText = "Hóa đơn",
            Width = 75
        });
        dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopCustomerDto.Revenue),
            HeaderText = "Chi tiêu",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
        });
        dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TopCustomerDto.TotalPoints),
            HeaderText = "Điểm",
            Width = 65
        });
    }

    /// <summary>Cập nhật giá trị của các thẻ số liệu.</summary>
    private void UpdateCards(DashboardStatisticsDto statistics)
    {
        var summary = statistics.Summary;

        SetCard("Revenue", FormatNumber(summary.TotalRevenue));
        SetCard("Orders", summary.OrderCount.ToString("N0"));
        SetCard("AverageOrder", FormatNumber(summary.AverageOrderValue));
        SetCard("QuantitySold", summary.TotalQuantitySold.ToString("N0"));
        SetCard("ImportAmount", FormatNumber(summary.TotalImportAmount));
        SetCard("ImportCount", summary.ImportReceiptCount.ToString("N0"));
        SetCard("Products", summary.TotalProducts.ToString("N0"));
        SetCard("LowStock", summary.LowStockProductCount.ToString("N0"));
        SetCard("Customers", summary.TotalCustomers.ToString("N0"));
        SetCard("Users", summary.TotalUsers.ToString("N0"));
    }

    private void SetCard(string key, string value)
    {
        if (_cardValueLabels.TryGetValue(key, out var label))
        {
            label.Text = value;
        }
    }

    private static string FormatNumber(decimal value) => value.ToString("N0");

    /// <summary>Cập nhật dữ liệu cho hai biểu đồ.</summary>
    private void UpdateCharts(DashboardStatisticsDto statistics)
    {
        var daySeries = chtRevenueByDay.Series[0];
        daySeries.Points.Clear();

        foreach (var day in statistics.RevenueByDay)
        {
            var index = daySeries.Points.AddXY(day.Date, day.Revenue);
            daySeries.Points[index].ToolTip =
                $"{day.Date:dd/MM/yyyy}\nDoanh thu: {day.Revenue:N0}\nHóa đơn: {day.OrderCount:N0}";
        }

        // Khoảng quá dài thì giảm mật độ nhãn trục ngày để không chồng chữ.
        chtRevenueByDay.ChartAreas[0].AxisX.Interval = statistics.RevenueByDay.Count > 45 ? 5 : 0;

        var categorySeries = chtRevenueByCategory.Series[0];
        categorySeries.Points.Clear();

        foreach (var category in statistics.RevenueByCategory)
        {
            var index = categorySeries.Points.AddXY(category.CategoryName, category.Revenue);
            categorySeries.Points[index].ToolTip =
                $"{category.CategoryName}\nDoanh thu: {category.Revenue:N0}\nSL bán: {category.QuantitySold:N0}";
        }
    }

    /// <summary>Cập nhật dữ liệu cho bốn bảng xếp hạng.</summary>
    private void UpdateTables(DashboardStatisticsDto statistics)
    {
        dgvTopProducts.DataSource = new List<TopProductDto>(statistics.TopProducts);
        dgvRevenueByCategory.DataSource = new List<RevenueByCategoryDto>(statistics.RevenueByCategory);
        dgvRevenueByUser.DataSource = new List<RevenueByUserDto>(statistics.RevenueByUser);
        dgvTopCustomers.DataSource = new List<TopCustomerDto>(statistics.TopCustomers);
    }

    /// <summary>Xóa toàn bộ dữ liệu biểu đồ và bảng khi không tải được số liệu.</summary>
    private void ClearChartsAndTables()
    {
        chtRevenueByDay.Series[0].Points.Clear();
        chtRevenueByCategory.Series[0].Points.Clear();

        dgvTopProducts.DataSource = null;
        dgvRevenueByCategory.DataSource = null;
        dgvRevenueByUser.DataSource = null;
        dgvTopCustomers.DataSource = null;
    }
}
