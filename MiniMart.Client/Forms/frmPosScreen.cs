using System.ComponentModel;
using System.Drawing.Printing;
using System.Net.Http.Json;
using MiniMart.Client.HttpClients;
using MiniMart.Shared.DTOs.Customer;
using MiniMart.Shared.DTOs.Orders;
using MiniMart.Shared.DTOs.Products;

namespace MiniMart.Client.Forms;

public class frmPosScreen : Form
{
    private const decimal MaxOrderAmount = 9999999999999999.99m;

    private readonly BaseHttpClient _apiClient = new();
    private readonly DataGridView dgvProducts = new()
    {
        Name = "dgvProducts",
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };
    private readonly DataGridView dgvOrderDetails = new()
    {
        Name = "dgvOrderDetails",
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };
    private readonly TextBox txtProductSearch = new()
    {
        Name = "txtProductSearch",
        Dock = DockStyle.Fill,
        PlaceholderText = "Tìm theo tên hoặc mã sản phẩm"
    };
    private readonly ComboBox cbxOrderCustomer = new()
    {
        Name = "cbxOrderCustomer",
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly NumericUpDown nudProductQuantity = new()
    {
        Name = "nudProductQuantity",
        DecimalPlaces = 0,
        Minimum = 1,
        Maximum = int.MaxValue,
        ThousandsSeparator = true,
        Value = 1,
        Width = 110
    };
    private readonly Button btnAddToOrder = new() { Name = "btnAddToOrder", Text = "Thêm vào hóa đơn" };
    private readonly Button btnRemoveOrderLine = new() { Name = "btnRemoveOrderLine", Text = "Xóa dòng" };
    private readonly Button btnClearOrder = new() { Name = "btnClearOrder", Text = "Làm mới hóa đơn" };
    private readonly Button btnRefreshPosData = new() { Name = "btnRefreshPosData", Text = "Tải lại sản phẩm" };
    private readonly Button btnCreateOrder = new() { Name = "btnCreateOrder", Text = "Thanh toán và tạo hóa đơn" };
    private readonly Button btnPrintReceipt = new() { Name = "btnPrintReceipt", Text = "Xem trước / in" };
    private readonly Label lblOrderTotal = new()
    {
        Name = "lblOrderTotal",
        AutoSize = true,
        Font = new Font("Segoe UI", 15F, FontStyle.Bold),
        ForeColor = Color.FromArgb(33, 37, 41),
        Text = "Tổng thanh toán: 0.00"
    };
    private readonly BindingList<PosOrderLine> _orderLines = new();
    private readonly List<ProductDto> _products = new();
    private List<ReceiptLine> _lastReceiptLines = new();
    private OrderDto? _lastCreatedOrder;
    private string _lastReceiptCustomer = "Khách lẻ";
    private int _receiptLineIndex;
    private decimal? _currentTotal;

    public frmPosScreen()
    {
        Name = "frmPosScreen";
        Text = "Bán hàng tại quầy (POS)";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 650);
        Width = 1200;
        Height = 780;

        InitializeLayout();
        InitializeEvents();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Name = "tblPosLayout",
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));

        var header = new TableLayoutPanel
        {
            Name = "tblPosHeader",
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lblTitle = new Label
        {
            Name = "lblPosTitle",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Text = "Bán hàng tại quầy",
            TextAlign = ContentAlignment.MiddleLeft
        };
        var lblDescription = new Label
        {
            Name = "lblPosDescription",
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(108, 117, 125),
            Text = "Chọn sản phẩm, kiểm tra hóa đơn và hoàn tất thanh toán.",
            TextAlign = ContentAlignment.MiddleLeft
        };
        btnRefreshPosData.AutoSize = true;
        btnRefreshPosData.Anchor = AnchorStyles.Right;
        btnRefreshPosData.Padding = new Padding(10, 5, 10, 5);
        header.Controls.Add(lblTitle, 0, 0);
        header.Controls.Add(lblDescription, 0, 1);
        header.Controls.Add(btnRefreshPosData, 1, 0);
        header.SetRowSpan(btnRefreshPosData, 2);

        var workspace = new TableLayoutPanel
        {
            Name = "tblPosWorkspace",
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 8, 0, 8),
            RowCount = 1
        };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));

        workspace.Controls.Add(CreateProductPanel(), 0, 0);
        workspace.Controls.Add(CreateOrderPanel(), 1, 0);

        var footer = new TableLayoutPanel
        {
            Name = "tblPosFooter",
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(4, 8, 4, 0),
            RowCount = 2
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lblCustomer = new Label
        {
            Name = "lblOrderCustomer",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Text = "Khách hàng (không bắt buộc)",
            TextAlign = ContentAlignment.MiddleLeft
        };
        footer.Controls.Add(lblCustomer, 0, 0);
        footer.Controls.Add(cbxOrderCustomer, 0, 1);

        var actions = new FlowLayoutPanel
        {
            Name = "flpPosActions",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 2, 0, 0)
        };
        foreach (var button in new[] { btnCreateOrder, btnPrintReceipt })
        {
            button.AutoSize = true;
            button.Margin = new Padding(6, 3, 6, 3);
            button.Padding = new Padding(10, 5, 10, 5);
            actions.Controls.Add(button);
        }
        lblOrderTotal.Dock = DockStyle.Fill;
        lblOrderTotal.TextAlign = ContentAlignment.MiddleRight;
        footer.Controls.Add(lblOrderTotal, 1, 0);
        footer.Controls.Add(actions, 1, 1);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(workspace, 0, 1);
        layout.Controls.Add(footer, 0, 2);
        Controls.Add(layout);
    }

    private Control CreateProductPanel()
    {
        var panel = new GroupBox
        {
            Name = "grpPosProducts",
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            Text = "Danh sách sản phẩm"
        };
        var layout = new TableLayoutPanel
        {
            Name = "tblPosProducts",
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colProductId",
            DataPropertyName = nameof(ProductDto.Id),
            HeaderText = "Mã",
            Width = 65
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colProductName",
            DataPropertyName = nameof(ProductDto.Name),
            HeaderText = "Tên sản phẩm",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 140
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colProductPrice",
            DataPropertyName = nameof(ProductDto.Price),
            HeaderText = "Giá",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colProductStock",
            DataPropertyName = nameof(ProductDto.StockQuantity),
            HeaderText = "Tồn",
            Width = 75
        });

        var addActions = new FlowLayoutPanel
        {
            Name = "flpAddOrderLine",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(2),
            WrapContents = false
        };
        var lblQuantity = new Label
        {
            Name = "lblProductQuantity",
            AutoSize = true,
            Margin = new Padding(4, 10, 2, 0),
            Text = "Số lượng:"
        };
        nudProductQuantity.Margin = new Padding(2, 5, 8, 2);
        btnAddToOrder.AutoSize = true;
        btnAddToOrder.Margin = new Padding(4);
        btnAddToOrder.Padding = new Padding(8, 3, 8, 3);
        addActions.Controls.Add(lblQuantity);
        addActions.Controls.Add(nudProductQuantity);
        addActions.Controls.Add(btnAddToOrder);

        layout.Controls.Add(txtProductSearch, 0, 0);
        layout.Controls.Add(dgvProducts, 0, 1);
        layout.Controls.Add(addActions, 0, 2);
        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateOrderPanel()
    {
        var panel = new GroupBox
        {
            Name = "grpPosOrder",
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            Text = "Hóa đơn hiện tại"
        };
        var layout = new TableLayoutPanel
        {
            Name = "tblPosOrder",
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

        dgvOrderDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colOrderProductName",
            DataPropertyName = nameof(PosOrderLine.ProductName),
            HeaderText = "Sản phẩm",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 120
        });
        dgvOrderDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colOrderQuantity",
            DataPropertyName = nameof(PosOrderLine.Quantity),
            HeaderText = "SL",
            Width = 48
        });
        dgvOrderDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colOrderUnitPrice",
            DataPropertyName = nameof(PosOrderLine.UnitPrice),
            HeaderText = "Đơn giá",
            Width = 92,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        dgvOrderDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colOrderLineTotal",
            DataPropertyName = nameof(PosOrderLine.LineTotal),
            HeaderText = "Thành tiền",
            Width = 105,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        dgvOrderDetails.DataSource = _orderLines;

        var lineActions = new FlowLayoutPanel
        {
            Name = "flpOrderLineActions",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(2),
            WrapContents = false
        };
        foreach (var button in new[] { btnClearOrder, btnRemoveOrderLine })
        {
            button.AutoSize = true;
            button.Margin = new Padding(4);
            button.Padding = new Padding(8, 3, 8, 3);
            lineActions.Controls.Add(button);
        }

        layout.Controls.Add(dgvOrderDetails, 0, 0);
        layout.Controls.Add(lineActions, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private void InitializeEvents()
    {
        btnAddToOrder.Click += (_, _) => AddSelectedProduct();
        btnRemoveOrderLine.Click += (_, _) => RemoveSelectedLine();
        btnClearOrder.Click += (_, _) => ClearOrder();
        btnRefreshPosData.Click += async (_, _) => await RefreshPosDataAsync();
        btnCreateOrder.Click += async (_, _) => await CreateOrderAsync();
        btnPrintReceipt.Click += (_, _) => ShowReceiptPreview();
        txtProductSearch.TextChanged += (_, _) => ApplyProductFilter();
        dgvProducts.DoubleClick += (_, _) => AddSelectedProduct();
        _orderLines.ListChanged += (_, _) => UpdateOrderSummary();
        Load += async (_, _) => await RefreshPosDataAsync();

        btnPrintReceipt.Enabled = false;
        btnCreateOrder.Enabled = false;
        btnRemoveOrderLine.Enabled = false;
        btnClearOrder.Enabled = false;
        dgvOrderDetails.SelectionChanged += (_, _) =>
            btnRemoveOrderLine.Enabled = dgvOrderDetails.CurrentRow?.DataBoundItem is PosOrderLine;
    }

    private async Task RefreshPosDataAsync()
    {
        btnRefreshPosData.Enabled = false;

        try
        {
            var productsTask = _apiClient.GetAsync<List<ProductDto>>("api/products");
            var customersTask = _apiClient.GetAsync<List<CustomerDto>>("api/customers");
            await Task.WhenAll(productsTask, customersTask);

            _products.Clear();
            _products.AddRange(productsTask.Result ?? new List<ProductDto>());
            ApplyProductFilter();

            var customers = customersTask.Result ?? new List<CustomerDto>();
            var customerOptions = new List<CustomerOption> { new(null, "Khách lẻ") };
            customerOptions.AddRange(customers
                .OrderBy(customer => customer.FullName)
                .Select(customer => new CustomerOption(
                    customer.Id,
                    $"{customer.FullName} - {customer.PhoneNumber}")));

            cbxOrderCustomer.DataSource = new BindingList<CustomerOption>(customerOptions);
            cbxOrderCustomer.DisplayMember = nameof(CustomerOption.DisplayName);
            cbxOrderCustomer.ValueMember = nameof(CustomerOption.CustomerId);
            cbxOrderCustomer.SelectedIndex = 0;
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
        finally
        {
            btnRefreshPosData.Enabled = true;
        }
    }

    private void ApplyProductFilter()
    {
        var searchText = txtProductSearch.Text.Trim();
        var filteredProducts = _products
            .Where(product => string.IsNullOrEmpty(searchText)
                || product.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
                || product.Id.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        dgvProducts.DataSource = new BindingList<ProductDto>(filteredProducts);
    }

    private void AddSelectedProduct()
    {
        if (dgvProducts.CurrentRow?.DataBoundItem is not ProductDto product)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm cần thêm.", "Chưa chọn sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var existingLine = _orderLines.FirstOrDefault(line => line.ProductId == product.Id);
        var requestedQuantity = (long)(existingLine?.Quantity ?? 0) + (long)nudProductQuantity.Value;
        if (requestedQuantity > product.StockQuantity)
        {
            MessageBox.Show("Số lượng yêu cầu vượt quá tồn kho hiện tại.", "Không đủ tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (existingLine is null)
        {
            _orderLines.Add(new PosOrderLine(product.Id, product.Name, product.Price, (int)nudProductQuantity.Value));
        }
        else
        {
            existingLine.Quantity = (int)requestedQuantity;
            dgvOrderDetails.Refresh();
            UpdateOrderSummary();
        }
    }

    private void RemoveSelectedLine()
    {
        if (dgvOrderDetails.CurrentRow?.DataBoundItem is PosOrderLine line)
        {
            _orderLines.Remove(line);
        }
    }

    private void ClearOrder()
    {
        _orderLines.Clear();
        nudProductQuantity.Value = 1;
    }

    private void UpdateOrderSummary()
    {
        decimal total = 0;
        try
        {
            foreach (var line in _orderLines)
            {
                total = checked(total + line.LineTotal);
            }

            _currentTotal = total <= MaxOrderAmount ? total : null;
        }
        catch (OverflowException)
        {
            _currentTotal = null;
        }

        lblOrderTotal.Text = _currentTotal.HasValue
            ? $"Tổng thanh toán: {_currentTotal.Value:N2}"
            : "Tổng thanh toán vượt giới hạn";
        btnCreateOrder.Enabled = _orderLines.Count > 0 && _currentTotal.HasValue;
        btnClearOrder.Enabled = _orderLines.Count > 0;
    }

    private async Task CreateOrderAsync()
    {
        if (_orderLines.Count == 0 || !_currentTotal.HasValue)
        {
            return;
        }

        btnCreateOrder.Enabled = false;
        btnRefreshPosData.Enabled = false;
        try
        {
            var selectedCustomer = cbxOrderCustomer.SelectedItem as CustomerOption;
            var request = new CreateOrderDto
            {
                CustomerId = selectedCustomer?.CustomerId,
                Details = _orderLines.Select(line => new CreateOrderDetailDto
                {
                    ProductId = line.ProductId,
                    Quantity = line.Quantity
                }).ToList()
            };

            using var response = await _apiClient.PostAsync("api/orders", request);
            await BaseHttpClient.EnsureSuccessAsync(response);
            var createdOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
            if (createdOrder is null)
            {
                throw new InvalidOperationException("API không trả về thông tin hóa đơn vừa tạo.");
            }

            _lastCreatedOrder = createdOrder;
            _lastReceiptCustomer = selectedCustomer?.DisplayName ?? "Khách lẻ";
            _lastReceiptLines = _orderLines
                .Select(line => new ReceiptLine(line.ProductName, line.Quantity, line.UnitPrice))
                .ToList();
            btnPrintReceipt.Enabled = true;

            MessageBox.Show(
                $"Đã tạo hóa đơn #{createdOrder.Id} thành công. Tổng tiền: {createdOrder.TotalAmount:N2}",
                "Thanh toán thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ClearOrder();
            try
            {
                await LoadProductsAsync();
            }
            catch (Exception exception)
            {
                ShowApiError(exception);
            }
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
        finally
        {
            btnCreateOrder.Enabled = _orderLines.Count > 0 && _currentTotal.HasValue;
            btnRefreshPosData.Enabled = true;
        }
    }

    private async Task LoadProductsAsync()
    {
        var products = await _apiClient.GetAsync<List<ProductDto>>("api/products") ?? new List<ProductDto>();
        _products.Clear();
        _products.AddRange(products);
        ApplyProductFilter();
    }

    private void ShowReceiptPreview()
    {
        if (_lastCreatedOrder is null)
        {
            return;
        }

        using var printDocument = new PrintDocument
        {
            DocumentName = $"Hóa đơn MiniMart {_lastCreatedOrder.Id}"
        };
        printDocument.BeginPrint += (_, _) => _receiptLineIndex = 0;
        printDocument.PrintPage += PrintReceiptPage;

        using var previewDialog = new PrintPreviewDialog
        {
            Document = printDocument,
            Width = 900,
            Height = 700
        };
        previewDialog.ShowDialog(this);
    }

    private void PrintReceiptPage(object? sender, PrintPageEventArgs eventArgs)
    {
        if (_lastCreatedOrder is null || eventArgs.Graphics is null)
        {
            return;
        }

        var graphics = eventArgs.Graphics;
        var bounds = eventArgs.MarginBounds;
        using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        using var bodyFont = new Font("Segoe UI", 10F);
        using var totalFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        var y = (float)bounds.Top;

        graphics.DrawString("MINIMART - HÓA ĐƠN BÁN LẺ", titleFont, Brushes.Black, bounds.Left, y);
        y += 34;
        graphics.DrawString($"Mã hóa đơn: {_lastCreatedOrder.Id}", bodyFont, Brushes.Black, bounds.Left, y);
        y += 22;
        graphics.DrawString($"Thời gian: {_lastCreatedOrder.OrderDate.ToLocalTime():dd/MM/yyyy HH:mm}", bodyFont, Brushes.Black, bounds.Left, y);
        y += 22;
        graphics.DrawString($"Khách hàng: {_lastReceiptCustomer}", bodyFont, Brushes.Black, bounds.Left, y);
        y += 30;

        while (_receiptLineIndex < _lastReceiptLines.Count && y + 24 < bounds.Bottom - 48)
        {
            var line = _lastReceiptLines[_receiptLineIndex++];
            var lineText = $"{line.ProductName}  x{line.Quantity}  {line.UnitPrice:N2}  {line.LineTotal:N2}";
            graphics.DrawString(lineText, bodyFont, Brushes.Black, bounds.Left, y);
            y += 22;
        }

        eventArgs.HasMorePages = _receiptLineIndex < _lastReceiptLines.Count;
        if (!eventArgs.HasMorePages)
        {
            graphics.DrawString($"TỔNG CỘNG: {_lastCreatedOrder.TotalAmount:N2}", totalFont, Brushes.Black, bounds.Left, bounds.Bottom - 36);
        }
    }

    private static void ShowApiError(Exception exception)
    {
        MessageBox.Show(exception.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private sealed record CustomerOption(int? CustomerId, string DisplayName);

    private sealed class PosOrderLine(int productId, string productName, decimal unitPrice, int quantity)
    {
        public int ProductId { get; } = productId;

        public string ProductName { get; } = productName;

        public decimal UnitPrice { get; } = unitPrice;

        public int Quantity { get; set; } = quantity;

        public decimal LineTotal => UnitPrice * Quantity;
    }

    private sealed record ReceiptLine(string ProductName, int Quantity, decimal UnitPrice)
    {
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
