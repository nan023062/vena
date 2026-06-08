using System;
using System.Runtime.CompilerServices;

namespace Vena
{
    #region PoolObject WeakReference

    /// <summary>
    /// a weak reference PoolObject with no parameter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Ref<T> : IEquatable<Ref<T>> where T : class, IPoolable, new()
    {
        /// <summary>
        /// The token of the object.
        /// </summary>
        public readonly int token;

        private readonly T _impl;

        public Ref(T impl)
        {
            _impl = impl;
            token = 0;
            if (null != _impl)
            {
                token = PoolManager.GetToken(impl);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(Ref<T> wref)
        {
            if (wref.token > 0 && wref._impl != null)
            {
                int token = PoolManager.GetToken(wref._impl);
                return wref.token == token;
            }

            return false;
        }

        public bool Equals(Ref<T> other)
        {
            return token == other.token && Equals(_impl, other._impl);
        }

        public override bool Equals(object obj)
        {
            return obj is Ref<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return token + (_impl != null ? _impl.GetHashCode() : 0);
        }

        /// <summary>
        /// Try to get the reference object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(out T value)
        {
            if (this)
            {
                value = _impl;
                return true;
            }

            value = default;
            return false;
        }
    }

    /// <summary>
    /// a weak reference PoolObject with 1 parameter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TArg"></typeparam>
    public readonly struct Ref<T, TArg> : IEquatable<Ref<T, TArg>>
        where T : class, IPoolable<TArg>, new()
    {
        /// <summary>
        /// The token of the object.
        /// </summary>
        public readonly int token;

        private readonly T _impl;

        public Ref(in T impl)
        {
            _impl = impl;
            token = 0;
            if (null != _impl)
            {
                token = PoolManager.GetToken<T, TArg>(impl);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(Ref<T, TArg> wref)
        {
            if (!PoolManager.UsePool)
            {
                return wref._impl != null;
            }

            if (wref.token > 0 && wref._impl != null)
            {
                int token = PoolManager.GetToken<T, TArg>(wref._impl);
                return wref.token == token;
            }

            return false;
        }

        public bool Equals(Ref<T, TArg> other)
        {
            return token == other.token && Equals(_impl, other._impl);
        }

        public override bool Equals(object obj)
        {
            return obj is Ref<T, TArg> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return token + (_impl != null ? _impl.GetHashCode() : 0);
        }

        /// <summary>
        /// Try to get the reference object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(out T value)
        {
            if (this)
            {
                value = _impl;
                return true;
            }

            value = default;
            return false;
        }
    }

    #endregion
}