using System;
using BattleTech;
using BattleTech.UI;
using UnityEngine;
using HBS;
using Harmony;

namespace ScalingAIDifficulty {
    [HarmonyPatch(typeof(SimGameState), "OnCareerModeStart")]
    public static class SimGameState_OnCareerModeStart_Patch {
        public static void Postfix(SimGameState __instance) {
            try {
                float points = (float)__instance.Constants.Story.ArgoMedTechs;

                SimGameStat stat = new SimGameStat("SAD_points", points);
                SimGameState.SetSimGameStat(stat, __instance.CompanyStats);

                SAD.modLog.Debug?.Write($"Set initial SAD_points to {points} based on ArgoMedTechs");
            } catch (Exception e) {
                SAD.modLog.Error?.Write(e);
            }
        }
    }
}
