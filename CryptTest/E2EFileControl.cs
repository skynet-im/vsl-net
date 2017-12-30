using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL.Crypt;

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

        private void startBtn_Click(object sender, EventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((o) =>
            {
                using (SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider())
                {
                    byte[] hash = csp.ComputeHash(new FileStream(sourceTb.Text, FileMode.Open));
                    Invoke((Action)(() => sourceShaLb.Text = Util.ToHexString(hash)));
                }
            });
        }
    }
}
