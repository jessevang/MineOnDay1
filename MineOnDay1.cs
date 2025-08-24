
using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;




namespace MineOnDay1
{
    //Create a class for our config file to define the properties
    public class Config
    {
        public bool TurnOnMiningOnDay1 { get; set; } = true;
        public bool TurnOnCommunityCenterOnDay1 { get; set; } = true;

        public bool TurnOnFishOnDay1 { get; set; } = true;


    }

    internal sealed class ModEntry : Mod
    {
        public uint realCurrentDay;
        public static ModEntry Instance { get; private set; }
        public Config Config { get; private set; }

        /*********
       ** Public methods
       *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this; // Assign the instance for global access
            Config = helper.ReadConfig<Config>() ?? new Config();

            helper.Events.GameLoop.DayStarted += onStartOfDay;
            helper.Events.GameLoop.DayEnding += onEndOfDay;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            if (!helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher"))
            {
                Monitor.Log("Content Patcher is not installed. Skipping patches.", LogLevel.Warn);
            }
           
            
            
            if (Instance.Config.TurnOnCommunityCenterOnDay1)
            {
                RegisterContentPatcherPack();


            }
           

        }

        private void RegisterContentPatcherPack()
        {
            string contentPackPath = Path.Combine(Helper.DirectoryPath, "assets");

            // Ensure content.json exists
            if (!File.Exists(Path.Combine(contentPackPath, "Content.json")))
            {
                Monitor.Log("ERROR: content.json not found! Content Patcher pack will not be loaded.", LogLevel.Error);
                return;
            }

            // Register the content pack as a temporary Content Patcher pack
            var contentPack = Helper.ContentPacks.CreateTemporary(
                directoryPath: contentPackPath,
                id: "Darkmushu.MineOnDay1",
                name: "MineOnDay1",
                description: "Temporary Content Patcher pack for MineOnDay1",
                author: "Darkmushu",
                version: new SemanticVersion(1, 0, 1) // SMAPI's version format
            );

            if (contentPack != null)
            {
                Monitor.Log("Content Patcher pack registered successfully!", LogLevel.Info);
            }
            else
            {
                Monitor.Log("Failed to register Content Patcher pack!", LogLevel.Error);
            }
        }



        //Code to change day 1,2,3,4 to day 5 to open up the mines.
        private void onStartOfDay(object sender, EventArgs e)
        {

            if (Config.TurnOnFishOnDay1)
            {
                this.realCurrentDay = (uint)Game1.stats.DaysPlayed;
                if (Game1.dayOfMonth == 1 && Game1.currentSeason.Equals("spring", StringComparison.OrdinalIgnoreCase) && realCurrentDay == 1)
                {
                    // Try to find & deliver Willy’s invite letter for *this* install/modpack
                    string? invite = WillyMailHelper.FindWillyInviteKey(Monitor);

                    if (invite != null && !Game1.player.hasOrWillReceiveMail(invite))
                    {
                        // Clear any bad leftovers
                        Game1.player.mailbox.Remove("willyLetter");
                        Game1.player.mailbox.Remove("willy1");
                        Game1.player.mailbox.Remove("willyIntro"); // flag, not a letter

                        Game1.player.mailbox.Add(invite);
                        Monitor.Log($"Queued Willy letter '{invite}' for today.", LogLevel.Info);
                    }
                }
            }
            

            if (Instance.Config.TurnOnMiningOnDay1)
            {
   
                this.realCurrentDay = (uint)Game1.stats.DaysPlayed;
                // Check the number of days played
                if (Game1.stats.DaysPlayed <= 5)
                {

                    Game1.stats.DaysPlayed = 5; // Set the days played to 5
                }
            }





            
        }

        //resets the day back to the original current day at the end of the day so calendar
        private void onEndOfDay(object sender, EventArgs e)
        {
            if (Instance.Config.TurnOnMiningOnDay1)
            {
                Game1.stats.DaysPlayed = this.realCurrentDay;
            }


        }


        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // 3. Get the GMCM API
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu is null)
            {
                this.Monitor.Log("Generic Mod Config Menu (GMCM) API not found. Configuration menu will not be available.", LogLevel.Warn);
                return; // GMCM isn't installed, so we can't do anything
            }

            // 4. Register your mod with GMCM
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new Config(), // Action to reset config to defaults
                save: () => this.Helper.WriteConfig(this.Config) // Action to save current config
            );

            // 5. Add options to the GMCM menu using i18n keys

            // --- Mining on Day 1 Option ---
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.mining-on-day1.name"), // Get translated name
                tooltip: () => this.Helper.Translation.Get("config.mining-on-day1.tooltip"), // Get translated tooltip
                getValue: () => this.Config.TurnOnMiningOnDay1, // How to get the current value
                setValue: value => this.Config.TurnOnMiningOnDay1 = value // How to set the value
            );

            // --- Community Center on Day 1 Option ---
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.cc-on-day1.name"), // Get translated name
                tooltip: () => this.Helper.Translation.Get("config.cc-on-day1.tooltip"), // Get translated tooltip
                getValue: () => this.Config.TurnOnCommunityCenterOnDay1, // How to get the current value
                setValue: value => this.Config.TurnOnCommunityCenterOnDay1 = value // How to set the value
            );

            // --- Mining on Day 1 Option ---
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.fishOnDay1.name"), // Get translated name
                tooltip: () => this.Helper.Translation.Get("config.fishOnDay1.tooltip"), // Get translated tooltip
                getValue: () => this.Config.TurnOnFishOnDay1, // How to get the current value
                setValue: value => this.Config.TurnOnFishOnDay1 = value // How to set the value
            );

        }



    }

}
