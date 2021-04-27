using HMUI;
using TMPro;
using System;
using Zenject;
using Polyglot;
using UnityEngine;
using System.Linq;
using SiraUtil.Interfaces;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable, IDisposable
    {
        private readonly DiContainer _container;
        private readonly ILocalizer _localizer;
        private readonly Config _config;
        private readonly GameplaySetupViewController _gameplaySetupViewController;
        private readonly SettingsNavigationController _settingsNavigationController;

        private CurvedTextMeshPro _credits;
        private LanguageSetting _languageSetting;
        private ToggleWithCallbacks _untranslatedToggle;

        internal LanguageSettingCreator(DiContainer container, [Inject(Id = "SIRA.Localizer")] ILocalizer localizer, Config config, GameplaySetupViewController gameplaySetupViewController, SettingsNavigationController settingsNavigationController)
        {
            _container = container;
            _localizer = localizer;
            _config = config;
            _gameplaySetupViewController = gameplaySetupViewController;
            _settingsNavigationController = settingsNavigationController;
        }

        public void Initialize()
        {
            Transform otherSettingsContent = _settingsNavigationController.transform.Find("OtherSettings/Content");

            if (!otherSettingsContent)
            {
                Plugin.Log.Error("OtherSettings/Content not found!");
                return;
            }

            AddMenuOptions(otherSettingsContent);
            AddIncompleteToggle(otherSettingsContent);

            _languageSetting.selectedLanguageChanged += OnSelectedLanguageChanged;
            _untranslatedToggle.onValueChanged.AddListener(OnShowUntranslatedLanguagesToggleValueChanged);
        }

        public void Dispose()
        {
            _languageSetting.selectedLanguageChanged -= OnSelectedLanguageChanged;
            _untranslatedToggle.onValueChanged.RemoveListener(OnShowUntranslatedLanguagesToggleValueChanged);
        }

        private void AddMenuOptions(Transform container)
        {
            Transform dropdownTemplate = _gameplaySetupViewController.transform.Find("EnvironmentOverrideSettings/Settings/Elements/NormalLevels");

            if (!dropdownTemplate)
            {
                Plugin.Log.Error("Dropdown template not found!");
                return;
            }

            GameObject gameObject = _container.InstantiatePrefab(dropdownTemplate.gameObject, container);
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
            _credits.fontMaterial = label.gameObject.GetComponent<CurvedTextMeshPro>().fontMaterial;

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(container, false);
            textRectTransform.offsetMin = new Vector2(-45f, -14.2f);
            textRectTransform.offsetMax = new Vector2(45f, -7.2f);

            _credits.alignment = TextAlignmentOptions.TopLeft;
            _credits.lineSpacing = -35f;
            _credits.fontSize = 3f;
            _credits.fontStyle = FontStyles.Italic;
        }

        private void AddIncompleteToggle(Transform container)
        {
            Transform toggleTemplate = _gameplaySetupViewController.transform.Find("EnvironmentOverrideSettings/Settings/OverrideEnvironmentsToggle");

            if (!toggleTemplate)
            {
                Plugin.Log.Error("Toggle template not found!");
                return;
            }

            GameObject gameObject = _container.InstantiatePrefab(toggleTemplate.gameObject, container);
            gameObject.name = "ShowIncompleteTranslationsToggle";
            
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(0, -27.4f);
            rectTransform.offsetMax = new Vector2(0, -20.4f);

            gameObject.transform.Find("NameText").GetComponent<LocalizedTextMeshProUGUI>().Key = "SHOW_INCOMPLETE_TRANSLATIONS";

            _untranslatedToggle = gameObject.transform.Find("SwitchView").GetComponent<ToggleWithCallbacks>();
            _untranslatedToggle.onValueChanged.RemoveAllListeners();
            _untranslatedToggle.isOn = _config.showIncompleteTranslations;
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

        private void OnShowUntranslatedLanguagesToggleValueChanged(bool selected)
        {
            _config.showIncompleteTranslations = selected;
            _localizer.RecalculateLanguages();
        }

        private bool KeyHasValueForLanguage(string key, Locale language)
        {
            return !string.IsNullOrWhiteSpace(LocalizationImporter.GetLanguages(key).ElementAtOrDefault((int)language));
        }
    }
}