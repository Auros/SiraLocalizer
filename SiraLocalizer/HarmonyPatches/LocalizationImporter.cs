using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Polyglot;

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
            var instructionsList = instructions.ToList();
            instructionsList.RemoveRange(48, 56);
            return instructionsList.AsEnumerable();
        }
    }
}
