using System;
using System.Collections.Generic;

namespace Cube.Utility
{
    public static class LinkedListExtensions
    {
        public static T Remove<T>(this LinkedList<T> list, Predicate<T> predicate)
        {
            if (list == null || list.Count < 1)
            {
                return default;
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            T removed = default;
            var node = list.First;
            while (node != null)
            {
                var next = node.Next;
                if (predicate(node.Value))
                {
                    removed = node.Value;
                    list.Remove(node);
                }
                node = next;
                if (removed != null)
                {
                    break;
                }
            }

            return removed;
        }

        public static int RemoveAll<T>(this LinkedList<T> list, Predicate<T> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var count = 0;
            var node = list.First;
            while (node != null)
            {
                var next = node.Next;
                if (predicate(node.Value))
                {
                    list.Remove(node);
                    count++;
                }
                node = next;
            }
            return count;
        }


    }
}
