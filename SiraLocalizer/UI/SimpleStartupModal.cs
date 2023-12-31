using System;
using HMUI;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class SimpleStartupModal : MonoBehaviour
    {
        public event Action<bool> closed;

        private ModalView _modalView;
        private MainMenuViewController _mainMenuViewController;
        private ScreenSystem _screenSystem;

        public static SimpleStartupModal Create(DiContainer container, string localizationKey)
        {
            Transform gameplaySetupViewController = container.Resolve<GameplaySetupViewController>().transform;
            GameObject modalViewObject = container.InstantiatePrefab(gameplaySetupViewController.transform.Find("ColorsOverrideSettings/Settings/Detail/ColorSchemeDropDown/DropdownTableView").gameObject);
            modalViewObject.name = "SiraLocalizerStartupModal";

            DestroyImmediate(modalViewObject.GetComponent<TableView>());
            DestroyImmediate(modalViewObject.GetComponent<ScrollRect>());
            DestroyImmediate(modalViewObject.GetComponent<ScrollView>());
            DestroyImmediate(modalViewObject.GetComponent<EventSystemListener>());

            foreach (RectTransform child in modalViewObject.transform)
            {
                if (child.name == "BG")
                {
                    continue;
                }

                Destroy(child.gameObject);
            }

            MainMenuViewController mainMenuViewController = container.Resolve<MainMenuViewController>();
            Transform mainMenuViewControllerTransform = mainMenuViewController.transform;

            #region SiraLocalizerStartupModal
            var rectTransform = (RectTransform)modalViewObject.transform;
            rectTransform.SetParent(mainMenuViewControllerTransform, false);
            rectTransform.anchorMin = Vector2.one * 0.5f;
            rectTransform.anchorMax = Vector2.one * 0.5f;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(100, 0);

            ContentSizeFitter modalSizeFitter = modalViewObject.AddComponent<ContentSizeFitter>();
            modalSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            modalSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var stackLayout = modalViewObject.AddComponent<StackLayoutGroup>();
            stackLayout.childAlignment = TextAnchor.MiddleCenter;
            #endregion

            #region Content
            var content = new GameObject("Content", typeof(RectTransform));
            var contentTransform = (RectTransform)content.transform;
            contentTransform.SetParent(rectTransform, false);

            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.padding = new RectOffset(3, 3, 3, 3);

            ContentSizeFitter contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            #endregion

            #region Text
            var textObject = new GameObject("Text", typeof(RectTransform));
            var textTransform = (RectTransform)textObject.transform;
            textTransform.SetParent(contentTransform, false);

            CurvedTextMeshPro text = textObject.AddComponent<CurvedTextMeshPro>();
            CurvedTextMeshPro template = gameplaySetupViewController.Find("GameplayModifiers/Info/MultiplierText").GetComponent<CurvedTextMeshPro>();
            text.font = template.font;
            text.fontMaterial = template.fontMaterial;
            text.fontSize = 5;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.richText = true;

            LocalizedTextMeshProUGUI localizedText = textObject.AddComponent<LocalizedTextMeshProUGUI>();
            localizedText.localizedComponent = text;
            localizedText.Key = localizationKey;
            #endregion

            #region Buttons
            var buttonsObject = new GameObject("Buttons", typeof(RectTransform));
            var buttonsTransform = (RectTransform)buttonsObject.transform;
            buttonsTransform.SetParent(contentTransform, false);

            HorizontalLayoutGroup buttonsLayoutGroup = buttonsObject.AddComponent<HorizontalLayoutGroup>();
            buttonsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayoutGroup.childControlHeight = false;
            buttonsLayoutGroup.childControlWidth = false;
            buttonsLayoutGroup.spacing = 5;

            ContentSizeFitter buttonsFitter = buttonsObject.AddComponent<ContentSizeFitter>();
            buttonsFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            buttonsFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            Transform settingsNavigationControllerTransform = container.Resolve<SettingsNavigationController>().transform;
            Button noButton = CreateButton(settingsNavigationControllerTransform.Find("BottomPanel/CancelButton"), "NoButton", "BUTTON_NO", buttonsObject.transform);
            Button yesButton = CreateButton(settingsNavigationControllerTransform.Find("BottomPanel/OkButton"), "YesButton", "BUTTON_YES", buttonsObject.transform);
            #endregion

            SimpleStartupModal modal = modalViewObject.AddComponent<SimpleStartupModal>();
            modal._modalView = modalViewObject.GetComponent<ModalView>();
            modal._mainMenuViewController = mainMenuViewController;
            modal._screenSystem = container.Resolve<HierarchyManager>().GetComponent<ScreenSystem>();

            noButton.onClick.AddListener(modal.OnNoButtonClicked);
            yesButton.onClick.AddListener(modal.OnYesButtonClicked);
            mainMenuViewController.didActivateEvent += modal.OnMainMenuViewControllerActivated;

            return modal;
        }

        private static Button CreateButton(Transform template, string name, string localizationKey, Transform parent)
        {
            var transform = (RectTransform)Instantiate(template);
            transform.name = name;
            transform.anchorMin = new Vector2(0.5f, 0);
            transform.anchorMax = new Vector2(0.5f, 0);
            transform.SetParent(parent, false);

            Button button = transform.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            transform.Find("Content/Text").GetComponent<LocalizedTextMeshProUGUI>().Key = localizationKey;

            return button;
        }

        private void OnMainMenuViewControllerActivated(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _modalView.SetupView(_screenSystem.mainScreen.transform);
            _modalView.Show(false);
            _mainMenuViewController.didActivateEvent -= OnMainMenuViewControllerActivated;
        }

        private void OnYesButtonClicked()
        {
            closed?.Invoke(true);
            _modalView.Hide(true);
        }

        private void OnNoButtonClicked()
        {
            closed?.Invoke(false);
            _modalView.Hide(true);
        }
    }
}
