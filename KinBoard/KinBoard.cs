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
        HandWriting hand_writing = null;

        private int whichHand = 0; // right = 0, left = 1

        public KinBoard() {
            OpenKinect();
            action = new Action();
            hand_writing = new HandWriting();
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
                        if(whichHand == 0) // 오른손잡이일 경우
                        {
                            if(bodies[i].HandRightState == HandState.Closed)
                            {
                                hand_writing.Pen(); // 필기 모드
                            }
                            else
                            {
                                action.compare(); // 동작 판단 함수
                            }
                        }
                        else // 왼손잡이일 경우
                        {
                            if (bodies[i].HandLeftState == HandState.Closed)
                            {
                                hand_writing.Pen(); // 필기 모드
                            }
                            else
                            {
                                action.compare(); // 동작 판단 함수
                            }
                        }
                    }
                }
            }
        }
    }
    
}
