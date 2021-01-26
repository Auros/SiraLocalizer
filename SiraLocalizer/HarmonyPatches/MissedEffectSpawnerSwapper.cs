using System;
using Zenject;
using HarmonyLib;
using System.Linq;
using IPA.Utilities;
using SiraLocalizer.UI;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
    internal class MissedEffectSpawnerSwapper
    {
        private static readonly MethodInfo _rootMethod = typeof(DiContainer).GetMethod("Bind", Array.Empty<Type>());
        private static readonly MethodInfo _ourMissAttacher = SymbolExtensions.GetMethodInfo(() => OurMissAttacher(null));
        private static readonly MethodInfo _originalMethod = _rootMethod.MakeGenericMethod(new Type[] { typeof(MissedNoteEffectSpawner) });
        private static readonly FieldInfo _missedSpawner = typeof(GameplayCoreInstaller).GetField("_missedNoteEffectSpawnerPrefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(MissedNoteEffectSpawner ____missedNoteEffectSpawnerPrefab)
        {
            var oldComponent = ____missedNoteEffectSpawnerPrefab.gameObject.GetComponent<TextBasedMissedNoteEffectSpawner>();
            if (oldComponent != null)
            {
                UnityEngine.Object.DestroyImmediate(oldComponent);
            }
            var textBased = ____missedNoteEffectSpawnerPrefab.gameObject.AddComponent<TextBasedMissedNoteEffectSpawner>();
            textBased.SetField<MissedNoteEffectSpawner, FlyingSpriteSpawner>("_missedNoteFlyingSpriteSpawner", ____missedNoteEffectSpawnerPrefab.GetField<FlyingSpriteSpawner, MissedNoteEffectSpawner>("_missedNoteFlyingSpriteSpawner"));
            textBased.transform.position = ____missedNoteEffectSpawnerPrefab.transform.position;
            textBased.enabled = false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand == null)
                    continue;
                
                if (codes[i].Calls(_originalMethod))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Callvirt, _ourMissAttacher));
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        private static FromBinderGeneric<TextBasedMissedNoteEffectSpawner> OurMissAttacher(ConcreteIdBinderGeneric<MissedNoteEffectSpawner> contract)
        {
            return contract.To<TextBasedMissedNoteEffectSpawner>();
        }
    }
}