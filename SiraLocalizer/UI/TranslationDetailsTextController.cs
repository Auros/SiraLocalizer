using System.Collections.Generic;
using System.Linq;
using BGLib.Polyglot;
using HMUI;
using JetBrains.Annotations;
using SiraLocalizer.Records;
using SiraLocalizer.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class TranslationDetailsTextController : MonoBehaviour, ILocalize
    {
        private LanguageSettingsController _languageSettingsController;
        private TextMeshProUGUI _credits;
        private TextMeshProUGUI _translationStatus;
        private LocalizationManager _localizationManager;

        public static TranslationDetailsTextController Create(DiContainer container, Transform parent, LanguageSettingsController languageSettingsController)
        {
            var translationDetails = new GameObject("TranslationDetails", typeof(RectTransform));
            translationDetails.SetActive(false);

            var translationDetailsTransform = (RectTransform)translationDetails.transform;
            translationDetailsTransform.SetParent(parent, false);

            VerticalLayoutGroup layoutGroup = translationDetails.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandHeight = false;

            CurvedTextMeshPro templateText = languageSettingsController.GetComponentInChildren<CurvedTextMeshPro>();
            TMP_FontAsset font = templateText.font;
            Material fontMaterial = templateText.fontMaterial;

            TranslationDetailsTextController controller = container.InstantiateComponent<TranslationDetailsTextController>(translationDetails);

            controller._languageSettingsController = languageSettingsController;
            controller._credits = AddCreditsTextObject(translationDetailsTransform, font, fontMaterial);
            controller._translationStatus = AddTranslationStatusTextObject(translationDetailsTransform, font, fontMaterial);

            translationDetails.SetActive(true);

            return controller;
        }

        public void OnLocalize(LocalizationModel model)
        {
            RefreshValues();
        }

        private static CurvedTextMeshPro AddCreditsTextObject(Transform parent, TMP_FontAsset font, Material fontMaterial)
        {
            var textGameObject = new GameObject("CreditsText");
            CurvedTextMeshPro credits = textGameObject.AddComponent<CurvedTextMeshPro>();
            credits.font = font;
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

        private static CurvedTextMeshPro AddTranslationStatusTextObject(Transform parent, TMP_FontAsset font, Material fontMaterial)
        {
            var textGameObject = new GameObject("TranslationStatusText");
            CurvedTextMeshPro translationStatus = textGameObject.AddComponent<CurvedTextMeshPro>();
            translationStatus.font = font;
            translationStatus.fontMaterial = fontMaterial;

            var textRectTransform = (RectTransform)textGameObject.transform;
            textRectTransform.SetParent(parent, false);

            translationStatus.alignment = TextAlignmentOptions.TopLeft;
            translationStatus.lineSpacing = -35f;
            translationStatus.fontSize = 3f;
            translationStatus.fontStyle = FontStyles.Italic;

            var contentSizeFitter = textGameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return translationStatus;
        }

        [Inject]
        [UsedImplicitly]
        private void Construct(LocalizationManager localizer)
        {
            _localizationManager = localizer;
        }

        private void OnEnable()
        {
            _languageSettingsController.dropDownValueDidChangeEvent += OnSelectedLanguageChanged;
            Localization.Instance.AddOnLocalizeEvent(this);
        }

        private void OnDisable()
        {
            _languageSettingsController.dropDownValueDidChangeEvent -= OnSelectedLanguageChanged;
            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        private void OnSelectedLanguageChanged()
        {
            RefreshValues();
        }

        private void RefreshValues()
        {
            if (!_credits || !_translationStatus) return;

            int idx = _languageSettingsController._idx;
            Language language = Localization.Instance.SupportedLanguages[idx];

            if (language > Language.English)
            {
                string contributors = LocalizationImporter.GetLanguages("LANGUAGE_CONTRIBUTORS").ElementAtOrDefault((int)language);
                _credits.text = string.Format(Localization.Instance.Get("TRANSLATED_BY", language), !string.IsNullOrWhiteSpace(contributors) ? contributors : "—");

                List<TranslationStatus> statuses = _localizationManager.GetTranslationStatuses((Locale)language);
                var fullyTranslated = statuses.Where(s => s.percentTranslated == 100).Select(s => s.name).ToList();
                var partiallyTranslated = statuses.Where(s => s.percentTranslated is < 100 and > 0).Select(s => $"{s.name} ({Mathf.Clamp(s.percentTranslated, 1, 99):0}%)").ToList();
                var notSupported = statuses.Where(s => s.percentTranslated == 0).Select(s => s.name).ToList();

                _translationStatus.text = string.Empty;

                if (fullyTranslated.Count > 0)
                {
                    _translationStatus.text += string.Format(Localization.Instance.Get("TRANSLATION_STATUS_FULL", language), string.Join(", ", fullyTranslated)) + "\n";
                }

                if (partiallyTranslated.Count > 0)
                {
                    _translationStatus.text += string.Format(Localization.Instance.Get("TRANSLATION_STATUS_PARTIAL", language), string.Join(", ", partiallyTranslated)) + "\n";
                }

                if (notSupported.Count > 0)
                {
                    _translationStatus.text += string.Format(Localization.Instance.Get("TRANSLATION_STATUS_NONE", language), string.Join(", ", notSupported));
                }

                _credits.gameObject.SetActive(true);
                _translationStatus.gameObject.SetActive(true);
            }
            else
            {
                _credits.gameObject.SetActive(false);
                _translationStatus.gameObject.SetActive(false);
            }

            // I don't know why this is necessary but without it the text objects don't resize properly
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
    }
}
