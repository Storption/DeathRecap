namespace DeathRecap.Modules
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Everything the recap formatter needs to know about one death.
    /// </summary>
    public sealed class DeathContext
    {
        public DeathCategory Category { get; set; }

        public string KillerName { get; set; } = string.Empty;

        public string KillerRole { get; set; } = string.Empty;

        public string CauseName { get; set; } = string.Empty;

        public string? CustomReason { get; set; }

        public float? Distance { get; set; }

        public float DamageDealtToKiller { get; set; }

        public List<KeyValuePair<string, float>> Breakdown { get; set; } = new();

        public string? HitLocation { get; set; }

        public int? KillerHealth { get; set; }

        public int? KillerMaxHealth { get; set; }

        public TimeSpan? Survived { get; set; }

        public string? Location { get; set; }

        public int KillsThisLife { get; set; }

        public string KillerColor { get; set; } = "#FFFFFF";

        public string LocationColor { get; set; } = "#FFFFFF";
    }
}