// -----------------------------------------------------------------------
// <copyright file="LethalInjection.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using CustomPlayerEffects;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp096;

namespace CustomItemsReborn.Items
{
    using System;
    using System.ComponentModel;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Player;
    using MEC;
    using PlayerStatsSystem;
    using UnityEngine;
    using CustomItemsReborn.API;

    /// <summary>
    /// Represents the Lethal Injection (LJ-119), an item that forces SCP-096 out of its rage state, typically killing the user.
    /// </summary>
    public class LethalInjection : CustomItem
    {
        private const float DefaultInjectionDelay = 1.5f;
        private const float MinInjectionDelay = 0.5f;
        private const float DefaultAhpPenalty = 30f;
        private const float MinAhpPenalty = 0f;

        /// <summary>
        /// Gets the globally unique identifier for this item type.
        /// </summary>
        public override string Id => "LJ-119";

        /// <summary>
        /// Gets the human-readable name of the item.
        /// </summary>
        public override string Name => "Lethal Injection";

        /// <summary>
        /// Gets the underlying item type used for spawning.
        /// </summary>
        public override ItemType BaseType => ItemType.Adrenaline;

        /// <summary>
        /// Gets the broadcast message shown when the item is picked up.
        /// </summary>
        public override string PickupBroadcast => "<b>You have picked up LJ-119</b>";

        /// <summary>
        /// Gets the hint shown when the item is selected.
        /// </summary>
        public override string ChangeHint => "Inject to force SCP-096 out of rage if targeted.\nYou will die after use.";

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
        /// Initializes the item by spawning it in the game world.
        /// </summary>
        public override void Initialize()
        {
            SpawnInRoom(RoomType.Hcz096, Vector3.zero, Vector3.zero);
        }

        /// <summary>
        /// Subscribes to the item usage event for Lethal Injection functionality.
        /// </summary>
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem += OnUsedItem;
        }

        /// <summary>
        /// Unsubscribes from the item usage event to prevent memory leaks.
        /// </summary>
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem -= OnUsedItem;
        }

        /// <summary>
        /// Handles the item usage event to attempt to break SCP-096's rage and apply consequences.
        /// </summary>
        private void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ev.Player.RemoveItem(ev.Player.CurrentItem);
            Timing.CallDelayed(InjectionDelay, () => ProcessInjection(ev.Player));
        }

        /// <summary>
        /// Processes the injection, attempting to break SCP-096's rage and applying consequences.
        /// </summary>
        private void ProcessInjection(Player player)
        {
            if (!player.IsAlive)
                return;

            bool brokeEnrage = TryBreakScp096Rage(player);

            if (brokeEnrage || KillOnFail)
            {
                player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Poisoned));
            }
            else
            {
                if (player.ArtificialHealth > AhpPenalty)
                    player.ArtificialHealth -= AhpPenalty;
                else
                    player.ArtificialHealth = 0;

                player.DisableEffect<Invigorated>();
            }
        }

        /// <summary>
        /// Attempts to break SCP-096's rage if the player is a target and SCP-096 is in a valid state.
        /// </summary>
        private bool TryBreakScp096Rage(Player player)
        {
            foreach (Player scp in Player.Get(RoleTypeId.Scp096))
            {
                if (scp.Role is not Scp096Role scp096)
                    continue;

                bool isTarget = scp096.HasTarget(player);
                bool canBreak = scp096.RageState is Scp096RageState.Enraged or Scp096RageState.Calming;

                if (!isTarget || !canBreak)
                    continue;

                scp096.RageManager.ServerEndEnrage();
                return true;
            }

            return false;
        }
    }
}