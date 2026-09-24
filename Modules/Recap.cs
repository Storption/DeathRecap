namespace DeathRecap.Modules
{
    using DamageType = Exiled.API.Enums.DamageType;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.DamageHandlers;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;
    using MEC;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Tracks per-life damage in both directions and shows a death recap once a player dies.
    /// </summary>
    public static class Recap
    {
        private const float RefreshIntervalSeconds = 2f;

        private static readonly Dictionary<int, Dictionary<int, float>> DamageTakenFrom = new();
        private static readonly Dictionary<int, Dictionary<int, float>> DamageDealtTo = new();
        private static readonly Dictionary<int, Dictionary<int, float>> LastKnownDistance = new();
        private static readonly Dictionary<int, float> HealthBeforeHit = new();
        private static readonly Dictionary<int, CoroutineHandle> ActiveRecaps = new();
        private static readonly Dictionary<int, Dictionary<DamageType, float>> DamageTakenByCause = new();
        private static readonly Dictionary<int, string> AttackerNames = new();
        private static readonly Dictionary<int, DateTime> LifeStartTimes = new();
        private static readonly Dictionary<int, int> KillsThisLife = new();
        private static readonly Dictionary<int, (string Text, string Color)> DeathLocations = new();

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        private static float TotalHealth(Player player) => player.Health + player.HumeShield + player.ArtificialHealth;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
            Exiled.Events.Handlers.Player.Hurt += OnPlayerHurt;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
            Exiled.Events.Handlers.Player.Hurt -= OnPlayerHurt;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

            ResetState();
        }

        private static void OnWaitingForPlayers()
        {
            ResetState();
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            int id = ev.Player.Id;

            if (ActiveRecaps.TryGetValue(id, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                ActiveRecaps.Remove(id);
            }

            DamageTakenFrom.Remove(id);
            DamageDealtTo.Remove(id);
            LastKnownDistance.Remove(id);
            HealthBeforeHit.Remove(id);
            DamageTakenByCause.Remove(id);
            AttackerNames.Remove(id);
            LifeStartTimes.Remove(id);
            KillsThisLife.Remove(id);
            DeathLocations.Remove(id);
        }

        private static void ResetState()
        {
            foreach (int id in ActiveRecaps.Keys.ToList())
                StopRecap(id);

            DamageTakenFrom.Clear();
            DamageDealtTo.Clear();
            LastKnownDistance.Clear();
            HealthBeforeHit.Clear();
            DamageTakenByCause.Clear();
            AttackerNames.Clear();
            LifeStartTimes.Clear();
            KillsThisLife.Clear();
            DeathLocations.Clear();
        }

        private static void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player.Role.Team == Team.Dead)
                return;

            int id = ev.Player.Id;
            if (!LifeStartTimes.ContainsKey(id))
                LifeStartTimes[id] = DateTime.Now;
            DamageTakenFrom[id] = new Dictionary<int, float>();
            DamageDealtTo[id] = new Dictionary<int, float>();
            LastKnownDistance[id] = new Dictionary<int, float>();
            DamageTakenByCause[id] = new Dictionary<DamageType, float>();

            foreach (Dictionary<int, float> inner in DamageDealtTo.Values)
                inner.Remove(id);
            foreach (Dictionary<int, float> inner in LastKnownDistance.Values)
                inner.Remove(id);

            StopRecap(id);
        }

        private static void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Player is null)
                return;

            HealthBeforeHit.Remove(ev.Player.Id);

            if (ev.Amount <= 0)
                return;

            HealthBeforeHit[ev.Player.Id] = TotalHealth(ev.Player);

            if (ev.Attacker is null || ev.Attacker == ev.Player)
                return;

            int attackerId = ev.Attacker.Id;
            int victimId = ev.Player.Id;

            if (!LastKnownDistance.TryGetValue(victimId, out Dictionary<int, float>? distances))
            {
                distances = new Dictionary<int, float>();
                LastKnownDistance[victimId] = distances;
            }

            distances[attackerId] = Vector3.Distance(ev.Attacker.Position, ev.Player.Position);
        }

        private static void OnPlayerHurt(HurtEventArgs ev)
        {
            if (ev.Player is null)
                return;

            int victimId = ev.Player.Id;

            if (!HealthBeforeHit.TryGetValue(victimId, out float healthBefore))
                return;

            HealthBeforeHit.Remove(victimId);

            float actualDamage = healthBefore - Math.Max(0f, TotalHealth(ev.Player));
            if (actualDamage <= 0)
                return;

            if (ev.Attacker is null || ev.Attacker == ev.Player)
            {
                if (!DamageTakenByCause.TryGetValue(victimId, out Dictionary<DamageType, float>? causes))
                {
                    causes = new Dictionary<DamageType, float>();
                    DamageTakenByCause[victimId] = causes;
                }

                DamageType cause = ev.DamageHandler.Type;
                causes.TryGetValue(cause, out float currentCause);
                causes[cause] = currentCause + actualDamage;

                if (Config.Debug)
                    Log.Debug($"Hurt: {ev.Player.Nickname} (id={victimId}) took {actualDamage:F1} from {cause}.");

                return;
            }

            int attackerId = ev.Attacker.Id;
            AttackerNames[attackerId] = ev.Attacker.Nickname;

            if (!DamageTakenFrom.TryGetValue(victimId, out Dictionary<int, float>? incoming))
            {
                incoming = new Dictionary<int, float>();
                DamageTakenFrom[victimId] = incoming;
            }

            incoming.TryGetValue(attackerId, out float currentIncoming);
            incoming[attackerId] = currentIncoming + actualDamage;

            if (!DamageDealtTo.TryGetValue(attackerId, out Dictionary<int, float>? outgoing))
            {
                outgoing = new Dictionary<int, float>();
                DamageDealtTo[attackerId] = outgoing;
            }

            outgoing.TryGetValue(victimId, out float currentOutgoing);
            outgoing[victimId] = currentOutgoing + actualDamage;

            if (Config.Debug)
                Log.Debug($"Hurt: {ev.Attacker.Nickname} (id={attackerId}) -> {ev.Player.Nickname} (id={victimId}), rawAmount={ev.Amount:F1}, actualDamage={actualDamage:F1}, totalTaken={incoming[attackerId]:F1}.");
        }

        private static void OnPlayerDying(DyingEventArgs ev)
        {
            if (ev.Player is null || !Config.ShowLocation)
                return;

            if (DeathDetails.TryDescribeLocation(ev.Player, out string location, out string color))
                DeathLocations[ev.Player.Id] = (location, color);
            else
                DeathLocations.Remove(ev.Player.Id);
        }

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (ev.TargetOldRole is RoleTypeId.Spectator or RoleTypeId.Overwatch or RoleTypeId.None)
                return;

            Player victim = ev.Player;
            Player? killer = ev.Attacker is not null && ev.Attacker != victim ? ev.Attacker : null;
            int victimId = victim.Id;

            DeathCategory category = DeathClassifier.Classify(ev, out string? customReason);
            List<KeyValuePair<string, float>> breakdown = BuildBreakdown(victimId);

            TimeSpan? survived = null;
            if (LifeStartTimes.TryGetValue(victimId, out DateTime lifeStart))
            {
                survived = DateTime.Now - lifeStart;
                LifeStartTimes.Remove(victimId);
            }

            KillsThisLife.TryGetValue(victimId, out int kills);
            KillsThisLife.Remove(victimId);

            DeathLocations.TryGetValue(victimId, out (string Text, string Color) deathLocation);
            DeathLocations.Remove(victimId);

            if (killer is not null && category is not DeathCategory.TeamKill)
            {
                KillsThisLife.TryGetValue(killer.Id, out int killerKills);
                KillsThisLife[killer.Id] = killerKills + 1;
            }

            if (Config.Debug)
            {
                Log.Debug($"Death: {victim.Nickname} (id={victimId}) - category={category}, type={ev.DamageHandler.Type}{(customReason is null ? string.Empty : $", reason='{customReason}'")}.");
                Log.Debug($"Damage taken by {victim.Nickname}: {string.Join(", ", breakdown.Select(entry => $"{entry.Key} {entry.Value:F0}"))}.");
            }

            if (!Config.ShowEnvironmentalDeaths && (category is DeathCategory.Environment or DeathCategory.CustomReason or DeathCategory.Unknown))
                return;

            DeathContext death = new()
            {
                Category = category,
                CauseName = CauseNames.Get(ev.DamageHandler.Type),
                CustomReason = customReason,
                Breakdown = breakdown,
                HitLocation = DeathDetails.DescribeHit(ev),
                Survived = survived,
                Location = deathLocation.Text,
                LocationColor = deathLocation.Color ?? "#FFFFFF",
                KillsThisLife = kills,
            };

            if (killer is not null)
            {
                int killerId = killer.Id;
                RoleTypeId killerRole = DeathClassifier.GetKillerRole(ev, killer);
                bool sameLife = DeathClassifier.IsSameLife(ev, killer);
                string roleSource = "RoleAtHit";

                death.KillerName = ColoredName(killer, killerRole);
                death.KillerColor = NameColor(killerRole);
                death.KillerRole = sameLife ? RoleDisplay.Get(killer, out roleSource) : RoleDisplay.GetBaseRoleName(killerRole);

                if (sameLife)
                {
                    death.KillerHealth = (int)Math.Ceiling(killer.Health);
                    death.KillerMaxHealth = (int)Math.Ceiling(killer.MaxHealth);
                }

                if (DamageDealtTo.TryGetValue(victimId, out Dictionary<int, float>? dealt) && dealt.TryGetValue(killerId, out float damageDealt))
                    death.DamageDealtToKiller = damageDealt;

                if (LastKnownDistance.TryGetValue(victimId, out Dictionary<int, float>? distances) && distances.TryGetValue(killerId, out float lastDistance))
                    death.Distance = Math.Max(lastDistance, 0.1f);

                if (Config.Debug)
                    Log.Debug($"Killer role: '{death.KillerRole}' (source={roleSource}, base={killer.Role.Type}, uniqueRole='{killer.UniqueRole}', customInfo='{killer.CustomInfo}').");
            }

            string text = RecapFormatter.Build(death);
            ShowRecap(victim, text);

            if (Config.Debug)
                Log.Debug($"Recap for {victim.Nickname} (id={victimId}): {System.Text.RegularExpressions.Regex.Replace(text.Replace("\n", " | "), "<.*?>", string.Empty)}");
        }

        private static string NameColor(RoleTypeId role) => role.GetColor().ToHex();

        private static string ColoredName(Player player, RoleTypeId role) => $"<color={NameColor(role)}>{player.Nickname}</color>";

        private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);

        private static List<KeyValuePair<string, float>> BuildBreakdown(int victimId)
        {
            List<KeyValuePair<string, float>> entries = new();
            
            if (DamageTakenFrom.TryGetValue(victimId, out Dictionary<int, float>? attackers))
            {
                foreach (KeyValuePair<int, float> attacker in attackers)
                {
                    AttackerNames.TryGetValue(attacker.Key, out string? name);
                    entries.Add(new KeyValuePair<string, float>(name ?? "Unknown", attacker.Value));
                }
            }

            if (DamageTakenByCause.TryGetValue(victimId, out Dictionary<DamageType, float>? causes))
            {
                foreach (KeyValuePair<DamageType, float> cause in causes)
                    entries.Add(new KeyValuePair<string, float>(Capitalize(CauseNames.Get(cause.Key)), cause.Value));
            }

            return entries.OrderByDescending(entry => entry.Value).ToList();
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (int id in ActiveRecaps.Keys.ToList())
                StopRecap(id);
        }

        private static void ShowRecap(Player player, string text)
        {
            int id = player.Id;
            StopRecap(id);

            ActiveRecaps[id] = Timing.RunCoroutine(RecapLoop(player, text));
        }

        private static IEnumerator<float> RecapLoop(Player player, string text)
        {
            string padding = new('\n', Math.Max(0, Config.HintLinePadding));
            string paddedText = $"{padding}<size={Math.Max(1, Config.HintTextSizePercent)}%>{text}</size>";
            float duration = Config.RecapDurationSeconds;
            float elapsed = 0f;

            while (player.IsConnected && (duration <= 0f || elapsed < duration))
            {
                float remaining = duration > 0f ? duration - elapsed : float.MaxValue;
                player.ShowHint(paddedText, Math.Min(RefreshIntervalSeconds + 1f, remaining));

                float step = Math.Min(RefreshIntervalSeconds, remaining);
                elapsed += step;
                yield return Timing.WaitForSeconds(step);
            }

            ActiveRecaps.Remove(player.Id);
        }

        private static void StopRecap(int id)
        {
            if (!ActiveRecaps.TryGetValue(id, out CoroutineHandle handle))
                return;

            Timing.KillCoroutines(handle);
            ActiveRecaps.Remove(id);

            Player.Get(id)?.ShowHint(" ", 0.1f);
        }
    }
}