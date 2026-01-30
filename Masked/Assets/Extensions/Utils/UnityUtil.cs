using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor.U2D;
#endif

namespace Extensions.Utils
{
    public static class UnityUtil
    {
        #region Transforms

        public static void SetRotation(this Transform transform, float rotation)
        {
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        public static void SetLocalRotation(this Transform transform, float rotation)
        {
            transform.localEulerAngles = new Vector3(0, 0, rotation);
        }

        #endregion

        #region Components

        public static void RemoveComponent<T>(this GameObject gameObject)
            where T : Component
        {
            Object.Destroy(gameObject.GetComponent<T>());
        }

        public static T TryAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component)
                return component;
            return gameObject.AddComponent<T>();
        }

        public static T GetChildComponent<T>(this Transform transform, int childIndex)
        {
            return transform.GetChild(childIndex).GetComponent<T>();
        }

        public static RectTransform GetRect(this Component component)
        {
            return component.GetComponent<RectTransform>();
        }

        public static RectTransform GetRect(this GameObject gameObject)
        {
            return gameObject.GetComponent<RectTransform>();
        }

        public static Transform DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform)
                Object.Destroy(child.gameObject);

            return transform;
        }

        #endregion

        #region Children

        public static GameObject AddChild(this Transform transform)
        {
            GameObject child = new GameObject();
            child.transform.SetParent(transform);
            child.transform.localScale = Vector3.one;
            child.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            return child;
        }

        public static GameObject AddChild(this Transform transform, String name)
        {
            GameObject child = transform.AddChild();
            child.name = name;
            return child;
        }

        #endregion

        #region UI

        public static Button ClearSettings(this Button button)
        {
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            button.transition = Selectable.Transition.None;
            return button;
        }

        public static void AddClickListener(this Button button, UnityEngine.Events.UnityAction call)
        {
            button.onClick.AddListener(call);
        }

        public static void SetClickListener(this Button button, UnityEngine.Events.UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }

        public static void Reset(this ScrollRect scrollRect)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1;
        }

        public static List<RaycastResult> UIRaycast(Vector2 position)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = position;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results;
        }

        public static void Enable(this CanvasGroup canvasGroup, bool changeAlpha = true)
        {
            if (changeAlpha)
                canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public static void Disable(this CanvasGroup canvasGroup, bool changeAlpha = true)
        {
            if (changeAlpha)
                canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        #endregion

        #region UI Positioning

        public static Vector2 GetPosition(this RectTransform rect, float scale)
        {
            return ((Vector2)rect.position / scale)
                   + (new Vector2(0.5f, 0.5f) - rect.pivot) * rect.sizeDelta;
        }

        public static float GetWidth(this RectTransform rect)
        {
            return rect.rect.size.x * rect.lossyScale.x;
        }

        public static float GetHeight(this RectTransform rect)
        {
            return rect.rect.size.y * rect.lossyScale.y;
        }

        public static Vector2 GetSize(this RectTransform rect)
        {
            return rect.rect.size * rect.lossyScale;
        }

        public static bool Contains(this RectTransform rect, Vector2 point)
        {
            Vector2 size = rect.GetSize();
            Vector2 position = rect.position;
            return point.x > position.x - size.x / 2
                   && point.x < position.x + size.x / 2
                   && point.y > position.y - size.y / 2
                   && point.y < position.y + size.y / 2;
        }

        public static bool Contains(this RectTransform rect, Vector2 point, float padding)
        {
            Vector2 size = rect.GetSize();
            Vector2 position = rect.position;
            return point.x > position.x - size.x / 2 - padding
                   && point.x < position.x + size.x / 2 + padding
                   && point.y > position.y - size.y / 2 - padding
                   && point.y < position.y + size.y / 2 + padding;
        }

        public static bool LaterallyContains(this RectTransform rect, Vector2 point, float padding)
        {
            Vector2 size = rect.GetSize();
            Vector2 position = rect.position;
            return point.x > position.x - size.x / 2 - padding
                   && point.x < position.x + size.x / 2 + padding;
        }

        public static void SetLeft(this RectTransform rect, float left)
        {
            rect.offsetMin = rect.offsetMin.WithX(left);
        }

        public static void SetRight(this RectTransform rect, float right)
        {
            rect.offsetMax = rect.offsetMax.WithX(-right);
        }

        public static void SetTop(this RectTransform rect, float top)
        {
            rect.offsetMax = rect.offsetMax.WithY(-top);
        }

        public static void SetBottom(this RectTransform rect, float bottom)
        {
            rect.offsetMin = rect.offsetMin.WithY(bottom);
        }

        #endregion

        #region Line Renderers

        public static void SetAllColorKeys(this LineRenderer lineRenderer, Color color)
        {
            Gradient colorGradient = lineRenderer.colorGradient;
            GradientColorKey[] colorKeys = colorGradient.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i].color = color;
            }

            colorGradient.colorKeys = colorKeys;
            lineRenderer.colorGradient = colorGradient;
        }

        public static void SetAllAlphaKeys(this LineRenderer lineRenderer, float alpha)
        {
            Gradient colorGradient = lineRenderer.colorGradient;
            GradientAlphaKey[] alphaKeys = colorGradient.alphaKeys;
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha = alpha;
            }

            colorGradient.alphaKeys = alphaKeys;
            lineRenderer.colorGradient = colorGradient;
        }

        #endregion

        #region Audio Sources

        public static void RandomizeTime(this AudioSource source)
        {
            source.time = source.clip.length * RandomUtil.RandomUFloat();
        }

        public static void PlayRandomly(this AudioSource source)
        {
            source.RandomizeTime();
            source.Play();
        }

        #endregion

        #region Other

        public static void Teleport(this CharacterController characterController, Vector3 newPosition)
        {
            characterController.enabled = false;
            characterController.transform.position = newPosition;
            characterController.enabled = true;
        }

        public static void RemoveAllDelegateListeners(this Delegate delegateObject)
        {
            if (delegateObject == null)
                return;

            foreach (Delegate d in delegateObject.GetInvocationList())
            {
                delegateObject = Delegate.Remove(delegateObject, d);
            }
        }

        #endregion

        #region Sprite Atlases

