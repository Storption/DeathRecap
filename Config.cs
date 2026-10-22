namespace DeathRecap
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether debug messages are shown.")]
        public bool Debug { get; set; } = false;

        [Description("How long, in seconds, the recap stays visible. 0 means it stays until the player leaves spectator or the round ends.")]
        public int RecapDurationSeconds { get; set; } = 0;

        [Description("How many blank lines to pad the recap hint with, controlling its vertical position on screen.")]
        public int HintLinePadding { get; set; } = 15;

        [Description("The recap text's size, as a percentage of the default hint size.")]
        public int HintTextSizePercent { get; set; } = 80;

        [Description("How the detail lines under the recap's first line are arranged, one entry per line. List what goes on each line, separated by commas: hit, distance, killer_health, damage_dealt, damage_taken, survival, location, kills. Anything switched off elsewhere in this config is skipped, and a line with nothing to show is left out.")]
        public List<string> RecapLayout { get; set; } = new()
        {
            "hit, distance, killer_health",
            "damage_dealt, damage_taken",
            "survival, location, kills",
        };

        [Description("Whether a recap is shown when nobody killed the player (falls, the Tesla gate, the warhead, scripted deaths and so on).")]
        public bool ShowEnvironmentalDeaths { get; set; } = true;

        [Description("Whether where a firearm hit landed (headshot, body shot, limb shot) is shown.")]
        public bool ShowHitLocation { get; set; } = true;

        [Description("Whether the distance of the killing blow is shown.")]
        public bool ShowDistance { get; set; } = true;

        [Description("Whether the killer's remaining health is shown.")]
        public bool ShowKillerHealth { get; set; } = true;

        [Description("Whether the damage the player dealt to their killer is shown.")]
        public bool ShowDamageDealt { get; set; } = true;

        [Description("Whether the damage the player took, and from whom or what, is shown.")]
        public bool ShowDamageTaken { get; set; } = true;

        [Description("How many damage sources are listed before the rest are summarized as '+N more'.")]
        public int MaxBreakdownEntries { get; set; } = 3;

        [Description("Whether how long the player survived that life is shown.")]
        public bool ShowSurvivalTime { get; set; } = true;

        [Description("Whether where the player died is shown.")]
        public bool ShowLocation { get; set; } = true;

        [Description("Whether the room is shown alongside the zone in the location line.")]
        public bool ShowRoom { get; set; } = true;

        [Description("Whether the number of kills the player got that life is shown. Not shown if they had none.")]
        public bool ShowKillsThisLife { get; set; } = true;

        [Description("Whether roles added by custom role plugins are shown by their own name instead of their base role's.")]
        public bool CustomRoleNamesEnabled { get; set; } = true;

        [Description("Whether UncomplicatedCustomRoles, if installed, is asked for a player's role name.")]
        public bool CustomRoleNamesFromUcr { get; set; } = true;

        [Description("Whether a custom role's name may be read from the player's custom info when it replaces the normal role display (used by SER and some other plugins). Off by default because other plugins can use custom info for other things - turn it on if you use SER custom roles.")]
        public bool CustomRoleNamesFromCustomInfo { get; set; } = false;

        [Description("Whether to check for and automatically install updates.")]
        public bool AutoUpdateEnabled { get; set; } = true;

        [Description("Whether to keep a backup of the previous .dll before replacing it with an update.")]
        public bool AutoUpdateBackup { get; set; } = true;

        [Description("Whether to automatically restart the server once the current round ends, to apply a downloaded update. Never restarts mid-round.")]
        public bool AutoUpdateRestart { get; set; } = true;
    }
}