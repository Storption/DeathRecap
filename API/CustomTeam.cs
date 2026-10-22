namespace DeathRecap.API
{
    using System;
    using Exiled.API.Features;
    using PlayerRoles;

    /// <summary>
    /// A team added by another plugin, such as a custom faction or role, that DeathRecap treats like a vanilla team:
    /// for teamkills and name colors.
    /// </summary>
    public sealed class CustomTeam
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTeam"/> class.
        /// </summary>
        /// <param name="name">The team's name, used in debug logs.</param>
        /// <param name="isMember">Gets whether a living player is on this team.</param>
        public CustomTeam(string name, Func<Player, bool> isMember)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsMember = isMember ?? throw new ArgumentNullException(nameof(isMember));
        }

        /// <summary>
        /// Gets the team's name, used in debug logs.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether a living player is on this team. It's checked as a player is dying, before their role changes, and
        /// for a killer still on the life they killed with.
        /// </summary>
        public Func<Player, bool> IsMember { get; }

        /// <summary>
        /// Gets or sets the color of members' names, as a hex code like <c>#FF96DE</c>, or null to use their role's color.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the vanilla faction this team fights alongside, or null if it's only on its own side. Kills between
        /// members, or between a member and an ally, are teamkills; any other kill involving a member is an enemy kill.
        /// </summary>
        public Faction? Allies { get; set; }
    }
}