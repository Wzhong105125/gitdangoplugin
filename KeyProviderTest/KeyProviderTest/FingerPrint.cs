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
        int portrandom=8000;
        static byte[] entropy = { 9, 8, 7, 6, 5 };
        String randomnumber;
        byte[] randombuffer;
        String publickey;
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
                int rr = rnd.Next(1, 100);
                portrandom+=rr;
                server.Start(ipAddr, portrandom);
            }
            
            txtStatus.Text += "Server Start...";
            txtStatus.Text += "\r\n" + ipAddr+"port"+portrandom ;
            publickey = ReadPublickey();
            string index = publickey.Substring(publickey.IndexOf("<Modulus>") + 9, 5);
            string random = GenerateRandom();
            QRcodeGenerater(ipAddr,portrandom,index,random);
            
            
        }

        private string ReadPublickey()
        {
            String filename = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\publickey.dat";
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            FileStream fStream = new FileStream(filename, FileMode.Open);
            byte[] decryptData = DecryptDataFromStream(entropy, DataProtectionScope.CurrentUser, fStream,2048);
            String publickey = Encoding.Default.GetString(decryptData);
            //      txtStatus.Text += "\r\n key:" + publickey;
            fStream.Close();
            return publickey;
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

        private void QRcodeGenerater(IPAddress ip,int portrandom,string index,string random)
        {
            MessagingToolkit.QRCode.Codec.QRCodeEncoder QREncoder = new MessagingToolkit.QRCode.Codec.QRCodeEncoder();
            String ipString;
            
            ipString = "L"+index+"random:"+random+"ip:" + ip.ToString() + "port:"+portrandom;        
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
                txtStatus.Text += "\r\nreceived:" + e.MessageString;
                if(checkrandom(e.MessageString) == true)
                {
                    txtStatus.Text += "\r\n" +"true";
                    FingerPrintCheck = true;
                    Close();
                }
                else
                {
                    txtStatus.Text += "\r\n" + "false";
                    Close();
                }
            });
        }

        private Boolean checkrandom(String received)
        {
            bool success = false;
            byte[] bytesToVerify = Encoding.UTF8.GetBytes(randomnumber);
            byte[] signedBytes = Convert.FromBase64String(received);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            try
            {
                
                rsa.FromXmlString(publickey);

                SHA256Managed Hash = new SHA256Managed();

                byte[] hashedData = Hash.ComputeHash(signedBytes);
                
                success = rsa.VerifyData(bytesToVerify, new SHA256CryptoServiceProvider(), signedBytes);
                txtStatus.Text += "\r\nsuccess:"+success.ToString();
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
            return success;

        }

        private String hashrandom(String random)
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] source = Encoding.Default.GetBytes(random);
            byte[] crypto = sha256.ComputeHash(source);
            string result = Convert.ToString(crypto);
            txtStatus.Text += "\r\n random :"+random+"\r\nhash :" + result;
            return result;
        }

        private String decryptrandom(String encryptedrandom)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            String decryptedrandom = "";
            rsa.FromXmlString(publickey);

            try
            {
                decryptedrandom = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encryptedrandom), false));
                txtStatus.Text += "\r\ndecrypted" + decryptedrandom;
            }
            catch (Exception e)
            {
                txtStatus.Text += "\r\nerror:" + e.ToString();
            }
            return decryptedrandom;      
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


            
        
    }
}
