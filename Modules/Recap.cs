namespace DeathRecap.Modules
{
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.DamageHandlers;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
        private static readonly Dictionary<int, CancellationTokenSource> ActiveRecaps = new();

        private static readonly Dictionary<string, string> BadgeColorHex = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pink"] = "#FF96DE",
            ["red"] = "#C50000",
            ["brown"] = "#944710",
            ["silver"] = "#A0A0A0",
            ["light_green"] = "#32CD32",
            ["crimson"] = "#DC143C",
            ["cyan"] = "#00B7EB",
            ["aqua"] = "#00FFFF",
            ["deep_pink"] = "#FF1493",
            ["tomato"] = "#FF6448",
            ["yellow"] = "#FAFF86",
            ["magenta"] = "#FF0090",
            ["blue_green"] = "#4DFFB8",
            ["orange"] = "#FF9966",
            ["lime"] = "#8FFF00",
            ["green"] = "#228B22",
            ["emerald"] = "#50C878",
            ["carmine"] = "#960018",
            ["nickel"] = "#727472",
            ["mint"] = "#98F898",
            ["army_green"] = "#4B5320",
            ["pumpkin"] = "#EE7600",
        };

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
            Exiled.Events.Handlers.Player.Hurt += OnPlayerHurt;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
            Exiled.Events.Handlers.Player.Hurt -= OnPlayerHurt;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private static void OnWaitingForPlayers()
        {
            DamageTakenFrom.Clear();
            DamageDealtTo.Clear();
            LastKnownDistance.Clear();
            HealthBeforeHit.Clear();

            foreach (int id in ActiveRecaps.Keys.ToList())
                StopRecap(id);
        }

        private static void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player.Role.Team == Team.Dead)
                return;

            int id = ev.Player.Id;
            DamageTakenFrom[id] = new Dictionary<int, float>();
            DamageDealtTo[id] = new Dictionary<int, float>();
            LastKnownDistance[id] = new Dictionary<int, float>();

            foreach (Dictionary<int, float> inner in DamageTakenFrom.Values)
                inner.Remove(id);
            foreach (Dictionary<int, float> inner in DamageDealtTo.Values)
                inner.Remove(id);
            foreach (Dictionary<int, float> inner in LastKnownDistance.Values)
                inner.Remove(id);

            StopRecap(id);
        }

        private static void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker is null || ev.Player is null || ev.Attacker == ev.Player)
                return;

            if (ev.Amount <= 0)
                return;

            HealthBeforeHit[ev.Player.Id] = ev.Player.Health;

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
            if (ev.Attacker is null || ev.Player is null || ev.Attacker == ev.Player)
                return;

            if (!HealthBeforeHit.TryGetValue(ev.Player.Id, out float healthBefore))
                return;

            float actualDamage = healthBefore - ev.Player.Health;
            if (actualDamage <= 0)
                return;

            int attackerId = ev.Attacker.Id;
            int victimId = ev.Player.Id;

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

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player is null || ev.Attacker is null || ev.Attacker == ev.Player)
                return;

            Player victim = ev.Player;
            Player killer = ev.Attacker;
            int victimId = victim.Id;
            int killerId = killer.Id;

            DamageTakenFrom.TryGetValue(victimId, out Dictionary<int, float>? victimIncoming);
            float damageTaken = victimIncoming is not null && victimIncoming.TryGetValue(killerId, out float dt) ? dt : 0f;

            DamageDealtTo.TryGetValue(victimId, out Dictionary<int, float>? victimOutgoing);
            float damageDealt = victimOutgoing is not null && victimOutgoing.TryGetValue(killerId, out float dd) ? dd : 0f;

            string weapon = GetWeapon(ev.DamageHandler);

            float distance = 0f;
            if (LastKnownDistance.TryGetValue(victimId, out Dictionary<int, float>? victimDistances) && victimDistances.TryGetValue(killerId, out float lastDistance))
                distance = Math.Max(lastDistance, 0.1f);

            string distanceFormatted = distance.ToString("F2");

            bool hasBadge = !string.IsNullOrEmpty(killer.RankColor) && killer.RankColor != "default";
            string nameColor = hasBadge && BadgeColorHex.TryGetValue(killer.RankColor, out string? badgeHex)
                ? badgeHex
                : killer.Role.Type.GetColor().ToHex();
            string coloredName = $"<color={nameColor}>{killer.Nickname}</color>";

            string text = string.Format(
                Translation.RecapText,
                coloredName,
                weapon,
                distanceFormatted,
                (int)damageTaken,
                (int)damageDealt);

            ShowRecap(victim, text);

            if (Config.Debug)
                Log.Debug($"Recap for {victim.Nickname} (id={victimId}): killer={killer.Nickname} (id={killerId}), weapon: {weapon}, distance={distance:F1}, damageTaken={damageTaken:F1}, damageDealt={damageDealt:F1}.");
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (int id in ActiveRecaps.Keys.ToList())
                StopRecap(id);
        }

        private static string GetWeapon(DamageHandlerBase damageHandler)
        {
            if (damageHandler is FirearmDamageHandler firearmHandler)
                return firearmHandler.Item?.Type.ToString() ?? "Unknown";

            return damageHandler.Type.ToString();
        }

        private static async void ShowRecap(Player player, string text)
        {
            int id = player.Id;
            StopRecap(id);

            CancellationTokenSource cts = new();
            ActiveRecaps[id] = cts;

            string padding = new('\n', Config.HintLinePadding);
            string paddedText = $"{padding}<size={Config.HintTextSizePercent}%>{text}</size>";

            try
            {
                float elapsed = 0f;
                while (!cts.IsCancellationRequested)
                {
                    player.ShowHint(paddedText, RefreshIntervalSeconds + 1f);

                    if (Config.RecapDurationSeconds > 0)
                    {
                        elapsed += RefreshIntervalSeconds;
                        if (elapsed >= Config.RecapDurationSeconds)
                            break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(RefreshIntervalSeconds), cts.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when cancelled - not an error.
            }
            finally
            {
                ActiveRecaps.Remove(id);
            }
        }

        private static void StopRecap(int id)
        {
            if (ActiveRecaps.TryGetValue(id, out CancellationTokenSource? cts))
            {
                cts.Cancel();
                ActiveRecaps.Remove(id);

                Player? player = Player.Get(id);
                player?.ShowHint(" ", 0.1f);
            }
        }
    }
}