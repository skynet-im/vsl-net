using System;
using System.Collections.Concurrent;
using VSL.BinaryTools;

namespace VSL
{
    /// <summary>
    /// An efficient byte queue to store them asynchronously
    /// </summary>
    public class Queue
    {
        // © 2017 Daniel Lerch
        // <fields
        private ConcurrentQueue<byte[]> queue;
        private byte[] cache;
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
        public int Length { get; private set; }
        //  properties>

        // <functions
        /// <summary>
        /// Adds a byte array to the queue
        /// </summary>
        /// <param name="value">byte array to add</param>
        public void Enqeue(byte[] value)
        {
            queue.Enqueue(value);
            RefreshProperties();
        }
        /// <summary>
        /// Dequeue a byte array from the queue
        /// </summary>
        /// <param name="target">byte array to override</param>
        /// <param name="count">count of bytes to read</param>
        /// <returns></returns>
        public bool Dequeue(out byte[] target, int count)
        {
            byte[] buffer = new byte[count];
            int done = 0;
            if (cache?.Length > 0)
            {
                if (count < cache.Length)
                {
                    Array.Copy(cache, buffer, count);
                    done = count;
                    cache = cache.Skip(count);
                }
                else
                {
                    Array.Copy(cache, buffer, cache.Length);
                    done = cache.Length;
                    cache = null;
                }
            }
            while (done < count)
            {
                if (queue.TryDequeue(out byte[] buf))
                {
                    int missing = count - done;
                    if (missing < buf.Length)
                    {
                        Array.Copy(buf, 0, buffer, done, missing);
                        cache = buf.Skip(missing);
                        done += missing;
                        break;
                    }
                    else
                    {
                        Array.Copy(buf, 0, buffer, done, buf.Length);
                        done += buf.Length;
                    }
                }
                else
                {
                    target = buffer;
                    RefreshProperties();
                    return false;
                }
            }
            target = buffer;
            RefreshProperties();
            return true;
        }
        /// <summary>
        /// Refreshes the count of bytes for the property "Length"
        /// </summary>
        private void RefreshProperties()
        {
            int i = 0;
            if (cache != null) i = cache.Length;
            foreach (byte[] b in queue) { i += b.Length; }
            Length = i;
        }
        //  functions>
    }
}