using System.Collections.Generic;
using System.IO;
//using PPt = Microsoft.Office.Interop.PowerPoint;
using Microsoft.Kinect;

namespace KinBoard
{
    class Action
    {
        //PPt.SlideShowSettings slideShowSettings;
        //PPt.SlideShowView slideShowView;

        private int finger_num;

        public Action()
        {
            //get_finger_num();
        }

        //get number of fingers
        public void get_finger_num(int fingers)
        {
            finger_num = fingers;
        }

        //distinguish action and decide motion num
        public void compare(Skeleton skeleton_i, int which_hand)
        {
            //get body Queue
            Queue<Body> bodies = skeleton_i.get_bodies();

            //joint arrays for each hands at each frames
            Joint[] joint_array_R = null;
            Joint[] joint_array_L = null;

            //관절 정보를 불러온다
            IReadOnlyDictionary<JointType, Joint> joints;

            int temp_count = 0;

            while (bodies != null)
            {
                joints = bodies.Dequeue().Joints;
                //양 손의 좌표를 따로 저장한다
                joint_array_R[temp_count] = joints[JointType.HandRight];
                joint_array_L[temp_count] = joints[JointType.HandLeft];
                temp_count++;

            }

            //right hand coordinate
            double rightX1, rightY1, rightZ1;
            double rightX2, rightY2, rightZ2;
            //left hand coordinate
            double leftX1, leftY1, leftZ1;
            double leftX2, leftY2, leftZ2;

            //check movement
            int check_x_R = 0;
            int check_x_L = 0;
            int check_y_R = 0;
            int check_y_L = 0;
            int check_z_R = 0;
            int check_z_L = 0;

            //calculate variance between adjacent frame coordinate component
            for (int i = 0; i < temp_count - 1; i++)
            {
                //get the individual points of the right hand
                rightX1 = joint_array_R[temp_count].Position.X;
                rightY1 = joint_array_R[temp_count].Position.Y;
                rightZ1 = joint_array_R[temp_count].Position.Z;

                rightX2 = joint_array_R[temp_count + 1].Position.X;
                rightY2 = joint_array_R[temp_count + 1].Position.Y;
                rightZ2 = joint_array_R[temp_count + 1].Position.Z;

                //get the individual points of the left hand
                leftX1 = joint_array_L[temp_count].Position.X;
                leftY1 = joint_array_L[temp_count].Position.Y;
                leftZ1 = joint_array_L[temp_count].Position.Z;

                leftX2 = joint_array_L[temp_count + 1].Position.X;
                leftY2 = joint_array_L[temp_count + 1].Position.Y;
                leftZ2 = joint_array_L[temp_count + 1].Position.Z;

                //check x change
                if (rightX1 - rightX2 > 0)
                {
                    check_x_R++;
                }
                else if (leftX1 - leftX2 > 0)
                {
                    check_x_L++;
                }

                //check y change
                if (rightY1 - rightY2 > 0)
                {
                    check_y_R++;
                }
                else if (leftY1 - leftY2 > 0)
                {
                    check_y_L++;
                }

                //check z change
                if (rightZ1 - rightZ2 > 0)
                {
                    check_z_R++;
                }
                else if (leftZ1 - leftZ2 > 0)
                {
                    check_z_L++;
                }
            }

            //return the motion num if the differentials match
            if (check_x_R > 5 && finger_num == 5)
            {
                turnPage();
            }

            else if (check_x_R > 5 && finger_num != 5)
            {
                turnNPage(finger_num);
            }

            else
            {
                //other motions

            }
        }

        void turnPage()
        {
            //SlideShowWindow.View.Next;
            //slideShowView.Next;
        }

        void turnNPage(int finger_num)
        {
            for (int i = 0; i < finger_num; i++)
            {
                turnPage();
            }
        }
    }
}
