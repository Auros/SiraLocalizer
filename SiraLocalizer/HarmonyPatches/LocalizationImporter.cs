using HarmonyLib;
using Polyglot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiraLocalizer.HarmonyPatches
{
    /// <summary>
    /// This patch gets rid of the debug code added to <see cref="LocalizationImporter"/>'s ImportTextFile (i.e. the addition of <see cref="Language.Debug_Keys"/>, <see cref="Language.Debug_English_Reverted"/>, and <see cref="Language.Debug_Word_With_Max_Lenght"/>).
    /// </summary>
    [HarmonyPatch(typeof(LocalizationImporter), "ImportTextFile")]
    internal static class LocalizationImporter_GetLanguages
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            instructionsList.RemoveRange(48, 56);
            return instructionsList.AsEnumerable();
        }
    }

    /// <summary>
    /// This patch refreshes the supported languages via <see cref="Localizer.UpdateSupportedLanguages"/> after a call to <see cref="LocalizationImporter"/>'s Initialize.
    /// </summary>
    [HarmonyPatch(typeof(LocalizationImporter), "Initialize")]
    internal static class LocalizationImporter_Initialize
    {
        public static event Action PreInitialize;
        public static event Action PostInitialize;

        public static void Prefix()
        {
            PreInitialize?.Invoke();
        }

        public static void Postfix()
        {
            PostInitialize?.Invoke();
        }
    }
}
