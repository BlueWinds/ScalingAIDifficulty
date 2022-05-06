using System;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using UnityEngine;
using HBS;
using Harmony;

namespace ScalingAIDifficulty {
    [HarmonyPatch(typeof(SGContractsWidget), "ListContracts")]
    public static class SGContractsWidget_ListContracts_Patch {
        public static void Postfix(SGContractsWidget __instance) {
            try {
                SimGameState sim = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;

                float points = 0.0f;
                if (sim.CompanyStats.ContainsStatistic("SAD_points")) {
                    points = sim.CompanyStats.GetStatistic("SAD_points").CurrentValue.Value<float>();
                }

                BaseDescriptionDef original = UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Get("ConceptContractDifficulty");
                string newDetails = $"<color=#F79B26>Scaling AI: {points}%</color>\n\n" + original.Details;

                BaseDescriptionDef descDef = new BaseDescriptionDef(original.Id, original.Name, newDetails, null);

                SGDifficultyIndicatorWidget indicator = Traverse.Create(__instance).Field("DifficultyIndicator").GetValue<SGDifficultyIndicatorWidget>();
                HBSTooltip tooltip = indicator.gameObject.GetComponent<HBSTooltip>();
                tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(descDef));
            } catch (Exception e) {
                SAD.modLog.Error?.Write(e);
            }
        }
    }
}
