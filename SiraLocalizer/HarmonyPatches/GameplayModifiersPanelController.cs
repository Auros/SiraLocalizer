using HarmonyLib;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayModifiersPanelController), nameof(GameplayModifiersPanelController.Awake))]
    internal class GameplayModifiersPanelController_Awake
    {
        public static void Postfix(GameplayModifiersPanelController __instance)
        {
            RectTransform rectTransform = (RectTransform)__instance.transform.Find("Info").transform;
            rectTransform.sizeDelta = new Vector2(100, rectTransform.sizeDelta.y);
        }
    }
}
