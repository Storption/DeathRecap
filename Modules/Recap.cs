namespace DeathRecap.Modules
{
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.DamageHandlers;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;
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

        private static readonly Dictionary<Player, Dictionary<Player, float>> DamageTakenFrom = new();
        private static readonly Dictionary<Player, Dictionary<Player, float>> DamageDealtTo = new();
        private static readonly Dictionary<Player, Dictionary<Player, float>> LastKnownDistance = new();
        private static readonly Dictionary<Player, CancellationTokenSource> ActiveRecaps = new();

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private static void OnWaitingForPlayers()
        {
            DamageTakenFrom.Clear();
            DamageDealtTo.Clear();
            LastKnownDistance.Clear();

            foreach (Player player in ActiveRecaps.Keys.ToList())
                StopRecap(player);
        }

        private static void OnSpawned(SpawnedEventArgs ev)
        {
            DamageTakenFrom[ev.Player] = new Dictionary<Player, float>();
            DamageDealtTo[ev.Player] = new Dictionary<Player, float>();
            LastKnownDistance[ev.Player] = new Dictionary<Player, float>();

            StopRecap(ev.Player);
        }

        private static void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker is null || ev.Player is null || ev.Attacker == ev.Player)
                return;

            if (!DamageTakenFrom.TryGetValue(ev.Player, out Dictionary<Player, float>? incoming))
            {
                incoming = new Dictionary<Player, float>();
                DamageTakenFrom[ev.Player] = incoming;
            }

            incoming.TryGetValue(ev.Attacker, out float currentIncoming);
            incoming[ev.Attacker] = currentIncoming + ev.Amount;

            if (!DamageDealtTo.TryGetValue(ev.Attacker, out Dictionary<Player, float>? outgoing))
            {
                outgoing = new Dictionary<Player, float>();
                DamageDealtTo[ev.Attacker] = outgoing;
            }

            outgoing.TryGetValue(ev.Player, out float currentOutgoing);
            outgoing[ev.Player] = currentOutgoing + ev.Amount;

            if (!LastKnownDistance.TryGetValue(ev.Player, out Dictionary<Player, float>? distances))
            {
                distances = new Dictionary<Player, float>();
                LastKnownDistance[ev.Player] = distances;
            }

            distances[ev.Attacker] = Vector3.Distance(ev.Attacker.Position, ev.Player.Position);
        }

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player is null || ev.Attacker is null || ev.Attacker == ev.Player)
                return;

            Player victim = ev.Player;
            Player killer = ev.Attacker;

            DamageTakenFrom.TryGetValue(victim, out Dictionary<Player, float>? victimIncoming);
            float damageTaken = victimIncoming is not null && victimIncoming.TryGetValue(killer, out float dt) ? dt : 0f;

            DamageDealtTo.TryGetValue(victim, out Dictionary<Player, float>? victimOutgoing);
            float damageDealt = victimOutgoing is not null && victimOutgoing.TryGetValue(killer, out float dd) ? dd : 0f;

            string weapon = GetWeapon(ev.DamageHandler);

            float distance = 0f;
            if (LastKnownDistance.TryGetValue(victim, out Dictionary<Player, float>? victimDistances) && victimDistances.TryGetValue(killer, out float lastDistance))
                distance = Math.Max(lastDistance, 0.1f);

            string roleColorHex = killer.Role.Type.GetColor().ToHex();
            string coloredName = $"<color={roleColorHex}>{killer.Nickname}</color>";

            string text = string.Format(
                Translation.RecapText,
                coloredName,
                weapon,
                distance,
                (int)damageTaken,
                (int)damageDealt);

            ShowRecap(victim, text);

            if (Config.Debug)
                Log.Debug($"Recap for {victim.Nickname}: killer={killer.Nickname}, weapon: {weapon}, distance={distance:F1}, damageTaken={damageTaken:F1}, damageDealt={damageDealt:F1}.");
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (Player player in ActiveRecaps.Keys.ToList())
                StopRecap(player);
        }

        private static string GetWeapon(DamageHandlerBase damageHandler)
        {
            if (damageHandler is FirearmDamageHandler firearmHandler)
                return firearmHandler.Item?.Type.ToString() ?? "Unknown";

            return damageHandler.Type.ToString();
        }

        private static async void ShowRecap(Player player, string text)
        {
            StopRecap(player);

            CancellationTokenSource cts = new();
            ActiveRecaps[player] = cts;

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
                ActiveRecaps.Remove(player);
            }
        }

        private static void StopRecap(Player player)
        {
            if (ActiveRecaps.TryGetValue(player, out CancellationTokenSource? cts))
            {
                cts.Cancel();
                ActiveRecaps.Remove(player);
            }
        }
    }
}