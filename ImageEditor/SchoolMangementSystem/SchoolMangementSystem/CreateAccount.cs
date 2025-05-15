using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Net.Mail;
using System.Net.NetworkInformation;
namespace SchoolMangementSystem
{
    public partial class CreateAccount : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\Julian\OneDrive\Desktop\IS_photoDatabase1.accdb";
        // --- Form Constructor ---
        public CreateAccount()
        {
            InitializeComponent();

            // Add this line for real-time email validation
            txtEmail.TextChanged += TxtEmail_TextChanged;
        }
        // Method to check if email exists
        private bool IsEmailAlreadyRegistered(string email)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM Users WHERE Email = ?";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.VarChar).Value = email;
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking email: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // Return false on error to allow continuation
            }
        }
        // Real-time email validation handler
        private void TxtEmail_TextChanged(object sender, EventArgs e)
        {
            // Skip check if email is empty or invalid format
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.BackColor = SystemColors.Window; // Reset to default
                return;
            }

            try
            {
                var emailAddress = new MailAddress(txtEmail.Text);

                // Only check database if we have a valid email format
                if (IsEmailAlreadyRegistered(txtEmail.Text))
                {
                    // Visual indication that email is already registered
                    txtEmail.BackColor = Color.MistyRose; // Light red background

                    // You could also add a tooltip if you want
                    ToolTip tip = new ToolTip();
                    tip.SetToolTip(txtEmail, "This email is already registered");
                }
                else
                {
                    // Reset to normal if email is available
                    txtEmail.BackColor = SystemColors.Window; // Default background
                }
            }
            catch (FormatException)
            {
                // Invalid email format, don't check database yet
                txtEmail.BackColor = SystemColors.Window;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            LoginForm mForm = new LoginForm();
            mForm.Show();
            this.Hide();
        }
 

        private void button1_Click(object sender, EventArgs e)
        {
            // Keep existing validation code
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                var emailAddress = new MailAddress(txtEmail.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid email format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            // Add this email duplicate check
            if (IsEmailAlreadyRegistered(txtEmail.Text))
            {
                MessageBox.Show("This email is already registered. Please use a different email or try to log in.",
                    "Email Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Password cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }


            string email = txtEmail.Text;
            string password = txtPassword.Text;
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                // Start a transaction to ensure all operations succeed or fail together
                OleDbTransaction transaction = connection.BeginTransaction();
                try
                {
                    // First, insert into Users table and get the ID
                    int newUserId = 0;

                    // First approach - insert into Users
                    using (OleDbCommand command = new OleDbCommand("INSERT INTO Users (Email, [Password]) VALUES (?, ?)", connection, transaction))
                    {
                        // Password is a reserved word in some contexts, so use square brackets
                        command.Parameters.Add("?", OleDbType.VarChar).Value = email;
                        command.Parameters.Add("?", OleDbType.VarChar).Value = password;

                        command.ExecuteNonQuery();

                        // Get the newly created user ID
                        command.CommandText = "SELECT @@IDENTITY";
                        newUserId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // Now insert the same ID into AccSaves table
                    using (OleDbCommand command = new OleDbCommand("INSERT INTO AccSaves (ID) VALUES (?)", connection, transaction))
                    {
                        command.Parameters.Add("?", OleDbType.Integer).Value = newUserId;
                        command.ExecuteNonQuery();
                    }

                    // Commit the transaction
                    transaction.Commit();

                    MessageBox.Show("Account Created Successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoginForm mForm = new LoginForm();
                    mForm.Show();
                    this.Hide();
                }
                catch (OleDbException ex)
                {
                    // Rollback the transaction if an error occurs
                    transaction.Rollback();

                    MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }



            }



        }
    }
}


