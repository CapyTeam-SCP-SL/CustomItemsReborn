// -----------------------------------------------------------------------
// <copyright file="Scp1499.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using CustomPlayerEffects;
using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerStatsSystem;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    /// <summary>
    /// Represents SCP-1499, an item that teleports the user to a designated dimension and returns them after a duration or specific conditions.
    /// </summary>
    public class Scp1499 : CustomItem
    {
        private const float LiftProximitySqr = 100f;
        private readonly Dictionary<Player, Vector3> _tracked = new();
        private float _duration;
        private Vector3 _teleportPosition;

        /// <summary>
        /// Gets the globally unique identifier for this item type.
        /// </summary>
        public override string Id => "SCP-1499";

        /// <summary>
        /// Gets the human-readable name of the item.
        /// </summary>
        public override string Name => "SCP-1499";

        /// <summary>
        /// Gets the underlying item type used for spawning.
        /// </summary>
        public override ItemType BaseType => ItemType.SCP268;

        /// <summary>
        /// Initializes the item by setting configuration values and spawning it in the game world.
        /// </summary>
        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.Scp1499;
            _duration = config.Duration;
            _teleportPosition = config.TeleportPosition;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnInRoom(RoomType.HczHid, Vector3.zero);
        }

        /// <summary>
        /// Subscribes to events for item usage, dropping, player death, destruction, and server waiting.
        /// </summary>
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem += OnUse;
            Exiled.Events.Handlers.Player.DroppingItem += OnDrop;
            Exiled.Events.Handlers.Player.Died += OnDied;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaiting;
        }

        /// <summary>
        /// Unsubscribes from events to prevent memory leaks.
        /// </summary>
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem -= OnUse;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDrop;
            Exiled.Events.Handlers.Player.Died -= OnDied;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaiting;
        }

        /// <summary>
        /// Clears the tracked players when the server is waiting for players.
        /// </summary>
        private void OnWaiting()
        {
            _tracked.Clear();
            Log.Debug("Cleared tracked players for SCP-1499 on server waiting.");
        }

        /// <summary>
        /// Handles item usage to teleport the player to the dimension.
        /// </summary>
        private void OnUse(UsedItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem) || !ev.Player.IsAlive)
                return;

            try
            {
                TeleportToDimension(ev.Player);
                if (_duration > 0)
                    Timing.CallDelayed(_duration, () => SendBack(ev.Player));
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnUse for SCP-1499: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Handles item dropping to return the player from the dimension.
        /// </summary>
        private void OnDrop(DroppingItemEventArgs ev)
        {
            if (!Check(ev.Item) || !ev.Player.IsAlive)
                return;

            try
            {
                if (_tracked.ContainsKey(ev.Player))
                {
                    ev.IsAllowed = false;
                    SendBack(ev.Player);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnDrop for SCP-1499: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Removes a player from tracking when they die.
        /// </summary>
        private void OnDied(DiedEventArgs ev)
        {
            if (_tracked.ContainsKey(ev.Player))
            {
                _tracked.Remove(ev.Player);
                Log.Debug($"Removed player {ev.Player.Nickname} from SCP-1499 tracking due to death.");
            }
        }

        /// <summary>
        /// Removes a player from tracking when they disconnect.
        /// </summary>
        private void OnDestroying(DestroyingEventArgs ev)
        {
            if (_tracked.ContainsKey(ev.Player))
            {
                _tracked.Remove(ev.Player);
                Log.Debug($"Removed player {ev.Player.Nickname} from SCP-1499 tracking due to disconnection.");
            }
        }

        /// <summary>
        /// Teleports the player to the dimension and tracks their original position.
        /// </summary>
        private void TeleportToDimension(Player player)
        {
            if (_tracked.ContainsKey(player))
                _tracked[player] = player.Position;
            else
                _tracked.Add(player, player.Position);

            player.Position = _teleportPosition;
            player.ReferenceHub.playerEffectsController.DisableEffect<Invisible>();
            Log.Debug($"Player {player.Nickname} teleported to dimension at {_teleportPosition}.");
        }

        /// <summary>
        /// Returns the player to their original position, applying damage if in a hazardous area.
        /// </summary>
        private void SendBack(Player player)
        {
            if (player == null || !player.IsAlive || !_tracked.TryGetValue(player, out Vector3 originalPos))
                return;

            try
            {
                player.Position = originalPos;

                if (ShouldKillPlayer(player))
                {
                    ApplyEnvironmentalDamage(player);
                }

                _tracked.Remove(player);
                Log.Debug($"Player {player.Nickname} returned to original position {originalPos}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error in SendBack for SCP-1499: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Determines if the player should be killed based on environmental conditions.
        /// </summary>
        private bool ShouldKillPlayer(Player player)
        {
            if (Warhead.IsDetonated)
            {
                return player.CurrentRoom.Zone != ZoneType.Surface ||
                       Lift.List.Any(lift => lift.Name.Contains("Gate") &&
                       (player.Position - lift.Position).sqrMagnitude <= LiftProximitySqr);
            }

            if (Map.IsLczDecontaminated)
            {
                return player.CurrentRoom.Zone == ZoneType.LightContainment ||
                       Lift.List.Any(lift => lift.Name.Contains("El") &&
                       (player.Position - lift.Position).sqrMagnitude <= LiftProximitySqr);
            }

            return false;
        }

        /// <summary>
        /// Applies environmental damage to the player based on the current hazard.
        /// </summary>
        private void ApplyEnvironmentalDamage(Player player)
        {
            if (Warhead.IsDetonated)
            {
                player.Hurt(new WarheadDamageHandler());
                Log.Debug($"Player {player.Nickname} killed by warhead detonation.");
            }
            else if (Map.IsLczDecontaminated)
            {
                player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Decontamination));
                Log.Debug($"Player {player.Nickname} killed by LCZ decontamination.");
            }
        }
    }
}