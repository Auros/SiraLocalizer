using HMUI;
using Zenject;
using Polyglot;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace SiraLocalizer.UI
{
    internal class LanguageSetting : MonoBehaviour
    {
        public event Action<Language> selectedLanguageChanged;

        private Config _config;

        private SimpleTextDropdown _dropdown;
        private SettingsNavigationController _settingsNavigationController;

        private Language _selectedLanguage;

        private IReadOnlyList<Language> _languages;
        private IReadOnlyList<string> _languageDisplayNames;

        [Inject]
        public void Construct(Config config)
        {
            _config = config;
        }

        public void Awake()
        {
            _dropdown = GetComponentInChildren<SimpleTextDropdown>();
            _settingsNavigationController = GetComponentInParent<SettingsNavigationController>();

            // AsReadOnly to avoid accidentally messing around with values inside Polyglot
            _languages = Localization.Instance.SupportedLanguages.AsReadOnly();
            _languageDisplayNames = Localization.Instance.LocalizedLanguageNames.AsReadOnly();

            _selectedLanguage = _config.language;
            selectedLanguageChanged?.Invoke(_selectedLanguage);
        }

        public void OnEnable()
        {
            _settingsNavigationController.didFinishEvent += OnSettingsDidFinish;
            _dropdown.didSelectCellWithIdxEvent += OnSelectedCell;

            _dropdown.SetTexts(_languageDisplayNames);

            int idx = _languages.IndexOf(_selectedLanguage);

            if (idx >= 0)
            {
                _dropdown.SelectCellWithIdx(idx);
            }
            else
            {
                _selectedLanguage = Language.English;
            }
        }

        public void OnDisable()
        {
            _settingsNavigationController.didFinishEvent -= OnSettingsDidFinish;
            _dropdown.didSelectCellWithIdxEvent -= OnSelectedCell;
        }

        private void OnSettingsDidFinish(SettingsNavigationController.FinishAction finishAction)
        {
            if (finishAction == SettingsNavigationController.FinishAction.Cancel)
            {
                _selectedLanguage = _config.language;
                selectedLanguageChanged?.Invoke(_selectedLanguage);
            }
            else
            {
                _config.language = _selectedLanguage;
            }
        }

        private void OnSelectedCell(DropdownWithTableView dropdown, int idx)
        {
            _selectedLanguage = _languages[idx];
            selectedLanguageChanged?.Invoke(_selectedLanguage);
        }
    }
}