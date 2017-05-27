using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.packet
{
    internal enum StreamMode : byte
    {
        GetHeader,
        GetFile,
        UploadFile
    }
}