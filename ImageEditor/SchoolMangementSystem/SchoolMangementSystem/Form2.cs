using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb; // Changed from SqlClient to OleDb
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SchoolMangementSystem
{
    public partial class Form2 : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\Julian\OneDrive\Desktop\IS_photoDatabase1.accdb";
        private string generatedCode;
        private int userId;

        public Form2()
        {
            InitializeComponent();
            // Initialize UI state - initially only email section should be enabled
            
        }

        private void btnSendCode_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            // Validate email
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter your email address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var mailAddress = new MailAddress(email);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid email address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if email exists in database
            if (!VerifyEmailExists(email, out userId))
            {
                MessageBox.Show("This email is not registered in our system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Generate a random 6-digit code
            Random random = new Random();
            generatedCode = random.Next(100000, 999999).ToString();

            // Send email with verification code
            if (SendVerificationEmail(email, generatedCode))
            {
                MessageBox.Show("Verification code has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Enable verification code input
                txtVerificationCode.Enabled = true;
                btnVerifyCode.Enabled = true;
                txtEmail.Enabled = false; // Prevent changing email after code is sent
            }
        }

        private void btnVerifyCode_Click(object sender, EventArgs e)
        {
            string enteredCode = txtVerificationCode.Text.Trim();
            if (string.IsNullOrEmpty(enteredCode))
            {
                MessageBox.Show("Please enter the verification code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (enteredCode == generatedCode)
            {
                MessageBox.Show("Code verified successfully. You can now reset your password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Enable password reset fields
                txtNewPassword.Enabled = true;
                txtConfirmPassword.Enabled = true;
                resetPasswordButton.Enabled = true;

                // Disable verification code section
                txtVerificationCode.Enabled = false;
                btnVerifyCode.Enabled = false;
            }
            else
            {
                MessageBox.Show("Invalid verification code. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void resetPasswordButton_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            // Validate password
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Please enter a new password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Update password in database
            if (UpdatePasswordInDatabase(userId, newPassword))
            {
                MessageBox.Show("Your password has been reset successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Navigate back to login form
                LoginForm mForm = new LoginForm();
                mForm.Show();
                this.Hide();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            LoginForm mForm = new LoginForm();
            mForm.Show();
            this.Hide();
        }

        private bool VerifyEmailExists(string email, out int userId)
        {
            userId = 0;

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT ID FROM Users WHERE Email = ?";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.VarChar).Value = email;

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            userId = Convert.ToInt32(result);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private bool SendVerificationEmail(string recipientEmail, string verificationCode)
        {
            try
            {
                string senderEmail = "taebahu07@gmail.com";
                string appPassword = "vefk isyj shrj wqlv"; // Replace with the app password you generated

                MailMessage mail = new MailMessage();
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(senderEmail);
                mail.To.Add(recipientEmail);
                mail.Subject = "Password Reset Verification Code";
                mail.Body = $"Your verification code for password reset is: {verificationCode}";

                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential(senderEmail, appPassword);
                smtpClient.EnableSsl = true;

                smtpClient.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending email: {ex.Message}", "Email Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool UpdatePasswordInDatabase(int userId, string newPassword)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string query = "UPDATE Users SET [Password] = ? WHERE ID = ?";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.VarChar).Value = newPassword;
                        command.Parameters.Add("?", OleDbType.Integer).Value = userId;

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating password: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Empty implementation
        }

       
    }
}