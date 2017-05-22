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
        private List<Skeleton> skeletons;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private BodyFrame bodyFrame = null;
        Action action = null;

        public KinBoard() {
            OpenKinect();
            action = new Action();
        }

        // open Kinect
        public void OpenKinect()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            BodyTracking();
        }

        // tracking body
        public void BodyTracking()
        {
            bodyFrame = bodyFrameReader.AcquireLatestFrame();
            if (bodyFrame != null)
            {
                bodyFrame.GetAndRefreshBodyData(bodies);
                if(bodies.Length != skeletons.Count)
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
                        action.compare(); // 동작 판단 함수
                    }
                }
            }
        }
    }
    
}
