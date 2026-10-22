namespace DeathRecap
{
    using System;
    using Exiled.API.Features;

    public class Plugin : Plugin<Config, Translation>
    {
        public static Plugin? Instance { get; private set; }

        public override string Author => "Storption";

        public override string Name => "DeathRecap";

        public override string Prefix => "DeathRecap";

        public override Version RequiredExiledVersion { get; } = new Version(9, 14, 2);

        public override Version Version { get; } = new Version(2, 1, 0);

        public override void OnEnabled()
        {
            Instance = this;

            Modules.Recap.RegisterEvents();
            Modules.AutoUpdate.RegisterEvents();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Modules.Recap.UnregisterEvents();
            Modules.AutoUpdate.UnregisterEvents();

            Instance = null;

            base.OnDisabled();
        }
    }
}