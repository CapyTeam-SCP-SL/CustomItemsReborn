// -----------------------------------------------------------------------
// <copyright file="Items.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1200

using CustomItemsReborn.Items;
using Exiled.API.Enums;
using PlayerRoles;
using UnityEngine;

namespace CustomItemsReborn.Configs;

using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
    /// Contains configuration classes for individual custom items.
    /// </summary>
    public class Configs
    {
        /// <summary>
        /// Configuration settings for all custom items.
        /// </summary>
        public class Items
        {
            public SniperRifleConfig SniperRifle { get; set; } = new();
            public TranquilizerGunConfig TranquilizerGun { get; set; } = new();
            public DeflectorShieldConfig DeflectorShield { get; set; } = new();
            public Scp2818Config Scp2818 { get; set; } = new();
            public AntiMemeticPillsConfig AntiMemeticPills { get; set; } = new();
            public EmpGrenadeConfig EmpGrenade { get; set; } = new();
            public GrenadeLauncherConfig GrenadeLauncher { get; set; } = new();
            public ImplosionGrenadeConfig ImplosionGrenade { get; set; } = new();
            public LethalInjectionConfig LethalInjection { get; set; } = new();
            public LuckyCoinConfig LuckyCoin { get; set; } = new();
            public MediGunConfig MediGun { get; set; } = new();
            public Scp714Config Scp714 { get; set; } = new();
            public Scp1499Config Scp1499 { get; set; } = new();
        }

        public class SniperRifleConfig
        {
            [Description("The damage multiplier for the sniper rifle shots.")]
            public float DamageMultiplier { get; set; } = 7.5f;

            [Description("The clip size for the sniper rifle.")]
            public byte ClipSize { get; set; } = 1;

            [Description("The broadcast message shown when the sniper rifle is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up the SR-119 Sniper Rifle</b>";

            [Description("The hint shown when the sniper rifle is selected.")]
            public string ChangeHint { get; set; } = "Fires high-velocity anti-personnel sniper rounds.";
        }

        public class TranquilizerGunConfig
        {
            [Description("Whether SCPs have a chance to resist tranquilization.")]
            public bool ResistantScps { get; set; } = true;

            [Description("The duration of a successful tranquilization in seconds.")]
            public float Duration { get; set; } = 5f;

            [Description("The exponential modifier for resistance to repeated tranquilizations.")]
            public float ResistanceModifier { get; set; } = 1.2f;

            [Description("The interval in seconds for reducing player resistance to tranquilizations.")]
            public float ResistanceFalloffDelay { get; set; } = 120f;

            [Description("Whether tranquilized players drop all their items.")]
            public bool DropItems { get; set; } = true;

            [Description("The percent chance an SCP resists tranquilization (0-100).")]
            public int ScpResistChance { get; set; } = 40;

            [Description("The clip size for the tranquilizer gun.")]
            public byte ClipSize { get; set; } = 2;

            [Description("The broadcast message shown when the tranquilizer gun is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You have picked up TG-119</b>";

            [Description("The hint shown when the tranquilizer gun is selected.")]
            public string ChangeHint { get; set; } = "Fires non-lethal tranquilizing darts. Unreliable against SCPs.";
        }

        public class DeflectorShieldConfig
        {
            [Description("The duration in seconds the deflector shield remains active (0 for no limit).")]
            public float Duration { get; set; } = 15f;

            [Description("The damage multiplier for bullets reflected by the shield.")]
            public float Multiplier { get; set; } = 1f;

            [Description("The broadcast message shown when the deflector shield is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up the Deflector Shield</b>";

            [Description("The hint shown when the deflector shield is selected.")]
            public string ChangeHint { get; set; } = "Deflects incoming bullets for a short duration.";
        }

        public class Scp2818Config
        {
            [Description("The delay between movement ticks in seconds for SCP-2818's projectile effect.")]
            public float TickFrequency { get; set; } = 0.00025f;

            [Description("The maximum distance moved per tick for SCP-2818's projectile effect.")]
            public float MaxDistancePerTick { get; set; } = 0.5f;

            [Description("Whether the gun should despawn after use.")]
            public bool DespawnAfterUse { get; set; } = false;

            [Description("The damage dealt to the user upon impact.")]
            public float Damage { get; set; } = float.MaxValue;

            [Description("The broadcast message shown when SCP-2818 is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up SCP-2818</b>";

            [Description("The hint shown when SCP-2818 is selected.")]
            public string ChangeHint { get; set; } = "Shoots the user as a projectile.";
        }

        public class AntiMemeticPillsConfig
        {
            [Description("The duration in seconds for the anti-memetic effect.")]
            public float AmnesiaVisionDuration { get; set; } = 10f;

            [Description("The broadcast message shown when the anti-memetic pills are picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up AM-119</b>";

            [Description("The hint shown when the anti-memetic pills are selected.")]
            public string ChangeHint { get; set; } = "Temporarily protects against Amnesia effects.";
        }

        public class EmpGrenadeConfig
        {
            [Description("Whether the EMP grenade can open locked doors.")]
            public bool OpenLockedDoors { get; set; } = true;

            [Description("Whether the EMP grenade can open keycard-locked doors.")]
            public bool OpenKeycardDoors { get; set; } = true;

            [Description("List of door types that the EMP grenade cannot open.")]
            public List<DoorType> BlackListedDoorTypes { get; set; } = new();

            [Description("Whether the EMP grenade disables Tesla gates.")]
            public bool DisableTeslaGates { get; set; } = true;

            [Description("The duration in seconds for the EMP effect.")]
            public float Duration { get; set; } = 20f;

            [Description("The broadcast message shown when the EMP grenade is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up EM-119</b>";

            [Description("The hint shown when the EMP grenade is selected.")]
            public string ChangeHint { get; set; } = "Disables electronics and opens doors in the blast radius.";
        }

        public class GrenadeLauncherConfig
        {
            [Description("Whether the grenade launcher consumes grenades from the inventory.")]
            public bool UseGrenades { get; set; } = true;

            [Description("Whether to ignore modded grenades.")]
            public bool IgnoreModded { get; set; } = false;

            [Description("The broadcast message shown when the grenade launcher is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up GL-119</b>";

            [Description("The hint shown when the grenade launcher is selected.")]
            public string ChangeHint { get; set; } = "Launches explosive grenades.";
        }

        public class ImplosionGrenadeConfig
        {
            [Description("The damage modifier for the implosion grenade.")]
            public float DamageModifier { get; set; } = 0.05f;

            [Description("The number of suction ticks for the implosion effect.")]
            public int SuctionCount { get; set; } = 90;

            [Description("The distance per tick for the implosion suction effect.")]
            public float SuctionPerTick { get; set; } = 0.125f;

            [Description("The tick rate in seconds for the implosion suction effect.")]
            public float SuctionTickRate { get; set; } = 0.025f;

            [Description("List of roles immune to the implosion effect.")]
            public List<RoleTypeId> BlackListedRoles { get; set; } = new() { RoleTypeId.Scp173, RoleTypeId.Tutorial };

            [Description("The broadcast message shown when the implosion grenade is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up IG-119</b>";

            [Description("The hint shown when the implosion grenade is selected.")]
            public string ChangeHint { get; set; } = "Pulls players toward the explosion center.";
        }

        public class LethalInjectionConfig
        {
            [Description("Whether the user dies if the injection fails.")]
            public bool KillOnFail { get; set; } = true;

            [Description("The delay in seconds before the injection takes effect.")]
            public float InjectionDelay { get; set; } = 1.5f;

            [Description("The AHP penalty applied when the injection fails.")]
            public float AhpPenalty { get; set; } = 30f;

            [Description("The broadcast message shown when the lethal injection is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up LJ-119</b>";

            [Description("The hint shown when the lethal injection is selected.")]
            public string ChangeHint { get; set; } = "Kills a target or the user if misused.";
        }

        public class LuckyCoinConfig
        {
            [Description("The duration in seconds for the luck effect.")]
            public float Duration { get; set; } = 10f;

            [Description("The cooldown duration in seconds before the coin can be flipped again.")]
            public float CooldownDuration { get; set; } = 120f;

            [Description("The broadcast message shown when the lucky coin is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up LC-119</b>";

            [Description("The hint shown when the lucky coin is selected.")]
            public string ChangeHint { get; set; } = "Grants luck-based effects when flipped.";
        }

        public class MediGunConfig
        {
            [Description("Whether the MediGun can heal zombies.")]
            public bool HealZombies { get; set; } = true;

            [Description("Whether to check team affiliation when healing zombies.")]
            public bool HealZombiesTeamCheck { get; set; } = true;

            [Description("The healing multiplier for the MediGun.")]
            public float HealingModifier { get; set; } = 1f;

            [Description("The amount of damage required to fully heal a zombie.")]
            public int ZombieHealingRequired { get; set; } = 200;

            [Description("The broadcast message shown when the MediGun is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up MG-119</b>";

            [Description("The hint shown when the MediGun is selected.")]
            public string ChangeHint { get; set; } = "Heals players or zombies with a healing beam.";
        }

        public class Scp714Config
        {
            [Description("List of roles that cannot deal damage to the player wearing SCP-714.")]
            public List<RoleTypeId> Scp714Roles { get; set; } = new() { RoleTypeId.Scp049, RoleTypeId.Scp0492 };

            [Description("List of effects applied when SCP-714 is equipped.")]
            public List<EffectType> Scp714Effects { get; set; } = new() { EffectType.Asphyxiated };

            [Description("List of effects prevented by SCP-714.")]
            public List<EffectType> PreventedEffects { get; set; } = new()
            {
                EffectType.AmnesiaItems,
                EffectType.AmnesiaVision,
                EffectType.Hypothermia,
                EffectType.Burned,
                EffectType.Concussed,
                EffectType.Blinded
            };

            [Description("The message shown when taking off SCP-714.")]
            public string TakeOffMessage { get; set; } = "You've taken off the ring.";

            [Description("The message shown when putting on SCP-714.")]
            public string PutOnMessage { get; set; } = "You have put on the ring.";

            [Description("The damage dealt to SCP-049 when attacking a player with SCP-714.")]
            public float Scp049Damage { get; set; } = 40f;

            [Description("The damage modifier for pocket dimension damage when wearing SCP-714.")]
            public float PocketDimensionModifier { get; set; } = 0.75f;

            [Description("The stamina limit modifier when SCP-714 is equipped.")]
            public float StamLimitModifier { get; set; } = 0.5f;

            [Description("The broadcast message shown when SCP-714 is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You have picked up SCP-714</b>";

            [Description("The hint shown when SCP-714 is selected.")]
            public string ChangeHint { get; set; } = "The jade ring that protects you from hazards.";
        }

        public class Scp1499Config
        {
            [Description("The maximum time in seconds the player can stay in the dimension (0 for no limit).")]
            public float Duration { get; set; } = 15f;

            [Description("The teleport position for the dimension (x, y, z).")]
            public Vector3 TeleportPosition { get; set; } = new(38.464f, 1014.112f, -32.689f);

            [Description("The broadcast message shown when SCP-1499 is picked up.")]
            public string PickupBroadcast { get; set; } = "<b>You picked up SCP-1499</b>";

            [Description("The hint shown when SCP-1499 is selected.")]
            public string ChangeHint { get; set; } = "Use to enter another dimension.";
        }
    }
