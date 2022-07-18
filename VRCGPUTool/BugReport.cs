using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;

namespace VRCGPUTool
{
    public partial class BugReport : Form
    {
        public BugReport(int typeIndex)
        {
            InitializeComponent();
            if(typeIndex == 0)
            {
                bug.Checked = true;
            }
            else
            {
                func.Checked = true;
            }
            openFileDialog1.Filter = "画像ファイル(*.png, *.jpg)|*.png;*.jpg";
        }

        private void Submit(object sender, EventArgs e)
        {

        }

        private void fileadd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = openFileDialog1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {

            }
        }
    }
}
