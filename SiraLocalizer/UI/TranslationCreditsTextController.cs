using HMUI;
using IPA.Utilities;
using Polyglot;
using TMPro;
using UnityEngine;

namespace SiraLocalizer.UI
{
    internal class TranslationCreditsTextController : MonoBehaviour
    {
        private LanguageSettingsController _languageSettingsController;
        private TextMeshProUGUI _credits;

        internal void Awake()
        {
            _languageSettingsController = GetComponent<LanguageSettingsController>();
        }

        internal void OnEnable()
        {
            _languageSettingsController.dropDownValueDidChangeEvent += OnSelectedLanguageChanged;
            OnSelectedLanguageChanged();
        }

        internal void Start()
        {
            AddCreditsTextObject();
            OnSelectedLanguageChanged();
        }

        internal void OnDisable()
        {
            _languageSettingsController.dropDownValueDidChangeEvent -= OnSelectedLanguageChanged;
        }

        private void AddCreditsTextObject()
        {
            var textGameObject = new GameObject("SiraLocalizerContributorsText");
            _credits = textGameObject.AddComponent<CurvedTextMeshPro>();
            _credits.fontMaterial = GetComponentInChildren<CurvedTextMeshPro>().fontMaterial;

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(transform, false);
            textRectTransform.offsetMin = new Vector2(-45f, 0f);
            textRectTransform.offsetMax = new Vector2(45f, -4f);

            _credits.alignment = TextAlignmentOptions.TopLeft;
            _credits.lineSpacing = -35f;
            _credits.fontSize = 3f;
            _credits.fontStyle = FontStyles.Italic;
        }

        private void OnSelectedLanguageChanged()
        {
            if (!_credits) return;

            Language language = _languageSettingsController.GetField<LanguageSO, LanguageSettingsController>("_settingsValue").value;

            if (language != Language.English)
            {
                string contributors = Localization.Get("LANGUAGE_CONTRIBUTORS", language);
                string translatedBy = Localization.Get("MENU_TRANSLATED_BY", language);

                if (!string.IsNullOrWhiteSpace(contributors) && !string.IsNullOrWhiteSpace(translatedBy))
                {
                    _credits.gameObject.SetActive(true);
                    _credits.text = $"<b>{translatedBy}</b>   <color=#bababa>{contributors}</color>";
                }
                else
                {
                    _credits.gameObject.SetActive(false);
                }
            }
            else
            {
                _credits.gameObject.SetActive(false);
            }
        }
    }
}
