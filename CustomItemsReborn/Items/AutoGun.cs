// -----------------------------------------------------------------------
// <copyright file="AutoGun.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItems.Items;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

/// <summary>
/// Represents a custom automatic firearm that targets and shoots enemies within range.
/// </summary>
[CustomItem(ItemType.GunCOM15)]
public class AutoGun : CustomWeapon
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 17;

    /// <inheritdoc/>
    public override string Name { get; set; } = "AutoGun";

    /// <inheritdoc/>
    public override string Description { get; set; } = "Fires at all enemies in range with a single trigger pull.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 2.35f;

    /// <inheritdoc/>
    public override bool ShouldMessageOnGban => true;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new() { Chance = 100, Location = SpawnLocationType.Inside173Armory },
        },
    };

    /// <inheritdoc/>
    public override float Damage { get; set; } = 25;

    /// <inheritdoc/>
    public override byte ClipSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether the gun can damage teammates.
    /// </summary>
    [Description("Whether the gun can damage teammates.")]
    public bool TeamKill { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum distance for targeting enemies.
    /// </summary>
    [Description("The maximum distance at which the gun can target enemies.")]
    public float MaxDistance { get; set; } = 100f;

    /// <summary>
    /// Gets or sets a value indicating whether ammo is consumed per hit or per shot.
    /// </summary>
    [Description("Whether ammo is consumed per hit (true) or per shot (false).")]
    public bool PerHitAmmo { get; set; } = true;

    /// <inheritdoc/>
    protected override void OnShooting(ShootingEventArgs ev)
    {
        if (ev.Player?.CurrentItem is not Firearm firearm || ev.Player == null)
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
                    target.ShowHint("<color=#FF0000>YOU HAVE BEEN KILLED BY AUTO AIM GUN</color>", 3f);
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