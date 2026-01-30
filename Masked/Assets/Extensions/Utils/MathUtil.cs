using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Extensions.Utils
{
    public static class MathUtil
    {
        public static readonly float SQRT_2 = Mathf.Sqrt(2);

        #region Transformations
        
        public static bool Vec3NotNull(this Vector3? vec)
        {
            return vec != null && !((Vector3) vec).x.IsNaN() && !((Vector3) vec).y.IsNaN() && !((Vector3) vec).z.IsNaN();
        }
        
        /*
         * Rotates a Vector2 by a given angle
         * @param vec: The Vector2 to rotate
         * @param angle: The angle to rotate by
         */
        public static Vector2 Rotate(this Vector2 vec, float angle)
        {
            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            return new Vector2(vec.x * cos - vec.y * sin, vec.x * sin + vec.y * cos);
        }
        
        public static Vector3 Rotate(this Vector3 vec, float degAngle, Vector3 axis)
        {
            return Quaternion.AngleAxis(degAngle, axis) * vec;
        }

        /*
         * Rotates a float2 by a given angle
         * @param vec: The float2 to rotate
         * @param angle: The angle to rotate by
         */
        public static float2 Rotate(this float2 vec, float angle)
        {
            float sin = math.sin(angle * Mathf.Deg2Rad);
            float cos = math.cos(angle * Mathf.Deg2Rad);
            return new float2(vec.x * cos - vec.y * sin, vec.x * sin + vec.y * cos);
        }

        /*
         * Creates a perpendicular Vector2 to the given Vector2 in the given direction
         * @param vector: The Vector2 to create a perpendicular Vector2 from
         * @param direction: The direction to create the perpendicular Vector2 in
         */
        public static Vector2 Perpendicular(this Vector2 vector, float direction)
        {
            if (direction > 0)
                return PerpendicularClockwise(vector);
            return PerpendicularCounterClockwise(vector);
        }
        
        /**
         * Gets the vector at the midpoint of two vectors
         * @param a: The first vector
         * @param b: The second vector
         */
        public static Vector3 Midpoint(this Vector3 a, Vector3 b)
        {
            return (a + b) / 2;
        }
        
        /**
         * Gets the vector at the midpoint of two vectors
         * @param a: The first vector
         * @param b: The second vector
         */
        public static Vector2 Midpoint(this Vector2 a, Vector2 b)
        {
            return (a + b) / 2;
        }

        
        /*
         * Creates a perpendicular Vector2 to the given Vector2 in the counter clockwise direction
         * @param vector: The Vector2 to create a perpendicular Vector2 from
         */
        public static Vector2 PerpendicularCounterClockwise(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        /*
         * Creates a perpendicular Vector2 to the given Vector2 in the clockwise direction
         * @param vector: The Vector2 to create a perpendicular Vector2 from
         */
        public static Vector2 PerpendicularClockwise(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        /*
         * Document later
         */
        public static Vector4 RotatedBoundingBox(Vector4 bounds, float rotation)
        {
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;

            for (int i = 0; i < 4; i++)
            {
                Vector2 position = bounds.XY() + bounds.ZW().FlipX(i < 2).FlipY(i % 2 == 0) / 2;
                position = position.Rotate(rotation);

                min = Vector2.Min(min, position);
                max = Vector2.Max(max, position);
            }

            // return bounds;
            return Merge(
                (min + max) / 2,
                new Vector2(Mathf.Abs(min.x - max.x), Mathf.Abs(min.y - max.y))
            );
        }
        #endregion

        
        #region Conversion
        

        public static Vector3 ToVector3(this Vector2 vec, char axisToIgnore = 'y')
        {
            switch (axisToIgnore)
            {
                case 'x':
                    return new Vector3(0, vec.x, vec.y);
                case 'z':
                    return new Vector3(vec.x, vec.y, 0);
                default:
                    return new Vector3(vec.x, 0, vec.y);
            }
        }


        public static Vector3 ZeroVector3Axis(this Vector3 vec, char axisToZero = 'y')
        {
            switch (axisToZero)
            {
                case 'x':
                    return new Vector3(0, vec.y, vec.z);
                case 'z':
                    return new Vector3(vec.x, vec.y, 0);
                default:
                    return new Vector3(vec.x, 0, vec.z);
            }
        }
        
        /**
         * Converts a Vector3 to a Vector2, with the option to ignore a specific axis
         * @param vec: The Vector2 to convert
         * @param axisToIgnore: The axis to ignore when converting
         */
        public static Vector2 ToVector2(this Vector3 vec, char axisToIgnore = 'y')
        {
            switch (axisToIgnore)
            {
                case 'x':
                    return new Vector2(vec.y, vec.z);
                case 'z':
                    return new Vector2(vec.x, vec.y);
                default:
                    return new Vector2(vec.x, vec.z);
            }
        }

        /**
         * Converts a Vector2Int to a Vector3Int, with the option to ignore a specific axis
         * @param vec: The Vector2Int to convert
         * @param axisToIgnore: The axis to ignore when converting
         */
        public static Vector3Int ToVector3Int(this Vector2Int vec, char axisToIgnore = 'y')
        {
            switch (axisToIgnore)
            {
                case 'x':
                    return new Vector3Int(0, vec.x, vec.y);
                case 'z':
                    return new Vector3Int(vec.x, vec.y, 0);
                default:
                    return new Vector3Int(vec.x, 0, vec.y);
            }
        }

        public static Vector2Int ToVector2Int(this Vector3Int vec)
        {
            return new Vector2Int(vec.x, vec.y);
        }

        public static int2 ToInt2(this Vector2Int vec)
        {
            return new int2(vec.x, vec.y);
        }

        public static Vector2Int ToVector2Int(this int2 vec)
        {
            return new Vector2Int(vec.x, vec.y);
        }

        public static Vector2 ToVector2(this int2 vec)
        {
            return new Vector2(vec.x, vec.y);
        }

        public static float2 ToFloat2(this Vector2 vec)
        {
            return new float2(vec.x, vec.y);
        }

        public static Vector2 ToVector2(this float2 vec)
        {
            return new Vector2(vec.x, vec.y);
        }

        /**
         * Retrieves the angle of a Vector2
         */
        public static float ToAngle(this Vector2 vec)
        {
            return Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
        }

        #endregion

        #region Basic Functions
        public static Vector2 Rounded(this Vector2 vec)
        {
            return new Vector2(Mathf.Round(vec.x), Mathf.Round(vec.y));
        }

        public static bool RoundEqual(this Vector2 a, Vector2 b)
        {
            return a.Rounded() == b.Rounded();
        }

        public static Vector2 Abs(this Vector2 vec)
        {
            return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
        }

        public static int Mod(int a, int b)
        {
            return (a % b + b) % b;
        }

        public static float Mod(float a, float b)
        {
            return (a % b + b) % b;
        }

        public static float LogN(float a, float n)
        {
            return Mathf.Log(a) / Mathf.Log(n);
        }

        public static float ModularDistance(float a, float b, float mod)
        {
            float dist = a.Diff(b);
            return Mathf.Min(dist, mod - dist);
        }

        public static float Sign(float n)
        {
            return n == 0 ? 0 : Mathf.Sign(n);
        }

        public static Vector2 Sign(Vector2 v)
        {
            return new Vector2(Sign(v.x), Sign(v.y));
        }

        public static bool IsNaN(this float n)
        {
            return float.IsNaN(n);
        }

        public static bool IsNaN(this double n)
        {
            return double.IsNaN(n);
        }

        public static float Squared(this float a)
        {
            return a * a;
        }

        public static float Pow(this float a, float b)
        {
            return Mathf.Pow(a, b);
        }

        public static float Diff(this float a, float b)
        {
            return Mathf.Abs(a - b);
        }

        public static float DirectionSignTo(this Vector2 from, Vector2 to)
        {
            return Sign(from.x * to.y - from.y * to.x);
        }
        #endregion

        #region Simple Adjustments
        public static Vector2 SwapAxes(this Vector2 vec, bool shouldFlip = true)
        {
            if (shouldFlip)
                return new Vector2(vec.y, vec.x);
            return vec;
        }

        public static Vector2Int SwapAxes(this Vector2Int vec, bool shouldFlip = true)
        {
            if (shouldFlip)
                return new Vector2Int(vec.y, vec.x);
            return vec;
        }

        public static bool EqualsPositiveInfinity(this Vector2 vec)
        {
            return float.IsPositiveInfinity(vec.x) && float.IsPositiveInfinity(vec.y);
        }

        public static int2 SwapAxes(this int2 vec, bool shouldFlip = true)
        {
            if (shouldFlip)
                return new int2(vec.y, vec.x);
            return vec;
        }

        public static float2 SwapAxes(this float2 vec, bool shouldFlip = true)
        {
            if (shouldFlip)
                return new float2(vec.y, vec.x);
            return vec;
        }

        public static float Max(this Vector2 vec)
        {
            return Mathf.Max(vec.x, vec.y);
        }

        public static float Max(this Vector3 vec)
        {
            return Mathf.Max(vec.x, vec.y, vec.z);
        }

        public static float Min(this Vector2 vec)
        {
            return Mathf.Min(vec.x, vec.y);
        }

        public static float Min(this Vector3 vec)
        {
            return Mathf.Min(vec.x, vec.y, vec.z);
        }

        public static float MinSigned(float a, float b, float sign)
        {
            return Mathf.Min(a * sign, b * sign) * sign;
        }

        public static float MaxSigned(float a, float b, float sign)
        {
            return Mathf.Max(a * sign, b * sign) * sign;
        }

        public static float MinAbs(float a, float min)
        {
            return Mathf.Abs(a) > min ? min * Mathf.Sign(a) : a;
        }

        public static Vector3 ScaledBy(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        
        public static Vector3 DividedBy(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static int Flip(this int a, bool isFlipped)
        {
            return isFlipped ? -a : a;
        }

        public static float Flip(this float a, bool isFlipped)
        {
            return isFlipped ? -a : a;
        }

        public static float Round(float a, float multiplier)
        {
            return Mathf.Round(a / multiplier) * multiplier;
        }

        public static float Floor(float a, float multiplier)
        {
            return Mathf.Floor(a / multiplier) * multiplier;
        }

        public static float Sin01(float f)
        {
            return 0.5f + Mathf.Sin(f) * 0.5f;
        }

        public static Vector2Int FlipX(this Vector2Int a, bool isFlipped = true)
        {
            return new Vector2Int(a.x.Flip(isFlipped), a.y);
        }

        public static Vector2Int FlipY(this Vector2Int a, bool isFlipped = true)
        {
            return new Vector2Int(a.x, a.y.Flip(isFlipped));
        }

        public static Vector2 FlipX(this Vector2 a, bool isFlipped = true)
        {
            return new Vector2(a.x.Flip(isFlipped), a.y);
        }

        public static Vector2 FlipY(this Vector2 a, bool isFlipped = true)
        {
            return new Vector2(a.x, a.y.Flip(isFlipped));
        }

        public static Vector3 FlipX(this Vector3 a, bool isFlipped = true)
        {
            return new Vector3(a.x.Flip(isFlipped), a.y, a.z);
        }

        public static Vector3 FlipY(this Vector3 a, bool isFlipped = true)
        {
            return new Vector3(a.x, a.y.Flip(isFlipped), a.z);
        }
        
        public static Vector3 FlipZ(this Vector3 a, bool isFlipped = true)
        {
            return new Vector3(a.x, a.y, a.z.Flip(isFlipped));
        }

        #endregion

        #region Swizzling
        public static Vector4 Merge(Vector2 a, Vector2 b)
        {
            return new Vector4(a.x, a.y, b.x, b.y);
        }

        public static Vector2 XY(this Vector4 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 ZW(this Vector4 v)
        {
            return new Vector2(v.z, v.w);
        }

        public static Vector3 WithX(this Vector3 vec, float x)
        {
            return new Vector3(x, vec.y, vec.z);
        }

        public static Vector3 WithY(this Vector3 vec, float y)
        {
            return new Vector3(vec.x, y, vec.z);
        }

        public static Vector3 WithZ(this Vector3 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }

        public static Vector2 WithX(this Vector2 vec, float x)
        {
            return new Vector2(x, vec.y);
        }

        public static Vector2 WithY(this Vector2 vec, float y)
        {
            return new Vector2(vec.x, y);
        }

        #endregion

        #region Specialized Functions
        public static Vector2 BezierCurve(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            return Vector2.Lerp(Vector2.Lerp(a, c, t), Vector2.Lerp(c, b, t), t);
        }

        public static float AddTowardsTarget(this float a, float target, float t)
        {
            if (a == target)
                return target;
            if (a < target)
                return Mathf.Min(target, a + t);
            return Mathf.Max(target, a - t);
        }

        public static float AccelerateToReachTarget(
            float current,
            float target,
            float velocity,
            float acceleration,
            float maxSpeed,
            float t
        )
        {
            float distance = target - current;
            float direction = Mathf.Sign(distance);

            float decelDistance = velocity.Squared() / (2 * acceleration);

            float aDistance = distance * direction;
            float aVelocity = velocity * direction;

            if (Mathf.Sign(aVelocity) == -1) //going in wrong direction
            {
                aVelocity = aVelocity + acceleration * t;
            }
            else if (aDistance > decelDistance)
            {
                aVelocity = Mathf.Min(aVelocity + acceleration * t, maxSpeed);
            }
            else
            {
                aVelocity = Mathf.Max(aVelocity - acceleration * t, 0);
            }

            return aVelocity * direction; //returns velocity not position
        }

        public static float AddAbs(this float a, float b, bool clamp = true)
        {
            float sign = Sign(a);
            float n = a + sign * b;

            if (!clamp)
                return n;

            switch (sign)
            {
                case 1:
                    return Mathf.Max(0, n);
                case -1:
                    return Mathf.Min(0, n);
                default:
                    return 0;
            }
        }

        #endregion

        #region Intersection + Distance
        public static float RectDistanceTo(this Vector2 a, Vector2 b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        public static float Vec2RectDistanceTo(this Vector3 a, Vector3 b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        public static Vector3 ClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
        {
            if (a == b)
                return a;

            Vector2 abDiff = b - a;
            float abDistance = abDiff.magnitude;

            Vector2 normalizedDiff = abDiff / abDistance;

            float dot = Vector2.Dot(normalizedDiff, point - a);

            if (dot < 0)
                return a;
            if (dot > abDistance)
                return b;
            return a + normalizedDiff * dot;
        }
        #endregion

        #region Binary Integer Math
        public static int BitToByteLength(int bitLength)
        {
            return (bitLength - 1) / 8 + 1;
        }

        public static int HashInt(params int[] vals)
        {
            int hash = 1009;
            foreach (int i in vals)
            {
                hash = (hash * 9176) + i;
            }
            return hash;
        }

        public static byte ClampToByte(this float i)
        {
            if (i < 0)
                return 0;
            if (i > byte.MaxValue)
                return byte.MaxValue;
            return (byte)i;
        }
        #endregion
        
        #region General Methods
    
        public static bool IsInDirectionCone(this Vector3 direction, Vector3 targetDirection, float angle, bool directional = true)
        {
            if (direction == targetDirection) return true;
            if (direction == Vector3.zero || targetDirection == Vector3.zero ) return false;
        
            //if the direction is not direction we dont care about the orientation of the vectors

            if (directional)
            {
                return Vector3.Dot(direction.normalized, targetDirection.normalized) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            }
            else
            {
                return Vector3.Angle(direction, targetDirection) < angle;
            }
        }
    
        public static bool IsInDirectionCone(this Vector2 direction, Vector2 targetDirection, float angle, bool directional = true)
        {
            if (direction == targetDirection) return true;
            if (direction == Vector2.zero || targetDirection == Vector2.zero ) return false;
        
            //if the direction is not direction we dont care about the orientation of the vectors

            if (directional)
            {
                return Vector2.Dot(direction.normalized, targetDirection.normalized) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            }
            else
            {
                return Vector2.Angle(direction, targetDirection) < angle;
            }
        }
        
    
        #endregion
    }
}
