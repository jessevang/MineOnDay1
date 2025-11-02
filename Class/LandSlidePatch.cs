using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace LevelForMore.Patches.MiningPatches
{
    internal class LandslidePatch
    {
        public void Apply(Harmony harmony, IMonitor monitor)
        {
            var resetSharedState = AccessTools.Method(typeof(Mountain), "resetSharedState");
            if (resetSharedState != null)
            {
                harmony.Patch(resetSharedState,
                    postfix: new HarmonyMethod(typeof(LandslidePatch), nameof(ForceClearLandslide_Mountain)));
            }
            else
            {
                monitor.Log("Failed to find Mountain.resetSharedState", LogLevel.Error);
            }

            var dayUpdate = AccessTools.Method(typeof(Mountain), nameof(Mountain.DayUpdate));
            if (dayUpdate != null)
            {
                harmony.Patch(dayUpdate,
                    postfix: new HarmonyMethod(typeof(LandslidePatch), nameof(ForceClearLandslide_Mountain)));
            }
            else
            {
                monitor.Log("Failed to find Mountain.DayUpdate(int)", LogLevel.Error);
            }

            var resetForPlayerEntryBase = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry));
            if (resetForPlayerEntryBase != null)
            {
                harmony.Patch(resetForPlayerEntryBase,
                    postfix: new HarmonyMethod(typeof(LandslidePatch), nameof(ForceClearLandslide_GameLocation)));
            }
            else
            {
                monitor.Log("Failed to find GameLocation.resetForPlayerEntry()", LogLevel.Error);
            }

            //monitor.Log("[MineOnDay1] Applied LandslidePatch successfully.", LogLevel.Info);
        }

        private static void ForceClearLandslide_Mountain(Mountain __instance)
            => ClearLandslide(__instance);

        private static void ForceClearLandslide_GameLocation(GameLocation __instance)
        {
            if (__instance is Mountain m)
                ClearLandslide(m);
        }

        private static void ClearLandslide(Mountain mountain)
        {
            if (mountain == null || !Context.IsMainPlayer)
                return;

            var f = AccessTools.Field(typeof(Mountain), "landslide");
            if (f?.GetValue(mountain) is NetBool nb)
                nb.Value = false;

            if (!Game1.player.mailReceived.Contains("landslideDone"))
                Game1.player.mailReceived.Add("landslideDone");

            //Monitor.Log("[MineOnDay1] Cleared landslide early for mine access.", LogLevel.Info);
        }
    }
}
