using System;
using System.Collections.Concurrent;

namespace VSL
{
    internal sealed class MemoryCache<T>
    {
        private readonly Func<T> constructor;
        private readonly Func<T, bool> validator;
        private readonly Action<T> destructor;
        private readonly ConcurrentStack<T> itemStack;

        public MemoryCache(int capacity, Func<T> constructor)
            : this(capacity, constructor, x => (x as IDisposable)?.Dispose()) { }

        public MemoryCache(int capacity, Func<T> constructor, Action<T> destructor)
            : this(capacity, constructor, x => true, destructor) { }

        public MemoryCache(int capacity, Func<T> constructor, Func<T, bool> validator, Action<T> destructor)
        {
            Capacity = capacity;
            this.constructor = constructor;
            this.validator = validator;
            this.destructor = destructor;
            itemStack = new ConcurrentStack<T>();
        }

        public int Capacity { get; set; }

        public int Count => itemStack.Count;

        public bool TryPop(out T item)
        {
            return itemStack.TryPop(out item);
        }

        public T PopOrCreate()
        {
            if (itemStack.TryPop(out T item))
                return item;
            else
                return constructor();
        }

        public void Push(T item)
        {
            if (itemStack.Count < Capacity && validator(item))
                itemStack.Push(item);
            else
                destructor(item);
        }
    }
}
