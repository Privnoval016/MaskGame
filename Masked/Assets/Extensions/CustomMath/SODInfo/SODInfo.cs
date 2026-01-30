using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.CustomMath
{
    [CreateAssetMenu(fileName = "SODInfo", menuName = "ExtensionSO/SODInfo", order = 0)]
    public class SODInfo : ScriptableObject
    {
        [Range(0, 10)] public float frequency = 3f;
        [Range(0, 2)] public float dampingRatio = 0.7f;
        [Range(0, 2)] public float responseTime = 0.5f;
    }
}
