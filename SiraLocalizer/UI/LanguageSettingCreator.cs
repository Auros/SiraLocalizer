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
        }
    }
}