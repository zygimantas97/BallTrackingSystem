using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;


namespace BallTrackingSystem
{
    public partial class Form1 : Form
    {
        int errindex = 0;
        Timer timer;
        GameZone gameZone;
        private List<MyPoint> selectedPoints = new List<MyPoint>();
        string fileName;
        string imageName="background.jpg";
        // initial min and max HSV filter values
        int H_MIN = 94;
        int H_MAX = 118;
        int S_MIN = 100;
        int S_MAX = 250;
        int V_MIN = 98;
        int V_MAX = 255;

        int ADDEDHEIGHT = 0;

        const int MIN_BALL_SIZE = 700;
        const int MAX_BALL_SIZE = 1800;

        //capture width and hight
        const int FRAME_WIDTH = 640;
        const int FRAME_HEIGHT = 480;
        
        // max number of objects to be detected
        const int MAX_NUM_OBJECTS = 10;

        // minimum and maximum object area
        const int MIN_OBJECT_AREA = 20 * 20;
        const int MAX_OBJECT_AREA = (int)((double)FRAME_HEIGHT * FRAME_WIDTH / 1.5);

        int Erode = 3;
        int Dilate = 3;
        int DrawErode = 3;
        int DrawDilate = 8;
        bool ShowSettings = false;

        // names of windows
        const string windowName = "Original image";
        const string windowName1 = "HSV image";
        const string windowName2 = "Thresholded image";
        const string windowName3 = "After morphological operations";
        const string trackBarWindowName = "Trackbars";
        static VideoCapture capture;

        MyVector[] DrawnVectors = new MyVector[3];
        int HowManyDrawn = 0;
        bool StartToRemove = false;
        int VectorCountLimit = 3;

        
        bool trackObjects;
        bool useMorphOps;
        Mat cameraFeed;
        Mat HSV;
        Mat treshold;
        Mat transformedFeed;
        Mat beforeTransformationFeed;
        int x;
        int y;
        VideoCapture videoCapture;
        VideoCapture imageCapture;

        public Form1()
        {
            InitializeComponent();
        }
        

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void timer_Tick(object sender, EventArgs e)
        {

            // store image to matrix
            
            videoCapture.Read(cameraFeed);
           

            // convert frame from RGB to HSV colorspace
            CvInvoke.CvtColor(cameraFeed, HSV, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
            

            //H_MIN = trackBar1.Value;
            //label1.Text = "H_MIN" + H_MIN;
            //H_MAX = trackBar2.Value;
            //label2.Text = "H_MAX" + H_MAX;
            //S_MIN = trackBar3.Value;
            //label3.Text = "S_MIN" + S_MIN;
            //S_MAX = trackBar4.Value;
           // label4.Text = "S_MAX" + S_MAX;
            //V_MIN = trackBar5.Value;
           // label5.Text = "V_MIN" + V_MIN;
            //V_MAX = trackBar6.Value;
           // label6.Text = "V_MAX" + V_MAX;
            // filter HSV image between values
            CvInvoke.InRange(HSV, new ScalarArray(new MCvScalar(H_MIN, S_MIN, V_MIN)), new ScalarArray(new MCvScalar(H_MAX, S_MAX, V_MAX)), treshold);
            
            // eliminate noise
            if (useMorphOps)
            {
                MorphOps(treshold);
            }

            if(ShowSettings==true)
            {
                UpdateSettingsTrackBar();
            }
            
            // get coordinates of tracking object
            VectorOfVectorOfPoint points;
            if (trackObjects)
            {
                Contours(treshold, cameraFeed,  transformedFeed, out points);
            }


            imageBox1.Image = cameraFeed.ToImage<Bgr, byte>();
            //show frames
            

            if (checkBox2.Checked)
            {
                imageBox5.Image = beforeTransformationFeed.ToImage<Bgr, byte>();
                imageBox5.Show();
            }
            else imageBox5.Hide();

            if(checkBox3.Checked)
            {
                imageBox3.Image= HSV.ToImage<Bgr, byte>();
                imageBox6.Image= treshold.ToImage<Bgr, byte>();
                imageBox3.Show();
                imageBox6.Show();
            }
            else
            {
                imageBox3.Hide();
                imageBox6.Hide();
            }
           
        }
        
        
        private void MorphOps(Mat threshold)
        {
            Mat erodeElement = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(DrawErode, DrawErode), new System.Drawing.Point(-1, -1));
            Mat dilateElement = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(DrawDilate, DrawDilate), new System.Drawing.Point(-1, -1));
            
