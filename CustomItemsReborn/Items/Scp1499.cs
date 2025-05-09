// -----------------------------------------------------------------------
// <copyright file="Scp1499.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.Items;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CustomItemsReborn.API;
using CustomItemsReborn.API.Interfaces;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerStatsSystem;
using UnityEngine;

/// <summary>
/// Represents SCP-1499, an item that teleports the user to a designated dimension and returns them after a duration or specific conditions.
/// </summary>
public class Scp1499 : CustomItemsAPI
{
    // Constants for teleportation and safety checks
    private const float DefaultTeleportX = 38.464f;
    private const float DefaultTeleportY = 1014.112f;
    private const float DefaultTeleportZ = -32.689f;
    private const float LiftProximitySqr = 100f;

    /// <summary>
    /// Tracks players and their original positions when in the dimension.
    /// </summary>
    private readonly Dictionary<Player, Vector3> _tracked = new();

    /// <summary>
    /// Stores serial numbers of all SCP-1499 instances for efficient lookup.
    /// </summary>
    private readonly HashSet<ushort> _serials = [];

    /// <summary>
    /// Reference to the spawn API for creating pickups.
    /// </summary>
    private readonly SpawnAPI _spawnApi;

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public override string ItemName => "SCP-1499";

    /// <summary>
    /// Gets the base item type used for SCP-1499.
    /// </summary>
    public override ItemType ItemType => ItemType.SCP268;

    /// <summary>
    /// Gets the broadcast message shown when the item is picked up.
    /// </summary>
    public override string PickupBroadcast => "<b>You picked up SCP-1499</b>";

    /// <summary>
    /// Gets the hint shown when the item is selected.
    /// </summary>
    public override string ChangeHint => "Use to enter another dimension.";

    /// <summary>
    /// Gets the List of serial numbers for tracking instances of this item.
    /// </summary>
    public override HashSet<ushort> ItemList => [.. _serials];

    /// <summary>
    /// Gets or sets the maximum time in seconds the player can stay in the dimension (0 for no limit).
    /// </summary>
    [Description("Max time in seconds inside dimension (0 = no limit).")]
    public float Duration { get; set; } = 15f;

    /// <summary>
    /// Gets or sets the teleport position for the dimension.
    /// </summary>
    [Description("Teleport position for dimension.")]
    public Vector3 TeleportPosition { get; set; } = new(DefaultTeleportX, DefaultTeleportY, DefaultTeleportZ);

    /// <summary>
    /// Initializes a new instance of the <see cref="Scp1499"/> class.
    /// </summary>
    public Scp1499()
    {
        _spawnApi = new SpawnAPI();
    }

    /// <summary>
    /// Spawns an SCP-1499 pickup in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        try
        {
            if (_spawnApi == null)
            {
                Log.Error("SpawnAPI is null in Scp1499.CreateCustomItem.");
                return;
            }

            _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.HczHid, Vector3.zero, Quaternion.identity, _serials);
            base.CreateCustomItem();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to create SCP-1499 item: {ex.Message}\n{ex.StackTrace}");
        }
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
        base.SubscribeEvents();
        Log.Debug("Subscribed to events for SCP-1499.");
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
        base.UnsubscribeEvents();
        Log.Debug("Unsubscribed from events for SCP-1499.");
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
    /// <param name="ev">The item usage event arguments.</param>
    private void OnUse(UsedItemEventArgs ev)
    {
        if (!IsValidEvent(ev, ev?.Player?.CurrentItem?.Serial ?? 0))
            return;

        try
        {
            TeleportToDimension(ev.Player);
            if (Duration > 0)
                Timing.CallDelayed(Duration, () => SendBack(ev.Player));
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnUse for SCP-1499: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Handles item dropping to return the player from the dimension.
    /// </summary>
    /// <param name="ev">The drop event arguments.</param>
    private void OnDrop(DroppingItemEventArgs ev)
    {
        if (!IsValidEvent(ev, ev?.Item?.Serial ?? 0))
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
    /// <param name="ev">The death event arguments.</param>
    private void OnDied(DiedEventArgs ev)
    {
        if (ev?.Player == null || !_tracked.ContainsKey(ev.Player))
            return;

        _tracked.Remove(ev.Player);
        Log.Debug($"Removed player {ev.Player.Nickname} from SCP-1499 tracking due to death.");
    }

    /// <summary>
    /// Removes a player from tracking when they disconnect.
    /// </summary>
    /// <param name="ev">The destruction event arguments.</param>
    private void OnDestroying(DestroyingEventArgs ev)
    {
        if (ev?.Player == null || !_tracked.ContainsKey(ev.Player))
            return;

        _tracked.Remove(ev.Player);
        Log.Debug($"Removed player {ev.Player.Nickname} from SCP-1499 tracking due to disconnection.");
    }

    /// <summary>
    /// Validates event arguments and item serial for event handlers.
    /// </summary>
    /// <param name="ev">The event arguments.</param>
    /// <param name="serial">The item serial number.</param>
    /// <returns>True if the event is valid for SCP-1499, otherwise false.</returns>
    private bool IsValidEvent(object ev, ushort serial)
    {
        return ev switch
        {
            UsedItemEventArgs useEv => useEv.Player?.CurrentItem != null && IsSelectedCustomItem(serial, _serials) && useEv.Player.IsAlive,
            DroppingItemEventArgs dropEv => dropEv.Item != null && IsSelectedCustomItem(serial, _serials) && dropEv.Player.IsAlive,
            _ => false
        };
    }

    /// <summary>
    /// Teleports the player to the dimension and tracks their original position.
    /// </summary>
    /// <param name="player">The player to teleport.</param>
    private void TeleportToDimension(Player player)
    {
        if (_tracked.ContainsKey(player))
            _tracked[player] = player.Position;
        else
            _tracked.Add(player, player.Position);

        player.Position = TeleportPosition;
        player.ReferenceHub.playerEffectsController.DisableEffect<Invisible>();
        Log.Debug($"Player {player.Nickname} teleported to dimension at {TeleportPosition}.");
    }

    /// <summary>
    /// Returns the player to their original position, applying damage if in a hazardous area.
    /// </summary>
    /// <param name="player">The player to return.</param>
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
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player should be killed, otherwise false.</returns>
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
    /// <param name="player">The player to damage.</param>
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