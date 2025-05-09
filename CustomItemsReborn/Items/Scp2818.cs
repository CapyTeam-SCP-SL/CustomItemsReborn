// -----------------------------------------------------------------------
// <copyright file="Scp2818.cs" company="CapyTeam SCP: SL">
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
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

/// <summary>
/// Represents SCP-2818, a custom firearm that propels the user as a projectile toward the aimed direction.
/// </summary>
public class Scp2818 : CustomItemsAPI
{
    // Constants for projectile transformation
    private const float ProjectileStartOffsetX = 0.0715f;
    private const float ProjectileStartOffsetY = 0.0225f;
    private const float ProjectileStartOffsetZ = 0.45f;
    private const float ProjectileScaleFactor = 0.15f;
    private const float NormalScaleFactor = 1f;
    private const float ScaleResetDelay = 0.01f;
    private const float MaxTravelDistanceSqr = 1000f;
    private const float MinDistanceToTarget = 0.5f;

    /// <summary>
    /// Stores serial numbers of all SCP-2818 instances for efficient lookup.
    /// </summary>
    private readonly HashSet<ushort> _serials = [];

    /// <summary>
    /// Reference to the spawn API for creating pickups.
    /// </summary>
    private readonly SpawnAPI _spawn;

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public override string ItemName => "SCP-2818";

    /// <summary>
    /// Gets the base item type used for SCP-2818.
    /// </summary>
    public override ItemType ItemType => ItemType.GunE11SR;

    /// <summary>
    /// Gets the broadcast message shown when the item is picked up.
    /// </summary>
    public override string PickupBroadcast => "<b>You picked up SCP-2818</b>";

    /// <summary>
    /// Gets the hint shown when the item is selected.
    /// </summary>
    public override string ChangeHint => "Shoots the user as a projectile.";

    /// <summary>
    /// Gets the List of serial numbers for tracking instances of this item.
    /// </summary>
    public override HashSet<ushort> ItemList => [.. _serials];

    /// <summary>
    /// Gets or sets the delay between movement ticks in seconds.
    /// </summary>
    [Description("Tick delay in seconds.")]
    public float TickFrequency { get; set; } = 0.00025f;

    /// <summary>
    /// Gets or sets the maximum distance moved per tick.
    /// </summary>
    [Description("Max distance moved per tick.")]
    public float MaxDistancePerTick { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets whether the gun should despawn after use.
    /// </summary>
    [Description("Despawn gun after use.")]
    public bool DespawnAfterUse { get; set; }

    /// <summary>
    /// Gets or sets the damage dealt to the user upon impact.
    /// </summary>
    [Description("Damage on hit.")]
    public float Damage { get; set; } = float.MaxValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scp2818"/> class.
    /// </summary>
    public Scp2818()
    {
        _spawn = new SpawnAPI();
    }

    /// <summary>
    /// Spawns SCP-2818 pickups in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        try
        {
            if (_spawn == null)
            {
                Log.Error("SpawnAPI is null in Scp2818.CreateCustomItem.");
                return;
            }

            _spawn.CreateAndSpawnPickup(ItemType, RoomType.HczHid, Vector3.zero, Quaternion.identity, _serials);
            _spawn.CreateAndSpawnPickup(ItemType, RoomType.HczArmory, Vector3.zero, Quaternion.identity, _serials);
            base.CreateCustomItem();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to create SCP-2818 item: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Subscribes to the shooting event for SCP-2818 functionality.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Shooting += OnShoot;
        base.SubscribeEvents();
        Log.Debug("Subscribed to shooting event for SCP-2818.");
    }

    /// <summary>
    /// Unsubscribes from the shooting event to prevent memory leaks.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Shooting -= OnShoot;
        base.UnsubscribeEvents();
        Log.Debug("Unsubscribed from shooting event for SCP-2818.");
    }

