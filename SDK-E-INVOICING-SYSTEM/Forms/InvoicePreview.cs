using QRCoder;
using SDK_E_INVOICING_SYSTEM.Data;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

public class InvoicePreviewForm : Form
{
    private int invoiceId;

    private Label lblInvoiceNumber, lblFbrNumber, lblDate, lblStatus;
    private Label lblSubTotal, lblTax, lblGrandTotal;
    private Label lblSellerInfo, lblCustomerInfo;
    private DataGridView dgvItems;
    private PictureBox fbrLogo, sellerlogo, qrBox;
    private Button btnPrint, btnPdf;

    private Bitmap invoiceBitmap;
    private Panel pnlInvoice;

    public InvoicePreviewForm(int invoiceId)
    {
        this.invoiceId = invoiceId;
        this.Text = "Invoice Preview";
        this.Width = 950;
        this.Height = 900;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(245, 247, 250);
        this.AutoScroll = true;
        this.MaximizeBox = false;
        this.Padding = new Padding(20);

        InitializeLayout();
        LoadInvoiceData();
    }

    private void InitializeLayout()
    {
        // ===== PRINT BUTTON =====
        btnPrint = new Button
        {
            Text = "🖨 Print Invoice",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Width = 180,
            Height = 38,
            BackColor = Color.FromArgb(30, 60, 114),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(this.ClientSize.Width - 410, 15)
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += BtnPrint_Click;
        this.Controls.Add(btnPrint);

        // ===== PDF BUTTON =====
        btnPdf = new Button
        {
            Text = "📄 Download PDF",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Width = 180,
            Height = 38,
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(this.ClientSize.Width - 210, 15)
        };
        btnPdf.FlatAppearance.BorderSize = 0;
        btnPdf.Click += BtnPdf_Click;
        this.Controls.Add(btnPdf);

        // ===== PANEL FOR INVOICE =====
        pnlInvoice = new Panel
        {
            Name = "pnlInvoice",
            Width = 794,
            Height = 1123,
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            Location = new Point((this.ClientSize.Width - 794) / 2, 70),
            // Padding = new Padding(30)
            Padding = new Padding(50, 20, 30, 30) // extra left padding for vertical text

        };
        this.Controls.Add(pnlInvoice);
        // ===== VERTICAL INVOICE TEXT =====
        Label lblVerticalInvoice = new Label
        {
            Font = new Font("Segoe UI", 27, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 114),
            AutoSize = false,
            Width = 40,
            Height = 300,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 200),
            BackColor = Color.Transparent
        };

        lblVerticalInvoice.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TranslateTransform(0, lblVerticalInvoice.Height);
            e.Graphics.RotateTransform(-90);

            using (SolidBrush brush = new SolidBrush(lblVerticalInvoice.ForeColor))
            {
                e.Graphics.DrawString("INVOICE", lblVerticalInvoice.Font, brush, 0, 0);
            }

            e.Graphics.ResetTransform();
        };


        pnlInvoice.Controls.Add(lblVerticalInvoice);
        lblVerticalInvoice.BringToFront();


