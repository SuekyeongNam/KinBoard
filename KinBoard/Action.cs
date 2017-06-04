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
        static public int inc_x_R;
        static public int dec_x_R;
        static public double[] kkk = new double[5];
        static public double[] aaa = new double[21];
        static public int count_temp = 0;

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
        public void compare(Skeleton skeleton_i, char which_hand)
        {
            //get body Queue
            
            Queue<Body> bodies = skeleton_i.get_bodies();

            //joint arrays for each hands at each frames
            List<Point> joint_array_R = new List<Point> ();
            List<Point> joint_array_L = new List<Point> ();

            //관절 정보를 불러온다
            IReadOnlyDictionary<JointType, Joint> joints;

            int temp_count = 0;

            joint_array_L.Clear();
            joint_array_R.Clear();

            int q = 0;
            for (int i = 0; i < 21; i++)
            {
                //joints = bodies.Dequeue().Joints;
                //bodies.Dequeue();
                //양 손의 좌표를 따로 저장한다
                joint_array_R.Add(skeleton_i.get_RHandPoint(i));
                joint_array_L.Add(skeleton_i.get_LHandPoint(i));
                aaa[q] = joint_array_R[q].X;
                q++;
                temp_count++;
            }
            //right hand coordinate
            double rightX1, rightY1, rightZ1;
            double rightX2, rightY2, rightZ2;
            //left hand coordinate
            double leftX1, leftY1, leftZ1;
            double leftX2, leftY2, leftZ2;

            //check movement
            inc_x_R = 0;
            int inc_x_L = 0;
            int inc_y_R = 0;
            int inc_y_L = 0;
            int inc_z_R = 0;
            int inc_z_L = 0;

            dec_x_R = 0;
            int dec_x_L = 0;
            int dec_y_R = 0;
            int dec_y_L = 0;
            int dec_z_R = 0;
            int dec_z_L = 0;

            //calculate variance between adjacent frame coordinate component
            for (int i = 0; i < temp_count - 5; i+=5)
            {
                //get the individual points of the right hand
                rightX1 = joint_array_R[i].X;
                rightY1 = joint_array_R[i].Y;
                //rightZ1 = joint_array_R[i].Z;

                rightX2 = joint_array_R[i + 5].X;
                rightY2 = joint_array_R[i + 5].Y;
               // rightZ2 = joint_array_R[i + 1].Position.Z;

                //get the individual points of the left hand
                leftX1 = joint_array_L[i].X;
                leftY1 = joint_array_L[i].Y;
               // leftZ1 = joint_array_L[i].Position.Z;

                leftX2 = joint_array_L[i + 5].X;
                leftY2 = joint_array_L[i + 5].Y;
                //leftZ2 = joint_array_L[i + 1].Position.Z;

                int a = i / 5;
                kkk[a] = rightX2 - rightX1;

                //check x change
                if (rightX2 - rightX1 > 0)
                {
                    inc_x_R++;
                }
                else if(rightX2 - rightX1 < 0)
                {
                    dec_x_R++;
                }
             //   else if (leftX2 - leftX1 > 0)
              //  {
               //     check_x_L++;
                //}

                //check y change
                if (rightY2 - rightY1 > 0)
                {
                    inc_y_R++;
                }
                else
                {
                    dec_y_R++;
                }
            //    else if (leftY2 - leftY1 > 0)
             //   {
              //      check_y_L++;
               // }

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
            if (inc_x_R > dec_x_R)
            {
                turnPage();
                
            }

            else if (inc_x_R < dec_x_R)
            {
                 PrevPage();
            }

                /*else
                {
                    //other motions

                }*/
            
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

        void PrevPage()
        {
            MainForm.slideIndex = MainForm.slide.SlideIndex - 1;
            if (MainForm.slideIndex == 0)
            {
                MessageBox.Show("It is the first page");
                MainForm.slideIndex = 1;
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
                    MainForm.pptApp.SlideShowWindows[1].View.Previous();
                    MainForm.slide = MainForm.pptApp.SlideShowWindows[1].View.Slide;
                }
            }
        }
    }
}
