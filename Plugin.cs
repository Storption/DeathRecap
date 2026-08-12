namespace DeathRecap
{
    using System;
    using Exiled.API.Features;

    /// <summary>
    /// The main plugin class.
    /// </summary>
    public class Plugin : Plugin<Config, Translation>
    {
        /// <summary>
        /// Gets the only existing instance of the <see cref="Plugin"/> class.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc/>
        public override string Author => "Storption";

        /// <inheritdoc/>
        public override string Name => "DeathRecap";

        /// <inheritdoc/>
        public override string Prefix => "DeathRecap";

        /// <inheritdoc/>
        public override Version RequiredExiledVersion { get; } = new Version(9, 14, 2);

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0, 0);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            Modules.Recap.RegisterEvents();

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Modules.Recap.UnregisterEvents();

            Instance = null;

            base.OnDisabled();
        }
    }
}