            CvInvoke.Erode(threshold, threshold, erodeElement, new Point(-1, -1), Erode, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(255, 255, 255));
            CvInvoke.Dilate(threshold, threshold, dilateElement, new Point(-1, -1), Dilate, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(255, 255, 255));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
            }
            
            videoCapture = new VideoCapture(fileName);
            cameraFeed = new Mat();
            videoCapture.Read(cameraFeed);
            Image<Bgr, byte> img = cameraFeed.ToImage<Bgr, byte>();
            imageBox4.Image = img;
            
            label7.Text = "GENERAL INFO\n"+"Matrica: " + cameraFeed.Width + "x" + cameraFeed.Height;
            label7.Text += "\nImageBox: " + imageBox4.Width + "x" + imageBox4.Height;
            label7.Text += "\n" + string.Format("{0}: {1:0.00}x{2:0.00}", "Koeficientai", cameraFeed.Width / imageBox4.Width, cameraFeed.Height / imageBox4.Height);
            panel2.Visible = false;
            panel3.Visible = true;
            
            
            
        }
        private void Start()
        {
            
            selectedPoints = gameZone.GetPoints();
            label8.Text += "\n" + selectedPoints[0].X + ";" + selectedPoints[0].Y;
            label8.Text += "     " + +selectedPoints[1].X + ";" + selectedPoints[1].Y;
            label8.Text += "\n" + selectedPoints[2].X + ";" + selectedPoints[2].Y;
            label8.Text += "     " + +selectedPoints[3].X + ";" + selectedPoints[3].Y;
            label17.Text = "How many panels to show?";
            trackObjects = true;
            useMorphOps = true;

            transformedFeed = new Mat();
            beforeTransformationFeed = new Mat();
            
            videoCapture = new VideoCapture(fileName);
            imageCapture = new VideoCapture(imageName);
            // matrix to store each frame of the webcam feed
            cameraFeed = new Mat();
            // matrix storage for HSV image
            HSV = new Mat();
            // matrix storage for binary threshold image
            treshold = new Mat();
            
            // x and y values for the location of the object
            x = 0;
            y = 0;
            //create slider bars for HSV filtering
            //createTrackBars();
            // video capture object to acquire webcam feed

            // set weight width and height of capture frame
            videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, FRAME_WIDTH);
            videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, FRAME_HEIGHT);
            imageCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, FRAME_WIDTH);
            imageCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, FRAME_HEIGHT);
            //MessageBox.Show(selectedPoints[0].Y.ToString() + " " + selectedPoints[1].Y.ToString());
            
            /*
            imageBox1.Image = cameraFeed.ToImage<Bgr, byte>();
            */

            imageCapture.Read(transformedFeed);
            imageCapture = new VideoCapture(imageName);
            imageCapture.Read(beforeTransformationFeed);
            DrawOriginalArena();
            DrawArena();

            imageBox2.Image = transformedFeed.ToImage<Bgr, byte>();


            timer = new Timer();
            timer.Interval = 1;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            
        }
        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void imageBox4_Click(object sender, EventArgs e)
        {

            
        }
        private void DrawPoint(int x, int y, Mat frame)
        {
            Graphics g = Graphics.FromHwnd(imageBox4.Handle);
            SolidBrush brush = new SolidBrush(Color.White);
            Point dPoint = new Point(x, y);
            dPoint.X = dPoint.X - 10;
            dPoint.Y = dPoint.Y - 10;
            Rectangle rect = new Rectangle(dPoint, new Size(20, 20));
            g.FillRectangle(brush, rect);
            g.Dispose();


            //use some of the openCV drawing functions to draw crosshairs
            //on your tracked image!

            //UPDATE:JUNE 18TH, 2013
            //added 'if' and 'else' statements to prevent
            //memory errors from writing off the screen(ie. (-25,-25) is not within the window!)

            //CvInvoke.Circle(frame, new Point(x, y), 20, new MCvScalar(0, 255, 0), 2);
            //if (y - 25 > 0)
            //    CvInvoke.Line(frame, new Point(x, y), new Point(x, y - 25), new MCvScalar(0, 255, 0), 2);
            //else CvInvoke.Line(frame, new Point(x, y), new Point(x, 0), new MCvScalar(0, 255, 0), 2);
            //if (y + 25 < FRAME_HEIGHT)
            //    CvInvoke.Line(frame, new Point(x, y), new Point(x, y + 25), new MCvScalar(0, 255, 0), 2);
            //else CvInvoke.Line(frame, new Point(x, y), new Point(x, FRAME_HEIGHT), new MCvScalar(0, 255, 0), 2);
            //if (x - 25 > 0)
            //    CvInvoke.Line(frame, new Point(x, y), new Point(x - 25, y), new MCvScalar(0, 255, 0), 2);
            //else CvInvoke.Line(frame, new Point(x, y), new Point(0, y), new MCvScalar(0, 255, 0), 2);
            //if (x + 25 < FRAME_WIDTH)
            //    CvInvoke.Line(frame, new Point(x, y), new Point(x + 25, y), new MCvScalar(0, 255, 0), 2);
            //else CvInvoke.Line(frame, new Point(x, y), new Point(FRAME_WIDTH, y), new MCvScalar(0, 255, 0), 2);

            //CvInvoke.PutText(frame, x.ToString() + "," + y.ToString(), new Point(x, y + 30), Emgu.CV.CvEnum.FontFace.HersheySimplex, 1, new MCvScalar(0, 255, 0), 2);

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (selectedPoints.Count == 4)
            {
                panel2.Visible = false;
                panel3.Visible = false;
                panel1.Visible = true;
                gameZone = new GameZone(selectedPoints);
                
                //Test();
                Start();
            }
        }

        private void Test()
        {
            MyPoint tempPoint = new MyPoint(900, 3000); 
            MyPoint point1 = new MyPoint(750, 2600);
            MyPoint point2 = new MyPoint(500, 3100);
            MyPoint point3 = new MyPoint(1250, 2600);
            MyPoint point4 = new MyPoint(1500, 3100);
            selectedPoints = new List<MyPoint>();
            selectedPoints.Add(point1);
            selectedPoints.Add(point2);
            selectedPoints.Add(point3);
            selectedPoints.Add(point4);
            gameZone = new GameZone(selectedPoints);
            selectedPoints = gameZone.GetPoints();

            transformedFeed = new Mat();


            
            imageCapture = new VideoCapture(imageName);
            imageCapture.Read(transformedFeed);
            MyPoint b;

            //Zymime netransformuotas kampu koordinates
            for (int i = 0; i <= 3; i++)
            {
                
                b = selectedPoints[i];
                CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString(), new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
                 /*           
                b = gameZone.videoOXY2gameZoneOXY(b);
                CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString() + "new", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
              */
            }
            
            /*
            b = gameZone.topMiddle;
            CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString() + "topMidle", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
            CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            */
            

            //Piešiame netranformuotos figuros konturus
            for (int i = gameZone.Points[0].X; i <= gameZone.Points[1].X; i++)
            {
                b = new MyPoint(i, (int)gameZone.topRotated.GetY(i));
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }
            for (int i = gameZone.Points[2].X; i <= gameZone.Points[3].X; i++)
            {
                b = new MyPoint(i, (int)gameZone.bottomRotated.GetY(i));
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }
            for (int i = gameZone.Points[2].X; i <= gameZone.Points[0].X; i++)
            {
                b = new MyPoint(i, (int)gameZone.leftRotated.GetY(i));
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }
            for (int i = gameZone.Points[1].X; i <= gameZone.Points[3].X; i++)
            {
                b = new MyPoint(i, (int)gameZone.rightRotated.GetY(i));
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }

            b = tempPoint;
            CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString() + " e", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
            CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);

            //Transformuojame tempPoint (testinio tasko) koordinates
            b = gameZone.videoOXY2gameZoneOXY(tempPoint, true);
            CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString() + " e", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
            CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);

            //Transformuojame kampu koordinates
            for (int i = 0; i < 4; i++)
            {
                
                b = gameZone.videoOXY2gameZoneOXY(gameZone.Points[i], false);
                CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString() + "t", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }

            //Piesiame transformuotos figuros kontura
            for(int i = gameZone.Points[0].X; i <= gameZone.Points[1].X; i++)
            {
                b = new MyPoint(i, gameZone.Points[0].Y);
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }
            for (int i = gameZone.Points[2].X; i <= gameZone.Points[3].X; i++)
            {
                b = new MyPoint(i, gameZone.Points[2].Y);
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }
            for (int i = gameZone.Points[0].Y; i <= gameZone.Points[2].Y; i++)
            {
                b = new MyPoint(gameZone.Points[0].X, i);
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }
            for (int i = gameZone.Points[1].Y; i <= gameZone.Points[3].Y; i++)
            {
                b = new MyPoint(gameZone.Points[1].X, i);
                CvInvoke.PutText(transformedFeed, "", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
            }

            imageBox2.Image = transformedFeed.ToImage<Bgr, byte>();
            //label8.Text = "SIN: " + gameZone.sinA + "\n" + "COS: " + gameZone.cosA;
            label8.Text = transformedFeed.Width + "x" + transformedFeed.Height;

        }

        private void DrawArena ()
        {
            MyPoint b;
            //Transformuojame kampu koordinates
            for (int i = 0; i < 4; i++)
            {

                b = gameZone.videoOXY2gameZoneOXY(gameZone.Points[i], false);
                CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString() + "t", new Point(b.X, b.Y + 100 + ADDEDHEIGHT), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y+ADDEDHEIGHT), 5, new MCvScalar(15, 15, 0), 20);
            }
            
            CvInvoke.Line(transformedFeed, new Point(100, 800 + ADDEDHEIGHT), new Point(1900, 800 + ADDEDHEIGHT), new MCvScalar(255, 160, 160), 10);
            CvInvoke.Line(transformedFeed, new Point(100, 1400 + ADDEDHEIGHT), new Point(1900, 1400 + ADDEDHEIGHT), new MCvScalar(255, 160, 160), 10);
            CvInvoke.Line(transformedFeed, new Point(100, 2000 + ADDEDHEIGHT), new Point(1900, 2000 + ADDEDHEIGHT), new MCvScalar(255, 160, 160), 10);
            CvInvoke.Line(transformedFeed, new Point(100, 2600 + ADDEDHEIGHT), new Point(1900, 2600 + ADDEDHEIGHT), new MCvScalar(255, 160, 160), 10);
            CvInvoke.Line(transformedFeed, new Point(100, 3200 + ADDEDHEIGHT), new Point(1900, 3200 + ADDEDHEIGHT), new MCvScalar(255, 160, 160), 10);

            CvInvoke.Line(transformedFeed, new Point(gameZone.Points[0].X, gameZone.Points[0].Y + ADDEDHEIGHT), new Point(gameZone.Points[2].X, gameZone.Points[2].Y + ADDEDHEIGHT), new MCvScalar(255, 0, 0), 10);
            CvInvoke.Line(transformedFeed, new Point(gameZone.Points[2].X, gameZone.Points[2].Y + ADDEDHEIGHT), new Point(gameZone.Points[3].X, gameZone.Points[3].Y + ADDEDHEIGHT), new MCvScalar(255, 0, 0), 10);
            CvInvoke.Line(transformedFeed, new Point(gameZone.Points[0].X, gameZone.Points[0].Y + ADDEDHEIGHT), new Point(gameZone.Points[1].X, gameZone.Points[1].Y + ADDEDHEIGHT), new MCvScalar(255, 0, 0), 10);
            CvInvoke.Line(transformedFeed, new Point(gameZone.Points[3].X, gameZone.Points[3].Y + ADDEDHEIGHT), new Point(gameZone.Points[1].X, gameZone.Points[1].Y + ADDEDHEIGHT), new MCvScalar(255, 0, 0), 10);
            for(int i = 0; i < GameZone.zones.Length; i++)
            {
                CvInvoke.Line(transformedFeed, new Point(100 + GameZone.zones[i] * 2, 200), new Point(100 + GameZone.zones[i] * 2, 800), new MCvScalar(255, 0, 0), 10);
                CvInvoke.Line(transformedFeed, new Point(100 + GameZone.zones[i] * 2, 3200), new Point(100 + GameZone.zones[i] * 2, 3800), new MCvScalar(255, 0, 0), 10);

            }
            
            renewZones();
            
        }
        private void renewZones()
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel2.Controls.Clear();
            for (int i = 0; i <= GameZone.zones.Length; i++)
            {


                Label tempLabel1 = new Label();
                Label tempLabel2 = new Label();
                Label tempLabel3 = new Label();
                Label tempLabel4 = new Label();
                tempLabel1.Text = gameZone.topOut[i].ToString();
                tempLabel2.Text = gameZone.topIn[i].ToString();
                tempLabel3.Text = gameZone.bottomIn[i].ToString();
                tempLabel4.Text = gameZone.bottomOut[i].ToString();
                tableLayoutPanel1.Controls.Add(tempLabel1, i, 1);
                tableLayoutPanel1.Controls.Add(tempLabel2, i, 2);
                tableLayoutPanel2.Controls.Add(tempLabel3, i, 1);
                tableLayoutPanel2.Controls.Add(tempLabel4, i, 2);

            }
        }
        private void DrawLine(MyPoint point1, MyPoint point2, MCvScalar color)
        {
            CvInvoke.Line(transformedFeed, new Point(point1.X, point1.Y + ADDEDHEIGHT), new Point(point2.X, point2.Y + ADDEDHEIGHT), color, 10);
           // CvInvoke.Circle(transformedFeed, new Point(point2.X, point2.Y), 5, color, 100);
            imageBox2.Image = transformedFeed.ToImage<Bgr, byte>();
        }
        private void EraseLine(MyPoint point1, MyPoint point2)
        {
            CvInvoke.Line(transformedFeed, new Point(point1.X, point1.Y + ADDEDHEIGHT), new Point(point2.X, point2.Y + ADDEDHEIGHT), new MCvScalar(255, 255, 255), 10);
           // CvInvoke.Circle(transformedFeed, new Point(point2.X, point2.Y), 5, new MCvScalar(255, 255, 255), 100);
            DrawArena();
        }
        private void DrawOriginalArena ()
        {
            MyPoint b;
            //Zymime netransformuotas kampu koordinates
            for (int i = 0; i <= 3; i++)
            {
                b = selectedPoints[i];
                CvInvoke.PutText(beforeTransformationFeed, b.X.ToString() + "," + b.Y.ToString(), new Point(b.X, b.Y + 100+ADDEDHEIGHT), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
               // CvInvoke.Circle(beforeTransformationFeed, new Point(b.X, b.Y + ADDEDHEIGHT), 5, new MCvScalar(15, 15, 0), 20);
               
            }
            CvInvoke.Line(beforeTransformationFeed, new Point(gameZone.Points[0].X, gameZone.Points[0].Y+ ADDEDHEIGHT), new Point(gameZone.Points[2].X, gameZone.Points[2].Y + ADDEDHEIGHT), new MCvScalar(77, 77, 0), 10);
            CvInvoke.Line(beforeTransformationFeed, new Point(gameZone.Points[2].X, gameZone.Points[2].Y+ ADDEDHEIGHT), new Point(gameZone.Points[3].X, gameZone.Points[3].Y + ADDEDHEIGHT), new MCvScalar(77, 77, 0), 10);
            CvInvoke.Line(beforeTransformationFeed, new Point(gameZone.Points[0].X, gameZone.Points[0].Y+ ADDEDHEIGHT), new Point(gameZone.Points[1].X, gameZone.Points[1].Y + ADDEDHEIGHT), new MCvScalar(77, 77, 0), 10);
            CvInvoke.Line(beforeTransformationFeed, new Point(gameZone.Points[3].X, gameZone.Points[3].Y+ ADDEDHEIGHT), new Point(gameZone.Points[1].X, gameZone.Points[1].Y + ADDEDHEIGHT), new MCvScalar(77, 77, 0), 10);
            
        }


        private void imageBox4_Click_1(object sender, EventArgs e)
        {
            if(selectedPoints.Count < 4)
            {
                MouseEventArgs me = (MouseEventArgs)e;
                Point coordinates = me.Location;                
                DrawPoint(coordinates.X, coordinates.Y, cameraFeed);
                coordinates.X = coordinates.X * (cameraFeed.Width / imageBox4.Width);
                coordinates.Y = coordinates.Y * (cameraFeed.Height / imageBox4.Height);
                selectedPoints.Add(new MyPoint(coordinates.X, coordinates.Y));

                if(selectedPoints.Count==1)
                {
                    label7.Text = "FIRST COORDINATE   "+"X=" + coordinates.X + " Y=" + coordinates.Y;
                }
                if (selectedPoints.Count == 2)
                {
                    label11.Text = "SECOND COORDINATE   " + "X=" + coordinates.X + " Y=" + coordinates.Y;
                    label11.Show();
                }
                if (selectedPoints.Count == 3)
                {
                    label12.Text = "THIRD COORDINATE   " + "X=" + coordinates.X + " Y=" + coordinates.Y;
                    label12.Show();
                }
                if (selectedPoints.Count == 4)
                {
                    label13.Text = "FOURTH COORDINATE   " + "X=" + coordinates.X + " Y=" + coordinates.Y;
                    label13.Show();
                    button1.Show();
                }

            }
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        
        public void Contours(Mat threshold, Mat cameraFeed, Mat transformedFeed, out VectorOfVectorOfPoint acceptableContours)
        {
            Mat temp = new Mat();

            threshold.CopyTo(temp);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            acceptableContours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();

            CvInvoke.FindContours(temp, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            // CvInvoke.DrawContours(cameraFeed, contours, -1, new MCvScalar(0, 0, 255), 5); //nupiešia visus kontūrus, (-1 reiškia, kad peišti visus)

            Dictionary<int, double> dict = new Dictionary<int, double>();
            if (contours.Size > 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    double area = CvInvoke.ContourArea(contours[i]);
                    if (area < MAX_BALL_SIZE && area > MIN_BALL_SIZE)
                    {
                        if(gameZone.IsPointSuitable(new MyPoint(contours[i][0].X, contours[i][0].Y)))
                        {
                            //label8.Text = "\n" + contours[i][0].X + " " + contours[i][0].Y;
                            dict.Add(i, area);
                            acceptableContours.Push(contours[i]);
                            //label8.Text += gameZone.IsPointSuitable(new MyPoint(1000, 500));
                        }
                        
                    }
                }
            }
            var item = dict.OrderBy(v => v.Value);


            foreach (var it in item)
            {
                int key = int.Parse(it.Key.ToString());


                Point a = new Point(contours[key][0].X, contours[key][0].Y); //kampinis taškas iš kontūrų
                
                CvInvoke.PutText(cameraFeed, a.X.ToString() + "," + a.Y.ToString() + " SIZE:" + CvInvoke.ContourArea(contours[key]), new Point(a.X, a.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);

                
                Point b = gameZone.videoOXY2gameZoneOXY(new MyPoint(a.X, a.Y), true).ConvertoToDrawingPoint(); //transformuotas taškas
                gameZone.AddPointToVectors(new MyPoint(b.X, b.Y));

                for(int i = 0; i < gameZone.completedVectors.Count; i++)
                {
                    //DrawLine(new MyPoint(
                    //    gameZone.completedVectors.First<MyVector>().pointsOfVector.First<MyPoint>().X,
                    //    gameZone.completedVectors.First<MyVector>().pointsOfVector.First<MyPoint>().Y
                    //    ), new MyPoint(
                    //    gameZone.completedVectors.First<MyVector>().pointsOfVector.Last<MyPoint>().X,
                    //    gameZone.completedVectors.First<MyVector>().pointsOfVector.Last<MyPoint>().Y
                    //    ));

                    DrawLine(gameZone.completedVectors.First<MyVector>().getBeginPoint(),
                        gameZone.completedVectors.First<MyVector>().getEndPoint(), new MCvScalar(0, 0, 255)); //draw a vector
                    renewZones();

                    if (StartToRemove == true) // jeigu atvaizduota pakankamai tasku, istrinti seniausia ir atlaisvinti vieta sekanciam
                    {
                        EraseLine(DrawnVectors[HowManyDrawn].getBeginPoint(), DrawnVectors[HowManyDrawn].getEndPoint());

                    }

                    DrawnVectors[HowManyDrawn] = gameZone.completedVectors.First<MyVector>();  //dedame vaktoriu i masyva
                    gameZone.completedVectors.RemoveAt(0);

                    if (HowManyDrawn == 0) //jeigu nupiestas 1 vektorius
                    {


                        if (DrawnVectors[2] != null)
                        {
                            DrawLine(DrawnVectors[2].getBeginPoint(), DrawnVectors[2].getEndPoint(), new MCvScalar(150, 150, 255));
                            DrawLine(DrawnVectors[1].getBeginPoint(), DrawnVectors[1].getEndPoint(), new MCvScalar(220, 220, 255));
                        }
                        else if (DrawnVectors[1] != null)
                        {
                            DrawLine(DrawnVectors[1].getBeginPoint(), DrawnVectors[1].getEndPoint(), new MCvScalar(220, 220, 255));
                        }
                    }
                    else if (HowManyDrawn == 1) //jeigu nupiesti 2 vektoriai
                    {
                        if (DrawnVectors[2] != null)
                        {
                            DrawLine(DrawnVectors[0].getBeginPoint(), DrawnVectors[0].getEndPoint(), new MCvScalar(150, 150, 255));
                            DrawLine(DrawnVectors[2].getBeginPoint(), DrawnVectors[2].getEndPoint(), new MCvScalar(220, 220, 255));
                        }
                        else
                        {
                            DrawLine(DrawnVectors[0].getBeginPoint(), DrawnVectors[0].getEndPoint(), new MCvScalar(150, 150, 255));
                        }
                    }

                    else if (HowManyDrawn == 2) //jeigu nupiesti 3 vektoriai
                    {
                        DrawLine(DrawnVectors[1].getBeginPoint(), DrawnVectors[1].getEndPoint(), new MCvScalar(150, 150, 255));
                        DrawLine(DrawnVectors[0].getBeginPoint(), DrawnVectors[0].getEndPoint(), new MCvScalar(220, 220, 255));
                    }
                    

                    HowManyDrawn++;
                    
                    if (HowManyDrawn==VectorCountLimit) //patikriname, ar virsijo limita ir reikia pradeti trinti vektorius
                    { 
                        //label16.Text = "AKTYVAVO " + HowManyDrawn;
                        StartToRemove = true;
                        HowManyDrawn = 0;

                    }
                   
            }

                
                label2.Text = "Remaining: " + gameZone.remainingVectors.Count + "\n" + "Finished : " + gameZone.completedVectors.Count;
                //CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString(), new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
               // CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y+ADDEDHEIGHT), 5, new MCvScalar(15, 15, 0), 20);

                

                CvInvoke.Circle(beforeTransformationFeed, new Point(a.X, a.Y + ADDEDHEIGHT), 5, new MCvScalar(255, 15, 0), 13);
                //CvInvoke.Circle(transformedFeed, new Point(a.X, a.Y), 5, new MCvScalar(255, 15, 0), 10);
                //5 žymi linijos storumą
                Rectangle rect = CvInvoke.BoundingRectangle(contours[key]);
                CvInvoke.Rectangle(cameraFeed, rect, new MCvScalar(0, 0, 255), 5); //5 žymi linijos storumą
                //CvInvoke.DrawContours(cameraFeed, contours, key, new MCvScalar(0, 0, 255), 5);
            }
        }
        private void button3_Click_1(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            label9.Visible = true;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            label3.Text = "Current MIN BALL SIZE: " + MIN_BALL_SIZE.ToString();
            label4.Text = "Current MAX BALL SIZE: " + MAX_BALL_SIZE.ToString();
            label5.Text = "ERODE: " + Erode + " Dilate: " + Dilate + " DrawErode: " + DrawErode + " DrawDilate: " + DrawDilate;
            textBox1.Text = MIN_BALL_SIZE.ToString();
            textBox2.Text = MAX_BALL_SIZE.ToString();
            textBox3.Text = Erode.ToString();
            textBox4.Text = Dilate.ToString();
            textBox5.Text = DrawErode.ToString();
            textBox6.Text = DrawDilate.ToString();

            label6.Text = "HUE -  " + H_MIN + " / " + H_MAX;
            label14.Text = "SAT -  " + S_MIN + " / " + S_MAX;
            label15.Text = "VAL -  " + V_MIN + " / " + V_MAX;

            trackBar1.Value = H_MIN;
            trackBar2.Value = H_MAX;
            trackBar3.Value = S_MIN;
            trackBar4.Value = S_MAX;
            trackBar5.Value = V_MIN;
            trackBar6.Value = V_MAX;




            if (ShowSettings == true)
            {
                ShowSettings = false;
                settingsPanel.Hide();
                settingsPanel.SendToBack();
            }
            else if (ShowSettings == false)
            {
                ShowSettings = true;
                settingsPanel.Show();
                settingsPanel.BringToFront();
            }
        }


        private void UpdateSettingsTrackBar()
        {

            label6.Text = "HUE -  " + H_MIN + " / " + H_MAX;
            label14.Text = "SAT -  " + S_MIN + " / " + S_MAX;
            label15.Text = "VAL -  " + V_MIN + " / " + V_MAX;

            H_MIN = trackBar1.Value;
            H_MAX = trackBar2.Value;
            S_MIN = trackBar3.Value;
            S_MAX = trackBar4.Value;
            V_MIN = trackBar5.Value;
            V_MAX = trackBar6.Value;
        }

        private void button7_Click_1(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            VectorCountLimit = Convert.ToInt32(textBox7.Text);
            
        }
    }
}
