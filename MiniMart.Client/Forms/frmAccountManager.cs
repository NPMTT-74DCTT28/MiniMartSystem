using System.ComponentModel;
using System.Net;
using MiniMart.Client.HttpClients;
using MiniMart.Shared.DTOs.Users;
using MiniMart.Shared.Enums;

namespace MiniMart.Client.Forms;

/// <summary>
/// Màn hình quản lý tài khoản nhân viên (dành cho Admin): xem danh sách, tạo, cập nhật,
/// vô hiệu hóa và xóa tài khoản thông qua <c>api/users</c>.
/// </summary>
/// <remarks>
/// Mật khẩu không bao giờ được đọc từ API (API không trả <c>PasswordHash</c>); ô mật khẩu chỉ dùng
/// để gửi mật khẩu mới lên server. Bỏ trống khi cập nhật nghĩa là giữ nguyên mật khẩu cũ.
/// </remarks>
public class frmAccountManager : Form
{
    private readonly BaseHttpClient _apiClient = new();

    private readonly DataGridView dgvUsers = new()
    {
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        Dock = DockStyle.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    private readonly TextBox txtId = new() { ReadOnly = true };
    private readonly TextBox txtUsername = new();
    private readonly TextBox txtPassword = new() { UseSystemPasswordChar = true };
    private readonly ComboBox cbxRole = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox chkIsActive = new() { Text = "Đang hoạt động", Checked = true };
    private readonly CheckBox chkShowPassword = new() { Text = "Hiện mật khẩu" };
    private readonly Label lblPasswordHint = new()
    {
        AutoSize = false,
        ForeColor = Color.FromArgb(107, 114, 125),
        Height = 20,
        Text = "Để trống nếu không đổi mật khẩu."
    };
    private readonly Button btnAdd = new() { Name = "btnAdd", Text = "Thêm" };
    private readonly Button btnEdit = new() { Name = "btnEdit", Text = "Cập nhật" };
    private readonly Button btnDelete = new() { Name = "btnDelete", Text = "Xóa" };
    private readonly Button btnClear = new() { Name = "btnClear", Text = "Làm mới" };

    public frmAccountManager()
    {
        Text = "Quản lý tài khoản nhân viên";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Width = 1100;
        Height = 700;

        // Vai trò dùng enum UserRole nên không hardcode chuỗi ở giao diện.
        cbxRole.DataSource = Enum.GetValues<UserRole>();
        cbxRole.SelectedItem = UserRole.Staff;

        BuildLayout();

        btnAdd.Click += BtnAdd_Click;
        btnEdit.Click += BtnEdit_Click;
        btnDelete.Click += BtnDelete_Click;
        btnClear.Click += BtnClear_Click;
        chkShowPassword.CheckedChanged += (_, _) =>
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        dgvUsers.SelectionChanged += (_, _) => SelectUser();
        Load += async (_, _) =>
        {
            try
            {
                await LoadUsersAsync();
            }
            catch (Exception exception)
            {
                ShowApiError(exception);
            }
        };
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var editor = new TableLayoutPanel
        {
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 2
        };
        for (var column = 0; column < editor.ColumnCount; column++)
        {
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

        editor.Controls.Add(CreateField("Mã tài khoản", txtId), 0, 0);
        editor.Controls.Add(CreateField("Tên đăng nhập", txtUsername), 1, 0);
        editor.Controls.Add(CreateField("Mật khẩu mới", txtPassword), 2, 0);

        var rolePanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(6) };
        cbxRole.Dock = DockStyle.Top;
        cbxRole.Font = new Font("Segoe UI", 11F);
        cbxRole.Height = 32;
        rolePanel.Controls.Add(cbxRole);
        rolePanel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 55, 72),
            Height = 30,
            Text = "Vai trò",
            TextAlign = ContentAlignment.MiddleLeft
        });
        editor.Controls.Add(rolePanel, 3, 0);

        var options = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(6),
            WrapContents = false
        };
        chkIsActive.AutoSize = true;
        chkIsActive.Margin = new Padding(0, 6, 18, 0);
        chkShowPassword.AutoSize = true;
        chkShowPassword.Margin = new Padding(0, 6, 18, 0);
        options.Controls.Add(chkIsActive);
        options.Controls.Add(chkShowPassword);
        options.Controls.Add(lblPasswordHint);
        editor.Controls.Add(options, 0, 1);
        editor.SetColumnSpan(options, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4),
            WrapContents = false
        };
        foreach (var button in new[] { btnAdd, btnEdit, btnDelete, btnClear })
        {
            button.AutoSize = true;
            button.Margin = new Padding(6);
            button.Padding = new Padding(8, 4, 8, 4);
            actions.Controls.Add(button);
        }

        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(UserDto.Id),
            HeaderText = "Mã tài khoản",
            Width = 280
        });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(UserDto.Username),
            HeaderText = "Tên đăng nhập",
            Width = 220
        });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(UserDto.Role),
            HeaderText = "Vai trò",
            Width = 140
        });
        dgvUsers.Columns.Add(new DataGridViewCheckBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DataPropertyName = nameof(UserDto.IsActive),
            HeaderText = "Đang hoạt động"
        });

        layout.Controls.Add(editor, 0, 0);
        layout.Controls.Add(actions, 0, 1);
        layout.Controls.Add(dgvUsers, 0, 2);
        Controls.Add(layout);
    }

    private static Panel CreateField(string labelText, TextBox textBox)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6)
        };

        panel.Controls.Add(textBox);
        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 55, 72),
            Height = 30,
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft
        });

        textBox.Dock = DockStyle.Top;
        textBox.Font = new Font("Segoe UI", 11F);
        textBox.Height = 32;
        textBox.BackColor = textBox.ReadOnly ? Color.FromArgb(240, 243, 247) : Color.White;

        return panel;
    }

    private async Task LoadUsersAsync()
    {
        var users = await _apiClient.GetAsync<List<UserDto>>("api/users");
        dgvUsers.DataSource = new BindingList<UserDto>(users ?? new List<UserDto>());
        ClearForm();
    }

    private void SelectUser()
    {
        if (dgvUsers.SelectedRows.Count == 0 || dgvUsers.CurrentRow?.DataBoundItem is not UserDto user)
        {
            return;
        }

        txtId.Text = user.Id.ToString();
        txtUsername.Text = user.Username;
        txtPassword.Clear();
        cbxRole.SelectedItem = user.Role;
        chkIsActive.Checked = user.IsActive;
    }

    private void ClearForm()
    {
        txtId.Clear();
        txtUsername.Clear();
        txtPassword.Clear();
        cbxRole.SelectedItem = UserRole.Staff;
        chkIsActive.Checked = true;
        dgvUsers.ClearSelection();
    }

    private bool ValidateForm(bool isCreating)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text))
        {
            MessageBox.Show("Vui lòng nhập tên đăng nhập.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUsername.Focus();
            return false;
        }

        if (isCreating && string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Vui lòng nhập mật khẩu cho tài khoản mới.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPassword.Focus();
            return false;
        }

        if (cbxRole.SelectedItem is not UserRole)
        {
            MessageBox.Show("Vui lòng chọn vai trò cho tài khoản.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private bool TryGetSelectedUserId(out Guid userId)
    {
        if (dgvUsers.SelectedRows.Count > 0 && dgvUsers.CurrentRow?.DataBoundItem is UserDto user)
        {
            userId = user.Id;
            return true;
        }

        userId = Guid.Empty;
        MessageBox.Show("Vui lòng chọn tài khoản.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
    }

    private async Task CreateUserAsync()
    {
        if (!ValidateForm(isCreating: true))
        {
            return;
        }

        using var response = await _apiClient.PostAsync("api/users", new CreateUserDto
        {
            Username = txtUsername.Text.Trim(),
            Password = txtPassword.Text,
            Role = (UserRole)cbxRole.SelectedItem!,
            IsActive = chkIsActive.Checked
        });

        await BaseHttpClient.EnsureSuccessAsync(response);
        await LoadUsersAsync();
        MessageBox.Show("Đã thêm tài khoản nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task UpdateUserAsync()
    {
        if (!TryGetSelectedUserId(out var userId) || !ValidateForm(isCreating: false))
        {
            return;
        }

        using var response = await _apiClient.PutAsync($"api/users/{userId}", new UpdateUserDto
        {
            Username = txtUsername.Text.Trim(),
            Password = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text,
            Role = (UserRole)cbxRole.SelectedItem!,
            IsActive = chkIsActive.Checked
        });

        await BaseHttpClient.EnsureSuccessAsync(response);
        await LoadUsersAsync();
        MessageBox.Show("Đã cập nhật tài khoản nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task DeleteUserAsync()
    {
        if (!TryGetSelectedUserId(out var userId))
        {
            return;
        }

        var confirmation = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa tài khoản này?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        using var response = await _apiClient.DeleteAsync($"api/users/{userId}");
        await BaseHttpClient.EnsureSuccessAsync(response);
        await LoadUsersAsync();
        MessageBox.Show("Đã xóa tài khoản nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void ShowApiError(Exception exception)
    {
        MessageBox.Show(exception.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private async void BtnAdd_Click(object? sender, EventArgs e)
    {
        try
        {
            await CreateUserAsync();
        }
        catch (HttpRequestException)
        {
            ShowApiError(new InvalidOperationException("Không kết nối được tới API. Hãy chắc chắn API đang chạy."));
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
            await UpdateUserAsync();
        }
        catch (HttpRequestException)
        {
            ShowApiError(new InvalidOperationException("Không kết nối được tới API. Hãy chắc chắn API đang chạy."));
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
            await DeleteUserAsync();
        }
        catch (HttpRequestException)
        {
            ShowApiError(new InvalidOperationException("Không kết nối được tới API. Hãy chắc chắn API đang chạy."));
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
            await LoadUsersAsync();
        }
        catch (HttpRequestException)
        {
            ShowApiError(new InvalidOperationException("Không kết nối được tới API. Hãy chắc chắn API đang chạy."));
        }
        catch (Exception exception)
        {
            ShowApiError(exception);
        }
    }
}
