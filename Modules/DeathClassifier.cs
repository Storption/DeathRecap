namespace DeathRecap.Modules
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using Footprinting;
    using PlayerRoles;
    using PlayerStatsSystem;

    /// <summary>
    /// The kind of death a recap is being shown for.
    /// </summary>
    public enum DeathCategory
    {
        Player,
        Scp,
        Zombie,
        TeamKill,
        Environment,
        CustomReason,
        Unknown,
    }

    /// <summary>
    /// Works out what kind of death a <see cref="DiedEventArgs"/> represents.
    /// </summary>
    public static class DeathClassifier
    {
        public static DeathCategory Classify(DiedEventArgs ev, out string? customReason)
        {
            customReason = null;

            if (ev.DamageHandler.Is(out CustomReasonDamageHandler customHandler))
            {
                customReason = customHandler.DeathScreenText;
                return DeathCategory.CustomReason;
            }

            Player? killer = ev.Attacker;
            if (killer is null || killer == ev.Player)
                return ev.DamageHandler.Type == DamageType.Unknown ? DeathCategory.Unknown : DeathCategory.Environment;

            RoleTypeId killerRole = GetKillerRole(ev, killer);
            if (killerRole == RoleTypeId.Scp0492)
                return DeathCategory.Zombie;

            if (killerRole.GetTeam() == Team.SCPs)
                return DeathCategory.Scp;

            return HitboxIdentity.IsEnemy(killerRole, ev.TargetOldRole) ? DeathCategory.Player : DeathCategory.TeamKill;
        }

        /// <summary>
        /// Gets the killer's role when the damage was dealt - not the same as their current role for delayed damage, like a grenade thrown before they died.
        /// </summary>
        public static RoleTypeId GetKillerRole(DiedEventArgs ev, Player killer)
        {
            Footprint footprint = ev.DamageHandler.AttackerFootprint;
            return footprint.IsSet ? footprint.Role : killer.Role.Type;
        }

        /// <summary>
        /// Whether the killer is still on the life they dealt the damage from.
        /// </summary>
        public static bool IsSameLife(DiedEventArgs ev, Player killer)
        {
            Footprint footprint = ev.DamageHandler.AttackerFootprint;
            return killer.IsAlive && (!footprint.IsSet || footprint.LifeIdentifier == killer.ReferenceHub.roleManager.CurrentRole.UniqueLifeIdentifier);
        }
    }
}