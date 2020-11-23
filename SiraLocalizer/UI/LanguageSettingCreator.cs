using HMUI;
using TMPro;
using System;
using Zenject;
using Polyglot;
using UnityEngine;
using IPA.Utilities;
using UnityEngine.UI;

namespace SiraLocalizer.UI
{
    internal class LanguageSettingCreator : IInitializable, IDisposable
    {
        private Button _creditsToggle;
        private CurvedTextMeshPro _credits;
        private readonly DiContainer _container;
        private readonly GameplaySetupViewController _gameplaySetupViewController;
        private readonly SettingsNavigationController _settingsNavigationController;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;

        internal LanguageSettingCreator(DiContainer container, GameplaySetupViewController gameplaySetupViewController, SettingsNavigationController settingsNavigationController, StandardLevelDetailViewController standardLevelDetailViewController)
        {
            _container = container;
            _gameplaySetupViewController = gameplaySetupViewController;
            _settingsNavigationController = settingsNavigationController;
            _standardLevelDetailViewController = standardLevelDetailViewController;
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
            _credits = textGameObject.AddComponent<CurvedTextMeshPro>();

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(otherSettingsContent, false);
            textRectTransform.offsetMin = new Vector2(-45f, -7.4f);
            textRectTransform.offsetMax = new Vector2(45f, -7.4f);

            _credits.alignment = TextAlignmentOptions.TopLeft;
            _credits.lineSpacing = -35f;
            _credits.fontSize = 3f;
            _credits.fontStyle = FontStyles.Italic;
            _credits.gameObject.SetActive(false);

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
                    _credits.text += $"<b>{name}</b>   <color=#bababa>{contributors}</color>\n";
                }
            }

            _creditsToggle = _container.InstantiatePrefabForComponent<Button>(_standardLevelDetailViewController.GetField<StandardLevelDetailView, StandardLevelDetailViewController>("_standardLevelDetailView").practiceButton);
            _creditsToggle.name = "LocalizationCreditsButton";
            UnityEngine.Object.Destroy(_creditsToggle.transform.Find("Content").GetComponent<LayoutElement>());
            _creditsToggle.gameObject.transform.SetParent(otherSettingsContent, false);
            var rect = (_creditsToggle.transform as RectTransform);
            rect.localPosition = new Vector3(-5f, -11f, 0f);
            
            ContentSizeFitter buttonSizeFitter = _creditsToggle.gameObject.AddComponent<ContentSizeFitter>();
            buttonSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            LocalizedTextMeshProUGUI localizer = _creditsToggle.GetComponentInChildren<LocalizedTextMeshProUGUI>(true);
            localizer.Key = "CREDITS_TITLE";

            _creditsToggle.onClick.AddListener(ToggleCredits);
        }

        private void ToggleCredits()
        {
            _credits.gameObject.SetActive(!_credits.isActiveAndEnabled);
        }

        public void Dispose()
        {
            if (_creditsToggle != null)
            {
                _creditsToggle.onClick.RemoveListener(ToggleCredits);
            }
        }
    }
}