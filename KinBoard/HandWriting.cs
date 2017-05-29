using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace KinBoard
{
    class HandWriting
    {
        [DllImport("user32")]
        public static extern Int32 SetCursorPos(Int32 x, Int32 y);

        PPt.SlideShowSettings slideShowSettings;
        PPt.SlideShowView slideShowView;

        public HandWriting()
        {

        }

        public void SetCursor(int _x, int _y)
        {
            SetCursorPos(_x, _y);
        }

        public void Pen()
        {
            slideShowView.PointerColor.RGB = Convert.ToInt32("FF0000", 16); // red
            slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerPen;
        }

        public void Erase()
        {
            slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerEraser;
        }
    }
}
