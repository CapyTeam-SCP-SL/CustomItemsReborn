// -----------------------------------------------------------------------
// <copyright file="C4Charge.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItems.Items;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items.ThrowableProjectiles;
using UnityEngine;
using YamlDotNet.Serialization;
using PlayerEvent = Exiled.Events.Handlers.Player;

/// <summary>
/// Represents a custom C4 explosive charge that can be remotely detonated.
/// </summary>
[CustomItem(ItemType.GrenadeHE)]
public class C4Charge : CustomGrenade
{
    /// <summary>
    /// Enum defining methods for removing a C4 charge.
    /// </summary>
    public enum C4RemoveMethod
    {
        /// <summary>
        /// Removes the C4 charge without exploding.
        /// </summary>
        Remove = 0,

        /// <summary>
        /// Detonates the C4 charge.
        /// </summary>
        Detonate = 1,

        /// <summary>
        /// Drops the C4 charge as a pickable item.
        /// </summary>
        Drop = 2,
    }

    /// <summary>
    /// Gets the singleton instance of this item manager.
    /// </summary>
    public static C4Charge Instance { get; private set; } = null!;

    /// <summary>
    /// Gets a dictionary of currently placed C4 charges and their owners.
    /// </summary>
    public static Dictionary<Pickup, Player> PlacedCharges { get; } = new();

    /// <inheritdoc/>
    public override uint Id { get; set; } = 15;

