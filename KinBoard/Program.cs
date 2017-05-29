using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KinBoard
{
    static class Program
    {
        /// <summary>
        /// main entry
        /// </summary>
        [STAThread]
        static void Main()
        {
            KinBoard _KinBoard;
            _KinBoard = new KinBoard();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
