namespace DeathRecap
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <summary>
    /// The plugin's configuration.
    /// </summary>
    public class Config : IConfig
    {
        /// <inheritdoc />
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc />
        [Description("Whether debug messages are shown.")]
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets how long, in seconds, the recap stays visible. 0 means it stays until the player leaves spectator or the round ends.
        /// </summary>
        [Description("How long, in seconds, the recap stays visible. 0 means it stays until the player leaves spectator or the round ends.")]
        public int RecapDurationSeconds { get; set; } = 0;

        /// <summary>
        /// Gets or sets how many blank lines to pad the recap hint with, controlling its vertical position on screen.
        /// </summary>
        [Description("How many blank lines to pad the recap hint with, controlling its vertical position on screen.")]
        public int HintLinePadding { get; set; } = 15;

        /// <summary>
        /// Gets or sets the recap text's size, as a percentage of the default hint size.
        /// </summary>
        [Description("The recap text's size, as a percentage of the default hint size.")]
        public int HintTextSizePercent { get; set; } = 80;

        /// <summary>
        /// Gets or sets how the detail lines under the recap's first line are arranged.
        /// </summary>
        [Description("How the detail lines under the recap's first line are arranged, one entry per line. List what goes on each line, separated by commas: hit, distance, killer_health, damage_dealt, damage_taken, survival, location, kills. Anything switched off elsewhere in this config is skipped, and a line with nothing to show is left out.")]
        public List<string> RecapLayout { get; set; } = new()
        {
            "hit, distance, killer_health",
            "damage_dealt, damage_taken",
            "survival, location, kills",
        };

        /// <summary>
        /// Gets or sets whether a recap is shown when nobody killed the player (falls, the Tesla gate, the warhead, scripted deaths and so on).
        /// </summary>
        [Description("Whether a recap is shown when nobody killed the player (falls, the Tesla gate, the warhead, scripted deaths and so on).")]
        public bool ShowEnvironmentalDeaths { get; set; } = true;

        /// <summary>
        /// Gets or sets whether where a firearm hit landed (headshot, body, limb) is shown.
        /// </summary>
        [Description("Whether where a firearm hit landed (headshot, body shot, limb shot) is shown.")]
        public bool ShowHitLocation { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the distance of the killing blow is shown.
        /// </summary>
        [Description("Whether the distance of the killing blow is shown.")]
        public bool ShowDistance { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the killer's remaining health is shown.
        /// </summary>
        [Description("Whether the killer's remaining health is shown.")]
        public bool ShowKillerHealth { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the damage the player dealt to their killer is shown.
        /// </summary>
        [Description("Whether the damage the player dealt to their killer is shown.")]
        public bool ShowDamageDealt { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the damage the player took, and from whom or what, is shown.
        /// </summary>
        [Description("Whether the damage the player took, and from whom or what, is shown.")]
        public bool ShowDamageTaken { get; set; } = true;

        /// <summary>
        /// Gets or sets how many damage sources are listed before the rest are summarized.
        /// </summary>
        [Description("How many damage sources are listed before the rest are summarized as '+N more'.")]
        public int MaxBreakdownEntries { get; set; } = 3;

        /// <summary>
        /// Gets or sets whether how long the player survived is shown.
        /// </summary>
        [Description("Whether how long the player survived that life is shown.")]
        public bool ShowSurvivalTime { get; set; } = true;

        /// <summary>
        /// Gets or sets whether where the player died is shown.
        /// </summary>
        [Description("Whether where the player died is shown.")]
        public bool ShowLocation { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the room is shown alongside the zone in the location.
        /// </summary>
        [Description("Whether the room is shown alongside the zone in the location line.")]
        public bool ShowRoom { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the number of kills the player got that life is shown.
        /// </summary>
        [Description("Whether the number of kills the player got that life is shown. Not shown if they had none.")]
        public bool ShowKillsThisLife { get; set; } = true;

        /// <summary>
        /// Gets or sets whether roles added by custom role plugins are shown by their own name instead of their base role's.
        /// </summary>
        [Description("Whether roles added by custom role plugins are shown by their own name instead of their base role's.")]
        public bool CustomRoleNamesEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether UncomplicatedCustomRoles, if installed, is asked for a player's role name.
        /// </summary>
        [Description("Whether UncomplicatedCustomRoles, if installed, is asked for a player's role name.")]
        public bool CustomRoleNamesFromUcr { get; set; } = true;

        /// <summary>
        /// Gets or sets whether a custom role's name may be read from the player's custom info when it replaces the normal role display.
        /// </summary>
        [Description("Whether a custom role's name may be read from the player's custom info when it replaces the normal role display (used by SER and some other plugins). Off by default because other plugins can use custom info for other things - turn it on if you use SER custom roles.")]
        public bool CustomRoleNamesFromCustomInfo { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to check for and automatically install updates.
        /// </summary>
        [Description("Whether to check for and automatically install updates.")]
        public bool AutoUpdateEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to keep a backup of the previous .dll before replacing it with an update.
        /// </summary>
        [Description("Whether to keep a backup of the previous .dll before replacing it with an update.")]
        public bool AutoUpdateBackup { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to automatically restart the server once the current round ends, to apply a downloaded update.
        /// </summary>
        [Description("Whether to automatically restart the server once the current round ends, to apply a downloaded update. Never restarts mid-round.")]
        public bool AutoUpdateRestart { get; set; } = true;
    }
}