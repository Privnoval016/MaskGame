using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialColorShifter : MonoBehaviour
{
    public float shiftDuration = 0.3f;
    [Header("Materials")]
    [SerializeField] private Material[] materialsToShiftRegular;

    [SerializeField] private Material[] materialsToShiftDark;
    [SerializeField] private Material[] materialsToShiftHDR;
    [SerializeField] private Material[] materialsToShiftMediumHDR;

    [SerializeField] private TMP_Text[] textsToApplyGlow;
    
    [SerializeField] private Image[] imagesToApplyColor;
    [SerializeField] private Image[] imagesToApplyInverseColor;
    
    [SerializeField] private CustomBaseColorOverride[] customBaseColorOverrides;

    [SerializeField] private Material scrollingMaterial;
    [SerializeField] private Material backgroundMaterial;
    
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

        foreach (TMP_Text text in textsToApplyGlow)
        {
            // duplicate material and enable glow keyword
            
            if (text == null) continue;
            Material textMat = Instantiate(text.fontMaterial);
            textMat.EnableKeyword("GLOW_ON");
            text.fontMaterial = textMat;
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
            Color newColor = new Color(info.baseColor.r, info.baseColor.g, info.baseColor.b, mat.color.a);
            Tween.MaterialProperty(mat, BaseColorID, newColor, shiftDuration);
        }
        
        // Custom base color overrides
        foreach (var overrideEntry in customBaseColorOverrides)
        {
            if (overrideEntry.material == null) continue;
            int propertyID = Shader.PropertyToID(overrideEntry.propertyName);
            Tween.MaterialProperty(overrideEntry.material, propertyID, info.baseColor, shiftDuration);
        }

        foreach (var mat in materialsToShiftDark)
        {
            if (mat == null) continue;
            Color newColor = new Color(info.darkColor.r, info.darkColor.g, info.darkColor.b, mat.color.a);
            Tween.MaterialProperty(mat, BaseColorID, newColor, shiftDuration);
        }

        // Emission / HDR materials
        foreach (var mat in materialsToShiftHDR)
        {
            if (mat == null) continue;

            mat.EnableKeyword("_EMISSION");

            // IMPORTANT: emission should be color * intensity
            Color emission = info.hdrColor;
            Tween.MaterialProperty(mat, EmissionColorID, emission, shiftDuration);
        }
        
        foreach (var mat in materialsToShiftMediumHDR)
        {
            if (mat == null) continue;

            mat.EnableKeyword("_EMISSION");

            // IMPORTANT: emission should be color * intensity
            Color emission = info.mediumHdrColor;
            Tween.MaterialProperty(mat, EmissionColorID, emission, shiftDuration);
        }
        
        // Text glow colors
        foreach (var text in textsToApplyGlow)
        {
            if (text == null) continue;
            Material textMat = text.fontMaterial;
            int glowColorID = Shader.PropertyToID("_GlowColor");
            Tween.MaterialProperty(textMat, glowColorID, info.baseColor, shiftDuration);
        }
        
        // Image colors
        foreach (var image in imagesToApplyColor)
        {
            if (image == null) continue;
            Tween.Color(image, info.baseColor, shiftDuration);
        }
        
        foreach (var image in imagesToApplyInverseColor)
        {
            if (image == null) continue;
            Color inverseColor = info.InvertedColor;
            Tween.Color(image, inverseColor, shiftDuration);
        }
        
        int neonColorID = Shader.PropertyToID("_CoreColor");
        int haloColorID = Shader.PropertyToID("_GlowColor");
        Sequence.Create()
            .Group(Tween.MaterialProperty(scrollingMaterial, neonColorID, info.mediumHdrColor, shiftDuration))
            .Group(Tween.MaterialProperty(scrollingMaterial, haloColorID, info.haloColor, shiftDuration));
        
        int colorID = Shader.PropertyToID("_Color");
        Tween.MaterialProperty(backgroundMaterial, colorID, info.baseColor, shiftDuration);
    }
    
    public LogicOperationInfo GetCurrentLogicData()
    {
        var operation = BeatMapManager.Instance.activeLogicOperation;
        if (logicOperationInfoDict.TryGetValue(operation, out var info))
        {
            return info;
        }
        return null;
    }

    [Serializable]
    public struct CustomBaseColorOverride
    {
        public Material material;
        public string propertyName;
        
    }
}