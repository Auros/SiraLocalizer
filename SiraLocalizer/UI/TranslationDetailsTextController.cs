using HMUI;
using IPA.Utilities;
using Polyglot;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class TranslationDetailsTextController : MonoBehaviour
    {
        private LanguageSettingsController _languageSettingsController;
        private TextMeshProUGUI _credits;
        private TextMeshProUGUI _translationStatus;
        private Localizer _localizer;

        public static TranslationDetailsTextController Create(DiContainer container, Transform parent, LanguageSettingsController languageSettingsController)
        {
            var translationDetails = new GameObject("TranslationDetails", typeof(RectTransform));
            var translationDetailsTransform = (RectTransform)translationDetails.transform;
            translationDetailsTransform.SetParent(parent, false);

            VerticalLayoutGroup layoutGroup = translationDetails.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter contentSizeFitter = translationDetails.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Material fontMaterial = languageSettingsController.GetComponentInChildren<CurvedTextMeshPro>().fontMaterial;

            TranslationDetailsTextController controller = container.InstantiateComponent<TranslationDetailsTextController>(translationDetails);

            controller._languageSettingsController = languageSettingsController;
            controller._credits = AddCreditsTextObject(translationDetailsTransform, fontMaterial);
            controller._translationStatus = AddTranslationStatusTextObject(translationDetailsTransform, fontMaterial);

            return controller;
        }

        private static CurvedTextMeshPro AddCreditsTextObject(Transform parent, Material fontMaterial)
        {
            var textGameObject = new GameObject("CreditsText");
            CurvedTextMeshPro credits = textGameObject.AddComponent<CurvedTextMeshPro>();
            credits.fontMaterial = fontMaterial;

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(parent, false);

            credits.alignment = TextAlignmentOptions.TopLeft;
            credits.lineSpacing = -35f;
            credits.fontSize = 3f;
            credits.fontStyle = FontStyles.Italic;

            var contentSizeFitter = textGameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return credits;
        }

        private static CurvedTextMeshPro AddTranslationStatusTextObject(Transform parent, Material fontMaterial)
        {
            var textGameObject = new GameObject("TranslationStatusText");
            CurvedTextMeshPro translationStatus = textGameObject.AddComponent<CurvedTextMeshPro>();
            translationStatus.fontMaterial = fontMaterial;

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(parent, false);

            Debug.Log("Transform parent " + parent);
            Debug.Log("textRectTransform parent " + textRectTransform.parent);

            translationStatus.alignment = TextAlignmentOptions.TopLeft;
            translationStatus.lineSpacing = -35f;
            translationStatus.fontSize = 3f;
            translationStatus.fontStyle = FontStyles.Italic;

            var contentSizeFitter = textGameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return translationStatus;
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
            OnSelectedLanguageChanged();
        }

        internal void OnDisable()
        {
            _languageSettingsController.dropDownValueDidChangeEvent -= OnSelectedLanguageChanged;
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
                var fullyTranslated = statuses.Where(s => s.percentTranslated == 100).Select(s => s.name).ToList();
                var partiallyTranslated = statuses.Where(s => s.percentTranslated is < 100 and > 0).Select(s => $"{s.name} ({Mathf.Clamp(s.percentTranslated, 1, 99):0}%)").ToList();
                var notSupported = statuses.Where(s => s.percentTranslated == 0).Select(s => s.name).ToList();

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
