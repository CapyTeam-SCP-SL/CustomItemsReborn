// -----------------------------------------------------------------------
// <copyright file="AutoGun.cs" company="CapyTeam SCP: SL">
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
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

/// <summary>
/// Represents a custom automatic firearm that targets and shoots enemies within range.
/// </summary>
public class AutoGun : CustomItemsAPI
{
    private readonly SpawnAPI _spawnApi = new();

    /// <inheritdoc/>
    public override string ItemName => "AutoGun";

    /// <inheritdoc/>
    public override ItemType ItemType => ItemType.GunCOM15;

    /// <inheritdoc/>
    public override string PickupBroadcast => "<b>You picked up the AutoGun</b>";

    /// <inheritdoc/>
    public override string ChangeHint => "Fires at all enemies in range with a single trigger pull.";

    /// <inheritdoc/>
    public override HashSet<ushort> ItemList => _serials;

    private readonly HashSet<ushort> _serials = new();

    /// <summary>
    /// Gets or sets a value indicating whether the gun can damage teammates.
    /// </summary>
    public bool TeamKill { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum distance for targeting enemies.
    /// </summary>
    public float MaxDistance { get; set; } = 100f;

    /// <summary>
    /// Gets or sets a value indicating whether ammo is consumed per hit or per shot.
    /// </summary>
    public bool PerHitAmmo { get; set; } = true;

    /// <summary>
    /// Gets or sets the damage per shot.
    /// </summary>
    public float Damage { get; set; } = 25;

    /// <summary>
    /// Gets or sets the clip size.
    /// </summary>
    public byte ClipSize { get; set; } = 5;

    /// <summary>
    /// Spawns the AutoGun in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.Lcz173, new Vector3(0, 1, 0), Quaternion.identity, _serials);
        base.CreateCustomItem();
    }

    /// <summary>
    /// Subscribes to the shooting event.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Shooting += OnShooting;
        base.SubscribeEvents();
    }

    /// <summary>
    /// Unsubscribes from the shooting event.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Shooting -= OnShooting;
        base.UnsubscribeEvents();
    }

    /// <summary>
    /// Handles the shooting event to automatically target enemies in range.
    /// </summary>
    private void OnShooting(ShootingEventArgs ev)
    {
        if (ev.Player?.CurrentItem == null || !IsSelectedCustomItem(ev.Player.CurrentItem.Serial, _serials))
        {
            ev.IsAllowed = false;
            return;
        }

        if (ev.Player.CurrentItem is not Firearm firearm)
        {
            ev.IsAllowed = false;
            return;
        }

        try
        {
            int ammoUsed = 0;
            Vector3 forward = ev.Player.CameraTransform.forward;

            foreach (Player target in Player.List)
            {
                if (target == null ||
                    target.Role == RoleTypeId.Scp079 ||
                    target.Role == RoleTypeId.Spectator ||
                    target == ev.Player ||
                    (!TeamKill && target.Role.Side == ev.Player.Role.Side) ||
                    Vector3.Distance(ev.Player.Position, target.Position) > MaxDistance ||
                    (PerHitAmmo && firearm.MagazineAmmo <= ammoUsed))
                    continue;

                if (Physics.Raycast(ev.Player.CameraTransform.position + forward, forward, out var hit, MaxDistance) &&
                    hit.collider.gameObject.GetComponentInParent<Player>() != target)
                    continue;

                ammoUsed++;
                target.Hurt(Damage, DamageType.AK, null);
                if (target.IsDead)
                    target.Hint("<color=#FF0000>YOU HAVE BEEN KILLED BY AUTO AIM GUN</color>", 3f);
                ev.Player.ShowHitMarker(1f);
            }

            firearm.MagazineAmmo -= (byte)(PerHitAmmo ? ammoUsed : 1);
            ev.IsAllowed = false;
        }
        catch (Exception ex)
        {
            Log.Error($"Error in AutoGun.OnShooting: {ex.Message}");
        }
    }
}