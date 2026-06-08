using System;
using System.Runtime.CompilerServices;

namespace Vena.Math
{
    public struct Hierarchy : IEquatable<Hierarchy>
    {
        #region private variable
        
        private float _radius;
        private Vector3 _position;
        private Quaternion _quaternion;

        #endregion

        #region public properties

        public Vector3 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _position = value;
        }

        public Quaternion Rotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _quaternion;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _quaternion = value;
        }

        public float Radius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _radius;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _radius = value;
        }
        
        #endregion

        public bool Equals(Hierarchy other)
        {
            return _radius.Equals(other._radius) && _position.Equals(other._position) && _quaternion.Equals(other._quaternion);
        }

        public override bool Equals(object obj)
        {
            return obj is Hierarchy other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _radius.GetHashCode();
                hashCode = (hashCode * 397) ^ _position.GetHashCode();
                hashCode = (hashCode * 397) ^ _quaternion.GetHashCode();
                return hashCode;
            }
        }
    }
}