using MiniMart.Shared.DTOs.Customer;

namespace MiniMart.Client.Forms;

public class frmCustomerManager : Form
{
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
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
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
            Text = labelText
        };
        textBox.Dock = DockStyle.Bottom;
        panel.Controls.Add(textBox);
        panel.Controls.Add(label);
        return panel;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
    }

    private void BtnUpdatePoints_Click(object? sender, EventArgs e)
    {
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
    }
}