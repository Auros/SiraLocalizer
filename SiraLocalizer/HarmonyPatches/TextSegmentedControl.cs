using TMPro;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable", MethodType.Normal)]
    internal static class TextMeshProUGUI_OnEnable
    {
        public static void Postfix(TextMeshProUGUI __instance)
        {
            if (__instance.alignment != TextAlignmentOptions.Midline) return;
            if (__instance.transform.parent && __instance.transform.parent.GetComponent<LayoutGroup>()) return;

            // Midline text alignment breaks if certain characters are taller than others
            // e.g. the accent on É makes it taller than regular ASCII letters
            __instance.alignment = TextAlignmentOptions.Baseline;

            // this value is eyeballed
            Vector2 offset = new Vector2(0, 0.31f * __instance.fontSize);

            __instance.rectTransform.offsetMin -= offset;
            __instance.rectTransform.offsetMax -= offset;
        }
    }
}