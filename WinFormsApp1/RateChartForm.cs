
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class RateChartForm : Form
    {
        string connectionString =
        @"Server=.\SQLEXPRESS;Database=dairy;Integrated Security=True;TrustServerCertificate=True;";

        ComboBox cmbMilkType = new ComboBox();
        NumericUpDown numBaseRate = new NumericUpDown();
        DataGridView dgv = new DataGridView();
        Label lblStatus = new Label();
        private Button btnClose = new Button();

        DataTable rateTable = new DataTable();

        public RateChartForm()
        {
            CreateControls();
            GenerateChart();
            LoadSavedChart();
        }

        private void CreateControls()
        {
            Text = "Milk Rate Chart";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;

            Label title = new Label();
            title.Text = "MILK RATE CHART";
            title.Location = new Point(300, 20);
            title.Size = new Size(300, 40);
            title.Font = new Font("Arial", 18, FontStyle.Bold);
            Controls.Add(title);

            Label milkLabel = new Label();
            milkLabel.Text = "Milk Type";
            milkLabel.Location = new Point(40, 90);
            milkLabel.Size = new Size(100, 30);
            Controls.Add(milkLabel);

            cmbMilkType.Location = new Point(150, 90);
            cmbMilkType.Size = new Size(150, 30);
            cmbMilkType.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbMilkType.Items.AddRange(
                new object[] { "Cow", "Buffalo" });

            cmbMilkType.SelectedIndex = 0;
            cmbMilkType.SelectedIndexChanged +=
                (s, e) => LoadSavedChart();

            Controls.Add(cmbMilkType);

            Label rateLabel = new Label();
            rateLabel.Text = "10% Fat Rate";
            rateLabel.Location = new Point(350, 90);
            rateLabel.Size = new Size(110, 30);
            Controls.Add(rateLabel);

            numBaseRate.Location = new Point(470, 90);
            numBaseRate.Size = new Size(120, 30);
            numBaseRate.DecimalPlaces = 2;
            numBaseRate.Maximum = 1000;
            numBaseRate.Value = 87;
            Controls.Add(numBaseRate);

            Button btnGenerate = new Button();
            btnGenerate.Text = "Generate Chart";
            btnGenerate.Location = new Point(40, 145);
            btnGenerate.Size = new Size(150, 40);
            btnGenerate.Click += (s, e) => GenerateChart();
            Controls.Add(btnGenerate);

            Button btnSave = new Button();
            btnSave.Text = "Save Chart";
            btnSave.Location = new Point(210, 145);
            btnSave.Size = new Size(130, 40);
            btnSave.Click += (s, e) => SaveChart();
            Controls.Add(btnSave);

            Button btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(360, 145);
            btnRefresh.Size = new Size(120, 40);
            btnRefresh.Click += (s, e) => LoadSavedChart();
            Controls.Add(btnRefresh);

            dgv.Location = new Point(40, 210);
            dgv.Size = new Size(800, 290);
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            Controls.Add(dgv);

            lblStatus.Location = new Point(40, 520);
            lblStatus.Size = new Size(800, 30);
            // CLOSE BUTTON
            btnClose.Text = "Close";
            btnClose.Location = new Point(780, 20);
            btnClose.Size = new Size(100, 35);
            btnClose.BackColor = Color.DarkRed;
            btnClose.ForeColor = Color.White;
            btnClose.Font = new Font("Arial", 10, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;

            btnClose.Click += (s, e) =>
            {
                this.Close();
            };

            Controls.Add(btnClose);
            Controls.Add(lblStatus);
        }

        private void GenerateChart()
        {
            rateTable = new DataTable();

            rateTable.Columns.Add("MilkType", typeof(string));
            rateTable.Columns.Add("FatPercent", typeof(decimal));
            rateTable.Columns.Add("SNFPercent", typeof(decimal));
            rateTable.Columns.Add("RatePerLitre", typeof(decimal));

            string milk =
                cmbMilkType.SelectedItem?.ToString() ?? "Cow";

            decimal baseRate = numBaseRate.Value;

            for (decimal fat = 5.0m; fat <= 10.0m; fat += 0.1m)
            {
                decimal rate = Math.Round(
                    fat * baseRate / 10m, 2);

                rateTable.Rows.Add(
                    milk, fat, 8.9m, rate);
            }

            dgv.DataSource = rateTable;

            lblStatus.Text = "Chart Generated Successfully";
        }


        private void SaveChart()
        {
            if (rateTable.Rows.Count == 0)
            {
                MessageBox.Show("Generate Chart First");
                return;
            }

            string milk =
                cmbMilkType.SelectedItem?.ToString() ?? "Cow";

            DialogResult answer = MessageBox.Show(
                "Update/Save " + milk + " Rate Chart?",
                "Confirm Save",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlTransaction trans = con.BeginTransaction())
                    {
                        try
                        {
                            // Check whether Id is Identity

                            bool idIsIdentity;

                            using (SqlCommand checkCmd =
                                new SqlCommand(
                                    @"SELECT COLUMNPROPERTY(
                                OBJECT_ID('dbo.MilkRateChart'),
                                'Id', 'IsIdentity');",
                                    con, trans))
                            {
                                idIsIdentity = Convert.ToInt32(
                                    checkCmd.ExecuteScalar()) == 1;
                            }

                            // Deactivate previous matching rates

                            string deactivateQuery = @"
                        UPDATE dbo.MilkRateChart
                        SET IsActive = 0
                        WHERE MilkType = @Milk
                        AND SNFPercent = 8.9
                        AND IsActive = 1;";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    deactivateQuery, con, trans))
                            {
                                cmd.Parameters.Add(
                                    "@Milk", SqlDbType.NVarChar, 20)
                                    .Value = milk;

                                cmd.ExecuteNonQuery();
                            }

                            // Update existing record or Insert new record

                            foreach (DataRow row in rateTable.Rows)
                            {
                                string query;

                                if (idIsIdentity)
                                {
                                    query = @"
                                IF EXISTS
                                (
                                    SELECT 1
                                    FROM dbo.MilkRateChart
                                    WHERE MilkType = @Milk
                                    AND FatPercent = @Fat
                                    AND SNFPercent = @SNF
                                )
                                BEGIN
                                    UPDATE dbo.MilkRateChart
                                    SET RatePerLitre = @Rate,
                                        IsActive = 1
                                    WHERE MilkType = @Milk
                                    AND FatPercent = @Fat
                                    AND SNFPercent = @SNF;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO dbo.MilkRateChart
                                    (
                                        MilkType,
                                        FatPercent,
                                        SNFPercent,
                                        RatePerLitre,
                                        IsActive
                                    )
                                    VALUES
                                    (
                                        @Milk,
                                        @Fat,
                                        @SNF,
                                        @Rate,
                                        1
                                    );
                                END";
                                }
                                else
                                {
                                    query = @"
                                IF EXISTS
                                (
                                    SELECT 1
                                    FROM dbo.MilkRateChart
                                    WHERE MilkType = @Milk
                                    AND FatPercent = @Fat
                                    AND SNFPercent = @SNF
                                )
                                BEGIN
                                    UPDATE dbo.MilkRateChart
                                    SET RatePerLitre = @Rate,
                                        IsActive = 1
                                    WHERE MilkType = @Milk
                                    AND FatPercent = @Fat
                                    AND SNFPercent = @SNF;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO dbo.MilkRateChart
                                    (
                                        Id,
                                        MilkType,
                                        FatPercent,
                                        SNFPercent,
                                        RatePerLitre,
                                        IsActive
                                    )
                                    SELECT
                                        ISNULL(MAX(Id), 0) + 1,
                                        @Milk,
                                        @Fat,
                                        @SNF,
                                        @Rate,
                                        1
                                    FROM dbo.MilkRateChart
                                    WITH (UPDLOCK, HOLDLOCK);
                                END";
                                }

                                using (SqlCommand cmd =
                                    new SqlCommand(query, con, trans))
                                {
                                    cmd.Parameters.Add(
                                        "@Milk", SqlDbType.NVarChar, 20)
                                        .Value = milk;

                                    SqlParameter pFat =
                                        cmd.Parameters.Add(
                                            "@Fat", SqlDbType.Decimal);

                                    pFat.Precision = 4;
                                    pFat.Scale = 1;
                                    pFat.Value = row["FatPercent"];

                                    SqlParameter pSnf =
                                        cmd.Parameters.Add(
                                            "@SNF", SqlDbType.Decimal);

                                    pSnf.Precision = 4;
                                    pSnf.Scale = 1;
                                    pSnf.Value = row["SNFPercent"];

                                    SqlParameter pRate =
                                        cmd.Parameters.Add(
                                            "@Rate", SqlDbType.Decimal);

                                    pRate.Precision = 10;
                                    pRate.Scale = 2;
                                    pRate.Value = row["RatePerLitre"];

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }

                MessageBox.Show(
                    milk + " Rate Chart Updated Successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadSavedChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Rate Chart Error:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void LoadSavedChart()
        {
            try
            {
                string milk =
                    cmbMilkType.SelectedItem?.ToString() ?? "Cow";

                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT
                    Id,
                    MilkType,
                    FatPercent,
                    SNFPercent,
                    RatePerLitre
                FROM dbo.MilkRateChart
                WHERE MilkType = @Milk
                AND SNFPercent = @SNF
                AND IsActive = 1
                ORDER BY FatPercent;";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@Milk", SqlDbType.NVarChar, 20)
                            .Value = milk;

                        SqlParameter snfParam =
                            cmd.Parameters.Add(
                                "@SNF", SqlDbType.Decimal);

                        snfParam.Precision = 4;
                        snfParam.Scale = 1;
                        snfParam.Value = 8.9m;

                        DataTable dt = new DataTable();

                        using (SqlDataAdapter adapter =
                            new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }

                        dgv.DataSource = dt;

                        lblStatus.Text =
                            dt.Rows.Count +
                            " Saved Records - " + milk;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Load Error: " + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
