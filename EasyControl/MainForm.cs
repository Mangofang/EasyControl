using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyControl
{
    public class MainForm : Form
    {
        private static PictureBox pictureBox;
        private static MainForm instance;

        public MainForm()
        {
            instance = this;
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(pictureBox);
        }

        public static void UpdateImage(Image image)
        {
            instance.Invoke(new Action(() =>
            {
                pictureBox.Image = image;
            }));
        }

        public static void CloseForm()
        {
            instance.Invoke(new Action(() =>
            {
                instance.Close();
            }));
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(719, 486);
            this.Name = "MainForm";
            this.Text = "屏幕监控";
            this.ResumeLayout(false);

        }
    }
}
