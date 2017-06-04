using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using OpenCvSharp.CPlusPlus;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Globalization;
using Emgu.CV;

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

            // color frame
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            kinectSensor.Open();


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
            if (this.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

                string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");

                // write the new file to disk
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
            
            MainForm _new = new MainForm();
            this.SetVisibleCore(false);
            _new.Show();

        }

        private void btn_setting_Click(object sender, EventArgs e)
        {
            PPt.CustomLayout temp = slides[1].CustomLayout;
            slides.AddSlide(1, temp);
            slides[1].Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeRound1Rectangle, (slideWidth / 2) - 20, (slideHeight / 2) - 20, 40, 40);
            slides[1].Shapes[3].Fill.ForeColor.RGB = System.Drawing.Color.Blue.ToArgb();
            slides[1].Shapes[3].Line.DashStyle = Microsoft.Office.Core.MsoLineDashStyle.msoLineSolid;

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
