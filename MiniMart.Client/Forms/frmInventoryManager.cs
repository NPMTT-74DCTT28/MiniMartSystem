using MiniMart.Client.HttpClients;

namespace MiniMart.Client.Forms;

public partial class frmInventoryManager : Form
{
    private readonly BaseHttpClient _apiClient = new();
    private readonly TabControl _tabInventory = new()
    {
        Name = "tabInventory",
        Dock = DockStyle.Fill
    };

    public frmInventoryManager()
    {
        Name = "frmInventoryManager";
        Text = "Nhập kho và nhà cung cấp";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Width = 1100;
        Height = 700;

        Controls.Add(_tabInventory);
        InitializeSupplierTab();
        InitializeImportReceiptTab();
    }

    private static void ShowApiError(Exception exception)
    {
        MessageBox.Show(exception.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
