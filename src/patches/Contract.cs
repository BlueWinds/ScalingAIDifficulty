using System;
using System.Linq;
using Harmony;
using HBS;
using BattleTech;

namespace ScalingAIDifficulty {
    [HarmonyPatch(typeof(Contract), "CompleteContract")]
    public static class Contract_CompleteContract_Patch {
        // This can be set to true in AbstractActor.cs, AbstractActor_HandleDeath_Patch
        // We use our own flag rather than Contract.PlayerUnitResults because we want to track units that were destroyed
        // but recovered.
        public static bool unitDestroyedThisContract = false;

        public static void Postfix(Contract __instance, MissionResult result, bool isGoodFaithEffort) {
            try {
                Settings s = SAD.settings;
                SimGameState sim = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;
                SAD.modLog.Debug?.Write($"Contract complete: {__instance.Name}, override: {__instance.Override.ID}");

                float basePointChangeMultiplier = (float)sim.Constants.Story.ArgoMechTechs / 100.0f;
                float basePointChange = 0;

                if (result == MissionResult.Victory) {
                  basePointChange = s.points.victory;
                } else if (result == MissionResult.Retreat) {
                  basePointChange = s.points.retreat;
                } else if (result == MissionResult.Defeat) {
                  basePointChange = s.points.defeat;
                }

                bool pilotInjured = __instance.PlayerUnitResults.Any(ur => ur.pilot.pilotDef.RecentInjuryDamageType != DamageType.NOT_SET);
                if (pilotInjured && s.points.pilotInjured < basePointChange) {
                  basePointChange = s.points.pilotInjured;
                }

                bool pilotKilled = __instance.KilledPilots.Count > 0;
                if (pilotKilled && s.points.pilotKilled < basePointChange) {
                  basePointChange = s.points.pilotKilled;
                }

                if (unitDestroyedThisContract && s.points.unitDestroyed < basePointChange) {
                  basePointChange = s.points.unitDestroyed;
                }

                SAD.modLog.Debug?.Write($"MissionResult: {result}, pilotInjured: {pilotInjured}, pilotKilled: {pilotKilled}, unitDestroyedThisContract: {unitDestroyedThisContract}");

                 float newPoints = basePointChange * basePointChangeMultiplier;
                if (sim.CompanyStats.ContainsStatistic("SAD_points")) {
                    newPoints += sim.CompanyStats.GetStatistic("SAD_points").CurrentValue.Value<float>();
                }

                newPoints = Math.Max(Math.Min(newPoints, s.maxPoints), s.minPoints);
                SimGameStat stat = new SimGameStat("SAD_points", newPoints, true);
                SimGameState.SetSimGameStat(stat, sim.CompanyStats);

                SAD.modLog.Debug?.Write($"basePointChange: {basePointChange}, basePointChangeMultiplier (story.ArgoMechTechs / 100): {basePointChangeMultiplier}, total SAD_points: {sim.CompanyStats.GetValue<float>("SAD_points")} (clamped between {s.minPoints}, {s.maxPoints})");
            }
            catch (Exception e) {
                SAD.modLog.Error?.Write(e);
            }

            unitDestroyedThisContract = false;
        }
    }
}
