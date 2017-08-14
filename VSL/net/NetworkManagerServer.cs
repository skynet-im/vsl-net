﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class NetworkManagerServer : NetworkManager
    {
        new internal VSLServer parent;

        internal NetworkManagerServer(VSLServer parent, string keypair)
        {
            this.parent = parent;
            base.parent = parent;
            Keypair = keypair;
            InitializeComponent();
        }

        internal override string Keypair { get; }

        internal override string PublicKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}