using HMUI;
using TMPro;
using Zenject;
using Polyglot;
using UnityEngine;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable
    {
        private readonly DiContainer _container;
        private readonly SettingsNavigationController _settingsNavigationController;
        private readonly GameplaySetupViewController _gameplaySetupViewController;

        internal LanguageSettingCreator(DiContainer container, SettingsNavigationController settingsNavigationController, GameplaySetupViewController gameplaySetupViewController)
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
            rectTransform.offsetMin = new Vector2(0, -14.2f);
            rectTransform.offsetMax = new Vector2(0, -7.2f);

            LocalizedTextMeshProUGUI label = gameObject.transform.Find("Label").GetComponent<LocalizedTextMeshProUGUI>();
            label.Key = "SETTINGS_LANGUAGE";

            _container.InstantiateComponent<LanguageSetting>(gameObject);

            Plugin.Log.Debug("Created language setting");

            var textGameObject = new GameObject("SiraLocalizerContributorsText");
            var curvedText = textGameObject.AddComponent<CurvedTextMeshPro>();

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(otherSettingsContent, false);
            textRectTransform.offsetMin = new Vector2(-45f, -7.4f);
            textRectTransform.offsetMax = new Vector2(45f, -7.4f);

            curvedText.alignment = TextAlignmentOptions.TopLeft;
            curvedText.lineSpacing = -35f;
            curvedText.fontSize = 3f;
            curvedText.fontStyle = FontStyles.Italic;
            curvedText.gameObject.SetActive(true);

            foreach (var lang in Localization.Instance.SupportedLanguages)
            {
                if (lang == Language.English)
                {
                    continue;
                }
                var contributors = Localization.Get("LANGUAGE_CONTRIBUTORS", lang);
                var name = Localization.Get("MENU_LANGUAGE_THIS", lang);
                if (!string.IsNullOrEmpty(contributors))
                {
                    curvedText.text += $"<b>{name}</b>   <color=#bababa>{contributors}</color>\n";
                }
            }
        }
    }
}