using HarmonyLib;
using TMPro;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelFailedController), nameof(StandardLevelFailedController.Start))]
    internal class StandardLevelFailedController_Start : LevelFailedController_Start
    {
        public static void Postfix(LevelFailedTextEffect ____levelFailedTextEffect)
        {
            FixLineSpacing(____levelFailedTextEffect);
        }
    }

    [HarmonyPatch(typeof(MultiplayerLocalActiveLevelFailController), nameof(MultiplayerLocalActiveLevelFailController.Start))]
    internal class MultiplayerLocalActiveLevelFailController_Start : LevelFailedController_Start
    {
        public static void Postfix(LevelFailedTextEffect ____levelFailedTextEffect)
        {
            FixLineSpacing(____levelFailedTextEffect);
        }
    }

    internal class LevelFailedController_Start
    {
        protected static void FixLineSpacing(LevelFailedTextEffect levelFailedTextEffect)
        {
            Transform textTransform = levelFailedTextEffect.transform.Find("Text");
            TextMeshPro text = textTransform.GetComponent<TextMeshPro>();
            text.lineSpacing = -40;
        }
    }
}
