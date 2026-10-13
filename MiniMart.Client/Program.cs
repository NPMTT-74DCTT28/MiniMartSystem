namespace MiniMart.Client;

using MiniMart.Client.Forms;
using MiniMart.Client.Security;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Vòng lặp đăng nhập -> form chung -> đăng xuất -> đăng nhập lại.
        // Nhờ vậy nút "Đăng xuất" của frmMain đưa người dùng về màn hình đăng nhập
        // thay vì đóng ứng dụng.
        while (true)
        {
            using var login = new frmLogin();

            if (login.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using var shell = new frmMain(AuthSession.CurrentUser);
            shell.LogoutRequested += (_, _) =>
            {
                AuthSession.SignOut();
                shell.Close();
            };

            Application.Run(shell);

            // Đóng form chung mà không bấm Đăng xuất (VD: bấm nút X) cũng kết thúc phiên.
            if (AuthSession.IsAuthenticated)
            {
                AuthSession.SignOut();
                return;
            }
        }
    }
}
