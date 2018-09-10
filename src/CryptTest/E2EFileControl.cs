using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL.BinaryTools;

namespace CryptTest
{
    public partial class E2EFileControl : UserControl
    {
        public E2EFileControl()
        {
            InitializeComponent();
        }

        private void selectSourceBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.ShowDialog();
                sourceTb.Text = fd.FileName;
            }
        }

        private void selectTargetBtn_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog fd = new SaveFileDialog())
            {
                fd.ShowDialog();
                targetTb.Text = fd.FileName;
            }
        }

        private void genKeyBtn_Click(object sender, EventArgs e)
        {
            byte[] key = new byte[32];
            using (RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider())
                csp.GetBytes(key);
            keyTb.Text = Util.ToHexString(key);
        }

        private void genIvBtn_Click(object sender, EventArgs e)
        {
            byte[] iv = new byte[16];
            using (RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider())
                csp.GetBytes(iv);
            ivTb.Text = Util.ToHexString(iv);
        }

        private async void encryptBtn_Click(object sender, EventArgs e)
        {
            using (Aes aes = Aes.Create())
            {
                using (ICryptoTransform transform = aes.CreateEncryptor(Util.GetBytes(keyTb.Text), Util.GetBytes(ivTb.Text)))
                {
                    await ProcessFile(transform);
                }
            }
        }

        private async void decryptBtn_Click(object sender, EventArgs e)
        {
            using (Aes aes = Aes.Create())
            {
                using (ICryptoTransform transform = aes.CreateDecryptor(Util.GetBytes(keyTb.Text), Util.GetBytes(ivTb.Text)))
                {
                    await ProcessFile(transform);
                }
            }
        }

        private async Task ProcessFile(ICryptoTransform cipher)
        {
            if (!File.Exists(sourceTb.Text))
            {
                MessageBox.Show("Die angegebene Datei konnte nicht gefunden werden");
                return;
            }
            if (!Directory.Exists(Path.GetDirectoryName(targetTb.Text)))
            {
                MessageBox.Show("Der angegebene Pfad konnte nicht gefunden werden");
                return;
            }
            byte[] outerHashRGB = null;
            byte[] innerHashRGB = null;
            await Task.Run(() =>
            {
                HashAlgorithm outerHash = SHA256.Create();
                HashAlgorithm innerHash = SHA256.Create();

                FileStream readStream = new FileStream(sourceTb.Text, FileMode.Open, FileAccess.Read);
                FileStream writeStream = new FileStream(targetTb.Text, FileMode.Create, FileAccess.Write);

                CryptoStream outerStream = new CryptoStream(readStream, outerHash, CryptoStreamMode.Read);
                CryptoStream cipherStream = new CryptoStream(outerStream, cipher, CryptoStreamMode.Read);
                CryptoStream innerStream = new CryptoStream(cipherStream, innerHash, CryptoStreamMode.Read);

                byte[] buffer = new byte[256];
                while (true)
                {
                    int length = innerStream.Read(buffer, 0, 256);
                    if (length > 0)
                        writeStream.Write(buffer, 0, length);
                    else
                        break;
                }

                outerHashRGB = outerHash.Hash;
                innerHashRGB = innerHash.Hash;

                outerStream.Close();
                cipherStream.Close();
                innerStream.Close();

                readStream.Close();
                writeStream.Close();

                outerHash.Dispose();
                innerHash.Dispose();
            });
            sourceShaLb.Text = Util.ToHexString(outerHashRGB);
            targetShaLb.Text = Util.ToHexString(innerHashRGB);
        }
    }
}
