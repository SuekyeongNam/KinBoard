using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using Microsoft.Kinect;


namespace KinBoard
{
    public class KinBoard
    {
        private int body_num = 0;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private BodyFrame bodyFrame = null;

        public KinBoard() { OpenKinect(); }
        public void OpenKinect()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            bodyFrame = bodyFrameReader.AcquireLatestFrame();
            if (bodyFrame != null)
            {
                bodyFrame.GetAndRefreshBodyData(bodies);
                for (int i = 0; i < bodies.Length; i++)
                {
                    if (bodies[i].IsTracked == true)
                    {

                    }
                }
            }
        }
        public void BodyTracking()
        {

        }
    }
    
}
