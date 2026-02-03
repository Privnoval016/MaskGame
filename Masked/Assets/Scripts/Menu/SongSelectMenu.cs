using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using System.Collections.Generic;
using System.Linq;

public class SongSelectMenu : MonoBehaviour
{
    [Header("UI References")]
    public Transform songCardsContainer; // Parent for song cards
    public GameObject songCardPrefab; // Prefab for song card
    public Button playButton;
    public Button backButton; // Button to return to song scrolling
    public GameObject settingsPanel; // Panel shown when a song is selected
    public Button playSpeedMinusButton;
    public Button playSpeedPlusButton;
    public TextMeshProUGUI playSpeedText;
    
    [Header("Operation Toggles")]
    public Toggle andToggle;
    public Toggle orToggle;
    public Toggle xorToggle;
    public Toggle nandToggle;
    public Toggle norToggle;
    public Toggle xnorToggle;
    
    [Header("Difficulty Selection")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button expertButton;
    public Button superExpertButton;
    public RectTransform difficultySelector; // The image that tweens to selected difficulty button
    
    [Header("Game Modifiers")]
    public Button autoplayButton; // Toggle button for autoplay
    public Toggle simpleModeToggle; // Toggle for simple mode
    
    [Header("Layout Settings")]
    public float cardSpacing = 400f; // Distance between cards
    public float swipeThreshold = 50f; // Minimum swipe distance
    public float dragSensitivity = 1f;
    public float selectedCardOffsetX = -200f; // How far left the selected card moves
    public float velocityThreshold = 500f; // Minimum swipe velocity for quick swipes
    
    [Header("Visual Settings")]
    public float nonCenterCardFadeAmount = 0.5f; // How much to fade cards that aren't centered (0-1)
    public float centeredCardScale = 1.15f; // Scale of the centered card (larger = more prominent)
    public float selectedCardScale = 1.15f; // Scale multiplier for selected card (should match centeredCardScale)
    
    [Header("Animation Settings")]
    public float transitionDuration = 0.5f; // Faster for smoother feel
    public Ease transitionEase = Ease.OutQuart; // Smoother easing curve
    public float settingsDelayMultiplier = 0.5f; // Delay before showing settings as fraction of transition duration
    public float settingsAnimDuration = 0.5f; // Duration for settings panel animations
    public float playButtonDelaySeconds = 0.3f; // Delay before loading scene after play click
    
    private List<SongCard> songCards = new List<SongCard>();
    private int currentIndex = 0;
    private bool isSelected = false;
    private SongCard selectedCard;
    private float currentPlaySpeed = 5f; // Current play speed value (1-10, increments of 0.5)
    private Difficulty selectedDifficulty = Difficulty.Medium; // Currently selected difficulty
    private bool autoplayEnabled = false; // Autoplay mode state
    
    // Drag/swipe handling
    private Vector3 dragStartPos;
    private Vector3 cardsStartPos;
    private bool isDragging = false;
    private bool hasDragged = false; // Track if user actually dragged (vs just clicked)
    private float dragStartTime;
    private const float dragThreshold = 10f; // Minimum distance to consider it a drag
    
    private void Start()
    {
        InitializeSongs();
        SetupUI();
        UpdateCardPositions(false);
    }
    
    
    private void OnEnable()
    {
        // Ensure time scale is normal (in case we're returning from game over)
        Time.timeScale = 1f;
        
        // Wait a frame to ensure scene is fully loaded
        StartCoroutine(ResetMenuStateNextFrame());
    }
    
    private System.Collections.IEnumerator ResetMenuStateNextFrame()
    {
        yield return null; // Wait one frame
        
        // Reset state when menu is enabled (e.g., returning from game scene)
        isSelected = false;
        selectedCard = null;
        
        // Ensure play button is hidden
        if (playButton != null)
        {
            playButton.gameObject.SetActive(false);
        }
        
        // Ensure back button is hidden
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }
        
        // Ensure settings panel is hidden
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Reset all cards to visible and proper scale
        foreach (var card in songCards)
        {
            if (card != null)
            {
                if (card.TryGetComponent<CanvasGroup>(out var canvasGroup))
                {
                    canvasGroup.alpha = 1f;
                }
                card.transform.localScale = Vector3.one;
                card.SetInteractable(true);
            }
        }
        
        // Update card positions to current index
        if (songCards.Count > 0)
        {
            UpdateCardPositions(false);
        }
        
        // Reposition difficulty selector after menu is re-enabled
        UpdateDifficultySelector(false);
        
        Debug.Log("Menu state reset complete!");
    }
    
