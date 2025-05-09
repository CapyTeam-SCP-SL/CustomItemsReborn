// -----------------------------------------------------------------------
// <copyright file="AntiMemeticPills.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.Items;

using System.Collections.Generic;
using System.Xml.Linq;
using CustomItemsReborn.API;
using CustomItemsReborn.API.Interfaces;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

public class AntiMemeticPills : CustomItemsAPI
{

    /// <inheritdoc/>
    public override string ItemName => "AM-119";
    /// <inheritdoc/>
    public override ItemType ItemType => ItemType.Painkillers;
    /// <inheritdoc/>
    public override string PickupBroadcast => "<b>You have Pickuped AM-119</b>";
    /// <inheritdoc/>
    public override string ChangeHint => "Drugs that make you forget things. If you use these while you are targeted by SCP-096, you will forget what his face looks like, and thus no longer be a target.";
    /// <inheritdoc/>
    public override HashSet<ushort> ItemList => AntiMemeticPillsList;
    /// <inheritdoc/>

    public HashSet<ushort> AntiMemeticPillsList = new HashSet<ushort>();
    private SpawnAPI spawnApi;

    public override void CreateCustomItem()
    {
        spawnApi.CreateAndSpawnPickup(ItemType, RoomType.Hcz127, new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Quaternion(0,0,0,0), ItemList);
        base.CreateCustomItem();
    }

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }


    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (ev.Player.CurrentItem == null || !IsSelectedCustomItem(ev.Player.CurrentItem.Serial, AntiMemeticPillsList))
            return;

        IEnumerable<Player> scp096S = Player.Get(RoleTypeId.Scp096);

        Timing.CallDelayed(1f, () =>
        {
            foreach (Player scp in scp096S)
            {
                if (scp.Role is Scp096Role scp096)
                {
                    if (scp096.HasTarget(ev.Player))
                        scp096.RemoveTarget(ev.Player);
                }
            }

            ev.Player.EnableEffect<AmnesiaVision>(10f, true);
        });
    }
}