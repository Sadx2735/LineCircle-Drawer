using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sparkle
{
    public enum Shape
    {
        Line,
        Circle
    }

    public partial class MainWindow : Window
    {

        public nint pbase;
        public int stride;
        public Shape scond = Shape.Line;
        public WriteableBitmap bitmap;

        public List<(int x, int y)> Lps = new List<(int x, int y)>();
        public List<(double x, double y)> Cps = new List<(double x, double y)>();

        public MainWindow()
        {

            InitializeComponent();
            this.MouseLeftButtonDown += DrawShape;
            this.KeyDown += Keypos;

            bitmap = new WriteableBitmap(800, 600, 96, 96, PixelFormats.Gray8, null);
            img.Source = bitmap;
        }

        void DrawShape(object sender, MouseButtonEventArgs e)
        {
            if (scond == Shape.Line) { DrawLine(sender, e); }
            if (scond == Shape.Circle) { DrawCircle(sender, e); }
        }
        void Keypos(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C)
            {
                scond = Shape.Circle;
            }

            if (e.Key == Key.L)
            {
                scond = Shape.Line;
            }

            if (e.Key == Key.E)
            {
                stride = bitmap.BackBufferStride;

                try
                {
                    bitmap.Lock();
                    pbase = bitmap.BackBuffer;
                    for (int x = 0; x < bitmap.PixelWidth; x++)
                    {
                        for (int y = 0; y < bitmap.PixelHeight; y++)
                        {

                            SetPixel(x, y, 0);
                        }
                    }
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                }

                finally
                {
                    bitmap.Unlock();
                    Cps.Clear();
                }

            }

        }


        void DrawCircle(object sender, MouseButtonEventArgs e)
        {
            Lps.Clear();
            var p = e.GetPosition(img);
            Cps.Add((p.X, p.Y));

            if (Cps.Count == 2)
            {
 
                stride = bitmap.BackBufferStride;

                try
                {

                    bitmap.Lock();
                    pbase = bitmap.BackBuffer;

                    double ysq = Math.Pow(Math.Abs(Cps[1].y - Cps[0].y), 2);
                    double xsq = Math.Pow(Math.Abs(Cps[1].x - Cps[0].x), 2);
                    double R = Math.Sqrt(ysq+xsq);

                    SetPixel((int)Math.Round(Cps[0].x,0), (int)Math.Round(Cps[0].y,0), 255);
                    for (double degree = 0; degree <= 360; degree += 0.25)
                    {
                        double rad = degree * Math.PI / 180;
                        int x = (int)Math.Round((Cps[0].x + R * Math.Cos(rad)));
                        int y = (int)Math.Round((Cps[0].y - R * Math.Sin(rad)));
                        SetPixel(x, y, 255);
                    }
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                }

                finally
                {
                    bitmap.Unlock();
                    Cps.Clear();
                }
            }
        }

        void DrawLine(object sender, MouseButtonEventArgs e)
        {

            Cps.Clear();
            var p = e.GetPosition(img);
            Lps.Add(((int)p.X, (int)p.Y));

            if (Lps.Count == 2)
            {
                stride = bitmap.BackBufferStride;

                try
                {
                    bitmap.Lock();
                    pbase = bitmap.BackBuffer;

                    int dx = Lps[1].x - Lps[0].x;
                    int dy = Lps[1].y - Lps[0].y;

                    double m = dy / (dx + 1e-15);
                    double c = Lps[0].y - m * Lps[0].x;

                    if (Math.Abs(dx) < Math.Abs(dy))
                    {
                        int step = 1;
                        if (dy < 0){step = -1;}

                        int xp;
                        for (int yp = Lps[0].y; yp != Lps[1].y; yp += step) 
                        {
                            xp = (int)((yp - c) / m);
                            SetPixel(xp, yp, 255);
                        }
                    }

                    else
                    {
                        int step = 1;
                        if (dx < 0)
                        {
                            step = -1;
                        }


                        int yp;
                        for (int xp = Lps[0].x; xp != Lps[1].x; xp += step)
                        {
                            yp = (int)(m * xp + c);
                            SetPixel(xp, yp, 255);
                        }

                    }

                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));

                }

                finally
                {
                    bitmap.Unlock();
                    Lps.Clear();
                }
            }
        }
        void SetPixel(int xp, int yp, byte color)
        {
            if (xp >= 0 && xp < bitmap.PixelWidth && yp >= 0 && yp < bitmap.PixelHeight)
            {
                unsafe
                {
                    var ptr = (byte*)pbase + (yp * stride) + xp;
                    *ptr = color;
                }
            }

        }

    }

}