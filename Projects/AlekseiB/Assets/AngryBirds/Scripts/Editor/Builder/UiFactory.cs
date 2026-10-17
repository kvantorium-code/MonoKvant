using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AngryBirds.EditorTools.Builder
{
    /// <summary>
    /// Small helpers for building uGUI hierarchies from editor code.
    /// Keeps the scene builder readable.
    /// </summary>
    internal static class UiFactory
    {
        public static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return rect;
        }

        public static RectTransform CreateStretch(string name, Transform parent)
        {
            return CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        public static Text CreateText(
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            string content,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            RectTransform rect = CreateRect(
                "Text",
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                anchoredPosition,
                sizeDelta);

            var text = rect.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public static Text CreateCenteredText(Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta, string content, int fontSize, Color color)
        {
            RectTransform rect = CreateRect(
                "Text",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPosition,
                sizeDelta);

            var text = rect.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public static Image CreateImage(
            string name,
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color,
            bool raycastTarget = true)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPosition,
                sizeDelta);

            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static Image CreateStar(Transform parent, Vector2 position, float size, Sprite sprite)
        {
            RectTransform rect = CreateRect(
                "Star",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(size, size));

            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        public static Button CreateButton(
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            string label,
            int fontSize,
            out Text labelText)
        {
            Image background = CreateImage("Button", parent, anchoredPosition, sizeDelta, new Color(0.16f, 0.42f, 0.24f, 0.96f));
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.25f, 0.58f, 0.33f, 1f);
            colors.pressedColor = new Color(0.12f, 0.32f, 0.18f, 1f);
            button.colors = colors;

            labelText = CreateCenteredText(background.transform, Vector2.zero, Vector2.zero, label, fontSize, Color.white);
            labelText.name = "Label";

            return button;
        }

        public static List<Image> CreateStarRow(Transform parent, Vector2 center, float spacing, float size, Sprite empty)
        {
            var stars = new List<Image>();

            for (int i = 0; i < 3; i++)
            {
                Vector2 position = center + new Vector2((i - 1) * spacing, 0f);
                stars.Add(CreateStar(parent, position, size, empty));
            }

            return stars;
        }
    }
}
