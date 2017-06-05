using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace KinBoard
{
    class HandWriting
    {
        [DllImport("user32")]
        public static extern Int32 SetCursorPos(Int32 x, Int32 y);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        int x, y;
        int prev_x, prev_y;

        //Mouse actions
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_MOVE = 0x01;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public HandWriting()
        {
            prev_x = 0;
            prev_y = 0;
            x = 0;
            y = 0;
        }

        public void SetCursor(int _x, int _y)
        {
            prev_x = x;
            prev_y = y;
            x = _x;
            y = _y;
        }

        public void DoMouseClick() {
            //Call the imported function with the cursor's current position
            //mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)x, (uint)y, 0, 0);
        }

        public void PenHide()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            MainForm.slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerPen;
            MainForm.slideShowView.PointerColor.RGB = Convert.ToInt32("0000FF", 16); // red color
            Cursor.Position = new Point(x, y);
        }
        public void EraseHide()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            MainForm.slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerEraser;
            Cursor.Position = new Point(x, y);
        }
        public void Pen()
        {
            if(prev_x != 0 && prev_y != 0)
            {
             MainForm.slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerPen;
             MainForm.slideShowView.PointerColor.RGB = Convert.ToInt32("0000FF", 16); // red color
             mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
             Cursor.Position = new Point(prev_x, prev_y);
             mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
             mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
             Cursor.Position = new Point(x, y);
             mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
             mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
             MainForm.slideShowView.DrawLine(0, 0, 10, 10);

            }
        }

        public void Erase()
        {
            MainForm.slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerEraser;
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Cursor.Position = new Point(prev_x, prev_y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Cursor.Position = new Point(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            //MainForm.slideShowView.DrawLine(prev_x, prev_y, x, y);

        }
        public void HighlightPen()
        {
            MainForm.slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerPen;
            MainForm.slideShowView.PointerColor.RGB = Convert.ToInt32("00FFFF", 16); // yellow color
            //MainForm.slideShowView.PointerColor.Brightness = 0.3F;
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Cursor.Position = new Point(prev_x, prev_y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Cursor.Position = new Point(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            MainForm.slideShowView.DrawLine(0, 0, 10, 10);

        }

        public void HighlightPenHide()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            MainForm.slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerPen;
            MainForm.slideShowView.PointerColor.RGB = Convert.ToInt32("00FFFF", 16); // yellow color
            //MainForm.slideShowView.PointerColor.Brightness = 0.3F;
            
            Cursor.Position = new Point(x, y);
        }

        public void EndClick()
        {
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_ABSOLUTE, (uint)x, (uint)y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }
    }
}
