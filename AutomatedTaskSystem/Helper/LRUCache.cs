using System.Collections.Concurrent;

namespace AutomatedTaskSystem.Helper
{
    public class LRUCache<T,R>
    {
        private readonly int capacity;
        private readonly Dictionary<T, LinkedListNode<CacheItem>> _cacheMap;
        private readonly LinkedList<CacheItem> _ltuList;

        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            _cacheMap = new(capacity);
            _ltuList = new();
        }

        public R Get(T key) {

            if (_cacheMap.TryGetValue(key, out var item))
                return default;

            _ltuList.Remove(item);
            _ltuList.AddFirst(item);

            return item.Value.Value;
        }

        public void Put(T key, R value)
        {
            if (_cacheMap.TryGetValue(key, out var item))
            {
                item.Value.Value = value;
                _ltuList.Remove(item);
                _ltuList.AddFirst(item);
            }
            else
            {
                if (_cacheMap.Count >= capacity)
                {
                    RemoveLeastRecentlyUsed();
                }

                var newItem = new CacheItem(key, value);
                var node = new LinkedListNode<CacheItem>(newItem);
                _ltuList.AddFirst(node);
                _cacheMap[key] = node;
            }
        }


        private void RemoveLeastRecentlyUsed()
        {
            var lastNode = _ltuList.Last;
            if (lastNode != null)
            {
                _cacheMap.Remove(lastNode.Value.Key);
                _ltuList.RemoveLast();
            }
        }

        private class CacheItem
        {
            public T Key { get; }
            public R Value { get; set; }
            public CacheItem(T key, R value) { Key = key; Value = value; }
        }
    }
}