        TableLayoutPanel mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Padding = new Padding(20)
        };
        pnlInvoice.Controls.Add(mainPanel);

        // ===== HEADER =====
        var headerTable = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        FlowLayoutPanel leftHeader = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true };
        sellerlogo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Width = 250, Height = 90 };
        leftHeader.Controls.Add(sellerlogo);

        lblInvoiceNumber = new Label { Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true };
        lblFbrNumber = new Label { Font = new Font("Segoe UI", 10), AutoSize = true };
        lblDate = new Label { Font = new Font("Segoe UI", 10), AutoSize = true };
        lblStatus = new Label { Font = new Font("Segoe UI", 10, FontStyle.Italic), AutoSize = true, ForeColor = Color.DarkGreen };

        leftHeader.Controls.Add(lblInvoiceNumber);
        leftHeader.Controls.Add(lblFbrNumber);
        leftHeader.Controls.Add(lblDate);
        leftHeader.Controls.Add(lblStatus);
        headerTable.Controls.Add(leftHeader, 0, 0);

        FlowLayoutPanel rightHeader = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, RightToLeft = RightToLeft.Yes };
        fbrLogo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Width =100, Height = 100 };
        qrBox = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Width = 100, Height = 100 };
        rightHeader.Controls.Add(fbrLogo);
        rightHeader.Controls.Add(qrBox);
        headerTable.Controls.Add(rightHeader, 1, 0);

        mainPanel.Controls.Add(headerTable);
        mainPanel.Controls.Add(new Label { Height = 2, Dock = DockStyle.Top, BackColor = Color.LightGray });

        // ===== SELLER / BUYER INFO =====
        var infoPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(0, 20, 0, 10)
        };
        infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        // ===== Seller Box =====
        var sellerBox = new GroupBox
        {
            Text = "Seller Information",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(10),
            ForeColor = Color.FromArgb(30, 60, 114),
            Height = 120
        };

        // Label with AutoSize false and Dock Fill
        lblSellerInfo = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5f),
            TextAlign = ContentAlignment.TopLeft
        };
        sellerBox.Controls.Add(lblSellerInfo);

        // ===== Buyer Box =====
        var buyerBox = new GroupBox
        {
            Text = "Buyer Information",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(10),
            ForeColor = Color.FromArgb(30, 60, 114),
            Height = 120
        };

        lblCustomerInfo = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5f),
            TextAlign = ContentAlignment.TopLeft
        };
        buyerBox.Controls.Add(lblCustomerInfo);

        // Add to info panel
        infoPanel.Controls.Add(sellerBox, 0, 0);
        infoPanel.Controls.Add(buyerBox, 1, 0);
        mainPanel.Controls.Add(infoPanel);

        // ===== DATAGRIDVIEW =====
        dgvItems = new DataGridView
        {
            Dock = DockStyle.Top,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            GridColor = Color.White,
            ColumnHeadersHeight = 70,
            EnableHeadersVisualStyles = false,
            Height = 280,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        dgvItems.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
        dgvItems.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        dgvItems.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
        dgvItems.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);

        dgvItems.Columns.Add("hsCode", "Item Code");
        dgvItems.Columns.Add("unitPrice", "Unit Price");
        dgvItems.Columns.Add("quantity", "Qty");
        dgvItems.Columns.Add("TotalEx", "Amount (Excl. S.Tax)");
        dgvItems.Columns.Add("rate", "Sales Tax %");
        dgvItems.Columns.Add("TaxValue", "Sales Tax Value");
        dgvItems.Columns.Add("TotalInc", "Amount (Incl. S.Tax)");

        mainPanel.Controls.Add(dgvItems);

        // ===== TOTALS =====
        var totalsPanel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new Padding(0, 15, 20, 10) };
        totalsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        totalsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        Label MakeRightLabel(string text, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", bold ? 10.5f : 10f, bold ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
        }

        totalsPanel.Controls.Add(MakeRightLabel("Subtotal:"));
        totalsPanel.Controls.Add(lblSubTotal = MakeRightLabel("0.00"));
        totalsPanel.Controls.Add(MakeRightLabel("Sales Tax:"));
        totalsPanel.Controls.Add(lblTax = MakeRightLabel("0.00"));
        totalsPanel.Controls.Add(MakeRightLabel("Total Due:", true));
        totalsPanel.Controls.Add(lblGrandTotal = MakeRightLabel("0.00", true));

        mainPanel.Controls.Add(totalsPanel);

        // ===== FOOTER =====
        var footerLabel = new Label
        {
            Text = "First Floor, Randhawa Plaza, AKM Fazl-ul-Haq Rd, behind Kulsum International Hospital, G 6/2 Blue Area, Islamabad, 44000\n" +
                   "Tel: +923000228444, • Website: www.sidekick.pk",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            ForeColor = Color.Gray,
            Height = 50
        };
        mainPanel.Controls.Add(footerLabel);
    }

    private void LoadInvoiceData()
    {
        DataSet ds = DatabaseHelper.GetInvoicePreviewData(invoiceId);
        if (ds.Tables["InvoiceHeader"].Rows.Count == 0)
        {
            MessageBox.Show("Invoice not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        DataRow header = ds.Tables["InvoiceHeader"].Rows[0];

        try
        {
            if (header.Table.Columns.Contains("sellerLogoPath") && header["sellerLogoPath"] != DBNull.Value)
            {
                byte[] logoBytes = (byte[])header["sellerLogoPath"];
                using (MemoryStream ms = new MemoryStream(logoBytes))
                    sellerlogo.Image = Image.FromStream(ms);
            }
        }
        catch { sellerlogo.Image = null; }

        try
        {
            string fbrPath = Path.Combine(Application.StartupPath, "FBR_DIGITAL.PNG");
            if (File.Exists(fbrPath))
                fbrLogo.Image = Image.FromFile(fbrPath);
        }
        catch { }

        lblInvoiceNumber.Text = $"Invoice #: {header["invoiceNumber"]}";
        lblFbrNumber.Text = $"FBR Invoice #: {header["fbrInvoiceNumber"]}";
        lblDate.Text = $"Date: {Convert.ToDateTime(header["invoiceDate"]):yyyy-MM-dd}";
        lblStatus.Text = $"Status: {header["postStatus"]}";

        lblSellerInfo.Text = $"{header["sellerBusinessName"]}\nNTN/CNIC: {header["sellerNTNCNIC"]}\nProvince: {header["sellerProvince"]}\nAddress: {header["sellerAddress"]}";
        lblCustomerInfo.Text = $"{header["customerBusinessName"]}\nNTN/CNIC: {header["customerNTNCNIC"]}\nProvince: {header["customerProvince"]}\nAddress: {header["customerAddress"]}";

        DataTable items = ds.Tables["InvoiceItems"];
        dgvItems.Rows.Clear();
        decimal subTotal = 0, totalTax = 0, grandTotal = 0;

        foreach (DataRow row in items.Rows)
        {
            decimal qty = Convert.ToDecimal(row["quantity"]);
            decimal unitPrice = Convert.ToDecimal(row["unitPrice"]);
            decimal rate = row.Table.Columns.Contains("rate") && row["rate"] != DBNull.Value ? Convert.ToDecimal(row["rate"]) : 0;

            decimal totalEx = qty * unitPrice;
            decimal taxValue = totalEx * rate / 100;
            decimal totalInc = totalEx + taxValue;

            subTotal += totalEx;
            totalTax += taxValue;
            grandTotal += totalInc;

            dgvItems.Rows.Add(
                row["hsCode"],
                unitPrice,
                qty,
                totalEx,
                rate,
                taxValue,
                totalInc
            );
        }

        lblSubTotal.Text = $"{subTotal:N2}";
        lblTax.Text = $"{totalTax:N2}";
        lblGrandTotal.Text = $"{grandTotal:N2}";

        string fbrInvoiceNo = header["fbrInvoiceNumber"]?.ToString();
        if (!string.IsNullOrEmpty(fbrInvoiceNo))
        {
            string qrData = $"https://www.fbr.gov.pk/verifyInvoice?invoice={fbrInvoiceNo}";
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrDataObj = qrGen.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrDataObj);
            qrBox.Image = qrCode.GetGraphic(8);
        }
    }

    // ===== PRINT =====
    private void BtnPrint_Click(object sender, EventArgs e)
    {
        try
        {
            Bitmap bmp = new Bitmap(pnlInvoice.Width, pnlInvoice.Height);
            pnlInvoice.DrawToBitmap(bmp, new Rectangle(0, 0, pnlInvoice.Width, pnlInvoice.Height));
            invoiceBitmap = bmp;

            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            printDoc.PrintPage += (s, e2) =>
            {
                float scale = Math.Min((float)e2.MarginBounds.Width / bmp.Width,
                                       (float)e2.MarginBounds.Height / bmp.Height);
                int newWidth = (int)(bmp.Width * scale);
                int newHeight = (int)(bmp.Height * scale);
                e2.Graphics.DrawImage(bmp, e2.MarginBounds.Left, e2.MarginBounds.Top, newWidth, newHeight);
            };

            using (PrintPreviewDialog previewDlg = new PrintPreviewDialog())
            {
                previewDlg.Document = printDoc;
                previewDlg.Width = 1000;
                previewDlg.Height = 800;
                previewDlg.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Printing failed: " + ex.Message, "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ===== PDF =====
    private void BtnPdf_Click(object sender, EventArgs e)
    {
        try
        {
            Bitmap bmp = new Bitmap(pnlInvoice.Width, pnlInvoice.Height);
            pnlInvoice.DrawToBitmap(bmp, new Rectangle(0, 0, pnlInvoice.Width, pnlInvoice.Height));

            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "PDF Files|*.pdf", FileName = $"Invoice_{invoiceId}.pdf" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    PdfDocument pdfDoc = new PdfDocument();
                    PdfPage page = pdfDoc.AddPage();
                    page.Width = XUnit.FromMillimeter(210);
                    page.Height = XUnit.FromMillimeter(297);

                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    // ===== Replace this part =====
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Save bitmap to stream
                        ms.Position = 0; // Reset stream position

                        XImage img = XImage.FromStream(ms); // Load image from stream

                        float scale = Math.Min((float)page.Width.Point / img.PixelWidth,
                                               (float)page.Height.Point / img.PixelHeight);

                        double newWidth = img.PixelWidth * scale;
                        double newHeight = img.PixelHeight * scale;

                        gfx.DrawImage(img, (page.Width.Point - newWidth) / 2, (page.Height.Point - newHeight) / 2, newWidth, newHeight);
                    }

                    pdfDoc.Save(sfd.FileName);
                    MessageBox.Show("PDF saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("PDF generation failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

}
