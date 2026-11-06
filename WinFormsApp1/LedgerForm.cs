
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class LedgerForm : Form
    {
        private readonly string connectionString =
            @"Server=.\SQLEXPRESS;Database=dairy;Integrated Security=True;TrustServerCertificate=True;";

        private ComboBox cmbFarmer = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private DataGridView dgvLedger = null!;

        private Button btnPrint = null!;
        private Button btnView = null!;
        private Button btnRefresh = null!;
        private Button btnClose = null!;

        private Label lblCollection = null!;
        private Label lblPayment = null!;
        private Label lblBalance = null!;

        private int printRowIndex = 0;

        public LedgerForm()
        {
            InitializeComponent();
            LoadFarmers();
        }

        // ==============================
        // FORM DESIGN
        // ==============================

        private void InitializeComponent()
        {
            Text = "Advanced Milk Dairy - Ledger";
            Size = new Size(1150, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            Label title = new Label
            {
                Text = "FARMER LEDGER MANAGEMENT",
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(20, 50, 90),
                ForeColor = Color.White,
                Font = new Font("Arial", 19, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(title);

            Label farmerLabel = new Label
            {
                Text = "Select Farmer:",
                Location = new Point(25, 90),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Controls.Add(farmerLabel);

            cmbFarmer = new ComboBox
            {
                Location = new Point(130, 85),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Controls.Add(cmbFarmer);

            Label fromLabel = new Label
            {
                Text = "From Date:",
                Location = new Point(350, 90),
                AutoSize = true
            };

            Controls.Add(fromLabel);

            dtpFrom = new DateTimePicker
            {
                Location = new Point(430, 85),
                Width = 130,
                Format = DateTimePickerFormat.Short
            };

            Controls.Add(dtpFrom);

            Label toLabel = new Label
            {
                Text = "To Date:",
                Location = new Point(580, 90),
                AutoSize = true
            };

            Controls.Add(toLabel);

            dtpTo = new DateTimePicker
            {
                Location = new Point(650, 85),
                Width = 130,
                Format = DateTimePickerFormat.Short
            };

            Controls.Add(dtpTo);

            btnView = new Button
            {
                Text = "View Ledger",
                Location = new Point(800, 82),
                Size = new Size(110, 35),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            btnView.Click += BtnView_Click;
            Controls.Add(btnView);

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(920, 82),
                Size = new Size(90, 35)
            };

            btnRefresh.Click += BtnRefresh_Click;
            Controls.Add(btnRefresh);

            lblCollection = CreateSummaryLabel(
                "Collection: ₹0.00", 25, Color.DarkBlue);

            lblPayment = CreateSummaryLabel(
                "Payments: ₹0.00", 350, Color.DarkGreen);

            lblBalance = CreateSummaryLabel(
                "Balance: ₹0.00", 700, Color.DarkRed);

            dgvLedger = new DataGridView
            {
                Location = new Point(25, 160),
                Size = new Size(1080, 440),
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                MultiSelect = false
            };

            Controls.Add(dgvLedger);

            btnPrint = new Button
            {
                Text = "Print Ledger",
                Location = new Point(830, 620),
                Size = new Size(130, 35),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnPrint.Click += BtnPrint_Click;
            Controls.Add(btnPrint);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(980, 620),
                Size = new Size(110, 35)
            };

            btnClose.Click += (s, e) => Close();
            Controls.Add(btnClose);
        }

        // ==============================
        // SUMMARY LABEL
        // ==============================

        private Label CreateSummaryLabel(
            string text, int x, Color color)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, 130),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = color
            };

            Controls.Add(lbl);
            return lbl;
        }

        // ==============================
        // LOAD FARMERS
        // ==============================

        private void LoadFarmers()
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 0 AS Id, 'ALL FARMERS' AS FarmerName
                        UNION ALL
                        SELECT Id, FarmerName
                        FROM dbo.Farmers
                        ORDER BY Id;";

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        cmbFarmer.DataSource = dt;
                        cmbFarmer.DisplayMember = "FarmerName";
                        cmbFarmer.ValueMember = "Id";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Farmer loading error:\n" + ex.Message);
            }
        }

        // ==============================
        // VIEW LEDGER BUTTON
        // ==============================

        private void BtnView_Click(object? sender, EventArgs e)
        {
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                MessageBox.Show(
                    "From Date should not be greater than To Date.");

                return;
            }

            if (cmbFarmer.SelectedValue == null)
                return;

            int farmerId =
                Convert.ToInt32(cmbFarmer.SelectedValue);

            if (farmerId == 0)
            {
                LoadAllFarmersLedger();
            }
            else
            {
                LoadFarmerLedger(farmerId);
            }
        }

        // ==============================
        // REFRESH BUTTON
        // ==============================

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            dtpFrom.Value = DateTime.Today;
            dtpTo.Value = DateTime.Today;

            LoadFarmers();

            dgvLedger.DataSource = null;
            ClearTotals();
        }

        // ==============================
        // ALL FARMERS LEDGER
        // ==============================

        private void LoadAllFarmersLedger()
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                    SELECT
                        f.FarmerCode AS [Card No],
                        f.FarmerName AS [Farmer Name],

                        ISNULL(c.Quantity, 0) AS [Total Quantity],

                        ISNULL(c.TotalAmount, 0)
                            AS [Collection Amount],

                        ISNULL(p.PaidAmount, 0)
                            AS [Payments],

                        ISNULL(c.TotalAmount, 0)
                        - ISNULL(p.PaidAmount, 0)
                            AS [Balance]

                    FROM dbo.Farmers f

                    LEFT JOIN
                    (
                        SELECT
                            FarmerId,
                            SUM(Quantity) AS Quantity,
                            SUM(ISNULL(TotalAmount, 0))
                                AS TotalAmount
                        FROM dbo.MilkCollections
                        WHERE CollectionDate >= @FromDate
                          AND CollectionDate < @ToDate
                        GROUP BY FarmerId
                    ) c ON f.Id = c.FarmerId

                    LEFT JOIN
                    (
                        SELECT
                            FarmerId,
                            SUM(Amount) AS PaidAmount
                        FROM dbo.Payments
                        WHERE PaymentDate >= @FromDate
                          AND PaymentDate < @ToDate
                        GROUP BY FarmerId
                    ) p ON f.Id = p.FarmerId

                    ORDER BY f.FarmerName;";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@FromDate", SqlDbType.Date).Value =
                            dtpFrom.Value.Date;

                        cmd.Parameters.Add(
                            "@ToDate", SqlDbType.Date).Value =
                            dtpTo.Value.Date.AddDays(1);

                        DataTable dt = new DataTable();

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dgvLedger.DataSource = dt;

                        UpdateTotals(
                            dt,
                            "Collection Amount",
                            "Payments",
                            "Balance");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "All Farmers Ledger Error:\n" + ex.Message);
            }
        }

        // ==============================
        // SINGLE FARMER LEDGER
        // ==============================

        private void LoadFarmerLedger(int farmerId)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                    SELECT
                        TransactionDate AS [Date],
                        Description AS [Description],
                        MilkType AS [Milk Type],
                        Quantity AS [Quantity],
                        Credit AS [Collection Amount],
                        Debit AS [Payment],
                        Credit - Debit AS [Transaction Balance]

                    FROM
                    (
                        SELECT
                            CollectionDate AS TransactionDate,
                            'Milk Collection' AS Description,
                            MilkType,
                            Quantity,
                            ISNULL(TotalAmount, 0) AS Credit,
                            CAST(0 AS DECIMAL(18,2)) AS Debit

                        FROM dbo.MilkCollections

                        WHERE FarmerId = @FarmerId
                          AND CollectionDate >= @FromDate
                          AND CollectionDate < @ToDate

                        UNION ALL

                        SELECT
                            PaymentDate,
                            'Payment',
                            'N/A',
                            CAST(0 AS DECIMAL(18,2)),
                            CAST(0 AS DECIMAL(18,2)),
                            Amount

                        FROM dbo.Payments

                        WHERE FarmerId = @FarmerId
                          AND PaymentDate >= @FromDate
                          AND PaymentDate < @ToDate

                    ) AS LedgerTransactions

                    ORDER BY TransactionDate;";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@FarmerId", SqlDbType.Int).Value =
                            farmerId;

                        cmd.Parameters.Add(
                            "@FromDate", SqlDbType.Date).Value =
                            dtpFrom.Value.Date;

                        cmd.Parameters.Add(
                            "@ToDate", SqlDbType.Date).Value =
                            dtpTo.Value.Date.AddDays(1);

                        DataTable dt = new DataTable();

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dgvLedger.DataSource = dt;

                        decimal collection = 0;
                        decimal payment = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            collection +=
                                Convert.ToDecimal(
                                    row["Collection Amount"]);

                            payment +=
                                Convert.ToDecimal(
                                    row["Payment"]);
                        }

                        lblCollection.Text =
                            "Collection: ₹" +
                            collection.ToString("N2");

                        lblPayment.Text =
                            "Payments: ₹" +
                            payment.ToString("N2");

                        lblBalance.Text =
                            "Balance: ₹" +
                            (collection - payment).ToString("N2");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Farmer Ledger Error:\n" + ex.Message);
            }
        }

        // ==============================
        // UPDATE TOTALS
        // ==============================

        private void UpdateTotals(
            DataTable dt,
            string collectionColumn,
            string paymentColumn,
            string balanceColumn)
        {
            decimal collection = 0;
            decimal payment = 0;
            decimal balance = 0;

            foreach (DataRow row in dt.Rows)
            {
                collection += Convert.ToDecimal(
                    row[collectionColumn]);

                payment += Convert.ToDecimal(
                    row[paymentColumn]);

                balance += Convert.ToDecimal(
                    row[balanceColumn]);
            }

            lblCollection.Text =
                "Collection: ₹" + collection.ToString("N2");

            lblPayment.Text =
                "Payments: ₹" + payment.ToString("N2");

            lblBalance.Text =
                "Balance: ₹" + balance.ToString("N2");
        }

        // ==============================
        // CLEAR TOTALS
        // ==============================

        private void ClearTotals()
        {
            lblCollection.Text = "Collection: ₹0.00";
            lblPayment.Text = "Payments: ₹0.00";
            lblBalance.Text = "Balance: ₹0.00";
        }

        // ==============================
        // PRINT BUTTON
        // ==============================

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            if (dgvLedger.Rows.Count == 0)
            {
                MessageBox.Show(
                    "No ledger records available to print.");

                return;
            }

            printRowIndex = 0;

            using (PrintDocument document = new PrintDocument())
            {
                document.DocumentName =
                    "Advanced Dairy Farmer Ledger";

                document.DefaultPageSettings.Landscape = true;

                document.PrintPage += PrintLedger_PrintPage;

                using (PrintPreviewDialog preview =
                    new PrintPreviewDialog())
                {
                    preview.Document = document;
                    preview.Width = 1000;
                    preview.Height = 700;
                    preview.ShowDialog();
                }
            }
        }

        // ==============================
        // PRINT PAGE
        // ==============================

        private void PrintLedger_PrintPage(
            object? sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics!;
            Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
            Font fontSubTitle = new Font("Arial", 11, FontStyle.Bold);
            Font fontRegular = new Font("Arial", 9, FontStyle.Regular);
            Font fontBold = new Font("Arial", 9, FontStyle.Bold);

            StringFormat rightAlign = new StringFormat { Alignment = StringAlignment.Far };

            int y = 40;
            int startX = 25;

            // 1. HEADER BRANDING BLOCK
            g.DrawString("ADVANCED MILK DAIRY SYSTEM", fontTitle, Brushes.DarkBlue, startX, y);
            y += 25;
            g.DrawString("FARMER LEDGER REPORT", fontSubTitle, Brushes.Black, startX, y);
            y += 22;

            string dateRangeStr = $"From: {dtpFrom.Value:dd-MMM-yy}  To: {dtpTo.Value:dd-MMM-yy}";
            g.DrawString(dateRangeStr, fontRegular, Brushes.Black, startX, y);
            y += 30;

            // 2. DEFINE EXPLICIT COLUMN X-POSITIONS
            int colCardNo = startX;
            int colName = startX + 60;
            int colQty = startX + 220;
            int colCollAmt = startX + 340;
            int colPayments = startX + 490;
            int colBalance = startX + 610;
            int endX = startX + 700;

            // 3. DRAW HEADERS WITH DIVIDER PIPES
            g.DrawString("Card No", fontBold, Brushes.DarkBlue, colCardNo, y);
            g.DrawString("|  Farmer Name", fontBold, Brushes.DarkBlue, colName - 10, y);
            g.DrawString("|  Total Quantity", fontBold, Brushes.DarkBlue, colQty - 10, y);
            g.DrawString("|  Collection Amount", fontBold, Brushes.DarkBlue, colCollAmt - 10, y);
            g.DrawString("|  Payments", fontBold, Brushes.DarkBlue, colPayments - 10, y);
            g.DrawString("|  Balance", fontBold, Brushes.DarkBlue, colBalance - 10, y);
            g.DrawString("|", fontBold, Brushes.DarkBlue, endX, y);

            y += 18;
            g.DrawLine(Pens.Black, startX, y, endX + 8, y);
            y += 12;

            decimal totalCollectionSummary = 0;
            decimal totalPaymentsSummary = 0;

            // 4. PRINT RECORDS IN GRID SYSTEM
            while (printRowIndex < dgvLedger.Rows.Count)
            {
                DataGridViewRow row = dgvLedger.Rows[printRowIndex];

                // Safe Farmer Card ID extraction
                string cardNo = "1";
                if (cmbFarmer.SelectedValue != null && Convert.ToInt32(cmbFarmer.SelectedValue) != 0)
                {
                    cardNo = cmbFarmer.SelectedValue.ToString()!;
                }

                string name = cmbFarmer.Text == "ALL FARMERS" ? "Summary View" : cmbFarmer.Text;
                if (dgvLedger.Columns.Contains("FarmerName"))
                    name = row.Cells["FarmerName"].Value?.ToString() ?? name;

                // --- MULTI-METHOD SAFE EXTRACTION BLOCK ---

                // 1. Extract Collection Amount (Grid Column Index 3)
                decimal colAmt = 0.0000m;
                if (dgvLedger.Columns.Contains("Collection Amount"))
                    colAmt = Convert.ToDecimal(row.Cells["Collection Amount"].Value ?? 0.0000m);
                else if (dgvLedger.Columns.Contains("CollectionAmount"))
                    colAmt = Convert.ToDecimal(row.Cells["CollectionAmount"].Value ?? 0.0000m);
                else if (row.Cells.Count > 3)
                    colAmt = Convert.ToDecimal(row.Cells[3].Value ?? 0.0000m); // Index fallback

                // 2. Extract Quantity (Grid Column Index 4)
                double qty = 0.0;
                if (dgvLedger.Columns.Contains("Quantity"))
                    qty = Convert.ToDouble(row.Cells["Quantity"].Value ?? 0.0);
                else if (row.Cells.Count > 4)
                    qty = Convert.ToDouble(row.Cells[4].Value ?? 0.0); // Index fallback

                // 3. Extract Payment (Grid Column Index 5)
                decimal payAmt = 0.00m;
                if (dgvLedger.Columns.Contains("Payment"))
                    payAmt = Convert.ToDecimal(row.Cells["Payment"].Value ?? 0.00m);
                else if (row.Cells.Count > 5)
                    payAmt = Convert.ToDecimal(row.Cells[5].Value ?? 0.00m); // Index fallback

                // 4. Extract Balance (Grid Column Index 6)
                decimal balance = 0.00m;
                if (dgvLedger.Columns.Contains("Transaction Balance"))
                    balance = Convert.ToDecimal(row.Cells["Transaction Balance"].Value ?? 0.00m);
                else if (dgvLedger.Columns.Contains("TransactionBalance"))
                    balance = Convert.ToDecimal(row.Cells["TransactionBalance"].Value ?? 0.00m);
                else if (row.Cells.Count > 6)
                    balance = Convert.ToDecimal(row.Cells[6].Value ?? 0.00m); // Index fallback

                // ------------------------------------------

                totalCollectionSummary += colAmt;
                totalPaymentsSummary += payAmt;

                // Draw data row text items
                g.DrawString(cardNo, fontRegular, Brushes.Black, colCardNo, y);
                g.DrawString($"|  {name}", fontRegular, Brushes.Black, colName - 10, y);

                g.DrawString($"|", fontRegular, Brushes.Black, colQty - 10, y);
                g.DrawString($"{qty:F2}", fontRegular, Brushes.Black, new RectangleF(colQty, y, 90, 20), rightAlign);

                g.DrawString($"|", fontRegular, Brushes.Black, colCollAmt - 10, y);
                g.DrawString($"{colAmt:F4}", fontRegular, Brushes.Black, new RectangleF(colCollAmt, y, 110, 20), rightAlign);

                g.DrawString($"|", fontRegular, Brushes.Black, colPayments - 10, y);
                g.DrawString($"{payAmt:F2}", fontRegular, Brushes.Black, new RectangleF(colPayments, y, 80, 20), rightAlign);

                g.DrawString($"|", fontRegular, Brushes.Black, colBalance - 10, y);
                g.DrawString($"{balance:F2}", fontRegular, Brushes.Black, new RectangleF(colBalance, y, 80, 20), rightAlign);

                g.DrawString("|", fontRegular, Brushes.Black, endX, y);

                y += 24;
                printRowIndex++;

                if (y > e.MarginBounds.Bottom - 60)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            // 5. DRAW CLOSING FOOTER LINE & TOTAL SUMMARY BLOCK
            g.DrawLine(Pens.Black, startX, y, endX + 8, y);
            y += 20;

            g.DrawString($"Collection: ₹{totalCollectionSummary:N2}", fontBold, Brushes.DarkBlue, startX, y);
            y += 20;
            g.DrawString($"Payments: ₹{totalPaymentsSummary:N2}", fontBold, Brushes.DarkGreen, startX, y);
            y += 20;
            g.DrawString($"Balance: ₹{(totalCollectionSummary - totalPaymentsSummary):N2}", fontBold, Brushes.DarkRed, startX, y);

            e.HasMorePages = false;
        }
    }
}
