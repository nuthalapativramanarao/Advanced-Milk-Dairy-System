using System;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // Your database connection string profile
        private readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=dairy;Trusted_Connection=True;TrustServerCertificate=True;";

        public Form1()
        {
            InitializeComponent();

            
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Left empty intentionally
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExecuteLoginAuthentication();
            if (string.IsNullOrWhiteSpace(un.Text) ||
       string.IsNullOrWhiteSpace(pwd.Text))
            {
                MessageBox.Show("Please enter Username and Password.");
                return;
            }

            DashboardForm dashboard = new DashboardForm();

            this.Hide();
            dashboard.FormClosed += (s, args) => this.Close();
            dashboard.Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            ExecuteLoginAuthentication();
        }

        private void ExecuteLoginAuthentication()
        {
            string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Safely handles values from your textboxes 'un' and 'pwd'
                        cmd.Parameters.AddWithValue("@Username", un.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", pwd.Text.Trim());

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count == 1)
                        {
                            MessageBox.Show("Login Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Optional: Add dashboard form routing code here if needed
                        }
                        else
                        {
                            MessageBox.Show("Invalid Username or Password.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Connection Error Details:\n\n{ex.Message}", "Connection Failure Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pwd_TextChanged(object sender, EventArgs e)
        {
            // Left empty intentionally
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Left empty intentionally
        }
    }
}
