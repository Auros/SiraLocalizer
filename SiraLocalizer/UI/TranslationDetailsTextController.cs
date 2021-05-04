using HMUI;
using IPA.Utilities;
using Polyglot;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class TranslationDetailsTextController : MonoBehaviour
    {
        private LanguageSettingsController _languageSettingsController;
        private TextMeshProUGUI _credits;
        private TextMeshProUGUI _translationStatus;
        private Localizer _localizer;

        internal void Awake()
        {
            _languageSettingsController = GetComponent<LanguageSettingsController>();
        }

        [Inject]
        internal void Construct(Localizer localizer)
        {
            _localizer = localizer;
        }

        internal void OnEnable()
        {
            _languageSettingsController.dropDownValueDidChangeEvent += OnSelectedLanguageChanged;
            OnSelectedLanguageChanged();
        }

        internal void Start()
        {
            AddCreditsTextObject();
            AddTranslationStatusTextObject();
            OnSelectedLanguageChanged();
        }

        internal void OnDisable()
        {
            _languageSettingsController.dropDownValueDidChangeEvent -= OnSelectedLanguageChanged;
        }

        private void AddCreditsTextObject()
        {
            var textGameObject = new GameObject("CreditsText");
            _credits = textGameObject.AddComponent<CurvedTextMeshPro>();
            _credits.fontMaterial = GetComponentInChildren<CurvedTextMeshPro>().fontMaterial;

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(transform, false);
            textRectTransform.offsetMin = new Vector2(-45f, -4f);
            textRectTransform.offsetMax = new Vector2(45f, -4f);

            _credits.alignment = TextAlignmentOptions.TopLeft;
            _credits.lineSpacing = -35f;
            _credits.fontSize = 3f;
            _credits.fontStyle = FontStyles.Italic;
        }

        private void AddTranslationStatusTextObject()
        {
            var textGameObject = new GameObject("TranslationStatusText");
            _translationStatus = textGameObject.AddComponent<CurvedTextMeshPro>();
            _translationStatus.fontMaterial = GetComponentInChildren<CurvedTextMeshPro>().fontMaterial;

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(transform, false);
            textRectTransform.offsetMin = new Vector2(-45f, -9f);
            textRectTransform.offsetMax = new Vector2(45f, -9f);

            _translationStatus.alignment = TextAlignmentOptions.TopLeft;
            _translationStatus.lineSpacing = -35f;
            _translationStatus.fontSize = 3f;
            _translationStatus.fontStyle = FontStyles.Italic;
        }

        private void OnSelectedLanguageChanged()
        {
            if (!_credits || !_translationStatus) return;

            int idx = _languageSettingsController.GetField<int, DropdownSettingsController>("_idx");
            Language language = Localization.Instance.SupportedLanguages[idx];

            if (language > Language.English)
            {
                string contributors = Localization.Get("LANGUAGE_CONTRIBUTORS", language);
                _credits.text = string.Format(Localization.Get("TRANSLATED_BY", language), !string.IsNullOrWhiteSpace(contributors) ? contributors : "—");

                List<Localizer.TranslationStatus> statuses = _localizer.GetTranslationStatuses((Locale)language);
                List<string> fullyTranslated = statuses.Where(s => s.percentTranslated == 100).Select(s => s.name).ToList();
                List<string> partiallyTranslated = statuses.Where(s => s.percentTranslated < 100 && s.percentTranslated > 0).Select(s => $"{s.name} ({Mathf.Clamp(s.percentTranslated, 1, 99):0}%)").ToList();
                List<string> notSupported = statuses.Where(s => s.percentTranslated == 0).Select(s => s.name).ToList();

                _translationStatus.text = string.Empty;

                if (fullyTranslated.Count > 0)
                {
                    _translationStatus.text += string.Format(Localization.Get("TRANSLATION_STATUS_FULL", language), string.Join(", ", fullyTranslated)) + "\n";
                }

                if (partiallyTranslated.Count > 0)
                {
                    _translationStatus.text += string.Format(Localization.Get("TRANSLATION_STATUS_PARTIAL", language), string.Join(", ", partiallyTranslated)) + "\n";
                }

                if (notSupported.Count > 0)
                {
                    _translationStatus.text += string.Format(Localization.Get("TRANSLATION_STATUS_NONE", language), string.Join(", ", notSupported));
                }

                _credits.gameObject.SetActive(true);
                _translationStatus.gameObject.SetActive(true);
            }
            else
            {
                _credits.gameObject.SetActive(false);
                _translationStatus.gameObject.SetActive(false);
            }
        }
    }
}