    /// <summary>
    /// Handles the shooting event to initiate the projectile effect.
    /// </summary>
    /// <param name="ev">The shooting event arguments.</param>
    private void OnShoot(ShootingEventArgs ev)
    {
        if (ev?.Player?.CurrentItem == null || !IsValidShooter(ev.Player))
            return;

        try
        {
            RemoveScp2818Items(ev.Player);
            Vector3 direction = ev.Direction;

            if (!IsValidDirection(direction, ev.Player.Position))
            {
                KillPlayer(ev.Player, "Invalid direction or excessive distance.");
                ev.IsAllowed = false;
                return;
            }

            Timing.RunCoroutine(LaunchProjectile(ev.Player, direction));
            ev.IsAllowed = false;
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnShoot for SCP-2818: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Validates if the player is holding an SCP-2818 item.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is a valid shooter, otherwise false.</returns>
    private bool IsValidShooter(Player player)
    {
        return player.CurrentItem != null && IsSelectedCustomItem(player.CurrentItem.Serial, _serials);
    }

    /// <summary>
    /// Removes all SCP-2818 items from the player's inventory.
    /// </summary>
    /// <param name="player">The player whose inventory to modify.</param>
    private void RemoveScp2818Items(Player player)
    {
        foreach (var item in player.Items.Where(item => IsSelectedCustomItem(item.Serial, _serials)).ToList())
        {
            player.RemoveItem(item);
        }
    }

    /// <summary>
    /// Validates the shooting direction and distance.
    /// </summary>
    /// <param name="direction">The direction of the shot.</param>
    /// <param name="playerPos">The player's position.</param>
    /// <returns>True if the direction is valid, otherwise false.</returns>
    private bool IsValidDirection(Vector3 direction, Vector3 playerPos)
    {
        return direction != Vector3.zero && (playerPos - direction).sqrMagnitude <= MaxTravelDistanceSqr;
    }

    /// <summary>
    /// Applies lethal damage to the player with a specified reason.
    /// </summary>
    /// <param name="player">The player to kill.</param>
    /// <param name="reason">The reason for the damage.</param>
    private void KillPlayer(Player player, string reason)
    {
        player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Warhead));
        Log.Debug($"Player {player.Nickname} killed due to: {reason}");
    }

    /// <summary>
    /// Coroutine that handles the projectile movement of the player.
    /// </summary>
    /// <param name="player">The player being propelled.</param>
    /// <param name="targetPos">The target position to move toward.</param>
    /// <returns>An iterator for the coroutine.</returns>
    private IEnumerator<float> LaunchProjectile(Player player, Vector3 targetPos)
    {
        if (player == null || !player.IsAlive)
            yield break;

        RoleTypeId originalRole = player.Role;
        Transform camera = player.CameraTransform;

        // Transform player into projectile
        player.Position = camera.TransformPoint(new Vector3(ProjectileStartOffsetX, ProjectileStartOffsetY, ProjectileStartOffsetZ));
        player.Scale = new Vector3(ProjectileScaleFactor, ProjectileScaleFactor, ProjectileScaleFactor);

        // Move player toward target
        while (Vector3.Distance(player.Position, targetPos) > MinDistanceToTarget && player.IsAlive)
        {
            if (player.Role != originalRole)
            {
                Log.Debug($"Projectile aborted for {player.Nickname}: Role changed from {originalRole}.");
                break;
            }

            player.Position = Vector3.MoveTowards(player.Position, targetPos, MaxDistancePerTick);
            yield return Timing.WaitForSeconds(TickFrequency);
        }

        // Reset player scale
        if (player.IsAlive)
        {
            player.Scale = new Vector3(NormalScaleFactor, NormalScaleFactor, NormalScaleFactor);
            yield return Timing.WaitForSeconds(ScaleResetDelay);
        }

        // Despawn item if configured
        if (DespawnAfterUse && player.IsAlive)
        {
            RemoveScp2818Items(player);
        }

        // Apply damage if player is not a spectator
        if (player.IsAlive && player.Role != RoleTypeId.Spectator)
        {
            KillPlayer(player, "Projectile impact.");
        }
    }
}