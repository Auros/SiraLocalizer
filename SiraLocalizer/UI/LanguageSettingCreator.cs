using System;
using System.Threading;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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
            var otherSettings = (RectTransform)_settingsNavigationController.transform.Find("OtherSettings/Content");

            VerticalLayoutGroup layoutGroup = otherSettings.GetComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.enabled = true;

            ContentSizeFitter contentSizeFitter = otherSettings.GetComponent<ContentSizeFitter>();
            contentSizeFitter.enabled = true;

            LanguageSettingsController languageSettingController = _settingsNavigationController.transform.Find("OtherSettings/Content/LanguageDropdown").GetComponent<LanguageSettingsController>();

            TranslationDetailsTextController.Create(_container, otherSettings, languageSettingController);
            CheckForUpdatesController.Create(_container, otherSettings);
            AutoCheckForUpdatesToggleController.Create(_container, otherSettings);

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
