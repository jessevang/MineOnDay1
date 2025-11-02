using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MineOnDay1.Patches
{
    [HarmonyPatch(typeof(Beach), nameof(Beach.checkAction))]
    internal static class BeachCheckActionOverridePatch
    {
        public static bool Prefix(
            Beach __instance,
            Location tileLocation,
            Rectangle viewport,
            Farmer who,
            ref bool __result)
        {
            // only apply if main player and Day 1
            if (!Context.IsMainPlayer || !ModEntry.Instance.Config.TurnOnFishOnDay1)
                return true; // let vanilla run

            if (Game1.Date.TotalDays >= 1)
                return true; // only override on Day 1

            // replicate switch but skip Gone Fishing message
            switch (__instance.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet"))
            {
                case 284:
                    if (who.Items.ContainsId("(O)388", 300))
                    {
                        __instance.createQuestionDialogue(
                            Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Question"),
                            __instance.createYesNoResponses(),
                            "BeachBridge");
                    }
                    else
                    {
                        Game1.drawObjectDialogue(
                            Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Hint"));
                    }
                    __result = true;
                    return false;

                case 496:
                    // skip “Gone Fishing” message and just allow entry
                    Game1.playSound("doorClose");
                    Game1.warpFarmer("FishShop", 5, 8, flip: false); // correct front entrance
                    //ModEntry.Instance.Monitor.Log("[MineOnDay1] Overrode Day 1 Beach door to allow Fish Shop entry.", LogLevel.Info);
                    __result = true;
                    return false;
            }

            // default to vanilla for all else
            return true;
        }
    }
}
