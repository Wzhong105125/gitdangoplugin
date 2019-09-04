using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeyProviderTest
{
    /// <summary>
    ///isdango = o =dango ,1=normal , 2=dangolite 
    /// </summary>
    public partial class DirectOrDango : Form
    {
        public int isDango = 0;
        public int Access()
        {
            return isDango;
        }
        public DirectOrDango()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isDango = 1;
            Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            isDango = 2;
            Close();
        }

    }
}
