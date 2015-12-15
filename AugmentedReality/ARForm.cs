using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using OpenCvSharp;

namespace AugmentedReality
{
    public partial class ARForm : Form
    {
        private Thread _thread;
        private int _option;

        public ARForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartRecording();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
                _thread = null;
            }
        }

        private void StartRecording()
        {
            if (button1.Text.Equals("Start"))
            {
                _option = Int32.Parse(cmbOption.Text);
                _thread = new Thread(new ThreadStart(Run));
                _thread.Start();
                button1.Text = "Stop";
            }
            else
            {
                if (_thread != null && _thread.IsAlive)
                {
                    _thread.Abort();
                    _thread = null;
                }

                button1.Text = "Start";
            }
        }

        private void Run()
        {
            CvCapture cap = Cv.CreateCameraCapture(1);
            CvCapture vid = CvCapture.FromFile("trailer.avi");
            IplImage pic = new IplImage("pic.jpg", LoadMode.AnyColor | LoadMode.AnyDepth);

            Cv.Flip(pic, pic, FlipMode.Y);

            int b_width = 5;
            int b_height = 4;
            int b_squares = 20;
            CvSize b_size = new CvSize(b_width, b_height);

            CvMat warp_matrix = Cv.CreateMat(3, 3, MatrixType.F32C1);
            CvPoint2D32f[] corners = new CvPoint2D32f[b_squares];

            IplImage img;
            IplImage frame;
            IplImage disp;
            IplImage cpy_img;
            IplImage neg_img;

            int corner_count;

            while (_thread != null)
            {
                img = Cv.QueryFrame(cap);

                Cv.Flip(img, img, FlipMode.Y);

                disp = Cv.CreateImage(Cv.GetSize(img), BitDepth.U8, 3);
                cpy_img = Cv.CreateImage(Cv.GetSize(img), BitDepth.U8, 3);
                neg_img = Cv.CreateImage(Cv.GetSize(img), BitDepth.U8, 3);

                IplImage gray = Cv.CreateImage(Cv.GetSize(img), img.Depth, 1);
                bool found = Cv.FindChessboardCorners(img, b_size, out corners, out corner_count, ChessboardFlag.AdaptiveThresh | ChessboardFlag.FilterQuads);

                Cv.CvtColor(img, gray, ColorConversion.BgrToGray);

                CvTermCriteria criteria = new CvTermCriteria(CriteriaType.Epsilon, 30, 0.1);
                Cv.FindCornerSubPix(gray, corners, corner_count, new CvSize(11, 11), new CvSize(-1, -1), criteria);

                if (corner_count == b_squares)
                {
                    if (_option == 1)
                    {
                        CvPoint2D32f[] p = new CvPoint2D32f[4];
                        CvPoint2D32f[] q = new CvPoint2D32f[4];

                        IplImage blank = Cv.CreateImage(Cv.GetSize(pic), BitDepth.U8, 3);
                        Cv.Zero(blank);

                        q[0].X = (float)pic.Width * 0;
                        q[0].Y = (float)pic.Height * 0;
                        q[1].X = (float)pic.Width;
                        q[1].Y = (float)pic.Height * 0;

                        q[2].X = (float)pic.Width;
                        q[2].Y = (float)pic.Height;
                        q[3].X = (float)pic.Width * 0;
                        q[3].Y = (float)pic.Height;

                        p[0].X = corners[0].X;
                        p[0].Y = corners[0].Y;
                        p[1].X = corners[4].X;
                        p[1].Y = corners[4].Y;

                        p[2].X = corners[19].X;
                        p[2].Y = corners[19].Y;
                        p[3].X = corners[15].X;
                        p[3].Y = corners[15].Y;

                        Cv.GetPerspectiveTransform(q, p, out warp_matrix);

                        Cv.Zero(neg_img);
                        Cv.Zero(cpy_img);

                        Cv.WarpPerspective(pic, neg_img, warp_matrix);
                        Cv.WarpPerspective(blank, cpy_img, warp_matrix);
                        Cv.Not(cpy_img, cpy_img);

                        Cv.And(cpy_img, img, cpy_img);
                        Cv.Or(cpy_img, neg_img, img);

                        Cv.Flip(img, img, FlipMode.Y);
                        //Cv.ShowImage("video", img);
                        Bitmap bm = BitmapConverter.ToBitmap(img);
                        bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = bm;
                    }
                    else if (_option == 2)
                    {
                        CvPoint2D32f[] p = new CvPoint2D32f[4];
                        CvPoint2D32f[] q = new CvPoint2D32f[4];

                        frame = Cv.QueryFrame(vid);

                        Cv.Flip(frame, frame, FlipMode.Y);

                        IplImage blank = Cv.CreateImage(Cv.GetSize(frame), BitDepth.U8, 3);
                        Cv.Zero(blank);
                        Cv.Not(blank, blank);

                        q[0].X = (float)frame.Width * 0;
                        q[0].Y = (float)frame.Height * 0;
                        q[1].X = (float)frame.Width;
                        q[1].Y = (float)frame.Height * 0;

                        q[2].X = (float)frame.Width;
                        q[2].Y = (float)frame.Height;
                        q[3].X = (float)frame.Width * 0;
                        q[3].Y = (float)frame.Height;

                        p[0].X = corners[0].X;
                        p[0].Y = corners[0].Y;
                        p[1].X = corners[4].X;
                        p[1].Y = corners[4].Y;

                        p[2].X = corners[19].X;
                        p[2].Y = corners[19].Y;
                        p[3].X = corners[15].X;
                        p[3].Y = corners[15].Y;

                        Cv.GetPerspectiveTransform(q, p, out warp_matrix);

                        Cv.Zero(neg_img);
                        Cv.Zero(cpy_img);

                        Cv.WarpPerspective(frame, neg_img, warp_matrix);
                        Cv.WarpPerspective(blank, cpy_img, warp_matrix);
                        Cv.Not(cpy_img, cpy_img);

                        Cv.And(cpy_img, img, cpy_img);
                        Cv.Or(cpy_img, neg_img, img);

                        Cv.Flip(img, img, FlipMode.Y);
                        //Cv.ShowImage("video", img);
                        Bitmap bm = BitmapConverter.ToBitmap(img);
                        bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = bm;
                    }
                    else
                    {/*
                        CvPoint[] p = new CvPoint[4];

                        p[0].X = (int)corners[0].X;
                        p[0].Y = (int)corners[0].Y;
                        p[1].X = (int)corners[4].X;
                        p[1].Y = (int)corners[4].Y;

                        p[2].X = (int)corners[19].X;
                        p[2].Y = (int)corners[19].Y;
                        p[3].X = (int)corners[15].X;
                        p[3].Y = (int)corners[15].Y;

                        Cv.Line(img, p[0], p[1], CvColor.Red, 2);
                        Cv.Line(img, p[1], p[2], CvColor.Green, 2);
                        Cv.Line(img, p[2], p[3], CvColor.Blue, 2);
                        Cv.Line(img, p[3], p[0], CvColor.Yellow, 2);
                        */
                        //or
                        Cv.DrawChessboardCorners(img, b_size, corners, found);
                        Cv.Flip(img, img, FlipMode.Y);

                        //Cv.ShowImage("video", img);
                        Bitmap bm = BitmapConverter.ToBitmap(img);
                        bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = bm;
                    }
                }
                else
                {
                    Cv.Flip(gray, gray, FlipMode.Y);
                    //Cv.ShowImage("video", gray);
                    Bitmap bm = BitmapConverter.ToBitmap(gray);
                    bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                    pictureBox1.Image = bm;
                }
            }
        }
    }
}
