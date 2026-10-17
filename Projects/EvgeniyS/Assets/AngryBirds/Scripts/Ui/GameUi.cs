using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using AngryBirds.Core;

namespace AngryBirds.Ui
{
    public sealed class GameUi : MonoBehaviour
    {
        private Font _font;
        private Text _pigsText;
        private Text _birdsText;
        private GameObject _hud;
        private GameObject _selectPanel;
        private GameObject _resultPanel;
        private Text _resultText;
        private Transform _selectButtonsRoot;
        private Action _restartHandler;

        public void SetRestartHandler(Action handler)
        {
            _restartHandler = handler;
        }

        public void Build()
        {
            _font = ResolveFont();
            EnsureEventSystem();
            CreateCanvas();
            CreateHud();
            CreateSelectPanel();
            CreateResultPanel();
            _hud.SetActive(false);
            _selectPanel.SetActive(false);
            _resultPanel.SetActive(false);
        }

        public void ShowCharacterSelection(IReadOnlyList<BirdDefinition> characters, Action<BirdDefinition> onSelect)
        {
            _hud.SetActive(false);
            _resultPanel.SetActive(false);
            _selectPanel.SetActive(true);

            ClearChildren(_selectButtonsRoot);

            int count = characters != null ? characters.Count : 0;
            const float spacing = 240f;
            float startX = -(count - 1) * spacing * 0.5f;

            for (int i = 0; i < count; i++)
            {
                BirdDefinition definition = characters[i];
                string label = definition.DisplayName
                    + "\nPower " + definition.LaunchPower.ToString("0.0")
                    + "\nWeight " + definition.Mass.ToString("0.0")
                    + "\nDamage " + definition.DamageMultiplier.ToString("0.0");

                Vector2 position = new Vector2(startX + i * spacing, 0f);
                BirdDefinition captured = definition;
                CreateButton(_selectButtonsRoot, label, new Vector2(200f, 160f), position, definition.Color,
                    () => onSelect?.Invoke(captured), Color.black);
            }
        }

        public void ShowHud(int pigs, int birds)
        {
            _selectPanel.SetActive(false);
            _resultPanel.SetActive(false);
            _hud.SetActive(true);
            UpdatePigs(pigs);
            UpdateBirds(birds);
        }

        public void UpdatePigs(int pigs)
        {
            if (_pigsText != null)
            {
                _pigsText.text = "Pigs: " + pigs;
            }
        }

        public void UpdateBirds(int birds)
        {
            if (_birdsText != null)
            {
                _birdsText.text = "Birds: " + birds;
            }
        }

        public void ShowResult(bool won)
        {
            _hud.SetActive(false);
            _selectPanel.SetActive(false);
            _resultPanel.SetActive(true);
            _resultText.text = won ? "You win!" : "You lose!";
            _resultText.color = won ? new Color(0.2f, 0.85f, 0.3f) : new Color(0.9f, 0.3f, 0.25f);
        }

        private void CreateCanvas()
        {
            var go = new GameObject("GameCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1024f, 768f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void CreateHud()
        {
            _hud = CreatePanel(transform, "Hud", new Color(0f, 0f, 0f, 0f));

            _pigsText = CreateAnchoredText(_hud.transform, "PigsText", "Pigs: 0", 30,
                new Vector2(0f, 1f), new Vector2(150f, -40f), new Vector2(280f, 50f), TextAnchor.MiddleLeft);

            _birdsText = CreateAnchoredText(_hud.transform, "BirdsText", "Birds: 0", 30,
                new Vector2(1f, 1f), new Vector2(-150f, -40f), new Vector2(280f, 50f), TextAnchor.MiddleRight);

            CreateButton(_hud.transform, "Restart", new Vector2(150f, 50f), Vector2.zero,
                new Color(0.2f, 0.25f, 0.35f, 0.95f), InvokeRestart, Color.white, new Vector2(0.5f, 1f), new Vector2(0f, -40f));
        }

        private void CreateSelectPanel()
        {
            _selectPanel = CreatePanel(transform, "CharacterSelect", new Color(0.05f, 0.07f, 0.12f, 0.92f));

            CreateAnchoredText(_selectPanel.transform, "Title", "Choose your bird", 44,
                new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(700f, 70f), TextAnchor.MiddleCenter);

            var root = new GameObject("Buttons", typeof(RectTransform));
            root.transform.SetParent(_selectPanel.transform, false);
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, -20f);
            rootRect.sizeDelta = new Vector2(900f, 260f);
            _selectButtonsRoot = root.transform;
        }

        private void CreateResultPanel()
        {
            _resultPanel = CreatePanel(transform, "Result", new Color(0.05f, 0.07f, 0.12f, 0.92f));

            _resultText = CreateAnchoredText(_resultPanel.transform, "ResultText", "You win!", 64,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(700f, 100f), TextAnchor.MiddleCenter);

            CreateButton(_resultPanel.transform, "Restart", new Vector2(240f, 70f), new Vector2(0f, -70f),
                new Color(0.2f, 0.5f, 0.85f, 1f), InvokeRestart, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -70f));
        }

        private void InvokeRestart()
        {
            _restartHandler?.Invoke();
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private Text CreateAnchoredText(Transform parent, string name, string content, int size,
            Vector2 anchor, Vector2 anchoredPosition, Vector2 sizeDelta, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var text = go.GetComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Vector2 size, Vector2 anchoredPosition,
            Color color, Action onClick, Color textColor, Vector2? anchor = null, Vector2? pivotOffset = null)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            Vector2 anchorValue = anchor ?? new Vector2(0.5f, 0.5f);
            rect.anchorMin = anchorValue;
            rect.anchorMax = anchorValue;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pivotOffset ?? anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = color;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            Text text = CreateAnchoredText(go.transform, "Label", label, 24,
                new Vector2(0.5f, 0.5f), Vector2.zero, size, TextAnchor.MiddleCenter);
            text.color = textColor;
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private static Font ResolveFont()
        {
            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                font = null;
            }

            if (font == null)
            {
                try
                {
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch
                {
                    font = null;
                }
            }

            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Arial", 24);
            }

            return font;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
