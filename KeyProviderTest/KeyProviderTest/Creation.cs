using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
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
                WriteEmail();
                m_Info.Secret = Encoding.UTF8.GetBytes(textBox1.Text);
                //m_Info.Secret = Encoding.UTF8.GetBytes(EnCode(textBox1.Text.ToString()));
                Close();
            }
        }
        public string EnCode(string EnString)  //將字串加密
        {
            byte[] Key = { 123, 123, 123, 123, 123, 123, 123, 123 };
            byte[] IV = { 123, 123, 123, 123, 123, 123, 123, 123 };

            byte[] b = Encoding.UTF8.GetBytes(EnString);

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            ICryptoTransform ict = des.CreateEncryptor(Key, IV);
            byte[] outData = ict.TransformFinalBlock(b, 0, b.Length);
            return Convert.ToBase64String(outData);  //回傳加密後的字串
        }

        public string DeCode(string DeString) //將字串解密
        {
            byte[] Key = { 123, 123, 123, 123, 123, 123, 123, 123 };
            byte[] IV = { 123, 123, 123, 123, 123, 123, 123, 123 };
            byte[] b = Convert.FromBase64String(DeString);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            ICryptoTransform ict = des.CreateDecryptor(Key, IV);
            byte[] outData = ict.TransformFinalBlock(b, 0, b.Length);
            return Encoding.UTF8.GetString(outData);//回傳解密後的字串
        }
        private void WriteEmail() {
            string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\email.txt";
            if (!File.Exists(path))
            {
                File.Create(path);
                TextWriter tw = new StreamWriter(path);
                tw.WriteLine(email.Text.ToString());
                tw.Close();
            }
            else if (File.Exists(path))
            {
                TextWriter tw = new StreamWriter(path);
                tw.WriteLine(email.Text.ToString());
                tw.Close();
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
