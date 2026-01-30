using Extensions.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions.UI
{
    public class EffectIconUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text stackText;
    
        public int NumStacks { get; private set; }
        public StatusEffect EffectType { get; private set; }

        private void Awake()
        {
            NumStacks = 0;
            EffectType = null;
            gameObject.SetActive(true);
            iconImage.enabled = true;
            backgroundImage.enabled = true;
            CanvasGroup cg = gameObject.GetOrAddComponent<CanvasGroup>();
            
            
            UpdateUI();
        }
    
        public void SetEffect(StatusEffect effectType, int numStacks)
        {
            EffectType = effectType;
            NumStacks = numStacks;
            UpdateUI();
        }
    
        public void ClearEffect()
        {
            EffectType = null;
            NumStacks = 0;
            UpdateUI();
        }
    
        private void UpdateUI()
        {
            if (EffectType != null)
            {
                iconImage.sprite = EffectType.icon;
                backgroundImage.color = EffectType.backgroundColor;
                stackText.text = NumStacks > 1 ? NumStacks.ToString() : "";
                gameObject.AlphaFadeToValue(0.1f, 1f);
            }
            else
            {
                gameObject.AlphaFadeToValue(0.1f, 0f);
            }
        }
    }
}