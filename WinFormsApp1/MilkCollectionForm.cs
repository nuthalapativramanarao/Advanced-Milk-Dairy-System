
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public partial class MilkCollectionForm : Form
    {
        // =========================================
        // CONTROL DECLARATIONS
        // =========================================

        private string farmerName = "";
        private string village = "";
        private string farmerVillage = "";
        private TextBox txtCardNo = null!;
        private TextBox txtQuantity = null!;
        private TextBox txtFat = null!;
        private TextBox txtSNF = null!;
        private TextBox txtRate = null!;
        private TextBox txtAmount = null!;

        private Label lblWeight = null!;
        private Label lblFat = null!;
        private Label lblSNF = null!;

        private DateTimePicker dtpDate = null!;
        private Button btnClose = new Button();
        private ComboBox cmbShift = null!;
        private ComboBox cmbMilkType = null!;

        private DataGridView dgv = null!;

        private Button btnPrint = null!;
       

        private readonly PrintDocument printDoc =
            new PrintDocument();
        // =========================================
        // AUTOMATIC FARMER RECEIPT DATA
        // =========================================

        private string receiptFarmerCode = "";
        private string receiptFarmerName = "";
        private string receiptVillage = "";
        private string receiptDate = "";
        private string receiptSession = "";
        private string receiptMilkType = "";

        private decimal receiptQuantity;
        private decimal receiptFat;
        private decimal receiptSNF;
        private decimal receiptRate;
        private decimal receiptAmount;
        // =========================================
        // DATABASE CONNECTION
        // =========================================


        private readonly string connectionString =
    @"Data Source=DESKTOP-3Q90JGV\SQLEXPRESS;
      Initial Catalog=dairy;
      Integrated Security=True;
      Encrypt=False;
      TrustServerCertificate=True;";


        private readonly Color sidebarColor =
            Color.FromArgb(0, 64, 64);

        private readonly Color headerColor =
            Color.FromArgb(0, 128, 128);

        // =========================================
        // CONSTRUCTOR
        // =========================================

        public MilkCollectionForm()
        {
            InitializeCustomComponent();

            ConfigureGridColumns();

            RegisterEvents();

            printDoc.PrintPage += PrintDoc_PrintPage;


            // AUTOMATIC RATE LOADING
            cmbMilkType.SelectedIndexChanged +=
                (s, e) => LoadRateFromDatabase();

            txtFat.Leave +=
                (s, e) => LoadRateFromDatabase();

            txtSNF.TextChanged += (s, e) => LoadRateFromDatabase();

            this.Shown +=
                (s, e) => LoadRateFromDatabase();

            printDoc.PrintPage += PrintDoc_PrintPage;
            LoadInitialData();

        }

        // =========================================
        // FORM DESIGN
        // =========================================


        private void InitializeCustomComponent()
        {
            Text = "Advanced Milk Dairy - Milk Collection";

            Size = new Size(1250, 750);
            MinimumSize = new Size(1050, 650);

            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            BackColor = Color.FromArgb(220, 240, 235);

            AddEndSessionSummaryButton();

            // SIDEBAR
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 210,
                BackColor = sidebarColor
            };

            sidebar.Controls.Add(new Label
            {
                Text = "ADVANCED DAIRY",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                Location = new Point(12, 20),
                AutoSize = true
            });

            string[] menus =
            {
        "Farmer Entry",
        "Farmer Transaction",
        "Manual Entry",
        "Farmer Statement"
    };

            for (int i = 0; i < menus.Length; i++)
            {
                Button b = new Button
                {
                    Text = menus[i],
                    Location = new Point(0, 80 + i * 45),
                    Size = new Size(210, 40),
                    ForeColor = Color.White,
                    BackColor = sidebarColor,
                    FlatStyle = FlatStyle.Flat
                };

                b.FlatAppearance.BorderSize = 0;
                sidebar.Controls.Add(b);
            }

            // HEADER
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 125,
                BackColor = headerColor
            };

            txtCardNo = new TextBox
            {
                Location = new Point(20, 48),
                Width = 170,
                Font = new Font("Arial", 20, FontStyle.Bold)
            };

            lblWeight = CreateReadout("0.00", 240);
            lblFat = CreateReadout("0.0", 420);
            lblSNF = CreateReadout("0.0", 600);

            header.Controls.Add(new Label
            {
                Text = "CARD NUMBER",
                Location = new Point(20, 15),
                ForeColor = Color.White,
                AutoSize = true
            });

            header.Controls.Add(txtCardNo);

            AddHeaderLabel(header, "WEIGHT (KG)", 240);
            AddHeaderLabel(header, "FAT %", 420);
            AddHeaderLabel(header, "SNF %", 600);

            header.Controls.Add(lblWeight);
            header.Controls.Add(lblFat);
            header.Controls.Add(lblSNF);

            // WORKSPACE
            Panel workspace = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            // CONFIGURATION PANEL
            Panel config = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(180, 225, 215)
            };

            config.Controls.Add(new Label
            {
                Text = "Collection Date:",
                Location = new Point(10, 14),
                AutoSize = true
            });

            dtpDate = new DateTimePicker
            {
                Location = new Point(135, 10),
                Width = 125,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd-MM-yyyy"
            };

            config.Controls.Add(dtpDate);

            config.Controls.Add(new Label
            {
                Text = "Shift:",
                Location = new Point(270, 14),
                AutoSize = true
            });

            cmbShift = new ComboBox
            {
                Location = new Point(320, 10),
                Width = 85,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbShift.Items.AddRange(new object[] { "AM", "PM" });
            cmbShift.SelectedIndex = 0;

            config.Controls.Add(cmbShift);

            // REFRESH BUTTON
            Button btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(420, 9),
                Size = new Size(90, 30),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnRefresh.Click += (s, e) => LoadInitialData();
            config.Controls.Add(btnRefresh);

            // PRINT BUTTON
            btnPrint = new Button
            {
                Text = "Print Report",
                Location = new Point(525, 9),
                Size = new Size(115, 30),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnPrint.Click += btnPrint_Click;
            config.Controls.Add(btnPrint);

            // NEW CLOSE BUTTON
            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(655, 9),
                Size = new Size(90, 30),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            btnClose.FlatAppearance.BorderSize = 0;

            btnClose.Click += (s, e) =>
            {
                this.Close();
            };

            config.Controls.Add(btnClose);

            // ENTRY PANEL
            Panel entry = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(225, 245, 240)
            };

            cmbMilkType = AddCombo(
                entry,
                "Milk Type",
                10,
                new object[] { "Cow", "Buffalo" });

            txtQuantity = AddText(
                entry, "Quantity (Kg)", 140);

            txtFat = AddText(
                entry, "Fat %", 270);

            txtSNF = AddText(
                entry, "SNF %", 380);

            txtRate = AddText(
                entry, "Rate (₹/L)", 490);

            txtRate.ReadOnly = true;

            txtAmount = AddText(
                entry, "Amount (₹)", 600);

            txtAmount.ReadOnly = true;

            Button btnSave = new Button
            {
                Text = "Save Transaction",
                Location = new Point(745, 38),
                Size = new Size(155, 32),
                BackColor = Color.FromArgb(0, 100, 80),
                ForeColor = Color.White
            };

            btnSave.Click += btnSave_Click;
            entry.Controls.Add(btnSave);

            // DATA GRID
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            // ADD CONTROLS
            workspace.Controls.Add(dgv);
            workspace.Controls.Add(entry);
            workspace.Controls.Add(config);

            Controls.Add(workspace);
            Controls.Add(header);
            Controls.Add(sidebar);
        }



        private void AddEndSessionSummaryButton()
        {
            Button btnEndSessionSummary = new Button
            {
                Name = "btnEndSessionSummary",
                Text = "END SESSION SUMMARY",
                Width = 190,
                Height = 40,
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnEndSessionSummary.Location = new Point(
                this.ClientSize.Width - 210, 15);

            btnEndSessionSummary.Click += btnEndSessionSummary_Click;

            this.Controls.Add(btnEndSessionSummary);

            btnEndSessionSummary.BringToFront();
        }


        // =========================================
        // HELPER METHODS - DECLARED ONLY ONCE
        // =========================================

        private Label CreateReadout(string value, int x)
        {
            return new Label
            {
                Text = value,
                Location = new Point(x, 48),
                Size = new Size(150, 50),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private void AddHeaderLabel(
            Panel panel, string text, int x)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, 15),
                ForeColor = Color.White,
                AutoSize = true
            });
        }

        private ComboBox AddCombo(
            Panel panel,
            string label,
            int x,
            object[] items)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, 12),
                AutoSize = true
            });

            ComboBox c = new ComboBox
            {
                Location = new Point(x, 38),
                Width = 110,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            c.Items.AddRange(items);

            c.SelectedIndex = 0;

            panel.Controls.Add(c);

            return c;
        }

        private TextBox AddText(
            Panel panel,
            string label,
            int x)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, 12),
                AutoSize = true
            });

            TextBox t = new TextBox
            {
                Location = new Point(x, 38),
                Width = 110
            };

            panel.Controls.Add(t);

            return t;
        }

        // =========================================
        // GRID COLUMNS
        // =========================================

        private void ConfigureGridColumns()
        {
            string[,] columns =
            {
                { "ID", "ID" },
                { "FarmerCode", "Card No" },
                { "FarmerName", "Farmer Name" },
                { "CollectionDate", "Date" },
                { "Session", "Session" },
                { "MilkType", "Milk Type" },
                { "Quantity", "Quantity (Kg)" },
                { "Fat", "Fat %" },
                { "SNF", "SNF %" },
                { "Rate", "Rate (₹/L)" },
                { "TotalAmount", "Amount (₹)" }
            };

            dgv.Columns.Clear();

            for (int i = 0; i < columns.GetLength(0); i++)
            {
                dgv.Columns.Add(
                    new DataGridViewTextBoxColumn
                    {
                        Name = "col" + columns[i, 0],
                        DataPropertyName = columns[i, 0],
                        HeaderText = columns[i, 1]
                    });
            }

            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(20, 80, 130);

            dgv.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgv.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            dgv.DefaultCellStyle.Format = "0.00";

            dgv.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(240, 248, 245);
        }

        // =========================================
        // REGISTER EVENTS
        // =========================================

        private void RegisterEvents()
        {
            txtCardNo.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;

                    LoadFarmerDetails(
                        txtCardNo.Text.Trim());
                }
            };

            txtQuantity.TextChanged +=
                (s, e) => CalculateRate();

            txtFat.TextChanged +=
                (s, e) => CalculateRate();

            txtSNF.TextChanged +=
                (s, e) => CalculateRate();

            cmbMilkType.SelectedIndexChanged +=
                (s, e) => CalculateRate();

            dtpDate.ValueChanged +=
                (s, e) => LoadInitialData();

            cmbShift.SelectedIndexChanged +=
                (s, e) => LoadInitialData();
        }

        // =========================================
        // DECIMAL CONVERSION
        // =========================================

        private static bool TryDecimal(
            string input,
            out decimal value)
        {
            return decimal.TryParse(
                input,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out value);
        }

        private static void AddDecimalParameter(
            SqlCommand cmd,
            string name,
            byte precision,
            byte scale,
            decimal value)
        {
            SqlParameter p =
                cmd.Parameters.Add(
                    name,
                    SqlDbType.Decimal);

            p.Precision = precision;

            p.Scale = scale;

            p.Value = value;
        }

        // =========================================
        // RATE CALCULATION
        // =========================================

        private void CalculateRate()
        {
            // SNF Validation: Minimum 8.5
            if (!decimal.TryParse(
                    txtSNF.Text.Trim(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal snfValue)
                || snfValue < 8.5m)
            {
                txtRate.Clear();
                txtAmount.Clear();
                return;
            }

            if (!TryDecimal(txtFat.Text, out decimal fat))
            {
                txtRate.Clear();
                txtAmount.Clear();
                return;
            }

            if (fat <= 0 || fat >= 100 ||
                snfValue <= 0 || snfValue >= 100)
            {
                txtRate.Clear();
                txtAmount.Clear();
                return;
            }

            try
            {
                decimal? rate = GetMilkRate(
                    cmbMilkType.Text,
                    Math.Round(fat, 1),
                    Math.Round(snfValue, 1));

                if (rate == null)
                {
                    txtRate.Clear();
                    txtAmount.Clear();
                    return;
                }

                txtRate.Text = rate.Value.ToString(
                    "0.00", CultureInfo.InvariantCulture);

                if (TryDecimal(txtQuantity.Text, out decimal qty) &&
                    qty > 0)
                {
                    txtAmount.Text =
                        (qty * rate.Value).ToString(
                            "0.00", CultureInfo.InvariantCulture);
                }
                else
                {
                    txtAmount.Clear();
                }
            }
            catch (Exception ex)
            {
                txtRate.Clear();
                txtAmount.Clear();

                MessageBox.Show(
                    "Rate Loading Error: " + ex.Message);
            }
        }

        // =========================================
        // GET RATE FROM MILK RATE CHART
        // =========================================

        private decimal? GetMilkRate(
            string milkType,
            decimal fat,
            decimal snf)
        {
            const string query = @"
                SELECT TOP 1 RatePerLitre
                FROM dbo.MilkRateChart
                WHERE MilkType = @MilkType
                  AND FatPercent = @Fat
                  AND SNFPercent = @SNF
                  AND IsActive = 1;";

            using SqlConnection con =
                new SqlConnection(connectionString);

            using SqlCommand cmd =
                new SqlCommand(query, con);

            cmd.Parameters.Add(
                "@MilkType",
                SqlDbType.NVarChar,
                50).Value = milkType;

            AddDecimalParameter(
                cmd, "@Fat", 4, 1, fat);

            AddDecimalParameter(
                cmd, "@SNF", 4, 1, snf);

            con.Open();

            object? result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return null;

            return Convert.ToDecimal(result);
        }

        // =========================================
        // LOAD GRID DATA
        // =========================================

        private void LoadInitialData()
        {
            if (dtpDate == null ||
                cmbShift == null ||
                dgv == null)
                return;

            DateTime selectedDate =
                dtpDate.Value.Date;

            string selectedShift =
                cmbShift.Text;

            string sql = @"
                SELECT
                    mc.Id AS ID,
                    f.FarmerCode AS FarmerCode,
                    f.FarmerName AS FarmerName,

                    FORMAT(
                        mc.CollectionDate,
                        'dd-MM-yyyy'
                    ) AS CollectionDate,

                    CASE
                        WHEN mc.Session = 'Morning'
                            THEN 'AM'
                        WHEN mc.Session = 'Evening'
                            THEN 'PM'
                        ELSE mc.Session
                    END AS Session,

                    mc.MilkType AS MilkType,
                    mc.Quantity AS Quantity,
                    mc.Fat AS Fat,
                    mc.SNF AS SNF,
                    mc.Rate AS Rate,
                    mc.TotalAmount AS TotalAmount

                FROM dbo.MilkCollections mc

                INNER JOIN dbo.Farmers f
                    ON f.Id = mc.FarmerId

                WHERE mc.CollectionDate >= @Date
                  AND mc.CollectionDate < DATEADD(day, 1, @Date)";

            if (selectedShift == "AM")
            {
                sql += " AND mc.Session = 'Morning'";
            }
            else if (selectedShift == "PM")
            {
                sql += " AND mc.Session = 'Evening'";
            }

            sql += " ORDER BY mc.Id DESC;";

            try
            {
                using SqlConnection con =
                    new SqlConnection(connectionString);

                using SqlCommand cmd =
                    new SqlCommand(sql, con);

                cmd.Parameters.Add(
                    "@Date",
                    SqlDbType.Date).Value = selectedDate;

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                adapter.Fill(dt);

                dgv.DataSource = null;

                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Grid loading error:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // LOAD FARMER DETAILS
        // =========================================

        private void LoadFarmerDetails(string farmerCode)
        {
            if (string.IsNullOrWhiteSpace(farmerCode))
                return;

            const string sql = @"
                SELECT TOP 1
                    mc.Quantity,
                    mc.Fat,
                    mc.SNF

                FROM dbo.MilkCollections mc

                INNER JOIN dbo.Farmers f
                    ON f.Id = mc.FarmerId

                WHERE f.FarmerCode = @Code

                ORDER BY mc.Id DESC;";

            try
            {
                using SqlConnection con =
                    new SqlConnection(connectionString);

                using SqlCommand cmd =
                    new SqlCommand(sql, con);

                cmd.Parameters.Add(
                    "@Code",
                    SqlDbType.NVarChar,
                    20).Value = farmerCode;

                con.Open();

                using SqlDataReader reader =
                    cmd.ExecuteReader();

                if (!reader.Read())
                {
                    lblWeight.Text = "0.00";

                    lblFat.Text = "0.0";

                    lblSNF.Text = "0.0";

                    MessageBox.Show(
                        "ఈ Farmer Code కు collection records లభించలేదు.");

                    return;
                }

                lblWeight.Text =
                    Convert.ToDecimal(
                        reader["Quantity"]).ToString("0.00");

                lblFat.Text =
                    Convert.ToDecimal(
                        reader["Fat"]).ToString("0.0");

                lblSNF.Text =
                    Convert.ToDecimal(
                        reader["SNF"]).ToString("0.0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Farmer details error:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // SAVE MILK COLLECTION
        // =========================================

         private void btnSave_Click(
            object? sender,
            EventArgs e)
        {
            string code = txtCardNo.Text.Trim();

            string milkType = cmbMilkType.Text;

            string shift = cmbShift.Text;

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show(
                    "Farmer Card Number నమోదు చేయండి.");

                txtCardNo.Focus();

                return;
            }

            if (!TryDecimal(
                    txtQuantity.Text,
                    out decimal qty) ||
                qty <= 0 ||
                !TryDecimal(
                    txtFat.Text,
                    out decimal fat) ||
                fat <= 0 || fat >= 100 ||
                !TryDecimal(
                    txtSNF.Text,
                    out decimal snf) ||
                snf <= 0 || snf >= 100)
            {
                MessageBox.Show(
                    "Quantity, Fat, SNF విలువలను సరిగా నమోదు చేయండి.");

                return;
            }

            fat = Math.Round(fat, 1);

            snf = Math.Round(snf, 1);

            decimal? rate;

            try
            {
                rate = GetMilkRate(
       milkType,
       fat,
       8.9m);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Rate Chart error:\n" + ex.Message);

                return;
            }

            if (rate == null)
            {
                MessageBox.Show(
                    "ఈ Milk Type, Fat, SNF కోసం active rate chart entry లేదు.\n" +
                    "Rate Chart లో సరైన rate నమోదు చేయండి.");

                return;
            }

            decimal amount =
                Math.Round(qty * rate.Value, 2);

            string dbSession =
                shift == "AM" ? "Morning" : "Evening";

            try
            {
                using SqlConnection con =
                    new SqlConnection(connectionString);

                con.Open();

                int farmerId;

                // FIND FARMER

                /*using (SqlCommand farmerCmd =
                    new SqlCommand(
                        @"SELECT Id
                          FROM dbo.Farmers
                          WHERE FarmerCode = @Code;",
                        con))
                {
                    farmerCmd.Parameters.Add(
                        "@Code",
                        SqlDbType.NVarChar,
                        20).Value = code;

                    object? result =
                        farmerCmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        MessageBox.Show(
                            "Farmer Card Number కనబడలేదు.");

                        return;
                    }

                    farmerId = Convert.ToInt32(result);
                }
                */
                //int farmerId;

                // FIND FARMER

                using (SqlCommand farmerCmd =
                    new SqlCommand(
                        @"SELECT Id
          FROM dbo.Farmers
          WHERE FarmerCode = @Code;",
                        con))
                {
                    farmerCmd.Parameters.Add(
                        "@Code",
                        SqlDbType.NVarChar,
                        20).Value = code;

                    object? result =
                        farmerCmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        MessageBox.Show(
                            "Farmer Card Number కనబడలేదు.");

                        return;
                    }

                    farmerId = Convert.ToInt32(result);
                }
                // INSERT COLLECTION

                const string insertSql = @"
                    INSERT INTO dbo.MilkCollections
                    (
                        FarmerId,
                        CollectionDate,
                        Session,
                        MilkType,
                        Quantity,
                        Fat,
                        SNF,
                        Rate,
                        CreatedAt
                    )
                    VALUES
                    (
                        @FarmerId,
                        @Date,
                        @Session,
                        @MilkType,
                        @Quantity,
                        @Fat,
                        @SNF,
                        @Rate,
                        @CreatedAt
                    );";

                using SqlCommand cmd =
                    new SqlCommand(insertSql, con);

                cmd.Parameters.Add(
                    "@FarmerId",
                    SqlDbType.Int).Value = farmerId;

                cmd.Parameters.Add(
                    "@Date",
                    SqlDbType.Date).Value = dtpDate.Value.Date;

                cmd.Parameters.Add(
                    "@Session",
                    SqlDbType.NVarChar,
                    20).Value = dbSession;

                cmd.Parameters.Add(
                    "@MilkType",
                    SqlDbType.NVarChar,
                    20).Value = milkType;

                AddDecimalParameter(
                    cmd, "@Quantity", 10, 2, qty);

                AddDecimalParameter(
                    cmd, "@Fat", 4, 1, fat);

                AddDecimalParameter(
                    cmd, "@SNF", 4, 1, snf);

                AddDecimalParameter(
                    cmd, "@Rate", 10, 2, rate.Value);

                cmd.Parameters.Add(
                    "@CreatedAt",
                    SqlDbType.DateTime2).Value = DateTime.Now;

                cmd.ExecuteNonQuery();

                // Prepare receipt data before printing
                // =========================================
                // PREPARE FARMER RECEIPT DATA
                // =========================================

                receiptFarmerCode = code;

                receiptFarmerName = farmerName;

                receiptVillage = farmerVillage;

                receiptDate =
                    dtpDate.Value.ToString("dd-MM-yyyy");

                receiptSession = shift;

                receiptMilkType = milkType;

                receiptQuantity = qty;

                receiptFat = fat;

                receiptSNF = snf;

                receiptRate = rate.Value;

                receiptAmount = amount;

                // =========================================
                // AUTOMATIC PRINT AFTER SUCCESSFUL SAVE
                // =========================================

                PrintFarmerReceipt();

                MessageBox.Show(
                    "Milk Collection Saved Successfully!\n\n" +
                    "Rate: ₹" + rate.Value.ToString("0.00") + "\n" +
                    "Amount: ₹" + amount.ToString("0.00"),
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // CLEAR INPUTS

                txtQuantity.Clear();

                txtFat.Clear();

                txtSNF.Clear();

                txtRate.Clear();

                txtAmount.Clear();

                // REFRESH GRID

                LoadInitialData();

                LoadFarmerDetails(code);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Save error:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // PRINT BUTTON
        // =========================================

        private void BtnPrint_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                using PrintPreviewDialog preview =
                    new PrintPreviewDialog();

                preview.Document = printDoc;

                preview.Width = 1000;

                preview.Height = 700;

                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Print error:\n" + ex.Message);
            }
        }

        // =========================================
        // PRINT SESSION REPORT
        // =========================================

        /* private void PrintDoc_PrintPage(
             object? sender,
             PrintPageEventArgs e)
         {
             Graphics? graphics = e.Graphics;

             if (graphics is null)
             {
                 e.HasMorePages = false;
                 return;
             }

             Graphics g = graphics;

             // --- Precise Font Matching (Arial Family) ---
             using Font titleFont = new Font("Arial", 12.5f, FontStyle.Bold);
             using Font subtitleFont = new Font("Arial", 9.5f, FontStyle.Bold);
             using Font regularFont = new Font("Arial", 9, FontStyle.Regular);
             using Font boldFont = new Font("Arial", 9, FontStyle.Bold);
             using Font amountFont = new Font("Arial", 12, FontStyle.Bold);

             using Brush brush = new SolidBrush(Color.Black);

             // Layout configuration constants
             int receiptWidth = 280;
             int y = 15;
             int xStart = 10;
             int colX = 75; // Adjusted horizontal shift position for perfectly aligned colons

             // Centered string helper function
             void DrawCenteredString(string text, Font font, int currentY)
             {
                 SizeF size = g.MeasureString(text, font);
                 float x = (receiptWidth - size.Width) / 2;
                 g.DrawString(text, font, brush, x, currentY);
             }

             // --- Header Section ---
             DrawCenteredString("ADVANCED MILK DAIRY", titleFont, y);
             y += 18;

             DrawCenteredString("MILK COLLECTION RECEIPT", subtitleFont, y);
             y += 16;

             // Solid double bar spacer line
             string doubleLine = new string('=', 44);
             DrawCenteredString(doubleLine, regularFont, y);
             y += 15;

             // --- Safe Dynamic Parsing (Using proper Row index) ---
             DataTable? sourceTable = dgv.DataSource as DataTable;
             DataTable dt = sourceTable ?? new DataTable();

             string receiptId = dt.Rows.Count > 0 ? (dt.Rows[0]["Id"]?.ToString() ?? "57") : "57";
             string dateStr = $"{dtpDate.Value:dd-MM-yyyy} ({cmbShift.Text})";
             string cardNo = dt.Rows.Count > 0 ? (dt.Rows[0]["FarmerCode"]?.ToString() ?? "103") : "103";
             string farmerName = dt.Rows.Count > 0 ? (dt.Rows[0]["FarmerName"]?.ToString() ?? "RAMSAI") : "RAMSAI";

             // --- Metadata Block (Aligned Colons) ---
             g.DrawString("Receipt ID", regularFont, brush, xStart, y);
             g.DrawString($": {receiptId}", regularFont, brush, colX, y);
             y += 16;

             g.DrawString("Date", regularFont, brush, xStart, y);
             g.DrawString($": {dateStr}", regularFont, brush, colX, y);
             y += 18;

             // Large Bold Card Identification Line
             g.DrawString("Card No", boldFont, brush, xStart, y);
             g.DrawString($": {cardNo}", boldFont, brush, colX, y);
             y += 16;

             g.DrawString($"Farmer Name: {farmerName}", regularFont, brush, xStart, y);
             y += 18;

             // Clean Dashed Divider Line
             string dashedLine = new string('-', 52);
             DrawCenteredString(dashedLine, regularFont, y);
             y += 14;

             // --- Safe Metrics Parsing (Using proper Row index) ---
             string milkType = dt.Rows.Count > 0 ? (dt.Rows[0]["MilkType"]?.ToString() ?? "Buffalo") : "Buffalo";

             double.TryParse(dt.Rows.Count > 0 ? dt.Rows[0]["Quantity"]?.ToString() : "3.50", out double quantity);
             double.TryParse(dt.Rows.Count > 0 ? dt.Rows[0]["Fat"]?.ToString() : "8.5", out double fat);
             double.TryParse(dt.Rows.Count > 0 ? dt.Rows[0]["SNF"]?.ToString() : "8.9", out double snf);
             double.TryParse(dt.Rows.Count > 0 ? dt.Rows[0]["Rate"]?.ToString() : "73.95", out double rate);
             double.TryParse(dt.Rows.Count > 0 ? dt.Rows[0]["TotalAmount"]?.ToString() : "258.83", out double totalAmount);

             // --- Metrics Block (Aligned Colons) ---
             g.DrawString("Milk Type", regularFont, brush, xStart, y);
             g.DrawString($": {milkType}", regularFont, brush, colX, y);
             y += 16;

             g.DrawString("Quantity", regularFont, brush, xStart, y);
             g.DrawString($": {quantity:F2} KG", regularFont, brush, colX, y);
             y += 16;

             g.DrawString("Fat %", regularFont, brush, xStart, y);
             g.DrawString($": {fat:F1}%", regularFont, brush, colX, y);
             y += 16;

             g.DrawString("SNF %", regularFont, brush, xStart, y);
             g.DrawString($": {snf:F1}%", regularFont, brush, colX, y);
             y += 16;

             g.DrawString("Rate", regularFont, brush, xStart, y);
             g.DrawString($": ₹ {rate:F2} /L", regularFont, brush, colX, y);
             y += 18;

             // Closing double bar section line
             DrawCenteredString(doubleLine, regularFont, y);
             y += 16;

             // --- Total Amount Block ---
             g.DrawString("TOTAL AMOUNT:", amountFont, brush, xStart, y);
             g.DrawString($"TOTAL AMOUNT: ₹ {totalAmount:F2}", amountFont, brush, xStart, y);
             y += 30;

             // Centered Footer Text
             DrawCenteredString("Thank You!", regularFont, y);

             e.HasMorePages = false;
         }*/
        // FIXED: Added '?' to object to resolve the CS8622 nullability mismatch warning

        private void btnPrint_Click(object? sender, EventArgs e)
        {
            if (dgv.Rows.Count == 0 || dgv.CurrentRow == null)
            {
                MessageBox.Show(
                    "Please select a collection record before printing!",
                    "Selection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dgv.CurrentRow;

            if (selectedRow.IsNewRow)
                return;

            // Read values using DataPropertyName
            string GetCellValue(string propertyName)
            {
                DataGridViewColumn? column = dgv.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(c =>
                        c.DataPropertyName == propertyName);

                if (column == null)
                    return "";

                return Convert.ToString(
                    selectedRow.Cells[column.Index].Value) ?? "";
            }

            string farmerCode = GetCellValue("FarmerCode");
            string quantity = GetCellValue("Quantity");

            if (string.IsNullOrWhiteSpace(farmerCode) ||
                string.IsNullOrWhiteSpace(quantity) ||
                !decimal.TryParse(quantity, out decimal qty) ||
                qty <= 0)
            {
                MessageBox.Show(
                    "Selected record does not contain valid data!",
                    "Print Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = printDoc;
                preview.ShowDialog();
            }
        }




        /*private void PrintDoc_PrintPage(object? sender, PrintPageEventArgs e)
        {
            Graphics? graphics = e.Graphics;
            if (graphics is null)
            {
                e.HasMorePages = false;
                return;
            }
            Graphics g = graphics;

            // --- Precise Font Matching (Arial Family) ---
            using Font titleFont = new Font("Arial", 12.5f, FontStyle.Bold);
            using Font subtitleFont = new Font("Arial", 9.5f, FontStyle.Bold);
            using Font regularFont = new Font("Arial", 9, FontStyle.Regular);
            using Font boldFont = new Font("Arial", 9, FontStyle.Bold);
            using Font amountFont = new Font("Arial", 12, FontStyle.Bold);
            using Brush brush = new SolidBrush(Color.Black);

            // Layout configuration constants
            int receiptWidth = 280;
            int y = 15;
            int xStart = 10;
            int colX = 75;

            // Centered string helper function
            void DrawCenteredString(string text, Font font, int currentY)
            {
                SizeF size = g.MeasureString(text, font);
                float x = (receiptWidth - size.Width) / 2;
                g.DrawString(text, font, brush, x, currentY);
            }

            // --- Header Section ---
            DrawCenteredString("ADVANCED MILK DAIRY", titleFont, y);
            y += 18;
            DrawCenteredString("MILK COLLECTION RECEIPT", subtitleFont, y);
            y += 16;

            // Solid double bar spacer line
            string doubleLine = new string('=', 44);
            DrawCenteredString(doubleLine, regularFont, y);
            y += 15;

            // --- Safe Dynamic Parsing (Double-check count array bounds safely) ---
            DataTable dt = (dgv.DataSource as DataTable) ?? new DataTable();

            if (dt.Rows.Count == 0)
            {
                e.HasMorePages = false;
                return;
            }

            // FIXED: Corrected syntax to look explicitly at Row Index [0]
            string receiptId = dt.Rows[0]["Id"]?.ToString() ?? "";
            string dateStr = $"{dtpDate.Value:dd-MM-yyyy} ({cmbShift.Text})";
            //string cardNo = dt.Rows[0]["FarmerCode"]?.ToString() ?? "";
            // string farmerName = dt.Rows[0]["FarmerName"]?.ToString() ?? "";
            // Farmer details from saved collection
            string cardNo = receiptFarmerCode;
            string farmerName = receiptFarmerName;
            string village = receiptVillage;

            // --- Metadata Block (Aligned Colons) ---
            g.DrawString("Receipt ID", regularFont, brush, xStart, y);
            g.DrawString($": {receiptId}", regularFont, brush, colX, y);
            y += 16;

            g.DrawString("Date", regularFont, brush, xStart, y);
            g.DrawString($": {dateStr}", regularFont, brush, colX, y);
            y += 18;

            g.DrawString("Card No", boldFont, brush, xStart, y);
            g.DrawString($": {cardNo}", boldFont, brush, colX, y);
            y += 16;

            g.DrawString($"Farmer Name: {farmerName}", regularFont, brush, xStart, y);
            y += 18;

            // Clean Dashed Divider Line
            string dashedLine = new string('-', 52);
            DrawCenteredString(dashedLine, regularFont, y);
            y += 14;

            // --- Metrics Parsing (FIXED: Row index [0] added explicitly) ---
            string milkType = dt.Rows[0]["MilkType"]?.ToString() ?? "";
            double.TryParse(dt.Rows[0]["Quantity"]?.ToString(), out double quantity);
            double.TryParse(dt.Rows[0]["Fat"]?.ToString(), out double fat);
            double.TryParse(dt.Rows[0]["SNF"]?.ToString(), out double snf);
            double.TryParse(dt.Rows[0]["Rate"]?.ToString(), out double rate);
            double.TryParse(dt.Rows[0]["TotalAmount"]?.ToString(), out double totalAmount);

            // --- Metrics Block (Aligned Colons) ---
            g.DrawString("Milk Type", regularFont, brush, xStart, y);
            g.DrawString($": {milkType}", regularFont, brush, colX, y);
            y += 16;

            g.DrawString("Quantity", regularFont, brush, xStart, y);
            g.DrawString($": {quantity:F2} KG", regularFont, brush, colX, y);
            y += 16;

            g.DrawString("Fat %", regularFont, brush, xStart, y);
            g.DrawString($": {fat:F1}%", regularFont, brush, colX, y);
            y += 16;

            g.DrawString("SNF %", regularFont, brush, xStart, y);
            g.DrawString($": {snf:F1}%", regularFont, brush, colX, y);
            y += 16;

            g.DrawString("Rate", regularFont, brush, xStart, y);
            g.DrawString($": ₹ {rate:F2} /L", regularFont, brush, colX, y);
            y += 18;

            // Closing double bar section line
            DrawCenteredString(doubleLine, regularFont, y);
            y += 16;

            // --- Total Amount Block ---
            g.DrawString($"TOTAL AMOUNT: ₹ {totalAmount:F2}", amountFont, brush, xStart, y);
            y += 30;

            // Centered Footer Text
            DrawCenteredString("Thank You!", regularFont, y);

            e.HasMorePages = false;
        }
*/

        private void PrintDoc_PrintPage(object? sender, PrintPageEventArgs e)
        {
            Graphics? graphics = e.Graphics;

            if (graphics is null)
            {
                e.HasMorePages = false;
                return;
            }

            Graphics g = graphics;

            // --- Existing Font Design ---
            using Font titleFont = new Font("Arial", 12.5f, FontStyle.Bold);
            using Font subtitleFont = new Font("Arial", 9.5f, FontStyle.Bold);
            using Font regularFont = new Font("Arial", 9, FontStyle.Regular);
            using Font boldFont = new Font("Arial", 9, FontStyle.Bold);
            using Font amountFont = new Font("Arial", 12, FontStyle.Bold);
            using Brush brush = new SolidBrush(Color.Black);

            // Layout configuration
            int receiptWidth = 280;
            int y = 15;
            int xStart = 10;
            int colX = 75;

            // Centered string helper
            void DrawCenteredString(string text, Font font, int currentY)
            {
                SizeF size = g.MeasureString(text, font);
                float x = (receiptWidth - size.Width) / 2;

                g.DrawString(text, font, brush, x, currentY);
            }

            // =========================================
            // READ SAVED RECEIPT VARIABLES
            // =========================================

            string dateStr =
                receiptDate + " (" + receiptSession + ")";

            string cardNo = receiptFarmerCode;
            string farmerName = receiptFarmerName;
            string village = receiptVillage;

            string milkType = receiptMilkType;

            decimal quantity = receiptQuantity;
            decimal fat = receiptFat;
            decimal snf = receiptSNF;
            decimal rate = receiptRate;
            decimal totalAmount = receiptAmount;

            // =========================================
            // HEADER SECTION
            // =========================================

            DrawCenteredString(
                "ADVANCED MILK DAIRY",
                titleFont, y);

            y += 18;

            DrawCenteredString(
                "MILK COLLECTION RECEIPT",
                subtitleFont, y);

            y += 16;

            string doubleLine = new string('=', 44);

            DrawCenteredString(doubleLine, regularFont, y);

            y += 15;


            // =========================================
            // FARMER DETAILS - ALIGNED RECEIPT
            // =========================================

            // Date
            g.DrawString(
                "Date",
                regularFont,
                brush,
                xStart,
                y);

            g.DrawString(
                ": " + dateStr,
                regularFont,
                brush,
                colX,
                y);

            y += 20;


            // Card Number
            g.DrawString(
                "Card No",
                boldFont,
                brush,
                xStart,
                y);

            g.DrawString(
                ": " + cardNo,
                boldFont,
                brush,
                colX,
                y);

            y += 20;

            g.DrawString(
     "Farmer Name",
     regularFont,
     brush,
     xStart,
     y);

            g.DrawString(
                ": " + this.farmerName,
                regularFont,
                brush,
                xStart + 85,
                y);

            y += 25;


            // Village
            g.DrawString(
                "Village",
                regularFont,
                brush,
                xStart,
                y);

            g.DrawString(
                ": " + this.village,
                regularFont,
                brush,
                colX,
                y);

            y += 25;


            // =========================================
            // DIVIDER
            // =========================================

            string dashedLine = new string('-', 52);

            DrawCenteredString(dashedLine, regularFont, y);

            y += 14;

            // =========================================
            // MILK COLLECTION DETAILS
            // =========================================

            g.DrawString("Milk Type", regularFont, brush, xStart, y);
            g.DrawString(": " + milkType, regularFont, brush, colX, y);

            y += 16;

            g.DrawString("Quantity", regularFont, brush, xStart, y);
            g.DrawString(
                ": " + quantity.ToString("F2") + " KG",
                regularFont, brush, colX, y);

            y += 16;

            g.DrawString("Fat %", regularFont, brush, xStart, y);
            g.DrawString(
                ": " + fat.ToString("F1") + "%",
                regularFont, brush, colX, y);

            y += 16;

            g.DrawString("SNF %", regularFont, brush, xStart, y);
            g.DrawString(
                ": " + snf.ToString("F1") + "%",
                regularFont, brush, colX, y);

            y += 16;

            g.DrawString("Rate", regularFont, brush, xStart, y);
            g.DrawString(
                ": ₹ " + rate.ToString("F2") + " /L",
                regularFont, brush, colX, y);

            y += 18;

            // =========================================
            // TOTAL AMOUNT
            // =========================================

            DrawCenteredString(
                doubleLine, regularFont, y);

            y += 16;

            g.DrawString(
                "TOTAL AMOUNT: ₹ " + totalAmount.ToString("F2"),
                amountFont,
                brush,
                xStart,
                y);

            y += 30;

            // =========================================
            // FOOTER
            // =========================================

            DrawCenteredString(
                "Thank You!",
                regularFont,
                y);

            e.HasMorePages = false;
        }


        private void ShowSessionSummaryPrintPreview(
            DateTime collectionDate,
            string session,
            int totalTransactions,
            decimal totalLitres,
            decimal averageFat,
            decimal averageSNF,
            decimal totalAmount)
        {
            using (PrintDocument doc = new PrintDocument())
            {
                doc.DocumentName = "Dairy Session Summary";

                // Paper settings
                doc.DefaultPageSettings.Landscape = false;
                doc.DefaultPageSettings.Margins =
                    new Margins(50, 50, 50, 50);

                doc.PrintPage += (sender, e) =>
                {
                    Graphics? g = e.Graphics;

                    if (g == null)
                    {
                        e.HasMorePages = false;
                        return;
                    }

                    using Font titleFont =
                        new Font("Arial", 16, FontStyle.Bold);

                    using Font subtitleFont =
                        new Font("Arial", 11, FontStyle.Bold);

                    using Font headingFont =
                        new Font("Arial", 10, FontStyle.Bold);

                    using Font regularFont =
                        new Font("Arial", 10, FontStyle.Regular);

                    using Font totalFont =
                        new Font("Arial", 12, FontStyle.Bold);

                    using Brush brush = new SolidBrush(Color.Black);

                    int y = 60;
                    int left = 60;
                    int right = 500;

                    void DrawCentered(string text, Font font)
                    {
                        SizeF size = g.MeasureString(text, font);

                        g.DrawString(
                            text,
                            font,
                            brush,
                            (e.PageBounds.Width - size.Width) / 2,
                            y);
                    }

                    // Header
                    DrawCentered("ADVANCED MILK DAIRY", titleFont);
                    y += 30;

                    DrawCentered("END OF SESSION SUMMARY", subtitleFont);
                    y += 35;

                    g.DrawLine(Pens.Black, left, y, right, y);
                    y += 25;

                    // Session information
                    void DrawRow(string label, string value, Font font)
                    {
                        g.DrawString(label, font, brush, left, y);
                        g.DrawString(":", font, brush, left + 170, y);
                        g.DrawString(value, font, brush, left + 190, y);

                        y += 30;
                    }

                    DrawRow(
                        "Collection Date",
                        collectionDate.ToString("dd-MM-yyyy"),
                        regularFont);

                    DrawRow(
                        "Session",
                        session,
                        regularFont);

                    y += 10;

                    g.DrawLine(Pens.Gray, left, y, right, y);
                    y += 25;

                    // Collection details
                    DrawRow(
                        "Total Transactions",
                        totalTransactions.ToString(),
                        regularFont);

                    DrawRow(
                        "Total Milk",
                        totalLitres.ToString("N2") + " Litres",
                        regularFont);

                    DrawRow(
                        "Average Fat",
                        averageFat.ToString("N2") + " %",
                        regularFont);

                    DrawRow(
                        "Average SNF",
                        averageSNF.ToString("N2") + " %",
                        regularFont);

                    y += 10;

                    g.DrawLine(Pens.Black, left, y, right, y);
                    y += 25;

                    // Total amount
                    g.DrawString(
                        "TOTAL AMOUNT",
                        totalFont,
                        brush,
                        left,
                        y);

                    g.DrawString(
                        "₹ " + totalAmount.ToString("N2"),
                        totalFont,
                        brush,
                        left + 190,
                        y);

                    y += 50;

                    g.DrawLine(Pens.Black, left, y, right, y);
                    y += 30;

                    DrawCentered("SESSION COMPLETED", headingFont);
                    y += 35;

                    DrawCentered("Thank You!", regularFont);

                    e.HasMorePages = false;
                };

                // Open print preview
                using (PrintPreviewDialog preview =
                    new PrintPreviewDialog())
                {
                    preview.Document = doc;
                    preview.Width = 900;
                    preview.Height = 700;

                    preview.ShowDialog(this);
                }
            }
        }

        private void btnEndSessionSummary_Click(object? sender, EventArgs e)
        {
            DateTime selectedDate = dtpDate.Value.Date;

            string selectedShift = cmbShift.Text.Trim();

            if (string.IsNullOrWhiteSpace(selectedShift))
            {
                MessageBox.Show("Please select AM or PM session.");
                return;
            }

            // Support both AM/PM and Morning/Evening database values.
            string session = selectedShift.ToUpperInvariant();

            if (session == "AM" || session == "MORNING")
                session = "AM";
            else if (session == "PM" || session == "EVENING")
                session = "PM";
            else
            {
                MessageBox.Show("Please select a valid AM or PM session.");
                return;
            }

            string query = @"
        SELECT
            COUNT(*) AS TotalTransactions,

            ISNULL(SUM(Quantity), 0) AS TotalLitres,

            CASE
                WHEN SUM(Quantity) > 0
                THEN SUM(Quantity * ISNULL(Fat, 0))
                     / SUM(Quantity)
                ELSE 0
            END AS AverageFat,

            CASE
                WHEN SUM(Quantity) > 0
                THEN SUM(Quantity * ISNULL(SNF, 0))
                     / SUM(Quantity)
                ELSE 0
            END AS AverageSNF,

            ISNULL(SUM(TotalAmount), 0) AS TotalAmount

        FROM dbo.MilkCollections

        WHERE CollectionDate = @CollectionDate
          AND (
                (@Session = 'AM'
                    AND [Session] IN ('AM', 'Morning'))
                OR
                (@Session = 'PM'
                    AND [Session] IN ('PM', 'Evening'))
              );";

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@CollectionDate", SqlDbType.Date).Value =
                            selectedDate;

                        cmd.Parameters.Add(
                            "@Session", SqlDbType.VarChar, 10).Value =
                            session;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int totalTransactions =
                                    Convert.ToInt32(reader["TotalTransactions"]);

                                decimal totalLitres =
                                    Convert.ToDecimal(reader["TotalLitres"]);

                                decimal averageFat =
                                    Convert.ToDecimal(reader["AverageFat"]);

                                decimal averageSNF =
                                    Convert.ToDecimal(reader["AverageSNF"]);

                                decimal totalAmount =
                                    Convert.ToDecimal(reader["TotalAmount"]);

                                if (totalTransactions == 0)
                                {
                                    MessageBox.Show(
                                        "No milk collection records found for this session.",
                                        "Session Summary",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);

                                    return;
                                }

                                string summary =
                                    "ADVANCED MILK DAIRY\n" +
                                    "END OF SESSION SUMMARY\n" +
                                    "--------------------------------\n\n" +

                                    "Collection Date : " +
                                    selectedDate.ToString("dd-MM-yyyy") + "\n\n" +

                                    "Session          : " + session + "\n\n" +

                                    "Total Transactions : " +
                                    totalTransactions + "\n\n" +

                                    "Total Milk       : " +
                                    totalLitres.ToString("N2") + " Litres\n\n" +

                                    "Average Fat      : " +
                                    averageFat.ToString("N2") + " %\n\n" +

                                    "Average SNF      : " +
                                    averageSNF.ToString("N2") + " %\n\n" +

                                    "Total Amount     : ₹ " +
                                    totalAmount.ToString("N2") + "\n\n" +

                                    "--------------------------------\n" +
                                    "SESSION COMPLETED";

                                ShowSessionSummaryPrintPreview(
    selectedDate,
    session,
    totalTransactions,
    totalLitres,
    averageFat,
    averageSNF,
    totalAmount);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to calculate session summary.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
private void LoadRateFromDatabase()
        {
            string milkType = cmbMilkType.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(milkType))
                return;

            if (!decimal.TryParse(
                txtFat.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal fat))
            {
                txtRate.Clear();
                return;
            }

            fat = Math.Round(fat, 1, MidpointRounding.AwayFromZero);

            if (!decimal.TryParse(
                    txtSNF.Text.Trim(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal snfValue))
            {
                txtRate.Clear();
                txtAmount.Clear();
                return;
            }

            if (snfValue < 8.5m)
            {
                txtRate.Clear();
                txtAmount.Clear();
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT TOP (1) RatePerLitre 
                             FROM MilkRateChart
                             WHERE MilkType = @MilkType 
                               AND FatPercent = @FatPercent 
                               AND SNFPercent = 8.9
                               AND IsActive = 1
                             ORDER BY Id DESC;";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@MilkType", SqlDbType.NVarChar, 20).Value = milkType;

                        SqlParameter p = cmd.Parameters.Add("@FatPercent", SqlDbType.Decimal);
                        p.Precision = 4;
                        p.Scale = 1;
                        p.Value = fat;

                        con.Open();
                        object? result = cmd.ExecuteScalar();

                        // ========================================================
                        // ડાયગ્નોસ્ટિક ચેક (આ તમને ભૂલ બતાવશે)
                        // ========================================================
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show($"ડેટાબેઝમાં આ ડેટાનો કોઈ રેટ નથી મળ્યો:\n" +
                                            $"- MilkType: {milkType}\n" +
                                            $"- FatPercent: {fat}\n" +
                                            $"- SNFPercent: 8.9\n\n" +
                                            $"કૃપા કરીને તમારા MilkRateChart ટેબલમાં ચેક કરો કે આ Fat અને 8.9 SNF નો રેટ એન્ટર કરેલો છે કે નહીં.");

                            txtRate.Clear();
                            txtAmount.Clear();
                            return;
                        }

                        decimal rate = Convert.ToDecimal(result);
                        txtRate.Text = rate.ToString("0.00", CultureInfo.InvariantCulture);

                        if (decimal.TryParse(
                            txtQuantity.Text.Trim(),
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out decimal quantity))
                        {
                            decimal amount = quantity * rate;
                            txtAmount.Text = amount.ToString("0.00", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            txtAmount.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rate Loading Error: " + ex.Message);
            }
        } 
        // =========================================
        // AUTOMATIC FARMER RECEIPT PRINTING
        // =========================================



        private void PrintFarmerReceipt()
        {
            try
            {
                // Reset farmer details
                farmerName = "";
                village = "";

                // Get Card Number
                string currentCardNo = txtCardNo.Text.Trim();

                if (string.IsNullOrWhiteSpace(currentCardNo))
                {
                    MessageBox.Show(
                        "Please enter Card Number.",
                        "Missing Card Number",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtCardNo.Focus();
                    return;
                }

                // SQL Query
                string query = @"
            SELECT FarmerName, Village
            FROM dbo.Farmers
            WHERE LTRIM(RTRIM(FarmerCode)) = @FarmerCode";

                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@FarmerCode",
                            SqlDbType.NVarChar,
                            20).Value = currentCardNo;

                        con.Open();

                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                farmerName =
                                    reader["FarmerName"] == DBNull.Value
                                    ? ""
                                    : Convert.ToString(
                                        reader["FarmerName"]) ?? "";

                                village =
                                    reader["Village"] == DBNull.Value
                                    ? ""
                                    : Convert.ToString(
                                        reader["Village"]) ?? "";
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Farmer details not found for Card No: "
                                    + currentCardNo,
                                    "Farmer Not Found",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                                return;
                            }
                        }

                    }
                }

                // Check farmer details
                if (string.IsNullOrWhiteSpace(farmerName))
                {
                    MessageBox.Show(
                        "Farmer Name is empty in database.",
                        "Missing Farmer Name",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (string.IsNullOrWhiteSpace(village))
                {
                    MessageBox.Show(
                        "Village is empty in database.",
                        "Missing Village",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
                MessageBox.Show(
    "Card No: " + currentCardNo +
    "\nFarmer Name: " + farmerName +
    "\nVillage: " + village,
    "Farmer Details Check");
                // Print Receipt
                printDoc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Receipt Printing Error:\n" + ex.Message,
                    "Printing Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
       


    }

}
