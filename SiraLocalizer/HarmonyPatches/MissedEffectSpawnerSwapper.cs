using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using IPA.Loader;
using SiraLocalizer.UI;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
    internal static class MissedEffectSpawnerSwapper
    {
        private static readonly MethodInfo kBindMissedNoteEffectSpawnerMethod = AccessTools.DeclaredMethod(typeof(DiContainer), nameof(DiContainer.Bind), Array.Empty<Type>(), new[] { typeof(MissedNoteEffectSpawner) });
        private static readonly MethodInfo kBindToTextBasedEffectSpawnerMethod = AccessTools.DeclaredMethod(typeof(ConcreteBinderGeneric<MissedNoteEffectSpawner>), nameof(ConcreteBinderGeneric<MissedNoteEffectSpawner>.To), Array.Empty<Type>(), new[] { typeof(TextBasedMissedNoteEffectSpawner) });

        private const string kCustomMissTextId = "CustomMissText";

        // Don't overwrite the miss effect when CustomMissText is installed.
        public static bool Prepare() => PluginManager.GetPluginFromId(kCustomMissTextId) == null;

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
