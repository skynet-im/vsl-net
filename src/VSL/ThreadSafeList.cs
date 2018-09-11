using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Represents a strongly typed, threadsafe list of refernce class objects that can't be accessed by index.
    /// </summary>
    /// <typeparam name="T">Generic type has to be a nullable value.</typeparam>
    public sealed class ThreadSafeList<T> where T : class
    {
        private List<T> currentList;
        private List<T> addToList;
        private List<T> removeFromList;
        private bool cleaning = false;
        private int count;
        /// <summary>
        /// Locks <see cref="void"/> <see cref="Cleanup"/>.
        /// </summary>
        private readonly object cleanupLock;
        /// <summary>
        /// Locks <see cref="bool"/> <see cref="cleaning"/> and is locked while finishing cleanup.
        /// </summary>
        private readonly object changeStateLock;
        /// <summary>
        /// Locks <see cref="bool"/> <see cref="count"/>.
        /// </summary>
        private readonly object counterLock;
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeList{T}"/> class.
        /// </summary>
        public ThreadSafeList()
        {
            currentList = new List<T>();
            addToList = new List<T>();
            removeFromList = new List<T>();
            cleanupLock = new object();
            changeStateLock = new object();
            counterLock = new object();
        }
        /// <summary>
        /// Gets the number of elements contained in the <see cref="ThreadSafeList{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                lock (counterLock)
                    return count;
            }
        }
        /// <summary>
        /// Adds an object to the <see cref="ThreadSafeList{T}"/>. 
        /// </summary>
        /// <param name="item">The item to be added to the <see cref="ThreadSafeList{T}"/>. The value can't be null.</param>
        /// <exception cref="ArgumentNullException"/>
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            lock (changeStateLock)
            {
                if (cleaning)
                    addToList.Add(item);
                currentList.Add(item);
            }
            lock (counterLock)
                count++;
        }
        /// <summary>
        /// Removes the first occurency of a specific object from the <see cref="ThreadSafeList{T}"/>
        /// </summary>
        /// <param name="item">The object to be removed from the <see cref="ThreadSafeList{T}"/>. The value can't be null.</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            bool final = false;
            // -TODO: Do not run loop in lock
            lock (changeStateLock)
            {
                if (cleaning)
                    removeFromList.Add(item);
                for (int i = 0; i < currentList.Count; i++)
                {
                    if (ReferenceEquals(currentList[i], item))
                    {
                        currentList[i] = null;
                        final = true;
                        break;
                    }
                }
            }
            if (final)
                lock (counterLock)
                    count--;
            return final;
        }
        /// <summary>
        /// Copies all still existing elements in a new internal list.
        /// </summary>
        public void Cleanup()
        {
            lock (cleanupLock)
            {
                List<T> newList = new List<T>();
                lock (changeStateLock)
                    cleaning = true;
                ForEach((type) => newList.Add(type));
                lock (changeStateLock)
                {
                    newList.AddRange(addToList);
                    foreach (T item in removeFromList)
                        newList.Remove(item);
                    currentList = newList;
                    cleaning = false;
                }
                addToList.Clear();
                removeFromList.Clear();
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