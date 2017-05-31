using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows.Forms;
using OpenCvSharp.CPlusPlus;

namespace KinBoard
{
    class Action
    {
  
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
            List<Point> joint_array_R = new List<Point> ();
            List<Point> joint_array_L = new List<Point> ();

            //관절 정보를 불러온다
            IReadOnlyDictionary<JointType, Joint> joints;

            int temp_count = 0;

            while (bodies.Count() != 0)
            {
                joints = bodies.Dequeue().Joints;
                //양 손의 좌표를 따로 저장한다
                joint_array_R.Add(skeleton_i.get_RHandPoint(temp_count));
                joint_array_L.Add(skeleton_i.get_LHandPoint(temp_count));
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
                rightX1 = joint_array_R[i].X;
                rightY1 = joint_array_R[i].Y;
                //rightZ1 = joint_array_R[i].Z;

                rightX2 = joint_array_R[i + 1].X;
                rightY2 = joint_array_R[i + 1].Y;
               // rightZ2 = joint_array_R[i + 1].Position.Z;

                //get the individual points of the left hand
                leftX1 = joint_array_L[i].X;
                leftY1 = joint_array_L[i].Y;
               // leftZ1 = joint_array_L[i].Position.Z;

                leftX2 = joint_array_L[i + 1].X;
                leftY2 = joint_array_L[i + 1].Y;
                //leftZ2 = joint_array_L[i + 1].Position.Z;

                //check x change
                if (rightX2 - rightX1 > 0)
                {
                    check_x_R++;
                }
                else if (leftX2 - leftX1 > 0)
                {
                    check_x_L++;
                }

                //check y change
                if (rightY2 - rightY1 > 0)
                {
                    check_y_R++;
                }
                else if (leftY2 - leftY1 > 0)
                {
                    check_y_L++;
                }

               /* //check z change
                if (rightZ1 - rightZ2 > 0)
                {
                    check_z_R++;
                }
                else if (leftZ1 - leftZ2 > 0)
                {
                    check_z_L++;
                }*/
            }

            //return the motion num if the differentials match
         //   MessageBox.Show(check_x_R.ToString());
            if (check_x_R > 10)
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
         //   MessageBox.Show("인식");
            MainForm.slideIndex = MainForm.slide.SlideIndex + 1;
            if (MainForm.slideIndex > MainForm.slidescount)
            {
                MessageBox.Show("It is already last page");
            }
            else
            {
                try
                {
                    MainForm.slide = MainForm.slides[MainForm.slideIndex];
                    MainForm.slides[MainForm.slideIndex].Select();
                }
                catch
                {
                    MainForm.pptApp.SlideShowWindows[1].View.Next();
                    MainForm.slide = MainForm.pptApp.SlideShowWindows[1].View.Slide;
                }
            }
            
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
