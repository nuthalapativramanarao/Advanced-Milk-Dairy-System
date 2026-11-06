
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class PaymentEntryForm : Form
    {
        // SQL Server Connection
        private readonly string connectionString =
            @"Server=.\SQLEXPRESS;Database=dairy;Integrated Security=True;TrustServerCertificate=True;";

        // Form Controls
        private ComboBox cmbFarmer = null!;
        private Label lblLedgerBalance = null!;
        private TextBox txtAmount = null!;
        private DateTimePicker dtpPaymentDate = null!;
        private TextBox txtRemarks = null!;
        private Button btnSave = null!;
        private Button btnClose = null!;

        // Receipt Information
        private string printFarmerName = "";
        private string printAmount = "";
        private string printDate = "";
        private string printRemarks = "";
        private string printRemainingBalance = "";

        public PaymentEntryForm()
        {
            InitializeComponent();

            EnsurePaymentsTable();
            LoadFarmers();
        }

        // ==========================================
        // FORM DESIGN
        // ==========================================

        private void InitializeComponent()
        {
            Text = "Advanced Milk Dairy - Payment Entry";
            Size = new Size(450, 480);

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.WhiteSmoke;

            Label lblTitle = new Label
            {
                Text = "NEW PAYMENT ENTRY",
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(20, 50, 90),
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(lblTitle);

            // Farmer
            Label lblFarmer = CreateLabel("Select Farmer:", 85);

            cmbFarmer = new ComboBox
            {
                Location = new Point(160, 82),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbFarmer.SelectedIndexChanged +=
                CmbFarmer_SelectedIndexChanged;

            Controls.Add(lblFarmer);
            Controls.Add(cmbFarmer);

            // Ledger Balance
            Label lblBalText = CreateLabel("Ledger Balance:", 135);

            lblLedgerBalance = new Label
            {
                Text = "₹ 0.00",
                Location = new Point(160, 135),
                AutoSize = true,
                Font = new Font("Arial", 11, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            Controls.Add(lblBalText);
            Controls.Add(lblLedgerBalance);

            // Payment Date
            Label lblDate = CreateLabel("Payment Date:", 185);

            dtpPaymentDate = new DateTimePicker
            {
                Location = new Point(160, 182),
                Width = 220,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd-MM-yyyy"
            };

            Controls.Add(lblDate);
            Controls.Add(dtpPaymentDate);

            // Amount
            Label lblAmount = CreateLabel("Amount (₹):", 235);

            txtAmount = new TextBox
            {
                Location = new Point(160, 232),
                Width = 220,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Controls.Add(lblAmount);
            Controls.Add(txtAmount);

            // Remarks
            Label lblRemarks = CreateLabel("Remarks:", 285);

            txtRemarks = new TextBox
            {
                Location = new Point(160, 282),
                Width = 220,
                Height = 50,
                Multiline = true
            };

            Controls.Add(lblRemarks);
            Controls.Add(txtRemarks);

            // Save Button
            btnSave = new Button
            {
                Text = "Save & Print",
                Location = new Point(160, 365),
                Size = new Size(115, 35),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            btnSave.Click += BtnSave_Click;

            Controls.Add(btnSave);

            // Close Button
            btnClose = new Button
            {
                Text = "Cancel",
                Location = new Point(285, 365),
                Size = new Size(95, 35)
            };

            btnClose.Click += (s, e) => Close();

            Controls.Add(btnClose);
        }

        private Label CreateLabel(string text, int yPosition)
        {
            return new Label
            {
                Text = text,
                Location = new Point(35, yPosition),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30)
            };
        }

        // ==========================================
        // CREATE PAYMENTS TABLE IF NOT EXISTS
        // ==========================================

        private void EnsurePaymentsTable()
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                        IF OBJECT_ID('dbo.Payments', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.Payments
                            (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                FarmerId INT NOT NULL,
                                PaymentDate DATE NOT NULL,
                                Amount DECIMAL(18,2) NOT NULL,
                                Remarks NVARCHAR(500) NULL,
                                CreatedAt DATETIME2
                                    DEFAULT SYSDATETIME()
                            );
                        END;";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Payments table error:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // LOAD FARMERS
        // ==========================================

        private void LoadFarmers()
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT Id, FarmerName
                        FROM dbo.Farmers
                        ORDER BY FarmerName;";

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        cmbFarmer.DataSource = null;
                        cmbFarmer.DisplayMember = "FarmerName";
                        cmbFarmer.ValueMember = "Id";
                        cmbFarmer.DataSource = dt;

                        if (dt.Rows.Count == 0)
                        {
                            lblLedgerBalance.Text =
                                "No farmers available";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading farmers:\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // FARMER SELECTION
        // ==========================================

        private void CmbFarmer_SelectedIndexChanged(
            object? sender, EventArgs e)
        {
            if (cmbFarmer.SelectedValue == null)
                return;

            if (!int.TryParse(
                Convert.ToString(cmbFarmer.SelectedValue),
                out int farmerId))
                return;

            FetchCurrentBalance(farmerId);
        }

        // ==========================================
        // FETCH FARMER LEDGER BALANCE
        // ==========================================

        private void FetchCurrentBalance(int farmerId)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            ISNULL(
                                (SELECT SUM(TotalAmount)
                                 FROM dbo.MilkCollections
                                 WHERE FarmerId = @FarmerId), 0
                            )
                            -
                            ISNULL(
                                (SELECT SUM(Amount)
                                 FROM dbo.Payments
                                 WHERE FarmerId = @FarmerId), 0
                            ) AS Balance;";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@FarmerId", SqlDbType.Int
                        ).Value = farmerId;

                        con.Open();

                        object? result = cmd.ExecuteScalar();

                        decimal balance =
                            result == null || result == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(result);

                        lblLedgerBalance.Text =
                            "₹ " + balance.ToString("N2");

                        lblLedgerBalance.ForeColor =
                            balance > 0
                            ? Color.DarkRed
                            : Color.DarkGreen;
                    }
                }
            }
            catch (Exception ex)
            {
                lblLedgerBalance.Text = "Balance unavailable";

                MessageBox.Show(
                    "Error fetching ledger balance:\n" + ex.Message,
                    "Balance Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // SAVE PAYMENT
        // ==========================================

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbFarmer.SelectedValue == null ||
                !int.TryParse(
                    Convert.ToString(cmbFarmer.SelectedValue),
                    out int farmerId))
            {
                MessageBox.Show(
                    "Please select a valid farmer.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!decimal.TryParse(
                txtAmount.Text.Trim(),
                out decimal amount) || amount <= 0)
            {
                MessageBox.Show(
                    "Please enter a valid positive payment amount.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtAmount.Focus();
                return;
            }

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO dbo.Payments
                        (
                            FarmerId,
                            PaymentDate,
                            Amount,
                            Remarks
                        )
                        VALUES
                        (
                            @FarmerId,
                            @PaymentDate,
                            @Amount,
                            @Remarks
                        );";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@FarmerId", SqlDbType.Int
                        ).Value = farmerId;

                        cmd.Parameters.Add(
                            "@PaymentDate", SqlDbType.Date
                        ).Value = dtpPaymentDate.Value.Date;

                        SqlParameter amountParameter =
                            cmd.Parameters.Add(
                                "@Amount", SqlDbType.Decimal);

                        amountParameter.Precision = 18;
                        amountParameter.Scale = 2;
                        amountParameter.Value = amount;

                        cmd.Parameters.Add(
                            "@Remarks", SqlDbType.NVarChar, 500
                        ).Value = string.IsNullOrWhiteSpace(txtRemarks.Text)
                            ? DBNull.Value
                            : txtRemarks.Text.Trim();

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Store receipt information
                printFarmerName = cmbFarmer.Text;
                printAmount = amount.ToString("N2");
                printDate =
                    dtpPaymentDate.Value.ToString("dd-MM-yyyy");

                printRemarks = string.IsNullOrWhiteSpace(txtRemarks.Text)
                    ? "-"
                    : txtRemarks.Text.Trim();

                // Refresh balance after payment
                FetchCurrentBalance(farmerId);

                printRemainingBalance =
                    lblLedgerBalance.Text;

                MessageBox.Show(
                    "Payment saved successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Open receipt preview
                TriggerReceiptPrint();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error saving payment:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // RECEIPT PRINT PREVIEW
        // ==========================================

        private void TriggerReceiptPrint()
        {
            using (PrintDocument pd = new PrintDocument())
            {
                pd.DocumentName =
                    "Advanced Dairy Farmer Payment Receipt";

                pd.DefaultPageSettings.Margins =
                    new Margins(20, 20, 20, 20);

                pd.PrintPage += Receipt_PrintPage;

                using (PrintPreviewDialog preview =
                    new PrintPreviewDialog())
                {
                    preview.Document = pd;
                    preview.Width = 650;
                    preview.Height = 600;

                    preview.StartPosition =
                        FormStartPosition.CenterScreen;

                    preview.ShowDialog(this);
                }
            }
        }

        // ==========================================
        // PRINT RECEIPT PAGE
        // ==========================================

        private void Receipt_PrintPage(
            object? sender, PrintPageEventArgs e)
        {
            Graphics? graphics = e.Graphics;

            if (graphics == null)
            {
                e.HasMorePages = false;
                return;
            }

            Graphics g = graphics;

            using (Font fontHeader =
                new Font("Arial", 13, FontStyle.Bold))
            using (Font fontSub =
                new Font("Arial", 11, FontStyle.Bold))
            using (Font fontBody =
                new Font("Arial", 10, FontStyle.Regular))
            using (Font fontAmount =
                new Font("Arial", 12, FontStyle.Bold))
            using (Pen blackPen = new Pen(Color.Black, 1))
            using (Pen grayPen = new Pen(Color.Gray, 1))
            {
                int x = 30;
                int y = 30;
                int width = 330;

                g.DrawString(
                    "ADVANCED MILK DAIRY SYSTEM",
                    fontHeader, Brushes.DarkBlue, x, y);

                y += 30;

                g.DrawString(
                    "PAYMENT RECEIPT",
                    fontSub, Brushes.Black, x, y);

                y += 30;

                g.DrawLine(blackPen, x, y, x + width, y);

                y += 20;

                g.DrawString(
                    "Date: " + printDate,
                    fontBody, Brushes.Black, x, y);

                y += 25;

                g.DrawString(
                    "Farmer Name: " + printFarmerName,
                    fontBody, Brushes.Black, x, y);

                y += 30;

                g.DrawLine(grayPen, x, y, x + width, y);

                y += 20;

                g.DrawString(
                    "AMOUNT PAID",
                    fontSub, Brushes.Black, x, y);

                y += 28;

                g.DrawString(
                    "Rs. " + printAmount,
                    fontAmount, Brushes.DarkGreen, x, y);

                y += 35;

                g.DrawString(
                    "Remarks: " + printRemarks,
                    fontBody, Brushes.Black,
                    new RectangleF(x, y, width, 45));

                y += 55;

                g.DrawLine(grayPen, x, y, x + width, y);

                y += 20;

                g.DrawString(
                    "Remaining Ledger Balance:",
                    fontSub, Brushes.DarkRed, x, y);

                y += 30;

                g.DrawString(
                    printRemainingBalance,
                    fontAmount, Brushes.DarkRed, x, y);

                y += 45;

                g.DrawLine(blackPen, x, y, x + width, y);

                y += 20;

                g.DrawString(
                    "Thank you!",
                    fontSub, Brushes.DarkBlue, x + 110, y);

                y += 25;

                g.DrawString(
                    "Visit Again",
                    fontBody, Brushes.Gray, x + 120, y);
            }

            e.HasMorePages = false;
        }
    }
}
