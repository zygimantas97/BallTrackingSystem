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
        Timer timer;
        GameZone gameZone;
        private List<MyPoint> selectedPoints = new List<MyPoint>();
        string fileName;
        string imageName="background.jpg";
        // initial min and max HSV filter values
        int H_MIN = 102;
        int H_MAX = 112;
        int S_MIN = 100;
        int S_MAX = 250;
        int V_MIN = 98;
        int V_MAX = 255;

        const int MIN_BALL_SIZE = 900;
        const int MAX_BALL_SIZE = 1550;

        //capture width and hight
        const int FRAME_WIDTH = 640;
        const int FRAME_HEIGHT = 480;
        
        // max number of objects to be detected
        const int MAX_NUM_OBJECTS = 10;

        // minimum and maximum object area
        const int MIN_OBJECT_AREA = 20 * 20;
        const int MAX_OBJECT_AREA = (int)((double)FRAME_HEIGHT * FRAME_WIDTH / 1.5);

        // names of windows
        const string windowName = "Original image";
        const string windowName1 = "HSV image";
        const string windowName2 = "Thresholded image";
        const string windowName3 = "After morphological operations";
        const string trackBarWindowName = "Trackbars";
        static VideoCapture capture;

        
        

        bool trackObjects;
        bool useMorphOps;
        Mat cameraFeed;
        Mat HSV;
        Mat treshold;
        Mat transformedFeed;
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

            /*
            //*******************
            //konvertuojamas image to hsv formatą
            Image<Hsv, Byte> hsvimg = cameraFeed.ToImage<Hsv, byte>().Convert<Hsv, byte>();

            //extract the hue and value channels
            Image<Gray, Byte>[] channels = hsvimg.Split();  //suskaidome į komponentus filtracijai
            Image<Gray, Byte> imghue = channels[0];            //hsv, channels [0] yra hue
            Image<Gray, Byte> imgsat = channels[1];            //hsv, channels [1] yra saturation
            Image<Gray, Byte> imgval = channels[2];            //hsv, channels[2] yra value.

            //filtruojama hue spalva
            Image<Gray, byte> huefilter = imghue.InRange(new Gray(180), new Gray(250));

            //filtruojama tos spalvos atspalviai (brightness)
            Image<Gray, byte> satfilter = imgsat.InRange(new Gray(150), new Gray(250));

            //filtruojama tos spalvos atspalviai (brightness)
            Image<Gray, byte> valfilter = imgval.InRange(new Gray(30), new Gray(80));

            //sujungiama spalva su brightness, kad gautume spalvos spektrą
            Image<Gray, byte> colordetimg = huefilter.And(valfilter).And(satfilter);
            
            //**********************
            */

            //H_MIN = trackBar1.Value;
            label1.Text = "H_MIN" + H_MIN;
            //H_MAX = trackBar2.Value;
            label2.Text = "H_MAX" + H_MAX;
            //S_MIN = trackBar3.Value;
            label3.Text = "S_MIN" + S_MIN;
            //S_MAX = trackBar4.Value;
            label4.Text = "S_MAX" + S_MAX;
            //V_MIN = trackBar5.Value;
            label5.Text = "V_MIN" + V_MIN;
            //V_MAX = trackBar6.Value;
            label6.Text = "V_MAX" + V_MAX;
            // filter HSV image between values
            CvInvoke.InRange(HSV, new ScalarArray(new MCvScalar(H_MIN, S_MIN, V_MIN)), new ScalarArray(new MCvScalar(H_MAX, S_MAX, V_MAX)), treshold);
            
            // eliminate noise
            if (useMorphOps)
            {
                MorphOps(treshold);
            }
            
            // get coordinates of tracking object
            VectorOfVectorOfPoint points;
            if (trackObjects)
            {
                Contours(treshold, cameraFeed,  transformedFeed, out points);
            }

            

            //show frames
             
            imageBox1.Image = cameraFeed.ToImage<Bgr, byte>();
            imageBox2.Image = transformedFeed.ToImage<Bgr, byte>();
           // imageBox2.Image = cameraFeed.ToImage<Hsv, byte>().Convert<Hsv, byte>(); ;
            //imageBox2.Image = HSV.ToImage<Bgr, byte>();
            imageBox3.Image = treshold.ToImage<Bgr, byte>();
            


        }

        
        


        
        private void MorphOps(Mat threshold)
        {
            Mat erodeElement = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));
            Mat dilateElement = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(15, 15), new System.Drawing.Point(-1, -1));
            
            CvInvoke.Erode(threshold, threshold, erodeElement, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(255, 255, 255));
            CvInvoke.Dilate(threshold, threshold, dilateElement, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(255, 255, 255));
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
            label7.Text = "Matrica: " + cameraFeed.Width + "x" + cameraFeed.Height;
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
            
            trackObjects = true;
            useMorphOps = true;

            transformedFeed = new Mat();
            
            
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
            //MessageBox.Show("paejo");


            /*
            imageBox1.Image = cameraFeed.ToImage<Bgr, byte>();
            */

            imageCapture.Read(transformedFeed);

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
                panel1.Visible = true;
                //gameZone = new GameZone(selectedPoints);
                Test();
                //Start();
            }
        }

        private void Test()
        {
            MyPoint tempPoint = new MyPoint(1350, 2800);
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
            b = gameZone.videoOXY2gameZoneOXY(tempPoint);
            CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString() + " e", new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
            CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);

            //Transformuojame kampu koordinates
            for (int i = 0; i < 4; i++)
            {


                b = gameZone.videoOXY2gameZoneOXY(gameZone.Points[i]);
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
                label7.Text = "X=" + coordinates.X + " Y=" + coordinates.Y;
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        /*
        private void trackFilteredObject(int x, int y, Mat threshold, Mat cameraFeed)
        {
            Mat temp = new Mat();
            threshold.CopyTo(temp);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(temp, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            double refArea = 0;
            bool objectFount = false;
            if(hierarchy.Size() > 0)
            {
                int numObjects = hierarchy.Size();
                
                if (numObjects < MAX_NUM_OBJECTS)
                {
                    
                    for (int i = 0; i >=0; i = hierarchy[i][0])
                }
            }
        }
        */
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

                Point b = gameZone.videoOXY2gameZoneOXY(new MyPoint(a.X, a.Y)).ConvertoToDrawingPoint(); //transformuotas taškas
                

                label8.Text = transformedFeed.Width + ";" + transformedFeed.Height;
                CvInvoke.PutText(transformedFeed, b.X.ToString() + "," + b.Y.ToString(), new Point(b.X, b.Y + 100), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Circle(transformedFeed, new Point(b.X, b.Y), 5, new MCvScalar(15, 15, 0), 20);
                CvInvoke.Circle(transformedFeed, new Point(a.X, a.Y), 5, new MCvScalar(255, 15, 0), 10);
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
    }
}
