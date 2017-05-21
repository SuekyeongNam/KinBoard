using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Add PowerPoint namespace
using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace KinBoard
{
    public partial class MainForm : Form
    {
        public static KinBoard _KinBoard;
        PPt.Application pptApp;
        bool isRightHanded = true;

        public MainForm()
        {
            InitializeComponent();

            // Set two buttons disable
            this.LHandedBtn.Enabled = false;
            this.RHandedBtn.Enabled = false;
        }

        public MainForm(KinBoard _temp)
        {
            InitializeComponent();
            _KinBoard = _temp;
        }

        private void KinBoard_Load(object sender, EventArgs e)
        {
            // Checking whether Powerpoint file is open
            try
            {   // Get running Powerpoint application object
                pptApp = Marshal.GetActiveObject("PowerPoint.Application") as PPt.Application;

                // Get powerpoint application successfully, then set two buttons enable
                this.LHandedBtn.Enabled = true;
                this.RHandedBtn.Enabled = true;
            }
            catch
            {
                MessageBox.Show("[Error] PowerPoint file did not open!\nYou must open a file before running this program to use.", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
        }

        private void LHandedBtn_Click(object sender, EventArgs e)
        {
            // For left-handed person
            isRightHanded = false;
        }

        private void RHandedBtn_Click(object sender, EventArgs e)
        {
            // For right-handed person
            isRightHanded = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If you click the close button on form
            if (MessageBox.Show("Do you want to exit the program?", "Exit", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
        }
    }
}
