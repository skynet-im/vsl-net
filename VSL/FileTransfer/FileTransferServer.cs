using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// The server implementation of VSL file transfer.
    /// </summary>
    public class FileTransferServer : FileTransferSocket
    {
        // <fields
        new internal VSLServer parent;
        //  fields>
        // <constructor
        internal FileTransferServer(VSLServer parent)
        {
            this.parent = parent;
            base.parent = parent;
        }
        //  constructor>
    }
}