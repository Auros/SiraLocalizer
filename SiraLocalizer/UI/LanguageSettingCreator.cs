using HMUI;
using TMPro;
using System;
using Zenject;
using Polyglot;
using UnityEngine;
using System.Linq;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable, IDisposable
    {
        private readonly DiContainer _container;
        private readonly GameplaySetupViewController _gameplaySetupViewController;
        private readonly SettingsNavigationController _settingsNavigationController;

        private CurvedTextMeshPro _credits;
        private LanguageSetting _languageSetting;

        internal LanguageSettingCreator(DiContainer container, GameplaySetupViewController gameplaySetupViewController, SettingsNavigationController settingsNavigationController)
        {
            _container = container;
            _gameplaySetupViewController = gameplaySetupViewController;
            _settingsNavigationController = settingsNavigationController;
        }

        public void Initialize()
        {
            AddMenuOption();

            _languageSetting.selectedLanguageChanged += OnSelectedLanguageChanged;
        }

        public void Dispose()
        {
            _languageSetting.selectedLanguageChanged -= OnSelectedLanguageChanged;
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

            _languageSetting = _container.InstantiateComponent<LanguageSetting>(gameObject);

            Plugin.Log.Debug("Created language setting");

            var textGameObject = new GameObject("SiraLocalizerContributorsText");
            _credits = textGameObject.AddComponent<CurvedTextMeshPro>();

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(otherSettingsContent, false);
            textRectTransform.offsetMin = new Vector2(-45f, -7.4f);
            textRectTransform.offsetMax = new Vector2(45f, -7.4f);

            _credits.alignment = TextAlignmentOptions.TopLeft;
            _credits.lineSpacing = -35f;
            _credits.fontSize = 3f;
            _credits.fontStyle = FontStyles.Italic;

        }

        private void OnSelectedLanguageChanged(Locale language)
        {
            if (language != Locale.English && KeyHasValueForLanguage("LANGUAGE_CONTRIBUTORS", language))
            {
                string contributors = Localization.Get("LANGUAGE_CONTRIBUTORS", (Language)language);
                string translatedBy = Localization.Get("MENU_TRANSLATED_BY", (Language)language);

                _credits.gameObject.SetActive(true);
                _credits.text = $"<b>{translatedBy}</b>   <color=#bababa>{contributors}</color>";
            }
            else
            {
                _credits.gameObject.SetActive(false);
            }
        }

        private bool KeyHasValueForLanguage(string key, Locale language)
        {
            return !string.IsNullOrWhiteSpace(LocalizationImporter.GetLanguages(key).ElementAtOrDefault((int)language));
        }
    }
}