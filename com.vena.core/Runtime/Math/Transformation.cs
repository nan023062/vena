namespace Vena.Math
{
    public static class Transformation
    {
        public static Vector3 PointToWorldSpace(Vector3 point, Vector3 heading, Vector3 side, Vector3 agentPosition)
        {
            //局部坐标到世界坐标
            //矩阵为当前局部坐标相对于世界坐标的变换
            Matrix2D localToWorld = new Matrix2D();
            localToWorld._11 = heading.X;
            localToWorld._12 = -heading.Z;
            localToWorld._13 = agentPosition.X;
            localToWorld._21 = heading.Z;
            localToWorld._22 = heading.X;
            localToWorld._23 = agentPosition.Z;
            localToWorld._31 = 0;
            localToWorld._32 = 0;
            localToWorld._33 = 1;

            var v = localToWorld.TransformPoint(new Vector2(point.X, point.Z));
            return new Vector3(v.X, 0, v.Y);
        }

        public static Vector3 PointToLocalSpace(Vector3 point, Vector3 heading, Vector3 siding, Vector3 agentPosition)
        {
            //世界坐标到局部坐标
            //矩阵为世界坐标相对于当前局部坐标的变换
            Matrix2D worldToLocal = new Matrix2D();
            float lx = Vector3.Dot(-agentPosition, heading);
            float ly = Vector3.Dot(-agentPosition, siding);
            worldToLocal._11 = heading.X;
            worldToLocal._12 = heading.Z;
            worldToLocal._13 = lx;
            worldToLocal._21 = -heading.Z;
            worldToLocal._22 = heading.X;
            worldToLocal._23 = ly;
            worldToLocal._31 = 0;
            worldToLocal._32 = 0;
            worldToLocal._33 = 1;

            var v = worldToLocal.TransformPoint(new Vector2(point.X, point.Z));

            return new Vector3(v.X, 0, v.Y);
        }

        public static Vector3 DirToWorldSpace(Vector3 dir, Vector3 heading, Vector3 siding)
        {
            Matrix2D localToWorld = new Matrix2D();
            localToWorld._11 = heading.X;
            localToWorld._12 = -heading.Z;
            localToWorld._13 = 0;
            localToWorld._21 = heading.Z;
            localToWorld._22 = heading.X;
            localToWorld._23 = 0;
            localToWorld._31 = 0;
            localToWorld._32 = 0;
            localToWorld._33 = 1;

            var v = localToWorld.TransformVector(new Vector2(dir.X, dir.Z));

            return new Vector3(v.X, 0, v.Y);
        }
    }
}