using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(BeatMapData))]
public class BeatMapDataEditor : Editor
{
    private BeatMapData beatMap;
    private bool showBeatData = true;
    private bool showCheckpoints = true;
    private Vector2 beatDataScrollPos;
    private Vector2 checkpointScrollPos;
    
    private void OnEnable()
    {
        beatMap = (BeatMapData)target;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawHeaderSection();
        EditorGUILayout.Space(10);
        
        DrawSongDataSection();
        EditorGUILayout.Space(5);
        
        DrawBeatDataSection();
        EditorGUILayout.Space(5);
        
        DrawCheckpointsSection();
        EditorGUILayout.Space(10);
        
        DrawFooter();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawHeaderSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.LabelField("BeatMap Editor", titleStyle);
        EditorGUILayout.Space(5);
        
        // Quick stats
        GUIStyle statsStyle = new GUIStyle(EditorStyles.miniLabel);
        statsStyle.alignment = TextAnchor.MiddleCenter;
        statsStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        
        int noteCount = beatMap.beatDataEntries?.Length ?? 0;
        int checkpointCount = beatMap.checkpoints?.Length ?? 0;
        float duration = beatMap.bpm > 0 ? (beatMap.beats / (float)beatMap.bpm) * 60f : 0f;
        
        EditorGUILayout.LabelField(
            $"{noteCount} Notes | {checkpointCount} Checkpoints | {duration:F1}s Duration", 
            statsStyle
        );
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSongDataSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Song Info", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        // Song Title
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Song Title", GUILayout.Width(100));
        SerializedProperty titleProp = serializedObject.FindProperty("songTitle");
        EditorGUILayout.PropertyField(titleProp, GUIContent.none);
        EditorGUILayout.EndHorizontal();
        
        // Author
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Author", GUILayout.Width(100));
        SerializedProperty authorProp = serializedObject.FindProperty("author");
        EditorGUILayout.PropertyField(authorProp, GUIContent.none);
        EditorGUILayout.EndHorizontal();
        
        // Cover Art
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Cover Art", GUILayout.Width(100));
        SerializedProperty coverProp = serializedObject.FindProperty("coverArt");
        EditorGUILayout.PropertyField(coverProp, GUIContent.none);
        EditorGUILayout.EndHorizontal();
        
        if (beatMap.coverArt == null)
        {
            EditorGUILayout.HelpBox("Assign a cover art sprite for menu display", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Song Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        // Audio Clip - Make it prominent
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Audio Clip", GUILayout.Width(100));
        SerializedProperty clipProp = serializedObject.FindProperty("clip");
        EditorGUILayout.PropertyField(clipProp, GUIContent.none);
        EditorGUILayout.EndHorizontal();
        
        if (beatMap.clip == null)
        {
            EditorGUILayout.HelpBox("Assign an audio clip to enable playback in the editor", MessageType.Warning);
        }
        
        EditorGUILayout.Space(5);
        
        // BPM and Beats in horizontal layout
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("BPM (Tempo)", EditorStyles.miniLabel);
        SerializedProperty bpmProp = serializedObject.FindProperty("bpm");
        bpmProp.intValue = EditorGUILayout.IntField(bpmProp.intValue);
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(20);
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Total Beats", EditorStyles.miniLabel);
        SerializedProperty beatsProp = serializedObject.FindProperty("beats");
        beatsProp.intValue = EditorGUILayout.IntField(beatsProp.intValue);
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(20);
        
        // Calculate and show duration
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Duration", EditorStyles.miniLabel);
        float duration = beatMap.bpm > 0 ? (beatMap.beats / (float)beatMap.bpm) * 60f : 0f;
        EditorGUILayout.LabelField($"{duration:F2}s", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Visual Data - Beat line settings
        EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Beat Line Rate", EditorStyles.miniLabel);
        SerializedProperty beatLineRateProp = serializedObject.FindProperty("beatLineRate");
        beatLineRateProp.intValue = EditorGUILayout.IntField(beatLineRateProp.intValue);
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(20);
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Super Beat Line Rate", EditorStyles.miniLabel);
        SerializedProperty superBeatLineRateProp = serializedObject.FindProperty("superBeatLineRate");
        superBeatLineRateProp.intValue = EditorGUILayout.IntField(superBeatLineRateProp.intValue);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox("Beat Line Rate: spawn a beat line every N beats\nSuper Beat Line Rate: spawn emphasized line every N beats", MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBeatDataSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        showBeatData = EditorGUILayout.Foldout(showBeatData, $"Beat Data ({beatMap.beatDataEntries?.Length ?? 0} notes)", true, EditorStyles.foldoutHeader);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Open Editor", GUILayout.Width(100)))
        {
            BeatMapEditorWindow.ShowWindow();
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (showBeatData)
        {
            EditorGUILayout.Space(5);
            
            if (beatMap.beatDataEntries == null || beatMap.beatDataEntries.Length == 0)
            {
                EditorGUILayout.HelpBox("No notes yet. Open the BeatMap Editor to start adding notes!", MessageType.Info);
            }
            else
            {
                // Show note distribution by lane
                var laneGroups = beatMap.beatDataEntries.GroupBy(e => e.laneIndex).OrderBy(g => g.Key);
                
                EditorGUILayout.LabelField("Notes per Lane:", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                
                foreach (var group in laneGroups)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(60));
                    EditorGUILayout.LabelField($"Lane {group.Key}", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.LabelField($"{group.Count()}", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Show detailed list (limited)
                EditorGUILayout.LabelField("Recent Notes:", EditorStyles.miniLabel);
                beatDataScrollPos = EditorGUILayout.BeginScrollView(beatDataScrollPos, GUILayout.Height(150));
                
                var sortedEntries = beatMap.beatDataEntries.OrderByDescending(e => e.beatStamp).Take(20);
                
                foreach (var entry in sortedEntries)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Beat {entry.beatStamp:F2}", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Lane {entry.laneIndex}", GUILayout.Width(60));
                    
                    // Color code truth value
                    GUIStyle truthStyle = new GUIStyle(EditorStyles.miniLabel);
                    truthStyle.normal.textColor = entry.truthValue == TruthValue.False ? new Color(1f, 0.3f, 0.3f) :
                                                   entry.truthValue == TruthValue.True ? new Color(0.3f, 1f, 0.3f) :
                                                   new Color(0.3f, 0.8f, 1f);
                    
                    EditorGUILayout.LabelField($"Value: {entry.truthValue}", truthStyle, GUILayout.Width(100));
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
                
                if (beatMap.beatDataEntries.Length > 20)
                {
                    EditorGUILayout.LabelField($"... and {beatMap.beatDataEntries.Length - 20} more", EditorStyles.miniLabel);
                }
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear All Notes"))
            {
                if (EditorUtility.DisplayDialog("Clear All Notes", 
                    "Are you sure you want to delete all notes? This cannot be undone.", 
                    "Delete", "Cancel"))
                {
                    beatMap.beatDataEntries = new BeatDataEntry[0];
                    EditorUtility.SetDirty(beatMap);
                }
            }
            
            if (GUILayout.Button("Sort by Beat"))
            {
                if (beatMap.beatDataEntries != null)
                {
                    System.Array.Sort(beatMap.beatDataEntries, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
                    EditorUtility.SetDirty(beatMap);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawCheckpointsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        showCheckpoints = EditorGUILayout.Foldout(showCheckpoints, $"Checkpoints ({beatMap.checkpoints?.Length ?? 0})", true, EditorStyles.foldoutHeader);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Add Checkpoint", GUILayout.Width(120)))
        {
            AddCheckpoint();
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (showCheckpoints)
        {
            EditorGUILayout.Space(5);
            
            if (beatMap.checkpoints == null || beatMap.checkpoints.Length == 0)
            {
                EditorGUILayout.HelpBox("No checkpoints. Add checkpoints to mark important sections of your song!", MessageType.Info);
            }
            else
            {
                checkpointScrollPos = EditorGUILayout.BeginScrollView(checkpointScrollPos, GUILayout.Height(150));
                
                SerializedProperty checkpointsProp = serializedObject.FindProperty("checkpoints");
                
                for (int i = 0; i < beatMap.checkpoints.Length; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField($"Checkpoint {i}", EditorStyles.boldLabel, GUILayout.Width(100));
                    
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        DeleteCheckpoint(i);
                        break;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    SerializedProperty checkpointProp = checkpointsProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Name:", GUILayout.Width(50));
                    EditorGUILayout.PropertyField(checkpointProp.FindPropertyRelative("checkpointName"), GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Beat:", GUILayout.Width(50));
                    EditorGUILayout.PropertyField(checkpointProp.FindPropertyRelative("beatStamp"), GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        

        EditorGUILayout.EndVertical();
    }
    
    private void DrawFooter()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Open BeatMap Editor", GUILayout.Height(35)))
        {
            BeatMapEditorWindow.ShowWindow();
        }
        
        if (GUILayout.Button("Save", GUILayout.Height(35), GUILayout.Width(80)))
        {
            EditorUtility.SetDirty(beatMap);
            AssetDatabase.SaveAssets();
            Debug.Log($"BeatMap '{beatMap.name}' saved!");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void AddCheckpoint()
    {
        var list = beatMap.checkpoints?.ToList() ?? new System.Collections.Generic.List<BeatMapCheckpoint>();
        list.Add(new BeatMapCheckpoint 
        { 
            checkpointName = $"Checkpoint {list.Count + 1}",
            beatStamp = 0f
        });
        beatMap.checkpoints = list.ToArray();
        EditorUtility.SetDirty(beatMap);
    }
    
    private void DeleteCheckpoint(int index)
    {
        var list = beatMap.checkpoints.ToList();
        list.RemoveAt(index);
        beatMap.checkpoints = list.ToArray();
        EditorUtility.SetDirty(beatMap);
    }
}

