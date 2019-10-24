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

namespace KeyProviderTest
{
    public partial class Login : Form
    {
        private Info m_Info = null;
        private KeyProviderQueryContext m_kpContext = null;
        private byte[] m_bmode = null;
        String mailbody;
        private string masterkey;
        private int counter = 0;
        
        public void InitEx(Info Info,KeyProviderQueryContext ctx)
        {
            
            m_Info = Info;
            m_kpContext = ctx;
            masterkey = System.Text.Encoding.UTF8.GetString(Info.Secret);

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

        private void sendmail() {
            try {
                mailbody_write();
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add("qaz90014@gmail.com");
                msg.From = new MailAddress("ACS105125@gm.ntcu.edu.tw", "NTCU_DangoPass", System.Text.Encoding.UTF8);
                msg.Subject = "資料庫開啟通知";//郵件標題
                msg.SubjectEncoding = System.Text.Encoding.UTF8;//郵件標題編碼
                msg.Body = mailbody; //郵件內容
                msg.BodyEncoding = System.Text.Encoding.UTF8;//郵件內容編碼 
  //              msg.Attachments.Add(new Attachment(@"D:\test2.docx"));  //附件
                msg.IsBodyHtml = false;//是否是HTML郵件 
                                      //msg.Priority = MailPriority.High;//郵件優先級 

                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential("ACS105125@gm.ntcu.edu.tw", "wzhong210512"); //這裡要填正確的帳號跟密碼
                client.Host = "smtp.gmail.com"; //設定smtp Server
                client.Port = 587; //設定Port
                client.EnableSsl = true; //gmail預設開啟驗證
                client.Send(msg); //寄出信件
                client.Dispose();
                msg.Dispose();
 //               MessageBox.Show(this, "郵件寄送成功！");
            }
            catch (Exception ex) {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void mailbody_write() {
            String username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //           txtStatus.Text += "UserName:" + Environment.UserName;
            //           txtStatus.Text += "\r\nTime:" + DateTime.Now.ToString();
            //           txtStatus.Text += "\r\nDataBase:" + System.IO.Path.GetDirectoryName(Application.ExecutablePath);
           // txtStatus.Text += "\r\nDataBase:" + AppDomain.CurrentDomain.FriendlyName;
            mailbody = "UserName:" + Environment.UserName + "\r\nTime:" + DateTime.Now.ToString() + "\r\nDataBase:" + System.IO.Path.GetDirectoryName(Application.ExecutablePath)+"\r\nFail:"+counter;
        }
    }
}
