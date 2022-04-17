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
            LanguageSettingsController languageSettingController = _settingsNavigationController.transform.Find("OtherSettings/Content/LanguageDropdown").GetComponent<LanguageSettingsController>();
            _container.InstantiateComponent<TranslationDetailsTextController>(languageSettingController.gameObject);

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
