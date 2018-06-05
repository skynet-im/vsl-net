using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Net.Channel
{
    internal struct ReceiveSendItem
    {
        internal ReceiveSendItem(byte[] buffer, int offset, int count)
        {
            Tcs = new TaskCompletionSource<bool>();
            Task = Tcs.Task;
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        internal TaskCompletionSource<bool> Tcs { get; }
        internal Task<bool> Task { get; }
        internal byte[] Buffer { get; }
        internal int Offset { get; set; }
        internal int Count { get; set; }
    }
}
