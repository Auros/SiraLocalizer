using HarmonyLib;
using HMUI;
using TMPro;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(ComboUIController), nameof(ComboUIController.Start))]
    internal class ComboUIController_Start
    {
        public static void Postfix(ComboUIController __instance)
        {
            var textTransform = (RectTransform)__instance.transform.Find("ComboText");
            GameObject textObject = textTransform.gameObject;

            TextMeshProUGUI text = textObject.GetComponent<CurvedTextMeshPro>();
            text.fontStyle |= FontStyles.UpperCase;
            text.alignment = TextAlignmentOptions.Baseline;

            var offset = new Vector2(0, 6);
            textTransform.offsetMin -= offset;
            textTransform.offsetMax -= offset;

            textTransform.sizeDelta = new Vector2(180, textTransform.sizeDelta.y);
        }
    }
}
