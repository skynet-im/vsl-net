using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// The client implementation of VSL file transfer.
    /// </summary>
    public class FileTransferClient : FileTransferSocket
    {
        // <fields
        new internal VSLClient parent;
        //  fields>
        // <constructor
        internal FileTransferClient(VSLClient parent)
        {
            this.parent = parent;
            base.parent = parent;
        }
        //  constructor>
    }
}