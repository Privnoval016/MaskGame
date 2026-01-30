using System;
using UnityEngine;

namespace Extensions.Utils
{
    public static class RandomUtil
    {
        #region UnityEngine.Random wrappers

        public static float RandomRange(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static int RandomRange(int min, int max)
        {
            return UnityEngine.Random.Range(min, max + 1);
        }

        public static int RandomRangeExclusive(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        #endregion

        #region System.Random extensions

        public static float NextFloat(this System.Random random)
        {
            return (float)random.NextDouble();
        }

        public static int Range(this System.Random random, int min, int max)
        {
            return random.RangeExclusive(min, max + 1);
        }

        public static float Range(this System.Random random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

        public static int RangeExclusive(this System.Random random, int min, int max)
        {
            return min + (int)(random.NextDouble() * (max - min));
        }

        public static int Range(this Unity.Mathematics.Random random, int min, int max)
        {
            return random.RangeExclusive(min, max + 1);
        }

        public static int RangeExclusive(this Unity.Mathematics.Random random, int min, int max)
        {
            return min + (int)(random.NextDouble() * (max - min));
        }

        public static uint NextUInt(this System.Random random)
        {
            byte[] bytes = new byte[4];
            random.NextBytes(bytes);
            return BitConverter.ToUInt32(bytes);
        }

        #endregion

        #region New Random Functions

        public static int RandomSign()
        {
            return RandomBool(0.5f) ? 1 : -1;
        }

        public static bool RandomBool(float weight = 0.5f)
        {
            return RandomUFloat() < weight;
        }

        public static float RandomFloat()
        {
            return RandomRange(-1f, 1f);
        }

        public static float RandomUFloat()
        {
            return RandomRange(0f, 1f);
        }

        public static Vector2 RandomVector2Circular()
        {
            float angle = RandomUFloat() * 360;
            float distance = Mathf.Sqrt(RandomUFloat());
            return Vector2.up.Rotate(angle) * distance;
        }

        #endregion
    }
}
