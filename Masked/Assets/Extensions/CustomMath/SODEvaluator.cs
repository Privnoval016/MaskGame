using Extensions.Utils;
using UnityEngine;

namespace Extensions.CustomMath
{

    public class SODEvaluator : MonoBehaviour
    {
        private SecondOrderDynamics _dynamics;
    
        [Header("Dynamics Parameters")]
    
        public SODInfo sodInfo;

        public Transform target;

        private float f0, z0, r0;

        [HideInInspector] public Vector3 output;

        private void Awake()
        {
            InitializeDynamics();
        }

        private void Update()
        {
            UpdateOutputPosition();
        }

        private void UpdateOutputPosition()
        {
            if (target == null) return;
            
            if (sodInfo.frequency != f0 || sodInfo.dampingRatio != z0 || sodInfo.responseTime != r0) 
                InitializeDynamics();
            else
            {
                Vector3? dynamicsOutput = _dynamics.Update(Time.unscaledDeltaTime, target.position);
            
                if (dynamicsOutput.Vec3NotNull())
                {
                    Vector3 nonNull = (Vector3) dynamicsOutput;
                    output = nonNull;
                }
            }
        }

        private void InitializeDynamics()
        {
            f0 = sodInfo.frequency;
            z0 = sodInfo.dampingRatio;
            r0 = sodInfo.responseTime;
            _dynamics = new SecondOrderDynamics(f0, z0, r0, transform.position);
        }
        
        public void SetTargetTransform(Transform t)
        {
            target = t;
        }
    }

}
