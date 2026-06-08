using System;
using System.Collections;
using System.Collections.Generic;

namespace Vena
{
    public sealed class LinkedStack<T> : IEnumerable<T>
    {
        private readonly LinkedList<T> _linkedList;
        
        public int Count => _linkedList?.Count ?? 0;

        public LinkedStack(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            _linkedList = new LinkedList<T>(collection);
        }

        public LinkedStack()
        {
            _linkedList = new LinkedList<T>();
        }

        public void Clear()
        {
            _linkedList.Clear();
        }

        public void Push(T value)
        {
            _linkedList.AddFirst(value);
        }

        public T Pop()
        {
            var last = _linkedList.First.Value;
            _linkedList.RemoveFirst();
            return last;
        }

        public T Peek()
        {
            var last = _linkedList.First.Value;
            return last;
        }

        public void Insert(LinkedListNode<T> node, T value)
        {
            if (node == null)
            {
                _linkedList.AddLast(value);
            }
            else
            {
                _linkedList.AddBefore(node, value);
            }
        }

        public LinkedListNode<T> First => _linkedList?.First;

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _linkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _linkedList.GetEnumerator();
        }

        #endregion
    }
}