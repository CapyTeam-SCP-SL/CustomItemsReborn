// -----------------------------------------------------------------------
// <copyright file="SniperRifle.cs" company="CapyTeam SCP: SL">
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
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using PlayerStatsSystem;
using UnityEngine;

/// <summary>
/// Represents a modified E-11 rifle that fires high-velocity sniper rounds.
/// </summary>
public class SniperRifle : CustomItemsAPI
{
    private readonly SpawnAPI _spawnApi = new();

    /// <inheritdoc/>
    public override string ItemName => "SR-119";

    /// <inheritdoc/>
    public override ItemType ItemType => ItemType.GunE11SR;

    /// <inheritdoc/>
    public override string PickupBroadcast => "<b>You picked up the SR-119 Sniper Rifle</b>";

    /// <inheritdoc/>
    public override string ChangeHint => "Fires high-velocity anti-personnel sniper rounds.";

    /// <inheritdoc/>
    public override HashSet<ushort> ItemList => _serials;

    private readonly HashSet<ushort> _serials = new();

    /// <summary>
    /// Gets or sets the damage multiplier for the sniper rifle.
    /// </summary>
    public float DamageMultiplier { get; set; } = 7.5f;

    /// <summary>
    /// Gets or sets the clip size.
    /// </summary>
    public byte ClipSize { get; set; } = 1;

    /// <summary>
    /// Gets the attachments for the sniper rifle.
    /// </summary>
    public AttachmentName[] Attachments { get; } = new[]
    {
        AttachmentName.ExtendedBarrel,
        AttachmentName.ScopeSight,
    };

    /// <summary>
    /// Spawns the Sniper Rifle in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.HczHid, new Vector3(0, 1, 0), Quaternion.identity, _serials);
        _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.HczArmory, new Vector3(0, 1, 0), Quaternion.identity, _serials);
        base.CreateCustomItem();
    }

    /// <summary>
    /// Subscribes to the hurting event.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        base.SubscribeEvents();
    }

    /// <summary>
    /// Unsubscribes from the hurting event.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        base.UnsubscribeEvents();
    }

    /// <summary>
    /// Applies the damage multiplier when the sniper rifle is used.
    /// </summary>
    private void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Attacker == null || ev.Attacker == ev.Player || ev.DamageHandler.Base is not FirearmDamageHandler firearmDamageHandler)
            return;

        if (!IsSelectedCustomItem(ev.Attacker.CurrentItem.Serial, _serials) || firearmDamageHandler.WeaponType != ItemType)
            return;

        ev.Amount *= DamageMultiplier;
    }

    /// <summary>
    /// Initializes the sniper rifle with attachments.
    /// </summary>
    public override void Initialize()
    {
        foreach (var serial in _serials)
        {
            if (Item.Get(serial) is Firearm firearm)
            {
                foreach (var attachment in Attachments)
                    firearm.AddAttachment(attachment);
            }
        }
        base.Initialize();
    }
}