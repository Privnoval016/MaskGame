using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component representing a single song in the selection menu
/// </summary>
public class SongCard : MonoBehaviour
{
    [Header("UI References")]
    public Image coverImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI authorText;
    public Button selectButton;
    
    [Header("Data")]
    public BeatMapData beatMapData;
    
    private SongSelectMenu songSelectMenu;
    
    public void Initialize(BeatMapData data, SongSelectMenu menu)
    {
        beatMapData = data;
        songSelectMenu = menu;
        
        // Ensure CanvasGroup exists for animations
        if (!TryGetComponent<CanvasGroup>(out _))
        {
            gameObject.AddComponent<CanvasGroup>();
        }
        
        // Set UI elements
        if (data.coverArt != null && coverImage != null)
        {
            coverImage.sprite = data.coverArt;
        }
        
        if (titleText != null)
        {
            titleText.text = data.songTitle;
        }
        
        if (authorText != null)
        {
            authorText.text = data.author;
        }
        
        // Setup button
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnCardClicked);
        }
    }
    
    private void OnCardClicked()
    {
        if (songSelectMenu == null) return;
        
        // Play UI select sound
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.uiSelect);
        }
        
        // Get the index of this card
        int cardIndex = songSelectMenu.GetCardIndex(this);
        
        if (cardIndex == -1) return;
        
        // If this card is centered, select it
        if (songSelectMenu.IsCardCentered(cardIndex))
        {
            songSelectMenu.OnSongSelected(this);
        }
        else
        {
            // If not centered, scroll to center it
            songSelectMenu.ScrollToCard(cardIndex);
        }
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public void SetInteractable(bool interactable)
    {
        if (selectButton != null)
        {
            selectButton.interactable = interactable;
        }
    }
}

