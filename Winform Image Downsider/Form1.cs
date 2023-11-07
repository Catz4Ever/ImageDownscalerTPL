using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Winform_Image_Downsider
{
    public partial class Form1 : Form
    {
        private object pictureBoxLock = new object();

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "Downscale Percentage";
            textBox1.ForeColor = System.Drawing.Color.Gray;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    try
                    {
                        pictureBox1.Image = Image.FromFile(selectedFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Selected file is not an image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Downscale Percentage")
            {
                textBox1.Text = "";
                textBox1.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Downscale Percentage";
                textBox1.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            string newText = textBox1.Text + e.KeyChar;
            if (!string.IsNullOrEmpty(newText) && (int.TryParse(newText, out int number) && number >= 0 && number <= 100))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void textBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (!double.TryParse(textBox1.Text, out double number) || number < 0 || number > 100)
                {
                    MessageBox.Show("Please enter a valid number between 0 and 100.");
                    textBox1.Text = "";
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (!double.TryParse(textBox1.Text, out double number) || number < 0 || number > 100 )
                {
                    MessageBox.Show("Please enter a valid number between 0 and 100.");
                    textBox1.Text = "";
                    return;
                }
            }

            int scalePercentage = Int32.Parse(textBox1.Text);
            Bitmap sourceImage = new Bitmap(pictureBox1.Image);

            int newWidth = sourceImage.Width * scalePercentage / 100;
            int newHeight = sourceImage.Height * scalePercentage / 100;
            Bitmap downscaledImage = new Bitmap(newWidth, newHeight);

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    int totalRed = 0, totalGreen = 0, totalBlue = 0;
                    int pixelCount = 0;

                    for (int newx = x * sourceImage.Width / newWidth; newx < (x + 1) * sourceImage.Width / newWidth; newx++)
                    {
                        for (int newy = y * sourceImage.Height / newHeight; newy < (y + 1) * sourceImage.Height / newHeight; newy++)
                        {
                            Color pixelColor = sourceImage.GetPixel(newx, newy);
                            totalRed += pixelColor.R;
                            totalGreen += pixelColor.G;
                            totalBlue += pixelColor.B;
                            pixelCount++;
                        }
                    }
                    Color averageColor = Color.FromArgb(totalRed / pixelCount, totalGreen / pixelCount, totalBlue / pixelCount);
                    Monitor.Enter(pictureBoxLock);
                    downscaledImage.SetPixel(x, y, averageColor);
                    Monitor.Exit(pictureBoxLock);
                }
            }
            lock (pictureBoxLock)
            {
                pictureBox1.Image = downscaledImage;
            }
        }
    }
}

