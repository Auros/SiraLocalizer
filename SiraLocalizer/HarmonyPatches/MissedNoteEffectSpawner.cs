using HarmonyLib;
using SiraLocalizer.UI;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(MissedNoteEffectSpawner), nameof(MissedNoteEffectSpawner.HandleNoteWasMissed))]
    internal static class MissedNoteEffectSpawner_HandleNoteWasMissed
    {
        [HarmonyPriority(Priority.Last)]
        public static void Prefix(MissedNoteEffectSpawner __instance, NoteController noteController, ref bool __runOriginal)
        {
            if (!__runOriginal || __instance is not TextBasedMissedNoteEffectSpawner textBasedMissedNoteEffectSpawner)
            {
                return;
            }

            textBasedMissedNoteEffectSpawner.HandleNoteWasMissed(noteController);
            __runOriginal = false;
        }
    }
}
