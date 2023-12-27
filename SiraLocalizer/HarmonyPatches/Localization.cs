using HarmonyLib;
using Polyglot;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(Localization), nameof(Localization.SelectedLanguage), MethodType.Setter)]
    internal class Localization_SelectedLanguage
    {
        private static readonly MethodInfo kLogWarningMethod = typeof(Debug).GetMethod(nameof(Debug.LogWarning), BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Standard, new[] { typeof(object) }, null);

        /// <summary>
        /// Replaces the <see cref="Language"/> passed to the <see cref="Debug.LogWarning"/> call with a cast to <see cref="Locale"/> so our languages also show up.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Box && typeof(Language).Equals(instruction.operand))
                {
                    yield return new CodeInstruction(OpCodes.Box, typeof(Locale));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
