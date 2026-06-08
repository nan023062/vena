using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable StaticMemberInGenericType
namespace Vena
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct ArchetypeId : IEquatable<ArchetypeId>
    {
        public static readonly ArchetypeId Invalid = new ArchetypeId(-1, -1);

        [FieldOffset(0)] private readonly int _id;

        [FieldOffset(2)] public readonly short bucket;

        [FieldOffset(0)] public readonly short index;

        private ArchetypeId(int id)
        {
            bucket = 0;
            index = 0;
            _id = id;
        }

        public ArchetypeId(short bucket, short index)
        {
            _id = 0;
            this.bucket = bucket;
            this.index = index;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Archetype_{bucket}_{index}:");
            foreach (var typeInfo in Archetype.GetTypes(this))
            {
                sb.Append($" {typeInfo.Type.GetTypeName()}");
            }

            return sb.ToString();
        }

        public bool Equals(ArchetypeId other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is ArchetypeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public static bool operator ==(ArchetypeId left, ArchetypeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ArchetypeId left, ArchetypeId right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(ArchetypeId id)
        {
            return Archetype.Valid(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyCollection<TypeInfo> GetTypes()
        {
            return Archetype.GetTypes(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has<T>()
        {
            return Archetype.Has<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T1, T2>()
        {
            return Archetype.HasAny<T1, T2>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T1, T2, T3>()
        {
            return Archetype.HasAny<T1, T2, T3>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T1, T2, T3, T4>()
        {
            return Archetype.HasAny<T1, T2, T3, T4>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T1, T2, T3, T4, T5>()
        {
            return Archetype.HasAny<T1, T2, T3, T4, T5>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny(in ArchetypeId other)
        {
            return Archetype.MatchAny(this, other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAll<T1, T2>()
        {
            return Archetype.HasAll<T1, T2>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAll<T1, T2, T3>()
        {
            return Archetype.HasAll<T1, T2, T3>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAll<T1, T2, T3, T4>()
        {
            return Archetype.HasAll<T1, T2, T3, T4>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAll<T1, T2, T3, T4, T5>()
        {
            return Archetype.HasAll<T1, T2, T3, T4, T5>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAll(in ArchetypeId other)
        {
            return Archetype.MatchAll(this, other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeId Add<T>()
        {
            return Archetype.Add<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeId Remove<T>()
        {
            return Archetype.Remove<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeId Combine(in ArchetypeId other)
        {
            return Archetype.Combine(this, other);
        }
    }

    public readonly struct TypeInfo : IComparer<TypeInfo>, IComparable<TypeInfo>
    {
        public readonly Type Type;
        public readonly int Id;
        public readonly ulong BloomFlag;
        public readonly int Order;
        public readonly Type[] Dependencies;

        public TypeInfo(Type type, int id, ulong bloomFlag, int order, Type[] dependencies)
        {
            Type = type;
            Id = id;
            BloomFlag = bloomFlag;
            Order = order;
            Dependencies = dependencies;
        }

        public int Compare(TypeInfo x, TypeInfo y)
        {
            return x.Id.CompareTo(y.Id);
        }

        public int CompareTo(TypeInfo other)
        {
            return Id.CompareTo(other.Id);
        }
    }

    /// <summary>
    /// Archetype is a type that contains a set of component types.
    /// </summary>
    public abstract class Archetype
    {
        private static readonly List<TypeInfo> _types;

        private static readonly HashSet<int> _hashSet;

        private const int BucketCount = 61;

        private static readonly Bucket[] _buckets;

        private static readonly Dictionary<Type, TypeWrap> _typeMap;

        static Archetype()
        {
            //using var _ = new TimeWatch(" ------- Archetype.ctor() ");
            _types = new List<TypeInfo>();
            _hashSet = new HashSet<int>();
            _buckets = new Bucket[BucketCount];
            _typeMap = new Dictionary<Type, TypeWrap>();
        }

        public static class type<T>
        {
            public static readonly int Id;

            public static readonly ulong BloomFlag;

            public static readonly TypeInfo Info;

            public static readonly int Order;

            static type()
            {
                if (!_typeMap.TryGetValue(typeof(T), out TypeWrap wrap))
                {
                    wrap = new TypeWrap(typeof(T));

                    _typeMap.Add(typeof(T), wrap);
                }

                Info = wrap.Info;

                Id = Info.Id;

                BloomFlag = Info.BloomFlag;

                Order = Info.Order;
            }
        }

        sealed class TypeWrap
        {
            private static int _typesCount;

            public readonly TypeInfo Info;

            public TypeWrap(Type type)
            {
                //using var _ = new TimeWatch($" ------- Archetype.TypeWrap({type}) ");

                int id = Interlocked.Increment(ref _typesCount);

                ulong bloomFlag = 1ul << (id % 64);

                OrderAttribute attr = type.GetCustomAttribute<OrderAttribute>();

                int order = attr?.Order ?? 0;

                List<Type> dependencies = new List<Type>();
                foreach (var attribute in type.GetCustomAttributes<RequireAttribute>())
                {
                    dependencies.AddRange(attribute.requireTypes);
                }

                Info = new TypeInfo(type, id, bloomFlag, order, dependencies.ToArray());
            }
        }

        public static ref readonly TypeInfo GetTypeInfo(Type type)
        {
            if (!_typeMap.TryGetValue(type, out TypeWrap wrap))
            {
                wrap = new TypeWrap(type);

                _typeMap.Add(type, wrap);
            }

            return ref wrap.Info;
        }

        private static ref readonly ArchetypeInfo Get(in ArchetypeId id)
        {
            if (id.bucket < 0 || id.bucket >= BucketCount)
            {
                return ref ArchetypeInfo.Default;
            }

            Bucket bucket = _buckets[id.bucket];

            if (null == bucket)
            {
                return ref ArchetypeInfo.Default;
            }

            return ref bucket.Get(id.index);
        }

        protected static ArchetypeId Register(ref TypeInfo[] types, ulong bloomFlag)
        {
            //using var _ = new TimeWatch(" ------- Archetype.Register ");

            _types.Clear();

            _types.AddRange(types);

            // sort the types and make template ArchetypeInfo
            _types.Sort();

            // register archetype
            return Register(_types, bloomFlag, out bool matched);
        }

        private static ArchetypeId Register(List<TypeInfo> types, ulong bloomFlag, out bool matched)
        {
            int bucketIndex = (int)(Hash64(bloomFlag) % BucketCount);
            Bucket bucket = _buckets[bucketIndex];
            if (null == bucket)
            {
                bucket = new Bucket();
                _buckets[bucketIndex] = bucket;
            }

            // match has the types archetypes
            ArchetypeId matchId = bucket.Match(types, bloomFlag);
            if (matchId.bucket >= 0)
            {
                matched = true;
                return matchId;
            }

            // add new archetype
            matched = false;
            return bucket.Add(types, bloomFlag, bucketIndex);
        }

        private static ulong Hash64(ulong value)
        {
            const ulong GoldenRatio64 = 0x9e3779b97f4a7c15;
            value ^= (value >> 30);
            value *= GoldenRatio64;
            value ^= (value >> 27);
            value *= GoldenRatio64;
            value ^= (value >> 31);
            return value;
        }

        /// <summary>
        /// multiple types with the same order
        /// </summary>
        readonly struct ArchetypeInfo
        {
            public static readonly ArchetypeInfo Default =
                new ArchetypeInfo(new List<TypeInfo>(), 0, ArchetypeId.Invalid);

            public readonly ArchetypeId Id;
            public readonly ulong BloomFlag;
            public readonly TypeInfo[] Types;
            public readonly HashSet<int> TypeSet;

            public ArchetypeInfo(List<TypeInfo> types, ulong bloomFlag, ArchetypeId id)
            {
                Id = id;
                BloomFlag = bloomFlag;
                Types = types.ToArray();
                TypeSet = new HashSet<int>();
                foreach (var type in types)
                {
                    TypeSet.Add(type.Id);
                }
            }

            public bool Match(List<TypeInfo> types, ulong bloomFlags)
            {
                if (BloomFlag != bloomFlags)
                    return false;

                int length1 = Types.Length;
                int length2 = types.Count;
                if (length1 != length2)
                    return false;

                for (int i = 0; i < length1; i++)
                {
                    ref readonly var type1 = ref Types[i];
                    var type2 = types[i];
                    if (type1.Id != type2.Id)
                        return false;
                }

                return true;
            }
        }

        sealed class Bucket
        {
            private int _count;

            private ArchetypeInfo[] _archetypes;

            public int Count => _count;

            internal Bucket()
            {
                _count = 0;
                _archetypes = new ArchetypeInfo[8];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArchetypeId Add(List<TypeInfo> types, ulong bloomFlag, int bucketIdx)
            {
                if (_count >= _archetypes.Length)
                {
                    ArchetypeInfo[] destinationArray = new ArchetypeInfo[_archetypes.Length * 2];
                    Array.Copy(_archetypes, destinationArray, _archetypes.Length);
                    _archetypes = destinationArray;
                }

                int index = _count++;
                ArchetypeId id = new ArchetypeId((short)bucketIdx, (short)index);
                _archetypes[index] = new ArchetypeInfo(types, bloomFlag, id);
                return id;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArchetypeId Match(List<TypeInfo> types, ulong bloomFlag)
            {
                for (int i = 0; i < _count; i++)
                {
                    ref readonly var archetypeInfo = ref _archetypes[i];
                    if (archetypeInfo.Match(types, bloomFlag))
                    {
                        return archetypeInfo.Id;
                    }
                }

                return ArchetypeId.Invalid;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly ArchetypeInfo Get(int index)
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Invalid Archetype index: {index}");

                return ref _archetypes[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IReadOnlyCollection<TypeInfo> GetTypes(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);

            return typeInfo.Types;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Valid(in ArchetypeId archetype)
        {
            if (archetype.bucket < 0 || archetype.bucket >= BucketCount)
                return false;

            ref readonly var bucket = ref _buckets[archetype.bucket];

            if (null == bucket)
                return false;

            return archetype.index >= 0 && archetype.index < bucket.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Has<T>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) != bloomFlag)
                return false;

            return typeInfo.TypeSet.Contains(type<T>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Has(in ArchetypeId id, in TypeInfo info)
        {
            ref readonly var typeInfo = ref Get(id);

            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = info.BloomFlag;

            if ((typeInfo.BloomFlag & bloomFlag) != bloomFlag)
            {
                return false;
            }

            return typeInfo.TypeSet.Contains(info.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAny<T1, T2>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);

            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag;

            if ((typeInfo.BloomFlag & bloomFlag) == 0)
            {
                return false;
            }

            return typeInfo.TypeSet.Contains(type<T1>.Id) || typeInfo.TypeSet.Contains(type<T2>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAny<T1, T2, T3>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) == 0)
                return false;

            return typeInfo.TypeSet.Contains(type<T1>.Id) || typeInfo.TypeSet.Contains(type<T2>.Id) ||
                   typeInfo.TypeSet.Contains(type<T3>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAny<T1, T2, T3, T4>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) == 0)
                return false;

            return typeInfo.TypeSet.Contains(type<T1>.Id) || typeInfo.TypeSet.Contains(type<T2>.Id) ||
                   typeInfo.TypeSet.Contains(type<T3>.Id) || typeInfo.TypeSet.Contains(type<T4>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAny<T1, T2, T3, T4, T5>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                              type<T5>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) == 0)
                return false;

            return typeInfo.TypeSet.Contains(type<T1>.Id) || typeInfo.TypeSet.Contains(type<T2>.Id) ||
                   typeInfo.TypeSet.Contains(type<T3>.Id) || typeInfo.TypeSet.Contains(type<T4>.Id) ||
                   typeInfo.TypeSet.Contains(type<T5>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAll<T1, T2>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) != bloomFlag)
                return false;

            return typeInfo.TypeSet.Contains(type<T1>.Id) && typeInfo.TypeSet.Contains(type<T2>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAll<T1, T2, T3>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) != bloomFlag)
                return false;

            return typeInfo.TypeSet.Contains(type<T1>.Id) && typeInfo.TypeSet.Contains(type<T2>.Id) &&
                   typeInfo.TypeSet.Contains(type<T3>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAll<T1, T2, T3, T4>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) != bloomFlag)
                return false;

            return typeInfo.TypeSet.Contains(type<T1>.Id) && typeInfo.TypeSet.Contains(type<T2>.Id) &&
                   typeInfo.TypeSet.Contains(type<T3>.Id) && typeInfo.TypeSet.Contains(type<T4>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAll<T1, T2, T3, T4, T5>(in ArchetypeId id)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ulong bloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                              type<T5>.BloomFlag;
            if ((typeInfo.BloomFlag & bloomFlag) != bloomFlag)
                return false;

            return typeInfo.TypeSet.Contains(type<T1>.Id) && typeInfo.TypeSet.Contains(type<T2>.Id) &&
                   typeInfo.TypeSet.Contains(type<T3>.Id) && typeInfo.TypeSet.Contains(type<T4>.Id) &&
                   typeInfo.TypeSet.Contains(type<T5>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool MatchAll(in ArchetypeId id, in ArchetypeId id2)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ref readonly var typeInfo2 = ref Get(id2);
            ulong bloomFlag = typeInfo2.BloomFlag;
            if (bloomFlag == 0) return false;

            if ((typeInfo.BloomFlag & bloomFlag) != bloomFlag)
                return false;

            int length = typeInfo2.Types.Length;
            for (int i = 0; i < length; i++)
            {
                ref readonly var type = ref typeInfo2.Types[i];
                if (!typeInfo.TypeSet.Contains(type.Id))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool MatchAny(in ArchetypeId id, in ArchetypeId id2)
        {
            ref readonly var typeInfo = ref Get(id);
            if (typeInfo.BloomFlag == 0) return false;

            ref readonly var typeInfo2 = ref Get(id2);
            ulong bloomFlag = typeInfo2.BloomFlag;
            if (bloomFlag == 0) return false;

            if ((typeInfo.BloomFlag & bloomFlag) == 0)
                return false;

            int length = typeInfo2.Types.Length;
            for (int i = 0; i < length; i++)
            {
                ref readonly var type = ref typeInfo2.Types[i];
                if (typeInfo.TypeSet.Contains(type.Id))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ArchetypeId Add<T>(in ArchetypeId id)
        {
            // already has the type
            if (Has<T>(id)) return id;

            return Add(id, type<T>.Info);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ArchetypeId Add(in ArchetypeId id, in TypeInfo info)
        {
#if UNITY_EDITOR
            if (Has(id, info))
                throw new InvalidOperationException($"Archetype already has the type: {info.Type}");
#endif

            //using var _ = new TimeWatch(" ------- Archetype.Add ");

            ref readonly var typeInfo = ref Get(id);

            int typeIndex = info.Id;

            _types.Clear();

            // insert the type
            int length = typeInfo.Types.Length;
            for (int i = 0; i < length; i++)
            {
                ref readonly var type = ref typeInfo.Types[i];

                // insert the type in order
                if (typeIndex >= 0 && typeIndex < type.Id)
                {
                    _types.Add(info);
                    typeIndex = -1;
                }

                // add the type
                _types.Add(type);
            }

            if (typeIndex >= 0)
            {
                _types.Add(info);
            }

            // add the type to the end
            ulong bloomFlag = typeInfo.BloomFlag | info.BloomFlag;
            return Register(_types, typeInfo.BloomFlag | bloomFlag, out var __);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ArchetypeId Remove<T>(in ArchetypeId id)
        {
            if (!Has<T>(id)) return id;

            return Remove(id, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ArchetypeId Remove(in ArchetypeId id, in Type type)
        {
            //using var _ = new TimeWatch(" ------- Archetype.Remove ");

            bool hasType = false;

            ulong bloomFlag = 0ul;

            ref readonly var typeInfo = ref Get(id);
            _types.Clear();
            int length = typeInfo.Types.Length;
            for (int i = 0; i < length; i++)
            {
                ref readonly var info = ref typeInfo.Types[i];

                // remove the type
                if (info.Type == type)
                {
                    hasType = true;
                    continue;
                }

                // update bloom flag
                _types.Add(info);
                bloomFlag |= info.BloomFlag;
            }

#if UNITY_EDITOR
            if (!hasType)
                throw new InvalidOperationException($"{id} does not have the type: {type}");
#endif

            // register archetype
            return Register(_types, bloomFlag, out var __);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ArchetypeId Combine(in ArchetypeId id1, in ArchetypeId id2)
        {
            //using var _ = new TimeWatch(" ------- Archetype.Combine ");
            ref readonly var typeInfo1 = ref Get(id1);
            ref readonly var typeInfo2 = ref Get(id2);

            _types.Clear();
            _hashSet.Clear();

            int i1 = 0, len1 = typeInfo1.Types.Length;
            int i2 = 0, len2 = typeInfo2.Types.Length;

            while (i1 < len1 || i2 < len2)
            {
                byte result = 0;
                TypeInfo info = default;

                if (i1 < len1)
                {
                    info = typeInfo1.Types[i1];
                    result = 1;
                }

                if (i2 < len2)
                {
                    var info2 = typeInfo2.Types[i2];
                    if (result == 0 || info2.Id < info.Id)
                    {
                        info = info2;
                        result = 2;
                    }
                }

                if (result == 1)
                {
                    i1++;
                    if (_hashSet.Add(info.Id))
                    {
                        _types.Add(info);
                    }
                }
                else if (result == 2)
                {
                    i2++;
                    if (_hashSet.Add(info.Id))
                    {
                        _types.Add(info);
                    }
                }
            }

            return Register(_types, typeInfo1.BloomFlag | typeInfo2.BloomFlag, out var __);
        }

        /// <summary>
        /// Register Archetype with multiple type infos
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static ArchetypeId RegisterTypes(IReadOnlyCollection<TypeInfo> types)
        {
            //using var _ = new TimeWatch(" ------- Archetype.RegisterTypes1 ");

            if (null == types || types.Count == 0)
            {
                return ArchetypeId.Invalid;
            }

            _types.Clear();

            ulong bloomFlag = 0;

            foreach (var info in types)
            {
                _types.Add(info);

                bloomFlag |= info.BloomFlag;
            }

            _types.Sort();

            // register archetype
            return Register(_types, bloomFlag, out bool _);
        }

        /// <summary>
        /// Register Archetype with multiple types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static ArchetypeId RegisterTypes(IReadOnlyCollection<Type> types)
        {
            //using var _ = new TimeWatch(" ------- Archetype.RegisterTypes2 ");

            if (null == types || types.Count == 0)
            {
                return ArchetypeId.Invalid;
            }

            _types.Clear();

            ulong bloomFlag = 0;

            foreach (var type in types)
            {
                ref readonly var info = ref GetTypeInfo(type);

                _types.Add(info);

                bloomFlag |= info.BloomFlag;
            }

            _types.Sort();

            // register archetype
            return Register(_types, bloomFlag, out bool _);
        }

        /// <summary>
        /// Register Archetype with multiple objects
        /// </summary>
        /// <param name="typeObjects"></param>
        /// <returns></returns>
        public static ArchetypeId RegisterObjects(IReadOnlyCollection<Object> typeObjects)
        {
            //using var _ = new TimeWatch(" ------- Archetype.RegisterObjects ");

            if (null == typeObjects || typeObjects.Count == 0)
            {
                return ArchetypeId.Invalid;
            }

            _types.Clear();

            ulong bloomFlag = 0;

            foreach (var obj in typeObjects)
            {
                ref readonly var info = ref GetTypeInfo(obj.GetType());

                _types.Add(info);

                bloomFlag |= info.BloomFlag;
            }

            _types.Sort();

            // register archetype
            return Register(_types, bloomFlag, out bool _);
        }
    }

    public sealed class Archetype<T> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T>.Info,
            };

            BloomFlag = type<T>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3, T4> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
                type<T4>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3, T4, T5> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
                type<T4>.Info,
                type<T5>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                        type<T5>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3, T4, T5, T6> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
                type<T4>.Info,
                type<T5>.Info,
                type<T6>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                        type<T5>.BloomFlag | type<T6>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3, T4, T5, T6, T7> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
                type<T4>.Info,
                type<T5>.Info,
                type<T6>.Info,
                type<T7>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                        type<T5>.BloomFlag | type<T6>.BloomFlag | type<T7>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3, T4, T5, T6, T7, T8> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
                type<T4>.Info,
                type<T5>.Info,
                type<T6>.Info,
                type<T7>.Info,
                type<T8>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                        type<T5>.BloomFlag | type<T6>.BloomFlag | type<T7>.BloomFlag | type<T8>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
                type<T4>.Info,
                type<T5>.Info,
                type<T6>.Info,
                type<T7>.Info,
                type<T8>.Info,
                type<T9>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                        type<T5>.BloomFlag | type<T6>.BloomFlag | type<T7>.BloomFlag | type<T8>.BloomFlag |
                        type<T9>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }

    public sealed class Archetype<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : Archetype
    {
        public static readonly ArchetypeId Id;
        public static readonly TypeInfo[] Types;
        public static readonly ulong BloomFlag;

        static Archetype()
        {
            Types = new TypeInfo[]
            {
                type<T1>.Info,
                type<T2>.Info,
                type<T3>.Info,
                type<T4>.Info,
                type<T5>.Info,
                type<T6>.Info,
                type<T7>.Info,
                type<T8>.Info,
                type<T9>.Info,
                type<T10>.Info,
            };
            BloomFlag = type<T1>.BloomFlag | type<T2>.BloomFlag | type<T3>.BloomFlag | type<T4>.BloomFlag |
                        type<T5>.BloomFlag | type<T6>.BloomFlag | type<T7>.BloomFlag | type<T8>.BloomFlag |
                        type<T9>.BloomFlag | type<T10>.BloomFlag;
            Id = Archetype.Register(ref Types, BloomFlag);
        }
    }
}