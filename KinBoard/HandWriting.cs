using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace KinBoard
{
    class HandWriting
    {
        PPt.SlideShowSettings slideShowSettings;
        PPt.SlideShowView slideShowView;
        //bool isRightHanded;

        //public HandWriting()
        //{
        //    isRightHanded = true;
        //}

        //public HandWriting(bool _isRightHanded)
        //{
        //    isRightHanded = _isRightHanded;
        //}

        //private void SetIsRightHanded(bool _isRightHanded)
        //{
        //    isRightHanded = _isRightHanded;
        //}

        private void Pen()
        {
            slideShowView.PointerColor.RGB = Convert.ToInt32("FF0000", 16);
            slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerPen;
        }

        private void Erase()
        {
            slideShowView.PointerType = PPt.PpSlideShowPointerType.ppSlideShowPointerEraser;
        }
    }
}
