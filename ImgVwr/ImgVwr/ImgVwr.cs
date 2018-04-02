using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImgVwr
{
    public partial class ImgVwr : Form
    {
        string currentDir = "C:\\Users\\Ricky\\Pictures";
        int zoomChange = 0;

        public ImgVwr()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            string argFileName="";

            foreach (string arg in args)
                if (arg.ToLower().EndsWith(".jpg") || arg.ToLower().EndsWith(".jpeg") || arg.ToLower().EndsWith(".png") || arg.ToLower().EndsWith(".bmp"))
                {
                    argFileName = Path.GetFileName(arg);
                    currentDir = Path.GetDirectoryName(arg);
                    break;
                }

            try
            {
                newDir();
                ImageList.SelectedIndex= ImageList.FindString(argFileName);
            }
            catch { }
        }

        private void newDir()
        {
            textBox1.Text = currentDir;
            var dirInfo = new DirectoryInfo(currentDir);
            var files = dirInfo.GetFiles().Where(c => (c.Extension.Equals(".jpg") || c.Extension.Equals(".jpeg") || c.Extension.Equals(".png") || c.Extension.Equals(".bmp")));
            ImageList.Items.Clear();
            foreach (var image in files)
                ImageList.Items.Add(image.Name);
            if(ImageList.Items.Count>0)
                ImageList.SelectedIndex = 0;
        }

        //load dir
        private void OpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                var fb = new FolderBrowserDialog();
                if(fb.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                {
                    currentDir = fb.SelectedPath; //get selected folder
                    newDir();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        string imgPath;
        int freshImg = 0;
        //load img
        private void ImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var selectedImage = ImageList.SelectedItems[0].ToString();
                if(!string.IsNullOrEmpty(selectedImage) && !string.IsNullOrEmpty(currentDir))
                {
                    imgPath= Path.Combine(currentDir, selectedImage);
                    DeleteCurrentImg();
                    LoadImg();
                    freshImg = 1;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteCurrentImg()
        {
            if (freshImg == 1)
                pictureBox1.Image.Dispose();
        }

        private void LoadImg()
        {
            pictureBox1.Image = Image.FromFile(imgPath);
            //Set the zoomed width and height
            widthOG = pictureBox1.Image.Width;
            heightOG = pictureBox1.Image.Height;
            zoomChange = 0;
        }
        int widthOG=0, heightOG=0;

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if(pictureBox1.Image!=null)
                ZoomButton_Click(0);
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if(pictureBox1.Image!=null)
                ZoomButton_Click(1);
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if(pictureBox1.Image!=null)
                ZoomButton_Click(-1);
        }

        private void LeftButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Image img = pictureBox1.Image;
                Bitmap bmp = new Bitmap(img, img.Width, img.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                pictureBox1.Image = bmp;
            }
        }

        private void RightButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Image img = pictureBox1.Image;
                Bitmap bmp = new Bitmap(img, img.Width, img.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                pictureBox1.Image = bmp;
            }
        }

        private void ImgVwr_DragDrop(object sender, DragEventArgs e)
        {
            string[] dropped = ((string[])e.Data.GetData(DataFormats.FileDrop, false));

            foreach (string drop in dropped)
                if (drop.ToLower().EndsWith(".jpg") || drop.ToLower().EndsWith(".jpeg") || drop.ToLower().EndsWith(".png") || drop.ToLower().EndsWith(".bmp"))
                {
                    ImageList.Items.Add(drop);
                    ImageList.SelectedIndex = ImageList.Items.Count-1;
                }
        }

        private void ImgVwr_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.All;
        }

        private Point startingPoint = Point.Empty;
        private Point movingPoint = Point.Empty;
        private bool panning = false;

        void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            panning = true;
            startingPoint = new Point(e.Location.X - movingPoint.X,
                                      e.Location.Y - movingPoint.Y);
        }

        void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            panning = false;
        }

        void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(panning && zoomChange==1)
            {
                movingPoint = new Point(e.Location.X - startingPoint.X,
                                        e.Location.Y - startingPoint.Y);
                pictureBox1.Invalidate();
            }
        }

        void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if(panning && zoomChange==1)
            {
               e.Graphics.Clear(Color.White);
               e.Graphics.DrawImage(pictureBox1.Image, movingPoint);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                if (Directory.Exists(textBox1.Text))
                {
                    currentDir = textBox1.Text;
                    newDir();
                    e.Handled=true;
                }
        }

        private void ZoomButton_Click(int zoom)
        {
            Image img = pictureBox1.Image;
            //Set the zoomed width and height
            int imgWidth = pictureBox1.Image.Width;
            int imgHeight = pictureBox1.Image.Height;
            if(zoomChange==0)
            {
                double imgW2H = Convert.ToDouble(imgWidth) / imgHeight;
                imgWidth = pictureBox1.Width;
                imgHeight = Convert.ToInt32( imgWidth / imgW2H);
            }

            if (zoom!=0)
            {
                pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
                int newHeight = Convert.ToInt32(imgHeight + (zoom*.1* imgHeight));
                int newWidth = Convert.ToInt32(imgWidth + (zoom *.1 * imgWidth));

                Bitmap bmp = new Bitmap(img, newWidth,newHeight);
                Graphics g = Graphics.FromImage(bmp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pictureBox1.Image = bmp;
                zoomChange = 1;
            }
            else
            {
                pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
                Bitmap bmp = new Bitmap(img);
                Graphics g = Graphics.FromImage(bmp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pictureBox1.Image = bmp;
                zoomChange = 0;
            }
        }
    }
}
