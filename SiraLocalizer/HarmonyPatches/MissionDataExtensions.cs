using HarmonyLib;
using Polyglot;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionDataExtensions), "Name", MethodType.Normal)]
    internal class MissionDataExtensions_Name
    {
        public static bool Prefix(MissionObjective.ReferenceValueComparisonType comparisonType, ref string __result)
        {
            __result = string.Empty;

            switch (comparisonType)
            {
                case MissionObjective.ReferenceValueComparisonType.Min:
                    __result = Localization.Get("OBJECTIVE_COMPARISON_MINIMUM");
                    break;

                case MissionObjective.ReferenceValueComparisonType.Max:
                    __result = Localization.Get("OBJECTIVE_COMPARISON_MAXIMUM");
                    break;
            }

            return false;
        }
    }
}
