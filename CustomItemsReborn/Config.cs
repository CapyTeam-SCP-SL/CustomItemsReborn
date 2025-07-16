// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Exiled.Loader;
    using PlayerRoles;
    using UnityEngine;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Configuration class for the CustomItemsReborn plugin.
    /// </summary>
    public class Config : IConfig
    {
        /// <summary>
        /// Gets the item configuration settings.
        /// </summary>
        [YamlIgnore]
        public CustomItemsReborn.Configs.Configs.Items ItemConfigs { get; private set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is enabled.
        /// </summary>
        [Description("Determines whether the CustomItemsReborn plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug mode is enabled.
        /// </summary>
        [Description("Enables or disables debug messages in the server console.")]
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets the mapping of subclasses to custom items with their spawn chances.
        /// </summary>
        [Description(
            "Defines which subclasses can spawn with specific custom items and their percentage chance. Requires the Advanced Subclassing plugin.")]
        public Dictionary<string, Dictionary<string, float>> SubclassList { get; set; } = new()
        {
            {
                "ExampleSubclass", new Dictionary<string, float>
                {
                    {"SR-119", 100f}, // Sniper Rifle
                    {"TG-119", 50f}, // Tranquilizer Gun
                    {"DeflectorShield", 30f}, // Deflector Shield
                    {"SCP-2818", 20f}, // SCP-2818
                    {"AM-119", 40f}, // Anti-Memetic Pills
                    {"EM-119", 60f}, // EMP Grenade
                    {"GL-119", 50f}, // Grenade Launcher
                    {"IG-119", 45f}, // Implosion Grenade
                    {"LJ-119", 30f}, // Lethal Injection
                    {"LC-119", 70f}, // Lucky Coin
                    {"MG-119", 55f}, // MediGun
                    {"SCP-714", 25f}, // SCP-714
                    {"SCP-1499", 35f} // SCP-1499
                }
            }
        };

        /// <summary>
        /// Gets or sets the folder path for storing item configuration files.
        /// </summary>
        [Description("The folder path where item configuration files are stored.")]
        public string ItemConfigFolder { get; set; } = Path.Combine(Paths.Configs, "CustomItems");

        /// <summary>
        /// Gets or sets the filename for the item configuration file.
        /// </summary>
        [Description("The filename for the item configuration file (e.g., 'global.yml').")]
        public string ItemConfigFile { get; set; } = "global.yml";

        /// <summary>
        /// Loads and initializes the item configurations from the specified file.
        /// </summary>
        public void LoadItems()
        {
            try
            {
                // Validate folder and file paths
                if (string.IsNullOrWhiteSpace(ItemConfigFolder))
                {
                    Log.Error("ItemConfigFolder is empty or invalid. Using default path.");
                    ItemConfigFolder = Path.Combine(Paths.Configs, "CustomItems");
                }

                if (string.IsNullOrWhiteSpace(ItemConfigFile) ||
                    !ItemConfigFile.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Error("ItemConfigFile is invalid or does not end with '.yml'. Using default filename.");
                    ItemConfigFile = "global.yml";
                }

                // Ensure the configuration directory exists
                if (!Directory.Exists(ItemConfigFolder))
                {
                    Log.Info($"Creating configuration directory: {ItemConfigFolder}");
                    Directory.CreateDirectory(ItemConfigFolder);
                }

                string filePath = Path.Combine(ItemConfigFolder, ItemConfigFile);
                Log.Info($"Loading item configurations from: {filePath}");

                // Initialize with default configurations if file doesn't exist
                if (!File.Exists(filePath))
                {
                    ItemConfigs = CreateDefaultItemConfigs();
                    File.WriteAllText(filePath, Loader.Serializer.Serialize(ItemConfigs));
                    Log.Info($"Created default item configuration file: {filePath}");
                }
                else
                {
                    // Load and update configurations
                    ItemConfigs = Loader.Deserializer.Deserialize<CustomItemsReborn.Configs.Configs.Items>(File.ReadAllText(filePath));
                    File.WriteAllText(filePath, Loader.Serializer.Serialize(ItemConfigs));
                    Log.Info($"Successfully loaded item configurations from: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load item configurations: {ex.Message}\n{ex.StackTrace}");
                ItemConfigs = CreateDefaultItemConfigs(); // Fallback to default configs on error
            }
        }

        /// <summary>
        /// Creates a default item configuration with sensible values for all custom items.
        /// </summary>
        /// <returns>A new instance of <see cref="Configs.Items"/> with default settings.</returns>
        private CustomItemsReborn.Configs.Configs.Items CreateDefaultItemConfigs()
        {
            return new CustomItemsReborn.Configs.Configs.Items
            {
                SniperRifle = new CustomItemsReborn.Configs.Configs.SniperRifleConfig
                {
                    DamageMultiplier = 7.5f,
                    ClipSize = 1,
                    PickupBroadcast = "<b>You picked up the SR-119 Sniper Rifle</b>",
                    ChangeHint = "Fires high-velocity anti-personnel sniper rounds."
                },
                TranquilizerGun = new CustomItemsReborn.Configs.Configs.TranquilizerGunConfig
                {
                    ResistantScps = true,
                    Duration = 5f,
                    ResistanceModifier = 1.2f,
                    ResistanceFalloffDelay = 120f,
                    DropItems = true,
                    ScpResistChance = 40,
                    ClipSize = 2,
                    PickupBroadcast = "<b>You have picked up TG-119</b>",
                    ChangeHint = "Fires non-lethal tranquilizing darts. Unreliable against SCPs."
                },
                DeflectorShield = new CustomItemsReborn.Configs.Configs.DeflectorShieldConfig
                {
                    Duration = 15f,
                    Multiplier = 1f,
                    PickupBroadcast = "<b>You picked up the Deflector Shield</b>",
                    ChangeHint = "Deflects incoming bullets for a short duration."
                },
                Scp2818 = new CustomItemsReborn.Configs.Configs.Scp2818Config
                {
                    TickFrequency = 0.00025f,
                    MaxDistancePerTick = 0.5f,
                    DespawnAfterUse = false,
                    Damage = float.MaxValue,
                    PickupBroadcast = "<b>You picked up SCP-2818</b>",
                    ChangeHint = "Shoots the user as a projectile."
                },
                AntiMemeticPills = new CustomItemsReborn.Configs.Configs.AntiMemeticPillsConfig
                {
                    AmnesiaVisionDuration = 10f,
                    PickupBroadcast = "<b>You picked up AM-119</b>",
                    ChangeHint = "Temporarily protects against Amnesia effects."
                },
                EmpGrenade = new CustomItemsReborn.Configs.Configs.EmpGrenadeConfig
                {
                    OpenLockedDoors = true,
                    OpenKeycardDoors = true,
                    BlackListedDoorTypes = new List<DoorType>(),
                    DisableTeslaGates = true,
                    Duration = 20f,
                    PickupBroadcast = "<b>You picked up EM-119</b>",
                    ChangeHint = "Disables electronics and opens doors in the blast radius."
                },
                GrenadeLauncher = new CustomItemsReborn.Configs.Configs.GrenadeLauncherConfig
                {
                    UseGrenades = true,
                    IgnoreModded = false,
                    PickupBroadcast = "<b>You picked up GL-119</b>",
                    ChangeHint = "Launches explosive grenades."
                },
                ImplosionGrenade = new CustomItemsReborn.Configs.Configs.ImplosionGrenadeConfig
                {
                    DamageModifier = 0.05f,
                    SuctionCount = 90,
                    SuctionPerTick = 0.125f,
                    SuctionTickRate = 0.025f,
                    BlackListedRoles = new List<RoleTypeId> {RoleTypeId.Scp173, RoleTypeId.Tutorial},
                    PickupBroadcast = "<b>You picked up IG-119</b>",
                    ChangeHint = "Pulls players toward the explosion center."
                },
                LethalInjection = new CustomItemsReborn.Configs.Configs.LethalInjectionConfig
                {
                    KillOnFail = true,
                    InjectionDelay = 1.5f,
                    AhpPenalty = 30f,
                    PickupBroadcast = "<b>You picked up LJ-119</b>",
                    ChangeHint = "Kills a target or the user if misused."
                },
                LuckyCoin = new CustomItemsReborn.Configs.Configs.LuckyCoinConfig
                {
                    Duration = 10f,
                    CooldownDuration = 120f,
                    PickupBroadcast = "<b>You picked up LC-119</b>",
                    ChangeHint = "Grants luck-based effects when flipped."
                },
                MediGun = new CustomItemsReborn.Configs.Configs.MediGunConfig
                {
                    HealZombies = true,
                    HealZombiesTeamCheck = true,
                    HealingModifier = 1f,
                    ZombieHealingRequired = 200,
                    PickupBroadcast = "<b>You picked up MG-119</b>",
                    ChangeHint = "Heals players or zombies with a healing beam."
                },
                Scp714 = new CustomItemsReborn.Configs.Configs.Scp714Config
                {
                    Scp714Roles = new List<RoleTypeId> {RoleTypeId.Scp049, RoleTypeId.Scp0492},
                    Scp714Effects = new List<EffectType> {EffectType.Asphyxiated},
                    PreventedEffects = new List<EffectType>
                    {
                        EffectType.AmnesiaItems,
                        EffectType.AmnesiaVision,
                        EffectType.Hypothermia,
                        EffectType.Burned,
                        EffectType.Concussed,
                        EffectType.Blinded
                    },
                    TakeOffMessage = "You've taken off the ring.",
                    PutOnMessage = "You have put on the ring.",
                    Scp049Damage = 40f,
                    PocketDimensionModifier = 0.75f,
                    StamLimitModifier = 0.5f,
                    PickupBroadcast = "<b>You have picked up SCP-714</b>",
                    ChangeHint = "The jade ring that protects you from hazards."
                },
                Scp1499 = new CustomItemsReborn.Configs.Configs.Scp1499Config
                {
                    Duration = 15f,
                    TeleportPosition = new Vector3(38.464f, 1014.112f, -32.689f),
                    PickupBroadcast = "<b>You picked up SCP-1499</b>",
                    ChangeHint = "Use to enter another dimension."
                }
            };
        }
    }
}