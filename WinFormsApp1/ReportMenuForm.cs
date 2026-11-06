using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class ReportMenuForm : Form
    {
        private Button btnMilkReport = null!;
        private Button btnPaymentReport = null!;
        private Button btnFarmerLedger = null!;
        private Button btnBack = null!;

        public ReportMenuForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form Window Configuration
            Text = "Advanced Milk Dairy - Reports Dashboard";
            Size = new Size(500, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(190, 210, 230); // Matches your light blue background

            // Header Banner Label
            Label lblHeader = new Label
            {
                Text = "REPORTS & SUMMARY CONTROL",
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(20, 50, 90),
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblHeader);

            // Create and Layout Buttons Vertically (Matching your aesthetic layout)
            btnMilkReport = CreateMenuButton("Milk Collection Report", 100);
            btnPaymentReport = CreateMenuButton("Payments Summary Report", 180);
            btnFarmerLedger = CreateMenuButton("Farmer General Ledger", 260);

            btnBack = CreateMenuButton("Close Menu", 360);
            btnBack.BackColor = Color.IndianRed; // Distinct color for exit action

            // Link click actions to methods
            btnMilkReport.Click += BtnMilkReport_Click;
            btnPaymentReport.Click += BtnPaymentReport_Click;
            btnFarmerLedger.Click += BtnFarmerLedger_Click;
            btnBack.Click += (s, e) => this.Close();
        }

        // Helper method to keep UI styling perfectly identical for all dashboard entries
        private Button CreateMenuButton(string text, int topLocationY)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(100, topLocationY),
                Size = new Size(300, 50),
                BackColor = Color.FromArgb(0, 120, 215), // Matches your solid blue button style
                ForeColor = Color.White,
                Font = new Font("Arial", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderColor = Color.White;
            btn.FlatAppearance.BorderSize = 1;

            Controls.Add(btn);
            return btn;
        }

        // ==========================================
        // BUTTON CLICK ACTIONS
        // ==========================================

        private void BtnMilkReport_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Opening Milk Collection Summary Report...", "Report Module");
            // Connect your specific layout view form here when ready:
            // using var frm = new MilkCollectionReportForm();
            // frm.ShowDialog();
        }

        private void BtnPaymentReport_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Opening Payments Summary Report...", "Report Module");
            // Connect your payment report form here:
        }

        private void BtnFarmerLedger_Click(object? sender, EventArgs e)
        {
            // Instantiates and shows your working LedgerForm code
            using (LedgerForm ledgerForm = new LedgerForm())
            {
                ledgerForm.ShowDialog(this);
            }
        }
    }
}


