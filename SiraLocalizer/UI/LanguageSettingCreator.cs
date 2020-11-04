using Polyglot;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable
    {
        private readonly DiContainer _container;
        private readonly SettingsNavigationController _settingsNavigationController;
        private readonly GameplaySetupViewController _gameplaySetupViewController;

        private LanguageSettingCreator(DiContainer container, SettingsNavigationController settingsNavigationController, GameplaySetupViewController gameplaySetupViewController)
        {
            _container = container;
            _settingsNavigationController = settingsNavigationController;
            _gameplaySetupViewController = gameplaySetupViewController;
        }

        public void Initialize()
        {
            AddMenuOption();
        }

        private void AddMenuOption()
        {
            Transform dropdownTemplate = _gameplaySetupViewController.transform.Find("EnvironmentOverrideSettings/Settings/Elements/NormalLevels");
            Transform otherSettingsContent = _settingsNavigationController.transform.Find("OtherSettings/Content");

            if (!dropdownTemplate)
            {
                Plugin.Log.Error("Dropdown template not found!");
                return;
            }

            if (!otherSettingsContent)
            {
                Plugin.Log.Error("OtherSettings/Content not found!");
                return;
            }

            GameObject gameObject = _container.InstantiatePrefab(dropdownTemplate.gameObject, otherSettingsContent);
            gameObject.name = "LanguageSetting";

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(0, -21.4f);
            rectTransform.offsetMax = new Vector2(0, -14.4f);

            LocalizedTextMeshProUGUI label = gameObject.transform.Find("Label").GetComponent<LocalizedTextMeshProUGUI>();
            label.Key = "SETTINGS_LANGUAGE";

            _container.InstantiateComponent<LanguageSetting>(gameObject);

            Plugin.Log.Debug("Created language setting");
        }
    }
}
