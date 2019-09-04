using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KeePassLib.Keys;

namespace KeyProviderTest.Forms
{
    public partial class Creation : Form
    {
        private Info m_Info = null;
        private KeyProviderQueryContext m_kpContext = null;
        public void InitEx(Info Info,KeyProviderQueryContext ctx)
        {
            m_Info = Info;
            m_kpContext = ctx;
        }
        public Creation()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.TextLength<8 || textBox1.TextLength>12)
            {
                MessageBox.Show("請輸入8到12位的密碼");
                textBox1.Text = "";
            }
            else
            {
                m_Info.Secret = Encoding.UTF8.GetBytes(textBox1.Text);
                Close();
            }
        }

        private void Creation_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.UseSystemPasswordChar) textBox1.UseSystemPasswordChar = false;
            else textBox1.UseSystemPasswordChar = true;
        }
    }
}
