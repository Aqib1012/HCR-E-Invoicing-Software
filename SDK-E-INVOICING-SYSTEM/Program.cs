using InvoiceApp;
using SDK_E_INVOICING_SYSTEM;
using SDK_E_INVOICING_SYSTEM.Data;
using System;
using System.Windows.Forms;

namespace Sidekick_eInvoice
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database initialization failed:\n" + ex.Message);
                return;
            }

             Application.Run(new LoginForm());
            //Application.Run(new GenerateInvoiceForm());
            //Application.Run(new DashboardForm());
           // Application.Run(new InvoiceViewerForm());

        }
    }
}