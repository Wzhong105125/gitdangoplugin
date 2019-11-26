using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using KeePassLib.Keys;
using System.Threading;
using System.Security.Cryptography;

namespace KeyProviderTest
{
    public partial class Login : Form
    {
        private Info m_Info = null;
        private KeyProviderQueryContext m_kpContext = null;
        private byte[] m_bmode = null;
        String mailbody,mail;
        private string masterkey;
        private int counter = 0;
        
        public void InitEx(Info Info,KeyProviderQueryContext ctx)
        {
            
            m_Info = Info;
            m_kpContext = ctx;
            masterkey = System.Text.Encoding.UTF8.GetString(Info.Secret);
            //masterkey = DeCode(Info.Secret.ToString());
        }
        public byte[] Access()
        {
            return m_bmode;
        }
        public Login()
        {
            InitializeComponent();
            
        }

        private String getMasterKey() {
            return masterkey;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread threadmail = new Thread(new ThreadStart(sendmail));
            string str = textBox1.Text.ToString();
            byte[] by = Encoding.UTF8.GetBytes(str);

            m_bmode = by;
            if (System.Text.Encoding.UTF8.GetString(Access()) == getMasterKey())
            {
                Close();
            }
            else {
                counter++;
                threadmail.Start();
                textBox1.Text = "";
                MessageBox.Show("Wrong Password");
            }
                   

        }
        
        

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.UseSystemPasswordChar) textBox1.UseSystemPasswordChar = false;
            else textBox1.UseSystemPasswordChar = true;
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    string str = textBox1.Text.ToString();
                    byte[] by = Encoding.UTF8.GetBytes(str);
                    Thread threadmail = new Thread(new ThreadStart(sendmail));
                    m_bmode = by;
                    if (System.Text.Encoding.UTF8.GetString(Access()) == getMasterKey())
                    {
                        Close();
                    }
                    else
                    {
                        counter++;
                        threadmail.Start();
                        textBox1.Text = "";
                        MessageBox.Show("Wrong Password");
                    }
                    break;
                default:
                    break;
            }
        }

        private String readmailaddress() {
            String mail = "";
            string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\email.txt";
            mail = System.IO.File.ReadAllText(path);
  //          MessageBox.Show(this,mail.ToString());
            return mail;
        }
        private void sendmail() {
            try {
                String mail = readmailaddress();
                mailbody_write();
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(mail);
                msg.From = new MailAddress("NTCU.DaogoPass@gmail.com", "NTCU_DangoPass", System.Text.Encoding.UTF8);
                msg.Subject = "資料庫開啟通知";//郵件標題
                msg.SubjectEncoding = System.Text.Encoding.UTF8;//郵件標題編碼
                msg.Body = mailbody; //郵件內容
                msg.BodyEncoding = System.Text.Encoding.UTF8;//郵件內容編碼 
  //              msg.Attachments.Add(new Attachment(@"D:\test2.docx"));  //附件
                msg.IsBodyHtml = false;//是否是HTML郵件 
                                      //msg.Priority = MailPriority.High;//郵件優先級 

                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential("NTCU.DangoPass@gmail.com", "Sec@Lab_3160"); //這裡要填正確的帳號跟密碼
                client.Host = "smtp.gmail.com"; //設定smtp Server
                client.Port = 587; //設定Port
                client.EnableSsl = true; //gmail預設開啟驗證
                client.Send(msg); //寄出信件
                client.Dispose();
                msg.Dispose();
   //             MessageBox.Show(this, "郵件寄送成功！");
            }
            catch (Exception ex) {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void mailbody_write() {
            String username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            
            mailbody = "UserName:" + Environment.UserName + "\r\nTime:" + DateTime.Now.ToString() + "\r\nDataBase:" + System.IO.Path.GetDirectoryName(Application.ExecutablePath)+"\r\nFail:"+counter;
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
    }
}
