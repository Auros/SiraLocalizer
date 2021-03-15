using HarmonyLib;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayModifierToggle), nameof(GameplayModifierToggle.Start))]
    public static class GameplayModifierToggle_Start
    {
        public static void Postfix(GameplayModifierToggle __instance)
        {
            RectTransform rectTransform = (RectTransform)__instance.transform.Find("Name");
            rectTransform.sizeDelta += new Vector2(0, 0.1f);
        }
    }
}
