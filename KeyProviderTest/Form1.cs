using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using KeePass.UI;
using KeePass.Plugins;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib.Serialization;
using KeyProviderTest.Forms;
using System.Net.Mail;
using System.Threading;
/*
pictureBox 1~36為第一個鍵盤
pictureBox 37~72為第二個鍵盤
pictureBox 73~108為認證系統之格
*/
namespace KeyProviderTest
{
    public partial class Form1 : Form
    {
        char[] Pass = new char[] { '[', '[', '[', '[', '[', '[', '[', '[', '[', '[', '[', '[' };//0~5 First Round, 6~11 Second Round
        bool[] modified = { true, false, false, false, false, false, true, false, false, false, false, false }; //檢驗有無更動
        int[] keyboard1 = new int[36];//大寫和特殊符號:前10為特殊符號,後26為大寫英文字母
        char[] Symbol_keyboard1 = { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M' };
        int[] keyboard2 = new int[36];//小寫和數字:前10為數字,後26為小寫英文字母
        char[] Symbol_keyboard2 = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };
        
        int[,] CirCle = new int[6, 6];//左方圓圈內之顏色
        int[] color = new int[6];//粉:0 黃:1 灰:2 藍:3 綠:4 白:5 null:6 
        int CurrentRow = 1,counter = 0;
        public int WhichRound = 1;
        Boolean[] Rounds = { false, false };
        Boolean check = false;
        String mailbody;
        private Info m_Info = null;
        private KeyProviderQueryContext m_kpContext = null;
        public void InitEx(Info Info, KeyProviderQueryContext ctx)
        {

            m_Info = Info;
            m_kpContext = ctx;
            char[] keyc = System.Text.Encoding.UTF8.GetString(m_Info.Secret).ToCharArray();
            
            for(int i = 0; i < keyc.Length; i++)
            {
                Pass[i] = keyc[i];
            }           
            //GetPassQ(Pass);

        }

        public Boolean Access()
        {
            return check;
        }
        public Form1()
        {
            InitializeComponent();
            SetlabelBack();
            PaintCircle();

            RandomColor();
            GenerateColor();
            RandomLoginColor();

        }
       
        
        public int[] GetPassQ(char[] PP)
        {
            int[] QQ = new int[6];
            switch (WhichRound)
            {
                case 1:
                    for (int i = 0; i < 6; i++)
                    {
                        QQ[i] = FindCharColor(PP[i]);
                    }
                    break;
                case 2:
                    for (int i = 6; i < 12; i++)
                    {
                        if (PP[i] == '[')
                        {
                            QQ[i-6] = 6;
                        }
                        else QQ[i-6] = FindCharColor(PP[i]);
                    }
                    break;
                default:
                    MessageBox.Show("Error Error");
                    break;
            }
            return QQ;
        }

        public void IsPassRight()//檢驗user密碼輸入正確與否
        {
            Boolean IsRight = false;
            int[] CurrentPass = GetPassQ(Pass);
            int count = 0;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (CurrentPass[j] == CirCle[j, i] || CurrentPass[j] == 6)
                        count++;
                }
                if (count == 6)
                {
                    IsRight = true;
                    break;
                }
                count = 0;
            }
            if (IsRight)
            {
                Rounds[WhichRound-1] = true;
            }
            else
            {
                Rounds[WhichRound-1] = false;
            }
            if (WhichRound == 1)
            {
                CurrentRow = 1;
                Arrow1.Location = new Point(636, 17+64);

                for (int i = 0; i < 6; i++)//將顏色清空重新產生
                {
                    color[i] = 0;
                }
                RRound.Text = "Current Round: 2";
                RandomColor();
                GenerateColor();
                RandomLoginColor();

            }

