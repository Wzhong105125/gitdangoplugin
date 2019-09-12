using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace KeyProviderTest
{
    public partial class FingerPrint : Form
    {
        public FingerPrint()
        {
            InitializeComponent();
        }

        SimpleTcpServer server;
        Boolean FingerPrintCheck = false;
        String Message;
        int createorlogin,number,portrandom=8000;
        static byte[] entropy = { 9, 8, 7, 6, 5 };
        String randomnumber;
        byte[] randombuffer;
        String privatekey;
        public Boolean Access()
        {
            
            return FingerPrintCheck;
        }
        private void FingerPrint_Load(object sender, EventArgs e)
        {
            
            //開啟server
            IPAddress ipAddr = null;
            IPAddress[] arrIP = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in arrIP)
            {
                if (System.Net.Sockets.AddressFamily.InterNetwork.Equals(ip.AddressFamily))
                {
                    ipAddr = ip;
                }
                else if (System.Net.Sockets.AddressFamily.InterNetworkV6.Equals(ip.AddressFamily))
                {
                    ipAddr = ip;
                }
            }
            server = new SimpleTcpServer();
            
            server.Delimiter = 0x13;
            server.StringEncoder = Encoding.UTF8;
            server.DataReceived += Data_Receceived;
            try
            {
                server.Start(ipAddr, portrandom);
            }
            catch (Exception error)
            {
                txtStatus.Text += "\r\n" + error.Message.ToString() ;
                Random rnd = new Random();
                int random = rnd.Next(1, 100);
                portrandom+=random;
                server.Start(ipAddr, portrandom);
            }
            
            txtStatus.Text += "Server Start...";
            txtStatus.Text += "\r\n" + ipAddr+"port"+portrandom ;
            QRcodeGenerater(ipAddr,portrandom);
            privatekey = ReadPrivatekey();
            
            
        }

        private string ReadPrivatekey()
        {
            String filename = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\privatekey.dat";
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            FileStream fStream = new FileStream(filename, FileMode.Open);
            byte[] decryptData = DecryptDataFromStream(entropy, DataProtectionScope.CurrentUser, fStream,2048);
            String privatekey = Encoding.Default.GetString(decryptData);
  //         txtStatus.Text += "\r\n key:" + privatekey;
            return privatekey;
        }

        public static byte[] DecryptDataFromStream(byte[] Entropy, DataProtectionScope Scope, Stream S, int Length)
        {
            if (S == null)
                throw new ArgumentNullException("S");
            if (Length <= 0)
                throw new ArgumentException("Length");
            if (Entropy == null)
                throw new ArgumentNullException("Entropy");
            if (Entropy.Length <= 0)
                throw new ArgumentException("Entropy");



            byte[] inBuffer = new byte[Length];
            byte[] outBuffer;

            // Read the encrypted data from a stream.
            if (S.CanRead)
            {
                S.Read(inBuffer, 0, Length);

                outBuffer = ProtectedData.Unprotect(inBuffer, Entropy, Scope);
            }
            else
            {
                throw new IOException("Could not read the stream.");
            }

            // Return the length that was written to the stream. 
            return outBuffer;

        }

        private void QRcodeGenerater(IPAddress ip,int portrandom)
        {
            MessagingToolkit.QRCode.Codec.QRCodeEncoder QREncoder = new MessagingToolkit.QRCode.Codec.QRCodeEncoder();
            String ipString;
            
            ipString = "Lip:" + ip.ToString() + "port:"+portrandom;
            


                
            // 2.大小
            QREncoder.QRCodeScale = 8;

            // 3.取得將編碼的內容
            string EnCoderString = ipString;

            // 4.編碼成Bitmap
            Bitmap bitmap = QREncoder.Encode(EnCoderString);

            // 5.於image元件顯示
            pictureBox1.Image = bitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Data_Receceived(object sender, SimpleTCP.Message e)
        {
            txtStatus.Invoke((MethodInvoker)delegate ()
            {
                if (e.MessageString == "123")
                {
                    randomnumber = GenerateRandom();
                    server.Broadcast(randombuffer);
                    Message = e.MessageString;
                    txtStatus.Text += "\r\nbroadcast: " + randomnumber;
                }else
                {
  //                privatekey = ReadPrivatekey();
                    txtStatus.Text += "\r\nreceived:" + e.MessageString+"\r\n length:"+System.Text.Encoding.Default.GetByteCount(e.MessageString);
                    byte[] aa = Encoding.UTF8.GetBytes(e.MessageString);
                    Boolean k = CheckRandom(e.MessageString);
                    if (k == true)
                        FingerPrintCheck = true;
                    else
                        FingerPrintCheck = false;
                    Close();
                }
                
               // e.ReplyLine(string.Format("You Said: {0}",e.MessageString));
            });
        }


        private Boolean CheckRandom(String encryptrandom)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            String decryptedrandom ="";
            Boolean kkk = true;
            rsa.FromXmlString(privatekey);
            
            try{
                decryptedrandom = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encryptrandom), false));
                txtStatus.Text += "\r\ndecrypted" + decryptedrandom;
            }
            catch(Exception e)
            {
                txtStatus.Text += "\r\nerror:" +e.ToString();
            }      

            if (decryptedrandom.Equals(randomnumber))
                return true;
            else
                return false;
            
            
        }
        private String GenerateRandom()
        {
            string s = "";
            int i, number;
            Random rnd = new Random();
            for (i = 0; i < 16; i++)
            {
                number = rnd.Next(0, 9);
                byte[] bb = BitConverter.GetBytes(number);
                s = s + number.ToString();
            }
            randomnumber = s;
            randombuffer = Encoding.UTF8.GetBytes(s+"\r\n");
  //          txtStatus.Text += "\r\nrandom:" + s;
            return s;
        }

        private void encrypt()
        {

        }


        
    }
}
