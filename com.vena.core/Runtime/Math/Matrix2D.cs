namespace Vena.Math
{
    public struct Matrix2D
    {
        private Value _matrix;

        public struct Value
        {
            public float _11, _12, _13;
            public float _21, _22, _23;
            public float _31, _32, _33;
        }

        public float _11
        {
            set => _matrix._11 = value;
        }

        public float _12
        {
            set => _matrix._12 = value;
        }

        public float _13
        {
            set => _matrix._13 = value;
        }

        public float _21
        {
            set => _matrix._21 = value;
        }

        public float _22
        {
            set => _matrix._22 = value;
        }

        public float _23
        {
            set => _matrix._23 = value;
        }

        public float _31
        {
            set => _matrix._31 = value;
        }

        public float _32
        {
            set => _matrix._32 = value;
        }

        public float _33
        {
            set => _matrix._33 = value;
        }

        public Value matrix => _matrix;

        public void Rotate(in Vector2 forward)
        {
            Value matrix = new Value
            {
                _11 = forward.X,
                _12 = -forward.Y,
                _13 = 0,
                _21 = forward.Y,
                _22 = forward.X,
                _23 = 0,
                _31 = 0,
                _32 = 0,
                _33 = 1
            };

            MultiplyMatrix(matrix);
        }

        public void Translate(in Vector2 point)
        {
            Value matrix = new Value
            {
                _11 = 1,
                _12 = 0,
                _13 = point.X,
                _21 = 0,
                _22 = 1,
                _23 = point.Y,
                _31 = 0,
                _32 = 0,
                _33 = 1
            };

            MultiplyMatrix(matrix);
        }

        public void MultiplyMatrix(in Value inMatrix)
        {
            _matrix = new Value
            {
                //matrix multiply
                _11 = inMatrix._11 * _matrix._11 + inMatrix._12 * _matrix._21 + inMatrix._13 * _matrix._31,
                _12 = inMatrix._11 * _matrix._12 + inMatrix._12 * _matrix._22 + inMatrix._13 * _matrix._32,
                _13 = inMatrix._11 * _matrix._13 + inMatrix._12 * _matrix._23 + inMatrix._13 * _matrix._33,
                _21 = inMatrix._21 * _matrix._11 + inMatrix._22 * _matrix._21 + inMatrix._23 * _matrix._31,
                _22 = inMatrix._21 * _matrix._12 + inMatrix._22 * _matrix._22 + inMatrix._23 * _matrix._32,
                _23 = inMatrix._21 * _matrix._13 + inMatrix._22 * _matrix._23 + inMatrix._23 * _matrix._33,
                _31 = inMatrix._31 * _matrix._11 + inMatrix._32 * _matrix._21 + inMatrix._33 * _matrix._31,
                _32 = inMatrix._31 * _matrix._12 + inMatrix._32 * _matrix._22 + inMatrix._33 * _matrix._32,
                _33 = inMatrix._31 * _matrix._13 + inMatrix._32 * _matrix._23 + inMatrix._33 * _matrix._33
            };
        }

        public readonly Vector2 TransformPoint(in Vector2 point)
        {
            return new Vector2
            {
                X = _matrix._11 * point.X + _matrix._12 * point.Y + _matrix._13 * 1,
                Y = _matrix._21 * point.X + _matrix._22 * point.Y + _matrix._23 * 1
            };
        }

        public readonly Vector2 TransformVector(in Vector2 direction)
        {
            return new Vector2
            {
                X = _matrix._11 * direction.X + _matrix._12 * direction.Y,
                Y = _matrix._21 * direction.X + _matrix._22 * direction.Y
            };
        }
    }
}