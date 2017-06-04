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
using System.IO;
using System.Net.Sockets;
using System.Net;

// Add PowerPoint namespace
using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;


namespace KinBoard
{
    public partial class MainForm : Form
    {
        private List<Skeleton> skeletons;
        private KinectSensor kinectSensor = null;

        // color frame 변수
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;
        private int frame_count = 0;

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

        private double x_ratio;
        private double y_ratio;
        private double depth_location;
        private double real_start_x;
        private double real_start_y;

        private int mode = 0;   // 0: Pen, 1: Eraser, 2: HighlightPen
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

        // server
        static string HOST = "127.0.0.1";
        static int PORT = 9000;
        static TcpClient client_upload;

        // Face recognition 
        //Kairos.API.KairosClient client = new Kairos.API.KairosClient();

        public MainForm(double x_ratio, double y_ratio, double depth_location, double real_start_x, double real_start_y)
        {
            InitializeComponent();
            kinectSensor = KinectSensor.GetDefault();
            handwriting = new HandWriting();

            if (kinectSensor != null)
            {
                // body frame
                bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
                bodyFrameReader.FrameArrived += BodyReader_FrameArrived;

                // color frame
                this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
                //this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
                FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                // create the bitmap to display
                this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

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

            this.x_ratio = x_ratio;
            this.y_ratio = y_ratio;
            this.depth_location = depth_location;
            this.real_start_x = real_start_x;
            this.real_start_y = real_start_y;
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

            if(colorFrameReader != null)
            {
                colorFrameReader.Dispose();
            }

            if (kinectSensor != null)
            {
                kinectSensor.Close();
            }
        }

        public ColorFrame get_color_frame()
        {
            ColorFrame colorFrame_ = colorFrameReader.AcquireLatestFrame();
            if (this.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));


                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "C:\\Users\\수경\\Desktop\\KinBoard\\KinBoard\\KinBoard\\KinBoard\\frame" + "1" + ".jpg");
                //image_count++;
                // write the new file t o disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (IOException)
                {

                }
            }
            return colorFrame_;
        }

        //private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        //{
            
        //    // ColorFrame is IDisposable
        //    using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
        //    {
        //        if (colorFrame != null)
        //        {
        //            FrameDescription colorFrameDescription = colorFrame.FrameDescription;

        //            using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
        //            {
        //                this.colorBitmap.Lock();

        //                // verify data and write the new color frame data to the display bitmap
        //                if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
        //                {
        //                    frame_count++;
        //                    if (frame_count == 80)
        //                    {
        //                        capture_photo();
        //                        string id = check_face();
        //                        //frame_count = 0;
        //                    }
        //                }

        //                this.colorBitmap.Unlock();
        //            }

        //        }
        //    }
        //}

        private void capture_photo()
        {
            if (this.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));


                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "C:\\Users\\수경\\Desktop\\KinBoard\\KinBoard\\KinBoard\\KinBoard\\frame\\KinectScreenshot-Color-" + "1" + ".jpg");
                //image_count++;
                // write the new file t o disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (IOException)
                {

                }
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            frame_count++;
            if(frame_count == 200)
            {
                get_color_frame();
                //string id = check_face();
                frame_count = 0;
            }
            using (var frame = e.FrameReference.AcquireFrame())
            {
               // 사람이 인식되지 않은 상황에서 프로그램을 시작하면 정상적으로 frame을 받아옴.
               // 그러나, 프로그램 시작 전 사람을 인식하고 있으면 frame == null...
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(bodies);

                    // stable 시키기
                    Stablization filter = new Stablization();
                    filter.Init();

                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();
                    filter.UpdateFilter(body);
                    CameraSpacePoint[] filteredJoints = filter.GetFilteredJoints();

                    if (body != null)
                    {
                        //Joint handRight = body.Joints[JointType.HandRight];
                        //Joint handLeft = body.Joints[JointType.HandLeft];

                        Joint handRight = body.Joints[JointType.HandRight];
                        handRight.Position.X = filteredJoints[11].X;
                        handRight.Position.Y = filteredJoints[11].Y;
                        handRight.Position.Z = filteredJoints[11].Z;
                        Joint handLeft = body.Joints[JointType.HandLeft];
                        handLeft.Position.X = filteredJoints[7].X;
                        handLeft.Position.Y = filteredJoints[7].Y;
                        handLeft.Position.Z = filteredJoints[7].Z;

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

                                    writing(R_a, R_b, R_c, mode);

                                    if (body.HandLeftState == HandState.Closed)
                                    {
                                        skeletons[0].set_body(body);
                                        skeletons[0].set_id(1);
                                        skeletons[0].set_hand_state(body.HandRightState, body.HandLeftState);
                                        skeletons[0].set_Hands(L, R);
                                        if (skeletons[0].get_bodies().Count() == 15)
                                        {
                                            mode = (mode + 1) % 3;
                                            skeletons[0].get_bodies().Clear();
                                        }
                                        
                                    }
                                    
                                    /*
                                    if (R_c > L_c)
                                    {
                                        writing(R_a, R_b, R_c, true);  // pen
                                    }
                                    else
                                    {
                                        writing(L_a, L_b, L_c, false);   // erase
                                    }
                                    */
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
                                            Delay(1);
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

        private void writing(float x, float y, float z, int mode)
        {
            x = x - (float)real_start_x;
            y = y - (float)real_start_y;
            x = System.Math.Abs(x);
            y = System.Math.Abs(y);

            int _x = (int)((1 + x_ratio) * x);
            int _y = (int)((1 + y_ratio) * y);

            handwriting.SetCursor(_x, _y);

            if (depth_location + 0.1 > z && depth_location - 0.1 < z)
            {

                if (mode == 0)
                    handwriting.Pen();
                else if (mode == 1)
                    handwriting.Erase();
                else if(mode == 2)
                    handwriting.HighlightPen();
            }
            else
            {
                if (mode == 0)
                    handwriting.PenHide();
                else if (mode == 1)
                    handwriting.EraseHide();
                else if(mode == 2)
                    handwriting.HighlightPenHide();
            }
        }

        DateTime Delay(double ms)
        {
            DateTime dateTimeNow = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, (int)ms);
            DateTime dateTimeAdd = dateTimeNow.Add(duration);

            while (dateTimeAdd >= dateTimeNow)
            {
                //System.Windows.Forms.Application.DoEvents();
                dateTimeNow = DateTime.Now;
            }
            return DateTime.Now;
        }

        private string check_face()
        {
            Kairos.API.KairosClient client = new Kairos.API.KairosClient();

            client.ApplicationID = "1bb5b32c";
            client.ApplicationKey = "240fc5eb22e3e22eacd70582b2c6608e";

            // Detect the face(s)
            if (client_upload != null)
                MessageBox.Show("이미 연결되어있습니다.");
            else
            {
                try
                {
                    client_upload = new TcpClient();
                    client_upload.Connect(HOST, PORT);
                }
                catch (Exception ex)
                {
                    client_upload = null;
                }
            }

            string image = "C:\\Users\\수경\\Desktop\\KinBoard\\KinBoard\\KinBoard\\KinBoard\\face\\KinectScreenshot-Color-1.jpg";

            //image 경로를 보내는 부분
            NetworkStream nwStream = client_upload.GetStream();
            byte[] byteToSend = ASCIIEncoding.ASCII.GetBytes(image);
            nwStream.Write(byteToSend, 0, byteToSend.Length);

            //보낸 image의 url을 receive한 부분
            byte[] bytesToRead = new byte[client_upload.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client_upload.ReceiveBufferSize);
            string recieve_url = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

            string imageUrl = recieve_url;

            var recognizeResponse = client.Recognize(imageUrl);

            // Get the recognized user ID
            return recognizeResponse;
        }

    }
}
