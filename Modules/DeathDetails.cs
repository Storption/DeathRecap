namespace DeathRecap.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using PlayerStatsSystem;

    /// <summary>
    /// Works out the extra details shown in a recap: where the killing shot landed and where the player died.
    /// </summary>
    public static class DeathDetails
    {
        private static readonly Regex ZonePrefix = new("^(Lcz|Hcz|Ez)", RegexOptions.Compiled);
        private static readonly Regex WordBoundary = new("(?<=[a-z0-9])(?=[A-Z])", RegexOptions.Compiled);

        public static Dictionary<ZoneType, string> BuildZoneDefaults()
        {
            return new Dictionary<ZoneType, string>
            {
                [ZoneType.LightContainment] = "Light Containment",
                [ZoneType.HeavyContainment] = "Heavy Containment",
                [ZoneType.Entrance] = "Entrance Zone",
                [ZoneType.Surface] = "Surface",
                [ZoneType.Pocket] = "Pocket Dimension",
                [ZoneType.Other] = "Unknown location",
                [ZoneType.Unspecified] = "Unknown location",
            };
        }

        public static Dictionary<ZoneType, string> BuildZoneColorDefaults()
        {
            return new Dictionary<ZoneType, string>
            {
                [ZoneType.LightContainment] = "#FF8C00",
                [ZoneType.HeavyContainment] = "#FF3B30",
                [ZoneType.Entrance] = "#FFD60A",
                [ZoneType.Surface] = "#4FC3F7",
                [ZoneType.Pocket] = "#2E8B57",
                [ZoneType.Other] = "#AAAAAA",
                [ZoneType.Unspecified] = "#AAAAAA",
            };
        }

        public static Dictionary<HitboxType, string> BuildHitboxDefaults()
        {
            return new Dictionary<HitboxType, string>
            {
                [HitboxType.Headshot] = "Headshot",
                [HitboxType.Body] = "Body shot",
                [HitboxType.Limb] = "Limb shot",
            };
        }

        /// <summary>
        /// Describes where a firearm hit landed, or returns null if the killing blow wasn't a firearm.
        /// </summary>
        public static string? DescribeHit(DiedEventArgs ev)
        {
            if (!ev.DamageHandler.Is(out FirearmDamageHandler firearm))
                return null;

            return Plugin.Instance!.Translation.HitboxNames.TryGetValue(firearm.Hitbox, out string? name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : firearm.Hitbox.ToString();
        }

        /// <summary>
        /// Describes where a player currently is. Returns false if their location can't be worked out.
        /// </summary>
        public static bool TryDescribeLocation(Player player, out string location, out string color)
        {
            location = string.Empty;
            color = "#FFFFFF";

            Translation translation = Plugin.Instance!.Translation;
            ZoneType zone = player.Zone;
            RoomType room = player.CurrentRoom?.Type ?? RoomType.Unknown;

            if ((zone == ZoneType.Unspecified || zone == ZoneType.Other) && room == RoomType.Unknown)
                return false;

            if (translation.ZoneColors.TryGetValue(zone, out string? zoneColor) && !string.IsNullOrWhiteSpace(zoneColor))
                color = zoneColor;

            string zoneName = translation.ZoneNames.TryGetValue(zone, out string? name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : WordBoundary.Replace(zone.ToString(), " ");

            if (!Plugin.Instance.Config.ShowRoom || room == RoomType.Unknown)
            {
                location = zoneName;
                return true;
            }

            string roomName = WordBoundary.Replace(ZonePrefix.Replace(room.ToString(), string.Empty), " ").Trim();

            if (string.Equals(roomName, zoneName, StringComparison.OrdinalIgnoreCase))
            {
                location = zoneName;
                return true;
            }

            location = string.Format(translation.LocationWithRoom, zoneName, roomName);
            return true;
        }
    }
}