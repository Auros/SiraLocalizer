using System;
using System.Collections.Generic;
using System.Threading;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using Polyglot;
using SiraLocalizer.Providers;
using SiraUtil.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class CheckForUpdatesController : MonoBehaviour
    {
        private Button _button;
        private LocalizedTextMeshProUGUI _text;
        private List<ILocalizationDownloader> _localizationsToDownload;

        private SiraLog _logger;
        private LocalizationManager _localizationManager;

        public static CheckForUpdatesController Create(DiContainer container, Transform parent)
        {
            GameObject template = container.Resolve<GameplaySetupViewController>().transform.Find("PlayerOptions/ViewPort/Content/CommonSection/PlayerHeight").gameObject;

            GameObject root = container.InstantiatePrefab(template, parent);
            root.name = "CheckForUpdates";

            var rootTransform = (RectTransform)root.transform;

            DestroyImmediate(root.GetComponent<PlayerHeightSettingsController>());
            DestroyImmediate(rootTransform.Find("Icon").gameObject);
            DestroyImmediate(rootTransform.Find("ValueText").gameObject);

            var textTransform = (RectTransform)rootTransform.Find("Title");
            textTransform.anchoredPosition = Vector2.zero;

            LocalizedTextMeshProUGUI title = textTransform.GetComponent<LocalizedTextMeshProUGUI>();
            title.Key = "LOCALIZATION_UPDATES";

            (Button button, LocalizedTextMeshProUGUI text) = SetUpButton((RectTransform)rootTransform.Find("MeassureButton"), textTransform.GetComponent<CurvedTextMeshPro>());

            var controller = container.InstantiateComponent<CheckForUpdatesController>(root);
            controller._button = button;
            controller._text = text;
            return controller;
        }

        private static (Button, LocalizedTextMeshProUGUI) SetUpButton(RectTransform buttonTransform, TextMeshProUGUI templateText)
        {
            GameObject buttonObject = buttonTransform.gameObject;

            buttonTransform.name = "CheckForUpdatesButton";
            buttonTransform.anchoredPosition = Vector2.zero;

            Destroy(buttonTransform.Find("Icon").gameObject);

            Button button = buttonTransform.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            var stackLayout = buttonTransform.gameObject.AddComponent<StackLayoutGroup>();
            stackLayout.childAlignment = TextAnchor.MiddleCenter;

            var contentObject = new GameObject("Content", typeof(RectTransform));
            var contentTransform = (RectTransform)contentObject.transform;
            contentTransform.SetParent(buttonTransform, false);

            StackLayoutGroup layoutGroup = contentObject.AddComponent<StackLayoutGroup>();
            layoutGroup.padding = new RectOffset(3, 3, 0, 0);

            var buttonTextObject = new GameObject("Text", typeof(RectTransform));
            buttonTextObject.transform.SetParent(contentTransform, false);
            ((RectTransform)buttonTextObject.transform).anchoredPosition = Vector2.zero;

            CurvedTextMeshPro checkButtonText = buttonTextObject.AddComponent<CurvedTextMeshPro>();
            checkButtonText.font = templateText.font;
            checkButtonText.fontMaterial = templateText.fontMaterial;
            checkButtonText.fontSize = 4;
            checkButtonText.fontStyle = FontStyles.Italic;
            checkButtonText.alignment = TextAlignmentOptions.Midline;

            LocalizedTextMeshProUGUI localizedText = buttonTextObject.AddComponent<LocalizedTextMeshProUGUI>();
            localizedText.SetField<LocalizedTextComponent<TextMeshProUGUI>, TextMeshProUGUI>("localizedComponent", checkButtonText);
            localizedText.Key = "CHECK_FOR_UPDATES_BUTTON";

            ContentSizeFitter textSizeFitter = buttonTextObject.AddComponent<ContentSizeFitter>();
            textSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ContentSizeFitter buttonSizeFitter = buttonObject.AddComponent<ContentSizeFitter>();
            buttonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return (button, localizedText);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
            _button.interactable = true;
            _text.Key = "CHECK_FOR_UPDATES_BUTTON";
            _localizationsToDownload = null;

            ForceRebuildTextLayout();
        }

        [Inject]
        [UsedImplicitly]
        private void Construct(SiraLog logger, LocalizationManager localizationManager)
        {
            _logger = logger;
            _localizationManager = localizationManager;
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (_localizationsToDownload != null)
            {
                DownloadUpdates();
            }
            else
            {
                CheckForUpdates();
            }
        }

        private async void DownloadUpdates()
        {
            _logger.Info("Downloading updates");

            _button.interactable = false;
            _text.Key = "DOWNLOADING_UPDATES";

            try
            {
                await _localizationManager.DownloadLocalizationsAsync(_localizationsToDownload, CancellationToken.None);
                _text.Key = "UPDATED_SUCCESSFULLY";
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _text.Key = "FAILED_TO_UPDATE";
            }

            ForceRebuildTextLayout();
        }

        private async void CheckForUpdates()
        {
            _logger.Info("Checking for updates");

            _button.interactable = false;
            _text.Key = "CHECKING_FOR_UPDATES";

            try
            {
                _localizationsToDownload = await _localizationManager.CheckForUpdatesAsync(CancellationToken.None);

                if (_localizationsToDownload != null)
                {
                    _button.interactable = true;
                    _text.Key = "DOWNLOAD_UPDATES";
                }
                else
                {
                    _text.Key = "NO_UPDATE_FOUND";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _text.Key = "FAILED_TO_UPDATE";
            }

            ForceRebuildTextLayout();
        }

        private void ForceRebuildTextLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_text.transform);
        }
    }
}
