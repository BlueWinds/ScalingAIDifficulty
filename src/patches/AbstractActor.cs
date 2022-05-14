using System;
using System.Reflection;
using System.Collections.Generic;
using BattleTech;
using Harmony;
using HBS;

namespace ScalingAIDifficulty {
    [HarmonyPatch(typeof(AbstractActor), "AddToTeam")]
    public static class AbstractActor_AddToTeam_Patch {
        public static void Postfix(AbstractActor __instance) {
            try {
                Settings s = SAD.settings;
                SimGameState sim = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;
                string overrideID = __instance.Combat.ActiveContract.Override.ID;

                if (s.IgnoreContracts.Contains(overrideID)) {
                  SAD.modLog.Debug?.Write($"Contract {overrideID} is in IgnoreContracts. Not applying effects to {__instance.UnitName}.");
                  return;
                }

                float basePoints = sim.CompanyStats.GetValue<float>("SAD_points");
                float pointsMultiplier = (float)sim.Constants.Story.ArgoMechTechs / 100.0f;

                float points = basePoints * pointsMultiplier;

                if (s.ContractDifficulty.ContainsKey(overrideID)) {
                    SAD.modLog.Debug?.Write($"Adding {s.ContractDifficulty[overrideID]} points to {overrideID} because of ContractDifficulty.");
                    points += s.ContractDifficulty[overrideID];
                }

                if (points == 0) {
                    SAD.modLog.Debug?.Write($"0 SAD_points, no effects applied to {__instance.UnitName}. basePoints: {basePoints}, pointsMultiplier: {pointsMultiplier}");
                    return;
                }

                if (__instance.team.IsLocalPlayer) {
                    SAD.modLog.Debug?.Write($"Applying SAD stats for {__instance.UnitName} on player team ({points} points from basePoints: {basePoints}, pointsMultiplier: {pointsMultiplier})");
                    foreach (PointEffect effect in s.SelfEffectsPerPoint) {
                        applyStatEffect(__instance, effect, points);
                    }
                } else if (__instance.team.IsEnemy(__instance.Combat.LocalPlayerTeam)) {
                    SAD.modLog.Debug?.Write($"Applying SAD stats for {__instance.UnitName} on enemy team ({points} points from basePoints: {basePoints}, pointsMultiplier: {pointsMultiplier})");
                    foreach (PointEffect effect in s.EnemyEffectsPerPoint) {
                        applyStatEffect(__instance, effect, points);
                    }
                } else {
                    SAD.modLog.Debug?.Write($"{__instance.UnitName} spawned, but was neither player nor enemy");
                }
            } catch (Exception e) {
                SAD.modLog.Error?.Write(e);
            }
        }

        public static void applyStatEffect(AbstractActor __instance, PointEffect effect, float points) {
            Statistic stat = __instance.StatCollection.GetStatistic(effect.statName);

            float value = 0;
            if (effect.operation == StatCollection.StatOperation.Float_Add) {
                value = effect.modValue * points;
                __instance.StatCollection.Float_Add(stat, value);
            } else if (effect.operation == StatCollection.StatOperation.Float_Multiply) {
                value = (float)Math.Pow(effect.modValue, points);
                __instance.StatCollection.Float_Multiply(stat, value);
            } else {
                SAD.modLog.Info?.Write($"Invalid operation.");
            }

            SAD.modLog.Trace?.Write($"    {effect.statName}: {effect.operation} {effect.modValue}; value {value}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "HandleDeath")]
    public static class AbstractActor_HandleDeath_Patch {
        public static void Postfix(AbstractActor __instance) {
            try {
                if (__instance.team.IsLocalPlayer) {
                    if (__instance.StatCollection.GetValue<bool>("CUFakeVehicle")) {
                        SAD.modLog.Debug?.Write($"{__instance.UnitName} (vehicle) destroyed on player team.");
                        Contract_CompleteContract_Patch.vehicleDestroyedThisContract = true;
                    } else if (__instance.StatCollection.GetValue<bool>("CUTrooperSquad")) {
                        SAD.modLog.Debug?.Write($"{__instance.UnitName} (battle armor) destroyed on player team.");
                        Contract_CompleteContract_Patch.baDestroyedThisContract = true;
                    } else {
                        SAD.modLog.Debug?.Write($"{__instance.UnitName} (mech) destroyed on player team.");
                        Contract_CompleteContract_Patch.mechDestroyedThisContract = true;
                    }
                }
            } catch (Exception e) {
                SAD.modLog.Error?.Write(e);
            }
        }
    }
}
