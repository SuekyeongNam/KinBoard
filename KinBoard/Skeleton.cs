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

namespace KinBoard
{
    public class Skeleton
    {
        private int id;
        private int hand_state;
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

        public void set_body(Body temp)
        {
            bodies.Enqueue(temp);
        }

    }
}


