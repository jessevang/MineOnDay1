// PetEventPreconditionPatch.cs
using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MineOnDay1
{
    /// <summary>
    /// Removes preconditions from the vanilla pet adoption event (1590166) by
    /// relocating its script under key "1590166" with no precondition suffix.
    /// We DO NOT change the script text; we only change the key.
    /// </summary>
    internal static class PetEventPreconditionPatch
    {
        private static IMonitor Monitor = null!;
        private static IModHelper Helper = null!;
        private static Func<Config> GetConfig = null!;

        private const string TargetAsset = "Data/Events/Farm";
        private const string CleanKey = "1590166";

        public static void Apply(IModHelper helper, IMonitor monitor, Func<Config> getConfig)
        {
            Helper = helper;
            Monitor = monitor;
            GetConfig = getConfig;

            helper.Events.Content.AssetRequested += OnAssetRequested;
            //monitor.Log("PetEventPreconditionPatch: hooked AssetRequested.", LogLevel.Trace);
        }

        private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (!GetConfig().TurnOnPetOnDay1)
                return;

            if (!e.NameWithoutLocale.IsEquivalentTo(TargetAsset))
                return;

            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                var oldKey = data.Keys
                    .FirstOrDefault(k => k.StartsWith(CleanKey + "/", StringComparison.Ordinal));

                if (oldKey == null)
                {
                    // Only log if something is missing — otherwise stay quiet
                    if (!data.ContainsKey(CleanKey))
                    {
                        Monitor.Log("Couldn’t find a 1590166 entry with preconditions to relax.", LogLevel.Warn);
                    }
                    return;
                }

                var script = data[oldKey];
                data[CleanKey] = script;
                data.Remove(oldKey);

                //Monitor.Log($"Relaxed preconditions for pet event: moved '{oldKey}' -> '{CleanKey}'.", LogLevel.Info);
            }, AssetEditPriority.Late);
        }
    }
}
