﻿using System;
using System.Text;
using System.Windows.Forms;
using VSL.BinaryTools;
using VSL.Crypt;

namespace VSLTest.Controls
{
    public partial class ShaControl : UserControl
    {
        public ShaControl()
        {
            InitializeComponent();
        }

        private void TbPlaintext_TextChanged(object sender, EventArgs e)
        {
            byte[] text;
            if (RbEncodingUTF8.Checked)
                text = Encoding.UTF8.GetBytes(TbPlaintext.Text);
            else
            {
                if (TbPlaintext.Text.Length % 2 != 0) return;
                try
                {
                    text = Util.GetBytes(TbPlaintext.Text);
                }
                catch { return; }
            }
            byte[] iteration1 = Hash.SHA256(text);
            byte[] iteration2 = Hash.SHA256(iteration1);
            Tb1Iteration.Text = Util.ToHexString(iteration1);
            Tb2Iteration.Text = Util.ToHexString(iteration2);
        }

        private void RbEncodingUTF8_CheckedChanged(object sender, EventArgs e)
        {
            if (RbEncodingUTF8.Checked)
            {
                TbPlaintext.UseSystemPasswordChar = true;
                TbPlaintext_TextChanged(this, new EventArgs());
            }
        }

        private void RbEncodingHex_CheckedChanged(object sender, EventArgs e)
        {
            if (RbEncodingHex.Checked)
            {
                TbPlaintext.UseSystemPasswordChar = false;
                TbPlaintext_TextChanged(this, new EventArgs());
            }
        }
    }
}
