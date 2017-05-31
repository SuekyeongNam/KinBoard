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

            //BodyTracking();
        }

        // hand change
        public void Change_hand(int num)
        {
            whichHand = num;
        }

        // tracking body
        /*
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
                        if (whichHand == 0) // for right-handed
                        {
                            if (bodies[i].HandLeftState == HandState.Closed) // pen mode
                            {
                                // get the right hand coordinates
                                int x = (int)bodies[i].Joints[JointType.HandRight].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandRight].Position.Y;
                                // change cursor coordinates
                                hand_writing.SetCursor(x, y);

                                hand_writing.Pen();
                            }
                            else if (bodies[i].HandRightState == HandState.Closed)  // eraser mode
                            {
                                // get the left hand coordinates
                                int x = (int)bodies[i].Joints[JointType.HandLeft].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandLeft].Position.Y;
                                // change cursor coordinates
                                hand_writing.SetCursor(x, y);

                                hand_writing.Erase();
                            }
                            else
                            {
                                action.compare(); // decide the action
                            }
                        }
                        else // for left-handed
                        {
                            if (bodies[i].HandRightState == HandState.Closed)   // pen mode
                            {
                                // get the left hand coordinates
                                int x = (int)bodies[i].Joints[JointType.HandLeft].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandLeft].Position.Y;
                                // change cursor coordinates
                                hand_writing.SetCursor(x, y);

                                hand_writing.Pen();
                            }
                            else if (bodies[i].HandLeftState == HandState.Closed)   // eraser mode
                            {
                                // get the right hand coordinates
                                int x = (int)bodies[i].Joints[JointType.HandRight].Position.X;
                                int y = (int)bodies[i].Joints[JointType.HandRight].Position.Y;
                                // change cursor coordinates
                                hand_writing.SetCursor(x, y);

                                hand_writing.Erase();
                            }
                            else
                            {
                                action.compare(); // decide the action
                            }
                        }
                    }
                }
            }
        }*/

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(bodies);

                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (body != null)
                    {
                        Joint handRight = body.Joints[JointType.HandRight];

                        if (handRight.TrackingState != TrackingState.NotTracked)
                        {
                            CameraSpacePoint handRightPosition = handRight.Position;
                            ColorSpacePoint handRightPoint = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(handRightPosition);

                            float x = handRightPoint.X;
                            float y = handRightPoint.Y;

                            
                        }
                    }
                }
            }
        }
    }
    
}
