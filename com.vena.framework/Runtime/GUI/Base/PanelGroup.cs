// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Vena.Framework
{
    public partial class GameWorld
    {
        sealed class PanelGroup<T>
        {
            public readonly PanelStyle Style;

            private readonly List<T> _list;

            public PanelGroup(PanelStyle style)
            {
                Style = style;
                _list = new List<T>();
            }
        
            public T Top()
            {
                T id = Count > 0 ? _list[Count - 1] : default(T);
                return id;
            }

            public T Pop()
            {
                T id = Count > 0 ? _list[Count - 1] : default(T);
                if (Count > 0) _list.RemoveAt(Count - 1);
                return id;
            }

            public void Push(T id)
            {
                _list.Add(id);
            }

            public void Enqueue(T id)
            {
                _list.Add(id);
            }

            public T Dequeue()
            {
                T id = Count > 0 ? _list[0] : default(T);
                if (Count > 0) _list.RemoveAt(0);
                return id;
            }

            public T Peek()
            {
                T id = Count > 0 ? _list[0] : default(T);
                return id;
            }

            public int Count => _list.Count;

            public void Remove(T id)
            {
                _list.Remove(id);
            }
        }
    }
  

}
