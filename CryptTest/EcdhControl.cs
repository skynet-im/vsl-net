using System;
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
    public partial class EcdhControl : UserControl
    {
        public EcdhControl()
        {
            InitializeComponent();
        }

        private void btnECDHAliceGenParams_Click(object sender, EventArgs e)
        {
            ECDH.GenerateKey(out byte[] privateKey, out byte[] publicKey);
            TbAlicePrivate.Text = Util.ToHexString(privateKey);
            TbAlicePublic.Text = Util.ToHexString(publicKey);
        }

        private void btnECDHBobGenParams_Click(object sender, EventArgs e)
        {
            ECDH.GenerateKey(out byte[] privateKey, out byte[] publicKey);
            TbBobPrivate.Text = Util.ToHexString(privateKey);
            TbBobPublic.Text = Util.ToHexString(publicKey);
        }

        private void btnECDHCreateKey_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TbAlicePrivate.Text) || string.IsNullOrEmpty(TbAlicePublic.Text))
                btnECDHAliceGenParams_Click(null, null);

            if (string.IsNullOrEmpty(TbBobPrivate.Text) || string.IsNullOrEmpty(TbBobPublic.Text))
                btnECDHBobGenParams_Click(null, null);

            byte[] alicePrivate = Util.GetBytes(TbAlicePrivate.Text);
            byte[] alicePublic = Util.GetBytes(TbAlicePublic.Text);

            byte[] bobPrivate = Util.GetBytes(TbBobPrivate.Text);
            byte[] bobPublic = Util.GetBytes(TbBobPublic.Text);

            TbAliceKey.Text = Util.ToHexString(ECDH.DeriveKey(alicePrivate, bobPublic));
            TbBobKey.Text = Util.ToHexString(ECDH.DeriveKey(bobPrivate, alicePublic));
        }
    }
}
