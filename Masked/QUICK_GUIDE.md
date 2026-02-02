# Quick Setup & Usage Guide

## 🎮 Setup Instructions

### 1. Enable AutoPlayer (Optional)
1. Find the **GameManager** GameObject in your scene hierarchy
2. In the Inspector, locate "Gameplay Modifiers" section
3. Check the **Auto Play Enabled** checkbox
4. **Important**: Add the `AutoPlayer` component to a GameObject in your gameplay scene
   - Suggested: Add it to the same GameObject as BeatMapManager or PlayerInputDetector

### 2. Enable Simple Mode (Optional)
1. In the same **GameManager** Inspector
2. Check the **Simple Mode Enabled** checkbox
3. No additional setup required

---

## 📝 BeatMap Editor - New Features

### Keyboard Shortcuts
| Key | Action |
|-----|--------|
| **0** | Set selected notes to False (0) |
| **1** | Set selected notes to True (1) |
| **R** | Set selected notes to Random |
| **Ctrl+C** | Copy selected notes |
| **Ctrl+V** | Paste notes at current beat |
| **Ctrl+A** | Select all notes |
| **ESC** | Clear selection |
| **Delete/Backspace** | Delete selected notes |

### Workflow
1. Open Editor: `Window > BeatMap Editor`
2. Load your BeatMap
3. Switch to **Select** tool (top toolbar)
4. Click notes to select them (hold Shift for multi-select)
5. Use number keys or buttons to assign truth values
6. Copy/paste to duplicate sections

### Note Colors
- 🔴 **Red** = False (0)
- 🟢 **Green** = True (1)  
- 🔵 **Blue** = Random
- 🟡 **Yellow** = Selected

---

## 🎯 What Each Mode Does

### Normal Mode (Both OFF)
- Standard gameplay with logic gates (AND, OR, XOR, etc.)
- Player must evaluate logic gates to determine correct input
- Checkpoint switches between enabled logic gates

### AutoPlay Mode (Auto Play ON)
- Computer plays perfectly
- Player can still press buttons to hear sound effects
- Useful for testing maps or demonstrations

### Simple Mode (Simple Mode ON)
- Removes logic gate mechanics
- Both top and bottom notes show the same value
- Player just hits the value shown (no logic evaluation)
- All gates cycle for visual variety only

### AutoPlay + Simple Mode (Both ON)
- Computer plays simple mode perfectly
- Great for testing simple mode functionality

---

## ⚠️ Important Notes

1. **AutoPlayer requires manual setup**: Add the AutoPlayer component to a GameObject in your gameplay scene
2. **Toggles affect new games**: Change settings before calling `GameManager.SetPlayData()`
3. **Player input during AutoPlay**: Sound effects still play but input is ignored for scoring
4. **Simple Mode visuals**: Colors still change but logic gates don't affect gameplay

---

## 🐛 Troubleshooting

**AutoPlayer not working?**
- Make sure AutoPlayer component is added to a GameObject in the scene
- Check that "Auto Play Enabled" is checked in GameManager
- Verify BeatMapManager and ScoreManager are present in scene

**Copy/paste not working in editor?**
- Make sure you're in Select tool mode
- Selected notes show in yellow
- Ctrl key works on both Windows and Mac

**Simple mode still using logic?**
- Check "Simple Mode Enabled" is ON in GameManager
- Verify LivePlayData is created with the correct settings
- Restart the gameplay scene

---

## 📞 Feature Summary

✅ Copy/paste notes in editor  
✅ Batch truth value assignment  
✅ Color-coded notes by truth value  
✅ Truth values in inspector panel  
✅ AutoPlayer for perfect gameplay  
✅ Simple mode for accessibility  
✅ Player input sound effects during AutoPlay  
✅ All features work together seamlessly  

---

**Need more details?** See `FEATURE_UPDATES.md` for complete documentation.

