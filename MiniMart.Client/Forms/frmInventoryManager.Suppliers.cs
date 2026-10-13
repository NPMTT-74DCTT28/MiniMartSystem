using System.ComponentModel;
using MiniMart.Client.HttpClients;
using MiniMart.Shared.DTOs.Suppliers;

namespace MiniMart.Client.Forms;

public partial class frmInventoryManager
{
    private readonly DataGridView dgvSuppliers = new()
    {
        Name = "dgvSuppliers",
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    private readonly TextBox txtSupplierId = new() { Name = "txtSupplierId", ReadOnly = true };
    private readonly TextBox txtSupplierName = new() { Name = "txtSupplierName" };
    private readonly TextBox txtSupplierPhone = new() { Name = "txtSupplierPhone" };
    private readonly TextBox txtSupplierAddress = new() { Name = "txtSupplierAddress" };
    private readonly Button btnAddSupplier = new() { Name = "btnAddSupplier", Text = "Thêm" };
    private readonly Button btnUpdateSupplier = new() { Name = "btnUpdateSupplier", Text = "Cập nhật" };
    private readonly Button btnDeleteSupplier = new() { Name = "btnDeleteSupplier", Text = "Xóa" };
    private readonly Button btnClearSupplier = new() { Name = "btnClearSupplier", Text = "Làm mới" };

    private void InitializeSupplierTab()
    {
        var tabPage = new TabPage("Nhà cung cấp")
        {
            Name = "tabSuppliers",
            Padding = new Padding(8)
        };

        var layout = new TableLayoutPanel
        {
            Name = "tblSuppliersLayout",
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var editor = new TableLayoutPanel
        {
            Name = "tblSupplierEditor",
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 1
        };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        editor.Controls.Add(CreateSupplierField("Mã nhà cung cấp", txtSupplierId, "lblSupplierId"), 0, 0);
        editor.Controls.Add(CreateSupplierField("Tên nhà cung cấp", txtSupplierName, "lblSupplierName"), 1, 0);
        editor.Controls.Add(CreateSupplierField("Số điện thoại", txtSupplierPhone, "lblSupplierPhone"), 2, 0);
        editor.Controls.Add(CreateSupplierField("Địa chỉ", txtSupplierAddress, "lblSupplierAddress"), 3, 0);

        var actions = new FlowLayoutPanel
        {
            Name = "flpSupplierActions",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4),
            WrapContents = false
        };
        foreach (var button in new[] { btnAddSupplier, btnUpdateSupplier, btnDeleteSupplier, btnClearSupplier })
        {
            button.AutoSize = true;
            button.Margin = new Padding(6);
            button.Padding = new Padding(8, 4, 8, 4);
            actions.Controls.Add(button);
        }

        dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colSupplierId",
            DataPropertyName = nameof(SupplierDto.Id),
            HeaderText = "Mã nhà cung cấp",
            Width = 140
        });
        dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colSupplierName",
            DataPropertyName = nameof(SupplierDto.Name),
            HeaderText = "Tên nhà cung cấp",
            Width = 260
        });
        dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colSupplierPhone",
            DataPropertyName = nameof(SupplierDto.Phone),
            HeaderText = "Số điện thoại",
            Width = 180
        });
        dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colSupplierAddress",
            DataPropertyName = nameof(SupplierDto.Address),
            HeaderText = "Địa chỉ",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        layout.Controls.Add(editor, 0, 0);
        layout.Controls.Add(actions, 0, 1);
        layout.Controls.Add(dgvSuppliers, 0, 2);
        tabPage.Controls.Add(layout);
        _tabInventory.TabPages.Add(tabPage);

        btnAddSupplier.Click += BtnAddSupplier_Click;
        btnUpdateSupplier.Click += BtnUpdateSupplier_Click;
        btnDeleteSupplier.Click += BtnDeleteSupplier_Click;
        btnClearSupplier.Click += BtnClearSupplier_Click;
        dgvSuppliers.SelectionChanged += (_, _) => SelectSupplier();
        Load += async (_, _) =>
        {
            try
            {
                await LoadSuppliersAsync();
            }
            catch (Exception exception)
            {
                ShowApiError(exception);
            }
        };

        ClearSupplierForm();
    }

    private static Panel CreateSupplierField(string labelText, TextBox textBox, string labelName)
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

        textBox.Dock = DockStyle.Top;
        textBox.Margin = new Padding(0, 8, 0, 0);
        textBox.Height = 35;
        textBox.Font = new Font("Segoe UI", 11F);
        textBox.BackColor = textBox.ReadOnly ? Color.FromArgb(240, 243, 247) : Color.White;
        textBox.ForeColor = Color.FromArgb(30, 30, 30);

        panel.Controls.Add(textBox);
        panel.Controls.Add(label);
        return panel;
    }

    private async Task LoadSuppliersAsync()
    {
        var suppliers = await _apiClient.GetAsync<List<SupplierDto>>("api/suppliers");
        dgvSuppliers.DataSource = new BindingList<SupplierDto>(suppliers ?? new List<SupplierDto>());
        ClearSupplierForm();
    }

    private void SelectSupplier()
    {
        if (dgvSuppliers.SelectedRows.Count == 0 || dgvSuppliers.CurrentRow?.DataBoundItem is not SupplierDto supplier)
        {
            return;
        }

        txtSupplierId.Text = supplier.Id.ToString();
        txtSupplierName.Text = supplier.Name;
        txtSupplierPhone.Text = supplier.Phone;
        txtSupplierAddress.Text = supplier.Address;
        btnUpdateSupplier.Enabled = true;
        btnDeleteSupplier.Enabled = true;
    }

    private void ClearSupplierForm()
    {
        txtSupplierId.Clear();
        txtSupplierName.Clear();
        txtSupplierPhone.Clear();
        txtSupplierAddress.Clear();
        dgvSuppliers.ClearSelection();
        btnUpdateSupplier.Enabled = false;
        btnDeleteSupplier.Enabled = false;
    }

    private bool ValidateSupplierForm()
    {
        if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên nhà cung cấp.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtSupplierName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtSupplierPhone.Text))
        {
            MessageBox.Show("Vui lòng nhập số điện thoại nhà cung cấp.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtSupplierPhone.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtSupplierAddress.Text))
        {
            MessageBox.Show("Vui lòng nhập địa chỉ nhà cung cấp.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtSupplierAddress.Focus();
            return false;
        }

        return true;
    }

    private async Task CreateSupplierAsync()
    {
        if (!ValidateSupplierForm())
        {
            return;
        }

        using var response = await _apiClient.PostAsync("api/suppliers", new CreateSupplierDto
        {
            Name = txtSupplierName.Text.Trim(),
            Phone = txtSupplierPhone.Text.Trim(),
            Address = txtSupplierAddress.Text.Trim()
        });
        await BaseHttpClient.EnsureSuccessAsync(response);
        await LoadSuppliersAsync();
        MessageBox.Show("Đã thêm nhà cung cấp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task UpdateSupplierAsync()
    {
        if (!int.TryParse(txtSupplierId.Text, out var supplierId) || !ValidateSupplierForm())
        {
            return;
        }

        using var response = await _apiClient.PutAsync($"api/suppliers/{supplierId}", new UpdateSupplierDto
        {
            Name = txtSupplierName.Text.Trim(),
            Phone = txtSupplierPhone.Text.Trim(),
            Address = txtSupplierAddress.Text.Trim()
        });
        await BaseHttpClient.EnsureSuccessAsync(response);
        await LoadSuppliersAsync();
        MessageBox.Show("Đã cập nhật nhà cung cấp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task DeleteSupplierAsync()
    {
        if (!int.TryParse(txtSupplierId.Text, out var supplierId))
        {
            return;
        }

        var confirmation = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa nhà cung cấp này?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        using var response = await _apiClient.DeleteAsync($"api/suppliers/{supplierId}");
        await BaseHttpClient.EnsureSuccessAsync(response);
        await LoadSuppliersAsync();
        MessageBox.Show("Đã xóa nhà cung cấp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void BtnAddSupplier_Click(object? sender, EventArgs e)
    {
        try
        {
            await CreateSupplierAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async void BtnUpdateSupplier_Click(object? sender, EventArgs e)
    {
        try
        {
            await UpdateSupplierAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async void BtnDeleteSupplier_Click(object? sender, EventArgs e)
    {
        try
        {
            await DeleteSupplierAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async void BtnClearSupplier_Click(object? sender, EventArgs e)
    {
        try
        {
            await LoadSuppliersAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }
}
