using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.UtilityAI
{
    public abstract class Sensor<T> : MonoBehaviour
    {
        public float detectionRadius = 10;
        
        public List<T> detectionTags = new();
        
        private readonly HashSet<Transform> detectedObjects = new(10);
        SphereCollider sphereCollider;
        
        private void Awake()
        {
            if (!TryGetComponent(out sphereCollider))
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }
            
            sphereCollider.isTrigger = true;
            sphereCollider.radius = detectionRadius;
            
            Collider[] initialColliders = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (var c in initialColliders)
            {
                ProcessTrigger(c, t => detectedObjects.Add(t));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            ProcessTrigger(other, t => detectedObjects.Add(t));
        }

        private void OnTriggerExit(Collider other)
        {
            ProcessTrigger(other, t => detectedObjects.Remove(t));
        }
        
        private void ProcessTrigger(Collider other, Action<Transform> action)
        {
            foreach (var detectionTag in detectionTags)
            {
                if (HasDetectionTag(detectionTag, other))
                {
                    action(other.transform);
                    break;
                }
            }
        }
        
        public Transform GetNearestDetectedObject(T actionKey)
        {
            Transform nearest = null;
            float nearestDistanceSqr = float.MaxValue;
            Vector3 currentPosition = transform.position;
            
            foreach (var obj in detectedObjects)
            {
                if (obj == null) continue; // Skip null references
                
                if (!HasDetectionTag(actionKey, obj.GetComponent<Collider>()))
                    continue;

                Vector3 directionToTarget = obj.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < nearestDistanceSqr)
                {
                    nearestDistanceSqr = dSqrToTarget;
                    nearest = obj;
                }
            }

            return nearest;
        }
        
        protected abstract bool HasDetectionTag(T actionKey, Collider other);
    }
}