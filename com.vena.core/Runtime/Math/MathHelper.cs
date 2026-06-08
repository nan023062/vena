using System.Runtime.CompilerServices;

namespace Vena.Math
{
    public static class MathHelper
    {
        public const float E = (float)System.Math.E;
        public const float Infinity = float.PositiveInfinity;
        public const float Log10E = 0.4342945f;
        public const float Log2E = 1.442695f;
        public const float Pi = (float)System.Math.PI;
        public const float PiOver2 = (float)(System.Math.PI / 2.0);
        public const float PiOver4 = (float)(System.Math.PI / 4.0);
        public const float TwoPi = (float)(System.Math.PI * 2.0);

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEqual(this float a, float b, float epsilon = 0.0001f)
        {
            return System.Math.Abs(a - b) < epsilon;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2)
        {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

        public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using doubles not to lose precission
            double amountSquared = amount * amount;
            double amountCubed = amountSquared * amount;
            return (float)(0.5 * (2.0 * value2 +
                                  (value3 - value1) * amount +
                                  (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                                  (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }

        public static float Clamp(float value, float min, float max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float Distance(float value1, float value2)
        {
            return System.Math.Abs(value1 - value2);
        }

        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                         (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                         t1 * s +
                         v1;
            return (float)result;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float Max(float value1, float value2)
        {
            return System.Math.Max(value1, value2);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static int Max(int value1, int value2)
        {
            return System.Math.Max(value1, value2);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static uint Max(uint value1, uint value2)
        {
            return System.Math.Max(value1, value2);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static long Max(long value1, long value2)
        {
            return System.Math.Max(value1, value2);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ulong Max(ulong value1, ulong value2)
        {
            return System.Math.Max(value1, value2);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float Min(float value1, float value2)
        {
            return System.Math.Min(value1, value2);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static int Min(int value1, int value2)
        {
            return System.Math.Min(value1, value2);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static uint Min(uint value1, uint value2)
        {
            return System.Math.Min(value1, value2);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static long Min(long value1, long value2)
        {
            return System.Math.Min(value1, value2);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ulong Min(ulong value1, ulong value2)
        {
            return System.Math.Min(value1, value2);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2(Max(value1.X, value2.X), Max(value1.Y, value2.Y));
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            return new Vector3(Max(value1.X, value2.X), Max(value1.Y, value2.Y), Max(value1.Z, value2.Z));
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2(Min(value1.X, value2.X), Min(value1.Y, value2.Y));
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            return new Vector3(Min(value1.X, value2.X), Min(value1.Y, value2.Y), Min(value1.Z, value2.Z));
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T value1, ref T value2)
        {
            (value1, value2) = (value2, value1);
        }

        public static float SmoothStep(float value1, float value2, float amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
#if(USE_FARSEER)
            float result = SilverSpriteMathHelper.Clamp(amount, 0f, 1f);
            result = SilverSpriteMathHelper.Hermite(value1, 0f, value2, 0f, result);
#else
            float result = MathHelper.Clamp(amount, 0f, 1f);
            result = MathHelper.Hermite(value1, 0f, value2, 0f, result);
#endif
            return result;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float ToDegrees(float radians)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = 180 / pi
            return (float)(radians * 57.295779513082320876798154814105);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float ToRadians(float degrees)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = pi / 180
            return (float)(degrees * 0.017453292519943295769236907684886);
        }

        public static float WrapAngle(float angle)
        {
            angle = (float)System.Math.IEEERemainder((double)angle, 6.2831854820251465);
            if (angle <= -3.14159274f)
            {
                angle += 6.28318548f;
            }
            else
            {
                if (angle > 3.14159274f)
                {
                    angle -= 6.28318548f;
                }
            }

            return angle;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float num)
        {
            return (float)System.Math.Sqrt(num);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float Abs(float f)
        {
            return System.Math.Abs(f);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static int Abs(int i)
        {
            return System.Math.Abs(i);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static long Abs(long l)
        {
            return System.Math.Abs(l);
        }
    }
}