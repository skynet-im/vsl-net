using System;
using System.Text;
using System.Windows.Forms;
using VSL.BinaryTools;
using VSL.Crypt;

namespace CryptTest.Controls
{
    public partial class AesControl : UserControl
    {
        public AesControl()
        {
            InitializeComponent();
        }

        private void btnAesGenerate_Click(object sender, EventArgs e)
            => tbAesKey.Text = Util.ToHexString(AesStatic.GenerateKey());

        private void btnAesGenerateIV_Click(object sender, EventArgs e)
            => tbAesIV.Text = Util.ToHexString(AesStatic.GenerateIV());

        private void btnAesDecrypt_Click(object sender, EventArgs e)
        {
            byte[] ciphertext = Util.GetBytes(tbAesCipherText.Text);
            byte[] key = Util.GetBytes(tbAesKey.Text);
            byte[] iv = Util.GetBytes(tbAesIV.Text);
            byte[] plaintext = AesStatic.Decrypt(ciphertext, key, iv);
            if (EncodingUTF8Rb.Checked)
                tbAesPlainText.Text = Encoding.UTF8.GetString(plaintext);
            else
                tbAesPlainText.Text = Util.ToHexString(plaintext);
            tbAesCipherText.Text = "";
        }

        private void btnAesEncrypt_Click(object sender, EventArgs e)
        {
            byte[] plaintext;
            if (EncodingUTF8Rb.Checked)
                plaintext = Encoding.UTF8.GetBytes(tbAesPlainText.Text);
            else
                plaintext = Util.GetBytes(tbAesPlainText.Text);
            byte[] key = Util.GetBytes(tbAesKey.Text);
            byte[] iv = Util.GetBytes(tbAesIV.Text);
            byte[] ciphertext = AesStatic.Encrypt(plaintext, key, iv);
            tbAesCipherText.Text = Util.ToHexString(ciphertext);
            tbAesPlainText.Text = "";
        }

        private void RadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbAesPlainText.Text))
            {
                if (EncodingUTF8Rb.Checked) // Convert to UTF-8
                    tbAesPlainText.Text = Encoding.UTF8.GetString(Util.GetBytes(tbAesPlainText.Text));
                else // Convert to binary
                    tbAesPlainText.Text = Util.ToHexString(Encoding.UTF8.GetBytes(tbAesPlainText.Text));
            }
        }
    }
}
