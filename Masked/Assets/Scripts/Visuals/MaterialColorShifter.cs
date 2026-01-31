using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class MaterialColorShifter : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material[] materialsToShiftRegular;

    [SerializeField] private Material[] materialsToShiftDark;
    [SerializeField] private Material[] materialsToShiftHDR;

    [Header("Logic Mapping")]
    [SerializeField] private LogicOperationInfo[] logicOperationInfos;

    private readonly Dictionary<LogicOperation, LogicOperationInfo> logicOperationInfoDict = new();

    // Cache shader IDs (important)
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        logicOperationInfoDict.Clear();

        foreach (var info in logicOperationInfos)
        {
            if (!logicOperationInfoDict.ContainsKey(info.operation))
                logicOperationInfoDict.Add(info.operation, info);
        }
    }

    public void ShiftColors(LogicOperation operation)
    {
        if (!logicOperationInfoDict.TryGetValue(operation, out var info))
        {
            Debug.LogWarning($"No LogicOperationInfo found for {operation}");
            return;
        }

        // Base color materials
        foreach (var mat in materialsToShiftRegular)
        {
            if (mat == null) continue;
            Tween.MaterialProperty(mat, BaseColorID, info.baseColor, 0.3f);
        }

        foreach (var mat in materialsToShiftDark)
        {
            if (mat == null) continue;
            Tween.MaterialProperty(mat, BaseColorID, info.darkColor, 0.3f);
        }

        // Emission / HDR materials
        foreach (var mat in materialsToShiftHDR)
        {
            if (mat == null) continue;

            mat.EnableKeyword("_EMISSION");

            // IMPORTANT: emission should be color * intensity
            Color emission = info.hdrColor;
            Tween.MaterialProperty(mat, EmissionColorID, emission, 0.3f);
        }
    }
}