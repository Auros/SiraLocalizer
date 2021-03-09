using HarmonyLib;
using IPA.Utilities;
using Polyglot;
using TMPro;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelFailedController), "Start")]
    internal class StandardLevelFailedController_Start : LevelFailedController_Start
    {
        public static void Postfix(LevelFailedTextEffect ____levelFailedTextEffect)
        {
            AddLocalizedText(____levelFailedTextEffect);
        }
    }

    [HarmonyPatch(typeof(MissionLevelFailedController), "Start")]
    internal class MissionLevelFailedController_Start : LevelFailedController_Start
    {
        public static void Postfix(LevelFailedTextEffect ____levelFailedTextEffect)
        {
            AddLocalizedText(____levelFailedTextEffect);
        }
    }

    [HarmonyPatch(typeof(MultiplayerLocalActiveLevelFailController), "Start")]
    internal class MultiplayerLocalActiveLevelFailController_Start : LevelFailedController_Start
    {
        public static void Postfix(LevelFailedTextEffect ____levelFailedTextEffect)
        {
            AddLocalizedText(____levelFailedTextEffect);
        }
    }

    internal class LevelFailedController_Start
    {
        protected static void AddLocalizedText(LevelFailedTextEffect levelFailedTextEffect)
        {
            bool wasActive = levelFailedTextEffect.gameObject.activeSelf;
            levelFailedTextEffect.gameObject.SetActive(false);

            RectTransform transform = (RectTransform)levelFailedTextEffect.transform;
            transform.sizeDelta = new Vector2(12, transform.sizeDelta.y);

            Transform textTransform = levelFailedTextEffect.transform.Find("Text");

            if (!textTransform)
            {
                Plugin.Log.Error($"Could not find 'Text' transform on '{levelFailedTextEffect}'");
                return;
            }

            TextMeshPro text = textTransform.GetComponent<TextMeshPro>();
            text.fontStyle |= FontStyles.UpperCase;
            text.lineSpacing = -40;

            LocalizedTextMeshPro localizedText = textTransform.gameObject.AddComponent<LocalizedTextMeshPro>();
            localizedText.SetField<LocalizedTextComponent<TextMeshPro>, TextMeshPro>("localizedComponent", text);
            localizedText.Key = "LEVEL_FAILED";

            levelFailedTextEffect.gameObject.SetActive(wasActive);
        }
    }
}
