using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL.BinaryTools;
using VSL.Crypt;

namespace VSLTest.Controls
{
    public partial class RsaControl : UserControl
    {
        public RsaControl()
        {
            InitializeComponent();
        }

        private async void btnRsaGenerate_Click(object sender, EventArgs e)
        {
            tbRsaPrivateKey.Text = await Task.Run(() => RsaStatic.GenerateKeyPairXml());
        }

        private async void btnRsaExtract_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbRsaPrivateKey.Text))
                tbRsaPrivateKey.Text = await Task.Run(() => RsaStatic.GenerateKeyPairXml());
            tbRsaPublicKey.Text = RsaStatic.ExtractPublicKey(tbRsaPrivateKey.Text);
        }

        private async void btnRsaEncrypt_Click(object sender, EventArgs e)
        {
            byte[] pt = Encoding.UTF8.GetBytes(tbRsaPlaintext.Text);
            byte[] ct = await Task.Run(() => RsaStatic.Encrypt(pt, tbRsaPublicKey.Text));
            tbRsaCiphertext.Text = Util.ToHexString(ct);
            tbRsaPlaintext.Text = "";
        }

        private async void btnRsaDecrypt_Click(object sender, EventArgs e)
        {
            byte[] ct = Util.GetBytes(tbRsaCiphertext.Text);
            byte[] pt = await Task.Run(() => RsaStatic.Decrypt(ct, tbRsaPrivateKey.Text));
            tbRsaPlaintext.Text = Encoding.UTF8.GetString(pt);
            tbRsaCiphertext.Text = "";
        }
    }
}
