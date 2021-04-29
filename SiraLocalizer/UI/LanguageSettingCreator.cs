using HMUI;
using Zenject;
using UnityEngine;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable
    {
        private readonly SettingsNavigationController _settingsNavigationController;

        internal LanguageSettingCreator(SettingsNavigationController settingsNavigationController)
        {
            _settingsNavigationController = settingsNavigationController;
        }

        public void Initialize()
        {
            Transform otherSettingsContent = _settingsNavigationController.transform.Find("OtherSettings/Content");
            LanguageSettingsController languageSettingController = otherSettingsContent.Find("LanguageDropdown").GetComponent<LanguageSettingsController>();
            languageSettingController.gameObject.AddComponent<TranslationCreditsTextController>();
        }
    }
}