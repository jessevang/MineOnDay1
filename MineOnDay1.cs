using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Linq;

namespace MineOnDay1
{
    //Create a class for our config file to define the properties
    public class Config
    {
        public bool TurnOnMiningOnDay1 { get; set; } = true;
        public bool TurnOnCommunityCenterOnDay1 { get; set; } = true;
        public bool TurnOnFishOnDay1 { get; set; } = true;
        public bool TurnOnPetOnDay1 { get; set; } = true;
    }

    internal sealed class ModEntry : Mod
    {
        public uint realCurrentDay;
        public static ModEntry Instance { get; private set; }
        public Config Config { get; private set; }

        /*********
        ** Public methods
        *********/
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = helper.ReadConfig<Config>() ?? new Config();

            helper.Events.GameLoop.DayStarted += onStartOfDay;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Player.Warped += OnWarped;

            var harmony = new Harmony(ModManifest.UniqueID);

            if (Config.TurnOnMiningOnDay1)
            {
                new LevelForMore.Patches.MiningPatches.LandslidePatch().Apply(harmony, Monitor);
            }

            if (Config.TurnOnPetOnDay1)
            {
                PetEventPreconditionPatch.Apply(helper, this.Monitor, () => this.Config);
            }

            if (Config.TurnOnFishOnDay1)
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Locations.Beach), nameof(StardewValley.Locations.Beach.checkAction)),
                    prefix: new HarmonyMethod(typeof(MineOnDay1.Patches.BeachCheckActionOverridePatch),
                                              nameof(MineOnDay1.Patches.BeachCheckActionOverridePatch.Prefix))
                );
                //Monitor.Log("Applied BeachCheckActionOverridePatch to skip 'Gone Fishing' lock on Day 1.", LogLevel.Info);
            }
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (!Config.TurnOnCommunityCenterOnDay1)
                return;

            if (!Context.IsMainPlayer)
                return;

            if (e.NewLocation.Name == "Town" && e.OldLocation.Name == "BusStop")
            {
                if (Game1.dayOfMonth <= 5)
                {
                    TryStartCommunityCenterEvent();
                }
            }
        }

        private void TryStartCommunityCenterEvent()
        {
            if (Game1.currentLocation == null || Game1.currentLocation.Name != "Town")
                return;

            if (Game1.eventUp || Game1.currentLocation.currentEvent != null)
                return;

            if (Game1.player.eventsSeen.Contains("611439"))
                return;

            var townEvents = Game1.content.Load<Dictionary<string, string>>("Data/Events/Town");
            if (townEvents == null)
            {
                Monitor.Log("Could not load Town events!", LogLevel.Warn);
                return;
            }

            if (!townEvents.TryGetValue("611439/j 4/t 800 1300/w sunny/a 0 54/H", out string eventData))
            {
                eventData = townEvents.FirstOrDefault(p => p.Key.StartsWith("611439")).Value;
            }

            if (string.IsNullOrEmpty(eventData))
            {
                //Monitor.Log("Community Center event data not found in Town events.", LogLevel.Warn);
                return;
            }

            var ev = new StardewValley.Event(eventData, "Data/Events/Town", "611439", Game1.player);
            Game1.currentLocation.startEvent(ev);
            Game1.player.eventsSeen.Add("611439");

            Monitor.Log("[MineOnDay1] Triggered Community Center event manually!", LogLevel.Info);
        }

        private void onStartOfDay(object sender, EventArgs e)
        {
            if (Config.TurnOnCommunityCenterOnDay1 && !Game1.player.mailReceived.Contains("landslideDone"))
            {
                Game1.player.mailReceived.Add("landslideDone");
                //Monitor.Log("Added 'landslideDone' flag early so Community Center event can trigger on Day 1.", LogLevel.Info);
            }

            if (Config.TurnOnFishOnDay1)
            {
                if (Game1.dayOfMonth == 1)
                {
                    NPC willy = Game1.getCharacterFromName("Willy");
                    if (willy != null)
                    {
                        willy.currentLocation = Game1.getLocationFromName("FishShop");
                        willy.setTileLocation(new Microsoft.Xna.Framework.Vector2(5f, 4f));
                        willy.ignoreScheduleToday = true;
                        willy.IsInvisible = false;
                        willy.Sprite.faceDirection(2);
                        Game1.getLocationFromName("FishShop").characters.Add(willy);

                        //Monitor.Log("[MineOnDay1] Forced Willy to appear at Fish Shop counter on Day 1.", LogLevel.Info);
                    }
                }
            }

            if (Config.TurnOnFishOnDay1)
            {
                string? invite = WillyMailHelper.FindWillyInviteKey(Monitor);

                if (invite != null && !Game1.player.hasOrWillReceiveMail(invite))
                {
                    Game1.player.mailbox.Remove("willyLetter");
                    Game1.player.mailbox.Remove("willy1");
                    Game1.player.mailbox.Remove("willyIntro");
                    Game1.player.mailbox.Add(invite);
                    //Monitor.Log($"Queued Willy letter '{invite}' for today.", LogLevel.Info);
                }

                if (!Game1.player.mailReceived.Contains("willyLetter"))
                    Game1.player.mailReceived.Add("willyLetter");

                if (!Game1.player.mailReceived.Contains("willy1"))
                    Game1.player.mailReceived.Add("willy1");

                GameLocation fishShop = Game1.getLocationFromName("FishShop");
                if (fishShop != null)
                {
                    fishShop.modData["MineOnDay1.WillyShopUnlocked"] = "true";
                    //Monitor.Log("Unlocked Willy's shop door for Day 1.", LogLevel.Info);
                }
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu is null)
            {
                this.Monitor.Log("Generic Mod Config Menu (GMCM) API not found.", LogLevel.Warn);
                return;
            }

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new Config(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.mining-on-day1.name"),
                tooltip: () => this.Helper.Translation.Get("config.mining-on-day1.tooltip"),
                getValue: () => this.Config.TurnOnMiningOnDay1,
                setValue: value => this.Config.TurnOnMiningOnDay1 = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.cc-on-day1.name"),
                tooltip: () => this.Helper.Translation.Get("config.cc-on-day1.tooltip"),
                getValue: () => this.Config.TurnOnCommunityCenterOnDay1,
                setValue: value => this.Config.TurnOnCommunityCenterOnDay1 = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.fishOnDay1.name"),
                tooltip: () => this.Helper.Translation.Get("config.fishOnDay1.tooltip"),
                getValue: () => this.Config.TurnOnFishOnDay1,
                setValue: value => this.Config.TurnOnFishOnDay1 = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.pet-on-day1.name"),
                tooltip: () => this.Helper.Translation.Get("config.pet-on-day1.tooltip"),
                getValue: () => this.Config.TurnOnPetOnDay1,
                setValue: value => this.Config.TurnOnPetOnDay1 = value
            );
        }
    }
}
