using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable
    {
        private readonly DiContainer _container;
        private readonly SettingsNavigationController _settingsNavigationController;

        internal LanguageSettingCreator(DiContainer container, SettingsNavigationController settingsNavigationController)
        {
            _container = container;
            _settingsNavigationController = settingsNavigationController;
        }

        public void Initialize()
        {
            var otherSettings = (RectTransform)_settingsNavigationController.transform.Find("OtherSettings/Content");

            VerticalLayoutGroup layoutGroup = otherSettings.GetComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.enabled = true;

            ContentSizeFitter modalSizeFitter = otherSettings.GetComponent<ContentSizeFitter>();
            modalSizeFitter.enabled = true;

            LanguageSettingsController languageSettingController = _settingsNavigationController.transform.Find("OtherSettings/Content/LanguageDropdown").GetComponent<LanguageSettingsController>();

            TranslationDetailsTextController.Create(_container, otherSettings, languageSettingController);
            CheckForUpdatesController.Create(_container, otherSettings);

            Config config = _container.Resolve<Config>();

            if (!config.startupModalDismissed)
            {
                var modal = SimpleStartupModal.Create(_container, "DOWNLOAD_TRANSLATIONS_MODAL_TEXT");

                modal.closed += (result) =>
                {
                    config.automaticallyDownloadLocalizations = result;
                    config.startupModalDismissed = true;
                };
            }
        }
    }
}
