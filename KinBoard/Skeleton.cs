using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using Microsoft.Kinect;
using System.Windows.Forms;

/* Joint Type name
    •SpineBase
    •SpineMid
    •Neck
    •Head
    •ShoulderLeft
    •ElbowLeft
    •WristLeft
    •HandLeft
    •ShoulderRight
    •ElbowRight
    •WristRight
    •HandRight
    •HipLeft
    •KneeLeft
    •AnkleLeft
    •FootLeft
    •HipRight
    •KneeRight
    •AnkleRight
    •FootRight
    •SpineShoulder
    •HandTipLeft
    •ThumbLeft
    •HandTipRight
    •ThumbRight 
*/

/* Hand State num
    Unknown = 0,
    NotTracked = 1,
    Open = 2,
    Closed = 3,
    Lasso = 4 
*/

namespace KinBoard
{
    public class Skeleton
    {
        private int id;
        private HandState right_hand_state;
        private HandState left_hand_state;
        private Queue<Body> bodies;
        private List<Point> Right_Hand;
        private List<Point> Left_Hand;

        public Skeleton() {

            bodies = new Queue<Body>();
            Right_Hand = new List<Point>();
            Left_Hand = new List<Point>();
        }

        public Point get_RHandPoint(int index)
        {
            return Right_Hand[index];
        }

        public Point get_LHandPoint(int index)
        {
            return Left_Hand[index];
        }

        public Body get_body()
        {
            Body temp = null;

            if(bodies.Count != 0)
            {
                temp = bodies.ElementAt(bodies.Count);
            }
            
            return temp;
        }

        public Queue<Body> get_bodies()
        {
            return bodies;
        }

        public void set_body(Body temp)
        {
            
            bodies.Enqueue(temp);
            if(bodies.Count > 10)
            {
                bodies.Dequeue();
                
            }
            
        }

        public void set_Hands(Point Left, Point Right)
        {
            Right_Hand.Add(Right);
            Left_Hand.Add(Left);
        }
        
        public void set_id(int num)
        {
            id = num;
        }

        public void set_hand_state(HandState Rstate, HandState Lstate)
        {
            right_hand_state = Rstate;
            left_hand_state = Lstate;
        }

    }
}


