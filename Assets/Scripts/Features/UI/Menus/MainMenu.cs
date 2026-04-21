using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Features.UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject menuOverlayRoot;

        private CanvasGroup _menuOverlayCanvasGroup;
        private Button _panelToggleButton;

        private void Awake()
        {
            FindSettingsPanelIfMissing();

            if (settingsPanel != null)
            {
                ConfigureSettingsPanel();
                settingsPanel.SetActive(false);
            }

            FindMenuOverlayIfMissing();
            CacheMenuOverlayCanvasGroup();
        }

        public void Play()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void ToggleSettingsPanel()
        {
            FindSettingsPanelIfMissing();
            var isCurrentlyVisible = settingsPanel != null && settingsPanel.activeSelf;
            SetSettingsPanelVisible(!isCurrentlyVisible);
        }

        public void ShowSettingsPanel()
        {
            SetSettingsPanelVisible(true);
        }

        private void SetSettingsPanelVisible(bool isVisible)
        {
            FindSettingsPanelIfMissing();
            if (settingsPanel == null) return;

            if (!isVisible)
            {
                SetMenuOverlayVisible(true);
                settingsPanel.SetActive(false);
                return;
            }

            settingsPanel.SetActive(true);
            ConfigureSettingsPanel();
            ApplyPortraitLayout();
            EnsurePanelToggleButton();
            EnsureChangeModeTextWrap();
            EnsureSettingsTextSizes();
            SetMenuOverlayVisible(false);

            if (settingsPanel.transform is RectTransform panelRect)
            {
                panelRect.SetAsLastSibling();
            }
        }

        private void FindSettingsPanelIfMissing()
        {
            if (settingsPanel != null) return;

            var allTransforms = transform.root.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t.name == "Settings Panel")
                {
                    settingsPanel = t.gameObject;
                    return;
                }
            }
        }

        private void FindMenuOverlayIfMissing()
        {
            if (menuOverlayRoot != null) return;

            var allTransforms = transform.root.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t.name == "MenuOverlay")
                {
                    menuOverlayRoot = t.gameObject;
                    return;
                }
            }
        }

        private void CacheMenuOverlayCanvasGroup()
        {
            FindMenuOverlayIfMissing();
            if (menuOverlayRoot == null) return;

            _menuOverlayCanvasGroup = menuOverlayRoot.GetComponent<CanvasGroup>();
        }

        private void SetMenuOverlayVisible(bool isVisible)
        {
            CacheMenuOverlayCanvasGroup();
            if (_menuOverlayCanvasGroup == null) return;

            _menuOverlayCanvasGroup.alpha = isVisible ? 1f : 0f;
            _menuOverlayCanvasGroup.interactable = isVisible;
            _menuOverlayCanvasGroup.blocksRaycasts = isVisible;
        }

        private void ConfigureSettingsPanel()
        {
            if (settingsPanel == null) return;

            if (settingsPanel.transform is RectTransform panelRect)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
                panelRect.sizeDelta = Vector2.zero;
            }

            var panelGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (panelGroup == null) panelGroup = settingsPanel.AddComponent<CanvasGroup>();
            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
            panelGroup.ignoreParentGroups = true;

            var panelImage = settingsPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                // Keep the dim visual, but do not consume clicks on the full-screen backdrop
                // so the top-right settings button can toggle the panel back off.
                panelImage.raycastTarget = false;
                EnsureImageSprite(panelImage);
            }

            var panelCanvas = settingsPanel.GetComponent<Canvas>();
            if (panelCanvas == null) panelCanvas = settingsPanel.AddComponent<Canvas>();
            panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            panelCanvas.worldCamera = null;
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 5000;

            if (settingsPanel.GetComponent<GraphicRaycaster>() == null)
            {
                settingsPanel.AddComponent<GraphicRaycaster>();
            }

            var panelButtons = settingsPanel.GetComponentsInChildren<Button>(true);
            foreach (var button in panelButtons)
            {
                if (button.name == "Quit Button") continue;

                if (button.targetGraphic is Image buttonImage)
                {
                    EnsureImageSprite(buttonImage);
                }
            }
        }

        private void ApplyPortraitLayout()
        {
            if (settingsPanel == null) return;

            var shortEdge = Mathf.Min(Screen.width, Screen.height);
            var longEdge = Mathf.Max(Screen.width, Screen.height);

            var buttonWidth = Mathf.Clamp(shortEdge * 0.9f, 380f, 700f);
            var buttonHeight = Mathf.Clamp(longEdge * 0.11f, 100f, 170f);

            SetCenteredRect("Settings Title", new Vector2(0f, buttonHeight * 1.8f), new Vector2(buttonWidth, buttonHeight * 0.9f));
            SetCenteredRect("Switch Mode Button", new Vector2(0f, buttonHeight * 0.55f), new Vector2(buttonWidth, buttonHeight));
            SetCenteredRect("Quit Button", new Vector2(0f, -buttonHeight * 0.75f), new Vector2(buttonWidth, buttonHeight));
        }

        private void SetCenteredRect(string childName, Vector2 anchoredPos, Vector2 size)
        {
            if (settingsPanel == null) return;

            var target = settingsPanel.transform.Find(childName) as RectTransform;
            if (target == null) return;

            target.anchorMin = new Vector2(0.5f, 0.5f);
            target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);
            target.anchoredPosition = anchoredPos;
            target.sizeDelta = size;
        }

        private void EnsurePanelToggleButton()
        {
            if (settingsPanel == null) return;

            if (_panelToggleButton == null)
            {
                var existing = settingsPanel.transform.Find("Settings Toggle Button");
                if (existing != null)
                {
                    _panelToggleButton = existing.GetComponent<Button>();
                }
            }

            if (_panelToggleButton == null)
            {
                var go = new GameObject("Settings Toggle Button", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(settingsPanel.transform, false);

                var image = go.GetComponent<Image>();
                EnsureImageSprite(image);
                image.color = Color.white;

                _panelToggleButton = go.GetComponent<Button>();
                _panelToggleButton.targetGraphic = image;
                _panelToggleButton.onClick.AddListener(ToggleSettingsPanel);
            }

            if (_panelToggleButton.targetGraphic is Image buttonImage)
            {
                var source = transform.root.GetComponentsInChildren<Button>(true);
                foreach (var b in source)
                {
                    if (b == _panelToggleButton) continue;
                    if (b.name != "Settings Button") continue;
                    if (b.targetGraphic is Image sourceImage && b.transform is RectTransform sourceRt)
                    {
                        buttonImage.sprite = sourceImage.sprite;
                        buttonImage.type = sourceImage.type;
                        buttonImage.color = sourceImage.color;
                        CopyRectTransform(sourceRt, _panelToggleButton.transform as RectTransform);
                        MatchPanelCanvasScalerToSource(sourceRt);
                        break;
                    }
                }
            }
        }

        private void EnsureChangeModeTextWrap()
        {
            if (settingsPanel == null) return;

            var changeModeButton = settingsPanel.transform.Find("Switch Mode Button");
            if (changeModeButton == null) return;

            var text = changeModeButton.GetComponentInChildren<Text>(true);
            if (text == null) return;

            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignment = TextAnchor.MiddleCenter;
        }

        private void EnsureSettingsTextSizes()
        {
            if (settingsPanel == null) return;

            SetTextSize("Settings Title", 44);
            SetTextSize("Switch Mode Button", 34);
            SetTextSize("Quit Button", 36);
        }

        private void SetTextSize(string objectName, int fontSize)
        {
            var node = settingsPanel.transform.Find(objectName);
            if (node == null) return;

            var text = node.GetComponentInChildren<Text>(true);
            if (text == null) return;

            text.resizeTextForBestFit = false;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
        }

        private static void CopyRectTransform(RectTransform source, RectTransform target)
        {
            if (source == null || target == null) return;

            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
        }

        private void MatchPanelCanvasScalerToSource(RectTransform sourceRect)
        {
            if (settingsPanel == null || sourceRect == null) return;

            var sourceScaler = sourceRect.GetComponentInParent<CanvasScaler>();
            if (sourceScaler == null) return;

            var panelScaler = settingsPanel.GetComponent<CanvasScaler>();
            if (panelScaler == null) panelScaler = settingsPanel.AddComponent<CanvasScaler>();

            panelScaler.uiScaleMode = sourceScaler.uiScaleMode;
            panelScaler.referencePixelsPerUnit = sourceScaler.referencePixelsPerUnit;
            panelScaler.scaleFactor = sourceScaler.scaleFactor;
            panelScaler.referenceResolution = sourceScaler.referenceResolution;
            panelScaler.screenMatchMode = sourceScaler.screenMatchMode;
            panelScaler.matchWidthOrHeight = sourceScaler.matchWidthOrHeight;
            panelScaler.physicalUnit = sourceScaler.physicalUnit;
            panelScaler.fallbackScreenDPI = sourceScaler.fallbackScreenDPI;
            panelScaler.defaultSpriteDPI = sourceScaler.defaultSpriteDPI;
            panelScaler.dynamicPixelsPerUnit = sourceScaler.dynamicPixelsPerUnit;
        }

        private static void EnsureImageSprite(Image image) { }


        public void ChangeGameMode()
        {
            Debug.Log("Game mode switching is not implemented yet.");
        }

        public void Quit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
            Debug.Log("Player Has Quit The Game");
        }
    }
}
