using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    public class ThreadSafeList<T>
    {
        private List<T> currentList;
        public ThreadSafeList()
        {
            currentList = new List<T>();
        }
        public void Add(T item)
        {

        }
        public void Remove(T item)
        {

        }
        public void RemoveAt(int index)
        {

        }
        public void Cleanup()
        {

        }
        public void ForEach(Action<T> action)
        {
            List<T> currentList = this.currentList;
            Action<int> work = RunForLoop(currentList, action);
            for (int i = 0; i < currentList.Count; i++)
            {
                work.Invoke(i);
            }
        }
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