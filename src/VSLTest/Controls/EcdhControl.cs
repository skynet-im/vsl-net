using System;
using System.Windows.Forms;
using VSL.BinaryTools;
using VSL.Crypt;

namespace VSLTest.Controls
{
    public partial class EcdhControl : UserControl
    {
        public EcdhControl()
        {
            InitializeComponent();
        }

        private void BtnAliceGenParams_Click(object sender, EventArgs e)
        {
            ECDH.GenerateKey(out byte[] privateKey, out byte[] publicKey);
            TbAlicePrivate.Text = Util.ToHexString(privateKey);
            TbAlicePublic.Text = Util.ToHexString(publicKey);
        }

        private void BtnBobGenParams_Click(object sender, EventArgs e)
        {
            ECDH.GenerateKey(out byte[] privateKey, out byte[] publicKey);
            TbBobPrivate.Text = Util.ToHexString(privateKey);
            TbBobPublic.Text = Util.ToHexString(publicKey);
        }

        private void BtnCreateKey_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TbAlicePrivate.Text) || string.IsNullOrEmpty(TbAlicePublic.Text))
                BtnAliceGenParams_Click(null, null);

            if (string.IsNullOrEmpty(TbBobPrivate.Text) || string.IsNullOrEmpty(TbBobPublic.Text))
                BtnBobGenParams_Click(null, null);

            byte[] alicePrivate = Util.GetBytes(TbAlicePrivate.Text);
            byte[] alicePublic = Util.GetBytes(TbAlicePublic.Text);

            byte[] bobPrivate = Util.GetBytes(TbBobPrivate.Text);
            byte[] bobPublic = Util.GetBytes(TbBobPublic.Text);

            TbAliceKey.Text = Util.ToHexString(ECDH.DeriveKey(alicePrivate, bobPublic));
            TbBobKey.Text = Util.ToHexString(ECDH.DeriveKey(bobPrivate, alicePublic));
        }
    }
}
