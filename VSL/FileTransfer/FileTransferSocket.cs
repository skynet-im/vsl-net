using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// The base class for file transfer implementations.
    /// </summary>
    public abstract class FileTransferSocket
    {
        // <fields
        internal VSLSocket parent;
        //  fields>
        // <constructor
        internal void InitializeComponent()
        {

        }
        //  constructor>
        // <function
        internal Packet.P08FileHeader GetHeaderPacket(string path)
        {
            FileInfo fi = new FileInfo(path);
            return new Packet.P08FileHeader(fi.FullName, Convert.ToUInt64(fi.Length), (uint)fi.Attributes, fi.CreationTime, fi.LastAccessTime, fi.LastWriteTime, new byte[0], new byte[0]);
        }
        //  function>
    }
}