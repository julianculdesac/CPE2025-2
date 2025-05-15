using SPixel; // Assuming this is needed for MainForm2
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb; // <<< Add this namespace
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SchoolMangementSystem.LoginForm;



namespace SchoolMangementSystem
{
    public partial class MainForm : Form, IUserIdentifiable
    {

        public class CommentInfo
        {
            public int CommentID { get; set; }
            public int UserID { get; set; }
            public string CommentText { get; set; }
            public DateTime CommentDate { get; set; }
            public string UserEmail { get; set; }
        }
        private int currentUserId;

        // Implement the interface method
        public void SetCurrentUser(int userId)
        {
            currentUserId = userId;
            // You can add additional logic here if needed
            LoadImagesIntoPanel();
            UpdateRatingAnalytics();

        }
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\Julian\OneDrive\Desktop\IS_photoDatabase1.accdb";

        private bool isDragging = false;
        private Point offset;


        private bool isFullScreen = false;
        private FormWindowState previousWindowState;
        private Rectangle previousBounds;


        private Bitmap originalImage;
        private Timer fadeTimer;
        private int alpha = 0; // Start fully transparent
        private int fadeSpeed = 5; // Adjust fade speed


        public MainForm()
        {
            InitializeComponent();

            // Make the form borderless
            this.FormBorderStyle = FormBorderStyle.None;

            // Attach event handlers to the MENU bar (assuming it's a Panel or similar control)
            MainGrey.MouseDown += panel3_MouseDown;
            MainGrey.MouseMove += panel3_MouseMove;
            MainGrey.MouseUp += panel3_MouseUp;

            // Make the form borderless
            this.FormBorderStyle = FormBorderStyle.None;

            // Attach event handlers to the MENU bar (assuming it's a Panel or similar control)
            MainGrey.MouseDown += panel3_MouseDown;
            MainGrey.MouseMove += panel3_MouseMove;
            MainGrey.MouseUp += panel3_MouseUp;

            // Load your image into the PictureBox (replace with your image path)
            originalImage = new Bitmap("C:\\Users\\Julian\\Downloads\\5454-removebg-preview.png"); // Replace with your image path
            pictureBox1.Image = ApplyOpacity(originalImage, alpha); // Initialize with transparent image

            // Initialize the timer for fade-in
            fadeTimer = new Timer();
            fadeTimer.Interval = 30; // Adjust interval for smoother fade
            fadeTimer.Tick += FadeTimer_Tick;
            fadeTimer.Start();


            this.FormBorderStyle = FormBorderStyle.None;
            MainGrey.MouseDown += panel3_MouseDown;
            MainGrey.MouseMove += panel3_MouseMove;
            MainGrey.MouseUp += panel3_MouseUp;

            // Your existing image fade-in logic for pictureBox1
            try // Add try-catch for file loading
            {
                originalImage = new Bitmap("C:\\Users\\Julian\\Downloads\\5454-removebg-preview.png");
                pictureBox1.Image = ApplyOpacity(originalImage, alpha);

                fadeTimer = new Timer();
                fadeTimer.Interval = 30;
                fadeTimer.Tick += FadeTimer_Tick;
                fadeTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial image: {ex.Message}", "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Optionally set a default placeholder image for pictureBox1 if loading fails
            }
            UpdateRatingAnalytics();

        }

        public class RoundedPanel : Panel
        {
            private int cornerRadius = 10;

