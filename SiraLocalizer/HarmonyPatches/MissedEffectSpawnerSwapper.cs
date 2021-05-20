using System;
using Zenject;
using HarmonyLib;
using SiraLocalizer.UI;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
    internal static class MissedEffectSpawnerSwapper
    {
        private static readonly MethodInfo kBindMissedNoteEffectSpawnerMethod = typeof(DiContainer).GetMethod("Bind", Array.Empty<Type>()).MakeGenericMethod(new[] { typeof(MissedNoteEffectSpawner) });
        private static readonly MethodInfo kBindToTextBasedEffectSpawnerMethod = typeof(ConcreteBinderGeneric<MissedNoteEffectSpawner>).GetMethod("To", Array.Empty<Type>()).MakeGenericMethod(new[] { typeof(TextBasedMissedNoteEffectSpawner) });

        public static void Prefix(MissedNoteEffectSpawner ____missedNoteEffectSpawnerPrefab)
        {
            GameObject gameObject = ____missedNoteEffectSpawnerPrefab.gameObject;

            // we can't destroy original MissedNoteEffectSpawner since it kills the reference given through [SerializeField]
            gameObject.GetComponent<MissedNoteEffectSpawner>().enabled = false;
            gameObject.transform.Find("MissedNoteFlyingSpriteSpawner").gameObject.SetActive(false);

            var textSpawner = gameObject.GetComponent<ItalicizedFlyingTextSpawner>();
            var effectSpawner = gameObject.GetComponent<TextBasedMissedNoteEffectSpawner>();

            if (!textSpawner)
            {
                gameObject.AddComponent<ItalicizedFlyingTextSpawner>();
            }

            if (!effectSpawner)
            {
                gameObject.AddComponent<TextBasedMissedNoteEffectSpawner>();
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction codeInstruction in instructions)
            {
                yield return codeInstruction;

                if (codeInstruction.operand != null && codeInstruction.Calls(kBindMissedNoteEffectSpawnerMethod))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, kBindToTextBasedEffectSpawnerMethod);
                }
            }
        }
    }
}
