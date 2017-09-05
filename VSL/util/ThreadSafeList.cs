using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    public class ThreadSafeList<T> where T : class
    {
        private List<T> currentList;
        private bool cleaning = false;
        private object cleanupLock;
        private object changeStateLock;
        public ThreadSafeList()
        {
            currentList = new List<T>();
            cleanupLock = new object();
            changeStateLock = new object();
        }
        public void Add(T item)
        {
            currentList.Add(item);
        }
        public void Remove(T item)
        {
            for (int i = 0; i < currentList.Count; i++)
            {
                if (ReferenceEquals(currentList[i], item))
                    currentList[i] = null;
            }
        }
        public void Cleanup()
        {
            lock (cleanupLock)
            {
                List<T> newList = new List<T>();
                lock (changeStateLock)
                    cleaning = true;

            }
        }
        /// <summary>
        /// Invokes an <see cref="Action{T}"/> for each member in the list.
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            List<T> currentList = this.currentList;
            Action<int> work = RunForLoop(currentList, action);
            for (int i = 0; i < currentList.Count; i++)
            {
                work.Invoke(i);
            }
        }
        /// <summary>
        /// Invokes an <see cref="Action{T}"/> for each member in the list. Iterations may run in parallel on the ThreadPool.
        /// </summary>
        /// <param name="action"></param>
        public void ParallelForEach(Action<T> action)
        {
            List<T> currentList = this.currentList;
            Parallel.For(0, currentList.Count, RunForLoop(currentList, action));
        }
        private Action<int> RunForLoop(List<T> currentList, Action<T> action)
        {
            return delegate (int actionIndex)
            {
                T item = currentList[actionIndex];
                if (item != null)
                    action.Invoke(item);
            };
        }
    }
}