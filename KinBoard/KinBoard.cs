using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using Microsoft.Kinect;
using LightBuzz.Vitruvius.FingerTracking;


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

        // hand change
        public void chand_hand(int num)
        {
            whichHand = num;
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
                        if (whichHand == 0) // 오른손잡이일 경우
                        {
                            if (bodies[i].HandLeftState == HandState.Closed) // 펜 모드
                            {
                                // 오른손가락 좌표 받아오기
                                int x = (int)bodies[i].Joints[JointType.HandRight].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandRight].Position.Y;
                                // 마우스 커서 좌표 변경
                                hand_writing.SetCursor(x, y);

                                hand_writing.Pen();
                            }
                            else if (bodies[i].HandRightState == HandState.Closed)  // 지우개 모드
                            {
                                // 왼손가락 좌표 받아오기
                                int x = (int)bodies[i].Joints[JointType.HandLeft].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandLeft].Position.Y;
                                // 마우스 커서 좌표 변경
                                hand_writing.SetCursor(x, y);

                                hand_writing.Erase();
                            }
                            else
                            {
                                action.compare(); // 동작 판단 함수
                            }
                        }
                        else // 왼손잡이일 경우
                        {
                            if (bodies[i].HandRightState == HandState.Closed)   // 펜 모드
                            {
                                // 왼손가락 좌표 받아오기
                                int x = (int)bodies[i].Joints[JointType.HandLeft].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandLeft].Position.Y;
                                // 마우스 커서 좌표 변경
                                hand_writing.SetCursor(x, y);

                                hand_writing.Pen();
                            }
                            else if (bodies[i].HandLeftState == HandState.Closed)   // 지우개 모드
                            {
                                // 오른손가락 좌표 받아오기
                                int x = (int)bodies[i].Joints[JointType.HandRight].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandRight].Position.Y;
                                // 마우스 커서 좌표 변경
                                hand_writing.SetCursor(x, y);

                                hand_writing.Erase();
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
