using System.ComponentModel;
using System.Net.Http.Json;
using MiniMart.Client.HttpClients;
using MiniMart.Shared.DTOs.ImportReceipts;
using MiniMart.Shared.DTOs.Products;
using MiniMart.Shared.DTOs.Suppliers;

namespace MiniMart.Client.Forms;

public partial class frmInventoryManager
{
    private const decimal MaxImportPrice = 9999999999999999.99m;

    private readonly ComboBox cbxReceiptSupplier = new()
    {
        Name = "cbxReceiptSupplier",
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly ComboBox cbxReceiptProduct = new()
    {
        Name = "cbxReceiptProduct",
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly NumericUpDown nudReceiptQuantity = new()
    {
        Name = "nudReceiptQuantity",
        DecimalPlaces = 0,
        Minimum = 1,
        Maximum = int.MaxValue,
        Value = 1,
        ThousandsSeparator = true
    };
    private readonly NumericUpDown nudReceiptImportPrice = new()
    {
        Name = "nudReceiptImportPrice",
        DecimalPlaces = 2,
        Minimum = 0,
        Maximum = MaxImportPrice,
        ThousandsSeparator = true
    };
    private readonly DataGridView dgvReceiptDetails = new()
    {
        Name = "dgvReceiptDetails",
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };
    private readonly BindingList<ImportReceiptLineRow> _receiptLines = new();
    private readonly Button btnAddReceiptLine = new() { Name = "btnAddReceiptLine", Text = "Thêm sản phẩm" };
    private readonly Button btnRemoveReceiptLine = new() { Name = "btnRemoveReceiptLine", Text = "Xóa dòng" };
    private readonly Button btnCreateReceipt = new() { Name = "btnCreateReceipt", Text = "Tạo phiếu nhập" };
    private readonly Button btnRefreshReceiptData = new() { Name = "btnRefreshReceiptData", Text = "Tải lại dữ liệu" };
    private readonly Label lblReceiptTotal = new()
    {
        Name = "lblReceiptTotal",
        AutoSize = true,
        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
        ForeColor = Color.FromArgb(33, 37, 41),
        Text = "Tổng tiền: 0.00"
    };

    private void InitializeImportReceiptTab()
    {
        var tabPage = new TabPage("Tạo phiếu nhập")
        {
            Name = "tabCreateImportReceipt",
            Padding = new Padding(8)
        };
        var layout = new TableLayoutPanel
        {
            Name = "tblReceiptLayout",
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));

        var editor = new TableLayoutPanel
        {
            Name = "tblReceiptEditor",
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 1
        };
        for (var column = 0; column < editor.ColumnCount; column++)
        {
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }
        editor.Controls.Add(CreateReceiptField("Nhà cung cấp", cbxReceiptSupplier, "lblReceiptSupplier"), 0, 0);
        editor.Controls.Add(CreateReceiptField("Sản phẩm", cbxReceiptProduct, "lblReceiptProduct"), 1, 0);
        editor.Controls.Add(CreateReceiptField("Số lượng", nudReceiptQuantity, "lblReceiptQuantity"), 2, 0);
        editor.Controls.Add(CreateReceiptField("Đơn giá nhập", nudReceiptImportPrice, "lblReceiptImportPrice"), 3, 0);

        var lineActions = new FlowLayoutPanel
        {
            Name = "flpReceiptLineActions",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4),
            WrapContents = false
        };
        foreach (var button in new[] { btnAddReceiptLine, btnRemoveReceiptLine, btnRefreshReceiptData })
        {
            button.AutoSize = true;
            button.Margin = new Padding(6);
            button.Padding = new Padding(8, 4, 8, 4);
            lineActions.Controls.Add(button);
        }

        dgvReceiptDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colReceiptProductId",
            DataPropertyName = nameof(ImportReceiptLineRow.ProductId),
            HeaderText = "Mã sản phẩm",
            Width = 130
        });
        dgvReceiptDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colReceiptProductName",
            DataPropertyName = nameof(ImportReceiptLineRow.ProductName),
            HeaderText = "Sản phẩm",
            Width = 240
        });
        dgvReceiptDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colReceiptQuantity",
            DataPropertyName = nameof(ImportReceiptLineRow.Quantity),
            HeaderText = "Số lượng",
            Width = 120
        });
        dgvReceiptDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colReceiptImportPrice",
            DataPropertyName = nameof(ImportReceiptLineRow.ImportPrice),
            HeaderText = "Đơn giá nhập",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" },
            Width = 160
        });
        dgvReceiptDetails.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colReceiptLineTotal",
            DataPropertyName = nameof(ImportReceiptLineRow.LineTotal),
            HeaderText = "Thành tiền",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" },
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        dgvReceiptDetails.DataSource = _receiptLines;

        var footer = new TableLayoutPanel
        {
            Name = "tblReceiptFooter",
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 1
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        footer.Controls.Add(lblReceiptTotal, 0, 0);
        btnCreateReceipt.AutoSize = true;
        btnCreateReceipt.Anchor = AnchorStyles.Right;
        btnCreateReceipt.Padding = new Padding(10, 5, 10, 5);
        footer.Controls.Add(btnCreateReceipt, 1, 0);

        layout.Controls.Add(editor, 0, 0);
        layout.Controls.Add(lineActions, 0, 1);
        layout.Controls.Add(dgvReceiptDetails, 0, 2);
        layout.Controls.Add(footer, 0, 3);
        tabPage.Controls.Add(layout);
        _tabInventory.TabPages.Add(tabPage);

        btnAddReceiptLine.Click += BtnAddReceiptLine_Click;
        btnRemoveReceiptLine.Click += BtnRemoveReceiptLine_Click;
        btnRefreshReceiptData.Click += BtnRefreshReceiptData_Click;
        btnCreateReceipt.Click += BtnCreateReceipt_Click;
        dgvReceiptDetails.SelectionChanged += (_, _) =>
            btnRemoveReceiptLine.Enabled = dgvReceiptDetails.SelectedRows.Count > 0;
        _tabInventory.SelectedIndexChanged += async (_, _) =>
        {
            if (_tabInventory.SelectedTab == tabPage)
            {
                await LoadReceiptOptionsWithErrorHandlingAsync();
            }
        };

        btnRemoveReceiptLine.Enabled = false;
    }

    private static Panel CreateReceiptField(string labelText, Control input, string labelName)
    {
        var panel = new Panel
        {
            Name = $"pnl{labelName[3..]}",
            Dock = DockStyle.Fill,
            Margin = new Padding(6)
        };
        var label = new Label
        {
            Name = labelName,
            Dock = DockStyle.Top,
            Text = labelText,
            Height = 30,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 55, 72),
            TextAlign = ContentAlignment.MiddleLeft
        };

        input.Dock = DockStyle.Top;
        input.Margin = new Padding(0, 8, 0, 0);
        input.Font = new Font("Segoe UI", 11F);
        panel.Controls.Add(input);
        panel.Controls.Add(label);
        return panel;
    }

    private async Task LoadReceiptOptionsWithErrorHandlingAsync()
    {
        try
        {
            await LoadReceiptOptionsAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async Task LoadReceiptOptionsAsync()
    {
        var suppliers = await _apiClient.GetAsync<List<SupplierDto>>("api/suppliers") ?? new List<SupplierDto>();
        var products = await _apiClient.GetAsync<List<ProductDto>>("api/products") ?? new List<ProductDto>();

        cbxReceiptSupplier.DataSource = suppliers;
        cbxReceiptSupplier.DisplayMember = nameof(SupplierDto.Name);
        cbxReceiptSupplier.ValueMember = nameof(SupplierDto.Id);
        cbxReceiptProduct.DataSource = products;
        cbxReceiptProduct.DisplayMember = nameof(ProductDto.Name);
        cbxReceiptProduct.ValueMember = nameof(ProductDto.Id);
    }

    private void AddReceiptLine()
    {
        if (cbxReceiptProduct.SelectedItem is not ProductDto product)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cbxReceiptProduct.Focus();
            return;
        }

        var quantity = (int)nudReceiptQuantity.Value;
        var importPrice = nudReceiptImportPrice.Value;
        _receiptLines.Add(new ImportReceiptLineRow(
            product.Id,
            product.Name,
            quantity,
            importPrice,
            quantity * importPrice));
        UpdateReceiptTotal();
    }

    private void RemoveSelectedReceiptLine()
    {
        if (dgvReceiptDetails.CurrentRow?.DataBoundItem is ImportReceiptLineRow line)
        {
            _receiptLines.Remove(line);
            UpdateReceiptTotal();
        }
    }

    private void UpdateReceiptTotal()
    {
        lblReceiptTotal.Text = $"Tổng tiền: {_receiptLines.Sum(line => line.LineTotal):N2}";
    }

    private async Task CreateImportReceiptAsync()
    {
        if (cbxReceiptSupplier.SelectedValue is not int supplierId)
        {
            MessageBox.Show("Vui lòng chọn nhà cung cấp.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cbxReceiptSupplier.Focus();
            return;
        }

        if (_receiptLines.Count == 0)
        {
            MessageBox.Show("Vui lòng thêm ít nhất một sản phẩm vào phiếu nhập.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var response = await _apiClient.PostAsync("api/importreceipts", new CreateImportReceiptDto
        {
            SupplierId = supplierId,
            Details = _receiptLines.Select(line => new CreateImportReceiptDetailDto
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                ImportPrice = line.ImportPrice
            }).ToList()
        });
        await BaseHttpClient.EnsureSuccessAsync(response);
        var receipt = await response.Content.ReadFromJsonAsync<ImportReceiptDto>();
        _receiptLines.Clear();
        UpdateReceiptTotal();
        MessageBox.Show(
            receipt is null ? "Đã tạo phiếu nhập kho." : $"Đã tạo phiếu nhập kho #{receipt.Id}.",
            "Thông báo",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private async void BtnAddReceiptLine_Click(object? sender, EventArgs e)
    {
        try
        {
            AddReceiptLine();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private void BtnRemoveReceiptLine_Click(object? sender, EventArgs e)
    {
        RemoveSelectedReceiptLine();
    }

    private async void BtnRefreshReceiptData_Click(object? sender, EventArgs e)
    {
        await LoadReceiptOptionsWithErrorHandlingAsync();
    }

    private async void BtnCreateReceipt_Click(object? sender, EventArgs e)
    {
        try
        {
            await CreateImportReceiptAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private sealed record ImportReceiptLineRow(
        int ProductId,
        string ProductName,
        int Quantity,
        decimal ImportPrice,
        decimal LineTotal);
}
