using KeePassLib.Keys;
using KeePassLib.Serialization;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyProviderTest
{
    public partial class CreateFingerPrint : Form
    {
        
        public CreateFingerPrint()
        {
            InitializeComponent();
        }

 
        Boolean FingerPrintCheck = false;
        String Message;
        int portrandom = 8000;
        byte[] publickeybuffer;
        static byte[] entropy = { 9, 8, 7, 6, 5 };
        public Boolean Access()
        {

            return FingerPrintCheck;
        }
        private void CreateFingerPrint_Load(object sender, EventArgs e)
        {
            //創建rsa加密
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            string publickey = rsa.ToXmlString(false);
            string privatekey = rsa.ToXmlString(true);
            string index = privatekey.Substring(privatekey.IndexOf("<Modulus>")+9, 5);
            
            QRcodeGenerater(index,privatekey);
            storepublickey(publickey);
            

        }

        

        private void storepublickey(String publickey)
        {
            try
            {
                String filename = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\publickey.dat";
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                byte[] toEncrypt = UnicodeEncoding.ASCII.GetBytes(publickey);
                FileStream fStream = new FileStream(filename, FileMode.OpenOrCreate);
                int bytesWritten = EncryptDataToStream(toEncrypt, entropy, DataProtectionScope.CurrentUser, fStream);
                txtStatus.Text += "\r\n" + filename;
                fStream.Close();
                FingerPrintCheck = true;
            }
            catch(Exception e)
            {
                txtStatus.Text += e.ToString();
            }
            
        }

        public static int EncryptDataToStream(byte[] Buffer, byte[] Entropy, DataProtectionScope Scope, Stream S)
        {
            if (Buffer == null)
                throw new ArgumentNullException("Buffer");
            if (Buffer.Length <= 0)
                throw new ArgumentException("Buffer");
            if (Entropy == null)
                throw new ArgumentNullException("Entropy");
            if (Entropy.Length <= 0)
                throw new ArgumentException("Entropy");
            if (S == null)
                throw new ArgumentNullException("S");

            int length = 0;

            // Encrypt the data and store the result in a new byte array. The original data remains unchanged.
            byte[] encryptedData = ProtectedData.Protect(Buffer, Entropy, Scope);

            // Write the encrypted data to a stream.
            if (S.CanWrite && encryptedData != null)
            {
                S.Write(encryptedData, 0, encryptedData.Length);

                length = encryptedData.Length;
            }

            // Return the length that was written to the stream. 
            return length;

        }

        private void Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void QRcodeGenerater(string index,string privatekey)
        {
            MessagingToolkit.QRCode.Codec.QRCodeEncoder QREncoder = new MessagingToolkit.QRCode.Codec.QRCodeEncoder();
            String ipString;

            ipString = "C"+index+"key:" +privatekey.ToString();

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
/*
        private void Data_Receceived(object sender, SimpleTCP.Message e)
        {
            txtStatus.Invoke((MethodInvoker)delegate ()
            {
                txtStatus.Text += e.MessageString;
                if (e.MessageString == "123")
                {
                    Message = e.MessageString;
                    server.Broadcast(publickeybuffer);
                    FingerPrintCheck = true;
                    Close();
                }

                // e.ReplyLine(string.Format("You Said: {0}",e.MessageString));
            });
        }
*/

    }
}
