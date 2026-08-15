namespace DeathRecap
{
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