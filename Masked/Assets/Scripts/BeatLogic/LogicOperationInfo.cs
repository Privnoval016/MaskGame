using UnityEngine;

[CreateAssetMenu(fileName = "LogicOperationInfo", menuName = "BeatLogic/LogicOperationInfo", order = 1)]
public class LogicOperationInfo : ScriptableObject
{
    public LogicOperation operation;
    public string displayName;
    
    [ColorUsage(true, false)]
    public Color baseColor;
    
    [ColorUsage(true, false)]
    public Color darkColor;
    
    [ColorUsage(true, true)]
    public Color hdrColor;
    
    [ColorUsage(true, true)]
    public Color mediumHdrColor;
    
    [ColorUsage(true, false)]
    public Color neonColor;
    
    [ColorUsage(true, false)]
    public Color haloColor;
    
    public Color InvertedColor => InvertHueKeepBrightness(baseColor);
    public Color InvertedNeonColor => InvertHueKeepBrightness(neonColor);
    
    private Color InvertHueKeepBrightness(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        
        h = (h + 0.5f) % 1f;

        Color result = Color.HSVToRGB(h, s, v);

        // Preserve alpha
        result.a = color.a;

        return result;
    }
}