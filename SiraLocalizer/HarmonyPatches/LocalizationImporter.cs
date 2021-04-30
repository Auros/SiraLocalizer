using HarmonyLib;
using Polyglot;
using System.Collections.Generic;
using System.Linq;

namespace SiraLocalizer.HarmonyPatches
{
    /// <summary>
    /// This patch gets rid of the debug code added to <see cref="LocalizationImporter"/>'s ImportTextFile.
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
}
