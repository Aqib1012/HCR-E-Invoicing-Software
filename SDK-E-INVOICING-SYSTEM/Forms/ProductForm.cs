using SDK_E_INVOICING_SYSTEM.Data;
using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class ProductForm : Form
{
    private TextBox txtHsCode, txtProductDescription, txtRate, txtUoM, txtSearch;
    private Button btnAdd, btnUpdate, btnDelete, btnClear;
    private DataGridView dgvProducts;
    private int selectedProductId = -1;

    public ProductForm()
    {
        
        this.Text = "Product Management";
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.White;

        // === Main layout ===
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // Header
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // Search
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));  // Fields
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));   // Buttons
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Grid

        // === HEADER ===
        var header = new Label
        {
            Text = "📦 Product Management",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = ColorTranslator.FromHtml("#2E7D32"),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // === SEARCH PANEL ===
        var searchPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(2) };
        txtSearch = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Dock = DockStyle.Fill,
            Text = "🔍 Search by HS Code or Description..."
        };
        var lblSearch = new Label
        {
            Text = "Search:",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Dock = DockStyle.Left,
            Width = 70,
            TextAlign = ContentAlignment.MiddleLeft
        };
        searchPanel.Controls.Add(txtSearch);
        searchPanel.Controls.Add(lblSearch);
        txtSearch.BringToFront();
        txtSearch.TextChanged += (s, e) => LoadProducts(txtSearch.Text);

        // === FIELDS PANEL ===
        var fieldsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            Padding = new Padding(5)
        };
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fieldsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        fieldsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var lblHsCode = new Label { Text = "HS Code *", Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
        var lblDesc = new Label { Text = "Product Description *", Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
        var lblRate = new Label { Text = "Rate (e.g. 20%) *", Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
        var lblUoM = new Label { Text = "Unit of Measure *", Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };

        txtHsCode = new TextBox { Font = new Font("Segoe UI", 11), Dock = DockStyle.Fill };
        txtProductDescription = new TextBox { Font = new Font("Segoe UI", 11), Dock = DockStyle.Fill };
        txtRate = new TextBox { Font = new Font("Segoe UI", 11), Dock = DockStyle.Fill };
        txtUoM = new TextBox { Font = new Font("Segoe UI", 11), Dock = DockStyle.Fill };

        fieldsPanel.Controls.Add(lblHsCode, 0, 0);
        fieldsPanel.Controls.Add(lblDesc, 1, 0);
        fieldsPanel.Controls.Add(lblRate, 2, 0);
        fieldsPanel.Controls.Add(lblUoM, 3, 0);
        fieldsPanel.Controls.Add(txtHsCode, 0, 1);
        fieldsPanel.Controls.Add(txtProductDescription, 1, 1);
        fieldsPanel.Controls.Add(txtRate, 2, 1);
        fieldsPanel.Controls.Add(txtUoM, 3, 1);

        // === BUTTON PANEL ===
        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        btnAdd = MakeButton("➕ Add", "#4CAF50");
        btnUpdate = MakeButton("✏️ Update", "#2196F3");
        btnDelete = MakeButton("🗑 Delete", "#F44336");
        btnClear = MakeButton("🧹 Clear", "Gray");

        btnPanel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

        // === DATAGRIDVIEW ===
        dgvProducts = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowTemplate = { Height = 35 },
            ColumnHeadersHeight = 50,
            RowHeadersVisible = false
        };

        // ===== STYLE LIKE INVOICE GRID =====
        dgvProducts.EnableHeadersVisualStyles = false;
        dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#2E7D32");
        dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvProducts.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvProducts.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;

        dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        dgvProducts.DefaultCellStyle.ForeColor = Color.Black;
        dgvProducts.DefaultCellStyle.BackColor = Color.White;
        dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        dgvProducts.DefaultCellStyle.SelectionBackColor = Color.White;
        dgvProducts.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgvProducts.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        // ===== EVENTS =====
        dgvProducts.CellClick += DgvProducts_CellClick;

        // Optional: double-click to edit fields
        dgvProducts.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
                txtHsCode.Focus(); // Or open an edit form
        };


        // Add to main layout
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(searchPanel, 0, 1);
        layout.Controls.Add(fieldsPanel, 0, 2);
        layout.Controls.Add(btnPanel, 0, 3);
        layout.Controls.Add(dgvProducts, 0, 4);

        this.Controls.Add(layout);

        // === BUTTON EVENTS ===
        btnAdd.Click += (s, e) => AddProduct();
        btnUpdate.Click += (s, e) => UpdateProduct();
        btnDelete.Click += (s, e) => DeleteProduct();
        btnClear.Click += (s, e) => ClearFields();

        LoadProducts();
    }

    private Button MakeButton(string text, string color)
    {
        return new Button
        {
            Text = text,
            Width = 90,
            Height = 32,
            BackColor = ColorTranslator.FromHtml(color),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(3)
        };
    }

    private void LoadProducts(string filter = "")
    {
        try
        {
            DataTable dt = DatabaseHelper.GetProducts(filter);
            dgvProducts.DataSource = dt;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading products: " + ex.Message);
        }
    }

    private bool ValidateFields()
    {
        if (string.IsNullOrWhiteSpace(txtHsCode.Text))
        {
            MessageBox.Show("HS Code is required.");
            return false;
        }
        if (!Regex.IsMatch(txtHsCode.Text.Trim(), @"^\d+\.\d+$"))
        {
            MessageBox.Show("HS Code must be a number with a decimal point (e.g., 1234.56).");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtProductDescription.Text))
        {
            MessageBox.Show("Product Description is required.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtRate.Text) || !Regex.IsMatch(txtRate.Text.Trim(), @"^\d+(\.\d{1,2})?%$"))
        {
            MessageBox.Show("Rate must be a valid percentage (e.g., 20%, 18.5%).");
            return false;
        }
        return true;
    }

    private void AddProduct()
    {
        try
        {
            if (!ValidateFields()) return;
            DatabaseHelper.AddProduct(txtHsCode.Text, txtProductDescription.Text, txtRate.Text, txtUoM.Text);
            LoadProducts();
            ClearFields();
            //MessageBox.Show("Product added successfully.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to add product: " + ex.Message);
        }
    }

    private void UpdateProduct()
    {
        try
        {
            if (selectedProductId == -1) return;
            if (!ValidateFields()) return;
            DatabaseHelper.UpdateProduct(selectedProductId, txtHsCode.Text, txtProductDescription.Text, txtRate.Text, txtUoM.Text);
            LoadProducts();
            ClearFields();
            MessageBox.Show("Product updated successfully.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to update product: " + ex.Message);
        }
    }

    private void DeleteProduct()
    {
        try
        {
            if (selectedProductId == -1) return;
            if (MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.DeleteProduct(selectedProductId);
                LoadProducts();
                ClearFields();
                MessageBox.Show("Product deleted successfully.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to delete product: " + ex.Message);
        }
    }

    private void ClearFields()
    {
        selectedProductId = -1;
        txtHsCode.Clear();
        txtProductDescription.Clear();
        txtRate.Clear();
        txtUoM.Clear();
    }

    private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && dgvProducts.Rows[e.RowIndex].Cells["productId"].Value != null)
        {
            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
            selectedProductId = Convert.ToInt32(row.Cells["productId"].Value);
            txtHsCode.Text = row.Cells["hsCode"].Value?.ToString() ?? "";
            txtProductDescription.Text = row.Cells["productDescription"].Value?.ToString() ?? "";
            txtRate.Text = row.Cells["rate"].Value?.ToString() ?? "";
            txtUoM.Text = row.Cells["uoM"].Value?.ToString() ?? "";
        }
    }
}