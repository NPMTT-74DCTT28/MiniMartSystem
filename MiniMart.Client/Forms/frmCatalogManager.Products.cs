using System.ComponentModel;
using MiniMart.Shared.DTOs.Categories;
using MiniMart.Shared.DTOs.Products;

namespace MiniMart.Client.Forms;

public partial class frmCatalogManager
{
    private readonly DataGridView dgvProducts = new()
    {
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    private readonly TextBox txtProductName = new();
    private readonly ComboBox cbxProductCategory = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown nudProductPrice = new() { DecimalPlaces = 2, Maximum = 1000000000, Minimum = 0 };
    private readonly NumericUpDown nudProductStock = new() { Maximum = int.MaxValue, Minimum = 0, ThousandsSeparator = true };
    private readonly TextBox txtProductImageUrl = new();
    private readonly Button btnCreateProduct = new() { Name = "btnCreateProduct", Text = "Thêm sản phẩm" };
    private readonly Button btnUpdateProduct = new() { Name = "btnUpdateProduct", Text = "Cập nhật" };
    private readonly Button btnDeleteProduct = new() { Name = "btnDeleteProduct", Text = "Xóa" };
    private readonly Button btnClearProduct = new() { Name = "btnClearProduct", Text = "Làm mới biểu mẫu" };
    private readonly Button btnRefreshProducts = new() { Name = "btnRefreshProducts", Text = "Tải lại" };
    private int? _selectedProductId;

    private void InitializeProductTab()
    {
        var tabPage = new TabPage("Sản phẩm");
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var editor = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            WrapContents = true,
            AutoScroll = true
        };
        editor.Controls.Add(CreateProductField("Tên sản phẩm", txtProductName, 220));
        editor.Controls.Add(CreateProductField("Danh mục", cbxProductCategory, 190));
        editor.Controls.Add(CreateProductField("Giá", nudProductPrice, 130));
        editor.Controls.Add(CreateProductField("Số lượng tồn", nudProductStock, 130));
        editor.Controls.Add(CreateProductField("Đường dẫn hình ảnh", txtProductImageUrl, 260));
        editor.Controls.Add(btnCreateProduct);
        editor.Controls.Add(btnUpdateProduct);
        editor.Controls.Add(btnDeleteProduct);
        editor.Controls.Add(btnClearProduct);
        editor.Controls.Add(btnRefreshProducts);

        btnUpdateProduct.Enabled = false;
        btnDeleteProduct.Enabled = false;
        foreach (var button in new[] { btnCreateProduct, btnUpdateProduct, btnDeleteProduct, btnClearProduct, btnRefreshProducts })
        {
            button.AutoSize = true;
            button.Margin = new Padding(6, 22, 6, 6);
            button.Padding = new Padding(8, 4, 8, 4);
        }

        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProductGridItem.Id),
            HeaderText = "Mã",
            Width = 65
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProductGridItem.Name),
            HeaderText = "Tên sản phẩm",
            Width = 180
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProductGridItem.CategoryName),
            HeaderText = "Danh mục",
            Width = 160
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProductGridItem.Price),
            HeaderText = "Giá",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProductGridItem.StockQuantity),
            HeaderText = "Tồn kho",
            Width = 90
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DataPropertyName = nameof(ProductGridItem.ImageUrl),
            HeaderText = "Đường dẫn hình ảnh"
        });

        layout.Controls.Add(editor, 0, 0);
        layout.Controls.Add(dgvProducts, 0, 1);
        tabPage.Controls.Add(layout);
        _tabCatalog.TabPages.Add(tabPage);

        btnCreateProduct.Click += async (_, _) => await CreateProductAsync();
        btnUpdateProduct.Click += async (_, _) => await UpdateProductAsync();
        btnDeleteProduct.Click += async (_, _) => await DeleteProductAsync();
        btnClearProduct.Click += (_, _) => ClearProductForm();
        btnRefreshProducts.Click += async (_, _) => await RefreshProductsAsync();
        dgvProducts.SelectionChanged += (_, _) => SelectProduct();
        Load += async (_, _) => await RefreshProductsAsync();
    }

    private static Panel CreateProductField(string labelText, Control control, int width)
    {
        var panel = new Panel
        {
            Height = 62,
            Margin = new Padding(6),
            Width = width
        };
        var label = new Label
        {
            Dock = DockStyle.Top,
            Text = labelText
        };
        control.Dock = DockStyle.Bottom;
        control.Width = width - 8;
        panel.Controls.Add(control);
        panel.Controls.Add(label);
        return panel;
    }

    private async Task LoadProductsAsync()
    {
        var categories = await _apiClient.GetAsync<List<CategoryDto>>("api/categories") ?? new List<CategoryDto>();
        var products = await _apiClient.GetAsync<List<ProductDto>>("api/products") ?? new List<ProductDto>();
        var categoryNames = categories.ToDictionary(category => category.Id, category => category.Name);

        cbxProductCategory.DataSource = new BindingList<CategoryDto>(categories);
        cbxProductCategory.DisplayMember = nameof(CategoryDto.Name);
        cbxProductCategory.ValueMember = nameof(CategoryDto.Id);

        dgvProducts.DataSource = new BindingList<ProductGridItem>(products
            .Select(product => new ProductGridItem(
                product.Id,
                product.Name,
                categoryNames.GetValueOrDefault(product.CategoryId, "Không xác định"),
                product.CategoryId,
                product.Price,
                product.StockQuantity,
                product.ImageUrl))
            .ToList());
        ClearProductForm();
    }

    private async Task RefreshProductsAsync()
    {
        try
        {
            await LoadProductsAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async Task CreateProductAsync()
    {
        if (!ValidateProductForm(out var categoryId))
        {
            return;
        }

        try
        {
            using var response = await _apiClient.PostAsync("api/products", new CreateProductDto
            {
                Name = txtProductName.Text.Trim(),
                CategoryId = categoryId,
                Price = nudProductPrice.Value,
                StockQuantity = (int)nudProductStock.Value,
                ImageUrl = txtProductImageUrl.Text.Trim()
            });
            await EnsureSuccessAsync(response);
            await LoadProductsAsync();
            MessageBox.Show("Đã thêm sản phẩm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async Task UpdateProductAsync()
    {
        if (_selectedProductId is null || !ValidateProductForm(out var categoryId))
        {
            return;
        }

        try
        {
            using var response = await _apiClient.PutAsync($"api/products/{_selectedProductId}", new UpdateProductDto
            {
                Name = txtProductName.Text.Trim(),
                CategoryId = categoryId,
                Price = nudProductPrice.Value,
                StockQuantity = (int)nudProductStock.Value,
                ImageUrl = txtProductImageUrl.Text.Trim()
            });
            await EnsureSuccessAsync(response);
            await LoadProductsAsync();
            MessageBox.Show("Đã cập nhật sản phẩm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async Task DeleteProductAsync()
    {
        if (_selectedProductId is null)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa sản phẩm này?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using var response = await _apiClient.DeleteAsync($"api/products/{_selectedProductId}");
            await EnsureSuccessAsync(response);
            await LoadProductsAsync();
            MessageBox.Show("Đã xóa sản phẩm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private bool ValidateProductForm(out int categoryId)
    {
        categoryId = 0;
        if (string.IsNullOrWhiteSpace(txtProductName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên sản phẩm.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtProductName.Focus();
            return false;
        }

        if (cbxProductCategory.SelectedValue is not int selectedCategoryId)
        {
            MessageBox.Show("Vui lòng chọn danh mục.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cbxProductCategory.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtProductImageUrl.Text))
        {
            MessageBox.Show("Vui lòng nhập đường dẫn hình ảnh.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtProductImageUrl.Focus();
            return false;
        }

        categoryId = selectedCategoryId;
        return true;
    }

    private void SelectProduct()
    {
        if (dgvProducts.SelectedRows.Count == 0 || dgvProducts.CurrentRow?.DataBoundItem is not ProductGridItem product)
        {
            return;
        }

        _selectedProductId = product.Id;
        txtProductName.Text = product.Name;
        cbxProductCategory.SelectedValue = product.CategoryId;
        nudProductPrice.Value = product.Price;
        nudProductStock.Value = product.StockQuantity;
        txtProductImageUrl.Text = product.ImageUrl;
        btnUpdateProduct.Enabled = true;
        btnDeleteProduct.Enabled = true;
    }

    private void ClearProductForm()
    {
        _selectedProductId = null;
        txtProductName.Clear();
        if (cbxProductCategory.Items.Count > 0)
        {
            cbxProductCategory.SelectedIndex = 0;
        }
        nudProductPrice.Value = 0;
        nudProductStock.Value = 0;
        txtProductImageUrl.Clear();
        dgvProducts.ClearSelection();
        btnUpdateProduct.Enabled = false;
        btnDeleteProduct.Enabled = false;
    }

    private sealed record ProductGridItem(
        int Id,
        string Name,
        string CategoryName,
        int CategoryId,
        decimal Price,
        int StockQuantity,
        string ImageUrl);
}
