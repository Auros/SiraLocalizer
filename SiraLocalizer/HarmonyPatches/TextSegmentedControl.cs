using HMUI;
using TMPro;
using HarmonyLib;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(TextSegmentedControl), "InstantiateCell", MethodType.Normal)]
    internal static class TextSegmentedControl_InstantiateCell
    {
        public static void Postfix(TextSegmentedControlCell __result)
        {
            CurvedTextMeshPro text = __result.transform.Find("Text").GetComponent<CurvedTextMeshPro>();

            if (text.alignment != TextAlignmentOptions.Midline) return;

            // Midline text alignment breaks if certain characters are taller than others
            // e.g. the accent on É makes it taller than regular ASCII letters
            text.alignment = TextAlignmentOptions.Baseline;

            // this value is eyeballed
            Vector2 offset = new Vector2(0, 0.3f * text.fontSize);

            text.rectTransform.offsetMin -= offset;
            text.rectTransform.offsetMax -= offset;
        }
    }
}