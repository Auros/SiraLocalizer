using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RTLTMPro;
using TMPro;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(TMP_Text))]
    internal class TMP_Text_RTL
    {
        private static readonly FieldInfo kTextField = AccessTools.DeclaredField(typeof(TMP_Text), nameof(TMP_Text.m_text));
        private static readonly MethodInfo kFixRTLMethod = AccessTools.DeclaredMethod(typeof(TMP_Text_RTL), nameof(FixRTL));

        private static readonly FastStringBuilder kStringBuilder = new(RTLSupport.DefaultBufferSize);

        [HarmonyPatch(nameof(TMP_Text.ParseInputText))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ParseInputText(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Ldfld, kTextField))
                .Repeat(cm => cm.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Call, kFixRTLMethod)))
                .Instructions();
        }

        private static string FixRTL(string text, TMP_Text instance)
        {
            bool isRightToLeft = text != null && TextUtils.IsRTLInput(text);
            instance.m_isRightToLeft = isRightToLeft;

            if (!isRightToLeft)
            {
                return text;
            }

            kStringBuilder.Clear();
            RTLSupport.FixRTL(text, kStringBuilder, true, true, true);
            kStringBuilder.Reverse();
            return kStringBuilder.ToString();
        }
    }
}