            public int CornerRadius
            {
                get { return cornerRadius; }
                set { cornerRadius = value; Invalidate(); }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = cornerRadius;
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseAllFigures();

                    this.Region = new Region(path);

                    using (Pen pen = new Pen(this.BorderStyle == BorderStyle.FixedSingle ? Color.Black : this.BackColor, 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                base.OnPaint(e);
            }
        }




        private void LoadImagesIntoPanel()
        {
            ClearImagePanel(); // Clear previous images
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    // Query to get all images from the new ImageSaves table
                    string query = "SELECT i.SaveID, i.UserID, i.PicSave, u.Email, i.SaveDate " +
                                   "FROM ImageSaves i INNER JOIN Users u ON i.UserID = u.ID " +
                                   "WHERE i.PicSave IS NOT NULL " +
                                   "ORDER BY i.SaveDate DESC"; // Most recent images first

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            bool imageFound = false;
                            while (reader.Read())
                            {
                                imageFound = true;
                                int saveId = reader.GetInt32(0);    // SaveID
                                int userId = reader.GetInt32(1);    // UserID
                                byte[] imageBytes = (byte[])reader.GetValue(2); // PicSave
                                string userEmail = reader.GetString(3); // Email

                                try
                                {
                                    // Create a panel to hold both the image and the label/rating
                                    Panel imagePanel = new Panel
                                    {
                                        Width = 200,
                                        Height = 240,
                                        BorderStyle = BorderStyle.FixedSingle, // Changed from FixedSingle to None
                                        BackColor = Color.LightGray,
                                        Margin = new Padding(62, 10, 10, 10),
                                        Padding = new Padding(5),
                                        Tag = saveId  // Store SaveID for deletion
                                    };
                                    // Convert to bitmap using memory stream
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        // Create PictureBox for displaying the image
                                        PictureBox picBox = new PictureBox
                                        {
                                            Width = 160,
                                            Height = 160,
                                            SizeMode = PictureBoxSizeMode.Zoom,
                                            Location = new Point(10, 10),
                                            Tag = saveId // Store SaveID for viewing
                   
                                        };
                                        // Create a new bitmap from the binary data
                                        picBox.Image = new Bitmap(ms);
                                        // Add click handler to open the image in a larger viewer
                                        picBox.Click += (s, e) => PictureBox_Click_OLE(s, e);
                                        // Only allow deletion if this user owns the image
                                        if (userId == currentUserId)
                                        {
                                            Button deleteBtn = new Button
                                            {
                                                Text = "X",
                                                Width = 25,
                                                Height = 25,
                                                Location = new Point(170, 10),
                                                Tag = saveId, // Store the SaveID
                                                ForeColor = Color.White,
                                                BackColor = Color.Red,
                                                FlatStyle = FlatStyle.Flat
                                            };
                                            deleteBtn.Click += (s, e) => DeleteImage((int)((Button)s).Tag);
                                            imagePanel.Controls.Add(deleteBtn);
                                        }
                                        // Create email label
                                        Label emailLabel = new Label
                                        {
                                            Text = userEmail,
                                            Width = 160,
                                            Height = 20,
                                            Location = new Point(10, 175),
                                            TextAlign = ContentAlignment.MiddleCenter,
                                            Font = new Font("Arial", 7,FontStyle.Italic)
                                        };
                                        // Create rating panel
                                        Panel ratingPanel = new Panel
                                        {
                                            Width = 170,
                                            Height = 30,
                                            Location = new Point(10, 200)
                                        };
                                        // Add star rating buttons (use saveId instead of userId now)
                                        for (int i = 1; i <= 5; i++)
                                        {
                                            Button starBtn = new Button
                                            {
                                                Text = "★",
                                                Width = 30,
                                                Height = 30,
                                                Location = new Point((i - 1) * 32, 0),
                                                Tag = new object[] { saveId, i }, // Store saveId and rating value
                                                FlatStyle = FlatStyle.Flat,
                                                Font = new Font("Arial", 16, FontStyle.Bold), // Increased font size

                                            };
                                            // Check if this user has already rated this image
                                            int existingRating = GetUserRating(currentUserId, saveId);
                                            if (i <= existingRating)
                                            {
                                                starBtn.ForeColor = Color.Black;
                                                starBtn.BackColor = Color.Yellow;
                                                starBtn.Font = new Font(starBtn.Font, FontStyle.Bold);
                                            }
                                            else
                                            {
                                                starBtn.ForeColor = Color.Gray;
                                            }
                                            starBtn.Click += StarButton_Click;
                                            ratingPanel.Controls.Add(starBtn);
                                        }
                                        // Add components to the panel
                                        imagePanel.Controls.Add(picBox);
                                        imagePanel.Controls.Add(emailLabel);
                                        imagePanel.Controls.Add(ratingPanel);
                                        // Add to the flow layout panel


                                        Panel commentsPanel = new Panel
                                        {
                                            Width = 180,
                                            Height = 150,
                                            Location = new Point(10, 230),
                                            AutoScroll = true
                                        };





                                        #region Comment2

                                        // Add comments to the panel
                                        List<CommentInfo> comments = GetCommentsForImage(saveId);
                                        int commentY = 0;
                                        foreach (CommentInfo comment in comments)
                                        {
                                            // Create comment container
                                            RoundedPanel commentContainer = new RoundedPanel
                                            {
                                                Width = 160,
                                                Height = 60,
                                                Location = new Point(0, commentY),
                                                CornerRadius = 10,  // Set the corner radius
                                                BackColor = Color.White,
                                                Padding = new Padding(2)
                                            };
                                            // Optional: Add a subtle shadow effect
                                            commentContainer.Paint += (s, e) =>
                                            {
                                                ControlPaint.DrawBorder(e.Graphics, commentContainer.ClientRectangle,
                                                    Color.LightGray, 1, ButtonBorderStyle.Solid,
                                                    Color.LightGray, 1, ButtonBorderStyle.Solid,
                                                    Color.Gray, 1, ButtonBorderStyle.Solid,
                                                    Color.Gray, 1, ButtonBorderStyle.Solid);
                                            };

                                            // User and date label
                                            Label userLabel = new Label
                                            {
                                                Text = $"{comment.UserEmail} - {comment.CommentDate.ToString("MM/dd/yyyy HH:mm")}",
                                                Width = 150,
                                                Height = 15,
                                                Location = new Point(3, 2),
                                                Font = new Font("Arial", 7, FontStyle.Bold),
                                                ForeColor = Color.Blue,
                                                BackColor = Color.White
                                            };

                                            // Comment text
                                            Label commentLabel = new Label
                                            {
                                                Text = comment.CommentText,
                                                Width = 150,
                                                Height = 30,
                                                Location = new Point(3, 18),
                                                Font = new Font("Arial", 8),
                                                AutoEllipsis = true
                                            };

                                            // Delete button (only for user's own comments)
                                            if (comment.UserID == currentUserId)
                                            {
                                                Button deleteBtn = new Button
                                                {
                                                    Text = "×",
                                                    Width = 20,
                                                    Height = 20,
                                                    Location = new Point(135, 2),
                                                    Tag = comment.CommentID,
                                                    FlatStyle = FlatStyle.Flat,
                                                    Font = new Font("Arial", 6),
                                                    ForeColor = Color.White,
                                                    BackColor = Color.Red
                                                };
                                                deleteBtn.Click += (s, e) => DeleteComment((int)((Button)s).Tag);
                                                commentContainer.Controls.Add(deleteBtn);
                                            }

                                            commentContainer.Controls.Add(userLabel);
                                            commentContainer.Controls.Add(commentLabel);
                                            commentsPanel.Controls.Add(commentContainer);

                                            commentY += 65; // Move to the next comment position
                                        }
                                        // Add comment input box
                                        TextBox commentInput = new TextBox
                                        {
                                            Width = 120,
                                            Height = 40,
                                            Location = new Point(0, commentY + 5),
                                            Multiline = true,
                                            Text = "Add a comment...",
                                            ForeColor = Color.Gray,
                                            BorderStyle = BorderStyle.FixedSingle,
                                            MaxLength = 30 // Limit to 30 characters
                                        };
                                        // Add a character count label
                                        Label charCountLabel = new Label
                                        {
                                            Text = "0/30",
                                            Width = 40,
                                            Height = 15,
                                            Location = new Point(0, commentY + 45),
                                            Font = new Font("Arial", 7),
                                            ForeColor = Color.Gray
                                        };
                                        // Update character count when text changes
                                        commentInput.TextChanged += (s, e) =>
                                        {
                                            if (commentInput.Text != "Add a comment...")
                                            {
                                                int count = commentInput.Text.Length;
                                                charCountLabel.Text = $"{count}/30";
                                                charCountLabel.ForeColor = count >= 50 ? Color.Red : Color.Gray;
                                            }
                                            else
                                            {
                                                charCountLabel.Text = "0/30";
                                                charCountLabel.ForeColor = Color.Gray;
                                            }
                                        };




                                        commentInput.Enter += (s, e) =>
                                        {
                                            if (commentInput.Text == "Add a comment...")
                                            {
                                                commentInput.Text = "";
                                                commentInput.ForeColor = Color.Black;
                                                charCountLabel.Text = "0/60";
                                            }
                                        };
                                        commentInput.Leave += (s, e) =>
                                        {
                                            if (string.IsNullOrWhiteSpace(commentInput.Text))
                                            {
                                                commentInput.Text = "Add a comment...";
                                                commentInput.ForeColor = Color.Gray;
                                                charCountLabel.Text = "0/60";
                                            }
                                        };


                                        commentsPanel.Controls.Add(commentInput);
                                        commentsPanel.Controls.Add(charCountLabel);

                                        #endregion

                                        Button postBtn = new Button
                                        {
                                            Text = "Post",
                                            Width = 50,
                                            Height = 25,
                                            Location = new Point(125, commentY + 10),
                                            Tag = saveId,
                                            BackColor = Color.LightBlue
                                        };
                                        postBtn.Click += (s, e) =>
                                        {
                                            if (commentInput.Text != "Add a comment..." && !string.IsNullOrWhiteSpace(commentInput.Text))
                                            {
                                                AddComment((int)((Button)s).Tag, commentInput.Text);
                                                commentInput.Text = "Add a comment...";
                                                commentInput.ForeColor = Color.Gray;
                                                LoadImagesIntoPanel(); // Reload to show new comment
                                            }
                                        };
                                        commentsPanel.Controls.Add(commentInput);
                                        commentsPanel.Controls.Add(charCountLabel);
                                        commentsPanel.Controls.Add(postBtn);
                                        // Add comments panel to the image panel
                                        imagePanel.Controls.Add(commentsPanel);
                                        imagePanel.Height += 150; // Increase panel height to 


                                        flowLayoutPanelImages.Controls.Add(imagePanel);



                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Handle image loading errors
                                    Label errorLabel = new Label
                                    {
                                        Text = $"Error:\nCould not load image\n{ex.Message}",
                                        ForeColor = Color.Red,
                                        Width = 180,
                                        Height = 240,
                                        TextAlign = ContentAlignment.MiddleCenter,
                                        BorderStyle = BorderStyle.FixedSingle
                                    };
                                    ToolTip tooltip = new ToolTip();
                                    tooltip.SetToolTip(errorLabel, ex.Message);
                                    flowLayoutPanelImages.Controls.Add(errorLabel);
                                }
                            }
                            if (!imageFound)
                            {
                                MessageBox.Show("No images found in the database.", "Information",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StarButton_Click(object sender, EventArgs e)
        {
            Button starBtn = (Button)sender;
            object[] tagData = (object[])starBtn.Tag;
            int imageId = (int)tagData[0];
            int rating = (int)tagData[1];
            SaveRating(currentUserId, imageId, rating);
            LoadImagesIntoPanel();
            UpdateRatingAnalytics(); // Add this line
        }
        private void SaveRating(int raterUserId, int imageId, int rating)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    // Check if rating exists
                    string checkQuery = "SELECT COUNT(*) FROM ImageRatings WHERE RaterID = ? AND ImageID = ?";
                    using (OleDbCommand checkCommand = new OleDbCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.Add("?", OleDbType.Integer).Value = raterUserId;
                        checkCommand.Parameters.Add("?", OleDbType.Integer).Value = imageId;

                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            // Update existing rating
                            string updateQuery = "UPDATE ImageRatings SET Rating = ?, RatingDate = ? WHERE RaterID = ? AND ImageID = ?";
                            using (OleDbCommand updateCommand = new OleDbCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.Add("?", OleDbType.Integer).Value = rating;
                                updateCommand.Parameters.Add("?", OleDbType.Date).Value = DateTime.Now;
                                updateCommand.Parameters.Add("?", OleDbType.Integer).Value = raterUserId;
                                updateCommand.Parameters.Add("?", OleDbType.Integer).Value = imageId;
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new rating
                            string insertQuery = "INSERT INTO ImageRatings (RaterID, ImageID, Rating, RatingDate) VALUES (?, ?, ?, ?)";
                            using (OleDbCommand insertCommand = new OleDbCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.Add("?", OleDbType.Integer).Value = raterUserId;
                                insertCommand.Parameters.Add("?", OleDbType.Integer).Value = imageId;
                                insertCommand.Parameters.Add("?", OleDbType.Integer).Value = rating;
                                insertCommand.Parameters.Add("?", OleDbType.Date).Value = DateTime.Now;
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving rating: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private int GetUserRating(int userId, int imageId)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    // Modified to use ImageID (SaveID) instead of ImageOwner
                    string query = "SELECT Rating FROM ImageRatings WHERE RaterID = ? AND ImageID = ?";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.Integer).Value = userId;
                        command.Parameters.Add("?", OleDbType.Integer).Value = imageId;

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently fail, returning 0 rating
                Console.WriteLine($"Error getting rating: {ex.Message}");
            }
            return 0;
        }
        private void PictureBox_Click_OLE(object sender, EventArgs e)
        {
            PictureBox clickedPicBox = sender as PictureBox;
            if (clickedPicBox != null && clickedPicBox.Image != null)
            {
                // Create a form to display the larger image
                Form imageViewerForm = new Form
                {
                    Text = "Image Viewer",
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterScreen,
                    MinimizeBox = true,
                    MaximizeBox = true
                };
                // Create a PictureBox for the larger image
                PictureBox largeImageBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = clickedPicBox.Image // Use the same image
                };
                // Add the PictureBox to the form and show it
                imageViewerForm.Controls.Add(largeImageBox);
                imageViewerForm.ShowDialog();
            }
        }

        private void ClearImagePanel()
        {
            // Properly dispose of all controls and their images
            foreach (Control control in flowLayoutPanelImages.Controls)
            {
                if (control is Panel panel)
                {
                    foreach (Control panelControl in panel.Controls)
                    {
                        if (panelControl is PictureBox picBox && picBox.Image != null)
                        {
                            picBox.Image.Dispose();
                            picBox.Image = null;
                        }
                        panelControl.Dispose();
                    }
                }
                control.Dispose();
            }
            flowLayoutPanelImages.Controls.Clear();
        }

        private void PictureBox_Click(object sender, EventArgs e, string imagePath)
        {
            // Show larger version of the image
            Form previewForm = new Form
            {
                Text = Path.GetFileName(imagePath),
                StartPosition = FormStartPosition.CenterScreen,
                Width = 800,
                Height = 600
            };

            PictureBox pbPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(imagePath)
            };

            previewForm.Controls.Add(pbPreview);
            previewForm.ShowDialog();
            pbPreview.Image.Dispose();
            previewForm.Dispose();
        }

        
        private void button2_Click(object sender, EventArgs e)
        {
            // Call the method to load images when the button is clicked
            LoadImagesIntoPanel();
        }


       
       

        // --- Override Form Closing to ensure panel images are disposed ---
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            ClearImagePanel(); // Dispose images when form closes
            originalImage?.Dispose(); // Dispose the main picturebox image
            base.OnFormClosing(e);
        }

        // --- Keep the rest of your existing methods ---
        // ... (FadeTimer_Tick, ApplyOpacity, etc.) ...

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (alpha < 255)
            {
                alpha += fadeSpeed;
                if (alpha > 255) alpha = 255;
                if (originalImage != null && pictureBox1 != null) // Check if objects exist
                {
                    try
                    {
                        // Re-apply opacity using the potentially updated originalImage
                        Bitmap fadedImage = ApplyOpacity(originalImage, alpha);
                        // Dispose the previous image in pictureBox1 before assigning the new one
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = fadedImage;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error applying opacity: {ex.Message}");
                        fadeTimer.Stop(); // Stop timer if error during apply
                    }
                }
            }
            else
            {
                fadeTimer.Stop();
            }
        }

