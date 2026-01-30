using UnityEngine;

namespace Extensions.Utils
{
    public static class EaseUtil
    {
        #region Damping

        public static float Damp(float a, float b, float damping, float dt)
        {
            return Mathf.Lerp(a, b, BaseDamp(damping, dt));
        }

        public static float DampAngle(float a, float b, float damping, float dt)
        {
            return Mathf.LerpAngle(a, b, BaseDamp(damping, dt));
        }

        public static Quaternion DampQuaternion(Quaternion a, Quaternion b, float damping, float dt)
        {
            return Quaternion.Slerp(a, b, BaseDamp(damping, dt));
        }

        public static Vector2 DampVector2(Vector2 a, Vector2 b, float damping, float dt)
        {
            return Vector2.Lerp(a, b, BaseDamp(damping, dt));
        }

        public static Vector2 DampVector2(Vector2 a, Vector2 b, Vector2 damping, float dt)
        {
            return new Vector2(Damp(a.x, b.x, damping.x, dt), Damp(a.y, b.y, damping.y, dt));
        }

        public static Vector3 DampVector3(Vector3 a, Vector3 b, float damping, float dt)
        {
            return Vector3.Lerp(a, b, BaseDamp(damping, dt));
        }

        public static Vector3 DampVector3(Vector3 a, Vector3 b, Vector3 damping, float dt)
        {
            return new Vector3(
                Damp(a.x, b.x, damping.x, dt),
                Damp(a.y, b.y, damping.y, dt),
                Damp(a.z, b.z, damping.z, dt)
            );
        }

        static float BaseDamp(float damping, float dt)
        {
            return 1 - Mathf.Exp(-damping * dt);
        }

        #endregion

        #region Easing

        public static float Ease(float x, float easing, bool reverse = false)
        {
            float sign = Mathf.Sign(x);
            float abs = Mathf.Abs(x);
            if (reverse)
                return (1 - Mathf.Pow(1 - abs, easing)) * sign;
            else
                return Mathf.Pow(abs, easing) * sign;
        }

        public static float EaseInOut(float x, float power = 3)
        {
            if (x < 0.5f)
                return Mathf.Pow(2, power - 1) * Mathf.Pow(x, power);
            else
                return 1 - Mathf.Pow(-2 * x + 2, power) / 2;
        }

        public static float EaseBack(float x, float overshoot = 1.7f, bool reverse = false)
        {
            float x2 = (reverse ? 1 - x : x) - 1;
            float result = x2 * x2 * ((x2 + 1) * overshoot + x2) + 1;

            return reverse ? 1 - result : result;
        }

        //<1 no in/out
        //~2 is when in/out becomes clear
        public static float BellCurveEase(float x, float power = 3)
        {
            return Mathf.Pow(-4 * x * (x - 1), power);
        }

        #endregion
    }
}