namespace MiniMart.Client;

using MiniMart.Client.Forms;

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

        // frmMain là form chung điều hướng tới màn hình chức năng của toàn bộ thành viên.
        // Khi có màn hình đăng nhập, truyền UserDto vào: Application.Run(new frmMain(loggedInUser));
        Application.Run(new frmMain());
    }    
}