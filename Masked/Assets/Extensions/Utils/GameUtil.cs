using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using PrimeTween;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.VFX;


namespace Extensions.Utils
{
    public static class GameUtil
    {
        #region Coroutines

        public static IEnumerator<float> RunAfterDelay(float delay, Action action)
        {
            yield return Timing.WaitForSeconds(delay);
            action();
        }

        public static CoroutineHandle RunSegmentCoroutine(this MonoBehaviour m, IEnumerator<float> coroutine,
            string tag = null,
            Segment segment = Segment.RealtimeUpdate)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return Timing.RunCoroutine(coroutine.CancelWith(m), segment, m.GetInstanceID());
            }

            return Timing.RunCoroutine(coroutine.CancelWith(m), segment, m.GetInstanceID(), tag);
        }

        public static void KillObjectCoroutines(this MonoBehaviour m, string tag = null)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Timing.KillCoroutines(m.GetInstanceID());
                return;
            }

            Timing.KillCoroutines(m.GetInstanceID(), tag);
        }

        #endregion

        /*
         *  Converts the target vector to a vector relative to the basis vector (treated as the forward vector)
         */
        public static Vector3 GetRelativeVector3(this Vector3 target, Vector3 basis)
        {
            return Quaternion.FromToRotation(Vector3.forward, basis) * target;
        }

        public static Vector2 GetRelativeVector2(this Vector2 target, Vector2 basis)
        {
            return Quaternion.FromToRotation(Vector2.up, basis) * target;
        }

        #region Tweening

        public static void TweenDistance(this Rigidbody rb, Vector3 direction, float distance, float time,
            Ease ease = Ease.Default)
        {
            direction.Normalize();
            rb.linearVelocity = Vector3.zero;
            Tween.RigidbodyMovePosition(rb, rb.position + direction * distance, time, ease);
        }
        
        public static void TweenDistance(this Transform t, Vector3 direction, float distance, float time,
            Ease ease = Ease.Default)
        {
            direction.Normalize();
            Tween.Position(t, t.position + direction * distance, time, ease);
        }

        #endregion

        #region Rigidbody Physics

        public static IEnumerator<float> TraverseDistanceInTime(this Rigidbody rb, Vector3 direction, float distance,
            float time, Func<bool> condition = null)
        {
            if (time <= 0 || distance <= 0)
            {
                yield break;
            }

            rb.linearVelocity = Vector3.zero;

            direction.Normalize();
            Vector3 initialPosition = rb.position;
            float startTime = Time.time;

            float impulse = rb.mass * (distance / time - rb.linearVelocity.magnitude);
            Debug.Log(
                $"Impulse: {impulse}, Mass: {rb.mass}, Distance: {distance}, Time: {time}, Direction: {direction}");
            rb.AddForce(direction * impulse, ForceMode.Impulse);

            yield return Timing.WaitUntilTrue(() => Vector3.Distance(rb.position, initialPosition) >= distance ||
                                                    Time.time - startTime >= time ||
                                                    (condition != null && condition()));

            rb.linearVelocity = Vector3.zero;
        }

        public static IEnumerator<float> TraverseWithVelocity(this Rigidbody rb, Vector3 direction, float magnitude,
            Func<bool> loopCondition)
        {
            direction.Normalize();

            while (loopCondition())
            {
                rb.linearVelocity = direction * magnitude;
                yield return Timing.WaitForOneFrame;
            }

            rb.linearVelocity = Vector3.zero;
        }

        public static Vector3 GetGroundedPosition(this Transform t, LayerMask groundMask = default,
            float distance = 100f)
        {
            RaycastHit hit;
            if (Physics.SphereCast(t.position, 0.1f, Vector3.down, out hit, distance, groundMask))
            {
                return hit.point;
            }

            return t.position;
        }

        #endregion

        #region Vector Transformations

        /*
         * Given a vector u, returns a vector that is radius units away from the origin and rotated by angle degrees
         */
        public static Vector3 FindRadialVector3(this Vector3 u, float radius, float angle)
        {
            u.Normalize();
            Vector3 axisOfRotation = Vector3.Cross(Vector3.forward, u);
            return Quaternion.AngleAxis(angle, axisOfRotation) * u * radius;
        }

        public static Transform GetClosestTransform(this Transform[] transforms, Vector3 v)
        {
            Transform closest = null;
            float minDistance = float.MaxValue;
            foreach (Transform tr in transforms)
            {
                float distance = Vector3.Distance(v, tr.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = tr;
                }
            }

            return closest;
        }

        #endregion

        #region VFX Graph

        #region Safe Setters (String)

        public static bool SafeSetVector4(this VisualEffect vfx, string name, Vector4 value)
        {
            if (!vfx.HasVector4(name)) return false;
            vfx.SetVector4(name, value);
            return true;
        }

        public static bool SafeSetFloat(this VisualEffect vfx, string name, float value)
        {
            if (!vfx.HasFloat(name)) return false;
            vfx.SetFloat(name, value);
            return true;
        }

        public static bool SafeSetInt(this VisualEffect vfx, string name, int value)
        {
            if (!vfx.HasInt(name)) return false;
            vfx.SetInt(name, value);
            return true;
        }

        public static bool SafeSetBool(this VisualEffect vfx, string name, bool value)
        {
            if (!vfx.HasBool(name)) return false;
            vfx.SetBool(name, value);
            return true;
        }

        public static bool SafeSetTexture(this VisualEffect vfx, string name, Texture value)
        {
            if (!vfx.HasTexture(name)) return false;
            vfx.SetTexture(name, value);
            return true;
        }

        public static bool SafeSetMesh(this VisualEffect vfx, string name, Mesh value)
        {
            if (!vfx.HasMesh(name)) return false;
            vfx.SetMesh(name, value);
            return true;
        }

        public static bool SafeSetVector3(this VisualEffect vfx, string name, Vector3 value)
        {
            if (!vfx.HasVector3(name)) return false;
            vfx.SetVector3(name, value);
            return true;
        }

        public static bool SafeSetVector2(this VisualEffect vfx, string name, Vector2 value)
        {
            if (!vfx.HasVector2(name)) return false;
            vfx.SetVector2(name, value);
            return true;
        }

        public static bool SafeSetGradient(this VisualEffect vfx, string name, Gradient value)
        {
            if (!vfx.HasGradient(name)) return false;
            vfx.SetGradient(name, value);
            return true;
        }

        #endregion

        #region Safe Setters (ID)

        public static bool SafeSetVector4(this VisualEffect vfx, int id, Vector4 value)
        {
            if (!vfx.HasVector4(id)) return false;
            vfx.SetVector4(id, value);
            return true;
        }

        public static bool SafeSetFloat(this VisualEffect vfx, int id, float value)
        {
            if (!vfx.HasFloat(id)) return false;
            vfx.SetFloat(id, value);
            return true;
        }

        public static bool SafeSetInt(this VisualEffect vfx, int id, int value)
        {
            if (!vfx.HasInt(id)) return false;
            vfx.SetInt(id, value);
            return true;
        }

        public static bool SafeSetBool(this VisualEffect vfx, int id, bool value)
        {
            if (!vfx.HasBool(id)) return false;
            vfx.SetBool(id, value);
            return true;
        }

        public static bool SafeSetTexture(this VisualEffect vfx, int id, Texture value)
        {
            if (!vfx.HasTexture(id)) return false;
            vfx.SetTexture(id, value);
            return true;
        }

        public static bool SafeSetMesh(this VisualEffect vfx, int id, Mesh value)
        {
            if (!vfx.HasMesh(id)) return false;
            vfx.SetMesh(id, value);
            return true;
        }

        public static bool SafeSetVector3(this VisualEffect vfx, int id, Vector3 value)
        {
            if (!vfx.HasVector3(id)) return false;
            vfx.SetVector3(id, value);
            return true;
        }

        public static bool SafeSetVector2(this VisualEffect vfx, int id, Vector2 value)
        {
            if (!vfx.HasVector2(id)) return false;
            vfx.SetVector2(id, value);
            return true;
        }

        public static bool SafeSetGradient(this VisualEffect vfx, int id, Gradient value)
        {
            if (!vfx.HasGradient(id)) return false;
            vfx.SetGradient(id, value);
            return true;
        }

        #endregion

        #endregion

        #region GameObject Functions

        public static void LookInDirection(this GameObject go, Vector3 direction)
        {
            go.transform.LookAt(direction + go.transform.position);
        }

        #endregion

        public static bool TryGetComponentInChildren<T>(this GameObject go, out T component) where T : Component
        {
            component = go.GetComponentInChildren<T>();
            return component != null;
        }

        public static void RefreshWith<T>(this List<T> list, IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }
        
        #region Collection Utilities
        
        /**
         * <summary>
         * Returns the first key in the dictionary that maps to the specified value.
         * </summary>
         *
         * <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
         * <typeparam name="TValue">The type of the values in the dictionary.</typeparam
         * <param name="dict">The dictionary to search.</param>
         * <param name="value">The value to look for.</param>
         *
         * <returns>The first key that maps to the specified value.</returns>
         * <exception cref="KeyNotFoundException">Thrown if the value is not found in the dictionary.</exception>
         */
        public static TKey InverseLookup<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value)
        {
            foreach (var kvp in dict)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException("The specified value was not found in the dictionary.");
        }
        
        /**
         * <summary>
         * Tries to get the first key in the dictionary that maps to the specified value.
         * </summary>
         *
         * <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
         * <typeparam name="TValue">The type of the values in the dictionary.</typeparam
         * <param name="dict">The dictionary to search.</param>
         * <param name="value">The value to look for.</param>
         * <param name="key">When this method returns, contains the first key that maps to the specified value,
         * if found; otherwise, the default value for the type of the key parameter.</param>
         *
         * <returns>true if the value was found; otherwise, false.</returns>
         */
        public static bool TryInverseLookup<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value, out TKey key)
        {
            foreach (var kvp in dict)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                {
                    key = kvp.Key;
                    return true;
                }
            }

            key = default;
            return false;
        }
        
        #endregion
    }

    [Serializable]
    public class CircularList<T> : List<T>
    {
        public CircularList() : base()
        {
        }

        public CircularList(List<T> list) : base(list)
        {
        }


        /**
         * <summary>
         * Returns the item at the index shifted by the specified amount, wrapping around the list if necessary.
         * </summary>
         * <param name="current">The current index.</param>
         * <param name="shift">The amount to shift the index by (can be negative).</param>
         * <returns>The item at the shifted index.</returns>
         */
        public T ItemAtShiftedIndex(int current, int shift)
        {
            return this[ShiftedIndex(current, shift)];
        }

        /**
         * <summary>
         * Returns the index shifted by the specified amount, wrapping around the list if necessary.
         * </summary>
         * <param name="current">The current index.</param>
         * <param name="shift">The amount to shift the index by (can be negative).</param>
         * <returns>The shifted index.</returns>
         */
        public int ShiftedIndex(int current, int shift)
        {
            int index = (current + shift) % Count;
            if (index < 0)
            {
                index = Count + index;
            }

            return index;
        }
    }
}