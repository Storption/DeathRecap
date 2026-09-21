namespace DeathRecap.Modules
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
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

            RoleTypeId killerRole = killer.Role.Type;
            if (killerRole == RoleTypeId.Scp0492)
                return DeathCategory.Zombie;

            if (killerRole.GetTeam() == Team.SCPs)
                return DeathCategory.Scp;

            return HitboxIdentity.IsEnemy(killerRole, ev.TargetOldRole) ? DeathCategory.Player : DeathCategory.TeamKill;
        }
    }
}