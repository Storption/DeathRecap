namespace DeathRecap.Modules
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using DamageType = Exiled.API.Enums.DamageType;

    /// <summary>
    /// Turns damage types into readable names for the recap.
    /// </summary>
    public static class CauseNames
    {
        private static readonly Regex WordBoundary = new("(?<=[a-z0-9])(?=[A-Z])", RegexOptions.Compiled);

        /// <summary>
        /// Builds the default damage type names shown in the translation file.
        /// </summary>
        public static Dictionary<DamageType, string> BuildDefaults()
        {
            return new Dictionary<DamageType, string>
            {
                [DamageType.Firearm] = "a firearm",
                [DamageType.E11Sr] = "the E-11 SR",
                [DamageType.Crossvec] = "the Crossvec",
                [DamageType.Logicer] = "the Logicer",
                [DamageType.Revolver] = "the Revolver",
                [DamageType.Shotgun] = "the Shotgun",
                [DamageType.AK] = "the AK",
                [DamageType.Com15] = "the COM-15",
                [DamageType.Com18] = "the COM-18",
                [DamageType.Com45] = "the COM-45",
                [DamageType.Fsp9] = "the FSP-9",
                [DamageType.ParticleDisruptor] = "the Particle Disruptor",
                [DamageType.Frmg0] = "the FR-MG-0",
                [DamageType.A7] = "the A7",
                [DamageType.Jailbird] = "the Jailbird",
                [DamageType.MicroHid] = "the Micro H.I.D.",
                [DamageType.Scp127] = "SCP-127",
                [DamageType.Explosion] = "an explosion",
                [DamageType.Scp018] = "SCP-018",
                [DamageType.Scp207] = "an SCP-207 overdose",
                [DamageType.Falldown] = "a fall",
                [DamageType.Warhead] = "the Alpha Warhead",
                [DamageType.Decontamination] = "decontamination",
                [DamageType.Asphyxiation] = "suffocation",
                [DamageType.Poison] = "poison",
                [DamageType.Bleeding] = "bleeding",
                [DamageType.Tesla] = "a Tesla gate",
                [DamageType.Recontainment] = "recontainment",
                [DamageType.Crushed] = "being crushed",
                [DamageType.FemurBreaker] = "the femur breaker",
                [DamageType.PocketDimension] = "the pocket dimension",
                [DamageType.FriendlyFireDetector] = "friendly fire",
                [DamageType.SeveredHands] = "severed hands",
                [DamageType.SeveredEyes] = "severed eyes",
                [DamageType.Hypothermia] = "hypothermia",
                [DamageType.CardiacArrest] = "cardiac arrest",
                [DamageType.Strangled] = "being strangled",
                [DamageType.Marshmallow] = "a marshmallow",
                [DamageType.Scp1507] = "SCP-1507",
                [DamageType.Scp956] = "SCP-956",
                [DamageType.SnowBall] = "a snowball",
                [DamageType.GrayCandy] = "gray candy",
                [DamageType.Scp1509] = "SCP-1509",
                [DamageType.Scp049] = "SCP-049's touch",
                [DamageType.Scp096] = "SCP-096's rage",
                [DamageType.Scp173] = "a neck snap",
                [DamageType.Scp939] = "SCP-939's bite",
                [DamageType.Scp0492] = "zombie claws",
                [DamageType.Scp106] = "SCP-106",
                [DamageType.Scp3114] = "SCP-3114",
                [DamageType.Scp] = "an SCP attack",
                [DamageType.Silent] = "unknown causes",
                [DamageType.Custom] = "unknown causes",
                [DamageType.Unknown] = "unknown causes",
            };
        }

        /// <summary>
        /// Gets the configured name for a damage type, falling back to a formatted version of its identifier.
        /// </summary>
        public static string Get(DamageType type)
        {
            return Plugin.Instance!.Translation.DamageTypeNames.TryGetValue(type, out string? name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : WordBoundary.Replace(type.ToString(), " ");
        }
    }
}