using System;
using System.Collections.Generic;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class UniqueQueue<T>
    {
        private Queue<T> queue;
        private HashSet<T> queueItems;

        public int Count => queue.Count;

        public UniqueQueue()
        {
            queue = new Queue<T>();
            queueItems = new HashSet<T>();
        }

        public bool Enqueue(T item)
        {
            if (queueItems.Add(item))
            {
                queue.Enqueue(item);
                return true;
            }

            return false;
        }

        public T Dequeue()
        {
            if(queue.Count > 0)
            {
                var item = queue.Dequeue();
                queueItems.Remove(item);
                return item;
            }

            throw new InvalidOperationException("No items left to dequeue");
        }
    }
}
