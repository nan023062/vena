using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vena
{
    /// <summary>
    /// InstanceID is a unique identifier for an entity in the world.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct InstanceID : System.IEquatable<InstanceID>
    {
        public static readonly InstanceID Invalid = new InstanceID(0, 0);

        [FieldOffset(0)] public readonly uint id;

        [FieldOffset(4)] public readonly int world;

        [FieldOffset(0)] private readonly long _longId;

        public InstanceID(short world, uint id)
        {
            _longId = 0;
            this.world = world;
            this.id = id;
        }

        private InstanceID(long longId)
        {
            world = 0;
            id = 0;
            _longId = longId;
        }

        public override string ToString()
        {
            return $"<{world}_{id}>";
        }

        public static implicit operator long(InstanceID entityId)
        {
            return entityId._longId;
        }

        public static implicit operator InstanceID(long longId)
        {
            return new InstanceID(longId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(InstanceID lhs, InstanceID rhs)
        {
            return lhs._longId == rhs._longId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(InstanceID lhs, InstanceID rhs)
        {
            return lhs._longId != rhs._longId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(InstanceID other)
        {
            return _longId == other._longId;
        }

        public override bool Equals(object obj)
        {
            return obj is InstanceID other && Equals(other);
        }

        public override int GetHashCode() => _longId.GetHashCode();
    }
}