using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Vena
{
    /// <summary>
    /// object in the world, such as actor, item, etc.
    /// for define UniqueId
    /// </summary>
    public abstract class ObjectWithId
    {
        private static uint _version;

        private uint _uniqueId;

        /// <summary>
        /// UniqueId
        /// </summary>
        /// <exception cref="InvalidOperationException">if the object is destroyed</exception>
        public uint Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _uniqueId > 0 ? _uniqueId : throw new InvalidOperationException("Object has been destroyed");
        }

        /// <summary>
        /// if the object is destroyed , return false
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(ObjectWithId obj) => obj != null && obj._uniqueId > 0;
        
        protected internal ObjectWithId()
        {
            Checker.ThrowIsUnSafe(CallFunc.Create);

            _uniqueId = ++_version;

            // if _version is overflow, reset to 0
            if (_version >= uint.MaxValue)
            {
                _version = 0;
            }
        }

        public override string ToString()
        {
            return $"worldObject:{_uniqueId}";
        }

        /// <summary>
        /// mark as destroyed
        /// </summary>
        /// <exception cref="InvalidOperationException">check caller is safe</exception>
        internal void InternalMarkAsDestroyed()
        {
            if (_uniqueId > 0)
            {
                Checker.ThrowIsUnSafe(CallFunc.Destroy);

                _uniqueId = 0;
            }
        }

        public abstract void CopyTo(ObjectWithId target);

        private static List<FieldInfo> GetAllInstanceFields(Type type)
        {
            var fields = new List<FieldInfo>();

            while (type != null && type != typeof(ObjectWithId))
            {
                FieldInfo[] infos = type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                foreach (var fieldInfo in infos)
                {
                    // 且未定义IgnoreCloneAttribute
                    if (fieldInfo.GetCustomAttribute<SerializeField>() == null)
                    {
                        fields.Add(fieldInfo);
                    }
                }

                type = type.BaseType;
            }

            return fields;
        }


        internal enum CallFunc : byte
        {
            Default,
            Create,
            Destroy,
        }

        private static CallFunc _callFunc;

        internal static readonly object __lock__ = new object();

        internal readonly struct Checker : IDisposable
        {
            public Checker(CallFunc type)
            {
                if (_callFunc != CallFunc.Default)
                {
                    throw new Exception($"Object.Checker is locked by other {_callFunc}");
                }

                _callFunc = type;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ThrowIsUnSafe(CallFunc type)
            {
                if (_callFunc != type)
                {
                    throw new InvalidOperationException("Object can only be created or destroyed by World");
                }
            }

            public void Dispose()
            {
                // TODO release managed resources here
                _callFunc = CallFunc.Default;
            }
        }
    }
}