
// -----------------------------------------------------------------------
// <copyright file="MediGun.cs" company="CapyTeam SCP: SL">
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
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Firearm = Exiled.API.Features.Items.Firearm;

/// <summary>
/// Represents the MediGun (MG-119), a firearm that shoots healing darts to heal allies or cure SCP-049-2 zombies.
/// </summary>
public class MediGun : CustomItemsAPI
{
    // Constants for default roles and healing parameters
    private const RoleTypeId DefaultCuredRole = RoleTypeId.ClassD;
    private const RoleTypeId MtfCuredRole = RoleTypeId.NtfPrivate;
    private const RoleTypeId ChaosCuredRole = RoleTypeId.ChaosConscript;
    private const float MinHealingModifier = 0.1f;
    private const int MinZombieHealingRequired = 50;

    /// <summary>
    /// Tracks previous roles of cured zombies for role restoration.
    /// </summary>
    private readonly Dictionary<Player, RoleTypeId> _prevRoles = new();

    /// <summary>
    /// Stores serial numbers of all MediGun instances for efficient lookup.
    /// </summary>
    private readonly HashSet<ushort> _itemList = [];

    /// <summary>
    /// Reference to the spawn API for creating pickups.
    /// </summary>
    private readonly SpawnAPI _spawnApi;

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public override string ItemName => "MG-119";

    /// <summary>
    /// Gets the base item type used for the MediGun.
    /// </summary>
    public override ItemType ItemType => ItemType.GunFSP9;

    /// <summary>
    /// Gets the broadcast message shown when the item is picked up.
    /// </summary>
    public override string PickupBroadcast => "<b>You picked up MG-119</b>";

    /// <summary>
    /// Gets the hint shown when the item is selected.
    /// </summary>
    public override string ChangeHint => "Shoots healing darts. Allies heal, 049-2 cures.";

    /// <summary>
    /// Gets the List of serial numbers for tracking instances of this item.
    /// </summary>
    public override HashSet<ushort> ItemList => [.. _itemList];

    /// <summary>
    /// Gets or sets whether zombies can be cured by the MediGun.
    /// </summary>
    [Description("Should zombies be curable?")]
    public bool HealZombies { get; set; } = true;

    /// <summary>
    /// Gets or sets whether cured zombies are assigned to the healer's team.
    /// </summary>
    [Description("Convert cured zombie to healer team.")]
    public bool HealZombiesTeamCheck { get; set; } = true;

    /// <summary>
    /// Gets or sets the percentage of damage converted to healing.
    /// </summary>
    [Description("Percent of damage converted to healing.")]
    public float HealingModifier
    {
        get => _healingModifier;
        set => _healingModifier = Math.Max(value, MinHealingModifier);
    }
    private float _healingModifier = 1f;

    /// <summary>
    /// Gets or sets the artificial health required to cure a zombie.
    /// </summary>
    [Description("AHP needed to cure a zombie.")]
    public int ZombieHealingRequired
    {
        get => _zombieHealingRequired;
        set => _zombieHealingRequired = Math.Max(value, MinZombieHealingRequired);
    }
    private int _zombieHealingRequired = 200;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediGun"/> class.
    /// </summary>
    public MediGun()
    {
        _spawnApi = new SpawnAPI();
    }

