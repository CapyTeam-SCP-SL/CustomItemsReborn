// -----------------------------------------------------------------------
// <copyright file="CustomItemsAPI.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItems.API;

using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;

/// <summary>
/// Abstract base class for managing custom items, providing event handling for item pickup and selection.
/// </summary>
public abstract class CustomItemsAPI
{
    /// <summary>
    /// Gets the name of the custom item.
    /// </summary>
    public abstract string ItemName { get; }

    /// <summary>
    /// Gets the type of the custom item.
    /// </summary>
    public abstract ItemType ItemType { get; }

    /// <summary>
    /// Gets the broadcast message shown when the item is picked up.
    /// </summary>
    public abstract string PickupBroadcast { get; }

    /// <summary>
    /// Gets the hint message shown when the item is selected.
    /// </summary>
    public abstract string ChangeHint { get; }

    /// <summary>
    /// Gets the list of serial numbers for this custom item type.
    /// </summary>
    protected abstract List<ushort> ItemList { get; }

    /// <summary>
    /// Gets the list of serial numbers for all created custom items.
    /// </summary>
    public static List<ushort> CreatedCustomItems { get; } = new List<ushort>();

    /// <summary>
    /// Subscribes to necessary events for handling custom item interactions.
    /// </summary>
    public virtual void SubscribeEvents()
    {
        try
        {
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangedItem += OnPlayerChangedItem;
            Log.Debug($"Subscribed to events for {ItemName} custom item.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error subscribing to events for {ItemName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Unsubscribes from all events to prevent memory leaks.
    /// </summary>
    public virtual void UnsubscribeEvents()
    {
        try
        {
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangedItem -= OnPlayerChangedItem;
            Log.Debug($"Unsubscribed from events for {ItemName} custom item.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error unsubscribing from events for {ItemName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if an item serial corresponds to this custom item type.
    /// </summary>
    /// <param name="serial">The serial number of the item.</param>
    /// <param name="itemList">The list of valid serial numbers for this item type.</param>
    /// <returns>True if the item is a valid custom item of this type, otherwise false.</returns>
    public static bool IsSelectedCustomItem(ushort serial, List<ushort> itemList)
    {
        return CreatedCustomItems.Contains(serial) && itemList.Contains(serial);
    }

    /// <summary>
    /// Handles the event when a player picks up an item.
    /// </summary>
    /// <param name="ev">The event arguments.</param>
    protected virtual void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (ev == null || ev.Player == null || ev.Pickup == null)
            return;

        try
        {
            if (ev.Pickup.Type != ItemType || !IsSelectedCustomItem(ev.Pickup.Serial, ItemList))
                return;

            ev.Player.Broadcast(3, PickupBroadcast, global::Broadcast.BroadcastFlags.Normal);
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnPickingUpItem for {ItemName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the event when a player changes their selected item.
    /// </summary>
    /// <param name="ev">The event arguments.</param>
    protected virtual void OnPlayerChangedItem(ChangedItemEventArgs ev)
    {
        if (ev == null || ev.Player == null || ev.Item == null)
            return;

        try
        {
            if (ev.Item.Type != ItemType || !IsSelectedCustomItem(ev.Item.Serial, ItemList))
                return;

            ev.Player.ShowHint(ChangeHint, 5f);
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnPlayerChangedItem for {ItemName}: {ex.Message}");
        }
    }
}