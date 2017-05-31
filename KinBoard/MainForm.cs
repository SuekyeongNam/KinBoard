using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using OpenCvSharp.CPlusPlus;
using KinBoard;


// Add PowerPoint namespace
using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace KinBoard
{
    public partial class MainForm : Form
    {
        //public static KinBoard _KinBoard;
        private List<Skeleton> skeletons;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private BodyFrame bodyFrame = null;
        Action action = null;

        private int whichHand = 0;

        private int _width = 0;
        private int _height = 0;
        private byte[] _pixels = null;

        
        static public PPt.Application pptApp;
        
        static public PPt.Slides slides;
        static public PPt.Slide slide;
        static public PPt.Presentation presentation;
        // Slide count
        static public int slidescount;
        // slide index
        static public int slideIndex;
        

        bool isRightHanded = true;

        public MainForm()
        {
            InitializeComponent();

            kinectSensor = KinectSensor.GetDefault();

            if (kinectSensor != null)
            {
                kinectSensor.Open();

                bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
                bodyFrameReader.FrameArrived += BodyReader_FrameArrived;

                _pixels = new byte[_width * _height * 4];
                bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
                skeletons = new List<Skeleton>();
                skeletons.Add(new Skeleton());
                action = new Action();
            }

            // Set two buttons disable
            this.LHandedBtn.Enabled = false;
            this.RHandedBtn.Enabled = false;
        }

        public MainForm(KinBoard _temp)
        {
            InitializeComponent();
        }

        private void KinBoard_Load(object sender, EventArgs e)
        {
            // Checking whether Powerpoint file is open
            try
            {   // Get running Powerpoint application object
                pptApp = Marshal.GetActiveObject("PowerPoint.Application") as PPt.Application;

                // Get powerpoint application successfully, then set two buttons enable
                this.LHandedBtn.Enabled = true;
                this.RHandedBtn.Enabled = true;
            }
            catch
            {
                MessageBox.Show("[Error] PowerPoint file did not open!\nYou must open a file before running this program to use.", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
            if(pptApp != null)
            {
                // Get Presentation Object
                presentation = pptApp.ActivePresentation;
                // Get Slide collection object
                slides = presentation.Slides;
                // Get Slide count
                slidescount = slides.Count;
                try
                {
                    // Get selected slide object in normal view
                    slide = slides[pptApp.ActiveWindow.Selection.SlideRange.SlideNumber];
                }
                catch
                {
                    // Get selected slide object in reading view
                    slide = pptApp.SlideShowWindows[1].View.Slide;
                }
            }
        }

        private void LHandedBtn_Click(object sender, EventArgs e)
        {
            // For left-handed person
            isRightHanded = false;
            whichHand = 1;
        }

        private void RHandedBtn_Click(object sender, EventArgs e)
        {
            // For right-handed person
            isRightHanded = true;
            whichHand = 0;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If you click the close button on form
            if (MessageBox.Show("Do you want to exit the program?", "Exit", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bodyFrameReader != null)
            {
                bodyFrameReader.Dispose();
            }

            if (kinectSensor != null)
            {
                kinectSensor.Close();
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {

            using (var frame = e.FrameReference.AcquireFrame())
            {
               // 사람이 인식되지 않은 상황에서 프로그램을 시작하면 정상적으로 frame을 받아옴.
               // 그러나, 프로그램 시작 전 사람을 인식하고 있으면 frame == null...
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(bodies);
                    
                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (body != null)
                    {
                        Joint handRight = body.Joints[JointType.HandRight];
                        Joint handLeft = body.Joints[JointType.HandLeft];

                        if (handRight.TrackingState != TrackingState.NotTracked && handLeft.TrackingState != TrackingState.NotTracked)
                        {
                            CameraSpacePoint handRightPosition = handRight.Position;
                            ColorSpacePoint handRightPoint = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(handRightPosition);
                            CameraSpacePoint handLeftPosition = handLeft.Position;
                            ColorSpacePoint handLeftPoint = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(handLeftPosition);

                            int R_x = (int)handRightPoint.X;
                            int R_y = (int)handRightPoint.Y;

                            int L_x = (int)handLeftPoint.X;
                            int L_y = (int)handLeftPoint.Y;

                            Point R = new Point(R_x, R_y);
                            Point L = new Point(L_x, L_y);

                            skeletons[0].set_id(1);
                            skeletons[0].set_hand_state(body.HandRightState,body.HandLeftState);
                            skeletons[0].set_body(body);
                            skeletons[0].set_Hands(L, R);

                            if (skeletons[0].get_bodies().Count() > 40)
                            {
                                action.compare(skeletons[0], whichHand);
                            }
                        }
                    }
                    /*
                    if (bodies.Length != skeletons.Count)
                    {
                        Skeleton temp = null;
                        temp.set_id(bodies.Length - 1);
                        skeletons.Add(temp);
                    }
                    for (int i = 0; i < bodies.Length; i++)
                    {
                        if (bodies[i].IsTracked == true)
                        {
                            skeletons[i].set_body(bodies[i]);
                            skeletons[i].set_hand_state(bodies[i].HandRightState, bodies[i].HandLeftState);
                            if (whichHand == 0) // 오른손잡이일 경우
                            {
                                if (bodies[i].HandRightState == HandState.Closed)
                                {
                                    //hand_writing.Pen(); // 필기 모드
                                }
                                else
                                {
                                    action.compare(skeletons[i], whichHand); // 동작 판단 함수
                                }
                            }
                            else // 왼손잡이일 경우
                            {
                                if (bodies[i].HandLeftState == HandState.Closed)
                                {
                                    //hand_writing.Pen(); // 필기 모드
                                }
                                else
                                {
                                    action.compare(skeletons[i], whichHand); // 동작 판단 함수
                                }
                            }
                        }
                        
                    }
                    */

                }
            }
        }
    }
}
