namespace DeathRecap
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Interfaces;
    using PlayerRoles;

    /// <summary>
    /// The plugin's user-facing messages.
    /// </summary>
    public class Translation : ITranslation
    {
        [Description("The first line of the recap when a player killed you. {0} is their colored name, {1} their role, {2} the weapon or cause.")]
        public string PlayerKillHeader { get; set; } = "<color=#FF0000>Killed by</color> {0} <color=#AAAAAA>({1})</color> with {2}";

        [Description("The first line of the recap when an SCP killed you. {0} is the player's colored name, {1} the SCP's name, {2} the attack.")]
        public string ScpKillHeader { get; set; } = "<color=#FF0000>Killed by</color> <color=#FF0000>{1}</color> <color=#AAAAAA>({0})</color>";

        [Description("The first line of the recap when a zombie killed you. {0} is the player's colored name, {1} the zombie's name, {2} the attack.")]
        public string ZombieKillHeader { get; set; } = "<color=#FF0000>Killed by</color> <color=#FF0000>{1}</color> <color=#AAAAAA>({0})</color>";

        [Description("The first line of the recap when a teammate killed you. {0} is their colored name, {1} their role, {2} the weapon or cause.")]
        public string TeamKillHeader { get; set; } = "<color=#FF8000>Teamkilled by</color> {0} <color=#AAAAAA>({1})</color> with {2}";

        [Description("The first line of the recap when nobody killed you. {0} is the cause, e.g. 'the Tesla gate' or 'a fall'.")]
        public string EnvironmentHeader { get; set; } = "<color=#FF0000>You died to</color> {0}";

        [Description("The first line of the recap when a script or plugin gave a custom death reason. {0} is that reason.")]
        public string CustomReasonHeader { get; set; } = "<color=#FF0000>{0}</color>";

        [Description("The first line of the recap when the cause of death is unknown.")]
        public string UnknownHeader { get; set; } = "<color=#FF0000>You died.</color>";

        [Description("The line showing where a firearm hit landed. {0} is the hit location, e.g. 'Headshot'.")]
        public string HitLocationLine { get; set; } = "<color=#FF4D4D>Hit:</color> {0}";

        [Description("The line showing the distance of the killing blow. {0} is the distance in meters.")]
        public string DistanceLine { get; set; } = "<color=#FFA500>Distance:</color> {0}m";

        [Description("The line showing the killer's remaining health. {0} is their health, {1} their maximum health, {2} their color.")]
        public string KillerHealthLine { get; set; } = "<color=#FFA500>Their health:</color> <color={2}>{0}/{1}</color>";

        [Description("The line showing the damage you dealt to your killer. Not shown if it was 0. {0} is the damage.")]
        public string DamageDealtLine { get; set; } = "<color=#00FF00>You dealt:</color> {0}";

        [Description("The line showing the damage you took when it all came from one source. {0} is the damage.")]
        public string DamageTakenLine { get; set; } = "<color=#FF0000>You took:</color> {0}";

        [Description("The line showing the damage you took when it came from several sources. {0} is the total damage, {1} is the list of sources.")]
        public string DamageBreakdownLine { get; set; } = "<color=#FF0000>You took {0}:</color> {1}";

        [Description("How each source is written in the damage list. {0} is the source's name, {1} is the damage.")]
        public string BreakdownEntry { get; set; } = "{0} {1}";

        [Description("What goes between the sources in the damage list.")]
        public string BreakdownSeparator { get; set; } = ", ";

        [Description("Added to the end of the damage list when there are more sources than can be shown. {0} is how many were left out.")]
        public string BreakdownMore { get; set; } = "+{0} more";

        [Description("The line showing how long you survived. {0} is minutes, {1} is seconds.")]
        public string SurvivalLine { get; set; } = "<color=#32CD32>You survived:</color> {0}m {1}s";

        [Description("The line showing where you died. {0} is the location, {1} is the zone's color.")]
        public string LocationLine { get; set; } = "<color=#FFA500>Location:</color> <color={1}>{0}</color>";

        [Description("How a location is written when the room is shown as well. {0} is the zone, {1} is the room.")]
        public string LocationWithRoom { get; set; } = "{0} - {1}";

        [Description("The line showing how many kills you got that life. {0} is the number of kills.")]
        public string KillsLine { get; set; } = "<color=#00FF00>Kills this life:</color> {0}";

        [Description("What goes between details that share a line.")]
        public string DetailSeparator { get; set; } = " <color=#777777>|</color> ";

        [Description("The names shown for each weapon and cause of death. Remove an entry to use an automatically formatted name instead.")]
        public Dictionary<Exiled.API.Enums.DamageType, string> DamageTypeNames { get; set; } = Modules.CauseNames.BuildDefaults();

        [Description("The name shown for each base role. Remove an entry to use an automatically formatted name instead.")]
        public Dictionary<RoleTypeId, string> RoleNames { get; set; } = Modules.RoleDisplay.BuildDefaults();

        [Description("The names shown for where a firearm hit landed.")]
        public Dictionary<HitboxType, string> HitboxNames { get; set; } = Modules.DeathDetails.BuildHitboxDefaults();

        [Description("The names shown for each zone.")]
        public Dictionary<Exiled.API.Enums.ZoneType, string> ZoneNames { get; set; } = Modules.DeathDetails.BuildZoneDefaults();

        [Description("The color of the location for each zone, as a hex color like #FF8C00.")]
        public Dictionary<Exiled.API.Enums.ZoneType, string> ZoneColors { get; set; } = Modules.DeathDetails.BuildZoneColorDefaults();

        [Description("The broadcast shown to everyone when a plugin update has been installed and the server will restart once the round ends. {0} is the plugin's name. Leave empty to disable.")]
        public string AutoUpdateRestartBroadcast { get; set; } = "<color=orange>[Update]</color> {0} was updated - the server will restart after this round to apply it.";
    }
}