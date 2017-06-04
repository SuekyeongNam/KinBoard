using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
//using Emgu.CV;

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

            // color frame
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
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

        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                            //this.colorBitmap.CopyPixels(, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight);
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }

            try
            {   // Get running Powerpoint application object
                pptApp = Marshal.GetActiveObject("PowerPoint.Application") as PPt.Application;
            }
            catch
            {
                MessageBox.Show("[Error] PowerPoint file did not open!\nYou must open a file before running this program to use.", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
            if (pptApp != null)
            {
                // Get Presentation Object
                presentation = pptApp.ActivePresentation;
                // Get Slide collection object
                slides = presentation.Slides;
                // Get Slide count
                slideShowSettings = presentation.SlideShowSettings;
                slideShowView = presentation.SlideShowWindow.View;
                slideHeight = presentation.PageSetup.SlideHeight;
                slideWidth = presentation.PageSetup.SlideWidth;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 캡쳐
            //if (this.colorBitmap != null)
            //{
            //    // create a png bitmap encoder which knows how to save a .png file
            //    BitmapEncoder encoder = new PngBitmapEncoder();

            //    // create frame from the writable bitmap and add to encoder
            //    encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            //    string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            //    string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            //    string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");

            //    // write the new file t o disk
            //    try
            //    {
            //        // FileStream is IDisposable
            //        using (FileStream fs = new FileStream(path, FileMode.Create))
            //        {
            //            encoder.Save(fs);
            //        }
            //    }
            //    catch (IOException)
            //    {

            //    }
            //}

            // 손가락으로 인식
            //set_ratio();
            IsBtnClick = 1;
            
        }

        private void btn_setting_Click(object sender, EventArgs e)
        {
            PPt.CustomLayout temp = slides[1].CustomLayout;
            slides.AddSlide(1, temp);
            slides[1].Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeRound1Rectangle, (slideWidth / 2) - 20, (slideHeight / 2) - 20, 40, 40);
            slides[1].Shapes[3].Fill.ForeColor.RGB = System.Drawing.Color.Blue.ToArgb();
            slides[1].Shapes[3].Line.DashStyle = Microsoft.Office.Core.MsoLineDashStyle.msoLineSolid;

        }

        private void screen_setting_Load(object sender, EventArgs e)
        {

        }

        // 수정이 필요함
        //public static Mat ToMat(BitmapSource source)
        //{
        //if (source.Format == PixelFormats.Bgra32)
        //{
        //    Mat result = new Mat();
        //    result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 4);
        //    source.CopyPixels(System.Windows.Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
        //    return result;
        //}
        //else if (source.Format == PixelFormats.Bgr24)
        //{
        //    Mat result = new Mat();
        //    result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 3);
        //    source.CopyPixels(System.Windows.Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
        //    return result;
        //}
        //else if (source.Format == PixelFormats.Pbgra32)
        //{
        //    Mat result = new Mat();
        //    result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 4);
        //    source.CopyPixels(System.Windows.Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
        //    return result;
        //}
        //else
        //{
        //    throw new Exception(String.Format("Conversion from BitmapSource of format {0} is not supported.", source.Format));
        //}
        //}
    }

}
