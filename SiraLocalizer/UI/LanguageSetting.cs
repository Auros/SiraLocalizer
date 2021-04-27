using HMUI;
using Zenject;
using Polyglot;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SiraLocalizer.UI
{
    internal class LanguageSetting : MonoBehaviour, ILocalize
    {
        public event Action<Locale> selectedLanguageChanged;

        private Config _config;

        private SimpleTextDropdown _dropdown;
        private SettingsNavigationController _settingsNavigationController;

        private Locale _selectedLanguage;

        private IReadOnlyList<Locale> _languages;
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

            UpdateLanguages();

            _selectedLanguage = _config.language;
            selectedLanguageChanged?.Invoke(_selectedLanguage);
        }

        public void OnEnable()
        {
            Localization.Instance.AddOnLocalizeEvent(this);

            _settingsNavigationController.didFinishEvent += OnSettingsDidFinish;
            _dropdown.didSelectCellWithIdxEvent += OnSelectedCell;

            int idx = _languages.IndexOf(_selectedLanguage);

            if (idx >= 0)
            {
                _dropdown.SelectCellWithIdx(idx);
            }
            else
            {
                _selectedLanguage = Locale.English;
            }
        }

        public void OnDisable()
        {
            Localization.Instance.RemoveOnLocalizeEvent(this);

            _settingsNavigationController.didFinishEvent -= OnSettingsDidFinish;
            _dropdown.didSelectCellWithIdxEvent -= OnSelectedCell;
        }

        public void OnLocalize()
        {
            UpdateLanguages();
        }

        private void UpdateLanguages()
        {
            // AsReadOnly to avoid accidentally messing around with values inside Polyglot
            _languages = Localization.Instance.SupportedLanguages.Select(l => (Locale)l).ToList().AsReadOnly();
            _languageDisplayNames = Localization.Instance.LocalizedLanguageNames.AsReadOnly();

            _dropdown.SetTexts(_languageDisplayNames);
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