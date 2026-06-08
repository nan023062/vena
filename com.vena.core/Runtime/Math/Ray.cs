namespace Vena.Math
{
    public struct Ray
    {
        public Vector3 Origin
        {
            get { return _origin; }
        }

        public Vector3 Dir
        {
            get { return _dir; }
        }

        public Vector3 InverDir
        {
            get { return _inverDir; }
        }

        public int[] Sign
        {
            get { return _sign; }
        }

        public Ray(Vector3 origin, Vector3 dir)
        {
            _origin = origin;
            _dir = dir;
            _inverDir = new Vector3(1 / dir.X, 1 / dir.Y, 1 / dir.Z);
            _sign = new int[3];
            _sign[0] = _inverDir.X < 0 ? 1 : 0;
            _sign[1] = _inverDir.Y < 0 ? 1 : 0;
            _sign[2] = _inverDir.Z < 0 ? 1 : 0;
        }

        private Vector3 _origin;
        private Vector3 _dir;
        private Vector3 _inverDir;
        private int[] _sign;
    }
}