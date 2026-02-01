using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class BeatMapEditorWindow : EditorWindow
{
    private BeatMapData beatMapData;
    private AudioSource previewAudioSource;
    
    // Timeline settings
    private float currentBeat;
    private float zoom = 1f;
    private Vector2 scrollPosition;
    private Vector2 timelineScrollPosition;
    
    // Grid settings
    private int numLanes = 8;
    private float beatsPerRow = 4f; // How many beats to show per row
    private float cellHeight = 40f;
    private float cellWidth = 80f;
    private float timelineWidth = 100f;
    
    // Playback
    private bool isPlaying;
    private float playbackStartBeat;
    
    // Selection
    private HashSet<int> selectedNotes = new HashSet<int>();
    
    // Tool mode
    private enum ToolMode { Place, Erase, Select }
    private ToolMode currentTool = ToolMode.Place;
    
    // Grid snapping
    private float[] snapValues = { 0.25f, 0.5f, 1f, 2f, 4f };
    private int currentSnapIndex = 2; // Default to 1 beat
    
    [MenuItem("Window/BeatMap Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<BeatMapEditorWindow>("BeatMap Editor");
        window.minSize = new Vector2(800, 600);
    }
    
    private void OnEnable()
    {
        // Create audio source for preview
        GameObject audioObj = EditorUtility.CreateGameObjectWithHideFlags("BeatMapEditorAudio", HideFlags.HideAndDontSave);
        previewAudioSource = audioObj.AddComponent<AudioSource>();
        previewAudioSource.playOnAwake = false;
        
        EditorApplication.update += OnEditorUpdate;
    }
    
    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        
        if (previewAudioSource != null)
        {
            DestroyImmediate(previewAudioSource.gameObject);
        }
    }
    
    private void OnEditorUpdate()
    {
        if (isPlaying && previewAudioSource != null && previewAudioSource.isPlaying && beatMapData != null)
        {
            // Update current beat based on audio playback
            float secondsPerBeat = 60f / beatMapData.bpm;
            currentBeat = playbackStartBeat + (previewAudioSource.time / secondsPerBeat);
            Repaint();
            
            // Stop when audio finishes
            if (!previewAudioSource.isPlaying)
            {
                isPlaying = false;
            }
        }
    }
    
    private void OnGUI()
    {
        DrawToolbar();
        
        if (beatMapData == null)
        {
            DrawNoBeatMapSelected();
            return;
        }
        
        DrawControlPanel();
        DrawBeatMapGrid();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // BeatMap selection
        EditorGUI.BeginChangeCheck();
        beatMapData = (BeatMapData)EditorGUILayout.ObjectField(beatMapData, typeof(BeatMapData), false, GUILayout.Width(200));
        if (EditorGUI.EndChangeCheck())
        {
            OnBeatMapChanged();
        }
        
        GUILayout.FlexibleSpace();
        
        // Tool buttons
        currentTool = (ToolMode)GUILayout.Toolbar((int)currentTool, new[] { "Place", "Erase", "Select" }, GUILayout.Width(200));
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            SaveBeatMap();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawNoBeatMapSelected()
    {
        GUILayout.BeginArea(new Rect(position.width / 2 - 150, position.height / 2 - 50, 300, 100));
        GUILayout.Label("No BeatMap Selected", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create New BeatMap"))
        {
            CreateNewBeatMap();
        }
        
        GUILayout.EndArea();
    }
    
    private void DrawControlPanel()
    {
        float panelHeight = selectedNotes.Count > 0 ? 150f : 120f;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(panelHeight));
        
        // Playback controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel, GUILayout.Width(80));
        
        if (GUILayout.Button(isPlaying ? "Pause" : "Play", GUILayout.Width(80)))
        {
            TogglePlayback();
        }
        
        if (GUILayout.Button("Stop", GUILayout.Width(60)))
        {
            StopPlayback();
        }
        
        GUILayout.Space(20);
        
        EditorGUILayout.LabelField("Beat:", GUILayout.Width(40));
        float newBeat = EditorGUILayout.FloatField(currentBeat, GUILayout.Width(60));
        if (Mathf.Abs(newBeat - currentBeat) > 0.01f)
        {
            currentBeat = Mathf.Max(0, newBeat);
            if (isPlaying)
            {
                StopPlayback();
                // Update audio position when manually changing beat
                if (previewAudioSource != null && beatMapData != null && beatMapData.clip != null)
                {
                    float secondsPerBeat = 60f / beatMapData.bpm;
                    previewAudioSource.time = Mathf.Clamp(currentBeat * secondsPerBeat, 0f, beatMapData.clip.length);
                }
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Beat scrubber
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(80)); // Spacer
        float maxBeat = beatMapData.beats > 0 ? beatMapData.beats : 100f;
        float scrubberBeat = GUILayout.HorizontalSlider(currentBeat, 0, maxBeat);
        if (Mathf.Abs(scrubberBeat - currentBeat) > 0.01f)
        {
            currentBeat = scrubberBeat;
            // Update audio position when scrubbing during playback
            if (isPlaying && previewAudioSource != null && beatMapData != null && beatMapData.clip != null)
            {
                float secondsPerBeat = 60f / beatMapData.bpm;
                previewAudioSource.time = Mathf.Clamp(currentBeat * secondsPerBeat, 0f, beatMapData.clip.length);
                playbackStartBeat = currentBeat;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Grid settings
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel, GUILayout.Width(80));
        
        EditorGUILayout.LabelField("Lanes:", GUILayout.Width(50));
        numLanes = EditorGUILayout.IntSlider(numLanes, 1, 16, GUILayout.Width(150));
        
        GUILayout.Space(20);
        
        EditorGUILayout.LabelField("Snap:", GUILayout.Width(40));
        string[] snapLabels = snapValues.Select(v => v >= 1 ? $"{v:F0}" : $"1/{(int)(1/v)}").ToArray();
        currentSnapIndex = EditorGUILayout.Popup(currentSnapIndex, snapLabels, GUILayout.Width(60));
        EditorGUILayout.LabelField("beat", GUILayout.Width(40));
        
        GUILayout.Space(20);
        
        EditorGUILayout.LabelField("Zoom:", GUILayout.Width(45));
        zoom = EditorGUILayout.Slider(zoom, 0.5f, 3f, GUILayout.Width(150));
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Truth value editing - only show when notes are selected
        if (selectedNotes.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Selected: {selectedNotes.Count}", EditorStyles.boldLabel, GUILayout.Width(80));
            
            EditorGUILayout.LabelField("Set Truth:", GUILayout.Width(70));
            
            if (GUILayout.Button("False (0)", GUILayout.Width(80)))
            {
                SetSelectedNotesTruthValue(TruthValue.False);
            }
            
            if (GUILayout.Button("True (1)", GUILayout.Width(80)))
            {
                SetSelectedNotesTruthValue(TruthValue.True);
            }
            
            if (GUILayout.Button("Random", GUILayout.Width(80)))
            {
                SetSelectedNotesTruthValue(TruthValue.Random);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // Stats
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"BPM: {beatMapData.bpm} | Total Beats: {beatMapData.beats} | Notes: {(beatMapData.beatDataEntries?.Length ?? 0)}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void SetSelectedNotesTruthValue(TruthValue value)
    {
        foreach (int index in selectedNotes)
        {
            if (index >= 0 && index < beatMapData.beatDataEntries.Length)
            {
                beatMapData.beatDataEntries[index].truthValue = value;
            }
        }
        
        EditorUtility.SetDirty(beatMapData);
        Repaint();
    }
    
    private void DrawBeatMapGrid()
    {
        if (beatMapData.beatDataEntries == null)
        {
            beatMapData.beatDataEntries = new BeatDataEntry[0];
        }
        
        Rect gridRect = new Rect(0, 130, position.width, position.height - 130);
        GUILayout.BeginArea(gridRect);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        float scaledCellHeight = cellHeight * zoom;
        float scaledCellWidth = cellWidth * zoom;
        
        // Calculate grid dimensions
        int numRows = Mathf.CeilToInt((beatMapData.beats > 0 ? beatMapData.beats : 100f) / beatsPerRow) + 1;
        float gridHeight = numRows * (scaledCellHeight + 2);
        float gridWidth = timelineWidth + (numLanes * scaledCellWidth);
        
        Rect gridContentRect = GUILayoutUtility.GetRect(gridWidth, gridHeight);
        
        // Draw grid background
        EditorGUI.DrawRect(gridContentRect, new Color(0.2f, 0.2f, 0.2f));
        
        // Draw grid
        DrawGridLines(gridContentRect, scaledCellHeight, scaledCellWidth, numRows);
        DrawTimelineLabels(gridContentRect, scaledCellHeight, numRows);
        DrawNotes(gridContentRect, scaledCellHeight, scaledCellWidth);
        DrawPlaybackLine(gridContentRect, scaledCellHeight);
        
        // Handle input
        HandleGridInput(gridContentRect, scaledCellHeight, scaledCellWidth);
        
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private void DrawGridLines(Rect gridRect, float cellHeight, float cellWidth, int numRows)
    {
        // Determine subdivision level based on zoom
        float beatSubdivision = 1f;
        
        if (zoom >= 2f)
        {
            beatSubdivision = 0.25f; // Quarter beats
        }
        else if (zoom >= 1.5f)
        {
            beatSubdivision = 0.5f; // Half beats
        }
        
        // Draw subdivided horizontal lines
        float totalBeats = numRows * beatsPerRow;
        int totalLines = Mathf.CeilToInt(totalBeats / beatSubdivision);
        
        for (int i = 0; i <= totalLines; i++)
        {
            float beat = i * beatSubdivision;
            int row = Mathf.FloorToInt(beat / beatsPerRow);
            float beatInRow = beat - (row * beatsPerRow);
            float progress = beatInRow / beatsPerRow;
            
            float y = gridRect.y + row * (cellHeight + 2) + (progress * cellHeight);
            
            // Determine line strength based on beat position
            bool isMainBeat = Mathf.Abs(beat % 1f) < 0.01f;
            bool isMeasure = Mathf.Abs(beat % 4f) < 0.01f;
            
            Color lineColor;
            float lineWidth;
            
            if (isMeasure)
            {
                lineColor = new Color(0.6f, 0.6f, 0.6f);
                lineWidth = 2f;
            }
            else if (isMainBeat)
            {
                lineColor = new Color(0.45f, 0.45f, 0.45f);
                lineWidth = 1f;
            }
            else
            {
                lineColor = new Color(0.3f, 0.3f, 0.3f);
                lineWidth = 1f;
            }
            
            EditorGUI.DrawRect(new Rect(gridRect.x + timelineWidth, y, gridRect.width - timelineWidth, lineWidth), lineColor);
        }
        
        // Vertical lines (lanes)
        for (int lane = 0; lane <= numLanes; lane++)
        {
            float x = gridRect.x + timelineWidth + lane * cellWidth;
            EditorGUI.DrawRect(new Rect(x, gridRect.y, 1, gridRect.height), new Color(0.4f, 0.4f, 0.4f));
        }
        
        // Timeline separator
        EditorGUI.DrawRect(new Rect(gridRect.x + timelineWidth - 1, gridRect.y, 2, gridRect.height), new Color(0.6f, 0.6f, 0.6f));
    }
    
    private void DrawTimelineLabels(Rect gridRect, float cellHeight, int numRows)
    {
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleRight;
        labelStyle.normal.textColor = Color.white;
        
        // Determine label frequency based on zoom
        float labelFrequency = zoom >= 2f ? 0.5f : 1f;
        
        float totalBeats = numRows * beatsPerRow;
        int totalLabels = Mathf.CeilToInt(totalBeats / labelFrequency);
        
        for (int i = 0; i <= totalLabels; i++)
        {
            float beat = i * labelFrequency;
            int row = Mathf.FloorToInt(beat / beatsPerRow);
            float beatInRow = beat - (row * beatsPerRow);
            float progress = beatInRow / beatsPerRow;
            
            float y = gridRect.y + row * (cellHeight + 2) + (progress * cellHeight);
            
            // Align label with the beat line
            Rect labelRect = new Rect(gridRect.x, y - 8, timelineWidth - 5, 16);
            
            string labelText = labelFrequency >= 1f ? $"{beat:F0}" : $"{beat:F2}";
            EditorGUI.LabelField(labelRect, labelText, labelStyle);
        }
    }
    
    private void DrawNotes(Rect gridRect, float cellHeight, float cellWidth)
    {
        if (beatMapData.beatDataEntries == null) return;
        
        for (int i = 0; i < beatMapData.beatDataEntries.Length; i++)
        {
            BeatDataEntry entry = beatMapData.beatDataEntries[i];
            
            if (entry.laneIndex < 0 || entry.laneIndex >= numLanes) continue;
            
            // Calculate position
            int row = Mathf.FloorToInt(entry.beatStamp / beatsPerRow);
            float beatInRow = entry.beatStamp - (row * beatsPerRow);
            float beatProgress = beatInRow / beatsPerRow;
            
            float x = gridRect.x + timelineWidth + entry.laneIndex * cellWidth;
            float y = gridRect.y + row * (cellHeight + 2) + (beatProgress * cellHeight);
            
            // Color-code notes by truth value
            Color noteColor;
            if (selectedNotes.Contains(i))
            {
                noteColor = new Color(1f, 0.8f, 0.2f); // Yellow for selected
            }
            else
            {
                switch (entry.truthValue)
                {
                    case TruthValue.False:
                        noteColor = new Color(1f, 0.3f, 0.3f); // Red for False (0)
                        break;
                    case TruthValue.True:
                        noteColor = new Color(0.3f, 1f, 0.3f); // Green for True (1)
                        break;
                    case TruthValue.Random:
                    default:
                        noteColor = new Color(0.3f, 0.8f, 1f); // Blue for Random
                        break;
                }
            }
            
            EditorGUI.DrawRect(new Rect(x + 2, y - 3, cellWidth - 4, 6), noteColor);
            
            // Draw note border
            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(new Rect(x + 2, y - 3, cellWidth - 4, 6), Color.clear, Color.white);
        }
    }
    
    private void DrawPlaybackLine(Rect gridRect, float cellHeight)
    {
        if (currentBeat < 0) return;

        int row = Mathf.FloorToInt(currentBeat / beatsPerRow);
        float beatInRow = currentBeat - (row * beatsPerRow);
        float beatProgress = beatInRow / beatsPerRow;

        float y = gridRect.y + row * (cellHeight + 2) + (beatProgress * cellHeight);

        // Only draw if the playback line is visible in the grid
        if (y >= gridRect.y && y <= gridRect.y + gridRect.height)
        {
            Handles.BeginGUI();
            Handles.color = new Color(1f, 0f, 0f, 0.8f);
            Handles.DrawLine(
                new Vector3(gridRect.x + timelineWidth, y, 0),
                new Vector3(gridRect.x + gridRect.width, y, 0)
            );

            // Draw a thicker line for better visibility
            Handles.DrawLine(
                new Vector3(gridRect.x + timelineWidth, y + 1, 0),
                new Vector3(gridRect.x + gridRect.width, y + 1, 0)
            );
            Handles.EndGUI();
        }
    }

    private void HandleGridInput(Rect gridRect, float cellHeight, float cellWidth)
    {
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;

        if (!gridRect.Contains(mousePos)) return;

        // Calculate grid position
        float relativeX = mousePos.x - gridRect.x - timelineWidth;
        float relativeY = mousePos.y - gridRect.y;

        if (relativeX < 0) return; // Click in timeline area

        int lane = Mathf.FloorToInt(relativeX / cellWidth);
        int row = Mathf.FloorToInt(relativeY / (cellHeight + 2));
        float yInRow = relativeY - (row * (cellHeight + 2));
        float beatInRow = (yInRow / cellHeight) * beatsPerRow;
        float beat = row * beatsPerRow + beatInRow;

        // Snap to grid
        float snapValue = snapValues[currentSnapIndex];
        beat = Mathf.Round(beat / snapValue) * snapValue;

        if (lane < 0 || lane >= numLanes) return;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (currentTool == ToolMode.Place)
            {
                PlaceNote(beat, lane);
                e.Use();
            }
            else if (currentTool == ToolMode.Erase)
            {
                EraseNoteAt(beat, lane);
                e.Use();
            }
            else if (currentTool == ToolMode.Select)
            {
                SelectNoteAt(beat, lane, e.shift);
                e.Use();
            }
        }

        if (e.type == EventType.MouseDrag && currentTool == ToolMode.Erase)
        {
            EraseNoteAt(beat, lane);
            e.Use();
        }

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
            {
                DeleteSelectedNotes();
                e.Use();
            }
            else if (e.keyCode == KeyCode.Alpha0 || e.keyCode == KeyCode.Keypad0)
            {
                if (selectedNotes.Count > 0)
                {
                    SetSelectedNotesTruthValue(TruthValue.False);
                    e.Use();
                }
            }
            else if (e.keyCode == KeyCode.Alpha1 || e.keyCode == KeyCode.Keypad1)
            {
                if (selectedNotes.Count > 0)
                {
                    SetSelectedNotesTruthValue(TruthValue.True);
                    e.Use();
                }
            }
            else if (e.keyCode == KeyCode.R)
            {
                if (selectedNotes.Count > 0)
                {
                    SetSelectedNotesTruthValue(TruthValue.Random);
                    e.Use();
                }
            }
        }
    }

    private void PlaceNote(float beat, int lane)
    {
        // Check if note already exists at this position
        bool exists = false;
        foreach (var entry in beatMapData.beatDataEntries)
        {
            if (Mathf.Abs(entry.beatStamp - beat) < 0.01f && entry.laneIndex == lane)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            var newEntry = new BeatDataEntry { beatStamp = beat, laneIndex = lane };
            var list = beatMapData.beatDataEntries.ToList();
            list.Add(newEntry);
            beatMapData.beatDataEntries = list.ToArray();

            EditorUtility.SetDirty(beatMapData);
            Repaint();
        }
    }

    private void EraseNoteAt(float beat, int lane)
    {
        var list = beatMapData.beatDataEntries.ToList();
        float snapTolerance = snapValues[currentSnapIndex] * 0.5f;

        list.RemoveAll(entry =>
            Mathf.Abs(entry.beatStamp - beat) < snapTolerance &&
            entry.laneIndex == lane
        );

        if (list.Count != beatMapData.beatDataEntries.Length)
        {
            beatMapData.beatDataEntries = list.ToArray();
            EditorUtility.SetDirty(beatMapData);
            Repaint();
        }
    }

    private void SelectNoteAt(float beat, int lane, bool addToSelection)
    {
        if (!addToSelection)
        {
            selectedNotes.Clear();
        }

        float snapTolerance = snapValues[currentSnapIndex] * 0.5f;

        for (int i = 0; i < beatMapData.beatDataEntries.Length; i++)
        {
            var entry = beatMapData.beatDataEntries[i];
            if (Mathf.Abs(entry.beatStamp - beat) < snapTolerance && entry.laneIndex == lane)
            {
                if (selectedNotes.Contains(i))
                {
                    selectedNotes.Remove(i);
                }
                else
                {
                    selectedNotes.Add(i);
                }
                break;
            }
        }

        Repaint();
    }

    private void DeleteSelectedNotes()
    {
        if (selectedNotes.Count == 0) return;

        var list = beatMapData.beatDataEntries.ToList();
        var indicesToRemove = selectedNotes.OrderByDescending(i => i).ToList();

        foreach (int index in indicesToRemove)
        {
            if (index >= 0 && index < list.Count)
            {
                list.RemoveAt(index);
            }
        }

        beatMapData.beatDataEntries = list.ToArray();
        selectedNotes.Clear();

        EditorUtility.SetDirty(beatMapData);
        Repaint();
    }

    private void TogglePlayback()
    {
        if (beatMapData.clip == null)
        {
            Debug.LogWarning("No audio clip assigned to BeatMap!");
            return;
        }

        if (!isPlaying)
        {
            // Start playback
            previewAudioSource.clip = beatMapData.clip;
            float secondsPerBeat = 60f / beatMapData.bpm;
            previewAudioSource.time = currentBeat * secondsPerBeat;
            previewAudioSource.Play();

            playbackStartBeat = currentBeat;
            isPlaying = true;
        }
        else
        {
            // Pause playback
            previewAudioSource.Pause();
            isPlaying = false;
        }
    }

    private void StopPlayback()
    {
        if (previewAudioSource != null && previewAudioSource.isPlaying)
        {
            previewAudioSource.Stop();
        }
        isPlaying = false;
        currentBeat = 0f;
        Repaint();
    }

    private void OnBeatMapChanged()
    {
        StopPlayback();
        currentBeat = 0f;
        selectedNotes.Clear();

        if (beatMapData != null)
        {
            // Auto-detect number of lanes
            if (beatMapData.beatDataEntries != null && beatMapData.beatDataEntries.Length > 0)
            {
                int maxLane = beatMapData.beatDataEntries.Max(e => e.laneIndex);
                numLanes = Mathf.Max(numLanes, maxLane + 1);
            }
        }
    }

    private void SaveBeatMap()
    {
        if (beatMapData != null)
        {
            EditorUtility.SetDirty(beatMapData);
            AssetDatabase.SaveAssets();
            Debug.Log($"BeatMap '{beatMapData.name}' saved!");
        }
    }

    private void CreateNewBeatMap()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New BeatMap",
            "NewBeatMap",
            "asset",
            "Choose a location for the new BeatMap"
        );

        if (!string.IsNullOrEmpty(path))
        {
            BeatMapData newBeatMap = CreateInstance<BeatMapData>();
            newBeatMap.bpm = 120;
            newBeatMap.beats = 100;
            newBeatMap.beatDataEntries = new BeatDataEntry[0];

            AssetDatabase.CreateAsset(newBeatMap, path);
            AssetDatabase.SaveAssets();

            beatMapData = newBeatMap;
            OnBeatMapChanged();
        }
    }
}


