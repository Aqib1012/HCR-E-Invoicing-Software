using SDK_E_INVOICING_SYSTEM.Data;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static QRCoder.PayloadGenerator;

public class InvoiceViewerForm : Form
{
    private DataGridView dgvInvoices;
    private Button btnPreview, btnDelete;
    private int currentInvoiceId = -1;

    public InvoiceViewerForm()
    {
        // ===== FORM PROPERTIES =====
        this.Icon = new Icon(@"C:\Users\PC\source\repos\SDK-E-INVOICING-SYSTEM\SDK-E-INVOICING-SYSTEM\Resources\icon-256x256.ico");
        this.Text = "Invoice Viewer - Sidekick";
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.White;

        // ===== LAYOUT =====
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Header
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Buttons
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid

        // ===== HEADER =====
        var header = new Label
        {
            Text = "🧾 Invoice Viewer",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = ColorTranslator.FromHtml("#2E7D32"),
            TextAlign = ContentAlignment.MiddleCenter
        };
        layout.Controls.Add(header, 0, 0);

        // ===== BUTTONS =====
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10)
        };

        btnPreview = MakeButton("👁 Preview", "#8E44AD");
        btnDelete = MakeButton("🗑 Delete", "#C0392B");

        btnPreview.Click += BtnPreview_Click;
        btnDelete.Click += BtnDelete_Click;

        buttonPanel.Controls.AddRange(new Control[] { btnPreview, btnDelete });
        layout.Controls.Add(buttonPanel, 0, 1);

        // ===== DATAGRIDVIEW =====
        dgvInvoices = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            RowTemplate = { Height = 35 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            ColumnHeadersHeight = 50,
            RowHeadersVisible = false
        };

        // ===== STYLE =====
        dgvInvoices.EnableHeadersVisualStyles = false;
        dgvInvoices.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#2E7D32");
        dgvInvoices.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvInvoices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvInvoices.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvInvoices.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;

        dgvInvoices.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        dgvInvoices.DefaultCellStyle.ForeColor = Color.Black;
        dgvInvoices.DefaultCellStyle.BackColor = Color.White;
        dgvInvoices.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        dgvInvoices.DefaultCellStyle.SelectionBackColor = Color.White;
        dgvInvoices.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgvInvoices.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        dgvInvoices.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && dgvInvoices.Rows[e.RowIndex].Cells["invoiceId"].Value != null)
            {
                DataGridViewRow row = dgvInvoices.Rows[e.RowIndex];
                currentInvoiceId = Convert.ToInt32(row.Cells["invoiceId"].Value);
            }
        };

        // Optional: double-click row to preview
        dgvInvoices.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
                BtnPreview_Click(s, e);
        };

        layout.Controls.Add(dgvInvoices, 0, 2);
        this.Controls.Add(layout);

        // ===== INITIALIZE GRID COLUMNS =====
        InitializeGrid();

        // ===== LOAD DATA =====
        LoadInvoices();
    }

    // ===== BUTTON MAKER =====
    private Button MakeButton(string text, string color)
    {
        return new Button
        {
            Text = text,
            Width = 130,
            Height = 40,
            BackColor = ColorTranslator.FromHtml(color),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(10)
        };
    }

    // ===== GRID COLUMNS =====
    private void InitializeGrid()
    {
        dgvInvoices.Columns.Clear();

        dgvInvoices.Columns.Add("invoiceId", "ID");
        dgvInvoices.Columns.Add("invoiceNumber", "Invoice Number");
        dgvInvoices.Columns.Add("fbrInvoiceNumber", "FBR Invoice No.");
        dgvInvoices.Columns.Add("invoiceDate", "Date");
        dgvInvoices.Columns.Add("subTotal", "Sub Total");
        dgvInvoices.Columns.Add("totalTax", "Total Tax");
        dgvInvoices.Columns.Add("grandTotal", "Grand Total");
        dgvInvoices.Columns.Add("status", "Status");
        dgvInvoices.Columns.Add("postStatus", "Post Status");
    }

    // ===== LOAD INVOICES =====
    private void LoadInvoices()
    {
        try
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                string sql = @"
SELECT 
    i.invoiceId,
    i.invoiceNumber,
    i.fbrInvoiceNumber,
    i.invoiceDate,
    i.subTotal,
    i.totalTax,
    i.grandTotal,
    i.status,
    i.postStatus
FROM Invoices i
WHERE i.invoiceDate BETWEEN @startDate AND @endDate
ORDER BY i.invoiceId ASC";

                using (var da = new System.Data.SQLite.SQLiteDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@startDate", startOfMonth.ToString("yyyy-MM-dd"));
                    da.SelectCommand.Parameters.AddWithValue("@endDate", endOfMonth.ToString("yyyy-MM-dd"));

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvInvoices.Rows.Clear();
                    foreach (DataRow dr in dt.Rows)
                    {
                        dgvInvoices.Rows.Add(
                            dr["invoiceId"],
                            dr["invoiceNumber"],
                            dr["fbrInvoiceNumber"],
                            dr["invoiceDate"],
                            dr["subTotal"],
                            dr["totalTax"],
                            dr["grandTotal"],
                            dr["status"],
                            dr["postStatus"]
                        );
                    }

                    dgvInvoices.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading invoices: " + ex.Message);
        }
    }

    // ===== PREVIEW =====
    private void BtnPreview_Click(object sender, EventArgs e)
    {
        if (currentInvoiceId == -1)
        {
            MessageBox.Show("⚠ Please select an invoice first!");
            return;
        }

        var previewForm = new InvoicePreviewForm(currentInvoiceId);
        previewForm.ShowDialog();
    }

    // ===== DELETE =====
    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (currentInvoiceId == -1)
        {
            MessageBox.Show("⚠ Please select an invoice first!");
            return;
        }

        var confirm = MessageBox.Show(
            "Are you sure you want to delete this invoice and all its items?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (confirm == DialogResult.Yes)
        {
            try
            {
                DatabaseHelper.DeleteInvoice(currentInvoiceId); // Deletes invoice + items
                MessageBox.Show("✅ Invoice deleted successfully!");
                LoadInvoices();
                currentInvoiceId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error deleting invoice: {ex.Message}");
            }
        }
    }
}
