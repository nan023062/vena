//****************************************************************************
// File: SafeMap.cs
// Author: Li Nan
// Date: 2023-01-06-12:05
// Version: 1.0
//****************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;

namespace Vena
{
    public class SafeMap<TKey, TValue>: IEnumerable<KeyValuePair<TKey, TValue>> where TValue : class
    {
        private readonly HashSet<TKey> _keyCheck;
        
        private readonly Dictionary<TKey, TValue> _dictionary;

        private readonly LinkedList<TKey> _keysLinkedList;
        
        private bool _dirtyList;
        
        private byte _iteratorType;
        
        public int Count => _dictionary.Count;

        public void Add(TKey key, TValue value)
        {
            if (!_dictionary.ContainsKey(key))
            {
                _dictionary.Add(key, value);

                _keysLinkedList.AddLast(key);
                
                _dirtyList = true;
            }
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                _dirtyList = true;
                return true;
            }

            return false;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            _dirtyList = false;
            
            _iteratorType = 0;
            
            _keyCheck.Clear();
            
            _dictionary.Clear();
            
            _keysLinkedList.Clear();
        }

        public SafeMap()
        {
            _keyCheck = new HashSet<TKey>();
            
            _dictionary = new Dictionary<TKey, TValue>();

            _keysLinkedList = new LinkedList<TKey>();

            _dirtyList = false;

            _iteratorType = 0;
        }
        
        public SafeMap<TKey, TValue>.Enumerator GetEnumerator()
        {
            return new SafeMap<TKey, TValue>.Enumerator(this);
        }
        
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<TKey, TValue>>)new SafeMap<TKey, TValue>.Enumerator(this);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)new SafeMap<TKey, TValue>.Enumerator(this);
        }
        
        [Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
        {
          private SafeMap<TKey, TValue> _list;
          
          private LinkedListNode<TKey> _iterator;
          
          private KeyValuePair<TKey, TValue> _current;
            
          internal Enumerator(SafeMap<TKey, TValue> list)
          {
              if (list._iteratorType > 0)
              {
                  throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
              }
              
              _list = list;
              
              _iterator = list._keysLinkedList.First;
              
              _current = new KeyValuePair<TKey, TValue>();
              
              _list._iteratorType = 1;
              
              if (list._dirtyList)
              {
                  list._keyCheck.Clear();
                  
                  _list._iteratorType = 2;
                  
                  list._dirtyList = false;
              }
          }

          public bool MoveNext()
          {
              TValue value = null;
              
              TKey key = default;
              
              // search next valid node
              while (null != _iterator)
              {
                  key = _iterator.Value;
                  
                  value = null;
                  
                  if (_list._iteratorType == 2)
                  {
                      if (_list._keyCheck.Contains(key))
                      {
                          LinkedListNode<TKey> next = _iterator.Next;
                          
                          _list._keysLinkedList.Remove(_iterator);
                          
                          _iterator = next;
                          
                          continue;
                      }
                      
                      _list._keyCheck.Add(key);
                  }

                  if (_list._dictionary.TryGetValue(key, out value) && null != value)
                  {
                      _iterator = _iterator.Next;
                      
                      break;
                  }
                  
                  LinkedListNode<TKey> next1 = _iterator.Next;
                  
                  _list._keysLinkedList.Remove(_iterator);
                  
                  _iterator = next1;
              }

              if (null != value)
              {
                  _current = new KeyValuePair<TKey, TValue>(key, value);
                  
                  return true;
              }
              
              _current = new KeyValuePair<TKey, TValue>();
              
              return false;
          }

          public KeyValuePair<TKey, TValue> Current => _current;
            
          public void Dispose()
          {
              _list._iteratorType = 0;
              
              _list = null;
              
              _iterator = null;
              
              _current = new KeyValuePair<TKey, TValue>();
          }

          object IEnumerator.Current
          {
              get
              {
                  if (_iterator == null)
                      throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                  
                  return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
              }
          }

          void IEnumerator.Reset()
          {
              _list._iteratorType = 0;
              
              _iterator = null;
              
              _current = new KeyValuePair<TKey, TValue>();
          }
        }
    }
}