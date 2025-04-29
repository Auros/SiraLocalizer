using System;
using System.Threading;
using HMUI;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable
    {
        private readonly DiContainer _container;
        private readonly Settings _config;
        private readonly SiraLog _logger;
        private readonly SettingsNavigationController _settingsNavigationController;
        private readonly LocalizationManager _localizationManager;
        private readonly MainMenuViewController _mainMenuViewController;

        private SimpleStartupModal _modal;

        internal LanguageSettingCreator(DiContainer container, Settings config, SiraLog logger, SettingsNavigationController settingsNavigationController, LocalizationManager localizationManager, MainMenuViewController mainMenuViewController)
        {
            _container = container;
            _config = config;
            _logger = logger;
            _settingsNavigationController = settingsNavigationController;
            _localizationManager = localizationManager;
            _mainMenuViewController = mainMenuViewController;
        }

        public void Initialize()
        {
            Transform settingsNavigationController = _settingsNavigationController.transform;
            Transform content = CreateScrollView((RectTransform)settingsNavigationController.Find("GraphicSettings"), (RectTransform)settingsNavigationController.Find("OtherSettings"));

            content.GetComponent<VerticalLayoutGroup>().enabled = true;
            content.GetComponent<ContentSizeFitter>().enabled = true;

            Transform languageDropdownTransform = content.Find("LanguageDropdown");
            languageDropdownTransform.SetSiblingIndex(content.childCount - 1);

            LanguageSettingsController languageSettingsController = languageDropdownTransform.GetComponent<LanguageSettingsController>();

            TranslationDetailsTextController.Create(_container, content, languageSettingsController);
            CheckForUpdatesController.Create(_container, content);
            AutoCheckForUpdatesToggleController.Create(_container, content);

            if (!_config.startupModalDismissed)
            {
                CreateStartupModal();
                _mainMenuViewController.didActivateEvent += OnMainMenuViewControllerActivated;
            }
        }

        private void OnMainMenuViewControllerActivated(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _modal.Show(false);
            _mainMenuViewController.didActivateEvent -= OnMainMenuViewControllerActivated;
        }

        private Transform CreateScrollView(RectTransform template, RectTransform target)
        {
            GameObject targetGameObject = target.gameObject;
            var copy = (RectTransform)_container.InstantiatePrefab(template.gameObject).transform;

            target.offsetMin = copy.offsetMin;
            target.offsetMax = copy.offsetMax;

            MoveChildren(copy, target);

            // add new ScrollView to root object & copy serialized 
            targetGameObject.AddComponent<Touchable>();
            ScrollView oldScrollView = copy.GetComponent<ScrollView>();
            ScrollView newScrollView = _container.InstantiateComponent<ScrollView>(targetGameObject);
            CopyScrollViewValues(oldScrollView, newScrollView);

            // move Content into Viewport
            RectTransform oldContent = oldScrollView._contentRectTransform;
            var newContent = (RectTransform)target.Find("Content");
            newContent.offsetMin = oldContent.offsetMin;
            newContent.offsetMax = oldContent.offsetMax;
            newContent.SetParent(oldScrollView._viewport, false);
            newScrollView._contentRectTransform = newContent;
            Object.Destroy(oldContent.gameObject);

            // make sure the ScrollView updates when the content size changes
            newContent.gameObject.AddComponent<ScrollViewContentUpdater>().scrollView = newScrollView;

            Object.Destroy(copy.gameObject);

            return newContent;
        }

        private void MoveChildren(Transform from, Transform to)
        {
            while (from.childCount > 0)
            {
                from.GetChild(0).SetParent(to, false);
            }
        }

        private void CopyScrollViewValues(ScrollView from, ScrollView to)
        {
            to._viewport = from._viewport;
            to._scrollViewDirection = from._scrollViewDirection;
            to._pageUpButton = from._pageUpButton;
            to._pageDownButton = from._pageDownButton;
            to._verticalScrollIndicator = from._verticalScrollIndicator;
            to._smooth = from._smooth;
            to._joystickScrollSpeed = from._joystickScrollSpeed;
            to._joystickQuickSnapMaxTime = from._joystickQuickSnapMaxTime;
            to._fixedCellSize = from._fixedCellSize;
            to._scrollItemRelativeThresholdPosition = from._scrollItemRelativeThresholdPosition;
            to._pageStepNormalizedSize = from._pageStepNormalizedSize;
        }

        private void CreateStartupModal()
        {
            _modal = SimpleStartupModal.Create(_container, "DOWNLOAD_TRANSLATIONS_MODAL_TEXT");

            _modal.closed += async (result) =>
            {
                _config.automaticallyDownloadLocalizations = result;
                _config.startupModalDismissed = true;

                if (!result)
                {
                    return;
                }

                try
                {
                    await _localizationManager.CheckForUpdatesAndDownloadIfAvailable(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            };
        }
    }
}
