using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDK_E_INVOICING_SYSTEM.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;


public class GenerateInvoiceForm : Form
{
    private ComboBox cmbBuyerName, cmbHSCode, cmbScenario, cmbSaleType;
    private TextBox txtSellerNTN, txtSellerBusiness, txtSellerProvince, txtSellerAddress;
    private TextBox txtBuyerNTN, txtBuyerProvince, txtBuyerAddress, txtBuyerRegType;
    private TextBox txtProdDesc, txtProdRate, txtProdUOM;
    private TextBox txtQuantity, txtTotalValue, txtValueExclGST;
    private TextBox txtSalesTaxAmount, txtFurtherTaxAmount, txtExtraTaxAmount;
    private TextBox txtUnitPrice;
    private TextBox txtItemNotes; // New Notes field
    private Button btnAddItem, btnSave, btnValidateInvoice, btnPost, btnDeleteItem;
    private DataGridView dgvItems;
    private DataTable buyers, products;
    private Label lblSubtotal;
    private DateTimePicker dtInvoiceDate;
    private TextBox txtInvoiceNumber;

    private ProgressBar progressBar;
    // Seller info variables
    private string sellerBusinessName;
    private string sellerNTN;
    private string sellerProvince;
    private string sellerAddress;
    private string sellerToken; // from login
    private ComboBox cmbSellerName;





