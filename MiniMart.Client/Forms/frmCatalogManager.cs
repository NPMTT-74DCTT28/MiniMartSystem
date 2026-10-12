using System.Net.Http;
using MiniMart.Client.HttpClients;

namespace MiniMart.Client.Forms;

public partial class frmCatalogManager : Form
{
    private readonly BaseHttpClient _apiClient = new();
    private readonly TabControl _tabCatalog = new()
    {
        Dock = DockStyle.Fill
    };

    public frmCatalogManager()
    {
        Text = "Quản lý danh mục và sản phẩm";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Width = 1100;
        Height = 700;

        Controls.Add(_tabCatalog);
        InitializeCategoryTab();
        InitializeProductTab();
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

    private static void ShowApiError(Exception exception)
    {
        MessageBox.Show(exception.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
