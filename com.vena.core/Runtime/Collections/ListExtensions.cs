// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Vena
{
    public static class ListExtensions
    {
        public static bool RemoveSwapBack<T>(this List<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index < 0)
                return false;

            RemoveAtSwapBack(list, index);
            return true;
        }
        
        public static bool RemoveSwapBack<T>(this List<T> list, Predicate<T> matcher)
        {
            int index = list.FindIndex(matcher);
            if (index < 0)
                return false;

            RemoveAtSwapBack(list, index);
            return true;
        }
        
        public static void RemoveAtSwapBack<T>(this List<T> list, int index)
        {
            int lastIndex = list.Count - 1;
            list[index] = list[lastIndex];
            list.RemoveAt(lastIndex);
        }

#if NET471
        public static bool TryPeek<T>(this Stack<T> stack, out T result)
        {
            if (stack.Count == 0)
            {
                result = default;
                return false;
            }

            result = stack.Peek();
            return true;
        }
#endif
    }
}