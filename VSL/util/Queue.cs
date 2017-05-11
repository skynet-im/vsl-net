using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace VSL
{
    /// <summary>
    /// An efficient byte queue to store them asynchronously
    /// </summary>
    public class Queue
    {
        // v4 © 2017 Daniel Lerch
        // <fields
        private ConcurrentQueue<byte[]> queue;
        private byte[] cache;
        private int _length;
        //  fields>

        // <constructor
        /// <summary>
        /// Initializes a new instance of the Queue class
        /// </summary>
        public Queue()
        {
            queue = new ConcurrentQueue<byte[]>();
            cache = new byte[0] { };
        }
        //  constructor>

        // <properties
        /// <summary>
        /// The count of bytes in the queue
        /// </summary>
        public int Length
        {
            get { return _length; }
        }
        //  properties>

        // <functions
        /// <summary>
        /// Adds a byte array to the queue
        /// </summary>
        /// <param name="value">byte array to add</param>
        public void Enqeue(byte[] value)
        {
            queue.Enqueue(value);
            refreshProperties();
        }
        /// <summary>
        /// Dequeue a byte array from the queue
        /// </summary>
        /// <param name="target">byte array to override</param>
        /// <param name="count">count of bytes to read</param>
        /// <returns></returns>
        public bool Dequeue(out byte[] target, int count)
        {
            byte[] buffer = new byte[0] { };
            if (cache?.Length > 0)
            {
                if (count < cache.Length)
                {
                    buffer = cache.Take(count).ToArray();
                    cache = cache.Skip(count).ToArray();
                }
                else
                {
                    buffer = cache;
                    cache = null;
                }
            }
            while (buffer.Length < count)
            {
                byte[] buf;
                if (queue.TryDequeue(out buf))
                {
                    if (count < buf.Length)
                    {
                        int missing = count - buffer.Length;
                        buffer = buffer.Concat(buf.Take(missing)).ToArray();
                        cache = buf.Skip(missing).ToArray();
                    }
                    else
                    {
                        buffer = buffer.Concat(buf).ToArray();
                    }
                }
                else
                {
                    target = buffer;
                    refreshProperties();
                    return false;
                }
            }
            target = buffer;
            refreshProperties();
            return true;
        }
        /// <summary>
        /// Refreshes the count of bytes for the property "Length"
        /// </summary>
        private void refreshProperties()
        {
            int i = 0;
            if (cache != null) i = cache.Length;
            foreach (byte[] b in queue) { i += b.Length; }
            _length = i;
        }
        //  functions>
    }
}