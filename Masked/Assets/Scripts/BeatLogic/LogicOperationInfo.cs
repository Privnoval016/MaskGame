using UnityEngine;

[CreateAssetMenu(fileName = "LogicOperationInfo", menuName = "BeatLogic/LogicOperationInfo", order = 1)]
public class LogicOperationInfo : ScriptableObject
{
    public LogicOperation operation;
    [ColorUsage(true, false)]
    public Color baseColor;
    
    [ColorUsage(true, false)]
    public Color darkColor;
    
    [ColorUsage(true, true)]
    public Color hdrColor;
}