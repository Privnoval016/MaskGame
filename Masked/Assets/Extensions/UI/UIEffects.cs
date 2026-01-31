using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Extensions.UI
{
    public static class UIEffects
    {
        private static Dictionary<GameObject, Vector3> pulseOutInOriginalScales = new Dictionary<GameObject, Vector3>();
        
        /**
         * <summary>
         * Applies a pulsing afterimage effect to the specified UI Element by making a copy of it,
         * scaling it up, and fading it out over the given duration.
         * </summary>
         *
         * <param name="uiElement">The UI GameObject to apply the afterimage effect to.</param>
         * <param name="pulseScale">The scale factor to which the afterimage will pulse.</param>
         * <param name="duration">The duration over which the afterimage will fade out.</param>
         * <param name="startAlpha">The starting alpha value of the afterimage.</param>
         * <param name="easeType">The easing type for the scaling animation (default is Ease.OutCubic).</param>
         *
         * <returns>A Sequence representing the afterimage effect animation.</returns>
         */
        public static Sequence PulseAfterimage(this GameObject uiElement, float pulseScale, float duration, float startAlpha, Ease easeType = Ease.OutCubic)
        {
            if (uiElement == null) return default;

            RectTransform sourceRT = uiElement.GetComponent<RectTransform>();
            if (sourceRT == null) return default;

            // Instantiate as sibling to preserve UI layout
            GameObject afterimage = Object.Instantiate(uiElement, sourceRT.parent, false);

            afterimage.transform.SetSiblingIndex(sourceRT.GetSiblingIndex());

            RectTransform afterRT = afterimage.GetComponent<RectTransform>();

            // Ensure identical layout
            afterRT.anchorMin = sourceRT.anchorMin;
            afterRT.anchorMax = sourceRT.anchorMax;
            afterRT.pivot = sourceRT.pivot;
            afterRT.anchoredPosition = sourceRT.anchoredPosition;
            afterRT.localRotation = sourceRT.localRotation;
            afterRT.localScale = sourceRT.localScale;

            // CanvasGroup for fade
            CanvasGroup cg = afterimage.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = afterimage.AddComponent<CanvasGroup>();

            cg.alpha = startAlpha;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            
            Vector3 baseScale = afterRT.localScale;
            return Sequence.Create()
                .Group(Tween.Scale(afterRT, baseScale * pulseScale, duration, easeType))
                .Group(Tween.Alpha(cg, 0.01f, duration)).ChainCallback(() => Object.Destroy(afterimage));
        }
        
        
        /**
         * <summary>
         * Applies a pulsing scale effect to the specified UI Element by scaling it up and then back down.
         * </summary>
         *
         * <param name="uiElement">The UI GameObject to apply the pulse effect to.</param>
         * <param name="pulseScale">The scale factor to which the UI element will pulse.</param>
         * <param name="durationOut">The duration of the scaling up phase.</param>
         * <param name="durationStay">The duration to stay at the peak scale.</param>
         * <param name="durationIn">The duration of the scaling down phase.</param>
         * <param name="easeOut">The easing type for the scaling up animation (default is Ease.OutCubic).</param>
         * <param name="easeIn">The easing type for the scaling down animation (default is Ease.InCubic).</param>
         *
         * <returns>A Sequence representing the pulse effect animation.</returns>
         */
        public static Sequence PulseOutIn(this GameObject uiElement, float pulseScale, float durationOut, float durationStay, float durationIn, Ease easeOut = Ease.OutCubic, Ease easeIn = Ease.InCubic)
        {
            if (uiElement == null) return default;

            RectTransform rt = uiElement.GetComponent<RectTransform>();
            if (rt == null) return default;
            
            if (pulseOutInOriginalScales.TryGetValue(uiElement, out Vector3 originalScale))
            {
                rt.localScale = originalScale;
            }
            else
            {
                pulseOutInOriginalScales[uiElement] = rt.localScale;
            }

            Vector3 baseScale = rt.localScale;
            Vector3 targetScale = baseScale * pulseScale;

            if (durationStay > 0f)
            {
                return Sequence.Create()
                .Group(Tween.Scale(rt, targetScale, durationOut, easeOut))
                .ChainDelay(durationStay)
                .Chain(Tween.Scale(rt, baseScale, durationIn, easeIn));
            }
            else
            {
                return Sequence.Create()
                .Group(Tween.Scale(rt, targetScale, durationOut, easeOut))
                .Chain(Tween.Scale(rt, baseScale, durationIn, easeIn));
            }
        }
        
        /**
         * <summary>
         * Fades the alpha of the specified UI Element from startAlpha to endAlpha over the given duration.
         * </summary>
         *
         * <param name="uiElement">The UI GameObject to apply the alpha fade effect to.</param>
         * <param name="duration">The duration over which the alpha fade will occur. If duration is zero or negative, the alpha is set instantly.</param>
         * <param name="startAlpha">The starting alpha value.</param>
         * <param name="endAlpha">The ending alpha value.</param>
         * <param name="easeType">The easing type for the alpha animation (default is Ease.Linear).</param>
         *
         * <returns>A Tween representing the alpha fade animation.</returns>
         */
        public static Tween AlphaFade(this GameObject uiElement, float duration, float startAlpha, float endAlpha, Ease easeType = Ease.Linear)
        {
            if (uiElement == null) return default;
            
            if (duration <= 0f)
            {
                CanvasGroup cgInstant = uiElement.GetComponent<CanvasGroup>();
                if (cgInstant == null)
                    cgInstant = uiElement.AddComponent<CanvasGroup>();

                cgInstant.alpha = endAlpha;
                cgInstant.blocksRaycasts = endAlpha > 0f;
                cgInstant.interactable = endAlpha > 0f;

                return default;
            }

            CanvasGroup cg = uiElement.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = uiElement.AddComponent<CanvasGroup>();

            cg.alpha = startAlpha;
            cg.blocksRaycasts = endAlpha > 0f;
            cg.interactable = endAlpha > 0f;

            return Tween.Alpha(cg, endAlpha, duration, easeType);
        }
        
        public static Tween AlphaFadeToValue(this GameObject uiElement, float duration, float targetAlpha, Ease easeType = Ease.Linear)
        {
            if (uiElement == null) return default;

            CanvasGroup cg = uiElement.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = uiElement.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = targetAlpha > 0f;
            cg.interactable = targetAlpha > 0f;
            
            if (Mathf.Approximately(cg.alpha, targetAlpha))
                return default;

            return Tween.Alpha(cg, targetAlpha, duration, easeType);
        }
        
        /**
         * <summary>
         * Crossfades the sprite of a UI Image from its current sprite to a new sprite over the specified duration.
         * </summary>
         *
         * <param name="uiImage">The UI Image to apply the crossfade effect to.</param>
         * <param name="newSprite">The new sprite to crossfade to.</param>
         * <param name="duration">The duration over which the crossfade will occur.</param>
         * <param name="inEase">The easing type for the fade-in animation (default is Ease.OutCubic).</param>
         * <param name="outEase">The easing type for the fade-out animation (default is Ease.InCubic).</param>
         *
         * <returns>A Sequence representing the crossfade animation.</returns>
         */
        public static Sequence TextureCrossFade(this Image uiImage, Sprite newSprite, float duration, Ease inEase = Ease.OutCubic, Ease outEase = Ease.InCubic)
        {
            if (uiImage == null || newSprite == null) return default;

            // Create temporary Image for crossFade
            GameObject tempGO = new GameObject("TempImage");
            tempGO.transform.SetParent(uiImage.transform.parent, false);
            Image tempImage = tempGO.AddComponent<Image>();
            tempImage.sprite = uiImage.sprite;
            tempImage.rectTransform.anchorMin = uiImage.rectTransform.anchorMin;
            tempImage.rectTransform.anchorMax = uiImage.rectTransform.anchorMax;
            tempImage.rectTransform.pivot = uiImage.rectTransform.pivot;
            tempImage.rectTransform.anchoredPosition = uiImage.rectTransform.anchoredPosition;
            tempImage.rectTransform.sizeDelta = uiImage.rectTransform.sizeDelta;
            tempImage.rectTransform.localRotation = uiImage.rectTransform.localRotation;
            tempImage.rectTransform.localScale = uiImage.rectTransform.localScale;

            // Set new sprite to original image
            uiImage.sprite = newSprite;

            // Cross fade
            return Sequence.Create()
                .Group(Tween.Alpha(tempImage, 0f, duration, outEase))
                .Group(Tween.Alpha(uiImage, 1f, duration, inEase))
                .ChainCallback(() => Object.Destroy(tempGO));
        }
        
        
    }
}