    private void OnDisable()
    {
        // Stop all tweens when menu is disabled/destroyed to prevent accessing destroyed objects
        Tween.StopAll(songCardsContainer);
        
        foreach (var card in songCards)
        {
            if (card != null)
            {
                Tween.StopAll(card.transform);
                if (card.TryGetComponent<CanvasGroup>(out var canvasGroup))
                {
                    Tween.StopAll(canvasGroup);
                }
            }
        }
    }
    
    private void InitializeSongs()
    {
        if (GameManager.Instance == null || GameManager.Instance.availableSongs == null)
        {
            Debug.LogError("GameManager or available songs not configured!");
            return;
        }
        
        // Create song cards
        foreach (var beatMap in GameManager.Instance.availableSongs)
        {
            if (beatMap == null) continue;
            
            GameObject cardObj = Instantiate(songCardPrefab, songCardsContainer);
            SongCard card = cardObj.GetComponent<SongCard>();
            
            if (card != null)
            {
                card.Initialize(beatMap, this);
                songCards.Add(card);
            }
        }
        
        // Handle edge case: no songs
        if (songCards.Count == 0)
        {
            Debug.LogWarning("No songs available!");
            if (playButton != null)
            {
                playButton.gameObject.SetActive(false);
            }
        }
    }
    
    private void SetupUI()
    {
        // Setup play button
        if (playButton != null)
        {
            playButton.gameObject.SetActive(false);
            playButton.onClick.AddListener(OnPlayClicked);
            
            // Ensure CanvasGroup is properly configured if it exists
            if (playButton.TryGetComponent<CanvasGroup>(out var buttonCanvas))
            {
                buttonCanvas.alpha = 1f;
                buttonCanvas.interactable = true;
                buttonCanvas.blocksRaycasts = true;
            }
        }
        
        // Setup back button
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(OnBackPressed);
        }
        
        // Setup settings panel
        if (settingsPanel != null)
        {
            // Add CanvasGroup if it doesn't exist
            if (!settingsPanel.TryGetComponent<CanvasGroup>(out var panelCanvas))
            {
                panelCanvas = settingsPanel.AddComponent<CanvasGroup>();
            }
            
            // Ensure CanvasGroup is properly configured
            panelCanvas.alpha = 1f;
            panelCanvas.interactable = true;
            panelCanvas.blocksRaycasts = true;
            
            settingsPanel.SetActive(false);
        }
        
        // Setup play speed buttons
        if (playSpeedMinusButton != null)
        {
            playSpeedMinusButton.onClick.AddListener(OnPlaySpeedMinus);
        }
        
        if (playSpeedPlusButton != null)
        {
            playSpeedPlusButton.onClick.AddListener(OnPlaySpeedPlus);
        }
        
        // Initialize play speed display
        UpdatePlaySpeedDisplay();
        
        // Setup operation toggles - default all enabled
        if (andToggle != null)
        {
            andToggle.isOn = true;
            andToggle.onValueChanged.AddListener((value) => OnOperationToggleChanged(andToggle));
        }
        if (orToggle != null)
        {
            orToggle.isOn = true;
            orToggle.onValueChanged.AddListener((value) => OnOperationToggleChanged(orToggle));
        }
        if (xorToggle != null)
        {
            xorToggle.isOn = true;
            xorToggle.onValueChanged.AddListener((value) => OnOperationToggleChanged(xorToggle));
        }
        if (nandToggle != null)
        {
            nandToggle.isOn = true;
            nandToggle.onValueChanged.AddListener((value) => OnOperationToggleChanged(nandToggle));
        }
        if (norToggle != null)
        {
            norToggle.isOn = true;
            norToggle.onValueChanged.AddListener((value) => OnOperationToggleChanged(norToggle));
        }
        if (xnorToggle != null)
        {
            xnorToggle.isOn = true;
            xnorToggle.onValueChanged.AddListener((value) => OnOperationToggleChanged(xnorToggle));
        }
        
