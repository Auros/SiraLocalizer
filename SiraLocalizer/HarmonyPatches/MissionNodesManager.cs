using HarmonyLib;
using IPA.Utilities;
using System;
using System.Linq;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionNodesManager), "GetAllMissionNodes", new Type[0])]
    internal class MissionNodesManager_GetAllMissionNodes
    {
        private static readonly FieldAccessor<MissionObjectiveTypeSO, string>.Accessor kObjectiveNameAccessor = FieldAccessor<MissionObjectiveTypeSO, string>.GetAccessor("_objectiveName");

        public static void Postfix(MissionNode[] ____allMissionNodes)
        {
            foreach (MissionObjectiveTypeSO objectiveType in ____allMissionNodes.SelectMany(n => n.missionData.missionObjectives).Select(o => o.type).Distinct())
            {
                MissionObjectiveTypeSO objectiveTypeRef = objectiveType;

                switch (objectiveType.name)
                {
                    case "MissMissionObjectiveType":
                        kObjectiveNameAccessor(ref objectiveTypeRef) = "OBJECTIVE_MISS";
                        break;

                    case "ComboMissionObjectiveType":
                        kObjectiveNameAccessor(ref objectiveTypeRef) = "OBJECTIVE_COMBO";
                        break;

                    case "HandsMovementMissionObjectiveType":
                        kObjectiveNameAccessor(ref objectiveTypeRef) = "OBJECTIVE_HANDS_MOVEMENT";
                        break;
                }
            }
        }
    }
}