    private void InitializeLoader()
    {
        progressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Dock = DockStyle.Top,
            Height = 20,
            Visible = false
        };
        this.Controls.Add(progressBar);
        progressBar.BringToFront();
    }



    private int editingRowIndex = -1;

    public GenerateInvoiceForm()
    {
        this.Icon = new Icon(@"C:\Users\PC\source\repos\SDK-E-INVOICING-SYSTEM\SDK-E-INVOICING-SYSTEM\Resources\icon-256x256.ico");
        this.Text = "Generate Invoice";
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.WhiteSmoke;

        DatabaseHelper.InitializeDatabase();
        InitializeLoader();
        // Initialize Invoice Number first
        txtInvoiceNumber = CreateTextBox(GetNextInvoiceNumber().ToString(), true);



        InitializeUI();
    }

    private void InitializeUI()
    {



        FlowLayoutPanel mainLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10),
            WrapContents = true,
        };
        this.Controls.Add(mainLayout);


        // Seller Info
        GroupBox gbSeller = CreateGroupBox("🏢 Seller Info", new Size(400, 150));
        cmbSellerName = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        txtSellerNTN = CreateTextBox("", true);
        txtSellerBusiness = CreateTextBox("", true);
        txtSellerProvince = CreateTextBox("", true);
        txtSellerAddress = CreateTextBox("", true);

        AddLabeledControls(gbSeller,
            ("Select Seller:", cmbSellerName),
            ("NTN:", txtSellerNTN),

            ("Province:", txtSellerProvince),
            ("Address:", txtSellerAddress));

        mainLayout.Controls.Add(gbSeller);
        DataTable sellers = DatabaseHelper.GetSellers(); // make sure you have a GetSellers() method

        cmbSellerName.DataSource = sellers;
        cmbSellerName.DisplayMember = "sellerBusinessName"; // column in DB
        cmbSellerName.ValueMember = "sellerId";

        cmbSellerName.SelectedIndexChanged += (s, e) =>
        {
            if (cmbSellerName.SelectedIndex >= 0)
            {
                DataRow row = sellers.Rows[cmbSellerName.SelectedIndex];
                txtSellerNTN.Text = row["sellerNTNCNIC"].ToString();
                // txtSellerBusiness.Text = row["sellerBusinessName"].ToString();
                txtSellerProvince.Text = row["sellerProvince"].ToString();
                txtSellerAddress.Text = row["sellerAddress"].ToString();
                sellerToken = row["token"].ToString();

                // optional: store in variables for API
                sellerNTN = txtSellerNTN.Text;
                sellerBusinessName = txtSellerBusiness.Text;
                sellerProvince = txtSellerProvince.Text;
                sellerAddress = txtSellerAddress.Text;
            }
        };

        // Buyer Info
        GroupBox gbBuyer = CreateGroupBox("👤 Buyer Info", new Size(400, 180));
        cmbBuyerName = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        txtBuyerNTN = CreateTextBox("", true);
        txtBuyerProvince = CreateTextBox("", true);
        txtBuyerAddress = CreateTextBox("", true);
        txtBuyerRegType = CreateTextBox("", true);
        AddLabeledControls(gbBuyer,
            ("Business:", cmbBuyerName), ("NTN/CNIC:", txtBuyerNTN),
            ("Province:", txtBuyerProvince), ("Address:", txtBuyerAddress),
            ("Reg Type:", txtBuyerRegType));
        mainLayout.Controls.Add(gbBuyer);

        // Product Info
        GroupBox gbProduct = CreateGroupBox("📦 Product Info", new Size(400, 150));
        cmbHSCode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        txtProdDesc = CreateTextBox("", true);
        txtProdRate = CreateTextBox("", true);
        txtProdUOM = CreateTextBox("", true);
        AddLabeledControls(gbProduct,
            ("HS Code:", cmbHSCode), ("Description:", txtProdDesc),
            ("Tax Rate:", txtProdRate), ("UOM:", txtProdUOM));
        mainLayout.Controls.Add(gbProduct);

        // Invoice Item Panel
        GroupBox gbItem = CreateGroupBox("🧾 Invoice Item", new Size(330, 300));

        cmbScenario = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        cmbScenario.Items.Add("SN002");
        cmbScenario.SelectedIndex = 0;

        txtQuantity = CreateTextBox();
        txtUnitPrice = CreateTextBox();
        txtTotalValue = CreateTextBox();
        txtValueExclGST = CreateTextBox();
        txtSalesTaxAmount = CreateTextBox();
        txtFurtherTaxAmount = CreateTextBox();
        //txtExtraTaxAmount = CreateTextBox();
        txtItemNotes = CreateTextBox(); // Notes field
        txtQuantity.TextChanged += RecalculateTotalValue;
        txtUnitPrice.TextChanged += RecalculateTotalValue;
        // ✅ Numbers only
        txtQuantity.KeyPress += NumericOnly_KeyPress;
        txtUnitPrice.KeyPress += NumericOnly_KeyPress;
        txtSalesTaxAmount.KeyPress += NumericOnly_KeyPress;
        txtFurtherTaxAmount.KeyPress += NumericOnly_KeyPress;
        txtValueExclGST.KeyPress += NumericOnly_KeyPress;

        // ✅ Notes (alphanumeric only, max length = 20)
        txtItemNotes.KeyPress += Notes_KeyPress;
        txtItemNotes.MaxLength = 20;

        //dtInvoiceDate = new DateTimePicker();
        //dtInvoiceDate.Format = DateTimePickerFormat.Short;

        cmbSaleType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        // cmbSaleType.Items.Add("Goods at standard rate");
        //cmbSaleType.SelectedIndex = 0;/

        AddLabeledControls(gbItem,
            //  ("Invoice No:", txtInvoiceNumber),
            ("Scenario:", cmbScenario),
            ("Quantity:", txtQuantity),
            ("Unit Price:", txtUnitPrice),
            // ("Invoice Date:", dtInvoiceDate),
            ("Total Value:", txtTotalValue),
            ("Excl GST:", txtValueExclGST),
            ("Sales Tax:", txtSalesTaxAmount),
            ("Further Tax:", txtFurtherTaxAmount),
            /* ("Extra Tax:", txtExtraTaxAmount),*/
            /*  ("Sale Type:", cmbSaleType),*/
            ("Notes:", txtItemNotes) // Add Notes to UI
        );

        FlowLayoutPanel itemButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40 };
        btnAddItem = CreateButton("➕ Add Item", Color.SeaGreen);
        btnDeleteItem = CreateButton("🗑️ Delete Item", Color.Firebrick);
        btnAddItem.Enabled = true;
        btnDeleteItem.Enabled = false;
        itemButtons.Controls.AddRange(new Control[] { btnAddItem, btnDeleteItem });
        gbItem.Controls.Add(itemButtons);

        // Invoice Grid
        GroupBox gbGrid = CreateGroupBox("📋 Invoice Items", new Size(980, 350));
        dgvItems = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };



        dgvItems.Columns.Add("ScenarioID", "Scenario ID");
        dgvItems.Columns.Add("HSCode", "HS Code");
        dgvItems.Columns.Add("Product_Desc", "Product Description");
        dgvItems.Columns.Add("UOM", "Unit of Measure");
        dgvItems.Columns.Add("Quantity", "Quantity");
        dgvItems.Columns.Add("UnitPrice", "Unit Price");
        dgvItems.Columns.Add("Rate", "Rate (%)");
        dgvItems.Columns.Add("TotalValue", "Total Value");
        dgvItems.Columns.Add("ValueExclGST", "Value Excl. GST");
        dgvItems.Columns.Add("SalesTaxAmount", "Sales Tax Amount");
        dgvItems.Columns.Add("FurtherTaxAmount", "Further Tax Amount");
        dgvItems.Columns.Add("Notes", "Notes");


        dgvItems.BackgroundColor = Color.White;
        dgvItems.DefaultCellStyle.BackColor = Color.ForestGreen;
        dgvItems.DefaultCellStyle.ForeColor = Color.White;
        dgvItems.DefaultCellStyle.SelectionBackColor = Color.LightSteelBlue;
        dgvItems.DefaultCellStyle.SelectionForeColor = Color.Black;

        dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue; // halka alternate shade
        dgvItems.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black;



        lblSubtotal = new Label
        {
            Text = "Grand Total: 0.00",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleRight
        };
        gbGrid.Controls.Add(dgvItems);
        gbGrid.Controls.Add(lblSubtotal);

        FlowLayoutPanel gridButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 45,
            FlowDirection = FlowDirection.RightToLeft
        };
        btnPost = CreateButton("📤 Post Invoice", Color.SteelBlue);
        btnValidateInvoice = CreateButton("✅ Validate Invoice", Color.OrangeRed);
        btnSave = CreateButton("💾 Save Invoice", Color.DarkSlateGray);
        gridButtons.Controls.AddRange(new Control[] { btnPost, btnValidateInvoice, btnSave });
        btnValidateInvoice.Click += btnValidate_Click;
        btnPost.Click += btnSubmit_Click;
        btnSave.Click += BtnSave_Click;


        btnPost.Enabled = false;
        btnSave.Enabled = false;  // default disabled
        txtQuantity.TextChanged += RecalculateSalesTax;
        txtUnitPrice.TextChanged += RecalculateSalesTax;
        txtProdRate.TextChanged += RecalculateSalesTax;

        gbGrid.Controls.Add(gridButtons);

        FlowLayoutPanel itemLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Top,
            Padding = new Padding(5),
        };
        itemLayout.Controls.Add(gbItem);
        itemLayout.Controls.Add(gbGrid);
        mainLayout.Controls.Add(itemLayout);

        // Load Data
        buyers = DatabaseHelper.GetCustomers();
        cmbBuyerName.DataSource = buyers;
        cmbBuyerName.DisplayMember = "customerBusinessName";
        cmbBuyerName.ValueMember = "customerId";
        cmbBuyerName.SelectedIndexChanged += (s, e) =>
        {
            if (cmbBuyerName.SelectedIndex >= 0)
            {
                DataRow row = buyers.Rows[cmbBuyerName.SelectedIndex];
                txtBuyerNTN.Text = row["customerNTNCNIC"].ToString();
                txtBuyerProvince.Text = row["customerProvince"].ToString();
                txtBuyerAddress.Text = row["customerAddress"].ToString();
                txtBuyerRegType.Text = row["registrationType"].ToString();
            }
        };

        products = DatabaseHelper.GetProducts();
        cmbHSCode.DataSource = products;
        cmbHSCode.DisplayMember = "hsCode";
        cmbHSCode.ValueMember = "productId";
        cmbHSCode.SelectedIndexChanged += (s, e) =>
        {
            if (cmbHSCode.SelectedIndex >= 0)
            {
                DataRow row = products.Rows[cmbHSCode.SelectedIndex];
                txtProdDesc.Text = row["productDescription"].ToString();
                txtProdRate.Text = row["rate"].ToString();
                txtProdUOM.Text = row["uoM"].ToString();
            }
        };

        // Events
        btnAddItem.Click += BtnAddItem_Click;
        btnDeleteItem.Click += BtnDeleteItem_Click;
        dgvItems.CellDoubleClick += DgvItems_CellDoubleClick;
        dgvItems.SelectionChanged += (s, e) =>
        {
            btnDeleteItem.Enabled = dgvItems.SelectedRows.Count > 0;
        };
    }

    private void BtnAddItem_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtQuantity.Text) || string.IsNullOrWhiteSpace(txtProdRate.Text))
        {
            MessageBox.Show("⚠️ Quantity and Rate required!");
            return;
        }

        if (editingRowIndex >= 0)
        {
            FillRow(dgvItems.Rows[editingRowIndex]);
            editingRowIndex = -1;
            btnAddItem.Text = "➕ Add Item";
        }
        else
        {
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.Cells["HSCode"].Value?.ToString() == cmbHSCode.Text &&
                    row.Cells["ScenarioID"].Value?.ToString() == cmbScenario.Text)
                {
                    MessageBox.Show("⚠️ Duplicate entry!");
                    return;
                }
            }
            int idx = dgvItems.Rows.Add();
            FillRow(dgvItems.Rows[idx]);
        }

        ClearFields();
        UpdateSubtotal();
        txtInvoiceNumber.Text = GetNextInvoiceNumber().ToString(); // auto increment for next item
    }

    private void BtnDeleteItem_Click(object sender, EventArgs e)
    {
        if (dgvItems.SelectedRows.Count > 0)
        {
            dgvItems.Rows.RemoveAt(dgvItems.SelectedRows[0].Index);
            editingRowIndex = -1;
            btnAddItem.Text = "➕ Add Item";
            ClearFields();
            UpdateSubtotal();
        }
    }

    private void DgvItems_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            DataGridViewRow row = dgvItems.Rows[e.RowIndex];
            editingRowIndex = e.RowIndex;


            txtQuantity.Text = row.Cells["Quantity"].Value?.ToString();
            txtUnitPrice.Text = row.Cells["UnitPrice"].Value?.ToString();
            //dtInvoiceDate.Value = DateTime.TryParse(row.Cells["InvoiceDate"].Value?.ToString(), out DateTime d) ? d : DateTime.Today;
            txtProdRate.Text = row.Cells["Rate"].Value?.ToString();
            txtTotalValue.Text = row.Cells["TotalValue"].Value?.ToString();
            txtValueExclGST.Text = row.Cells["ValueExclGST"].Value?.ToString();
            txtSalesTaxAmount.Text = row.Cells["SalesTaxAmount"].Value?.ToString();
            txtFurtherTaxAmount.Text = row.Cells["FurtherTaxAmount"].Value?.ToString();
            //txtExtraTaxAmount.Text = row.Cells["ExtraTaxAmount"].Value?.ToString();
            //cmbSaleType.Text = row.Cells["SaleType"].Value?.ToString();
            txtItemNotes.Text = row.Cells["Notes"].Value?.ToString();

            btnAddItem.Text = "✏️ Update Item";
        }
    }

    private void FillRow(DataGridViewRow row)
    {
        //row.Cells["InvoiceNo"].Value = txtInvoiceNumber.Text;
        row.Cells["ScenarioID"].Value = cmbScenario.Text;
        row.Cells["HSCode"].Value = cmbHSCode.Text;
        row.Cells["Product_Desc"].Value = txtProdDesc.Text;
        row.Cells["UOM"].Value = txtProdUOM.Text;
        row.Cells["Quantity"].Value = txtQuantity.Text;
        row.Cells["UnitPrice"].Value = txtUnitPrice.Text;
        // row.Cells["InvoiceDate"].Value = dtInvoiceDate.Value.ToShortDateString();
        row.Cells["Rate"].Value = txtProdRate.Text;
        row.Cells["TotalValue"].Value = txtTotalValue.Text;
        row.Cells["ValueExclGST"].Value = txtValueExclGST.Text;
        row.Cells["SalesTaxAmount"].Value = txtSalesTaxAmount.Text;
        row.Cells["FurtherTaxAmount"].Value = txtFurtherTaxAmount.Text;
        // row.Cells["ExtraTaxAmount"].Value = txtExtraTaxAmount.Text;
        // row.Cells["SaleType"].Value = cmbSaleType.Text;
        row.Cells["Notes"].Value = txtItemNotes.Text;
    }

    private void ClearFields()
    {
        txtQuantity.Clear();
        txtUnitPrice.Clear();
        txtTotalValue.Clear();
        txtValueExclGST.Clear();
        txtSalesTaxAmount.Clear();
        txtFurtherTaxAmount.Clear();
        // txtExtraTaxAmount.Clear();
        txtItemNotes.Clear();
        editingRowIndex = -1;
        btnAddItem.Text = "➕ Add Item";
    }

    private void UpdateSubtotal()
    {
        decimal subtotal = 0;
        decimal salesTax = 0;
        decimal furtherTax = 0;

        foreach (DataGridViewRow row in dgvItems.Rows)
        {
            subtotal += Convert.ToDecimal(row.Cells["TotalValue"].Value ?? 0);
            salesTax += Convert.ToDecimal(row.Cells["SalesTaxAmount"].Value ?? 0);
            furtherTax += Convert.ToDecimal(row.Cells["FurtherTaxAmount"].Value ?? 0);
        }

        // Subtotal ab product total + sales tax
        // lblSubtotal.Text = $"Subtotal (Incl. Sales Tax): {(subtotal + salesTax):C}";

        // Total ab subtotal + sales tax + further tax
        lblSubtotal.Text = $"Grand Total: {(subtotal + salesTax + furtherTax):N2}";

    }



    private GroupBox CreateGroupBox(string title, Size size) => new GroupBox
    {
        Text = title,
        Size = size,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.DarkSlateGray,
        Padding = new Padding(8),
        Margin = new Padding(8),
    };

    private TextBox CreateTextBox(string text = "", bool readOnly = false) => new TextBox { Text = text, ReadOnly = readOnly, Width = 200 };

    private Button CreateButton(string text, Color color) => new Button
    {
        Text = text,
        BackColor = color,
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Width = 140,
        Height = 35,
        Margin = new Padding(5),
        Font = new Font("Segoe UI", 9, FontStyle.Bold)
    };

    private void AddLabeledControls(GroupBox gb, params (string, Control)[] pairs)
    {
        TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var (labelText, control) in pairs)
        {
            tlp.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(2) });
            tlp.Controls.Add(control);
        }
        gb.Controls.Add(tlp);
    }
    // ✅ Allow only numbers + control keys (Backspace, Delete etc.)
    private void NumericOnly_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true; // block invalid input
        }
    }

    // ✅ Allow only letters, numbers, space, and basic symbols (alphanumeric for Notes)
    private void Notes_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) &&
            !char.IsLetterOrDigit(e.KeyChar) &&
            !char.IsWhiteSpace(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private void RecalculateTotalValue(object sender, EventArgs e)
    {
        if (decimal.TryParse(txtQuantity.Text, out decimal qty) &&
            decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) &&
            decimal.TryParse(txtProdRate.Text, out decimal rate)) // Rate in %
        {
            decimal total = qty * unitPrice;
            txtTotalValue.Text = total.ToString("N2");

            decimal valueExclGST = total / (1 + rate / 100);
            decimal salesTax = total - valueExclGST;

            txtValueExclGST.Text = valueExclGST.ToString("N2");
            txtSalesTaxAmount.Text = salesTax.ToString("N2");
        }
        else
        {
            txtTotalValue.Text = "0.00";
            txtValueExclGST.Text = "0.00";
            txtSalesTaxAmount.Text = "0.00";
        }
    }


    public static int GetNextInvoiceNumber()
    {
        using (var conn = new SQLiteConnection(DatabaseHelper.ConnectionString))
        {
            conn.Open();
            string sql = "SELECT MAX(invoiceId) FROM Invoices";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                    return Convert.ToInt32(result) + 1;
                else
                    return 1;
            }
        }
    }




    // Inside your button click:
    private async void btnValidate_Click(object sender, EventArgs e)
    {
        try
        {
            btnValidateInvoice.Enabled = false;
            progressBar.Visible = true;
            this.UseWaitCursor = true;

            string jsonPayload = BuildInvoiceJson();

            // Fetch token of the selected seller
            // string sellerToken = GetSelectedSellerToken(); // Implement this method to return token

            var service = new FbrApiService();
            string result = await service.ValidateInvoiceDataAsync(jsonPayload, sellerToken);

            ShowApiResponse("Validation", result);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Validation Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnValidateInvoice.Enabled = true;
            progressBar.Visible = false;
            this.UseWaitCursor = false;
        }
    }







    // Custom converter to remove .0 from integer values
    public class IntDecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(double);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is decimal dec)
            {
                writer.WriteRawValue(dec % 1 == 0 ? dec.ToString("0") : dec.ToString());
            }
            else if (value is double dbl)
            {
                writer.WriteRawValue(dbl % 1 == 0 ? dbl.ToString("0") : dbl.ToString());
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Convert.ToDecimal(reader.Value);
        }
    }

    private async void btnSubmit_Click(object sender, EventArgs e)
    {
        try
        {
            // Disable buttons during processing
            btnPost.Enabled = false;
            btnSave.Enabled = false;
            btnValidateInvoice.Enabled = false;
            progressBar.Visible = true;
            this.UseWaitCursor = true;

            // --- 1. Build JSON payload for FBR API ---
            string jsonPayload = BuildInvoiceJson();

            // --- 2. Call FBR API ---
            var service = new FbrApiService();
            string result = await service.PostInvoiceDataAsync(jsonPayload, sellerToken);

            // --- 3. Parse response ---
            string invoiceNumber = null;

            if (!string.IsNullOrWhiteSpace(result))
            {
                // Sirf JSON portion nikaalo (in case API returns extra headers)
                int bodyIndex = result.IndexOf("{");
                if (bodyIndex >= 0)
                {
                    string jsonBody = result.Substring(bodyIndex);

                    try
                    {
                        var jsonResponse = JObject.Parse(jsonBody);

                        // FBR response key check
                        if (jsonResponse["invoiceNumber"] != null)
                            invoiceNumber = jsonResponse["invoiceNumber"].ToString();
                    }
                    catch
                    {
                        MessageBox.Show("⚠️ Failed to parse invoice number from FBR response.",
                            "Parsing Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            // --- 4. Save to database if successful ---
            if (!string.IsNullOrEmpty(invoiceNumber))
            {

                PostAndSave(invoiceNumber);
                MessageBox.Show($"✅ Invoice posted successfully!\nFBR Invoice #: {invoiceNumber}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("❌ FBR response did not contain a valid invoice number.",
                    "Posting Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("❌ Error while posting invoice: " + ex.Message,
                "Posting Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Enable UI again
            btnPost.Enabled = true;
            btnSave.Enabled = true;
            btnValidateInvoice.Enabled = true;
            progressBar.Visible = false;
            this.UseWaitCursor = false;
        }
    }






    private string BuildInvoiceJson()
    {
        var invoice = new
        {
            invoiceType = "Sale Invoice",
            invoiceDate = DateTime.Now.ToString("yyyy-MM-dd"),
            sellerNTNCNIC = txtSellerNTN.Text,
            sellerBusinessName = txtSellerBusiness.Text,
            sellerProvince = txtSellerProvince.Text,
            sellerAddress = txtSellerAddress.Text,
            buyerNTNCNIC = txtBuyerNTN.Text,
            buyerBusinessName = cmbBuyerName.Text,
            buyerProvince = txtBuyerProvince.Text,
            buyerAddress = txtBuyerAddress.Text,
            buyerRegistrationType = txtBuyerRegType.Text,
            invoiceRefNo = txtInvoiceNumber.Text,
            scenarioId = cmbScenario.Text,
            items = GetInvoiceItems()
        };

        return JsonConvert.SerializeObject(invoice, Newtonsoft.Json.Formatting.Indented);
    }

    private List<object> GetInvoiceItems()
    {
        var items = new List<object>();

        foreach (DataGridViewRow row in dgvItems.Rows)
        {
            if (row.IsNewRow) continue;

            items.Add(new
            {
                hsCode = row.Cells["HSCode"].Value?.ToString(),
                productDescription = row.Cells["Product_Desc"].Value?.ToString(),
                rate = row.Cells["Rate"].Value?.ToString(),
                uoM = row.Cells["UOM"].Value?.ToString(),

                // Numbers properly formatted
                quantity = Convert.ToDecimal(row.Cells["Quantity"].Value ?? 0).ToString("0.####"), // up to 4 decimals
                totalValues = Convert.ToDecimal(row.Cells["TotalValue"].Value ?? 0).ToString("0.00"),
                valueSalesExcludingST = Convert.ToDecimal(row.Cells["ValueExclGST"].Value ?? 0).ToString("0.00"),
                salesTaxApplicable = Convert.ToDecimal(row.Cells["SalesTaxAmount"].Value ?? 0).ToString("0.00"),
                furtherTax = Convert.ToDecimal(row.Cells["FurtherTaxAmount"].Value ?? 0).ToString("0.00"),

                fixedNotifiedValueOrRetailPrice = 0.00m,
                salesTaxWithheldAtSource = 0.00m,
                extraTax = "",
                fedPayable = 0.00m,
                discount = 0.00m,
                saleType = "Goods at standard rate (default)",
                sroItemSerialNo = "",
                notes = row.Cells["Notes"].Value?.ToString()
            });

        }

        return items;
    }
    private void ShowApiResponse(string action, string rawResponse)
    {
        try
        {
            // Sirf JSON nikaalna (agar headers bhi aaye to)
            if (rawResponse.Contains("Response Body:"))
            {
                int index = rawResponse.IndexOf("Response Body:") + "Response Body:".Length;
                rawResponse = rawResponse.Substring(index).Trim();
            }

            var parsed = JObject.Parse(rawResponse);

            string status = parsed["validationResponse"]?["status"]?.ToString() ?? "UNKNOWN";
            string statusCode = parsed["validationResponse"]?["statusCode"]?.ToString() ?? "";
            string errorMsg = parsed["validationResponse"]?["error"]?.ToString() ?? "";

            if (status.Equals("Valid", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    $"✅ {action} Successful!\n\n🟢 Status: {status}\nCode: {statusCode}",
                    $"{action} Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                if (action == "Validation")
                    btnPost.Enabled = true;
                btnSave.Enabled = true;
                // sirf validation ke baad post enable hoga
                // sirf validation ke baad post enable hoga
            }
            else
            {
                MessageBox.Show(
                    $"❌ {action} Failed!\n\nRaw Response:\n{rawResponse}",
                    $"{action} Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                if (action == "Validation")
                    btnPost.Enabled = false;
                btnSave.Enabled = false;
            }
        }
        catch
        {
            MessageBox.Show(
                $"⚠️ Could not parse response.\n\nRaw Response:\n{rawResponse}",
                $"{action} Result",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            if (action == "Validation")
                btnPost.Enabled = false;
            btnSave.Enabled = false;
        }
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        try
        {
            if (dgvItems.Rows.Count == 0)
            {
                MessageBox.Show("⚠️ No items to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int customerId = 0, sellerId = 0;
            int.TryParse(cmbBuyerName.SelectedValue?.ToString(), out customerId);
            int.TryParse(cmbSellerName.SelectedValue?.ToString(), out sellerId);
            DateTime invoiceDate = DateTime.Now;

            decimal subTotal = 0, totalTax = 0, discount = 0;

            // Collect invoice-level notes from all items
            string notesValue = string.Join("; ", dgvItems.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => r.Cells["Notes"].Value?.ToString()));

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;

                decimal rowTotal = 0, rowSalesTax = 0, rowFurtherTax = 0;
                decimal.TryParse(row.Cells["TotalValue"].Value?.ToString(), out rowTotal);
                decimal.TryParse(row.Cells["SalesTaxAmount"].Value?.ToString(), out rowSalesTax);
                decimal.TryParse(row.Cells["FurtherTaxAmount"].Value?.ToString(), out rowFurtherTax);

                subTotal += rowTotal;
                totalTax += rowSalesTax + rowFurtherTax;
            }

            decimal grandTotal = subTotal + totalTax - discount;

            // Insert Invoice with notes
            int invoiceId = DatabaseHelper.AddInvoice(
                customerId,
                sellerId,
                invoiceDate,
                subTotal,
                totalTax,
                discount,
                grandTotal,
                notes: notesValue      // ✅ Save all item notes

            );

            if (invoiceId <= 0)
                throw new Exception("❌ Failed to insert invoice.");

            // Insert Items
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;

                int productId = 0;
                if (cmbHSCode.SelectedIndex >= 0)
                    int.TryParse(products.Rows[cmbHSCode.SelectedIndex]["productId"]?.ToString(), out productId);

                decimal quantity = 0, unitPrice = 0, rate = 0, totalValues = 0, valueExclGST = 0, salesTax = 0, furtherTax = 0;
                decimal.TryParse(row.Cells["Quantity"].Value?.ToString(), out quantity);
                decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice);
                decimal.TryParse(row.Cells["rate"].Value?.ToString(), out rate);
                decimal.TryParse(row.Cells["TotalValue"].Value?.ToString(), out totalValues);
                decimal.TryParse(row.Cells["ValueExclGST"].Value?.ToString(), out valueExclGST);
                decimal.TryParse(row.Cells["SalesTaxAmount"].Value?.ToString(), out salesTax);
                decimal.TryParse(row.Cells["FurtherTaxAmount"].Value?.ToString(), out furtherTax);
                decimal ratee = 0;
                decimal.TryParse(row.Cells["Rate"].Value?.ToString().Replace("%", "").Trim(), out ratee);
                DatabaseHelper.AddInvoiceItem(
                    invoiceId,
                    productId,
                    description: row.Cells["Product_Desc"].Value?.ToString(),
                    quantity: quantity,
                    rate: ratee.ToString(),
                    unitPrice: unitPrice,
                    totalValues: totalValues,
                    valueSalesExcludingST: valueExclGST,
                    fixedNotifiedValueOrRetailPrice: 0,
                    salesTaxApplicable: salesTax,
                    salesTaxWithheldAtSource: 0,
                    extraTax: 0,
                    furtherTax: furtherTax,
                    fedPayable: 0,
                    discount: 0,
                    saleType: "Goods at standard rate",
                    sroItemSerialNo: ""
                );
            }

            MessageBox.Show($"✅ Invoice Saved  successfully\nInvoice ID: {invoiceId}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            dgvItems.Rows.Clear();
            ClearFields();
            UpdateSubtotal();
            txtInvoiceNumber.Text = GetNextInvoiceNumber().ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Error saving posted invoice: {ex.Message}",
                            "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    private void PostAndSave(string fbrInvoiceNo)
    {
        try
        {
            if (dgvItems.Rows.Count == 0)
            {
                MessageBox.Show("⚠️ No items to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int customerId = 0, sellerId = 0;
            int.TryParse(cmbBuyerName.SelectedValue?.ToString(), out customerId);
            int.TryParse(cmbSellerName.SelectedValue?.ToString(), out sellerId);
            DateTime invoiceDate = DateTime.Now;

            decimal subTotal = 0, totalTax = 0, discount = 0;

            // Collect invoice-level notes from all items
            string notesValue = string.Join("; ", dgvItems.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => r.Cells["Notes"].Value?.ToString()));

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;

                decimal rowTotal = 0, rowSalesTax = 0, rowFurtherTax = 0;
                decimal.TryParse(row.Cells["TotalValue"].Value?.ToString(), out rowTotal);
                decimal.TryParse(row.Cells["SalesTaxAmount"].Value?.ToString(), out rowSalesTax);
                decimal.TryParse(row.Cells["FurtherTaxAmount"].Value?.ToString(), out rowFurtherTax);

                subTotal += rowTotal;
                totalTax += rowSalesTax + rowFurtherTax;
            }

            decimal grandTotal = subTotal + totalTax - discount;

            // Insert Invoice with notes
            int invoiceId = DatabaseHelper.PostInvoice(
                customerId,
                sellerId,
                invoiceDate,
                subTotal,
                totalTax,
                discount,
                grandTotal,
                notes: notesValue,        // ✅ Save all item notes

                fbrInvoiceNumber: fbrInvoiceNo
            );

            if (invoiceId <= 0)
                throw new Exception("❌ Failed to insert invoice.");

            // Insert Items
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;

                int productId = 0;
                if (cmbHSCode.SelectedIndex >= 0)
                    int.TryParse(products.Rows[cmbHSCode.SelectedIndex]["productId"]?.ToString(), out productId);

                decimal quantity = 0, unitPrice = 0, rate = 0, totalValues = 0, valueExclGST = 0, salesTax = 0, furtherTax = 0;
                decimal.TryParse(row.Cells["Quantity"].Value?.ToString(), out quantity);
                decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice);
                decimal.TryParse(row.Cells["Rate"].Value?.ToString(), out rate);
                decimal.TryParse(row.Cells["TotalValue"].Value?.ToString(), out totalValues);
                decimal.TryParse(row.Cells["ValueExclGST"].Value?.ToString(), out valueExclGST);
                decimal.TryParse(row.Cells["SalesTaxAmount"].Value?.ToString(), out salesTax);
                decimal.TryParse(row.Cells["FurtherTaxAmount"].Value?.ToString(), out furtherTax);
                decimal rateee = 0;
                decimal.TryParse(row.Cells["Rate"].Value?.ToString().Replace("%", "").Trim(), out rateee);
                DatabaseHelper.AddInvoiceItem(
                    invoiceId,
                    productId,
                    description: row.Cells["Product_Desc"].Value?.ToString(),
                    quantity: quantity,
                    rate: rateee.ToString(),
                    unitPrice: unitPrice,
                    totalValues: totalValues,
                    valueSalesExcludingST: valueExclGST,
                    fixedNotifiedValueOrRetailPrice: 0,
                    salesTaxApplicable: salesTax,
                    salesTaxWithheldAtSource: 0,
                    extraTax: 0,
                    furtherTax: furtherTax,
                    fedPayable: 0,
                    discount: 0,
                    saleType: "Goods at standard rate",
                    sroItemSerialNo: ""
                );
            }

            MessageBox.Show($"✅ Invoice posted successfully!\nFBR Invoice #: {fbrInvoiceNo}\nInvoice ID: {invoiceId}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            dgvItems.Rows.Clear();
            ClearFields();
            UpdateSubtotal();
            txtInvoiceNumber.Text = GetNextInvoiceNumber().ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Error saving posted invoice: {ex.Message}",
                            "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }



    private void RecalculateSalesTax(object sender, EventArgs e)
    {
        decimal qty = 0, unitPrice = 0, rate = 0;

        // Qty aur UnitPrice parse karo
        decimal.TryParse(txtQuantity.Text, out qty);
        decimal.TryParse(txtUnitPrice.Text, out unitPrice);

        // Rate textbox se % remove karke number nikaal lo
        string rateText = txtProdRate.Text.Replace("%", "").Trim();
        decimal.TryParse(rateText, out rate);

        // 1️⃣ Total Value = Qty * UnitPrice
        decimal totalValue = qty * unitPrice;
        txtTotalValue.Text = totalValue.ToString("N2");

        // 2️⃣ ValueExclGST = Total / (1 + Rate/100)
        decimal valueExclGST = qty * unitPrice ;
        txtValueExclGST.Text = valueExclGST.ToString("N2");

        // 3️⃣ Sales Tax = Total - ValueExclGST
        decimal salesTax = totalValue / 100 * 18;
        txtSalesTaxAmount.Text = salesTax.ToString("N2");
    }



}