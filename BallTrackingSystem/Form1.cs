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

namespace BallTrackingSystem
{
    public partial class Form1 : Form
    {
        // initial min and max HSV filter values
        int H_MIN = 0;
        int H_MAX = 256;
        int S_MIN = 0;
        int S_MAX = 256;
        int V_MIN = 0;
        int V_MAX = 256;

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

        // status of inifinitive process
        bool RUNNING = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool trackObjects = false;
            bool useMorphOps = false;

            // matrix to store each frame of the webcam feed
            Mat cameraFeed = new Mat();
            // matrix storage for HSV image
            Mat HSV = new Mat();
            // matrix storage for binary threshold image
            Mat treshold = new Mat();
            // x and y values for the location of the object
            int x = 0;
            int y = 0;
            //create slider bars for HSV filtering
            createTrackBars();
            // video capture object to acquire webcam feed
            VideoCapture capture = new VideoCapture(0);
            // set weight width and height of capture frame
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, FRAME_WIDTH);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, FRAME_HEIGHT);
            while (RUNNING)
            {
                // store image to matrix
                capture.Read(cameraFeed);
                // convert frame from RGB to HSV colorspace
                CvInvoke.CvtColor(cameraFeed, HSV, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                // filter HSV image between values
                InputArray low = new InputArray(H_MIN, S_MIN, V_MIN);
                MCvScalar up = new MCvScalar(H_MAX, S_MAX, V_MAX);
                CvInvoke.InRange(HSV, new ScalarArray(new MCvScalar(H_MIN, S_MIN, V_MIN)), new ScalarArray(new MCvScalar(H_MAX, S_MAX, V_MAX)), treshold);

                // eliminate noise
                if (useMorphOps)
                {
                    MorphOps(out treshold);
                }
                // get coordinates of tracking object
                if (trackObjects)
                {
                    trackFilteredObject(out x, out y, treshold, out cameraFeed);
                }
                //show frames
                imageBox1.Image = cameraFeed.ToImage<Rgb, byte>();
                imageBox2.Image = cameraFeed.ToImage<Hsv, byte>();
                
                
            }
            


        }

        private void MorphOps(out Mat treshold)
        {

        }
        private void trackFilteredObject(out int x, out int y, Mat threshold, out Mat cameraFeed)
        {

        }


    }
}
