# BeatMap Editor & Gameplay Feature Updates

## Summary
This document outlines all the new features and enhancements made to the BeatMap Editor and gameplay systems.

---

## 1. Enhanced BeatMap Editor

### New Features

#### **Copy/Paste Functionality**
- **Ctrl+C**: Copy selected notes
- **Ctrl+V**: Paste notes at current beat position (maintains relative positioning)
- **Ctrl+A**: Select all notes
- **ESC**: Clear selection

#### **Batch Truth Value Assignment**
- Select multiple notes using the Select tool
- Assign truth values to all selected notes at once:
  - Click "False (0)" button or press **0** key
  - Click "True (1)" button or press **1** key
  - Click "Random" button or press **R** key

#### **Enhanced Note Display**
- Notes are now color-coded by truth value:
  - **Red**: False (0)
  - **Green**: True (1)
  - **Blue**: Random
  - **Yellow**: Selected notes

#### **Inspector Panel Updates**
- Truth values now displayed in the inspector for each note
- Color-coded truth values matching editor display
- Shows up to 20 most recent notes with their beat, lane, and truth value

### Keyboard Shortcuts Reference
- **Place/Erase/Select Tools**: Top toolbar buttons
- **Delete/Backspace**: Delete selected notes
- **0**: Set selected notes to False
- **1**: Set selected notes to True
- **R**: Set selected notes to Random
- **Ctrl+C**: Copy selected notes
- **Ctrl+V**: Paste notes
- **Ctrl+A**: Select all notes
- **ESC**: Clear selection

---

## 2. AutoPlayer System

### Overview
A new `AutoPlayer` class has been created that automatically plays the game by hitting all notes perfectly with the correct truth values.

### Implementation Details
- Located at: `Assets/Scripts/Player/AutoPlayer.cs`
- Mirrors `PlayerInputDetector` but triggers inputs programmatically
- Hits notes at perfect timing window
- Evaluates correct truth values based on active logic gate (or real truth value in simple mode)
- Uses reflection to access active notes from BeatMapManager

### How to Enable
1. **In GameManager Inspector**: Toggle the `Auto Play Enabled` checkbox under "Gameplay Modifiers"
2. This setting is passed to `LivePlayData` when starting a game session

### Behavior When Enabled
- AutoPlayer automatically hits all notes perfectly
- Player input still triggers sound effects but doesn't affect scoring
- All notes are cleared with perfect timing
- Works with both normal mode and simple mode

### Setup Requirements
To use AutoPlayer in your scene:
1. Add the `AutoPlayer` component to a GameObject in your gameplay scene
2. The component will automatically initialize and read settings from `GameManager`

---

## 3. Simple Mode

### Overview
A new gameplay mode that removes logic gate mechanics entirely, making the game more accessible.

### How It Works
- Both top and bottom notes (all spawn locations) display the **same value**
- Notes pass through directly:
  - If the real truth value is 1, both notes show 1, player should hit 1 (up)
  - If the real truth value is 0, both notes show 0, player should hit 0 (down)
- No logic gate evaluation needed

### Visual Behavior
- All logic gate types are enabled for checkpoints
- The scene still cycles through different logic gates for visual variety
- Colors and materials shift normally at checkpoints
- The gate display is purely aesthetic - no logic evaluation occurs

### How to Enable
1. **In GameManager Inspector**: Toggle the `Simple Mode Enabled` checkbox under "Gameplay Modifiers"
2. This setting is passed to `LivePlayData` when starting a game session

### Implementation Changes
- **BeatSplitter**: Modified `GetNoteSpawnIndicesForBeat()` to spawn identical notes in all spawn locations
- **BeatMapManager**: 
  - `OnButtonPressed()` checks real truth value instead of logic evaluation
  - `CycleOperation()` enables all logic operations for visual cycling
- **AutoPlayer**: Handles simple mode by checking real truth value directly

---

## 4. GameManager Updates

### New Inspector Fields
```csharp
[Header("Gameplay Modifiers")]
public bool autoPlayEnabled = false;
public bool simpleModeEnabled = false;
```

### LivePlayData Enhancement
The `LivePlayData` class now includes:
```csharp
public bool autoPlayEnabled = false;
public bool simpleModeEnabled = false;
```