    /// <summary>
    /// Spawns MediGun pickups in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        try
        {
            if (_spawnApi == null)
            {
                Log.Error("SpawnAPI is null in MediGun.CreateCustomItem.");
                return;
            }

            _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.HczArmory, Vector3.zero, Quaternion.identity, _itemList);
            _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.EzGateA, Vector3.zero, Quaternion.identity, _itemList);
            _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.EzGateB, Vector3.zero, Quaternion.identity, _itemList);
            base.CreateCustomItem();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to create MediGun item: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Subscribes to hurting and dying events for healing and curing logic.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        if (HealZombies)
            Exiled.Events.Handlers.Player.Dying += OnDying;
        base.SubscribeEvents();
        Log.Debug("Subscribed to events for MediGun.");
    }

    /// <summary>
    /// Unsubscribes from hurting and dying events.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        if (HealZombies)
            Exiled.Events.Handlers.Player.Dying -= OnDying;
        base.UnsubscribeEvents();
        Log.Debug("Unsubscribed from events for MediGun.");
    }

    /// <summary>
    /// Handles the hurting event to heal allies or cure zombies.
    /// </summary>
    /// <param name="ev">The hurting event arguments.</param>
    private void OnHurting(HurtingEventArgs ev)
    {
        if (!IsValidHurtingEvent(ev))
            return;

        try
        {
            if (ev.Player.Role.Side == ev.Attacker.Role.Side)
            {
                HealAlly(ev.Player, ev.Amount);
                ev.IsAllowed = false;
                return;
            }

            if (HealZombies && ev.Player.Role == RoleTypeId.Scp0492)
            {
                HealZombie(ev.Player, ev.Amount, ev.Attacker);
                ev.IsAllowed = false;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnHurting for MediGun: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Tracks the previous role of a dying zombie for curing purposes.
    /// </summary>
    /// <param name="ev">The dying event arguments.</param>
    private void OnDying(DyingEventArgs ev)
    {
        if (ev?.Player == null || ev.Player.Role != RoleTypeId.Scp0492 || ev.Attacker?.Role == RoleTypeId.Scp049)
            return;

        _prevRoles[ev.Player] = ev.Player.Role;
        Log.Debug($"Tracked previous role for player {ev.Player.Nickname} as {ev.Player.Role}.");
    }

    /// <summary>
    /// Validates the hurting event for MediGun functionality.
    /// </summary>
    /// <param name="ev">The hurting event arguments.</param>
    /// <returns>True if the event is valid, otherwise false.</returns>
    private bool IsValidHurtingEvent(HurtingEventArgs ev)
    {
        return ev?.Attacker?.CurrentItem != null &&
               ev.Player != null &&
               ev.Player.IsAlive &&
               IsSelectedCustomItem(ev.Attacker.CurrentItem.Serial, _itemList);
    }

    /// <summary>
    /// Heals an allied player based on the damage amount and healing modifier.
    /// </summary>
    /// <param name="player">The player to heal.</param>
    /// <param name="amount">The base damage amount.</param>
    private void HealAlly(Player player, float amount)
    {
        float healAmount = amount * HealingModifier;
        player.Heal(healAmount);
        Log.Debug($"Healed player {player.Nickname} for {healAmount} HP.");
    }

    /// <summary>
    /// Applies healing to a zombie, potentially curing it if enough artificial health is accumulated.
    /// </summary>
    /// <param name="zombie">The zombie player to heal.</param>
    /// <param name="amount">The base damage amount.</param>
    /// <param name="healer">The player using the MediGun.</param>
    private void HealZombie(Player zombie, float amount, Player healer)
    {
        if (!zombie.ActiveArtificialHealthProcesses.Any())
            zombie.AddAhp(0f, ZombieHealingRequired, 0f);

        zombie.ArtificialHealth += amount;

        if (zombie.ArtificialHealth >= ZombieHealingRequired)
        {
            CureZombie(zombie, healer);
        }

        Log.Debug($"Applied {amount} AHP to zombie {zombie.Nickname}. Current AHP: {zombie.ArtificialHealth}.");
    }

    /// <summary>
    /// Cures a zombie, assigning a new role based on configuration.
    /// </summary>
    /// <param name="zombie">The zombie player to cure.</param>
    /// <param name="healer">The player using the MediGun.</param>
    private void CureZombie(Player zombie, Player healer)
    {
        try
        {
            RoleTypeId newRole = DetermineCuredRole(zombie, healer);
            zombie.Role.Set(newRole);
            _prevRoles.Remove(zombie); // Clean up to prevent memory leaks
            Log.Debug($"Cured zombie {zombie.Nickname} to role {newRole}.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error curing zombie {zombie.Nickname}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Determines the role to assign to a cured zombie based on configuration.
    /// </summary>
    /// <param name="zombie">The zombie player.</param>
    /// <param name="healer">The player using the MediGun.</param>
    /// <returns>The role to assign to the cured zombie.</returns>
    private RoleTypeId DetermineCuredRole(Player zombie, Player healer)
    {
        if (HealZombiesTeamCheck)
        {
            return healer.Role.Side == Side.Mtf ? MtfCuredRole : ChaosCuredRole;
        }

        return _prevRoles.TryGetValue(zombie, out RoleTypeId oldRole) ? oldRole : DefaultCuredRole;
    }
}