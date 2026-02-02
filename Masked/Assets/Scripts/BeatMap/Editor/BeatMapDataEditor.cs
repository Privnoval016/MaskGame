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
        
        DrawPlayableDifficultiesSection();
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
        
        // Quick stats - sum all difficulties
        GUIStyle statsStyle = new GUIStyle(EditorStyles.miniLabel);
        statsStyle.alignment = TextAnchor.MiddleCenter;
        statsStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        
        int totalNotes = 0;
        if (beatMap.difficultyBeatMaps != null)
        {
            foreach (var diffMap in beatMap.difficultyBeatMaps)
            {
                if (diffMap.beatDataEntries != null)
                    totalNotes += diffMap.beatDataEntries.Length;
            }
        }
        
        int checkpointCount = beatMap.checkpoints?.Length ?? 0;
        float duration = beatMap.bpm > 0 ? (beatMap.beats / (float)beatMap.bpm) * 60f : 0f;
        
        EditorGUILayout.LabelField(
            $"{totalNotes} Total Notes | {checkpointCount} Checkpoints | {duration:F1}s Duration", 
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
    
    private void DrawPlayableDifficultiesSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Playable Difficulties", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        
        EditorGUILayout.HelpBox("Check which difficulties are ready to be played in the menu. Unchecked difficulties will be grayed out and unselectable.", MessageType.Info);
        EditorGUILayout.Space(5);
        
        // Get serialized properties
        SerializedProperty easyPlayableProp = serializedObject.FindProperty("easyPlayable");
        SerializedProperty mediumPlayableProp = serializedObject.FindProperty("mediumPlayable");
        SerializedProperty hardPlayableProp = serializedObject.FindProperty("hardPlayable");
        SerializedProperty expertPlayableProp = serializedObject.FindProperty("expertPlayable");
        SerializedProperty superExpertPlayableProp = serializedObject.FindProperty("superExpertPlayable");
        
        // Draw checkboxes with color-coded labels
        DrawDifficultyCheckbox(easyPlayableProp, "Easy", GetDifficultyColor(Difficulty.Easy));
        DrawDifficultyCheckbox(mediumPlayableProp, "Medium", GetDifficultyColor(Difficulty.Medium));
        DrawDifficultyCheckbox(hardPlayableProp, "Hard", GetDifficultyColor(Difficulty.Hard));
        DrawDifficultyCheckbox(expertPlayableProp, "Expert", GetDifficultyColor(Difficulty.Expert));
        DrawDifficultyCheckbox(superExpertPlayableProp, "Super Expert", GetDifficultyColor(Difficulty.SuperExpert));
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDifficultyCheckbox(SerializedProperty prop, string label, Color color)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Checkbox
        prop.boolValue = EditorGUILayout.Toggle(prop.boolValue, GUILayout.Width(20));
        
        // Colored label
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = color;
        labelStyle.fontStyle = FontStyle.Bold;
        
        EditorGUILayout.LabelField(label, labelStyle);
        
        // Show note count for this difficulty
        int difficultyIndex = GetDifficultyIndex(label);
        if (difficultyIndex >= 0 && beatMap.difficultyBeatMaps != null && difficultyIndex < beatMap.difficultyBeatMaps.Length)
        {
            int noteCount = beatMap.difficultyBeatMaps[difficultyIndex].beatDataEntries?.Length ?? 0;
            EditorGUILayout.LabelField($"({noteCount} notes)", EditorStyles.miniLabel, GUILayout.Width(80));
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private int GetDifficultyIndex(string label)
    {
        switch (label)
        {
            case "Easy": return 0;
            case "Medium": return 1;
            case "Hard": return 2;
            case "Expert": return 3;
            case "Super Expert": return 4;
            default: return -1;
        }
    }
    
    private void DrawBeatDataSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        int totalNotes = 0;
        if (beatMap.difficultyBeatMaps != null)
        {
            foreach (var diffMap in beatMap.difficultyBeatMaps)
            {
                if (diffMap.beatDataEntries != null)
                    totalNotes += diffMap.beatDataEntries.Length;
            }
        }
        
        showBeatData = EditorGUILayout.Foldout(showBeatData, $"Beat Data by Difficulty ({totalNotes} total notes)", true, EditorStyles.foldoutHeader);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Open Editor", GUILayout.Width(100)))
        {
            BeatMapEditorWindow.ShowWindow();
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (showBeatData)
        {
            EditorGUILayout.Space(5);
            
            if (beatMap.difficultyBeatMaps == null || beatMap.difficultyBeatMaps.Length == 0)
            {
                EditorGUILayout.HelpBox("No difficulty beatmaps initialized. Open the BeatMap Editor to start adding notes!", MessageType.Info);
            }
            else
            {
                // Show each difficulty with note count
                for (int i = 0; i < beatMap.difficultyBeatMaps.Length && i < 5; i++)
                {
                    var diffMap = beatMap.difficultyBeatMaps[i];
                    int noteCount = diffMap.beatDataEntries?.Length ?? 0;
                    
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    
                    // Difficulty name with color coding
                    GUIStyle diffStyle = new GUIStyle(EditorStyles.boldLabel);
                    diffStyle.normal.textColor = GetDifficultyColor((Difficulty)i);
                    
                    EditorGUILayout.LabelField(((Difficulty)i).ToString(), diffStyle, GUILayout.Width(100));
                    EditorGUILayout.LabelField($"{noteCount} notes", EditorStyles.miniLabel);
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Use the BeatMap Editor window to add and edit notes for each difficulty.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private Color GetDifficultyColor(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return new Color(0.3f, 1f, 0.3f); // Green
            case Difficulty.Medium: return new Color(0.3f, 0.8f, 1f); // Blue
            case Difficulty.Hard: return new Color(1f, 0.8f, 0.2f); // Yellow
            case Difficulty.Expert: return new Color(1f, 0.5f, 0.2f); // Orange
            case Difficulty.SuperExpert: return new Color(1f, 0.3f, 0.3f); // Red
            default: return Color.white;
        }
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

