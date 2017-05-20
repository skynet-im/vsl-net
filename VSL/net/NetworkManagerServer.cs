using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class NetworkManagerServer : NetworkManager
    {
        new internal VSLServer parent;

        internal NetworkManagerServer(VSLServer parent)
        {
            this.parent = parent;
            base.parent = parent;
            InitializeComponent();
        }

        internal override byte[] AesKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal override string Keypair
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal override string PublicKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal override byte[] ReceiveIV
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal override byte[] SendIV
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}