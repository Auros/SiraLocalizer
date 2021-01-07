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
            Transform textTransform = __instance.transform.Find("ComboText");

            if (!textTransform) return;

            GameObject textObject = textTransform.gameObject;

            bool wasActive = textObject.activeSelf;
            textObject.SetActive(false);

            TextMeshProUGUI text = textObject.GetComponent<CurvedTextMeshPro>();
            text.fontStyle |= FontStyles.UpperCase;

            LocalizedTextMeshProUGUI localizedText = textObject.AddComponent<LocalizedTextMeshProUGUI>();
            localizedText.SetField<LocalizedTextComponent<TextMeshProUGUI>, TextMeshProUGUI>("localizedComponent", text);
            localizedText.Key = "LABEL_COMBO";

            textObject.SetActive(wasActive);
        }
    }
}