#if UNITY_EDITOR
        public static void ClearPackables(this SpriteAtlas atlas)
        {
            atlas.Remove(atlas.GetPackables());
        }

        public static void SetPackables(this SpriteAtlas atlas, Object[] objects)
        {
            atlas.Remove(atlas.GetPackables());
            atlas.Add(objects);
        }

        public static void SetImporterCompression(
            this SpriteAtlas atlas,
            TextureImporterCompression compression
        )
        {
            TextureImporterPlatformSettings platformSettings = atlas.GetPlatformSettings(
                "DefaultTexturePlatform"
            );
            platformSettings.textureCompression = compression;
            atlas.SetPlatformSettings(platformSettings);
        }
#endif

        #endregion

        #region Textures

        public static Texture2D ToTexture2D(this RenderTexture rTexture)
        {
            Texture2D tex = new Texture2D(rTexture.width, rTexture.height, TextureFormat.ARGB32, false);
            RenderTexture oldActiveTexture = RenderTexture.active;
            RenderTexture.active = rTexture;
            tex.ReadPixels(new Rect(0, 0, rTexture.width, rTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = oldActiveTexture;
            return tex;
        }

        public static Texture2D FixPremultipliedAlpha(this Texture2D texture)
        {
            Color32[] data = texture.GetPixels32();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].a == 0 || data[i].a == byte.MaxValue)
                    continue;

                data[i].r = (data[i].r / (data[i].a / 255f)).ClampToByte();
                data[i].g = (data[i].g / (data[i].a / 255f)).ClampToByte();
                data[i].b = (data[i].b / (data[i].a / 255f)).ClampToByte();
            }

            texture.SetPixels32(data);
            texture.Apply();
            return texture;
        }

        public static Texture2D FadeEdges(this Texture2D texture, float fading, float corner)
        {
            Color32[] data = texture.GetPixels32();
            Vector2 halfTextureSize = new Vector2(texture.width / 2f, texture.height / 2f);
            for (int i = 0; i < data.Length; i++)
            {
                int x = i % texture.width;
                int y = i / texture.width;

                float xDist = -(Mathf.Abs(x - halfTextureSize.x) - halfTextureSize.x);
                float yDist = -(Mathf.Abs(y - halfTextureSize.y) - halfTextureSize.y);
                float total = xDist + yDist;
                float distance = Mathf.Min(xDist, yDist);

                float fade = distance / fading;

                if (total / MathUtil.SQRT_2 < corner + fading)
                {
                    fade = Mathf.Min(fade, (total / MathUtil.SQRT_2 - corner) / fading);
                }

                fade = Mathf.Clamp01(fade);
                if (fade != 1)
                    data[i].a = (data[i].a * EaseUtil.Ease(fade, 2, true)).ClampToByte();
            }

            texture.SetPixels32(data);
            texture.Apply();
            return texture;
        }

        #endregion

        public static Thread StartThread(Action action)
        {
            ThreadStart threadStart = new ThreadStart(action);
            Thread thread = new Thread(threadStart);
            thread.Start();
            return thread;
        }
        
        public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
        
        public static bool IsTrue(this NBool nBool)
        {
            //returns true if nBool is True, false if nBool is False
            
            switch (nBool)
            {
                case NBool.True:
                    return true;
                case NBool.False:
                    return false;
                case NBool.Both:
                    return true;
                default:
                    return false;
            }
        }
        
        public static void Print(object message)
        {
            Debug.Log(message);
        }
        
        public static string ArrayAsString<T>(this T[] array)
        {
            return string.Join(", ", array.Select(x => x.ToString()).ToArray());
        }

        public static T[] Add<T>(this T[] array, T item)
        {
            T[] newArray = new T[array.Length + 1];
            Array.Copy(array, newArray, array.Length);
            newArray[array.Length] = item;
            return newArray;
        }
        
        public static T[] Remove<T>(this T[] array, T item)
        {
            List<T> list = new List<T>(array);
            list.Remove(item);
            return list.ToArray();
        }
    }
    
    public enum NBool
    {
        False,
        True,
        Both
    }
}