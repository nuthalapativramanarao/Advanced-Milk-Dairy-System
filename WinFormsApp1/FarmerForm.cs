
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class FarmerForm : Form
    {
        // IMPORTANT:
        // Replace the server name with your actual SQL Server instance.
        private readonly string connectionString =
            @"Server=.\SQLEXPRESS;Database=dairy;Integrated Security=True;TrustServerCertificate=True;";

        private TextBox txtFarmerCode = new();
        private TextBox txtFarmerName = new();
        private TextBox txtPhone = new();
        private TextBox txtVillage = new();
        private TextBox txtSearch = new();
        private Button btnClose = new();

        private Button btnAdd = new();
        private Button btnUpdate = new();
        private Button btnDelete = new();
        private Button btnClear = new();
        private Button btnSearch = new();
        private Button btnRefresh = new();

        private DataGridView dgvFarmers = new();
        private int selectedFarmerId = 0;

        public FarmerForm()
        {
            BuildForm();
            LoadFarmers();
        }

        private void BuildForm()
        {
            Text = "Advanced Milk Dairy - Farmer Master";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1050, 700);
            MinimumSize = new Size(900, 600);
            BackColor = Color.WhiteSmoke;
            Font = new Font("Segoe UI", 10);

            Label title = new Label
            {
                Text = "FARMER MASTER",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location = new Point(25, 20),
                AutoSize = true
            };
            Controls.Add(title);

            AddLabel("Farmer Code", 30, 85);
            txtFarmerCode = AddTextBox(160, 82);

            AddLabel("Farmer Name", 30, 130);
            txtFarmerName = AddTextBox(160, 127);

            AddLabel("Phone", 30, 175);
            txtPhone = AddTextBox(160, 172);
            txtPhone.MaxLength = 15;

            AddLabel("Village", 30, 220);
            txtVillage = AddTextBox(160, 217);

            btnAdd = AddButton("Add Farmer", 400, 82);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = AddButton("Update", 400, 127);
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = AddButton("Delete", 400, 172);
            btnDelete.Click += BtnDelete_Click;

            btnClear = AddButton("Clear", 400, 217);
            btnClear.Click += (s, e) => ClearFields();

            AddLabel("Search Code / Name", 30, 285);
            txtSearch = AddTextBox(200, 282);
            txtSearch.Width = 220;

            btnSearch = AddButton("Search", 440, 280);
            btnSearch.Click += (s, e) => LoadFarmers(txtSearch.Text.Trim());

            btnRefresh = AddButton("Refresh All", 560, 280);
            btnRefresh.Click += (s, e) =>
            {
                txtSearch.Clear();
                LoadFarmers();
            };

            // Close Button
            btnClose = AddButton("Close", 700, 280);

            btnClose.BackColor = Color.DarkRed;
            btnClose.Click += (s, e) =>
            {
                this.Close();
            };

            dgvFarmers = new DataGridView
            {
                Location = new Point(30, 335),
                Size = new Size(960, 290),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                         | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White
            };

            dgvFarmers.CellClick += DgvFarmers_CellClick;
            Controls.Add(dgvFarmers);
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private TextBox AddTextBox(int x, int y)
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(210, 28)
            };

            Controls.Add(txt);
            return txt;
        }

        private Button AddButton(string text, int x, int y)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(125, 35),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Controls.Add(btn);
            return btn;
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }

        private void LoadFarmers(string search = "")
        {
            try
            {
                using SqlConnection con = CreateConnection();

                string sql = @"
                    SELECT Id, FarmerCode, FarmerName, Phone, Village
                    FROM dbo.Farmers
                    WHERE FarmerCode LIKE @Search
                       OR FarmerName LIKE @Search
                    ORDER BY Id DESC;";

                using SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100)
                    .Value = "%" + search + "%";

                using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dgvFarmers.DataSource = table;

                if (dgvFarmers.Columns["Id"] is DataGridViewColumn idColumn)
                {
                    idColumn.Visible = false;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Database error:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFarmerCode.Text))
            {
                MessageBox.Show("Enter Farmer Code.");
                txtFarmerCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFarmerName.Text))
            {
                MessageBox.Show("Enter Farmer Name.");
                txtFarmerName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                foreach (char c in txtPhone.Text)
                {
                    if (!char.IsDigit(c) && c != '+' &&
                        c != '-' && c != ' ')
                    {
                        MessageBox.Show("Enter a valid phone number.");
                        txtPhone.Focus();
                        return false;
                    }
                }
            }

            return true;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using SqlConnection con = CreateConnection();
                con.Open();

                string sql = @"
                    INSERT INTO dbo.Farmers
                    (FarmerCode, FarmerName, Phone, Village)
                    VALUES
                    (@Code, @Name, @Phone, @Village);";

                using SqlCommand cmd = new SqlCommand(sql, con);
                AddFarmerParameters(cmd);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Farmer added successfully!");
                ClearFields();
                LoadFarmers();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("Farmer Code already exists.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to add farmer:\n" + ex.Message);
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedFarmerId == 0)
            {
                MessageBox.Show("Select a farmer from the table first.");
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                using SqlConnection con = CreateConnection();
                con.Open();

                string sql = @"
                    UPDATE dbo.Farmers
                    SET FarmerCode = @Code,
                        FarmerName = @Name,
                        Phone = @Phone,
                        Village = @Village
                    WHERE Id = @Id;";

                using SqlCommand cmd = new SqlCommand(sql, con);
                AddFarmerParameters(cmd);
                cmd.Parameters.Add("@Id", SqlDbType.Int)
                    .Value = selectedFarmerId;

                cmd.ExecuteNonQuery();

                MessageBox.Show("Farmer updated successfully!");
                ClearFields();
                LoadFarmers();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("That Farmer Code already exists.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to update farmer:\n" + ex.Message);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedFarmerId == 0)
            {
                MessageBox.Show("Select a farmer from the table first.");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Are you sure you want to delete this farmer?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            try
            {
                using SqlConnection con = CreateConnection();
                con.Open();

                string sql = "DELETE FROM dbo.Farmers WHERE Id = @Id;";

                using SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Id", SqlDbType.Int)
                    .Value = selectedFarmerId;

                int rows = cmd.ExecuteNonQuery();

                MessageBox.Show(rows > 0
                    ? "Farmer deleted successfully."
                    : "Farmer record was not found.");

                ClearFields();
                LoadFarmers();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Unable to delete farmer. Check whether other records depend on this farmer.\n"
                    + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void AddFarmerParameters(SqlCommand cmd)
        {
            cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20)
                .Value = txtFarmerCode.Text.Trim();

            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
                .Value = txtFarmerName.Text.Trim();

            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 15)
                .Value = string.IsNullOrWhiteSpace(txtPhone.Text)
                    ? DBNull.Value : txtPhone.Text.Trim();

            cmd.Parameters.Add("@Village", SqlDbType.NVarChar, 100)
                .Value = string.IsNullOrWhiteSpace(txtVillage.Text)
                    ? DBNull.Value : txtVillage.Text.Trim();
        }

        private void DgvFarmers_CellClick(object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvFarmers.Rows[e.RowIndex];

            selectedFarmerId = Convert.ToInt32(row.Cells["Id"].Value);
            txtFarmerCode.Text =
                Convert.ToString(row.Cells["FarmerCode"].Value) ?? "";
            txtFarmerName.Text =
                Convert.ToString(row.Cells["FarmerName"].Value) ?? "";
            txtPhone.Text =
                Convert.ToString(row.Cells["Phone"].Value) ?? "";
            txtVillage.Text =
                Convert.ToString(row.Cells["Village"].Value) ?? "";
        }

        private void InitializeComponent()
        {

        }

        private void ClearFields()
        {
            selectedFarmerId = 0;
            txtFarmerCode.Clear();
            txtFarmerName.Clear();
            txtPhone.Clear();
            txtVillage.Clear();
            dgvFarmers.ClearSelection();
            txtFarmerCode.Focus();
        }
    }
}
