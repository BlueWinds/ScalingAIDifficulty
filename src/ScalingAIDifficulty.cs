using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Harmony;
using IRBTModUtils.Logging;
using BattleTech;
using UnityEngine;

namespace ScalingAIDifficulty {
    public class SAD {
        internal static DeferringLogger modLog;
        internal static string modDir;
        internal static Settings settings;

        public static void Init(string modDirectory, string settingsJSON) {
            modDir = modDirectory;

            try {
                using (StreamReader reader = new StreamReader($"{modDir}/settings.json")) {
                    string jdata = reader.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(jdata);
                }
                modLog = new DeferringLogger(modDirectory, "ScalingAIDifficulty", "SAD", settings.debug, settings.trace);
                modLog.Debug?.Write($"Loaded settings from {modDir}/settings.json. Version {typeof(Settings).Assembly.GetName().Version}");
            }

            catch (Exception e) {
                settings = new Settings();
                modLog = new DeferringLogger(modDir, "ScalingAIDifficulty", "SAD", true, true);
                modLog.Error?.Write(e);
            }

            var harmony = HarmonyInstance.Create("blue.winds.ScalingAIDifficulty");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
