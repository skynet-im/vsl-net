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

        internal NetworkManagerClient(VSLClient parent, string publicKey)
        {
            this.parent = parent;
            base.parent = parent;
            PublicKey = publicKey;
            InitializeComponent();
        }

        internal override string Keypair
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal override string PublicKey { get; }
    }
}