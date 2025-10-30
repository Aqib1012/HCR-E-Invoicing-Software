using InvoiceApp;
using SDK_E_INVOICING_SYSTEM.Data;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace SDK_E_INVOICING_SYSTEM
{
    public class DashboardForm : Form
    {
        private Panel sidebar, mainPanel;
        private Button activeButton = null;
        private Label lblTotalCustomers, lblTotalProducts, lblPostedInvoices, lblPendingInvoices, lblPendingPayments, lblTotalSellers;


        public DashboardForm()
        {
            

            // ===== Form Settings =====
            this.Text = "Sidekick e-Invoice - Dashboard";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.FormClosing += DashboardForm_FormClosing;

            // ===== Sidebar (Full Height) =====
            sidebar = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = ColorTranslator.FromHtml("#263238")
            };
            this.Controls.Add(sidebar);

            // ===== FBR Logo (Top of Sidebar) =====
            PictureBox fbrLogo = new PictureBox()
            {
                // Image = Image.FromFile("fbr_logo2.PNG"),
                Image = Image.FromFile("Logo@200-white.png"),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(200, 90),
                Location = new Point(10, 20),
                BackColor = Color.Transparent
            };
            sidebar.Controls.Add(fbrLogo);

            // ===== Sidebar Buttons =====
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0, 140, 0, 10),
                AutoScroll = false
            };
            sidebar.Controls.Add(buttonPanel);

            // Section Labels
            buttonPanel.Controls.Add(CreateSidebarLabel("MAIN"));

            // Buttons
            Button btnDashboard = CreateSidebarButton("📊 Dashboard");
            Button btnCustomers = CreateSidebarButton("👤 Customers");
            Button btnSeller = CreateSidebarButton("🏷️ Sellers");
            Button btnProducts = CreateSidebarButton("📦 Products");
            Button btnInvoice = CreateSidebarButton("🧾 Create Invoice");
            Button btnViewInvoices = CreateSidebarButton("📑 View Invoices");

            buttonPanel.Controls.AddRange(new Control[] {
                btnDashboard,
                btnCustomers,
                btnSeller,
                btnProducts,
                btnInvoice,
                btnViewInvoices
            });

            buttonPanel.Controls.Add(CreateSidebarLabel("MANAGEMENT"));
            Button btnPayments = CreateSidebarButton("💳 Payments");
            Button btnLogout = CreateSidebarButton("🚪 Logout");
            buttonPanel.Controls.Add(btnPayments);
            buttonPanel.Controls.Add(btnLogout);

            // ===== Bottom Logo =====
            PictureBox logoBox = new PictureBox()
            {
                Image = Image.FromFile("fbr_logo2.PNG"),
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 5, 0, 10)
            };
            sidebar.Controls.Add(logoBox);
            sidebar.Controls.SetChildIndex(logoBox, 0); // ensure it's at bottom


        

            // ===== Main Panel =====
            mainPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#F5F7FA"),
                Padding = new Padding(30, 30, 30, 30)
            };
            this.Controls.Add(mainPanel);

            // ===== Top Welcome Text =====
            Label lblWelcome = new Label()
            {
                Text = "Sidekick FBR Integrated e-Invoicing System 🚀",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#2E7D32"),
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblWelcome);
            lblWelcome.BringToFront();



            // ===== Info Cards =====
            TableLayoutPanel infoGrid = new TableLayoutPanel()
            {
                ColumnCount = 3,
                AutoSize = true,
                Anchor = AnchorStyles.Top,
                Location = new Point((mainPanel.Width - 700) / 2, 120),
                BackColor = Color.Transparent
            };
            infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            mainPanel.Controls.Add(infoGrid);
            mainPanel.Controls.Add(lblWelcome);
            lblWelcome.BringToFront();

            // ===== Info Cards =====
            infoGrid.Controls.Add(CreateInfoCard("Total Customers", "0", "👤", Color.FromArgb(76, 175, 80), out lblTotalCustomers), 0, 0);
            infoGrid.Controls.Add(CreateInfoCard("Total Products", "0", "📦", Color.FromArgb(33, 150, 243), out lblTotalProducts), 1, 0);
            infoGrid.Controls.Add(CreateInfoCard("Posted Invoices", "0", "✅", Color.FromArgb(156, 39, 176), out lblPostedInvoices), 2, 0);
            infoGrid.Controls.Add(CreateInfoCard("Pending Invoices", "0", "🕓", Color.FromArgb(255, 152, 0), out lblPendingInvoices), 0, 1);
            infoGrid.Controls.Add(CreateInfoCard("Pending Payments", "0", "🏦", Color.FromArgb(244, 67, 54), out lblPendingPayments), 1, 1);
            infoGrid.Controls.Add(CreateInfoCard("Total Sellers", "0", "🏷️", Color.FromArgb(255, 193, 7), out lblTotalSellers), 2, 1);

            // ===== Refresh Button =====
            Button btnRefresh = new Button()
            {
                Text = "🔄 Refresh",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 40),
                BackColor = ColorTranslator.FromHtml("#2E7D32"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefresh.FlatAppearance.BorderSize = 0;

            btnRefresh.Location = new Point(mainPanel.ClientSize.Width - btnRefresh.Width - 30, 80);
            mainPanel.Resize += (s, e) =>
            {
                btnRefresh.Location = new Point(mainPanel.ClientSize.Width - btnRefresh.Width - 30, 80);
            };
            mainPanel.Controls.Add(btnRefresh);
            btnRefresh.BringToFront();

            btnRefresh.MouseEnter += (s, e) => btnRefresh.BackColor = ColorTranslator.FromHtml("#1B5E20");
            btnRefresh.MouseLeave += (s, e) => btnRefresh.BackColor = ColorTranslator.FromHtml("#2E7D32");
            btnRefresh.Click += (s, e) => LoadDashboardValues();

            // ===== Load Data =====
            LoadDashboardValues();

            // ===== Button Events =====
            btnDashboard.Click += (s, e) => SetActiveButton(btnDashboard, "You are already on Dashboard");
            btnCustomers.Click += (s, e) => { SetActiveButton(btnCustomers); new CustomerForm().ShowDialog(); };
            btnProducts.Click += (s, e) => { SetActiveButton(btnProducts); new ProductForm().ShowDialog(); };
            btnInvoice.Click += (s, e) => { SetActiveButton(btnInvoice); new GenerateInvoiceForm().ShowDialog(); };
            btnViewInvoices.Click += (s, e) => { SetActiveButton(btnViewInvoices); new InvoiceViewerForm().ShowDialog(); };
            btnSeller.Click += (s, e) => { SetActiveButton(btnSeller); new SellerForm().ShowDialog(); };
            btnPayments.Click += (s, e) => { SetActiveButton(btnPayments); new PaymentForm().ShowDialog(); };
            btnLogout.Click += (s, e) =>
            {
                new LoginForm().Show();
                this.Hide();
            };
        }

        // ===== Sidebar Label =====
        private Label CreateSidebarLabel(string text)
        {
            return new Label()
            {
                Text = text,
                Width = 230,
                Height = 28,
                ForeColor = ColorTranslator.FromHtml("#90A4AE"),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 5, 0, 0),
                BackColor = ColorTranslator.FromHtml("#263238")
            };
        }

        // ===== Active Button =====
        private void SetActiveButton(Button btn, string message = "")
        {
            if (activeButton != null)
                activeButton.BackColor = ColorTranslator.FromHtml("#37474F");

            activeButton = btn;
            activeButton.BackColor = ColorTranslator.FromHtml("#1B5E20");

            if (!string.IsNullOrEmpty(message))
                MessageBox.Show(message, "Info");
        }

        // ===== Load Data =====
        private void LoadDashboardValues()
        {
            lblTotalCustomers.Text = DatabaseHelper.GetCount("Customers").ToString();
            lblTotalProducts.Text = DatabaseHelper.GetCount("Products").ToString();
            lblTotalSellers.Text = DatabaseHelper.GetCount("Sellers").ToString();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Invoices WHERE postStatus='Posted'", conn))
                    lblPostedInvoices.Text = cmd.ExecuteScalar().ToString();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Invoices WHERE postStatus='Saved' OR postStatus='Unpost'", conn))
                    lblPendingInvoices.Text = cmd.ExecuteScalar().ToString();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Invoices WHERE status='Unpaid'", conn))
                    lblPendingPayments.Text = cmd.ExecuteScalar().ToString();
            }
        }

        // ===== Info Card =====
        private Panel CreateInfoCard(string title, string value, string emoji, Color accentColor, out Label valueLabel)
        {
            Panel card = new Panel()
            {
                Size = new Size(200, 100),
                Margin = new Padding(15),
                BackColor = Color.White
            };
            card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 12, 12));
            card.Padding = new Padding(10);
            card.BorderStyle = BorderStyle.FixedSingle;

            Label lblIcon = new Label()
            {
                Text = emoji,
                Font = new Font("Segoe UI Emoji", 20, FontStyle.Regular),
                ForeColor = accentColor,
                AutoSize = false,
                Size = new Size(45, 45),
                Location = new Point(10, 25)
            };
            card.Controls.Add(lblIcon);

            valueLabel = new Label()
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                Location = new Point(65, 25)
            };
            card.Controls.Add(valueLabel);

            Label lblTitle = new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(65, 60)
            };
            card.Controls.Add(lblTitle);

            card.MouseEnter += (s, e) => card.BackColor = ColorTranslator.FromHtml("#F1F1F1");
            card.MouseLeave += (s, e) => card.BackColor = Color.White;

            return card;
        }

        // ===== Sidebar Button =====
        private Button CreateSidebarButton(string text)
        {
            Button btn = new Button()
            {
                Text = text,
                Width = 230,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = ColorTranslator.FromHtml("#37474F"),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ColorTranslator.FromHtml("#455A64");
            btn.MouseLeave += (s, e) =>
            {
                if (btn != activeButton)
                    btn.BackColor = ColorTranslator.FromHtml("#37474F");
            };
            return btn;
        }

        private void DashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                new LoginForm().Show();
            }
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );
    }
}