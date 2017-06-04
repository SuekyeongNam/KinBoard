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
using System.Windows.Media.Imaging;
using System.Windows.Media;


// Add PowerPoint namespace
using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;


namespace KinBoard
{
    public partial class MainForm : Form
    {
        private List<Skeleton> skeletons;
        private KinectSensor kinectSensor = null;

       

        // body frmae 변수
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private BodyFrame bodyFrame = null;
        Action action = null;
        
        private char whichHand = 'R';
        private bool Semaphore = false;

        private int _width = 0;
        private int _height = 0;
        private byte[] _pixels = null;
        private bool lasso = false;

        static public PPt.Application pptApp;   
        static public PPt.Slides slides;
        static public PPt.Slide slide;
        static public PPt.Presentation presentation;
        static public PPt.SlideShowSettings slideShowSettings;
        static public PPt.SlideShowView slideShowView;
        static public float slideHeight;
        static public float slideWidth;

        // Slide count
        static public int slidescount;
        // slide index
        static public int slideIndex;
        HandWriting handwriting;

        bool isRightHanded = true;

        // Face recognition 
        //Kairos.API.KairosClient client = new Kairos.API.KairosClient();

        public MainForm()
        {
            InitializeComponent();
            kinectSensor = KinectSensor.GetDefault();
            handwriting = new HandWriting();

            if (kinectSensor != null)
            {
              

                // body frame
                bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
                bodyFrameReader.FrameArrived += BodyReader_FrameArrived;

                kinectSensor.Open();

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
                slideIndex = presentation.SlideShowWindow.View.Slide.SlideIndex;
;
                slideShowSettings = presentation.SlideShowSettings;
                slideShowView = presentation.SlideShowWindow.View;
                slideHeight = presentation.PageSetup.SlideHeight;
                slideWidth = presentation.PageSetup.SlideWidth;

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
            whichHand = 'L';
        }

        private void RHandedBtn_Click(object sender, EventArgs e)
        {
            // For right-handed person
            isRightHanded = true;
            whichHand = 'R';
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

                            if (whichHand == 'R' )
                            {                         
                                if (lasso)
                                {
                                    // 필기
                                    //float R_a = handRightPosition.X;
                                    //float R_b = handRightPosition.Y;
                                    float R_a = handRightPoint.X;
                                    float R_b = handRightPoint.Y;
                                    float R_c = handRightPosition.Z;

                                    //float L_a = handLeftPosition.X;
                                    //float L_b = handLeftPosition.Y;
                                    float L_a = handLeftPoint.X;
                                    float L_b = handLeftPoint.Y;
                                    float L_c = handLeftPosition.Z;

                                    if (R_c > L_c)
                                    {
                                        writing(R_a, R_b, R_c, true);  // pen
                                    }
                                    else
                                    {
                                        writing(L_a, L_b, L_c, false);   // erase
                                    }

                                    if (body.HandRightState == HandState.Lasso)
                                    {

                                        skeletons[0].set_body(body);
                                        skeletons[0].set_id(1);
                                        skeletons[0].set_hand_state(body.HandRightState, body.HandLeftState);
                                        skeletons[0].set_Hands(L, R);
                                        if (skeletons[0].get_bodies().Count() == 15)
                                        {
                                            lasso = false;
                                            handwriting.EndClick();
                                            skeletons[0].get_bodies().Clear();
                                        }
                                    }
                                }
                                else // 필기모드 아님
                                {
                                    
                                    // 넘기기 동작
                                    if (body.HandLeftState == HandState.Closed)
                                    {
                                        skeletons[0].set_body(body);
                                        skeletons[0].set_id(1);
                                        skeletons[0].set_hand_state(body.HandRightState, body.HandLeftState);                                      
                                        skeletons[0].set_Hands(L, R);
                                        if (skeletons[0].get_bodies().Count() == 21)
                                        {
                                            //Semaphore = true;
                                            action.compare(skeletons[0], whichHand);
                                            skeletons[0].clear_hand();
                                            skeletons[0].get_bodies().Clear();
                                            //Semaphore = false;
                                        }
                                    }
                                    // 필기모드 진입
                                    else if(body.HandLeftState == HandState.Lasso)
                                    {
                                        skeletons[0].set_body(body);
                                        skeletons[0].set_id(1);
                                        skeletons[0].set_hand_state(body.HandRightState, body.HandLeftState);

                                        skeletons[0].set_Hands(L, R);
                                        if (skeletons[0].get_bodies().Count() == 15)
                                        {
                                            lasso = true;
                                            skeletons[0].get_bodies().Clear();
                                        }
                                    }
                                    // 아무것도 아닌 상태
                                    else
                                    {
                                        skeletons[0].clear_hand();
                                        skeletons[0].get_bodies().Clear();
                                    }
                                }

                            }
                         
                        }

                    }

                }
            }
        }

        private void writing(float x, float y, float z, bool myhand)
        {
         //   if (z > 2.48 && z < 2.60)
          //  {
            handwriting.SetCursor((int)(x), (int)(y));
            if (myhand)
                handwriting.Pen();
            else
                handwriting.Erase();
        }

    }
}
