// -----------------------------------------------------------------------
// <copyright file="DeflectorShield.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.Items;

using System;
using System.Collections.Generic;
using CustomItemsReborn.API;
using CustomItemsReborn.API.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerStatsSystem;
using UnityEngine;

/// <summary>
/// Represents a deflector shield that reflects bullets back at the shooter.
/// </summary>
public class DeflectorShield : CustomItemsAPI
{
    private readonly SpawnAPI _spawnApi = new();
    private readonly List<Player> _deflectorPlayers = new();

    /// <inheritdoc/>
    public override string ItemName => "Deflector Shield";

    /// <inheritdoc/>
    public override ItemType ItemType => ItemType.SCP268;

    /// <inheritdoc/>
    public override string PickupBroadcast => "<b>You picked up the Deflector Shield</b>";

    /// <inheritdoc/>
    public override string ChangeHint => "Reflects bullets back at the shooter.";

    /// <inheritdoc/>
    public override HashSet<ushort> ItemList => _serials;

    private readonly HashSet<ushort> _serials = new();

    /// <summary>
    /// Gets or sets how long the deflector shield can be worn before automatically being removed (set to 0 for no limit).
    /// </summary>
    public float Duration { get; set; } = 15f;

    /// <summary>
    /// Gets or sets the damage multiplier for reflected bullets.
    /// </summary>
    public float Multiplier { get; set; } = 1f;

    /// <summary>
    /// Spawns the Deflector Shield in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.HczHid, new Vector3(0, 1, 0), Quaternion.identity, _serials);
        base.CreateCustomItem();
    }

    /// <summary>
    /// Subscribes to necessary events.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem += OnItemUsed;
        Exiled.Events.Handlers.Player.Destroying += OnDestroying;
        Exiled.Events.Handlers.Player.Hurting += OnHurt;
        Exiled.Events.Handlers.Player.DroppingItem += OnDropping;
        Exiled.Events.Handlers.Player.Dying += OnDying;
        base.SubscribeEvents();
    }

    /// <summary>
    /// Unsubscribes from events.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem -= OnItemUsed;
        Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
        Exiled.Events.Handlers.Player.Hurting -= OnHurt;
        Exiled.Events.Handlers.Player.DroppingItem -= OnDropping;
        Exiled.Events.Handlers.Player.Dying -= OnDying;
        base.UnsubscribeEvents();
    }

    /// <summary>
    /// Clears the list of players with active shields at the start of a new round.
    /// </summary>
    public override void Initialize()
    {
        _deflectorPlayers.Clear();
        base.Initialize();
    }

    /// <summary>
    /// Prevents dropping the shield if active.
    /// </summary>
    private void OnDropping(DroppingItemEventArgs ev)
    {
        if (!IsSelectedCustomItem(ev.Item.Serial, _serials) || !_deflectorPlayers.Contains(ev.Player))
            return;

        ev.IsAllowed = false;
        _deflectorPlayers.Remove(ev.Player);
    }

    /// <summary>
    /// Removes the player from the active shield list when they die.
    /// </summary>
    private void OnDying(DyingEventArgs ev)
    {
        if (_deflectorPlayers.Contains(ev.Player))
            _deflectorPlayers.Remove(ev.Player);
    }

    /// <summary>
    /// Removes the player from the active shield list when they disconnect.
    /// </summary>
    private void OnDestroying(DestroyingEventArgs ev)
    {
        if (_deflectorPlayers.Contains(ev.Player))
            _deflectorPlayers.Remove(ev.Player);
    }

    /// <summary>
    /// Activates the shield when the item is used.
    /// </summary>
    private void OnItemUsed(UsedItemEventArgs ev)
    {
        if (!IsSelectedCustomItem(ev.Player.CurrentItem.Serial, _serials))
            return;

        if (!_deflectorPlayers.Contains(ev.Player))
            _deflectorPlayers.Add(ev.Player);

        ev.Player.DisableEffect(EffectType.Invisible);

        if (Duration > 0)
        {
            Timing.CallDelayed(Duration, () =>
            {
                _deflectorPlayers.Remove(ev.Player);
            });
        }
    }

    /// <summary>
    /// Reflects bullet damage back to the attacker if the shield is active.
    /// </summary>
    private void OnHurt(HurtingEventArgs ev)
    {
        if (!_deflectorPlayers.Contains(ev.Player) || ev.DamageHandler.Base is not FirearmDamageHandler || ev.Player == ev.Attacker)
            return;

        ev.IsAllowed = false;
        ev.Attacker.Hurt(ev.Player, ev.Amount * Multiplier, ev.DamageHandler.Type, DamageHandlerBase.CassieAnnouncement.Default);
    }
}