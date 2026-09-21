namespace DeathRecap.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Builds the recap text from a <see cref="DeathContext"/> using the configured wording and options.
    /// </summary>
    public static class RecapFormatter
    {
        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        public static string Build(DeathContext death)
        {
            List<string> rows = new() { BuildHeader(death) };

            foreach (string row in Config.RecapLayout)
            {
                string line = string.Join(
                    Translation.DetailSeparator,
                    row.Split(',').Select(key => BuildSegment(key.Trim().ToLowerInvariant(), death)).OfType<string>());

                if (line.Length > 0)
                    rows.Add(line);
            }

            return string.Join("\n", rows);
        }

        private static string? BuildSegment(string key, DeathContext death) => key switch
        {
            "hit" => Config.ShowHitLocation && death.HitLocation is not null
                ? string.Format(Translation.HitLocationLine, death.HitLocation)
                : null,
            "distance" => Config.ShowDistance && death.Distance is float distance
                ? string.Format(Translation.DistanceLine, distance.ToString("F1"))
                : null,
            "killer_health" => Config.ShowKillerHealth && death.KillerHealth is int health && death.KillerMaxHealth is int maxHealth
                ? string.Format(Translation.KillerHealthLine, health, maxHealth, death.KillerColor)
                : null,
            "damage_dealt" => Config.ShowDamageDealt && death.DamageDealtToKiller > 0f
                ? string.Format(Translation.DamageDealtLine, (int)Math.Round(death.DamageDealtToKiller))
                : null,
            "damage_taken" => Config.ShowDamageTaken && death.Breakdown.Count > 0
                ? BuildDamageTaken(death)
                : null,
            "survival" => Config.ShowSurvivalTime && death.Survived is TimeSpan survived
                ? string.Format(Translation.SurvivalLine, (int)survived.TotalMinutes, survived.Seconds)
                : null,
            "location" => Config.ShowLocation && death.Location is not null
                ? string.Format(Translation.LocationLine, death.Location, death.LocationColor)
                : null,
            "kills" => Config.ShowKillsThisLife && death.KillsThisLife > 0
                ? string.Format(Translation.KillsLine, death.KillsThisLife)
                : null,
            _ => null,
        };

        private static string BuildHeader(DeathContext death) => death.Category switch
        {
            DeathCategory.Player => string.Format(Translation.PlayerKillHeader, death.KillerName, death.KillerRole, death.CauseName),
            DeathCategory.Scp => string.Format(Translation.ScpKillHeader, death.KillerName, death.KillerRole, death.CauseName),
            DeathCategory.Zombie => string.Format(Translation.ZombieKillHeader, death.KillerName, death.KillerRole, death.CauseName),
            DeathCategory.TeamKill => string.Format(Translation.TeamKillHeader, death.KillerName, death.KillerRole, death.CauseName),
            DeathCategory.Environment => string.Format(Translation.EnvironmentHeader, death.CauseName),
            DeathCategory.CustomReason => string.Format(Translation.CustomReasonHeader, death.CustomReason),
            _ => Translation.UnknownHeader,
        };

        private static string BuildDamageTaken(DeathContext death)
        {
            int total = (int)Math.Round(death.Breakdown.Sum(entry => entry.Value));

            if (death.Breakdown.Count == 1)
                return string.Format(Translation.DamageTakenLine, total);

            int shown = Math.Max(1, Config.MaxBreakdownEntries);
            string list = string.Join(
                Translation.BreakdownSeparator,
                death.Breakdown.Take(shown).Select(entry => string.Format(Translation.BreakdownEntry, entry.Key, (int)Math.Round(entry.Value))));

            int hidden = death.Breakdown.Count - shown;
            if (hidden > 0)
                list += Translation.BreakdownSeparator + string.Format(Translation.BreakdownMore, hidden);

            return string.Format(Translation.DamageBreakdownLine, total, list);
        }
    }
}