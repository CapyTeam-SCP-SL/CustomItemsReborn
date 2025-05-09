// -----------------------------------------------------------------------
// <copyright file="LuckyCoin.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.Items;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using CustomItemsReborn.API;
using CustomItemsReborn.API.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;
using ItemEx = Exiled.API.Features.Items.Item;

/// <summary>
/// Represents the Lucky Coin (LC-119), an item that triggers a special effect when dropped in SCP-106's pocket dimension.
/// </summary>
public class LuckyCoin : CustomItemsAPI
{
    // Constants for spawn position and timing
    private const float SpawnHeightOffset = 1.2f;
    private const float MinDuration = 1f;
    private const float DefaultCooldownDuration = 120f;

    /// <summary>
    /// Stores serial numbers of all Lucky Coin instances for efficient lookup.
    /// </summary>
    private readonly HashSet<ushort> _coinList = [];

    /// <summary>
    /// Reference to the spawn API for creating pickups.
    /// </summary>
    private readonly SpawnAPI _spawnApi;

    /// <summary>
    /// Tracks whether the coin has been dropped in the pocket dimension.
    /// </summary>
    private bool _dropped;

    /// <summary>
    /// Tracks whether the coin effect is on cooldown.
    /// </summary>
    private bool _cooldown;

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public override string ItemName => "LC-119";

    /// <summary>
    /// Gets the base item type used for the Lucky Coin.
    /// </summary>
    public override ItemType ItemType => ItemType.Coin;

    /// <summary>
    /// Gets the broadcast message shown when the item is picked up.
    /// </summary>
    public override string PickupBroadcast => "<b>You picked up LC-119</b>";

    /// <summary>
    /// Gets the hint shown when the item is selected.
    /// </summary>
    public override string ChangeHint => "Drop it inside SCP-106 pocket dimension.";

    /// <summary>
    /// Gets the List of serial numbers for tracking instances of this item.
    /// </summary>
    public override HashSet<ushort> ItemList => [.. _coinList];

    /// <summary>
    /// Gets or sets the duration the spawned coin stays in the pocket dimension.
    /// </summary>
    [Description("How long the spawned coin stays inside PD.")]
    public float Duration
    {
        get => _duration;
        set => _duration = Math.Max(value, MinDuration);
    }
    private float _duration = 10f;

    /// <summary>
    /// Gets or sets the cooldown duration in seconds before the effect can trigger again.
    /// </summary>
    [Description("Cooldown duration in seconds before the effect can trigger again.")]
    public float CooldownDuration { get; set; } = DefaultCooldownDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="LuckyCoin"/> class.
    /// </summary>
    public LuckyCoin()
    {
        _spawnApi = new SpawnAPI();
    }

    /// <summary>
    /// Spawns Lucky Coin pickups in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        try
        {
            if (_spawnApi == null)
            {
                Log.Error("SpawnAPI is null in LuckyCoin.CreateCustomItem.");
                return;
            }

            _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.Lcz330, Vector3.zero, Quaternion.identity, _coinList);
            _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.HczArmory, Vector3.zero, Quaternion.identity, _coinList);
            base.CreateCustomItem();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to create Lucky Coin item: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Subscribes to pickup, drop, and pocket dimension entry events.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.PickingUpItem += OnPickup;
        Exiled.Events.Handlers.Player.DroppingItem += OnDrop;
        Exiled.Events.Handlers.Player.EnteringPocketDimension += OnEnterPd;
        base.SubscribeEvents();
        Log.Debug("Subscribed to events for Lucky Coin.");
    }

    /// <summary>
    /// Unsubscribes from pickup, drop, and pocket dimension entry events.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.PickingUpItem -= OnPickup;
        Exiled.Events.Handlers.Player.DroppingItem -= OnDrop;
        Exiled.Events.Handlers.Player.EnteringPocketDimension -= OnEnterPd;
        base.UnsubscribeEvents();
        Log.Debug("Unsubscribed from events for Lucky Coin.");
    }

    /// <summary>
    /// Prevents picking up the coin in the pocket dimension.
    /// </summary>
    /// <param name="ev">The pickup event arguments.</param>
    private void OnPickup(PickingUpItemEventArgs ev)
    {
        if (ev?.Pickup == null || ev.Player == null || !ev.Player.IsAlive)
            return;

        try
        {
            if (ev.Pickup.Type == ItemType.Coin && ev.Player.CurrentRoom.Type == RoomType.Pocket)
            {
                ev.IsAllowed = false;
                Log.Debug($"Prevented player {ev.Player.Nickname} from picking up coin in pocket dimension.");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnPickup for Lucky Coin: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Handles dropping the coin in the pocket dimension to trigger its effect.
    /// </summary>
    /// <param name="ev">The drop event arguments.</param>
    private void OnDrop(DroppingItemEventArgs ev)
    {
        if (!IsValidDropEvent(ev))
            return;

        try
        {
            ev.IsAllowed = false;
            _dropped = true;
            ev.Player.RemoveItem(ev.Item);
            Log.Debug($"Player {ev.Player.Nickname} dropped Lucky Coin in pocket dimension.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnDrop for Lucky Coin: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Spawns a new coin in the pocket dimension when a player enters, if conditions are met.
    /// </summary>
    /// <param name="ev">The pocket dimension entry event arguments.</param>
    private void OnEnterPd(EnteringPocketDimensionEventArgs ev)
    {
        if (ev?.Player == null || !ev.Player.IsAlive || _cooldown || !_dropped)
            return;

        try
        {
            _dropped = false;
            _cooldown = true;

            SpawnCoinInPocketDimension();
            Timing.CallDelayed(CooldownDuration, () => _cooldown = false);
            Log.Debug($"Player {ev.Player.Nickname} triggered Lucky Coin effect in pocket dimension.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnEnterPd for Lucky Coin: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Validates the drop event for Lucky Coin functionality.
    /// </summary>
    /// <param name="ev">The drop event arguments.</param>
    /// <returns>True if the event is valid, otherwise false.</returns>
    private bool IsValidDropEvent(DroppingItemEventArgs ev)
    {
        return ev?.Player != null &&
               ev.Player.IsAlive &&
               ev.Item != null &&
               IsSelectedCustomItem(ev.Item.Serial, _coinList) &&
               ev.Player.CurrentRoom.Type == RoomType.Pocket;
    }

    /// <summary>
    /// Spawns a new coin in the pocket dimension and schedules its destruction.
    /// </summary>
    private void SpawnCoinInPocketDimension()
    {
        Room pocketDimension = Room.Get(RoomType.Pocket);
        if (pocketDimension == null)
        {
            Log.Error("Failed to find pocket dimension room for Lucky Coin effect.");
            return;
        }

        Vector3 spawnPos = pocketDimension.Position + Vector3.up * SpawnHeightOffset;
        Pickup pickup = ItemEx.Create(ItemType.Coin).CreatePickup(spawnPos, Quaternion.identity);

        if (pickup != null)
        {
            Timing.CallDelayed(Duration, () =>
            {
                pickup?.Destroy();
                Log.Debug("Destroyed spawned Lucky Coin in pocket dimension.");
            });
        }
        else
        {
            Log.Error("Failed to create Lucky Coin pickup in pocket dimension.");
        }
    }
}