            if (Rounds[0] && Rounds[1])
            {
                Boolean allmod = true;
                for(int i = 0; i < 12; i++)
                {
                    if (!modified[i]) allmod = false;
                }
                if (allmod) check = true;
            }
        }

        public int FindCharColor(char CC) // 找char的可愛顏色
        {
            if (Array.IndexOf(Symbol_keyboard1, CC) != -1)
            {
                return keyboard1[Array.IndexOf(Symbol_keyboard1, CC)];
            }
            else 
            {
                return keyboard2[Array.IndexOf(Symbol_keyboard2, CC)];
            }
        }

        public void SetlabelBack()//設定右邊符號的背景色
        {
            for(int i =2; i <= 73; i++)
            {
                string labelll = "label" + i;
                Control ctro = this.Controls[labelll]; //控制TABLE中之圖
                ctro.BackColor = Color.White;
            }
        }

        public void ShiftColor(int row, int LR)//Test SLL=>LR:0 SRL=>LR:1
        {
            int[] temp = new int[6];
            switch (LR)
            {
                case 0:
                    for (int j = 0; j < 6; j++)
                    {
                        temp[j] = CirCle[row, j];
                    }
                    for (int k = 0; k < 6; k++)
                    {
                        if (k == 5) CirCle[row, k] = temp[0];
                        else CirCle[row, k] = temp[k + 1];
                    }
                    break;
                case 1:
 
                    for (int j = 0; j < 6; j++)
                    {
                        temp[j] = CirCle[row, j];
                    }
                    for (int k = 0; k < 6; k++)
                    {
                        if (k == 0) CirCle[row, k] = temp[5];
                        else CirCle[row, k] = temp[k - 1];
                    }
                    break;
                default:
                    Console.WriteLine("Error Error!!!");
                    break;
            }

             for(int i = 0; i < 6; i++)
             {
                string pictbox = "pictureBox" + (73 + 6 * row+i);
                Control ctro = tableLayoutPanel1.Controls[pictbox]; //控制TABLE中之圖
                int currentColor = CirCle[row,i];
                if (currentColor == 0) //紅色
                {
                    ctro.BackColor = Color.Pink;
                }
                else if (currentColor == 1) //黃色
                {
                    ctro.BackColor = Color.Yellow;
                }
                else if (currentColor == 2) //灰色
                {
                    ctro.BackColor = Color.Gray;
                }
                else if (currentColor == 3) //藍色
                {
                    ctro.BackColor = Color.Aqua;
                }
                else if (currentColor == 4) //綠色
                {
                    ctro.BackColor = Color.PaleGreen;
                }
                else if (currentColor == 5) //白色
                {
                    ctro.BackColor = Color.White;
                }
             }
        }

        public void PaintCircle()//把左方的pictureBox變成圓形
        {
            for(int i = 73; i <= 108; i++)
            {
                string pictbox = "pictureBox" + i;
                Control ctro = tableLayoutPanel1.Controls[pictbox]; //控制TABLE中之圖
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(ctro.ClientRectangle);
                Region reg = new Region(path);
                ctro.Region = reg;
            }
        }

        public void RandomLoginColor()//將左方之pictureBox著色並記錄
        {
            Random rnd = new Random();
            for(int i = 73; i <= 108; i+=6)
            {
                int[] ColorInRow = new int[6];
                for (int j = 0; j < 6; j++)
                {
                    string pictbox = "pictureBox" + (i+j);
                    Control ctro = tableLayoutPanel1.Controls[pictbox]; //控制TABLE中之圖

                    int dice = rnd.Next(0, 6);
                    while (ColorInRow[dice] == 1)
                    {
                        dice = rnd.Next(0, 6);
                    }
                    ColorInRow[dice] = 1;
                    this.CirCle[(i-73)/6,j] = dice; //紀錄Circle之顏色

                    int currentColor = dice;
                    if (currentColor == 0) //紅色
                    {
                        ctro.BackColor = Color.Pink;
                    }
                    else if (currentColor == 1) //黃色
                    {
                        ctro.BackColor = Color.Yellow;
                    }
                    else if (currentColor == 2) //灰色
                    {
                        ctro.BackColor = Color.Gray;
                    }
                    else if (currentColor == 3) //藍色
                    {
                        ctro.BackColor = Color.Aqua;
                    }
                    else if (currentColor == 4) //綠色
                    {
                        ctro.BackColor = Color.PaleGreen;
                    }
                    else if (currentColor == 5) //白色
                    {
                        ctro.BackColor = Color.White;
                    }
                }
            }
        }

        public void RandomColor()//random character's color
        {
            Random rnd = new Random();

            for (int i = 0; i < 36; i++) //random 大寫keyboard
            {
                int dice = rnd.Next(0, 6);
                while (this.color[dice] == 12)
                {
                    dice = rnd.Next(0, 6);
                }
                this.keyboard1[i] = dice;
                this.color[dice]++;
            }
            for (int i = 0; i < 36; i++) //random 小寫keyboard
            {
                int dice = rnd.Next(0, 6);
                while (this.color[dice] == 12)
                {
                    dice = rnd.Next(0, 6);
                }
                this.keyboard2[i] = dice;
                this.color[dice]++;
            }
        }

        public void GenerateColor()//random後將對應之格子著色
        {
            for (int i = 0; i < 36; i++)
            {
                string picbox = "pictureBox" + (i + 1);
                Control ctn = this.Controls[picbox];
                int currentColor = this.keyboard1[i];

                if (currentColor == 0) //紅色
                {
                    ctn.BackColor = Color.Pink;

                }
                else if(currentColor == 1) //黃色
                {
                    ctn.BackColor = Color.Yellow;
                }
                else if (currentColor == 2) //灰色
                {
                    ctn.BackColor = Color.Gray;
                }
                else if (currentColor == 3) //藍色
                {
                    ctn.BackColor = Color.Aqua;
                }
                else if (currentColor == 4) //綠色
                {
                    ctn.BackColor = Color.PaleGreen;
                }
                else if (currentColor == 5) //白色
                {
                    ctn.BackColor = Color.White;
                }
            }
            for (int i = 0; i < 36; i++)
            {
                string picbox = "pictureBox" + (i + 1 + 36);
                Control ctn = this.Controls[picbox];
                int currentColor = this.keyboard2[i];

                if (currentColor == 0) //紅色
                {
                    ctn.BackColor = Color.Pink;
                }
                else if (currentColor == 1) //黃色
                {
                    ctn.BackColor = Color.Yellow;
                }
                else if (currentColor == 2) //灰色
                {
                    ctn.BackColor = Color.Gray;
                }
                else if (currentColor == 3) //藍色
                {
                    ctn.BackColor = Color.Aqua;
                }
                else if (currentColor == 4) //綠色
                {
                    ctn.BackColor = Color.PaleGreen;
                }
                else if (currentColor == 5) //白色
                {
                    ctn.BackColor = Color.White;
                }
            }
        }
        
        private void Form1_KeyDown_1(object sender, KeyEventArgs e)//WASD控制方向
        {
            switch (e.KeyCode)
            {
                case Keys.R:
                    for (int i = 0; i < 6; i++)//清空色彩數目
                    {
                        color[i] = 0;
                    }
                    for (int i = 1; i < 6; i++)//reset modified
                    {
                        modified[i + (WhichRound-1) * 6] = false;
                    }
                    CurrentRow = 1;
                    Arrow1.Location = new Point(636, 17 + 64);
                    RandomColor();
                    GenerateColor();
                    RandomLoginColor();
                    GetPassQ(Pass);
                    ResetStick();
                    break;
                case Keys.Enter:
                    IsPassRight();
                    if (check && WhichRound == 2 && CurrentRow == 5)
                    {
                        Close();
                    }
                    else if (!check && WhichRound == 2)
                    {
                        MessageBox.Show("Wrong Password");
                        counter++;
                        Thread threadmail = new Thread(new ThreadStart(sendmail));
                        threadmail.Start();
                        Close();
                    }
                    if (WhichRound == 1)
                    {
                        RRound.Text = "Current Round: 2";
                        WhichRound = 2;
                    }
                    ResetStick();
                    break;
                case Keys.Left:
                    modified[CurrentRow + (6 * (WhichRound - 1))] = true;
                    ShiftColor(CurrentRow, 0);
                    break;
                case Keys.Right:
                    modified[CurrentRow + (6 * (WhichRound - 1))] = true;
                    ShiftColor(CurrentRow, 1);
                    break;
                    /*
                case Keys.Up:
                    if (CurrentRow == 0) break;
                    else CurrentRow -= 1;
                    break;
                    */
                case Keys.Down:
                    if (CurrentRow == 5) break;
                    else
                    {
                        CurrentRow += 1;
                        StickM();
                    }
                    break;
                default:
                    break;
            }
            switch (CurrentRow)
            {
                case 0:
                    Arrow1.Location = new Point(636, 17);
                    break;
                case 1:
                    Arrow1.Location = new Point(636, 17+64);
                    break;
                case 2:
                    Arrow1.Location = new Point(636, 17+64*2);
                    break;
                case 3:
                    Arrow1.Location = new Point(636, 17+64*3);
                    break;
                case 4:
                    Arrow1.Location = new Point(636, 17+64*4);
                    break;
                case 5:
                    Arrow1.Location = new Point(636, 17 + 64 * 5);
                    break;
            }
        }

        public void StickM()
        {
            for(int i = 1; i<=6; i++)
            {
                string stickS = "stick" + i;
                Control ctro = this.Controls[stickS]; //控制stick
                ctro.Location = new Point(ctro.Location.X, ctro.Location.Y + 58);
            }
        }
        public void ResetStick()
        {
            for (int i = 1; i <= 6; i++)
            {
                string stickS = "stick" + i;
                Control ctro = this.Controls[stickS]; //控制stick
                ctro.Location = new Point(ctro.Location.X, -243);
            }
        }

        private void RegenerateB_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < 6; i++)//清空色彩數目
            {
                color[i] = 0;
            }
            RandomColor();
            GenerateColor();
            RandomLoginColor();
            GetPassQ(Pass);
        }

        

        private void label1_Click(object sender, EventArgs e)
        {
            help dlg = new help();
            dlg.ShowDialog();
        }
        private String readmailaddress()
        {
            String mail = "";
            string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\email.txt";
            mail = System.IO.File.ReadAllText(path);
            //          MessageBox.Show(this,mail.ToString());
            return mail;
        }
        private void sendmail()
        {
            try
            {
                String mail = readmailaddress();
                mailbody_write();
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(mail);
                msg.From = new MailAddress("NTCU.DangoPass@gmail.com", "NTCU_DangoPass", System.Text.Encoding.UTF8);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void mailbody_write()
        {
            String username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            mailbody = "UserName:" + Environment.UserName + "\r\nTime:" + DateTime.Now.ToString() + "\r\nDataBase:" + System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\r\nFail:" + counter;
        }

    }
}
