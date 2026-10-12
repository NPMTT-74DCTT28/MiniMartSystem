using System.ComponentModel;
using MiniMart.Client.HttpClients;
using MiniMart.Shared.DTOs.Customer;

namespace MiniMart.Client.Forms;

public class frmCustomerManager : Form
{
    private readonly BaseHttpClient _apiClient = new();

    private readonly DataGridView dgvCustomers = new()
    {
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    private readonly TextBox txtId = new() { ReadOnly = true };
    private readonly TextBox txtFullName = new();
    private readonly TextBox txtPhoneNumber = new();
    private readonly TextBox txtTotalPoints = new() { ReadOnly = true };
    private readonly Button btnAdd = new() { Name = "btnAdd", Text = "Thêm" };
    private readonly Button btnEdit = new() { Name = "btnEdit", Text = "Cập nhật" };
    private readonly Button btnDelete = new() { Name = "btnDelete", Text = "Xóa" };
    private readonly Button btnUpdatePoints = new() { Name = "btnUpdatePoints", Text = "Cập nhật điểm" };
    private readonly Button btnClear = new() { Name = "btnClear", Text = "Làm mới" };

    public frmCustomerManager()
    {
        Text = "Quản lý khách hàng";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Width = 1100;
        Height = 700;

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var editor = new TableLayoutPanel
        {
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 1
        };
        for (var column = 0; column < editor.ColumnCount; column++)
        {
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }
        editor.Controls.Add(CreateCustomerField("Mã khách hàng", txtId), 0, 0);
        editor.Controls.Add(CreateCustomerField("Họ và tên", txtFullName), 1, 0);
        editor.Controls.Add(CreateCustomerField("Số điện thoại", txtPhoneNumber), 2, 0);
        editor.Controls.Add(CreateCustomerField("Tổng điểm", txtTotalPoints), 3, 0);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4),
            WrapContents = false
        };
        foreach (var button in new[] { btnAdd, btnEdit, btnDelete, btnUpdatePoints, btnClear })
        {
            button.AutoSize = true;
            button.Margin = new Padding(6);
            button.Padding = new Padding(8, 4, 8, 4);
            actions.Controls.Add(button);
        }

        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(CustomerDto.Id),
            HeaderText = "Mã khách hàng",
            Width = 140
        });
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(CustomerDto.FullName),
            HeaderText = "Họ và tên",
            Width = 260
        });
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(CustomerDto.PhoneNumber),
            HeaderText = "Số điện thoại",
            Width = 180
        });
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DataPropertyName = nameof(CustomerDto.TotalPoints),
            HeaderText = "Tổng điểm"
        });

        layout.Controls.Add(editor, 0, 0);
        layout.Controls.Add(actions, 0, 1);
        layout.Controls.Add(dgvCustomers, 0, 2);
        Controls.Add(layout);

        btnAdd.Click += BtnAdd_Click;
        btnEdit.Click += BtnEdit_Click;
        btnDelete.Click += BtnDelete_Click;
        btnUpdatePoints.Click += BtnUpdatePoints_Click;
        btnClear.Click += BtnClear_Click;
        dgvCustomers.SelectionChanged += (_, _) => SelectCustomer();
        Load += async (_, _) =>
        {
            try
            {
                await LoadCustomersAsync();
            }
            catch (Exception exception)
            {
                ShowApiError(exception);
            }
        };
    }

    private async Task LoadCustomersAsync()
    {
        var customers = await _apiClient.GetAsync<List<CustomerDto>>("api/customers");
        dgvCustomers.DataSource = new BindingList<CustomerDto>(customers ?? new List<CustomerDto>());
        ClearCustomerForm();
    }

    private void SelectCustomer()
    {
        if (dgvCustomers.SelectedRows.Count == 0 || dgvCustomers.CurrentRow?.DataBoundItem is not CustomerDto customer)
        {
            return;
        }

        txtId.Text = customer.Id.ToString();
        txtFullName.Text = customer.FullName;
        txtPhoneNumber.Text = customer.PhoneNumber;
        txtTotalPoints.Text = customer.TotalPoints.ToString();
    }

    private void ClearCustomerForm()
    {
        txtId.Clear();
        txtFullName.Clear();
        txtPhoneNumber.Clear();
        txtTotalPoints.Clear();
        dgvCustomers.ClearSelection();
    }

    private static void ShowApiError(Exception exception)
    {
        MessageBox.Show(exception.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        response.EnsureSuccessStatusCode();
    }

    private bool ValidateCustomerForm()
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text))
        {
            MessageBox.Show("Vui lòng nhập họ và tên khách hàng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtFullName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
        {
            MessageBox.Show("Vui lòng nhập số điện thoại khách hàng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPhoneNumber.Focus();
            return false;
        }

        return true;
    }

    private bool TryGetSelectedCustomerId(out int customerId)
    {
        if (dgvCustomers.SelectedRows.Count > 0 && dgvCustomers.CurrentRow?.DataBoundItem is CustomerDto customer)
        {
            customerId = customer.Id;
            return true;
        }

        customerId = 0;
        MessageBox.Show("Vui lòng chọn khách hàng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
    }

    private async Task CreateCustomerAsync()
    {
        if (!ValidateCustomerForm())
        {
            return;
        }

        using var response = await _apiClient.PostAsync("api/customers", new CreateCustomerDto
        {
            FullName = txtFullName.Text.Trim(),
            PhoneNumber = txtPhoneNumber.Text.Trim()
        });
        await EnsureSuccessAsync(response);
        await LoadCustomersAsync();
        MessageBox.Show("Đã thêm khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task UpdateCustomerAsync()
    {
        if (!TryGetSelectedCustomerId(out var customerId) || !ValidateCustomerForm())
        {
            return;
        }

        using var response = await _apiClient.PutAsync($"api/customers/{customerId}", new UpdateCustomerDto
        {
            FullName = txtFullName.Text.Trim(),
            PhoneNumber = txtPhoneNumber.Text.Trim()
        });
        await EnsureSuccessAsync(response);
        await LoadCustomersAsync();
        MessageBox.Show("Đã cập nhật khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task DeleteCustomerAsync()
    {
        if (!TryGetSelectedCustomerId(out var customerId))
        {
            return;
        }

        var confirmation = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa khách hàng này?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        using var response = await _apiClient.DeleteAsync($"api/customers/{customerId}");
        await EnsureSuccessAsync(response);
        await LoadCustomersAsync();
        MessageBox.Show("Đã xóa khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task UpdatePointsAsync()
    {
        if (!TryGetSelectedCustomerId(out var customerId))
        {
            return;
        }

        var pointsText = Microsoft.VisualBasic.Interaction.InputBox(
            "Nhập tổng điểm tích lũy mới:",
            "Cập nhật điểm",
            txtTotalPoints.Text);
        if (string.IsNullOrWhiteSpace(pointsText))
        {
            return;
        }

        if (!int.TryParse(pointsText, out var totalPoints) || totalPoints < 0)
        {
            MessageBox.Show("Vui lòng nhập số điểm nguyên không âm.", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var response = await _apiClient.PatchAsync($"api/customers/{customerId}/points", new UpdatePointsDto
        {
            TotalPoints = totalPoints
        });
        await EnsureSuccessAsync(response);
        await LoadCustomersAsync();
        MessageBox.Show("Đã cập nhật điểm khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static Panel CreateCustomerField(string labelText, TextBox textBox)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6)
        };

        var label = new Label
        {
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
        textBox.BackColor = textBox.ReadOnly
            ? Color.FromArgb(240, 243, 247)
            : Color.White;
        textBox.ForeColor = Color.FromArgb(30, 30, 30);

        panel.Controls.Add(textBox);
        panel.Controls.Add(label);

        return panel;
    }


    private async void BtnAdd_Click(object? sender, EventArgs e)
    {
        try
        {
            await CreateCustomerAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async void BtnEdit_Click(object? sender, EventArgs e)
    {
        try
        {
            await UpdateCustomerAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        try
        {
            await DeleteCustomerAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async void BtnUpdatePoints_Click(object? sender, EventArgs e)
    {
        try
        {
            await UpdatePointsAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }

    private async void BtnClear_Click(object? sender, EventArgs e)
    {
        try
        {
            await LoadCustomersAsync();
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }
}