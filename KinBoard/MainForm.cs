﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// add PowerPoint namespace
using PPt = Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace KinBoard
{
    public partial class MainForm : Form
    {
        public static KinBoard _KinBoard;
        PPt.Application pptApp;

        public MainForm()
        {
            InitializeComponent();
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
            {
                pptApp = Marshal.GetActiveObject("PowerPoint.Application") as PPt.Application;
            }
            catch
            {
                MessageBox.Show("Please open powerpoint file");   
            }
        }

        private void LHandedBtn_Click(object sender, EventArgs e)
        {
            // For left-handed person
        }

        private void RHandedBtn_Click(object sender, EventArgs e)
        {
            
        }

    }
}