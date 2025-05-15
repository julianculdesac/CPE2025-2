using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SchoolMangementSystem
{
    public partial class LoginForm : Form
    {
        private readonly string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\Julian\OneDrive\Desktop\IS_photoDatabase1.accdb";

        // Image fading variables
        private Bitmap originalImage;
        private Timer fadeTimer;
        private int alpha = 0;
        private const int fadeSpeed = 5;

        public LoginForm()
        {
            InitializeComponent();
            InitializeFormComponents();
        }

        private void InitializeFormComponents()
        {
            // Initialize image fading
            InitializeImageFading();

            // Set password character
            password.PasswordChar = '*';

            // Set form properties
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void InitializeImageFading()
        {
            try
            {
                originalImage = new Bitmap("C:\\Users\\Julian\\Downloads\\5454-removebg-preview.png");
                pictureBox1.Image = ApplyOpacity(originalImage, alpha);

                fadeTimer = new Timer { Interval = 30 };
                fadeTimer.Tick += FadeTimer_Tick;
                fadeTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Image Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (alpha < 255)
            {
                alpha += fadeSpeed;
                alpha = Math.Min(alpha, 255);

                var oldImage = pictureBox1.Image;
                pictureBox1.Image = ApplyOpacity(originalImage, alpha);
                oldImage?.Dispose();
            }
            else
            {
                fadeTimer.Stop();
                fadeTimer.Dispose();
            }
        }

        private Bitmap ApplyOpacity(Bitmap original, int alphaValue)
        {
            if (original == null) return null;

            Bitmap fadedImage = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(fadedImage))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = alphaValue / 255f;

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(matrix);
                    g.DrawImage(original,
                        new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height,
                        GraphicsUnit.Pixel, attributes);
                }
            }
            return fadedImage;
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(password.Text))
            {
                MessageBox.Show("Please enter both Email and Password.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                // Get the user ID and validate credentials in one step
                int userId = GetUserIdFromCredentials(txtEmail.Text, password.Text);

                if (userId > 0) // Valid user ID found
                {
                    this.Hide();

                    // Check if we need to use SPixel.MainForm2 or MainForm
                    // If using your MainForm:
                    var mainForm = new MainForm();
                    if (mainForm is IUserIdentifiable userForm)
                    {
                        userForm.SetCurrentUser(userId);
                    }
                    mainForm.Show();

                    // If using SPixel.MainForm2 instead, uncomment these lines and comment out the above
                    /*
                    var mainForm = new SPixel.MainForm2();
                    mainForm.SetCurrentUser(userId);
                    mainForm.Show();
                    */
                }
                else
                {
                    MessageBox.Show("Invalid Email or Password.", "Login Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // New method to get user ID while validating credentials
        private int GetUserIdFromCredentials(string email, string password)
        {
            const string query = "SELECT ID FROM [Users] WHERE [Email] = ? AND [Password] = ?";

            using (var connection = new OleDbConnection(connectionString))
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.Add("?", OleDbType.VarChar).Value = email;
                command.Parameters.Add("?", OleDbType.VarChar).Value = password;

                connection.Open();

                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }

                return -1; // Return -1 for invalid credentials
            }
        }
        // Optional: Add this interface to standardize user ID setting
        public interface IUserIdentifiable
        {
            void SetCurrentUser(int userId);
        }

        private bool ValidateCredentials(string email, string password)
        {
            const string query = "SELECT [Password] FROM [Users] WHERE [Email] = @Email";

            using (var connection = new OleDbConnection(connectionString))
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                connection.Open();

                var result = command.ExecuteScalar();
                if (result == null) return false;

                // WARNING: This compares plain text passwords - not secure!
                return password == result.ToString();
            }
        }

        private void showPass_CheckedChanged(object sender, EventArgs e)
        {
            password.PasswordChar = showPass.Checked ? '\0' : '*';
        }

        private void Forgotpass_Click(object sender, EventArgs e)
        {
            this.Hide();
            new Form2().Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            new CreateAccount().Show();
        }

        private void exiT(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            originalImage?.Dispose();
            fadeTimer?.Dispose();
        }
    }
}