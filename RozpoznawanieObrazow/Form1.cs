using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RozpoznawanieObrazow
{
    public partial class Form1 : Form
    {
        public static Bitmap imageOrginal0 = Properties.Resources._1a;

        public static Bitmap imageOrginal1 = Properties.Resources._1;

        public static string filePath1 = "";
        public static string filePath2 = "";
        public static string filePath3 = "";

        private bool isCloseEnough(Color first, Color second)
        {
            return Math.Abs(first.R - second.R) < 10 && Math.Abs(first.G - second.G) < 10 && Math.Abs(first.B - second.B) < 10;
        }
        private bool isCloseEnough(Color first, Color second, int offset)
        {
            return Math.Abs(first.R - second.R) < offset && Math.Abs(first.G - second.G) < offset && Math.Abs(first.B - second.B) < offset;
        }
        private List<Color> checkColor(Bitmap image)
        {
            List<Color> colors = new List<Color>();
            
            if (image.Width < 6 || image.Height < 6)
            {
               
                return colors;
            }
            Dictionary<Color, int> colorOccurrence = new Dictionary<Color, int>();

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pixel = image.GetPixel(i, j);
                    Color color = pixel;
                    if (colorOccurrence.ContainsKey(color))
                    {
                        colorOccurrence[color]++;
                    }
                    else
                    {
                        colorOccurrence.Add(color, 1);
                    }
                }
            }
            {
                int occurrence1 = 0;
                int occurrence2 = 0;
                {
                    List<Color> toDel = new List<Color>();
                    foreach (var item in colorOccurrence)
                    {
                        if (item.Value < 100)
                            toDel.Add(item.Key);
                    }
                    foreach(var id in toDel)
                    {
                        colorOccurrence.Remove(id);
                    }
                }

                colors.Add(colorOccurrence.ElementAt(0).Key);
                foreach (var item in colorOccurrence)
                {
                    if (item.Value > occurrence1)
                    {
                        
                        occurrence1 = item.Value;
                        colors[0] = item.Key;
                    }
                }
                colors.Add(colorOccurrence.ElementAt(0).Key);
                foreach (var item in colorOccurrence)
                {
                    if (item.Value > occurrence2 && !isCloseEnough(item.Key, colors[0]))
                    {
                        occurrence2 = item.Value;
                        colors[1] = item.Key;
                        isCloseEnough(item.Key, colors[0]);
                    }
                }
            }
            {
                Color temp = colors[0];
                colors[0] = colors[1];
                colors[1] = temp;
            }
            int fm = 0;
            int sm = 0;
            for(int i = image.Width/2 - 20; i < image.Width / 2 + 20; i++)
            {
                if (image.GetPixel(i, image.Height/2).ToArgb() == colors[0].ToArgb())
                {
                    fm++;
                }
                else if((image.GetPixel(i, image.Height / 2).ToArgb() == colors[1].ToArgb()))
                {
                    sm++;
                }
            }
                            
            if (fm < sm)
            {
                Color temp = colors[0];
                colors[0]             = colors[1];
                colors[1] = temp;
            }
            return colors;


        }

        private Bitmap createSample(Color main, Color background, int type, int size, int rotaction)
        {
            Bitmap imSamp;
            switch (type)
            {
                case 0:
                    imSamp = Properties.Resources._3;
                    break;
                case 3:
                    imSamp = Properties.Resources._6;
                    break;
                case 4:
                    imSamp = Properties.Resources._1;
                    break;
                default:
                    return null;
            }
            for (int i = 0; i < imSamp.Width; i++)
            {
                for (int j = 0; j < imSamp.Height; j++)
                {
                    Color pixel = imSamp.GetPixel(i, j);
                    if (pixel.R + pixel.G + pixel.B > 760)
                    {
                        imSamp.SetPixel(i, j, background);
                    }
                    else
                    {
                        imSamp.SetPixel(i, j, main);
                    }
                }
            }
            int sizeA = 0;
            for (int j = 0; j < imSamp.Height / 2; j++)
            {
                Color pixel = imSamp.GetPixel(imSamp.Width/2, j);
                if (pixel.ToArgb() == main.ToArgb())
                {
                    //pixel == main
                    //bool check1 = false;
                    int j2 = j;
                    int check2 = 0;
                    
                    while(j2 < j + 10)
                    {
                        if (imSamp.GetPixel(imSamp.Width / 2, j2).ToArgb() == main.ToArgb())
                        {
                            
                            check2++;
                        }
                        j2++;
                    }
                    if (check2 > 5)
                    {
                        sizeA = j;
                        break;
                    }
                    
                }
            }
            
            int newHigh = (imSamp.Height * size)/ sizeA;
            int newWidth = (imSamp.Width * size) / sizeA;
            imSamp = new Bitmap(imSamp, new Size(newWidth, newHigh));

            Rectangle rectangle = new Rectangle(0, 0, imSamp.Width, imSamp.Height);
            BitmapData bmpData = imSamp.LockBits(rectangle, System.Drawing.Imaging.ImageLockMode.ReadWrite, imSamp.PixelFormat);
            Image<Bgra, byte> imSampOCV = new Image<Bgra, byte>(imSamp.Width, imSamp.Height, bmpData.Stride, bmpData.Scan0);
            imSamp.UnlockBits(bmpData);
            imSampOCV = imSampOCV.Rotate(rotaction, new Bgra(background.B, background.G, background.R, background.A), true);
            //imSampOCVBM = 
            imSamp = imSampOCV.ToBitmap();

            return imSamp;
        }
        
        private int imageDifrence(Bitmap that, Bitmap sample, Color main)
        {
            int difrence = 0;
            if (sample.Width != that.Width || sample.Height != that.Height)
            {
                sample = new Bitmap(sample, new Size(that.Width, that.Height));
            }
            for (int i = 0; i < that.Width; i++)
            {
                for (int j = 0; j < that.Height; j++)
                {
                    Color pixel = that.GetPixel(i, j);
                    Color pixel2 = sample.GetPixel(i, j);
                    //if (pixel.ToArgb() != pixel2.ToArgb())
                    //{
                    //     difrence++;
                    //}
                    if (isCloseEnough(main, pixel) && !isCloseEnough(pixel, pixel2) || isCloseEnough(main, pixel2) && !isCloseEnough(pixel, pixel2))
                    {
                        difrence++;
                    }
                   
                }
            }
            return difrence;
        }

        private string recognizeShape(Bitmap that)
        {
            List<Color> colors = checkColor(that);
            if (isCloseEnough(colors[0], colors[1]))
            {
                for (int i = 0; i < that.Width; i++)
                {
                    for (int j = 0; j < that.Height; j++)
                    {
                        if (isCloseEnough(colors[0], that.GetPixel(i, j)))
                        {
                            that.SetPixel(i, j, Color.White);
                        }
                        else {
                            that.SetPixel(i, j, Color.Black);
                        }
                    }
                }
            }
            List<List<int>> mask = new List<List<int>>() { new List<int> { 1, 1, 1 }, new List<int> { 1, -1, 1 }, new List<int> { 1, 1, 1 } };
            int offset = (mask.Count / 2);
            for (int i = offset; i < that.Width - offset; i++)
            {
                for (int j = offset; j < that.Height - offset; j++)
                {
                    Color midPxl = that.GetPixel(i, j);
                    int oR = 0;
                    int oG = 0;
                    int oB = 0;
                    int K = 0;

                    for (int a = 0; a < mask.Count; a++)
                    {
                        for (int b = 0; b < mask.Count; b++)
                        {
                            int x = i - offset + a;
                            int y = j - offset + b;
                            if (x >= 0 && x < that.Width && y >= 0 && y < that.Height)
                            {
                                Color pxl = that.GetPixel(x, y);
                                oR += pxl.R * mask[a][b];
                                oG += pxl.G * mask[a][b];
                                oB += pxl.B * mask[a][b];
                                K += mask[a][b];
                            }
                        }
                    }
                    if (K == 0)
                        K = 1;
                    oR = oR / K;
                    oG = oG / K;
                    oB = oB / K;
                    if (oR > 255)
                    {
                        oR = 255;
                    }
                    if (oG > 255)
                    {
                        oG = 255;
                    }
                    if (oB > 255)
                    {
                        oB = 255;
                    }
                    that.SetPixel(i, j, Color.FromArgb(oR, oG, oB));
                }
            }
            pictureBox1.Image = that;
            pictureBox1.Refresh();
             colors = checkColor(that);
            List<List<List<int>>> shapeRotacionScale = new List<List<List<int>>>();
            shapeRotacionScale.Add(new List<List<int>>());
            shapeRotacionScale.Add(new List<List<int>>());
            shapeRotacionScale.Add(new List<List<int>>());
            //0 circle 3 triangle 4 squere 


            //shapeRotacionScale[0][0].Add(imageDifrence(that, createSample(colors[0], colors[1], 0, , 0)));

            int sizeA = 0;
            List<int> sizes = new List<int>();
            List<int> rotacions3 = new List<int>();
            rotacions3.Add(0);
            /*
            for(int i = 0; i < 70; i = i + 15)
            {
                rotacions3.Add(i);
            }
            */
            List<int> rotacions4 = new List<int>();
            /*
            for (int i = 0; i < 100; i = i + 15)
            {
                rotacions4.Add(i);
            }*/
            rotacions4.Add(0);
            for (int j = 0; j < that.Height / 2; j++)
            {
                Color pixel = that.GetPixel(that.Width / 2, j);
                if (isCloseEnough(pixel, colors[0]))
                {
                    //pixel == main
                    //bool check1 = false;
                    int j2 = j;
                    int check2 = 0;

                    while (j2 < j + 10)
                    {
                        if (isCloseEnough(that.GetPixel(that.Width / 2, j2), colors[0]))
                        {

                            check2++;
                        }
                        j2++;
                    }
                    if (check2 > 5)
                    {
                        sizeA = j;
                        break;
                    }

                }
            }
            sizes.Add(sizeA);
            /*for (int i = sizeA; i > 1; i = i - 30)
            {
                if (i != 0)
                {
                    sizes.Add(i);
                }
                
            }*/
            /*
            for (int i = sizeA; i < 3 * that.Height / 5; i = i + 30)
            {
                if (i != 0)
                {
                    sizes.Add(i);
                }
            }*/

            //0 circle
            
            shapeRotacionScale[0].Add(new List<int>());
            int iter = 0;
            for (int i = 0; i < sizes.Count; i++)
            {
                
                shapeRotacionScale[0][0].Add(imageDifrence(that, createSample(colors[0], colors[1], 0, sizes[i], 0), colors[0]));
                if(shapeRotacionScale[0][0][i] == shapeRotacionScale[0][0].Min())
                {
                    iter = i;
                }
            }
            pictureBox2.Image =  createSample(colors[0], colors[1], 0, sizes[iter], 0);
            //3 triangle
            //shapeRotacionScale[1].Add(new List<int>());
            int jter = 0;
            iter = 0;
            for (int j = 0; j < rotacions3.Count; j++)
            {
                shapeRotacionScale[1].Add(new List<int>());
                for (int i = 0; i < sizes.Count; i++)
                {
                    shapeRotacionScale[1][j].Add(imageDifrence(that, createSample(colors[0], colors[1], 3, sizes[i], rotacions3[j]), colors[0]));
                    if (shapeRotacionScale[0][0][i] == shapeRotacionScale[0][0].Min())
                    {
                        jter = j;
                        iter = i;
                    }
                }
                
            }
            pictureBox3.Image = createSample(colors[0], colors[1], 3, sizes[iter], rotacions3[jter]);
            //4 squere
            jter = 0;
            iter = 0;
            for (int j = 0; j < rotacions4.Count; j++)
            {
                shapeRotacionScale[2].Add(new List<int>());
                for (int i = 0; i < sizes.Count; i++)
                {
                    shapeRotacionScale[2][j].Add(imageDifrence(that, createSample(colors[0], colors[1], 4, sizes[i], rotacions4[j]), colors[0]));
                    if (shapeRotacionScale[0][0][i] == shapeRotacionScale[0][0].Min())
                    {
                        jter = j;
                        iter = i;
                    }
                }
            }
            pictureBox4.Image = createSample(colors[0], colors[1], 4, sizes[iter], rotacions4[jter]);

            int min = shapeRotacionScale[0][0][0];
            int shape = 0;
            for (int i = 0; i < shapeRotacionScale.Count; i++)
            {
                for (int j = 0; j < shapeRotacionScale[i].Count; j++)
                {
                    for (int k = 0; k < shapeRotacionScale[i][j].Count; k++)
                    {
                        if (shapeRotacionScale[i][j][k] < min)
                        {
                            min = shapeRotacionScale[i][j][k];
                            shape = i;
                        }
                    }
                }
            }

            switch(shape)
            {
                case 0:
                    return "circle";
                case 1:
                    return "triangle";
                case 2:
                    return "squere";
                default:
                    return "error";
            }


        }
        public Form1()
        {
            InitializeComponent();
            //imageOrginal0 = new Bitmap(imageOrginal0, new Size(pictureBox1.Width, pictureBox1.Height));
            pictureBox1.Image = imageOrginal0;

            //
            //pictureBox1.Image = createSample(checkColor(Properties.Resources._4)[0], checkColor(Properties.Resources._4)[1], 4);
            //pictureBox1.Image = createSample(Color.Black, Color.Yellow, 4 , 100, 45);
           // Color a = checkColor(Properties.Resources._4)[0];
           // Color b = checkColor(Properties.Resources._4)[1];
        }

        

        private void chooseFile1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            filePath1 = openFileDialog.FileName;
            textBox1.Text = filePath1;
        }

        private void loadButton1_Click_1(object sender, EventArgs e)
        {
            //imageOrginal0 = new Bitmap(Image.FromFile(filePath1), new Size(pictureBox1.Width, pictureBox1.Height));
            imageOrginal0 = new Bitmap(Image.FromFile(filePath1));
            pictureBox1.Image = imageOrginal0;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            label2.Text = "Working....";
            label2.Refresh();
            string abc = recognizeShape(imageOrginal0);
            label2.Text = abc;
        }
    }
}
