using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AwaitTest;
using NewLife.Log;

namespace WinFormTest
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(Object sender, EventArgs e)
        {
            Await.Start();
            XTrace.WriteLine("OK!");
        }

        private void FrmMain_Load(Object sender, EventArgs e)
        {
            XTrace.UseWinFormControl(richTextBox1);
        }
    }
}