
using StardewModdingAPI;
using StardewValley;




namespace MineOnDay1
{
    //Create a class for our config file to define the properties
    public class Config
    {
        public bool TurnOnMiningOnDay1 { get; set; } = true;
        public bool TurnOnCommunityCenterOnDay1 { get; set; } = true;


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

            if (Instance.Config.TurnOnCommunityCenterOnDay1)
            {
                if (!helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher"))
                {
                    Monitor.Log("Content Patcher is not installed. Skipping patches.", LogLevel.Warn);
                }
                else
                {
                    RegisterContentPatcherPack();
                }


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
                version: new SemanticVersion(1, 0, 0) // SMAPI's version format
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
            if (!Instance.Config.TurnOnMiningOnDay1)
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
            if (!Instance.Config.TurnOnMiningOnDay1)
            {
                Game1.stats.DaysPlayed = this.realCurrentDay;
            }


        }




    }

}
