using HMUI;
using Polyglot;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class LanguageSetting : MonoBehaviour
    {
        private LanguageManager _languageManager;

        private SimpleTextDropdown _dropdown;
        private SettingsNavigationController _settingsNavigationController;

        private Language _selectedLanguage;

        private IReadOnlyList<Language> _languages;
        private IReadOnlyList<string> _languageDisplayNames;

        [Inject]
        public void Construct(LanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        public void Awake()
        {
            _dropdown = GetComponentInChildren<SimpleTextDropdown>();
            _settingsNavigationController = GetComponentInParent<SettingsNavigationController>();

            // AsReadOnly to avoid accidentally messing around with values inside Polyglot
            _languages = Localization.Instance.SupportedLanguages.AsReadOnly();
            _languageDisplayNames = Localization.Instance.LocalizedLanguageNames.AsReadOnly();

            _selectedLanguage = _languageManager.selectedLanguage;
        }

        public void OnEnable()
        {
            _settingsNavigationController.didFinishEvent += OnSettingsDidFinish;
            _dropdown.didSelectCellWithIdxEvent += OnSelectedCell;

            _dropdown.SetTexts(_languageDisplayNames);
            _dropdown.SelectCellWithIdx(_languages.IndexOf(_selectedLanguage));
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
                _selectedLanguage = _languageManager.selectedLanguage;
            }
            else
            {
                _languageManager.selectedLanguage = _selectedLanguage;
            }
        }

        private void OnSelectedCell(DropdownWithTableView dropdown, int idx)
        {
            _selectedLanguage = _languages[idx];
        }
    }
}
