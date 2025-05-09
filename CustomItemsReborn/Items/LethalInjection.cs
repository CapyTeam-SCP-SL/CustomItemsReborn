// -----------------------------------------------------------------------
// <copyright file="LethalInjection.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.Items;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using CustomItemsReborn.API;
using CustomItemsReborn.API.Interfaces;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp096;
using PlayerStatsSystem;
using UnityEngine;

/// <summary>
/// Represents the Lethal Injection (LJ-119), an item that forces SCP-096 out of its rage state, typically killing the user.
/// </summary>
public class LethalInjection : CustomItemsAPI
{
    // Constants for injection parameters
    private const float DefaultInjectionDelay = 1.5f;
    private const float MinInjectionDelay = 0.5f;
    private const float DefaultAhpPenalty = 30f;
    private const float MinAhpPenalty = 0f;

    /// <summary>
    /// Stores serial numbers of all Lethal Injection instances for efficient lookup.
    /// </summary>
    private readonly HashSet<ushort> _itemList = [];

    /// <summary>
    /// Reference to the spawn API for creating pickups.
    /// </summary>
    private readonly SpawnAPI _spawnApi;

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public override string ItemName => "LJ-119";

    /// <summary>
    /// Gets the base item type used for the Lethal Injection.
    /// </summary>
    public override ItemType ItemType => ItemType.Adrenaline;

    /// <summary>
    /// Gets the broadcast message shown when the item is picked up.
    /// </summary>
    public override string PickupBroadcast => "<b>You have picked up LJ-119</b>";

    /// <summary>
    /// Gets the hint shown when the item is selected.
    /// </summary>
    public override string ChangeHint => "Inject to force SCP-096 out of rage if targeted.\nYou will die after use.";

    /// <summary>
    /// Gets the List of serial numbers for tracking instances of this item.
    /// </summary>
    public override HashSet<ushort> ItemList => [.. _itemList];

    /// <summary>
    /// Gets or sets whether the injector always kills the user, even if no rage is stopped.
    /// </summary>
    [Description("Should the injector always kill the user even when no enrage is stopped.")]
    public bool KillOnFail { get; set; } = true;

    /// <summary>
    /// Gets or sets the delay in seconds before the injection takes effect.
    /// </summary>
    [Description("Delay in seconds before the injection takes effect.")]
    public float InjectionDelay
    {
        get => _injectionDelay;
        set => _injectionDelay = Math.Max(value, MinInjectionDelay);
    }
    private float _injectionDelay = DefaultInjectionDelay;

    /// <summary>
    /// Gets or sets the artificial health penalty applied if the injection fails to break rage.
    /// </summary>
    [Description("Artificial health penalty applied if the injection fails to break rage.")]
    public float AhpPenalty
    {
        get => _ahpPenalty;
        set => _ahpPenalty = Math.Max(value, MinAhpPenalty);
    }
    private float _ahpPenalty = DefaultAhpPenalty;

    /// <summary>
    /// Initializes a new instance of the <see cref="LethalInjection"/> class.
    /// </summary>
    public LethalInjection()
    {
        _spawnApi = new SpawnAPI();
    }

    /// <summary>
    /// Spawns a Lethal Injection pickup in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        try
        {
            if (_spawnApi == null)
            {
                Log.Error("SpawnAPI is null in LethalInjection.CreateCustomItem.");
                return;
            }

            _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.Hcz096, Vector3.zero, Quaternion.identity, _itemList);
            base.CreateCustomItem();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to create Lethal Injection item: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Subscribes to the item usage event for Lethal Injection functionality.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem += OnUsedItem;
        base.SubscribeEvents();
        Log.Debug("Subscribed to item usage event for Lethal Injection.");
    }

    /// <summary>
    /// Unsubscribes from the item usage event to prevent memory leaks.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem -= OnUsedItem;
        base.UnsubscribeEvents();
        Log.Debug("Unsubscribed from item usage event for Lethal Injection.");
    }

    /// <summary>
    /// Handles the item usage event to attempt to break SCP-096's rage and apply consequences.
    /// </summary>
    /// <param name="ev">The item usage event arguments.</param>
    private void OnUsedItem(UsedItemEventArgs ev)
    {
        if (!IsValidEvent(ev))
            return;

        try
        {
            ev.Player.RemoveItem(ev.Player.CurrentItem);
            Timing.CallDelayed(InjectionDelay, () => ProcessInjection(ev.Player));
        }
        catch (Exception ex)
        {
            Log.Error($"Error in OnUsedItem for Lethal Injection: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Validates the item usage event for Lethal Injection functionality.
    /// </summary>
    /// <param name="ev">The item usage event arguments.</param>
    /// <returns>True if the event is valid, otherwise false.</returns>
    private bool IsValidEvent(UsedItemEventArgs ev)
    {
        return ev?.Player?.CurrentItem != null &&
               ev.Player.IsAlive &&
               IsSelectedCustomItem(ev.Player.CurrentItem.Serial, _itemList);
    }

    /// <summary>
    /// Processes the injection, attempting to break SCP-096's rage and applying consequences.
    /// </summary>
    /// <param name="player">The player who used the injection.</param>
    private void ProcessInjection(Player player)
    {
        if (!player.IsAlive)
        {
            Log.Debug($"Injection aborted for {player.Nickname}: Player is not alive.");
            return;
        }

        bool brokeEnrage = TryBreakScp096Rage(player);

        if (brokeEnrage || KillOnFail)
        {
            KillPlayer(player);
        }
        else
        {
            ApplyFailurePenalty(player);
        }
    }

    /// <summary>
    /// Attempts to break SCP-096's rage if the player is a target and SCP-096 is in a valid state.
    /// </summary>
    /// <param name="player">The player using the injection.</param>
    /// <returns>True if rage was broken, otherwise false.</returns>
    private bool TryBreakScp096Rage(Player player)
    {
        foreach (Player scp in Player.Get(RoleTypeId.Scp096))
        {
            if (scp.Role is not Exiled.API.Features.Roles.Scp096Role scp096)
                continue;

            bool isTarget = scp096.HasTarget(player);
            bool canBreak = scp096.RageState is Scp096RageState.Enraged or Scp096RageState.Calming;

            if (!isTarget || !canBreak)
                continue;

            scp096.RageManager.ServerEndEnrage();
            Log.Debug($"Player {player.Nickname} broke SCP-096 rage for {scp.Nickname}.");
            return true;
        }

        Log.Debug($"Player {player.Nickname} failed to break any SCP-096 rage.");
        return false;
    }

    /// <summary>
    /// Kills the player with a poison damage effect.
    /// </summary>
    /// <param name="player">The player to kill.</param>
    private void KillPlayer(Player player)
    {
        player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Poisoned));
        Log.Debug($"Player {player.Nickname} killed by Lethal Injection.");
    }

    /// <summary>
    /// Applies a penalty to the player if the injection fails to break rage.
    /// </summary>
    /// <param name="player">The player to penalize.</param>
    private void ApplyFailurePenalty(Player player)
    {
        if (player.ArtificialHealth > AhpPenalty)
            player.ArtificialHealth -= AhpPenalty;
        else
            player.ArtificialHealth = 0;

        player.DisableEffect<Invigorated>();
        Log.Debug($"Applied penalty to {player.Nickname}: Reduced AHP by {AhpPenalty} and disabled Invigorated effect.");
    }
}