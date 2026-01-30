using System;
using UnityEngine;

namespace Extensions.Utils
{
    [Serializable]
    public struct TransformInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Vector3 Forward => Rotation * Vector3.forward;
        public Vector3 Right => Rotation * Vector3.right;
        public Vector3 Up => Rotation * Vector3.up;
        

        public TransformInfo(bool defaultTransform = true)
        {
            this.Position = Vector3.zero;
            this.Rotation = Quaternion.identity;
            this.Scale = defaultTransform ? Vector3.one : Vector3.zero;
        }

        public TransformInfo(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        public override string ToString()
        {
            return $"Position: {Position}, Rotation: {Rotation.eulerAngles}, Scale: {Scale}";
        }


        public TransformInfo(Transform t, bool local = true)
        {
            if (local)
            {
                Position = t.localPosition;
                Rotation = t.localRotation;
                Scale = t.localScale;
            }
            else
            {
                Position = t.position;
                Rotation = t.rotation;
                Scale = t.lossyScale;
            }
        }

        /*
         * Converts the TransformInfo from a local space relative to the parent transform to world space
         */
        public TransformInfo ConvertToWorldSpace(Transform parent, bool ignoreScale = false)
        {
            if (parent == null) return this;
            
            Vector3 newPos = parent.TransformPoint(Position);
            Quaternion newRot = parent.rotation * Rotation;
            
            if (ignoreScale)
            {
                return new TransformInfo(newPos, newRot, Scale);
            }
            
            Vector3 newScale = new Vector3(Scale.x * parent.lossyScale.x, Scale.y * parent.lossyScale.y,
                Scale.z * parent.lossyScale.z);

            return new TransformInfo(newPos, newRot, newScale);
        }

        /*
         * Converts the TransformInfo from world space to a local space relative to the parent transform
         */
        public TransformInfo ConvertToLocalSpace(Transform parent, bool ignoreScale = false)
        {
            if (parent == null) return this;
            
            Vector3 newPos = parent.InverseTransformPoint(Position);
            Quaternion newRot = Quaternion.Inverse(parent.rotation) * Rotation;
            
            if (ignoreScale)
            {
                return new TransformInfo(newPos, newRot, Scale);
            }
            
            Vector3 newScale = new Vector3(Scale.x / parent.lossyScale.x, Scale.y / parent.lossyScale.y,
                Scale.z / parent.lossyScale.z);

            return new TransformInfo(newPos, newRot, newScale);
        }
    }
}