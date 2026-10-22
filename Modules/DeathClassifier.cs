namespace DeathRecap.Modules
{
    using DeathRecap.API;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using Footprinting;
    using PlayerRoles;
    using PlayerStatsSystem;

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

    public static class DeathClassifier
    {
        public static DeathCategory Classify(DiedEventArgs ev, CustomTeam? killerTeam, CustomTeam? victimTeam, out string? customReason)
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
            if (!CustomTeams.AreEnemies(killerRole, killerTeam, ev.TargetOldRole, victimTeam))
                return DeathCategory.TeamKill;

            if (killerRole == RoleTypeId.Scp0492)
                return DeathCategory.Zombie;

            return killerRole.GetTeam() == Team.SCPs ? DeathCategory.Scp : DeathCategory.Player;
        }

        public static RoleTypeId GetKillerRole(DiedEventArgs ev, Player killer)
        {
            Footprint footprint = ev.DamageHandler.AttackerFootprint;
            return footprint.IsSet ? footprint.Role : killer.Role.Type;
        }

        public static bool IsSameLife(DiedEventArgs ev, Player killer)
        {
            Footprint footprint = ev.DamageHandler.AttackerFootprint;
            return killer.IsAlive && (!footprint.IsSet || footprint.LifeIdentifier == killer.ReferenceHub.roleManager.CurrentRole.UniqueLifeIdentifier);
        }
    }
}