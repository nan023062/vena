// ***********************************************************************************
// * Author : LiNan
// * File : NoAllocLinkList.cs
// * Date : 2023-02-16-11:16
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vena
{
    public class NonAllocLinkList
    {
        public const int InValid = -1;
    }

    public sealed class NonAllocLinkList<T> : NonAllocLinkList
    {
        static readonly IEqualityComparer<T> Comparer = EqualityComparer<T>.Default;

        struct Node
        {
            public T value;
            public int prev;
            public int next;
        }

        private Node[] _nodes;
        private int _head;
        private int _tail;
        private int _freeList;
        private int _count;

        public int First => _head;

        public int Tail => _tail;

        public int Count => _count;

        public NonAllocLinkList(int capacity = 4)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("capacity cannot be negative");
            }

            _nodes = new Node[capacity];
            _head = InValid;
            _tail = InValid;
            _freeList = capacity > 0 ? 0 : InValid;
            for (int i = 0; i < capacity - 1; ++i)
            {
                _nodes[i].next = i + 1;
            }

            if (capacity > 0)
            {
                _nodes[capacity - 1].next = InValid;
            }
        }

        public int AddLast(in T value)
        {
            EnlargeCapacityIfFull();
            if (_tail != InValid)
            {
                var newFreeList = _nodes[_freeList].next;
                _nodes[_tail].next = _freeList;
                _nodes[_freeList] = new Node
                {
                    prev = _tail,
                    next = InValid,
                    value = value
                };
                _tail = _freeList;
                _freeList = newFreeList;
            }
            else
            {
                var newFreeList = _nodes[_freeList].next;
                _head = _tail = _freeList;
                _nodes[_tail] = new Node
                {
                    prev = InValid,
                    next = InValid,
                    value = value
                };
                _freeList = newFreeList;
            }

            _count++;
            return _tail;
        }

        public int AddFirst(in T value)
        {
            EnlargeCapacityIfFull();
            if (_head != InValid)
            {
                var newFreeList = _nodes[_freeList].next;
                _nodes[_head].prev = _freeList;
                _nodes[_freeList] = new Node
                {
                    prev = InValid,
                    next = _head,
                    value = value
                };
                _head = _freeList;
                _freeList = newFreeList;
            }
            else
            {
                var newFreeList = _nodes[_freeList].next;
                _head = _tail = _freeList;
                _nodes[_head] = new Node
                {
                    prev = InValid,
                    next = InValid,
                    value = value
                };
                _freeList = newFreeList;
            }

            _count++;
            return _head;
        }

        public int AddAfter(int handle, T value)
        {
            if (handle == Tail)
            {
                return AddLast(value);
            }

            EnlargeCapacityIfFull();
            var next_handler = GetNext(handle);

            var newFreeList = _nodes[_freeList].next;
            var curr_index = _freeList;
            _freeList = newFreeList;
            _nodes[handle].next = curr_index;
            _nodes[next_handler].prev = curr_index;
            _nodes[curr_index] = new Node
            {
                prev = handle,
                next = next_handler,
                value = value
            };

            _count++;
            return curr_index;
        }

        private void EnlargeCapacityIfFull()
        {
            if (_freeList == -1)
            {
                int oldCapacity = _nodes.Length;
                int newCapacity = _nodes.Length == 0 ? 4 : _nodes.Length * 2;
                Array.Resize(ref _nodes, newCapacity);
                for (int i = oldCapacity; i < newCapacity - 1; ++i)
                {
                    _nodes[i].next = i + 1;
                }

                _nodes[newCapacity - 1].next = -1;
                _freeList = oldCapacity;
            }
        }

        public bool RemoveAndGet(int handle, out T ret)
        {
            if (handle < 0)
            {
                ret = default;
                return false;
            }

            ref var node = ref _nodes[handle];
            ret = node.value;
            int prev = node.prev;
            int next = node.next;
            if (_head == handle)
            {
                _head = next;
            }

            if (_tail == handle)
            {
                _tail = prev;
            }

            if (prev != InValid)
            {
                _nodes[prev].next = next;
            }

            if (next != InValid)
            {
                _nodes[next].prev = prev;
            }

            node.prev = InValid;
            node.next = _freeList;
            node.value = default(T);
            _freeList = handle;
            _count--;
            return true;
        }

        public bool Remove(int handle)
        {
            return RemoveAndGet(handle, out _);
        }

        public ref T GetValue(int handle)
        {
            if (handle < 0)
            {
                throw new ArgumentException("Invalid Handle");
            }

            if (handle >= _nodes.Length)
            {
                throw new ArgumentException("Invalid Handle");
            }

            return ref _nodes[handle].value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueUncheck(int handle)
        {
            return ref _nodes[handle].value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueAndNextUncheck(int handle, out int next)
        {
            ref var node = ref _nodes[handle];
            next = node.next;
            return ref node.value;
        }

        public bool Replace(int handle, T value)
        {
            if (handle < 0)
            {
                return false;
            }

            _nodes[handle].value = value;
            return true;
        }

        public bool TryGetValue(int handle, out T retVal)
        {
            if (handle < 0)
            {
                retVal = default(T);
                return false;
            }

            retVal = _nodes[handle].value;
            return true;
        }

        public bool CheckValid(int handle)
        {
            if (handle < 0)
            {
                return false;
            }

            if (handle >= _nodes.Length)
            {
                return false;
            }

            return true;
        }

        public int GetNext(int handle)
        {
            if (handle < 0)
            {
                return InValid;
            }

            var next = _nodes[handle].next;
            return next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNextUncheck(int handle)
        {
            var next = _nodes[handle].next;
            return next;
        }

        public int GetPrev(int handle)
        {
            if (handle < 0)
            {
                return InValid;
            }

            var prev = _nodes[handle].prev;
            return prev;
        }

        public int Find(T elem)
        {
            for (int i = _head; i != InValid; i = _nodes[i].next)
            {
                if (Comparer.Equals(_nodes[i].value, elem))
                {
                    return i;
                }
            }

            return InValid;
        }

        public bool Contains(T elem)
        {
            for (int i = _head; i != InValid; i = _nodes[i].next)
            {
                if (Comparer.Equals(_nodes[i].value, elem))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _count = 0;

            for (int i = _head; i != InValid;)
            {
                ref var node = ref _nodes[i];

                var next = node.next;

                node.prev = InValid;

                node.next = _freeList;

                node.value = default(T);

                _freeList = i;

                i = next;
            }

            _head = InValid;
            _tail = InValid;
        }
    }
}