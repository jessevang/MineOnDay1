using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

public static class WillyMailHelper
{
    /// <summary>Logs every mail key that looks Willy-related and returns them.</summary>
    public static List<string> ProbeWillyMailKeys(IMonitor monitor)
    {
        var mail = Game1.content.Load<Dictionary<string, string>>("Data\\Mail");
        var results =
            mail.Where(kv =>
                        kv.Key.IndexOf("willy", StringComparison.OrdinalIgnoreCase) >= 0
                        || kv.Value.IndexOf("Willy", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(kv => kv.Key)
                .OrderBy(k => k)
                .ToList();

        monitor.Log($"[Probe] Found {results.Count} Willy-like mail keys: {string.Join(", ", results)}", LogLevel.Info);
        return results;
    }

    /// <summary>
    /// Best-effort guess for the “come to the beach” invite letter.
    /// Picks the first key that looks like an early invite among Willy-related keys.
    /// </summary>
    public static string? FindWillyInviteKey(IMonitor monitor)
    {
        var mail = Game1.content.Load<Dictionary<string, string>>("Data\\Mail");

        // Gather candidates (by key or by text mentioning beach / fishing)
        var candidates = mail
            .Where(kv =>
                kv.Key.IndexOf("willy", StringComparison.OrdinalIgnoreCase) >= 0
                || kv.Value.IndexOf("Willy", StringComparison.OrdinalIgnoreCase) >= 0
                || kv.Value.IndexOf("beach", StringComparison.OrdinalIgnoreCase) >= 0
                || kv.Value.IndexOf("fish", StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        // Prefer early-looking keys (willy1 / willyLetter), then anything with “beach”
        string? pick =
            candidates.Select(kv => kv.Key).FirstOrDefault(k => k.Equals("willy1", StringComparison.OrdinalIgnoreCase))
            ?? candidates.Select(kv => kv.Key).FirstOrDefault(k => k.Equals("willyLetter", StringComparison.OrdinalIgnoreCase))
            ?? candidates.Where(kv => kv.Value.IndexOf("beach", StringComparison.OrdinalIgnoreCase) >= 0)
                         .Select(kv => kv.Key).FirstOrDefault()
            ?? candidates.Select(kv => kv.Key).FirstOrDefault();

        if (pick != null)
            monitor.Log($"[Probe] Using invite candidate: {pick}", LogLevel.Info);
        else
            monitor.Log("[Probe] No suitable Willy invite mail key found.", LogLevel.Warn);

        return pick;
    }
}