These flags are passed through when creating a new game session via `SetPlayData()`.

---

## 5. PlayerInputDetector Updates

### AutoPlay Integration
- When AutoPlay is enabled, player input still plays sound effects
- However, input events are **not raised** when AutoPlay is active
- This prevents player input from interfering with AutoPlayer's perfect execution
- Player can still "play along" and hear the button sounds

### Code Changes
All input callbacks now check:
```csharp
// Always play sound effect
SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.buttonHit);

// Only raise event if autoplay is not enabled
if (GameManager.Instance?.livePlayData?.autoPlayEnabled != true)
{
    EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(laneIndex, direction));
}
```

---

## 6. Compatibility & Integration

### Mode Combinations
All features work together seamlessly:

| AutoPlay | Simple Mode | Result |
|----------|-------------|--------|
| OFF | OFF | Normal gameplay with logic gates |
| ON | OFF | AutoPlayer hits notes perfectly using logic evaluation |
| OFF | ON | Player plays with pass-through notes (no logic gates) |
| ON | ON | AutoPlayer hits pass-through notes perfectly |

### Backward Compatibility
- All existing functionality is preserved
- Features are opt-in via GameManager toggles
- Default behavior unchanged when both toggles are OFF

---

## 7. Testing Checklist

### BeatMap Editor
- [ ] Copy/paste notes works correctly
- [ ] Truth value assignment works for single and multiple notes
- [ ] Keyboard shortcuts work as expected
- [ ] Inspector displays truth values with correct colors
- [ ] Note colors match their truth values in editor

### AutoPlayer
- [ ] AutoPlayer component added to gameplay scene
- [ ] AutoPlayer toggle works in GameManager
- [ ] All notes are hit perfectly when enabled
- [ ] Player input doesn't affect score when AutoPlayer is active
- [ ] Sound effects still play for player input
- [ ] Works correctly in both normal and simple modes

### Simple Mode
- [ ] Simple mode toggle works in GameManager
- [ ] Both spawn locations show identical notes
- [ ] Notes pass through correctly (no logic evaluation)
- [ ] Logic gates still cycle for visual variety
- [ ] Colors shift correctly at checkpoints
- [ ] Scoring works correctly with pass-through logic

### Integration
- [ ] All mode combinations work as expected
- [ ] No existing functionality broken
- [ ] Game Over and Level Complete still work
- [ ] Score tracking works in all modes

---

## 8. Known Limitations

1. **AutoPlayer Setup**: Must manually add AutoPlayer component to gameplay scene
2. **Selection Dragging**: Visual selection box not implemented (shift-click for multi-select instead)
3. **Editor Warnings**: Some IDE warnings about namespace conventions (non-breaking)

---

## 9. Future Enhancement Ideas

- Visual selection rectangle for dragging over multiple notes
- AutoPlayer timing offset adjustment
- Difficulty scaling for simple mode
- Per-song AutoPlay/SimpleMode settings
- Editor undo/redo functionality
- Note quantization tools

---

## Files Modified

### Core Systems
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/LivePlayData.cs`
- `Assets/Scripts/Player/BeatMapManager.cs`
- `Assets/Scripts/Player/PlayerInputDetector.cs`
- `Assets/Scripts/BeatLogic/BeatSplitter.cs`

### Editor
- `Assets/Scripts/BeatMap/Editor/BeatMapEditorWindow.cs`
- `Assets/Scripts/BeatMap/Editor/BeatMapDataEditor.cs`

### New Files
- `Assets/Scripts/Player/AutoPlayer.cs` *(NEW)*

---

## Quick Start Guide

### For Map Creators
1. Open BeatMap Editor: `Window > BeatMap Editor`
2. Load or create a BeatMap
3. Use Select tool to select multiple notes
4. Press 0, 1, or R to assign truth values in bulk
5. Use Ctrl+C and Ctrl+V to copy/paste sections

### For Testing/Demonstration
1. Open GameManager in inspector
2. Enable "Auto Play Enabled" to watch perfect gameplay
3. Enable "Simple Mode Enabled" for easier gameplay
4. Add AutoPlayer component to gameplay scene GameObject

---

**Created**: February 2, 2026
**Version**: 1.0

