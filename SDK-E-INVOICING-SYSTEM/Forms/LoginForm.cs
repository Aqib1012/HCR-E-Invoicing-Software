using SDK_E_INVOICING_SYSTEM;
using SDK_E_INVOICING_SYSTEM.Data;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace InvoiceApp
{
    public class LoginForm : Form
    {
        private Panel leftPanel, footerPanel;
        private Label lblBrand, lblTitle, lblUsername, lblPassword, lblFooter;
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnForgot;

        private bool isUserPlaceholder = true;
        private bool isPassPlaceholder = true;

        public LoginForm()
        {

            InitializeUI();
        }

        private void InitializeUI()
        {

            // Form Properties
            this.Text = "Login - Invoice System";
            this.MinimumSize = new Size(700, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            this.SuspendLayout();

            // Left Branding Panel (with gradient)
            // Left Branding Panel (Solid Color – Safe)
            leftPanel = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = ColorTranslator.FromHtml("#2E7D32") // solid green
            };
            this.Controls.Add(leftPanel);

            // =====================
            // Logo Icon
            // =====================
            var logoIcon = new Label()
            {
                Text = "🧾",
                Font = new Font("Segoe UI Emoji", 70, FontStyle.Regular),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 150,
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(logoIcon);

            // =====================
            // Brand Text
            // =====================
            lblBrand = new Label()
            {
                Text = "SIDEKICK",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblBrand);

            // =====================
            // Tagline
            // =====================
            var lblTagline = new Label()
            {
                Text = "Digital Invoicing System",
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                ForeColor = Color.WhiteSmoke,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblTagline);

            // =====================
            // Divider Line
            // =====================
            var divider = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = Color.White
            };
            leftPanel.Controls.Add(divider);

            // =====================
            // Spacer (Filler)
            // =====================
            var filler = new Panel()
            {
                Dock = DockStyle.Fill
            };
            leftPanel.Controls.Add(filler);

            // =====================
            // Developer Credit
            // =====================
            var lblDev = new Label()
            {
                Text = "© 2025 | Developed by Sidekick",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Dock = DockStyle.Bottom,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblDev);

            // Title
            lblTitle = new Label()
            {
                Text = "LOGIN TO SYSTEM",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(280, 50)
            };
            this.Controls.Add(lblTitle);

            // Username Label
            lblUsername = new Label()
            {
                Text = "Username:",
                Location = new Point(280, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true
            };
            this.Controls.Add(lblUsername);

            // Username TextBox
            txtUsername = new TextBox()
            {
                Location = new Point(380, 115),
                Width = 250,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Text = "Enter username"
            };
            this.Controls.Add(txtUsername);

            txtUsername.GotFocus += RemoveUserPlaceholder;
            txtUsername.LostFocus += AddUserPlaceholder;
            txtUsername.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    txtPassword.Focus();
                }
            };

            // Password Label
            lblPassword = new Label()
            {
                Text = "Password:",
                Location = new Point(280, 170),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true
            };
            this.Controls.Add(lblPassword);

            // Password TextBox
            txtPassword = new TextBox()
            {
                Location = new Point(380, 165),
                Width = 250,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Text = "Enter password",
                UseSystemPasswordChar = false
            };
            this.Controls.Add(txtPassword);

            txtPassword.GotFocus += RemovePassPlaceholder;
            txtPassword.LostFocus += AddPassPlaceholder;
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    PerformLogin();
                }
            };

            // Login Button
            btnLogin = new Button()
            {
                Text = "Login",
                Location = new Point(380, 220),
                Width = 250,
                Height = 45,
                BackColor = ColorTranslator.FromHtml("#2E7D32"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => PerformLogin();
            this.Controls.Add(btnLogin);

            // Forgot Password Button
            btnForgot = new Button()
            {
                Text = "Forgot Password?",
                Location = new Point(380, 270),
                Width = 250,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Blue,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnForgot.FlatAppearance.BorderSize = 0;


            // Footer Panel
            footerPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.LightGray
            };
            this.Controls.Add(footerPanel);

            lblFooter = new Label()
            {
                Text = "0300-0228444 | WWW.SIDEKICK.PK | INFO@SIDEKICK.PK",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            footerPanel.Controls.Add(lblFooter);




            this.ResumeLayout(false);
        }

        // Placeholder Handlers
        private void RemoveUserPlaceholder(object sender, EventArgs e)
        {
            if (isUserPlaceholder)
            {
                txtUsername.Text = "";
                txtUsername.ForeColor = Color.Black;
                isUserPlaceholder = false;
            }
        }
        private void AddUserPlaceholder(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = "Enter username";
                txtUsername.ForeColor = Color.Gray;
                isUserPlaceholder = true;
            }
        }

        // Password Placeholder Safe Handling
        // ---- Password Placeholder Safe Handling ----
        private void RemovePassPlaceholder(object sender, EventArgs e)
        {
            if (isPassPlaceholder)
            {
                txtPassword.Clear();
                txtPassword.ForeColor = Color.Black;
                isPassPlaceholder = false;

                // ✅ sirf tab enable karo jab asli password likhna ho
                if (!string.IsNullOrEmpty(txtPassword.Text))
                {
                    txtPassword.UseSystemPasswordChar = true;
                }
            }
        }

        private void AddPassPlaceholder(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                isPassPlaceholder = true;
                txtPassword.UseSystemPasswordChar = false;  // placeholder ke liye plain text
                txtPassword.ForeColor = Color.Gray;
                txtPassword.Text = "Enter password";
            }
        }


        // Login Logic
        private void PerformLogin()
        {
            string username = isUserPlaceholder ? "" : txtUsername.Text.Trim();
            string password = isPassPlaceholder ? "" : txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Username is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Password is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (ValidateUser(username, password))
            {



                // Open Dashboard
                var dashboard = new DashboardForm();
                dashboard.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // SQLite Validation
        private bool ValidateUser(string username, string password)
        {
            // First check for default admin
            if (username.Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                password == "admin123")
            {
                return true;
            }

            // Otherwise check in database
            try
            {
                using (var conn = new SQLiteConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Users WHERE Username=@Username AND Password=@Password";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

    }
}