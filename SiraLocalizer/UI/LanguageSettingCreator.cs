using System;
using System.Threading;
using SiraLocalizer.Crowdin;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable
    {
        private readonly DiContainer _container;
        private readonly SiraLog _logger;
        private readonly SettingsNavigationController _settingsNavigationController;
        private readonly CrowdinDownloader _crowdinDownloader;

        internal LanguageSettingCreator(DiContainer container, SiraLog logger, SettingsNavigationController settingsNavigationController, CrowdinDownloader crowdinDownloader)
        {
            _container = container;
            _logger = logger;
            _settingsNavigationController = settingsNavigationController;
            _crowdinDownloader = crowdinDownloader;
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

            Config config = _container.Resolve<Config>();

            if (!config.startupModalDismissed)
            {
                var modal = SimpleStartupModal.Create(_container, "DOWNLOAD_TRANSLATIONS_MODAL_TEXT");

                modal.closed += async (result) =>
                {
                    config.automaticallyDownloadLocalizations = result;
                    config.startupModalDismissed = true;

                    if (result)
                    {
                        try
                        {
                            await _crowdinDownloader.DownloadLocalizationsAsync(CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex);
                        }
                    }
                };
            }
        }
    }
}
