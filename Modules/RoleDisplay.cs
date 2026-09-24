namespace DeathRecap.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Exiled.API.Features;
    using PlayerRoles;

    /// <summary>
    /// Works out the name to show for a player's role, including roles added by custom role plugins.
    /// </summary>
    public static class RoleDisplay
    {
        private static readonly Regex RichTextTags = new("<.*?>", RegexOptions.Compiled);
        private static readonly Regex WordBoundary = new("(?<=[a-z])(?=[A-Z])");

        private static MethodInfo? ucrTryGet;
        private static PropertyInfo? ucrRole;
        private static PropertyInfo? ucrName;

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        /// <summary>
        /// Builds the default role name dictionary shown in the translation file.
        /// </summary>
        public static Dictionary<RoleTypeId, string> BuildDefaults()
        {
            return Enum.GetValues(typeof(RoleTypeId))
                .Cast<RoleTypeId>()
                .Where(role => role != RoleTypeId.None)
                .ToDictionary(role => role, FormatRoleName);
        }

        /// <summary>
        /// Gets the name to show for a player's role, and where that name came from.
        /// </summary>
        public static string Get(Player player, out string source)
        {
            if (Config.CustomRoleNamesEnabled)
            {
                string? uniqueRole = player.UniqueRole;
                if (!string.IsNullOrWhiteSpace(uniqueRole))
                {
                    source = "UniqueRole";
                    return uniqueRole!.Trim();
                }

                string? ucrName = Config.CustomRoleNamesFromUcr ? GetUcrRoleName(player) : null;
                if (!string.IsNullOrWhiteSpace(ucrName))
                {
                    source = "UCR";
                    return ucrName!.Trim();
                }

                string? infoName = Config.CustomRoleNamesFromCustomInfo ? GetCustomInfoRoleName(player) : null;
                if (!string.IsNullOrWhiteSpace(infoName))
                {
                    source = "CustomInfo";
                    return infoName!;
                }
            }

            source = "BaseRole";
            return GetBaseRoleName(player.Role.Type);
        }

        /// <summary>
        /// Gets the configured name for a base role, falling back to a formatted version of its identifier.
        /// </summary>
        public static string GetBaseRoleName(RoleTypeId role)
        {
            return Translation.RoleNames.TryGetValue(role, out string? name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : FormatRoleName(role);
        }

        private static string FormatRoleName(RoleTypeId role)
        {
            string name = role.ToString();

            if (name.StartsWith("Scp", StringComparison.Ordinal) && name.Length > 3 && char.IsDigit(name[3]))
            {
                string digits = name.Substring(3);
                return digits == "0492" ? "SCP-049-2" : "SCP-" + digits;
            }

            return WordBoundary.Replace(name, " ").Replace("Ntf", "NTF");
        }

        private static string? GetCustomInfoRoleName(Player player)
        {
            string info = player.CustomInfo;
            if (string.IsNullOrWhiteSpace(info) || player.InfoArea.HasFlag(PlayerInfoArea.Role))
                return null;

            string[] lines = RichTextTags.Replace(info, string.Empty).Split('\n');
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    return lines[i].Trim();
            }

            return null;
        }

        private static string? GetUcrRoleName(Player player)
        {
            try
            {
                if (ucrTryGet is null && !ResolveUcr())
                    return null;

                object?[] args = { player.ReferenceHub, null };
                if (ucrTryGet!.Invoke(null, args) is not true || args[1] is null)
                    return null;

                object? role = ucrRole!.GetValue(args[1]);
                return role is null ? null : ucrName!.GetValue(role) as string;
            }
            catch (Exception ex)
            {
                if (Config.Debug)
                    Log.Debug($"UCR role lookup failed - {ex.Message}");

                return null;
            }
        }

        public static bool ResolveUcr()
        {
            Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "UncomplicatedCustomRoles");

            Type? summonedRole = assembly?.GetType("UncomplicatedCustomRoles.API.Features.SummonedCustomRole");
            if (summonedRole is null)
                return false;

            MethodInfo? tryGet = summonedRole.GetMethod("TryGet", new[] { typeof(ReferenceHub), summonedRole.MakeByRefType() });
            PropertyInfo? role = summonedRole.GetProperty("Role");
            PropertyInfo? name = role?.PropertyType.GetProperty("Name");
            if (tryGet is null || role is null || name is null)
                return false;

            ucrTryGet = tryGet;
            ucrRole = role;
            ucrName = name;
            return true;
        }
    }
}