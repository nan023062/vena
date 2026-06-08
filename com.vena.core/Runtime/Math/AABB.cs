namespace Vena.Math
{
    public class AABB
    {
        public Vector3[] Bounds => _bounds;

        private readonly Vector3[] _bounds = new Vector3[2];

        #region public method

        /// <summary>
        /// 测试aabb和ray的相交
        /// </summary>
        /// <returns></returns>
        public bool InsectRay(Ray ray, float t)
        {
            var inverDir = ray.InverDir;
            var sign = ray.Sign;
            var origin = ray.Origin;

            var bounds = _bounds;

            float tmin = (bounds[sign[0]].X - origin.X) * inverDir.X;
            float tmax = (bounds[1 - sign[0]].X - origin.X) * inverDir.X;
            float tzmin = (bounds[sign[2]].Z - origin.Z) * inverDir.Z;
            float tzmax = (bounds[1 - sign[2]].Z - origin.Z) * inverDir.Z;

            if (tmin > tzmax || tzmin > tmax)
            {
                return false;
            }

            if (tzmin > tmin)
            {
                tmin = tzmin;
            }

            if (tzmax < tmax)
            {
                tmax = tzmax;
            }

            return tmin > 0 && tmax < t;
        }

        #endregion
    }
}