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
        if (andToggle != null) andToggle.isOn = true;
        if (orToggle != null) orToggle.isOn = true;
        if (xorToggle != null) xorToggle.isOn = true;
        if (nandToggle != null) nandToggle.isOn = true;
        if (norToggle != null) norToggle.isOn = true;
        if (xnorToggle != null) xnorToggle.isOn = true;
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
        currentPlaySpeed = Mathf.Max(1f, currentPlaySpeed - 0.5f);
        UpdatePlaySpeedDisplay();
    }
    
    private void OnPlaySpeedPlus()
    {
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
    
    private LogicOperation[] GetEnabledOperations()
    {
        List<LogicOperation> enabled = new List<LogicOperation>();
        
        if (andToggle != null && andToggle.isOn) enabled.Add(LogicOperation.And);
        if (orToggle != null && orToggle.isOn) enabled.Add(LogicOperation.Or);
        if (xorToggle != null && xorToggle.isOn) enabled.Add(LogicOperation.Xor);
        if (nandToggle != null && nandToggle.isOn) enabled.Add(LogicOperation.Nand);
        if (norToggle != null && norToggle.isOn) enabled.Add(LogicOperation.Nor);
        if (xnorToggle != null && xnorToggle.isOn) enabled.Add(LogicOperation.Xnor);
        
        // Ensure at least one operation is enabled
        if (enabled.Count == 0)
        {
            enabled.Add(LogicOperation.And); // Default fallback
        }
        
        return enabled.ToArray();
    }
    
    private void OnPlayClicked()
    {
        if (selectedCard == null) return;
        
        // Get settings
        LogicOperation[] enabledOps = GetEnabledOperations();
        
        // Set play data in GameManager
        GameManager.Instance.SetPlayData(selectedCard.beatMapData, currentPlaySpeed, enabledOps);
        
        // Animate button press
        if (playButton != null)
        {
            Tween.PunchScale(playButton.transform, Vector3.one * 0.2f, duration: 0.3f);
        }
        
        // Load game scene
        Debug.Log($"Starting game with: {selectedCard.beatMapData.songTitle}, Speed: {currentPlaySpeed}, Operations: {enabledOps.Length}");
        
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

