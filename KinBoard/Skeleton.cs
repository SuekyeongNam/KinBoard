using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using Microsoft.Kinect;

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

        public Skeleton() { }

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