        // Setup difficulty buttons
        if (easyButton != null)
            easyButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Easy));
        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Medium));
        if (hardButton != null)
            hardButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Hard));
        if (expertButton != null)
            expertButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Expert));
        if (superExpertButton != null)
            superExpertButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.SuperExpert));
        
        // Setup autoplay button
        if (autoplayButton != null)
        {
            autoplayButton.onClick.AddListener(OnAutoplayToggled);
            UpdateAutoplayButtonVisual();
        }
        
        // Setup simple mode toggle
        if (simpleModeToggle != null)
        {
            simpleModeToggle.isOn = false;
            simpleModeToggle.onValueChanged.AddListener(OnSimpleModeToggled);
        }
    }
    
    private void Update()
    {
        if (isSelected || songCards.Count <= 1) return;
        
        HandleDragInput();
    }
    
    private void HandleDragInput()
    {
        // Mouse/touch input handling
        bool inputDown = Input.GetMouseButtonDown(0);
        bool inputHeld = Input.GetMouseButton(0);
        bool inputUp = Input.GetMouseButtonUp(0);
        Vector3 inputPos = Input.mousePosition;
        
        if (inputDown)
        {
            dragStartPos = inputPos;
            cardsStartPos = songCardsContainer.position;
            isDragging = true;
            hasDragged = false;
            dragStartTime = Time.time;
            
            // Stop any ongoing tweens
            Tween.StopAll(songCardsContainer);
        }
        else if (inputHeld && isDragging)
        {
            float dragDistance = Vector3.Distance(inputPos, dragStartPos);
            
            // Mark as dragged if moved beyond threshold
            if (dragDistance > dragThreshold)
            {
                hasDragged = true;
            }
            
            // Drag cards
            float dragDelta = (inputPos.x - dragStartPos.x) * dragSensitivity;
            
            // Clamp drag to prevent scrolling too far past ends
            float maxDragLeft = currentIndex * cardSpacing;
            float maxDragRight = (songCards.Count - 1 - currentIndex) * cardSpacing;
            
            // Allow one extra card spacing beyond the ends for elastic feel
            float clampedDelta = Mathf.Clamp(dragDelta, -maxDragLeft - cardSpacing, maxDragRight + cardSpacing);
            
            songCardsContainer.position = cardsStartPos + new Vector3(clampedDelta, 0, 0);
        }
        else if (inputUp && isDragging)
        {
            isDragging = false;
            
            // Only process swipe if user actually dragged
            if (hasDragged)
            {
                float dragDelta = inputPos.x - dragStartPos.x;
                float dragTime = Time.time - dragStartTime;
                float dragVelocity = dragDelta / Mathf.Max(dragTime, 0.01f);
                
                // Determine if swipe was significant enough
                if (Mathf.Abs(dragDelta) > swipeThreshold || Mathf.Abs(dragVelocity) > velocityThreshold)
                {
                    if (dragDelta > 0 && currentIndex > 0)
                    {
                        // Swipe right - go to previous song
                        currentIndex--;
                    }
                    else if (dragDelta < 0 && currentIndex < songCards.Count - 1)
                    {
                        // Swipe left - go to next song
                        currentIndex++;
                    }
                }
            }
            
            UpdateCardPositions(true);
            hasDragged = false;
        }
    }
    
    private void UpdateCardPositions(bool animate)
    {
        if (songCards.Count == 0) return;
        
        float centerX = 0f; // Center position
        
        for (int i = 0; i < songCards.Count; i++)
        {
            float targetX = centerX + (i - currentIndex) * cardSpacing;
            Vector3 targetPos = new Vector3(targetX, songCards[i].transform.localPosition.y, 0);
            
            // Scale centered card
            bool isCentered = i == currentIndex;
            float targetScale = isCentered ? centeredCardScale : 1f;
            
            if (animate)
            {
                Tween.LocalPosition(songCards[i].transform, targetPos, transitionDuration, transitionEase);
                Tween.Scale(songCards[i].transform, targetScale, transitionDuration, transitionEase);
                
                // Fade out cards that are too far
                float alpha = Mathf.Clamp01(1f - Mathf.Abs(i - currentIndex) * nonCenterCardFadeAmount);
                if (songCards[i].TryGetComponent<CanvasGroup>(out var canvasGroup))
                {
                    Tween.Alpha(canvasGroup, alpha, transitionDuration, transitionEase);
                }
            }
            else
            {
                songCards[i].transform.localPosition = targetPos;
                songCards[i].transform.localScale = Vector3.one * targetScale;
            }
            
            // Make only center card interactable for selection, but ALL cards clickable for centering
            songCards[i].SetInteractable(true);
        }
        
        // Reset container position
        if (animate)
        {
            Tween.LocalPosition(songCardsContainer, Vector3.zero, transitionDuration, transitionEase);
        }
        else
        {
            songCardsContainer.localPosition = Vector3.zero;
        }
    }
    
    public void OnSongSelected(SongCard card)
    {
        if (isSelected || card != songCards[currentIndex]) return;
        
        // Don't allow selection if user just dragged
        if (hasDragged) return;
        
        isSelected = true;
        selectedCard = card;
        
        // Animate: move selected card left, hide others, show play button and settings
        AnimateToSelectedState();
    }
    
    /// <summary>
    /// Check if a card at the given index is currently centered
    /// </summary>
    public bool IsCardCentered(int cardIndex)
    {
        return cardIndex == currentIndex;
    }
    
    /// <summary>
    /// Get the index of a specific card in the list
    /// </summary>
    public int GetCardIndex(SongCard card)
    {
        return songCards.IndexOf(card);
    }
    
    /// <summary>
    /// Scroll to center a specific card
    /// </summary>
    public void ScrollToCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= songCards.Count) return;
        
        currentIndex = cardIndex;
        UpdateCardPositions(true);
    }
    
    private void AnimateToSelectedState()
    {
        // Check if current difficulty is playable for this song, if not switch to first available
        if (selectedCard != null && selectedCard.beatMapData != null)
        {
            if (!selectedCard.beatMapData.IsDifficultyPlayable(selectedDifficulty))
            {
                // Find first playable difficulty
                Difficulty firstPlayable = FindFirstPlayableDifficulty(selectedCard.beatMapData);
                selectedDifficulty = firstPlayable;
                Debug.Log($"Current difficulty not available, switched to {firstPlayable}");
            }
        }
        
        // Update difficulty button states based on what's playable for this song
        UpdateDifficultyButtonStates();
        
        // Update difficulty selector to show correct difficulty (without animation since panel is appearing)
        UpdateDifficultySelector(false);
        
        // Move selected card to the left by the configured offset (keep its current scale)
        Vector3 selectedTargetPos = new Vector3(selectedCardOffsetX, selectedCard.transform.localPosition.y, 0);
        Tween.LocalPosition(selectedCard.transform, selectedTargetPos, transitionDuration, transitionEase);
        
        // Keep the selected card at its centered scale (don't scale it again)
        // It should already be at centeredCardScale from UpdateCardPositions
        
        // Hide all other cards
        foreach (var card in songCards)
        {
            if (card != selectedCard)
            {
                if (card.TryGetComponent<CanvasGroup>(out var canvasGroup))
                {
                    Tween.Alpha(canvasGroup, 0f, transitionDuration, transitionEase);
                }
                card.SetInteractable(false);
            }
        }
        
        // Show play button and settings panel with delay - fade in only, no scaling or moving
        Tween.Delay(transitionDuration * settingsDelayMultiplier).OnComplete(() =>
        {
            if (playButton != null)
            {
                playButton.gameObject.SetActive(true);
                // Fade in the button if it has a CanvasGroup
                if (playButton.TryGetComponent<CanvasGroup>(out var buttonCanvas))
                {
                    buttonCanvas.interactable = true;
                    buttonCanvas.blocksRaycasts = true;
                    buttonCanvas.alpha = 0f;
                    Tween.Alpha(buttonCanvas, 1f, settingsAnimDuration, Ease.OutCubic);
                }
            }
            
            if (backButton != null)
            {
                backButton.gameObject.SetActive(true);
                // Fade in the back button if it has a CanvasGroup
                if (backButton.TryGetComponent<CanvasGroup>(out var backButtonCanvas))
                {
                    backButtonCanvas.interactable = true;
                    backButtonCanvas.blocksRaycasts = true;
                    backButtonCanvas.alpha = 0f;
                    Tween.Alpha(backButtonCanvas, 1f, settingsAnimDuration, Ease.OutCubic);
                }
            }
            
            if (settingsPanel != null)
            {
                // Ensure CanvasGroup exists and is properly configured
                if (!settingsPanel.TryGetComponent<CanvasGroup>(out var panelCanvas))
                {
                    panelCanvas = settingsPanel.AddComponent<CanvasGroup>();
                }
                
                settingsPanel.SetActive(true);
                
                // Set properties to ensure interaction works
                panelCanvas.alpha = 0f;
                panelCanvas.interactable = true;
                panelCanvas.blocksRaycasts = true;
                
                // Fade in the panel
                Tween.Alpha(panelCanvas, 1f, settingsAnimDuration, Ease.OutCubic);
            }
        });
    }
    
    private void OnPlaySpeedMinus()
    {
        // Play UI select sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.uiSelect);
        }
        
        currentPlaySpeed = Mathf.Max(1f, currentPlaySpeed - 0.5f);
        UpdatePlaySpeedDisplay();
    }
    
    private void OnPlaySpeedPlus()
    {
        // Play UI select sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.uiSelect);
        }
        
        currentPlaySpeed = Mathf.Min(10f, currentPlaySpeed + 0.5f);
        UpdatePlaySpeedDisplay();
    }
    
    private void UpdatePlaySpeedDisplay()
    {
        if (playSpeedText != null)
        {
            playSpeedText.text = $"{currentPlaySpeed:F1}x";
        }
    }
    
    /// <summary>
    /// Validates that at least one operation is always selected
    /// </summary>
    private void OnOperationToggleChanged(Toggle changedToggle)
    {
        // Count how many toggles are currently on
        int enabledCount = 0;
        if (andToggle != null && andToggle.isOn) enabledCount++;
        if (orToggle != null && orToggle.isOn) enabledCount++;
        if (xorToggle != null && xorToggle.isOn) enabledCount++;
        if (nandToggle != null && nandToggle.isOn) enabledCount++;
        if (norToggle != null && norToggle.isOn) enabledCount++;
        if (xnorToggle != null && xnorToggle.isOn) enabledCount++;
        
        // If user tried to turn off the last operation, force it back on
        if (enabledCount == 0)
        {
            changedToggle.isOn = true;
            Debug.Log("At least one operation must be selected!");
        }
    }
    
    private void OnDifficultySelected(Difficulty difficulty)
    {
        // Don't allow selecting unplayable difficulties
        if (selectedCard != null && selectedCard.beatMapData != null)
        {
            if (!selectedCard.beatMapData.IsDifficultyPlayable(difficulty))
            {
                Debug.Log($"Difficulty {difficulty} is not playable for this song");
                return;
            }
        }
        
        // Play UI select sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.uiSelect);
        }
        
        selectedDifficulty = difficulty;
        UpdateDifficultySelector(true);
        Debug.Log($"Difficulty selected: {difficulty}");
    }
    
    private void UpdateDifficultyButtonStates()
    {
        if (selectedCard == null || selectedCard.beatMapData == null) return;
        
        UpdateDifficultyButton(easyButton, Difficulty.Easy);
        UpdateDifficultyButton(mediumButton, Difficulty.Medium);
        UpdateDifficultyButton(hardButton, Difficulty.Hard);
        UpdateDifficultyButton(expertButton, Difficulty.Expert);
        UpdateDifficultyButton(superExpertButton, Difficulty.SuperExpert);
    }
    
    private void UpdateDifficultyButton(Button button, Difficulty difficulty)
    {
        if (button == null || selectedCard == null || selectedCard.beatMapData == null) return;
        
        bool isPlayable = selectedCard.beatMapData.IsDifficultyPlayable(difficulty);
        button.interactable = isPlayable;
        
        // Gray out text for unplayable difficulties
        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = isPlayable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }
    
    private Difficulty FindFirstPlayableDifficulty(BeatMapData beatMapData)
    {
        if (beatMapData == null) return Difficulty.Medium; // Default fallback
        
        // Check difficulties in order: Easy, Medium, Hard, Expert, SuperExpert
        if (beatMapData.IsDifficultyPlayable(Difficulty.Easy)) return Difficulty.Easy;
        if (beatMapData.IsDifficultyPlayable(Difficulty.Medium)) return Difficulty.Medium;
        if (beatMapData.IsDifficultyPlayable(Difficulty.Hard)) return Difficulty.Hard;
        if (beatMapData.IsDifficultyPlayable(Difficulty.Expert)) return Difficulty.Expert;
        if (beatMapData.IsDifficultyPlayable(Difficulty.SuperExpert)) return Difficulty.SuperExpert;
        
        // If none are playable (shouldn't happen), default to Medium
        Debug.LogWarning($"No playable difficulties found for {beatMapData.songTitle}, defaulting to Medium");
        return Difficulty.Medium;
    }
    
    private void UpdateDifficultySelector(bool animate)
    {
        if (difficultySelector == null)
        {
            Debug.LogWarning("Difficulty selector is null!");
            return;
        }
        
        Button targetButton = null;
        
        switch (selectedDifficulty)
        {
            case Difficulty.Easy: targetButton = easyButton; break;
            case Difficulty.Medium: targetButton = mediumButton; break;
            case Difficulty.Hard: targetButton = hardButton; break;
            case Difficulty.Expert: targetButton = expertButton; break;
            case Difficulty.SuperExpert: targetButton = superExpertButton; break;
        }
        
        if (targetButton != null)
        {
            // Check if selector and button share the same parent (for local positioning)
            if (difficultySelector.parent == targetButton.transform.parent)
            {
                // Use local position if they're siblings
                Vector3 targetPos = targetButton.transform.localPosition;
                
                if (animate)
                {
                    Tween.LocalPosition(difficultySelector, targetPos, 0.3f, Ease.OutBack);
                }
                else
                {
                    difficultySelector.localPosition = targetPos;
                }
            }
            else
            {
                // Use world position if different parents
                Vector3 targetPos = targetButton.transform.position;
                
                if (animate)
                {
                    Tween.Position(difficultySelector, targetPos, 0.3f, Ease.OutBack);
                }
                else
                {
                    Debug.Log("Moving to position: " + targetPos);
                    //difficultySelector.position = targetPos;
                }
            }
        }
        else
        {
            Debug.LogWarning($"Target button for difficulty {selectedDifficulty} is null!");
        }
    }
    
    private void OnAutoplayToggled()
    {
        // Play UI select sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.uiSelect);
        }
        
        autoplayEnabled = !autoplayEnabled;
        UpdateAutoplayButtonVisual();
        Debug.Log($"Autoparser: {(autoplayEnabled ? "ON" : "OFF")}");
    }
    
    private void UpdateAutoplayButtonVisual()
    {
        if (autoplayButton == null) return;
        
        // Update button text or color to show state
        var buttonText = autoplayButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = autoplayEnabled ? "AUTO-PARSER:\nON" : "AUTO-PARSER:\nOFF";
        }
        
        // Optional: Change button color
        var buttonImage = autoplayButton.GetComponent<UnityEngine.UI.Image>();
        if (buttonImage != null)
        {
            buttonImage.color = autoplayEnabled ? new Color(0.2f, 1f, 0.3f, 0.5f) : Color.white;
        }
    }
    
    private void OnSimpleModeToggled(bool isOn)
    {
        // Play UI select sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.uiSelect);
        }
        
        // When simple mode is on, disable logic operation toggles
        bool togglesInteractable = !isOn;
        
        if (andToggle != null) andToggle.interactable = togglesInteractable;
        if (orToggle != null) orToggle.interactable = togglesInteractable;
        if (xorToggle != null) xorToggle.interactable = togglesInteractable;
        if (nandToggle != null) nandToggle.interactable = togglesInteractable;
        if (norToggle != null) norToggle.interactable = togglesInteractable;
        if (xnorToggle != null) xnorToggle.interactable = togglesInteractable;
        
        Debug.Log($"Simple Mode: {(isOn ? "ON" : "OFF")} - Logic toggles {(togglesInteractable ? "enabled" : "disabled")}");
    }
    
    private LogicOperation[] GetEnabledOperations()
    {
        List<LogicOperation> enabled = new List<LogicOperation>();
        
        if (andToggle != null && andToggle.isOn) enabled.Add(LogicOperation.And);
        if (orToggle != null && orToggle.isOn) enabled.Add(LogicOperation.Or);
        if (xorToggle != null && xorToggle.isOn) enabled.Add(LogicOperation.Xor);
        if (nandToggle != null && nandToggle.isOn) enabled.Add(LogicOperation.Nand);
        if (norToggle != null && norToggle.isOn) enabled.Add(LogicOperation.Nor);
        if (xnorToggle != null && xnorToggle.isOn) enabled.Add(LogicOperation.Xnor);
        
        // This should never be 0 due to OnOperationToggleChanged validation
        return enabled.ToArray();
    }
    
    private void OnPlayClicked()
    {
        if (selectedCard == null) return;
        
        // Play song begin sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.songBegin);
        }
        
        // Get settings
        LogicOperation[] enabledOps = GetEnabledOperations();
        bool simpleMode = simpleModeToggle != null && simpleModeToggle.isOn;
        
        // Set game modifiers in GameManager
        GameManager.Instance.autoPlayEnabled = autoplayEnabled;
        GameManager.Instance.simpleModeEnabled = simpleMode;
        
        // Set play data in GameManager with difficulty
        GameManager.Instance.SetPlayData(selectedCard.beatMapData, selectedDifficulty, currentPlaySpeed, enabledOps);
        
        // Animate button press
        if (playButton != null)
        {
            Tween.PunchScale(playButton.transform, Vector3.one * 0.2f, duration: 0.3f);
        }
        
        // Load game scene
        Debug.Log($"Starting game with: {selectedCard.beatMapData.songTitle}, Difficulty: {selectedDifficulty}, Speed: {currentPlaySpeed}, Operations: {enabledOps.Length}, Autoplay: {autoplayEnabled}, SimpleMode: {simpleMode}");
        
        // Delay to allow button animation, then load scene
        Tween.Delay(playButtonDelaySeconds).OnComplete(() =>
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadGameScene();
            }
            else
            {
                Debug.LogError("SceneLoader not found! Make sure SceneLoader is in the scene.");
            }
        });
    }
    
    public void OnBackPressed()
    {
        if (!isSelected) return;
        
        // Play UI select sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.uiSelect);
        }
        
        isSelected = false;
        
        // Fade out play button
        if (playButton != null)
        {
            if (playButton.TryGetComponent<CanvasGroup>(out var buttonCanvas))
            {
                buttonCanvas.interactable = false;
                buttonCanvas.blocksRaycasts = false;
                Tween.Alpha(buttonCanvas, 0f, settingsAnimDuration * 0.5f, transitionEase)
                    .OnComplete(() => playButton.gameObject.SetActive(false));
            }
            else
            {
                playButton.gameObject.SetActive(false);
            }
        }
        
        // Fade out back button
        if (backButton != null)
        {
            if (backButton.TryGetComponent<CanvasGroup>(out var backButtonCanvas))
            {
                backButtonCanvas.interactable = false;
                backButtonCanvas.blocksRaycasts = false;
                Tween.Alpha(backButtonCanvas, 0f, settingsAnimDuration * 0.5f, transitionEase)
                    .OnComplete(() => backButton.gameObject.SetActive(false));
            }
            else
            {
                backButton.gameObject.SetActive(false);
            }
        }
        
        // Fade out settings panel
        if (settingsPanel != null)
        {
            if (settingsPanel.TryGetComponent<CanvasGroup>(out var panelCanvas))
            {
                panelCanvas.interactable = false;
                panelCanvas.blocksRaycasts = false;
                Tween.Alpha(panelCanvas, 0f, settingsAnimDuration * 0.5f, transitionEase)
                    .OnComplete(() => settingsPanel.SetActive(false));
            }
            else
            {
                settingsPanel.SetActive(false);
            }
        }
        
        // Reset selected card scale to normal (will be scaled to centeredCardScale in UpdateCardPositions)
        if (selectedCard != null)
        {
            // Reset to 1.0 initially, then UpdateCardPositions will scale it to centeredCardScale
            Tween.Scale(selectedCard.transform, 1f, transitionDuration * 0.5f, transitionEase);
        }
        
        // Show all cards again
        foreach (var card in songCards)
        {
            if (card.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                Tween.Alpha(canvasGroup, 1f, transitionDuration, transitionEase);
            }
        }
        
        selectedCard = null;
        
        // Reset positions
        Tween.Delay(transitionDuration * 0.3f).OnComplete(() => UpdateCardPositions(true));
    }
}
