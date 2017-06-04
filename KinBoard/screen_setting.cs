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
using OpenCvSharp.CPlusPlus;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Globalization;


// Add PowerPoint namespace
using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace KinBoard
{
    public partial class screen_setting : Form
    {

        // color frame 변수
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;
        private KinectSensor kinectSensor = null;

        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private BodyFrame bodyFrame = null;

        private Skeleton _skeleton;

        private double ratio = 0;
        private OpenCvSharp.CPlusPlus.Point[] _point = new OpenCvSharp.CPlusPlus.Point[2];
        private int count = 0;
        private int IsBtnClick = 0;

        private double x_ratio;
        private double y_ratio;
        private double depth_location = 0.0;

        double real_start_y;
        double real_start_x;

        static public PPt.Application pptApp;
        static public PPt.Slides slides;
        static public PPt.Slide slide;
        static public PPt.Presentation presentation;
        static public PPt.SlideShowSettings slideShowSettings;
        static public PPt.SlideShowView slideShowView;
        static public float slideHeight;
        static public float slideWidth;

        public screen_setting()
        {
            InitializeComponent();
           
            kinectSensor = KinectSensor.GetDefault();

            //body frame
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyReader_FrameArrived;

            kinectSensor.Open();

            bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
            _skeleton = new Skeleton();
        }

        public double get_ratio()
        {
            return ratio;
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

                            OpenCvSharp.CPlusPlus.Point R = new OpenCvSharp.CPlusPlus.Point(R_x, R_y);
                            OpenCvSharp.CPlusPlus.Point L = new OpenCvSharp.CPlusPlus.Point(L_x, L_y);

                            if (IsBtnClick == 1)
                            {
                                _skeleton.set_body(body);
                                _skeleton.set_id(1);
                                _skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                _skeleton.set_Hands(L, R);
                                depth_location = body.Joints[JointType.HandRight].Position.Z;
                                count++;
                                IsBtnClick = 0;

                                // 다음 창으로 넘어감
                                if (count == 2)
                                {
                                    _point[0] = _skeleton.get_RHandPoint(0);
                                    _point[1] = _skeleton.get_RHandPoint(1);
                                    ratio = _point[0].X - _point[1].X;
                                    ratio = Math.Abs(ratio);
                                    set_ratio();
                                    MainForm _new = new MainForm(x_ratio, y_ratio, depth_location, real_start_x, real_start_y);
                                    this.SetVisibleCore(false);
                                    _new.Show();
                                }
                            }
                        }

                    }

                }
            }
        }

        public void set_ratio()
        {
            double height = ((double)540 / 960) * ratio;

            real_start_y = _point[1].Y - height;
            //real_start_y = _point[0].Y;
            //if(real_start_y < 0)
            //{
            //    real_start_y = 0;
            //}
            real_start_x = _point[0].X;
            //if(real_start_x < 0)
            //{
            //    real_start_x = 0;
            //}
            x_ratio = (960 / (double)ratio);
            y_ratio = (540 / (double)height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 손가락으로 인식
            IsBtnClick = 1;
        }

        private void screen_setting_Load(object sender, EventArgs e)
        {

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
    }

}
