namespace WinFormsApp1
{
    partial class DashboardForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnFarmers = new Button();
            btnMilkCollection = new Button();
            btnPayments = new Button();
            btnReports = new Button();
            btnLogout = new Button();
            Rate_Chart = new Button();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // btnFarmers
            // 
            btnFarmers.BackColor = SystemColors.Highlight;
            btnFarmers.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFarmers.ForeColor = SystemColors.ButtonHighlight;
            btnFarmers.Location = new Point(63, 84);
            btnFarmers.Name = "btnFarmers";
            btnFarmers.Size = new Size(220, 57);
            btnFarmers.TabIndex = 1;
            btnFarmers.Text = "Farmer Master";
            btnFarmers.UseVisualStyleBackColor = false;
            btnFarmers.Click += btnFarmers_Click;
            // 
            // btnMilkCollection
            // 
            btnMilkCollection.BackColor = SystemColors.Highlight;
            btnMilkCollection.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnMilkCollection.ForeColor = SystemColors.ButtonHighlight;
            btnMilkCollection.Location = new Point(63, 206);
            btnMilkCollection.Name = "btnMilkCollection";
            btnMilkCollection.Size = new Size(220, 55);
            btnMilkCollection.TabIndex = 2;
            btnMilkCollection.Text = "Milk Collection";
            btnMilkCollection.UseVisualStyleBackColor = false;
            btnMilkCollection.Click += btnMilkCollection_Click;
            // 
            // btnPayments
            // 
            btnPayments.BackColor = SystemColors.Highlight;
            btnPayments.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPayments.ForeColor = SystemColors.ButtonHighlight;
            btnPayments.Location = new Point(653, 332);
            btnPayments.Name = "btnPayments";
            btnPayments.Size = new Size(214, 63);
            btnPayments.TabIndex = 3;
            btnPayments.Text = "Payments";
            btnPayments.UseVisualStyleBackColor = false;
            btnPayments.Click += btnPayments_Click;
            // 
            // btnReports
            // 
            btnReports.BackColor = SystemColors.Highlight;
            btnReports.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnReports.ForeColor = SystemColors.ButtonHighlight;
            btnReports.Location = new Point(653, 206);
            btnReports.Name = "btnReports";
            btnReports.Size = new Size(214, 55);
            btnReports.TabIndex = 4;
            btnReports.Text = "Reports";
            btnReports.UseVisualStyleBackColor = false;
            btnReports.Click += btnReports_Click;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.IndianRed;
            btnLogout.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogout.ForeColor = SystemColors.ButtonHighlight;
            btnLogout.Location = new Point(1138, 33);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(138, 45);
            btnLogout.TabIndex = 5;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // Rate_Chart
            // 
            Rate_Chart.BackColor = SystemColors.Highlight;
            Rate_Chart.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Rate_Chart.ForeColor = SystemColors.ButtonHighlight;
            Rate_Chart.Location = new Point(53, 341);
            Rate_Chart.Name = "Rate_Chart";
            Rate_Chart.Size = new Size(230, 65);
            Rate_Chart.TabIndex = 6;
            Rate_Chart.Text = "Rate_Chart";
            Rate_Chart.UseVisualStyleBackColor = false;
            Rate_Chart.Click += Rate_Chart_Click;
            // 
            // button1
            // 
            button1.BackColor = SystemColors.Highlight;
            button1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button1.ForeColor = SystemColors.ButtonHighlight;
            button1.Location = new Point(653, 84);
            button1.Name = "button1";
            button1.Size = new Size(214, 57);
            button1.TabIndex = 7;
            button1.Text = "Ledger";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.BackColor = SystemColors.Highlight;
            button2.Font = new Font("Verdana", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button2.ForeColor = Color.Red;
            button2.Location = new Point(-7, -5);
            button2.Name = "button2";
            button2.Size = new Size(1283, 83);
            button2.TabIndex = 8;
            button2.Text = "ADVANCED MILK DAIRY SYSTEM";
            button2.TextImageRelation = TextImageRelation.ImageAboveText;
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // DashboardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1266, 450);
            Controls.Add(button1);
            Controls.Add(Rate_Chart);
            Controls.Add(btnLogout);
            Controls.Add(btnReports);
            Controls.Add(btnPayments);
            Controls.Add(btnMilkCollection);
            Controls.Add(btnFarmers);
            Controls.Add(button2);
            Name = "DashboardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Advanced Milk Dairy System - Dashboard";
            WindowState = FormWindowState.Maximized;
            Load += DashboardForm_Load;
            ResumeLayout(false);
        }

        #endregion
        private Button btnFarmers;
        private Button btnMilkCollection;
        private Button btnPayments;
        private Button btnReports;
        private Button btnLogout;
        private Button Rate_Chart;
        private Button button1;
        private Button button2;
    }
}