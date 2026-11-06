using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {

        }

        private void btnPayments_Click(object sender, EventArgs e)
        {
            // Open the PaymentEntryForm as a modal dialog box
            using (PaymentEntryForm paymentForm = new PaymentEntryForm())
            {
                paymentForm.ShowDialog(this);
            }
        }


        private void btnFarmers_Click(object sender, EventArgs e)
        {
            using FarmerForm farmerForm = new FarmerForm();
            farmerForm.ShowDialog();
        }

        private void btnMilkCollection_Click(object sender, EventArgs e)
        {
            using MilkCollectionForm form = new MilkCollectionForm();
            form.ShowDialog();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
       "Are you sure you want to logout?",
       "Confirm Logout",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Form1 loginForm = new Form1();

                loginForm.Show();

                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (LedgerForm form = new LedgerForm())
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ledger opening error: " + ex.Message,
                    "Advanced Dairy System",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Rate_Chart_Click(object sender, EventArgs e)
        {
            using (RateChartForm frm = new RateChartForm())
            {
                frm.ShowDialog(this);
            }

        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            // Pop open your brand new reports hub form context overlay
            using (ReportMenuForm reportsMenu = new ReportMenuForm())
            {
                reportsMenu.ShowDialog(this);
            }
        }

    }
}
