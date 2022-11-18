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
        private readonly Config _config;
        private readonly SiraLog _logger;
        private readonly SettingsNavigationController _settingsNavigationController;
        private readonly LocalizationManager _localizationManager;

        internal LanguageSettingCreator(DiContainer container, Config config, SiraLog logger, SettingsNavigationController settingsNavigationController, LocalizationManager localizationManager)
        {
            _container = container;
            _config = config;
            _logger = logger;
            _settingsNavigationController = settingsNavigationController;
            _localizationManager = localizationManager;
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
                ShowConfigStartupModal();
            }
        }

        private void ShowConfigStartupModal()
        {
            var modal = SimpleStartupModal.Create(_container, "DOWNLOAD_TRANSLATIONS_MODAL_TEXT");

            modal.closed += async (result) =>
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
