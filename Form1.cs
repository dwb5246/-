using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<byte> RList = new List<byte>();
        List<byte> GList = new List<byte>();
        List<byte> BList = new List<byte>();

        private void button1_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Text_FilePath.Text);
            if (!Directory.Exists(fileInfo.DirectoryName + @"\Image"))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName + @"\Image");
            }
            using (FileStream stream = File.OpenRead(Text_FilePath.Text))
            using (FileStream output = File.OpenWrite(string.Format(fileInfo.DirectoryName+@"\Image\new_"+ fileInfo.Name)))
            using (Image<Rgba32> image = Image.Load(stream))
            {
                ClipMethod2(output, image);
            }
            MessageBox.Show(string.Format("文件已保存到->{0}", string.Format(fileInfo.DirectoryName + @"\Image\new_" + fileInfo.Name)), "提示");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Text_FilePath.Text);
            if (!Directory.Exists(fileInfo.DirectoryName + @"\Image"))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName + @"\Image");
            }
            using (FileStream stream = File.OpenRead(Text_FilePath.Text))
            using (FileStream output = File.OpenWrite(string.Format(fileInfo.DirectoryName + @"\Image\new_" + fileInfo.Name)))
            using (Image<Rgba32> image = Image.Load(stream))
            {
                ClipMethod3(output, image);
            }
            MessageBox.Show(string.Format("文件已保存到->{0}", string.Format(fileInfo.DirectoryName + @"\Image\new_" + fileInfo.Name)), "提示");
        }

        private void ClipMethod(FileStream output, Image<Rgba32> image)
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    RList.Add(image[i, j].R);
                    GList.Add(image[i, j].G);
                    BList.Add(image[i, j].B);
                }
            }
            byte mR = RList.Max();
            byte mG = GList.Max();
            byte mB = BList.Max();
            Rgb24 rgb = new Rgb24(mR, mG, mB);
            double max = 0;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double curDis = HSV.distanceOf(rgb, image[i, j].Rgb);
                    max = curDis > max ? curDis : max;
                }
            }
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double curDis = HSV.distanceOf(rgb, image[i, j].Rgb) / max;
                    Rgba32 item = image[i, j];
                    if (curDis <= Convert.ToDouble(text_box1.Value))
                    {
                        item.A = 1;
                        image[i, j] = item;
                    }
                }
            }
            image.SaveAsPng(output);
        }

        /// <summary>
        /// 通过RGB每个差值移除背景
        /// </summary>
        /// <param name="image"></param>
        private void ClipMethod1(FileStream output, Image<Rgba32> image)
        {
            double des = Convert.ToDouble(text_box1.Value);
            Image<Rgba32> image2 = image.Clone();
            image2.Mutate(x => x.Grayscale());
            Dictionary<int, int> dic = new Dictionary<int, int>();
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Rgba32 item = image[i, j];
                    if (Math.Abs(item.R - item.G) + Math.Abs(item.G - item.B) + Math.Abs(item.R - item.B) < des)
                    {
                        item.A = 1;
                        image[i, j] = item;
                    }
                }
            }
            image.SaveAsPng(output);
        }
        /// <summary>
        /// 通过RGB距离移除背景
        /// </summary>
        /// <param name="output"></param>
        /// <param name="image"></param>
        private void ClipMethod2(FileStream output, Image<Rgba32> image)
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    RList.Add(image[i, j].R);
                    GList.Add(image[i, j].G);
                    BList.Add(image[i, j].B);
                }
            }
            byte mR = RList.Max();
            byte mG = GList.Max();
            byte mB = BList.Max();
            Rgb24 bgr = new Rgb24(mR, mG, mB);
            double max = 0;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double curDis = HSV.distanceOf(bgr, image[i, j].Rgb);
                    max = curDis > max ? curDis : max;
                }
            }
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double curDis = 1-HSV.distanceOf(bgr, image[i, j].Rgb) / max;
                    Rgba32 item = image[i, j];
                    if (curDis >= Convert.ToDouble(text_box1.Text))
                    {
                        item.A = 1;
                        image[i, j] = item;
                    }
                }
            }
            image.SaveAsPng(output);
        }

        /// <summary>
        /// 通过灰度图像，移除差距较小的像素
        /// </summary>
        /// <param name="output"></param>
        /// <param name="image"></param>
        private void ClipMethod3(FileStream output, Image<Rgba32> image)
        {
            double des = Convert.ToDouble(text_box2.Text);
            Image<Rgba32> image2 = image.Clone();
            image2.Mutate(x => x.Grayscale());
            Dictionary<int, int> dic = new Dictionary<int, int>();
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Rgba32 item = image[i, j];
                    int distance = (int)DistanceOf(image2[i, j].Rgb, image[i, j].Rgb);
                    if (distance <= des)
                    {
                        item.A = 1;
                        image[i, j] = item;
                    }
                }
            }
            image.SaveAsPng(output);
        }
        public double DistanceOf(Rgb24 hsv1, Rgb24 hsv2)
        {
            double x = hsv1.R - hsv2.R;
            double y = hsv1.G - hsv2.G;
            double z = hsv1.B - hsv2.B;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            Text_FilePath.Text = path;
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
                this.Text_FilePath.Cursor = System.Windows.Forms.Cursors.Arrow;  //指定鼠标形状（更好看）
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择图片文件";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Text_FilePath.Text = openFileDialog.FileName;
            }
        }
    }
    public static class HSV
    {
        public static double distanceOf(Rgb24 hsv1, Rgb24 hsv2)
        {
            double x = hsv1.R - hsv2.R;
            double y = hsv1.G - hsv2.G;
            double z = hsv1.B - hsv2.B;
            return Math.Sqrt(x * x + y * y + z * z);
        }
    }
}
