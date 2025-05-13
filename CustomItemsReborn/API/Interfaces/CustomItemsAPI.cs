// -----------------------------------------------------------------------
// <copyright file="CustomItemsAPI.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.API.Interfaces;

using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;

/// <summary>
/// Abstract base class for custom items, providing event handling and utilities for item pickup and selection.
/// </summary>
public abstract class CustomItemsAPI
{
    private static readonly HashSet<ushort> CreatedCustomItems = new();

    /// <summary>
    /// Gets the display name of the custom item.
    /// </summary>
    public abstract string ItemName { get; }

    /// <summary>
    /// Gets the base item type used for the custom item.
    /// </summary>
    public abstract ItemType ItemType { get; }

    /// <summary>
    /// Gets the broadcast message displayed when the item is picked up.
    /// </summary>
    public abstract string PickupBroadcast { get; }

    /// <summary>
    /// Gets the hint message displayed when the item is selected.
    /// </summary>
    public abstract string ChangeHint { get; }

    /// <summary>
    /// Gets the set of serial numbers for instances of this custom item.
    /// </summary>
    public abstract HashSet<ushort> ItemList { get; }

    /// <summary>
    /// Initializes the custom item, allowing derived classes to set up initial state.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Creates and spawns the custom item in the game world.
    /// </summary>
    public virtual void CreateCustomItem() { }

    /// <summary>
    /// Subscribes to events for handling custom item interactions.
    /// </summary>
    public void SubscribeToEvents()
    {
        try
        {
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangedItem += OnPlayerChangedItem;
            SubscribeEvents();
            Log.Debug($"Subscribed to events for {ItemName}.");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to subscribe to events for {ItemName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Unsubscribes from events to prevent memory leaks.
    /// </summary>
    public void UnsubscribeToEvents()
    {
        try
        {
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangedItem -= OnPlayerChangedItem;
            UnsubscribeEvents();
            Log.Debug($"Unsubscribed from events for {ItemName}.");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to unsubscribe from events for {ItemName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if an item is a valid custom item of this type.
    /// </summary>
    /// <param name="serial">The serial number of the item.</param>
    /// <param name="itemList">The set of valid serial numbers for this item type.</param>
    /// <returns>True if the item is a valid custom item, otherwise false.</returns>
    public static bool IsSelectedCustomItem(ushort serial, HashSet<ushort> itemList)
    {
        return itemList.Contains(serial); // CreatedCustomItems check is redundant
    }

    /// <summary>
    /// Adds a serial number to the global set of created custom items.
    /// </summary>
    /// <param name="serial">The serial number to add.</param>
    protected static void AddCreatedCustomItem(ushort serial)
    {
        CreatedCustomItems.Add(serial);
    }

    /// <summary>
    /// Validates event arguments and item type for event handlers.
    /// </summary>
    private bool IsValidEvent(Player? player, ItemType itemType, ushort serial)
    {
        return player != null && itemType == ItemType && IsSelectedCustomItem(serial, ItemList);
    }

    /// <summary>
    /// Handles the pickup event to display a broadcast message.
    /// </summary>
    protected virtual void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (ev?.Pickup == null || !IsValidEvent(ev.Player, ev.Pickup.Type, ev.Pickup.Serial))
            return;

        try
        {
            ev.Player.Broadcast(3, PickupBroadcast, global::Broadcast.BroadcastFlags.Normal);
        }
        catch (Exception ex)
        {
            Log.Error($"Error handling pickup for {ItemName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the item selection event to display a hint.
    /// </summary>
    protected virtual void OnPlayerChangedItem(ChangedItemEventArgs ev)
    {
        if (ev?.Item == null || !IsValidEvent(ev.Player, ev.Item.Type, ev.Item.Serial))
            return;

        try
        {
            ev.Player.Hint(ChangeHint, 5f);
        }
        catch (Exception ex)
        {
            Log.Error($"Error handling item selection for {ItemName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Allows derived classes to subscribe to additional events.
    /// </summary>
    protected virtual void SubscribeEvents() { }

    /// <summary>
    /// Allows derived classes to unsubscribe from additional events.
    /// </summary>
    protected virtual void UnsubscribeEvents() { }
}