    /// <inheritdoc/>
    public override string Name { get; set; } = "C4-119";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 0.75f;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 5,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new() { Chance = 10, Location = SpawnLocationType.InsideLczArmory },
            new() { Chance = 25, Location = SpawnLocationType.InsideHczArmory },
            new() { Chance = 50, Location = SpawnLocationType.Inside049Armory },
            new() { Chance = 100, Location = SpawnLocationType.InsideSurfaceNuke },
        },
    };

    /// <inheritdoc/>
    public override string Description { get; set; } = "An explosive charge that can be remotely detonated.";

    /// <summary>
    /// Gets or sets a value indicating whether the C4 charge sticks to walls or ceilings.
    /// </summary>
    [Description("Should C4 charge stick to walls or ceilings.")]
    public bool IsSticky { get; set; } = true;

    /// <summary>
    /// Gets or sets the multiplier for the C4 throwing force.
    /// </summary>
    [Description("Defines the strength of the C4 throw.")]
    public float ThrowMultiplier { get; set; } = 40f;

    /// <summary>
    /// Gets or sets a value indicating whether a specific item is required to detonate the C4.
    /// </summary>
    [Description("Should C4 require a specific item to be detonated.")]
    public bool RequireDetonator { get; set; } = true;

    /// <summary>
    /// Gets or sets the item type used as the detonator for C4 charges.
    /// </summary>
    [Description("The item type used to detonate C4 charges.")]
    public ItemType DetonatorItem { get; set; } = ItemType.Radio;

    /// <summary>
    /// Gets or sets the method to handle C4 charges when the owner dies or leaves the game.
    /// </summary>
    [Description("What happens to C4 charges when the player dies or leaves. (Remove / Detonate / Drop)")]
    public C4RemoveMethod MethodOnDeath { get; set; } = C4RemoveMethod.Drop;

    /// <summary>
    /// Gets or sets a value indicating whether C4 can be defused by shooting.
    /// </summary>
    [Description("Should shooting at C4 charges trigger an action.")]
    public bool AllowShoot { get; set; } = true;

    /// <summary>
    /// Gets or sets the method to handle C4 charges when shot.
    /// </summary>
    [Description("What happens to C4 charges when shot. (Remove / Detonate / Drop)")]
    public C4RemoveMethod ShotMethod { get; set; } = C4RemoveMethod.Remove;

    /// <summary>
    /// Gets or sets the maximum distance for detonating a C4 charge.
    /// </summary>
    [Description("Maximum distance between C4 charge and player to detonate.")]
    public float MaxDistance { get; set; } = 100f;

    /// <summary>
    /// Gets or sets the time after which the C4 charge will automatically detonate.
    /// </summary>
    [Description("Time after which the C4 charge will automatically detonate.")]
    public override float FuseTime { get; set; } = 9999f;

    /// <inheritdoc/>
    [YamlIgnore]
    public override bool ExplodeOnCollision { get; set; } = false;

    /// <inheritdoc/>
    [YamlIgnore]
    public override ItemType Type { get; set; } = ItemType.GrenadeHE;

    /// <summary>
    /// Handles the removal or detonation of a C4 charge.
    /// </summary>
    /// <param name="charge">The C4 charge to handle.</param>
    /// <param name="removeMethod">The method to use for removal.</param>
    public void C4Handler(Pickup? charge, C4RemoveMethod removeMethod = C4RemoveMethod.Detonate)
    {
        if (charge == null || charge.Position == null)
            return;

        try
        {
            switch (removeMethod)
            {
                case C4RemoveMethod.Remove:
                    break;

                case C4RemoveMethod.Detonate:
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(Type);
                    grenade.FuseTime = 0.1f;
                    grenade.SpawnActive(charge.Position);
                    break;

                case C4RemoveMethod.Drop:
                    TrySpawn(Id, charge.Position, out _);
                    break;
            }

            PlacedCharges.Remove(charge);
            charge.Destroy();
        }
        catch (Exception ex)
        {
            Log.Error($"Error in C4Handler: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Instance = this;

        PlayerEvent.Destroying += OnDestroying;
        PlayerEvent.Died += OnDied;
        PlayerEvent.Shooting += OnShooting;
        Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;

        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        PlayerEvent.Destroying -= OnDestroying;
        PlayerEvent.Died -= OnDied;
        PlayerEvent.Shooting -= OnShooting;
        Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;

        base.UnsubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void OnWaitingForPlayers()
    {
        PlacedCharges.Clear();
        base.OnWaitingForPlayers();
    }

    /// <inheritdoc/>
    protected override void OnThrownProjectile(ThrownProjectileEventArgs ev)
    {
        if (ev.Projectile == null || ev.Player == null)
            return;

        if (!PlacedCharges.ContainsKey(ev.Projectile))
            PlacedCharges.Add(ev.Projectile, ev.Player);

        base.OnThrownProjectile(ev);
    }

    /// <inheritdoc/>
    protected override void OnExploding(ExplodingGrenadeEventArgs ev)
    {
        if (ev.Projectile?.Base == null)
            return;

        Pickup pickup = Pickup.Get(ev.Projectile.Base);
        if (pickup != null)
            PlacedCharges.Remove(pickup);
    }

    private void OnDestroying(DestroyingEventArgs ev)
    {
        if (ev.Player == null)
            return;

        foreach (var charge in PlacedCharges.Where(c => c.Value == ev.Player).ToList())
        {
            C4Handler(charge.Key, C4RemoveMethod.Remove);
        }
    }

    private void OnDied(DiedEventArgs ev)
    {
        if (ev.Player == null)
            return;

        foreach (var charge in PlacedCharges.Where(c => c.Value == ev.Player).ToList())
        {
            C4Handler(charge.Key, MethodOnDeath);
        }
    }

    private void OnShooting(ShootingEventArgs ev)
    {
        if (!AllowShoot || ev.Player == null)
            return;

        Vector3 forward = ev.Player.CameraTransform.forward;
        if (!Physics.Raycast(ev.Player.CameraTransform.position + forward, forward, out var hit, 500))
            return;

        EffectGrenade? grenade = hit.collider.gameObject.GetComponentInParent<EffectGrenade>();
        if (grenade == null)
            return;

        Pickup pickup = Pickup.Get(grenade);
        if (pickup != null && PlacedCharges.ContainsKey(pickup))
        {
            C4Handler(pickup, ShotMethod);
        }
    }

    private void OnRoundEnded(RoundEndedEventArgs ev)
    {
        PlacedCharges.Clear();
    }
}