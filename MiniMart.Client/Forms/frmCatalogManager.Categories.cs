using System.ComponentModel;
using MiniMart.Shared.DTOs.Categories;

namespace MiniMart.Client.Forms;

public partial class frmCatalogManager
{
    private readonly DataGridView dgvCategories = new()
    {
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    private readonly TextBox txtCategoryName = new();
    private readonly TextBox txtCategoryDescription = new();
    private readonly Button btnCreateCategory = new() { Name = "btnCreateCategory", Text = "Thêm danh mục" };
    private readonly Button btnUpdateCategory = new() { Name = "btnUpdateCategory", Text = "Cập nhật" };
    private readonly Button btnDeleteCategory = new() { Name = "btnDeleteCategory", Text = "Xóa" };
    private readonly Button btnClearCategory = new() { Name = "btnClearCategory", Text = "Làm mới biểu mẫu" };
    private readonly Button btnRefreshCategories = new() { Name = "btnRefreshCategories", Text = "Tải lại" };
    private int? _selectedCategoryId;

    private void InitializeCategoryTab()
    {
        var tabPage = new TabPage("Danh mục");
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var editor = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            WrapContents = true,
            AutoScroll = true
        };
        editor.Controls.Add(CreateCategoryField("Tên danh mục", txtCategoryName, 260));
        editor.Controls.Add(CreateCategoryField("Mô tả", txtCategoryDescription, 320));
        editor.Controls.Add(btnCreateCategory);
        editor.Controls.Add(btnUpdateCategory);
        editor.Controls.Add(btnDeleteCategory);
        editor.Controls.Add(btnClearCategory);
        editor.Controls.Add(btnRefreshCategories);

        btnUpdateCategory.Enabled = false;
        btnDeleteCategory.Enabled = false;
        foreach (var button in new[] { btnCreateCategory, btnUpdateCategory, btnDeleteCategory, btnClearCategory, btnRefreshCategories })
        {
            button.AutoSize = true;
            button.Margin = new Padding(6, 22, 6, 6);
            button.Padding = new Padding(8, 4, 8, 4);
        }

        dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(CategoryDto.Id),
            HeaderText = "Mã",
            Width = 80
        });
        dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(CategoryDto.Name),
            HeaderText = "Tên danh mục",
            Width = 220
        });
        dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DataPropertyName = nameof(CategoryDto.Description),
            HeaderText = "Mô tả"
        });

        layout.Controls.Add(editor, 0, 0);
        layout.Controls.Add(dgvCategories, 0, 1);
        tabPage.Controls.Add(layout);
        _tabCatalog.TabPages.Add(tabPage);

        btnCreateCategory.Click += async (_, _) => await CreateCategoryAsync();
        btnUpdateCategory.Click += async (_, _) => await UpdateCategoryAsync();
        btnDeleteCategory.Click += async (_, _) => await DeleteCategoryAsync();
        btnClearCategory.Click += (_, _) => ClearCategoryForm();
        btnRefreshCategories.Click += async (_, _) => await RefreshCategoriesAsync();
        dgvCategories.SelectionChanged += (_, _) => SelectCategory();
        Load += async (_, _) => await RefreshCategoriesAsync();
    }

    private static Panel CreateCategoryField(string labelText, TextBox textBox, int width)
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
        textBox.Dock = DockStyle.Bottom;
        textBox.Width = width - 8;
        panel.Controls.Add(textBox);
        panel.Controls.Add(label);
        return panel;
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _apiClient.GetAsync<List<CategoryDto>>("api/categories");
        dgvCategories.DataSource = new BindingList<CategoryDto>(categories ?? new List<CategoryDto>());
        ClearCategoryForm();
    }

    private async Task RefreshCategoriesAsync()
    {
        try
        {
            await LoadCategoriesAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async Task CreateCategoryAsync()
    {
        if (!ValidateCategoryForm())
        {
            return;
        }

        try
        {
            using var response = await _apiClient.PostAsync("api/categories", new CreateCategoryDto
            {
                Name = txtCategoryName.Text.Trim(),
                Description = txtCategoryDescription.Text.Trim()
            });
            await EnsureSuccessAsync(response);
            await LoadCategoriesAsync();
            MessageBox.Show("Đã thêm danh mục.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async Task UpdateCategoryAsync()
    {
        if (_selectedCategoryId is null || !ValidateCategoryForm())
        {
            return;
        }

        try
        {
            using var response = await _apiClient.PutAsync($"api/categories/{_selectedCategoryId}", new UpdateCategoryDto
            {
                Name = txtCategoryName.Text.Trim(),
                Description = txtCategoryDescription.Text.Trim()
            });
            await EnsureSuccessAsync(response);
            await LoadCategoriesAsync();
            MessageBox.Show("Đã cập nhật danh mục.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async Task DeleteCategoryAsync()
    {
        if (_selectedCategoryId is null)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa danh mục này?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using var response = await _apiClient.DeleteAsync($"api/categories/{_selectedCategoryId}");
            await EnsureSuccessAsync(response);
            await LoadCategoriesAsync();
            MessageBox.Show("Đã xóa danh mục.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private bool ValidateCategoryForm()
    {
        if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên danh mục.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCategoryName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtCategoryDescription.Text))
        {
            MessageBox.Show("Vui lòng nhập mô tả danh mục.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCategoryDescription.Focus();
            return false;
        }

        return true;
    }

    private void SelectCategory()
    {
        if (dgvCategories.SelectedRows.Count == 0 || dgvCategories.CurrentRow?.DataBoundItem is not CategoryDto category)
        {
            return;
        }

        _selectedCategoryId = category.Id;
        txtCategoryName.Text = category.Name;
        txtCategoryDescription.Text = category.Description;
        btnUpdateCategory.Enabled = true;
        btnDeleteCategory.Enabled = true;
    }

    private void ClearCategoryForm()
    {
        _selectedCategoryId = null;
        txtCategoryName.Clear();
        txtCategoryDescription.Clear();
        dgvCategories.ClearSelection();
        btnUpdateCategory.Enabled = false;
        btnDeleteCategory.Enabled = false;
    }
}
