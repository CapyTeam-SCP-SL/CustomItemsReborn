// -----------------------------------------------------------------------
// <copyright file="Rock.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItems.Components;

using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using InventorySystem.Items.ThrowableProjectiles;
using UnityEngine;

/// <summary>
/// A custom component for handling rock projectile collisions.
/// </summary>
public class Rock : Scp018Projectile
{
    /// <summary>
    /// Gets the owner of the rock projectile.
    /// </summary>
    public GameObject? Owner { get; private set; }

    /// <summary>
    /// Gets the side of the rock for friendly fire checks.
    /// </summary>
    public Side Side { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the rock can damage allies.
    /// </summary>
    public bool FriendlyFire { get; private set; }

    /// <summary>
    /// Gets the damage dealt by the rock when thrown.
    /// </summary>
    public float ThrownDamage { get; private set; }

    /// <summary>
    /// Initializes the rock projectile.
    /// </summary>
    /// <param name="owner">The owner of the rock.</param>
    /// <param name="side">The side of the rock for friendly fire checks.</param>
    /// <param name="friendlyFire">Whether the rock can damage allies.</param>
    /// <param name="thrownDamage">The damage dealt by the rock.</param>
    public void Init(GameObject owner, Side side, bool friendlyFire, float thrownDamage)
    {
        Owner = owner;
        Side = side;
        FriendlyFire = friendlyFire;
        ThrownDamage = thrownDamage;
    }

    /// <inheritdoc/>
    public override void ProcessCollision(Collision collision)
    {
        try
        {
            if (collision.gameObject == Owner)
                return;

            Player? target = Player.Get(collision.collider.GetComponentInParent<ReferenceHub>());
            if (target != null && (FriendlyFire || target.Role.Side != Side))
            {
                target.Hurt(ThrownDamage, "Smashed with a heavy rock.");
                Player.Get(Owner)?.ShowHitMarker(1f);
            }

            CustomItem? rockItem = CustomItem.Registered.FirstOrDefault(item => item.Name == "Rock");
            if (rockItem != null)
            {
                rockItem.Spawn(collision.GetContact(0).point + Vector3.up, Player.Get(Owner));
            }

            Destroy(gameObject);
        }
        catch (Exception ex)
        {
            Log.Error($"Error in Rock.ProcessCollision: {ex.Message}");
        }
    }
}