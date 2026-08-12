namespace DeathRecap
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <summary>
    /// The plugin's user-facing messages.
    /// </summary>
    public class Translation : ITranslation
    {
        /// <summary>
        /// Gets or sets the recap text shown to a player after they die. {0} is the killer's name (colored by role), {1} is the weapon, {2} is the distance in meters, {3} is damage the killer dealt (damage taken), {4} is damage the victim dealt to the killer.
        /// </summary>
        [Description("The recap text shown to a player after they die. {0} is the killer's name (colored by role), {1} is the weapon, {2} is the distance in meters, {3} is damage the killer dealt (damage taken), {4} is damage the victim dealt to the killer.")]
        public string RecapText { get; set; } = "You were killed by {0}! | <color=#FFA500>Weapon:</color> {1}\n<color=#FFA500>Distance:</color> {2}m | <color=#FF0000>Damage taken:</color> {3}\n<color=#00FF00>Damage dealt:</color> {4}";
    }
}