        private Bitmap ApplyOpacity(Bitmap original, int alphaValue)
        {
            if (original == null) return null; // Handle null original image

            Bitmap fadedImage = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb); // Use ARGB format
            using (Graphics g = Graphics.FromImage(fadedImage))
            {
                g.Clear(Color.Transparent); // Ensure background is transparent

                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = Math.Max(0, Math.Min(1, alphaValue / 255f)); // Clamp alpha between 0 and 1

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.DrawImage(original,
                                new Rectangle(0, 0, original.Width, original.Height), // Destination rectangle
                                0, 0, original.Width, original.Height, // Source rectangle
                                GraphicsUnit.Pixel,
                                attributes);
                }
            }
            return fadedImage;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Are you sure you want to logout?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {

                LoginForm lForm = new LoginForm();
                lForm.Show();
                this.Hide();

            }
        }


        private void Move(object sender, EventArgs e)
        {

        }

        private void panel3_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                offset = new Point(e.X, e.Y);
            }
        }

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Corrected drag logic - calculate screen position relative to form's current location
                Point currentScreenPos = this.PointToScreen(e.Location);
                this.Location = new Point(currentScreenPos.X - offset.X, currentScreenPos.Y - offset.Y);
            }
        }


        private void panel3_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

      

        private void Fullscreenn(object sender, EventArgs e) // Assuming this is tied to a different button/menu?
        {
            ToggleFullScreen();
        }

        // Helper method to avoid duplicate fullscreen logic
        private void ToggleFullScreen()
        {
            if (!isFullScreen)
            {
                // Save current state before going fullscreen
                previousWindowState = this.WindowState;
                previousBounds = this.Bounds;

                // Go to fullscreen
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal; // Temporarily set to normal
                this.WindowState = FormWindowState.Maximized;
                
                isFullScreen = true;

                // Hide any controls you want to hide in fullscreen mode
                // For example: MainGrey.Visible = false;
            }
            else
            {
                // Restore from fullscreen
                this.TopMost = false;

                // Important: Set the FormBorderStyle to None BEFORE restoring WindowState
                this.FormBorderStyle = FormBorderStyle.None;

                // Restore previous state
                this.WindowState = previousWindowState;

                if (previousWindowState == FormWindowState.Normal)
                {
                    this.Bounds = previousBounds;
                }

                isFullScreen = false;

                // Show any controls you hid in fullscreen mode
                // For example: MainGrey.Visible = true;
            }
        }


        private void Minimize(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void MainGrey_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e) // Opens MainForm2
        {
            SPixel.MainForm2 editForm = new SPixel.MainForm2();

            // Pass the current user ID directly
            editForm.SetCurrentUser(this.currentUserId);

            editForm.ShowDialog();
        }


        private List<CommentInfo> GetCommentsForImage(int saveId)
        {
            List<CommentInfo> comments = new List<CommentInfo>();

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT c.CommentID, c.UserID, c.CommentText, c.CommentDate, u.Email " +
                                  "FROM Comments c INNER JOIN Users u ON c.UserID = u.ID " +
                                  "WHERE c.SaveID = ? ORDER BY c.CommentDate DESC";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.Integer).Value = saveId;

                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CommentInfo comment = new CommentInfo
                                {
                                    CommentID = reader.GetInt32(0),
                                    UserID = reader.GetInt32(1),
                                    CommentText = reader.GetString(2),
                                    CommentDate = reader.GetDateTime(3),
                                    UserEmail = reader.GetString(4)
                                };

                                comments.Add(comment);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading comments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return comments;
        }

        #region Comment
        // Add this method to add a new comment
        private void AddComment(int saveId, string commentText)
        {
            // Ensure comment doesn't exceed 60 characters
            if (commentText.Length > 30)
            {
                commentText = commentText.Substring(0, 30);
            }

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Comments (SaveID, UserID, CommentText, CommentDate) VALUES (?, ?, ?, ?)";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.Integer).Value = saveId;
                        command.Parameters.Add("?", OleDbType.Integer).Value = currentUserId;
                        command.Parameters.Add("?", OleDbType.VarChar, 60).Value = commentText; // Limit to 60 chars
                        command.Parameters.Add("?", OleDbType.Date).Value = DateTime.Now;

                        command.ExecuteNonQuery();
                        MessageBox.Show("Comment added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding comment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Add this method to delete a comment
        private void DeleteComment(int commentId)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    // First check if the current user is the comment owner
                    string checkQuery = "SELECT UserID FROM Comments WHERE CommentID = ?";
                    using (OleDbCommand checkCommand = new OleDbCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.Add("?", OleDbType.Integer).Value = commentId;

                        object result = checkCommand.ExecuteScalar();
                        if (result == null || (int)result != currentUserId)
                        {
                            MessageBox.Show("You can only delete your own comments", "Permission Denied",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Delete the comment
                    string deleteQuery = "DELETE FROM Comments WHERE CommentID = ?";
                    using (OleDbCommand command = new OleDbCommand(deleteQuery, connection))
                    {
                        command.Parameters.Add("?", OleDbType.Integer).Value = commentId;
                        command.ExecuteNonQuery();
                        MessageBox.Show("Comment deleted successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadImagesIntoPanel();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting comment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        private void DeleteImage(int saveId)
        {
            // Confirm deletion
            if (MessageBox.Show("Are you sure you want to delete this image?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();

                        // First verify this user owns the image
                        string checkQuery = "SELECT UserID FROM ImageSaves WHERE SaveID = ?";
                        using (OleDbCommand checkCommand = new OleDbCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.Add("?", OleDbType.Integer).Value = saveId;
                            object result = checkCommand.ExecuteScalar();

                            if (result == null || Convert.ToInt32(result) != currentUserId)
                            {
                                MessageBox.Show("You can only delete your own images.", "Permission Denied",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        // Delete image
                        string deleteQuery = "DELETE FROM ImageSaves WHERE SaveID = ?";
                        using (OleDbCommand command = new OleDbCommand(deleteQuery, connection))
                        {
                            command.Parameters.Add("?", OleDbType.Integer).Value = saveId;
                            int rowsDeleted = command.ExecuteNonQuery();

                            if (rowsDeleted > 0)
                            {
                                // Also delete any ratings for this image
                                string deleteRatingsQuery = "DELETE FROM ImageRatings WHERE ImageID = ?";
                                using (OleDbCommand ratingCommand = new OleDbCommand(deleteRatingsQuery, connection))
                                {
                                    ratingCommand.Parameters.Add("?", OleDbType.Integer).Value = saveId;
                                    ratingCommand.ExecuteNonQuery();
                                }

                                MessageBox.Show("Image deleted successfully.", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadImagesIntoPanel(); // Refresh the panel
                                UpdateRatingAnalytics();
                            }
                            else
                            {
                                MessageBox.Show("Image not found or could not be deleted.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting image: {ex.Message}", "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Analytics

        //ANALITICS
        private Dictionary<int, int> GetRatingCounts(int userId)
        {
            Dictionary<int, int> ratingCounts = new Dictionary<int, int>
    {
        { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
    };
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    // Modified query to avoid reserved words
                    string query = @"
                SELECT r.Rating, Count(*) AS RatingCount 
                FROM ImageRatings r 
                INNER JOIN ImageSaves i ON r.ImageID = i.SaveID 
                WHERE i.UserID = ? 
                GROUP BY r.Rating";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.Integer).Value = userId;

                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int rating = reader.GetInt32(0);
                                int count = reader.GetInt32(1);

                                if (rating >= 1 && rating <= 5)
                                {
                                    ratingCounts[rating] = count;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting rating counts: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ratingCounts;
        }

      private void DrawRatingBarGraph(Dictionary<int, int> ratingCounts, Panel panel)
{
    // Create image for drawing
    Bitmap bmp = new Bitmap(panel.Width, panel.Height);
    using (Graphics g = Graphics.FromImage(bmp))
    {
        // Clear background
        g.Clear(Color.White);
        // Calculate total ratings
        int totalRatings = ratingCounts.Values.Sum();
        // Skip drawing if no ratings
        if (totalRatings == 0)
        {
            g.DrawString("No ratings yet", new Font("Arial", 12), Brushes.Gray,
                new PointF(panel.Width / 2 - 50, panel.Height / 2 - 10));
            panel.BackgroundImage = bmp;
            return;
        }
        // Define colors for each star rating
        Color[] colors = new Color[] {
            Color.Red, Color.Orange, Color.Yellow, Color.LightGreen, Color.Green
        };
        // Draw title
        g.DrawString("Rating Distribution", new Font("Arial", 12, FontStyle.Bold),
            Brushes.Black, new PointF(10, 10));
        // Draw total count
        string totalRatingsText = $"Total Ratings: {totalRatings}";
        g.DrawString(totalRatingsText, new Font("Arial", 10, FontStyle.Bold),
            Brushes.Black, new PointF(panel.Width - 120, 10));
        // Calculate dimensions with space for text
        int titleHeight = 30;  // Space for title
        int bottomSpace = 40;  // Space for bottom labels
        int topSpace = 40;     // Space for top labels (counts)
        
        int graphHeight = panel.Height - titleHeight - bottomSpace - topSpace;
        int graphWidth = panel.Width - 80;    // Leave space for labels
        int barWidth = graphWidth / 5 - 10;   // Width of each bar with spacing
        int maxCount = ratingCounts.Values.Max() > 0 ? ratingCounts.Values.Max() : 1;
        // Draw the bottom axis line first
        int bottomLineY = panel.Height - bottomSpace;
        g.DrawLine(Pens.Black, 40, bottomLineY,
                  40 + 5 * (barWidth + 10), bottomLineY);
        // Draw bars and labels
        for (int i = 1; i <= 5; i++)
        {
            int count = ratingCounts[i];
            float percentage = (float)count / totalRatings * 100;
            // Calculate bar height based on count relative to max count
            int barHeight = (int)((float)count / maxCount * graphHeight);
            // Calculate bar position
            int x = 40 + (i - 1) * (barWidth + 10);
            int y = bottomLineY - barHeight; // Start from bottom line
            // Draw count and percentage (above the bars)
            string countLabel = $"{count} ({percentage:0.0}%)";
            SizeF countLabelSize = g.MeasureString(countLabel, new Font("Arial", 8));
            g.DrawString(countLabel, new Font("Arial", 8),
                Brushes.Black, new PointF(x + barWidth / 2 - countLabelSize.Width / 2, y - countLabelSize.Height - 5));
            // Draw the bar
            using (SolidBrush brush = new SolidBrush(colors[i - 1]))
            {
                g.FillRectangle(brush, x, y, barWidth, barHeight);
            }
            // Draw border
            g.DrawRectangle(Pens.Black, x, y, barWidth, barHeight);
            // Draw star label (below the x-axis)
            string starLabel = i.ToString() + "★";
            SizeF starLabelSize = g.MeasureString(starLabel, new Font("Arial", 9, FontStyle.Bold));
            g.DrawString(starLabel, new Font("Arial", 9, FontStyle.Bold),
                Brushes.Black, new PointF(x + barWidth / 2 - starLabelSize.Width / 2, bottomLineY + 5));
        }
    }
    // Set the panel's background image
    panel.BackgroundImage = bmp;
}

        private void UpdateRatingAnalytics()
        {
            // Get the rating counts for current user
            Dictionary<int, int> ratingCounts = GetRatingCounts(currentUserId);

            // Draw the bar graph
            DrawRatingBarGraph(ratingCounts, panel2);
        }
        #endregion



        private void button1_Click(object sender, EventArgs e)//DELETE BUTTON
        {
            // Your existing file deletion code... (looks reasonable)
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Image File to Delete";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff|All Files|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                openFileDialog.Multiselect = false;


                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePathToDelete = openFileDialog.FileName;

                    DialogResult confirmation = MessageBox.Show(
                        $"Are you absolutely sure you want to permanently delete this file?\n\n{filePathToDelete}",
                        "Confirm Permanent File Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);


                    if (confirmation == DialogResult.Yes)
                    {
                        try
                        {
                            File.Delete(filePathToDelete);

                            MessageBox.Show($"File deleted successfully:\n{filePathToDelete}",
                                            "Deletion Complete",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                            // TODO: Consider removing the corresponding entry from the database as well!
                            // You would need an OleDb DELETE command here using the filePathToDelete
                            // DeleteFromDatabase(filePathToDelete);
                        }
                        catch (IOException ioEx)
                        {
                            MessageBox.Show($"Could not delete the file. It might be in use by another program.\n\nError: {ioEx.Message}",
                                            "File Deletion Error",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }
                        catch (UnauthorizedAccessException uaEx)
                        {
                            MessageBox.Show($"Could not delete the file. Permission denied.\n\nError: {uaEx.Message}",
                                            "File Deletion Error",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An unexpected error occurred while trying to delete the file.\n\nError: {ex.Message}",
                                            "File Deletion Error",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    } // End of MainForm class
} // End of namespace