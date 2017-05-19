using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class NetworkManagerClient : NetworkManager
    {
        new internal VSLClient parent;

        internal NetworkManagerClient(VSLClient parent)
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

        internal override byte[] ReceiveIV
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}