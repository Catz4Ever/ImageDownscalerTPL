using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System;

namespace Winform_Image_Downsider
{
    public partial class Form1 : Form
    {

        int scalePercentage, newHeight, newWidth;
        Bitmap sourceImage;
        Bitmap downscaledImage;

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
                        button2.Enabled = true;
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
                    textBox1.Text = "Downscale Percentage";
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private object lockObject = new object();
        void ImageDownscale(int initial, int upper, Bitmap threadSourceImage)
        {
            int sourceWidth = threadSourceImage.Width;
            int sourceHeight = threadSourceImage.Height;

            for (int x = initial; x < upper; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    int totalRed = 0, totalGreen = 0, totalBlue = 0;
                    int pixelCount = 0;

                    for (int newX = x * sourceWidth / newWidth; newX < (x + 1) * sourceWidth / newWidth; newX++)
                    {
                        if (newX >= 0 && newX < sourceWidth)
                        {
                            for (int newY = y * sourceHeight / newHeight; newY < (y + 1) * sourceHeight / newHeight; newY++)
                            {
                                if (newY >= 0 && newY < sourceHeight)
                                {
                                    Color color = threadSourceImage.GetPixel(newX, newY);
                                    totalRed += color.R;
                                    totalGreen += color.G;
                                    totalBlue += color.B;
                                    pixelCount++;
                                }
                            }
                        }
                    }

                    if (pixelCount > 0)
                    {
                        Color averageColor = Color.FromArgb(totalRed / pixelCount, totalGreen / pixelCount, totalBlue / pixelCount);
                        lock (lockObject)
                        {
                            downscaledImage.SetPixel(x, y, averageColor);
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (!double.TryParse(textBox1.Text, out double number) || number < 0 || number > 100)
                {
                    MessageBox.Show("Please enter a valid number between 0 and 100.");
                    textBox1.Text = "Downscale Percentage";
                    return;
                }
            }
            else if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid number between 0 and 100.");
                textBox1.Text = "Downscale Percentage";
                return;
            }

            scalePercentage = Int32.Parse(textBox1.Text);
            sourceImage = new Bitmap(pictureBox1.Image);
            newWidth = sourceImage.Width * scalePercentage / 100;
            newHeight = sourceImage.Height * scalePercentage / 100;
            downscaledImage = new Bitmap(newWidth, newHeight);

            int totalThreads = Environment.ProcessorCount;
            int toBeDivided = newWidth;
            int partSize = toBeDivided / totalThreads;

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < totalThreads; i++)
            {
                int partStart = i * partSize;
                int partEnd = (i + 1) * partSize;

                if (i == totalThreads - 1)
                {
                    partEnd = newWidth - 1;
                }

                Bitmap threadSourceImage = new Bitmap(sourceImage);
                Thread t = new Thread(() => ImageDownscale(partStart, partEnd, threadSourceImage));
                t.Start();
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }
            pictureBox1.Image = downscaledImage;

        }
    }
}

