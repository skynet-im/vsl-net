﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL.Crypt;

namespace CryptTest
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

        private async void btnAesDecrypt_Click(object sender, EventArgs e)
        {
            byte[] ciphertext = Util.GetBytes(tbAesCipherText.Text);
            byte[] key = Util.GetBytes(tbAesKey.Text);
            byte[] iv = Util.GetBytes(tbAesIV.Text);
            byte[] plaintext = await AesStatic.DecryptAsync(ciphertext, key, iv);
            if (EncodingUTF8Rb.Checked)
                tbAesPlainText.Text = Encoding.UTF8.GetString(plaintext);
            else
                tbAesPlainText.Text = Util.ToHexString(plaintext);
            tbAesCipherText.Text = "";
        }

        private async void btnAesEncrypt_Click(object sender, EventArgs e)
        {
            byte[] plaintext;
            if (EncodingUTF8Rb.Checked)
                plaintext = Encoding.UTF8.GetBytes(tbAesPlainText.Text);
            else
                plaintext = Util.GetBytes(tbAesPlainText.Text);
            byte[] key = Util.GetBytes(tbAesKey.Text);
            byte[] iv = Util.GetBytes(tbAesIV.Text);
            byte[] ciphertext = await AesStatic.EncryptAsync(plaintext, key, iv);
            tbAesCipherText.Text = Util.ToHexString(ciphertext);
            tbAesPlainText.Text = "";
        }

        private void RadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: Convert input
        }
    }
}
