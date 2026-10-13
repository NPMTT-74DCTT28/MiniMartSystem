using System.Net;
using System.Net.Http.Json;
using MiniMart.Client.HttpClients;
using MiniMart.Client.Security;
using MiniMart.Shared.DTOs.Auth;
using MiniMart.Shared.DTOs.Users;

namespace MiniMart.Client.Forms;

/// <summary>
/// Màn hình đăng nhập: gửi tên đăng nhập/mật khẩu tới <c>POST api/auth/login</c>,
/// lưu phiên đăng nhập vào <see cref="AuthSession"/> và đóng với <see cref="DialogResult.OK"/> khi thành công.
/// </summary>
/// <remarks>
/// Form chạy ở chế độ hộp thoại: người gọi (xem <c>Program.cs</c>) mở bằng <c>ShowDialog()</c>,
/// kiểm tra <see cref="DialogResult"/>, rồi mở <see cref="frmMain"/> với tài khoản vừa đăng nhập.
/// </remarks>
public class frmLogin : Form
{
    private static readonly Color PrimaryColor = Color.FromArgb(37, 99, 235);
    private static readonly Color ErrorColor = Color.FromArgb(185, 28, 28);
    private static readonly Color MutedColor = Color.FromArgb(107, 114, 125);

    private readonly BaseHttpClient _apiClient = new();

    private readonly TextBox txtUsername = new() { Name = "txtUsername" };
    private readonly TextBox txtPassword = new() { Name = "txtPassword", UseSystemPasswordChar = true };
    private readonly CheckBox chkShowPassword = new()
    {
        AutoSize = true,
        Name = "chkShowPassword",
        Text = "Hiện mật khẩu"
    };
    private readonly Button btnLogin = new() { Name = "btnLogin", Text = "Đăng nhập" };
    private readonly Button btnExit = new() { Name = "btnExit", Text = "Thoát" };
    private readonly Label lblMessage = new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        ForeColor = ErrorColor,
        Height = 40,
        Name = "lblMessage",
        TextAlign = ContentAlignment.MiddleLeft
    };

    public frmLogin()
    {
        Text = "Đăng nhập - MiniMart System";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(460, 424);

        BuildLayout();

        btnLogin.Click += async (_, _) => await SignInAsync();
        btnExit.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        chkShowPassword.CheckedChanged += (_, _) =>
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;

        // Enter trong ô mật khẩu kích hoạt đăng nhập, giống thói quen sử dụng thông thường.
        txtPassword.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SignInAsync();
            }
        };

        AcceptButton = btnLogin;
    }

    /// <summary>Tài khoản vừa đăng nhập thành công, <c>null</c> nếu chưa đăng nhập.</summary>
    public UserDto? AuthenticatedUser => AuthSession.CurrentUser;

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(32, 24, 32, 24),
            RowCount = 6
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        header.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0),
            Text = "Đăng nhập hệ thống"
        });
        header.Controls.Add(new Label
        {
            AutoSize = true,
            ForeColor = MutedColor,
            Margin = new Padding(0, 6, 0, 0),
            Text = "Dùng tài khoản nhân viên được cấp bởi quản trị viên."
        });

        txtUsername.Font = new Font("Segoe UI", 11F);
        txtPassword.Font = new Font("Segoe UI", 11F);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(CreateField("Tên đăng nhập", txtUsername), 0, 1);
        root.Controls.Add(CreateField("Mật khẩu", txtPassword), 0, 2);
        root.Controls.Add(chkShowPassword, 0, 3);
        root.Controls.Add(lblMessage, 0, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        StylePrimaryButton(btnLogin);
        btnLogin.Width = 150;
        btnExit.Width = 100;
        btnExit.Height = 38;
        btnExit.Margin = new Padding(10, 4, 0, 0);
        actions.Controls.Add(btnLogin);
        actions.Controls.Add(btnExit);

        root.Controls.Add(actions, 0, 5);
        Controls.Add(root);
    }

    private static Panel CreateField(string labelText, TextBox textBox)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 6)
        };

        panel.Controls.Add(textBox);
        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            Height = 26,
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft
        });

        textBox.Dock = DockStyle.Top;
        textBox.Height = 32;

        return panel;
    }

    private static void StylePrimaryButton(Button button)
    {
        button.BackColor = PrimaryColor;
        button.FlatAppearance.BorderSize = 0;
        button.FlatStyle = FlatStyle.Flat;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.ForeColor = Color.White;
        button.Height = 38;
        button.Margin = new Padding(0, 4, 0, 0);
        button.UseMnemonic = false;
    }

    private async Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
        {
            ShowMessage("Vui lòng nhập tên đăng nhập và mật khẩu.");
            return;
        }

        SetBusy(true);

        try
        {
            using var response = await _apiClient.PostAsync("api/auth/login", new LoginRequestDto
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text
            });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ShowMessage("Tên đăng nhập hoặc mật khẩu không đúng.");
                txtPassword.SelectAll();
                txtPassword.Focus();
                return;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ShowMessage("Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
                return;
            }

            await BaseHttpClient.EnsureSuccessAsync(response);

            var session = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            if (session is null || string.IsNullOrWhiteSpace(session.AccessToken))
            {
                ShowMessage("Phản hồi đăng nhập không hợp lệ. Vui lòng thử lại.");
                return;
            }

            AuthSession.SignIn(session);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (HttpRequestException)
        {
            ShowMessage("Không kết nối được tới API. Hãy chắc chắn API đang chạy (xem docs/API.md).");
        }
        catch (TaskCanceledException)
        {
            ShowMessage("Yêu cầu đăng nhập quá thời gian chờ. Vui lòng thử lại.");
        }
        catch (Exception exception)
        {
            ShowMessage(exception.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        txtUsername.Enabled = !isBusy;
        txtPassword.Enabled = !isBusy;
        chkShowPassword.Enabled = !isBusy;
        btnExit.Enabled = !isBusy;
        btnLogin.Enabled = !isBusy;
        btnLogin.Text = isBusy ? "Đang đăng nhập..." : "Đăng nhập";
        UseWaitCursor = isBusy;
    }

    private void ShowMessage(string message)
    {
        lblMessage.Text = message;
        lblMessage.ForeColor = ErrorColor;
    }
}
