using HarmonyLib;
using HMUI;
using IPA.Utilities;
using Polyglot;
using TMPro;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(ComboUIController), nameof(ComboUIController.Start))]
    internal class ComboUIController_Start
    {
        public static void Postfix(ComboUIController __instance)
        {
            RectTransform textTransform = __instance.transform.Find("ComboText") as RectTransform;

            if (!textTransform) return;

            GameObject textObject = textTransform.gameObject;

            bool wasActive = textObject.activeSelf;
            textObject.SetActive(false);

            TextMeshProUGUI text = textObject.GetComponent<CurvedTextMeshPro>();
            text.fontStyle |= FontStyles.UpperCase;
            text.alignment = TextAlignmentOptions.Baseline;

            var offset = new Vector2(0, 6);
            textTransform.offsetMin -= offset;
            textTransform.offsetMax -= offset;

            textTransform.sizeDelta = new Vector2(180, textTransform.sizeDelta.y);

            LocalizedTextMeshProUGUI localizedText = textObject.AddComponent<LocalizedTextMeshProUGUI>();
            localizedText.SetField<LocalizedTextComponent<TextMeshProUGUI>, TextMeshProUGUI>("localizedComponent", text);
            localizedText.Key = "LABEL_COMBO";

            textObject.SetActive(wasActive);
        }
    }
}
