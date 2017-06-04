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
        private int current_skeleton_id = 0;
        private KinectSensor kinectSensor = null;

        // color frame 변수
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;
        private int frame_count = 0;

        // body frmae 변수
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private BodyFrame bodyFrame = null;
        Body body = null;
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

        private Skeleton current_skeleton;

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

        Kairos.API.KairosClient client = null;
        string imageUrl = "";
        int count_id = 0;
        // Face recognition 
        //Kairos.API.KairosClient client = new Kairos.API.KairosClient();

        public MainForm(double x_ratio, double y_ratio, double depth_location, double real_start_x, double real_start_y)
        {
            InitializeComponent();
            kinectSensor = KinectSensor.GetDefault();
            handwriting = new HandWriting();

            string[] lines = System.IO.File.ReadAllLines("skeleton.txt");
            for(int i = 0; i < lines.Count(); i++)
            {
                Skeleton newSkelton= new Skeleton();
                newSkelton.set_RHand(Int32.Parse(lines[i]));
                newSkelton.set_id(i);
                skeletons.Add(newSkelton);
            }
            count_id = lines.Count();

            client = new Kairos.API.KairosClient();

            client.ApplicationID = "1bb5b32c";
            client.ApplicationKey = "fc94f86519c41c4ca922a68012ae9eab";

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
                current_skeleton = new Skeleton();
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
            current_skeleton.set_RHand(1);
            isRightHanded = false;
            whichHand = 'L';
        }

        private void RHandedBtn_Click(object sender, EventArgs e)
        {
            // For right-handed person
            current_skeleton.set_RHand(0);
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
            string filetext = "";
            for(int i = 0; i < skeletons.Count; i++)
            {
                if (skeletons[i].get_RHand() == 0)
                {
                    filetext += " 0\n";
                }
                else
                {
                    filetext += " 1\n";
                }
            }

            System.IO.File.WriteAllText("skeleton.txt", filetext);

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

        public string get_color_frame()
        {
            string id = "";
            ColorFrame colorFrame_ = colorFrameReader.AcquireLatestFrame();
            if(colorFrame_ != null)
            {
                FrameDescription colorFrameDescription = colorFrame_.FrameDescription;

                using (KinectBuffer colorBuffer = colorFrame_.LockRawImageBuffer())
                {
                    this.colorBitmap.Lock();

                    // verify data and write the new color frame data to the display bitmap
                    if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                    {
                        colorFrame_.CopyConvertedFrameDataToIntPtr(
                            this.colorBitmap.BackBuffer,
                            (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                            ColorImageFormat.Bgra);

                        this.colorBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        //this.colorBitmap.CopyPixels(, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight);
                    }

                    this.colorBitmap.Unlock();
                }

                if (this.colorBitmap != null)
                {
                    // create a png bitmap encoder which knows how to save a .png file
                    BitmapEncoder encoder = new PngBitmapEncoder();

                    // create frame from the writable bitmap and add to encoder
                    encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));


                    //string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                    string path = "C:\\Users\\KHUNET\\Desktop\\NoMore\\KinBoard\\KinBoard\\frame\\1.jpg";
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
                id = check_face();

            }
            return id;
        }
        public void add_new_face()
        {
            var temp = client.Detect(imageUrl);
            var face = temp.Images.First().Faces[0];
            
            client.Enroll(imageUrl, count_id.ToString(), face.topLeftX, face.topLeftY, face.width, face.height );
            current_skeleton_id = count_id;
            Skeleton new_one = new Skeleton();
            //new_one = null;
            skeletons.Add(current_skeleton);
            skeletons[count_id].set_body(body);
            skeletons[count_id].set_id(count_id);
            skeletons[count_id].set_RHand(current_skeleton.get_RHand());
            count_id++;
        }

        public void find_skeleton(int id)
        {
            int m = 0;
            for(int i = 0; i < skeletons.Count(); i++)
            {
                if (skeletons[i].get_id() == id)
                {
                    current_skeleton_id = id;
                    m++;
                    break;
                }
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            frame_count++;
            if(frame_count == 200)
            {
                string id = get_color_frame();
                if(id == "")
                {
                    add_new_face();
                }
                else
                {
                    find_skeleton(Int32.Parse(id));
                }
                //MessageBox.Show(id);
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

                    body = bodies.Where(b => b.IsTracked).FirstOrDefault();
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
                        
                        if(current_skeleton.get_RHand() == 0) //오른손
                        {
                            whichHand = 'R';
                        }
                        else
                        {
                            whichHand = 'L';
                        }

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
                                    float R_a = handRightPoint.X;
                                    float R_b = handRightPoint.Y;
                                    float R_c = handRightPosition.Z;
                                    
                                    float L_a = handLeftPoint.X;
                                    float L_b = handLeftPoint.Y;
                                    float L_c = handLeftPosition.Z;

                                    writing(R_a, R_b, R_c, mode);

                                    if (body.HandLeftState == HandState.Closed)
                                    {
                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 15)
                                        {
                                            mode = (mode + 1) % 3;
                                            current_skeleton.get_bodies().Clear();
                                        }
                                        
                                    }
                                    
                                    if (body.HandRightState == HandState.Lasso)
                                    {

                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 15)
                                        {
                                            lasso = false;
                                            handwriting.EndClick();
                                            current_skeleton.get_bodies().Clear();
                                        }
                                    }
                                }
                                else // 필기모드 아님
                                {
                                    
                                    // 넘기기 동작
                                    if (body.HandLeftState == HandState.Closed)
                                    {
                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 21)
                                        {
                                            action.compare(current_skeleton, whichHand);
                                            Delay(1);
                                            current_skeleton.clear_hand();
                                            current_skeleton.get_bodies().Clear();
                                        }
                                    }
                                    // 필기모드 진입
                                    else if(body.HandLeftState == HandState.Lasso)
                                    {
                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 15)
                                        {
                                            lasso = true;
                                            current_skeleton.get_bodies().Clear();
                                        }
                                    }
                                    // 아무것도 아닌 상태
                                    else
                                    {
                                        current_skeleton.clear_hand();
                                        current_skeleton.get_bodies().Clear();
                                    }
                                }

                            }
                            else
                            {
                                if (lasso)
                                {
                                    // 필기
                                    float R_a = handRightPoint.X;
                                    float R_b = handRightPoint.Y;
                                    float R_c = handRightPosition.Z;
                                    
                                    float L_a = handLeftPoint.X;
                                    float L_b = handLeftPoint.Y;
                                    float L_c = handLeftPosition.Z;

                                    writing(L_a, L_b, L_c, mode);

                                    if (body.HandRightState == HandState.Closed)
                                    {
                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 15)
                                        {
                                            mode = (mode + 1) % 3;
                                            current_skeleton.get_bodies().Clear();
                                        }
                                        Delay(1);
                                    }

                                    if (body.HandLeftState == HandState.Lasso)
                                    {

                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 15)
                                        {
                                            lasso = false;
                                            handwriting.EndClick();
                                            current_skeleton.get_bodies().Clear();
                                        }
                                    }
                                }
                                else // 필기모드 아님
                                {

                                    // 넘기기 동작
                                    if (body.HandRightState == HandState.Closed)
                                    {
                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 21)
                                        {
                                            action.compare(current_skeleton, whichHand);
                                            Delay(1);
                                            current_skeleton.clear_hand();
                                            current_skeleton.get_bodies().Clear();
                                        }
                                    }
                                    else if (body.HandRightState == HandState.Lasso)
                                    {
                                        current_skeleton.set_body(body);
                                        current_skeleton.set_id(1);
                                        current_skeleton.set_hand_state(body.HandRightState, body.HandLeftState);
                                        current_skeleton.set_Hands(L, R);
                                        if (current_skeleton.get_bodies().Count() == 15)
                                        {
                                            lasso = true;
                                            current_skeleton.get_bodies().Clear();
                                        }
                                    }
                                    // 아무것도 아닌 상태
                                    else
                                    {
                                        current_skeleton.clear_hand();
                                        current_skeleton.get_bodies().Clear();
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
            

            // Detect the face(s)
            if (client_upload != null) { }
                //MessageBox.Show("이미 연결되어있습니다.");
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

            string image = "C:\\Users\\KHUNET\\Desktop\\NoMore\\KinBoard\\KinBoard\\frame\\1.jpg";

            //image 경로를 보내는 부분
            NetworkStream nwStream = client_upload.GetStream();
            byte[] byteToSend = ASCIIEncoding.ASCII.GetBytes(image);
            nwStream.Write(byteToSend, 0, byteToSend.Length);

            //보낸 image의 url을 receive한 부분
            byte[] bytesToRead = new byte[client_upload.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client_upload.ReceiveBufferSize);
            string recieve_url = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

            imageUrl = recieve_url;

            var recognizeResponse = client.Recognize(imageUrl);
            if(recognizeResponse.Contains("subject_id"))
            {
                int _index = recognizeResponse.IndexOf(':');
                recognizeResponse = recognizeResponse.Substring(_index + 1);
                recognizeResponse = recognizeResponse.Replace("\"", "");
            }
            else
            {
                recognizeResponse = "";
            }
            // Get the recognized user ID
            return recognizeResponse;
        }
    }
}
