// -----------------------------------------------------------------------
// <copyright file="GrenadeLauncher.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.Items;

using System.Collections.Generic;
using System.Linq;
using CustomItemsReborn.API;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;
using ProjectileType = Exiled.API.Enums.ProjectileType;
using CollisionHandler = Exiled.API.Features.Components.CollisionHandler;
using Player = Exiled.API.Features.Player;
using CustomItemsReborn.API.Interfaces;
// ◊® ›“Œ ¿¬’«¿¬’€«¿€«¬’¿Ÿ’«¬€«¿’¬€«¿¬€¿«’¬€«¿’¬€«’¿«’¬€¿¬€¿’¬€¿«¬€¿’«¬€«
/*using global::CustomItemsReborn.API.Interfaces;
using global::CustomItemsReborn.API;*/

/// <summary>
/// Represents a custom grenade launcher (GL-119) that shoots grenades instead of bullets and reloads with grenade items.
/// </summary>
public class GrenadeLauncher : CustomItemsAPI
{
    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public override string ItemName => "GL-119";

    /// <summary>
    /// Gets the base item type used for the grenade launcher.
    /// </summary>
    public override ItemType ItemType => ItemType.GunCOM15;

    /// <summary>
    /// Gets the broadcast message shown when the item is picked up.
    /// </summary>
    public override string PickupBroadcast => "<b>You picked up the Grenade Launcher</b>";

    /// <summary>
    /// Gets the hint shown when the item is selected.
    /// </summary>
    public override string ChangeHint => "Shoots grenades instead of bullets. Reload with grenades.";

    /// <summary>
    /// Gets the HashSet of serial numbers for tracking instances of this item.
    /// </summary>
    public override HashSet<ushort> ItemList => _serials;

    /// <summary>
    /// Stores the serial numbers of all grenade launcher instances.
    /// </summary>
    private readonly HashSet<ushort> _serials = new();

    /// <summary>
    /// Indicates whether the launcher requires grenades to reload.
    /// </summary>
    private readonly bool _useGrenades = true;

    /// <summary>
    /// Indicates whether modded grenades should be ignored during reloading.
    /// </summary>
    private readonly bool _ignoreModded = false;

    /// <summary>
    /// Stores the type of projectile currently loaded in the launcher.
    /// </summary>
    private ProjectileType _loadedType = ProjectileType.FragGrenade;

    /// <summary>
    /// Reference to the spawn API for creating pickups.
    /// </summary>
    private SpawnAPI _spawnApi;

    /// <summary>
    /// Initializes and spawns the grenade launcher pickup in the game world.
    /// </summary>
    public override void CreateCustomItem()
    {
        _spawnApi.CreateAndSpawnPickup(ItemType, RoomType.Hcz049, Vector3.zero, Quaternion.identity, _serials);
        base.CreateCustomItem();
    }

    /// <summary>
    /// Subscribes to player shooting and reloading events.
    /// </summary>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Shooting += OnShooting;
        Exiled.Events.Handlers.Player.ReloadingWeapon += OnReloading;
        base.SubscribeEvents();
    }

    /// <summary>
    /// Unsubscribes from player shooting and reloading events.
    /// </summary>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Shooting -= OnShooting;
        Exiled.Events.Handlers.Player.ReloadingWeapon -= OnReloading;
        base.UnsubscribeEvents();
    }

    /// <summary>
    /// Handles the reloading event to load grenades into the launcher.
    /// </summary>
    /// <param name="ev">The reloading event arguments.</param>
    private void OnReloading(ReloadingWeaponEventArgs ev)
    {
        if (!_useGrenades)
            return;

        ev.IsAllowed = false;

        if (ev.Player.CurrentItem is not Firearm firearm || firearm.MagazineAmmo >= 1)
            return;

        foreach (Item it in ev.Player.Items.ToHashSet())
        {
            if (it.Type is not (ItemType.GrenadeHE or ItemType.GrenadeFlash or ItemType.SCP018))
                continue;

            if (_ignoreModded && IsSelectedCustomItem(it.Serial, _serials))
                continue;

            _loadedType = it.Type switch
            {
                ItemType.GrenadeFlash => ProjectileType.Flashbang,
                ItemType.SCP018 => ProjectileType.Scp018,
                _ => ProjectileType.FragGrenade
            };

            ev.Player.DisableEffect(EffectType.Invisible);
            ev.Firearm.Reload();
            Timing.CallDelayed(3f, () => firearm.MagazineAmmo = 1);
            ev.Player.RemoveItem(it);
            return;
        }
    }

    /// <summary>
    /// Handles the shooting event to fire a grenade projectile.
    /// </summary>
    /// <param name="ev">The shooting event arguments.</param>
    private void OnShooting(ShootingEventArgs ev)
    {
        if (ev.Player.CurrentItem == null || !IsSelectedCustomItem(ev.Player.CurrentItem.Serial, _serials))
            return;

        ev.IsAllowed = false;

        if (ev.Player.CurrentItem is Firearm firearm)
            firearm.MagazineAmmo = 0;

        var thrown = ev.Player.ThrowGrenade(_loadedType, false);
        thrown.Projectile.GameObject
              .AddComponent<CollisionHandler>()
              .Init(ev.Player.GameObject, thrown.Projectile.Base);
    }
}