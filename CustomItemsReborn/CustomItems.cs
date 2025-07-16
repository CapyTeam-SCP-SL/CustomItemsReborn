// -----------------------------------------------------------------------
// <copyright file="CustomItems.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1200
namespace CustomItemsReborn;

using System;
using Events;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using global::CustomItemsReborn.API;
using global::CustomItemsReborn.API.Other;
using HarmonyLib;
using CustomItemsReborn.API;
using Server = Exiled.Events.Handlers.Server;

/// <summary>
/// Main plugin class for Custom Items Reborn, handling initialization and custom item management.
/// </summary>
public class CustomItems : Plugin<Config>
{
    private ServerHandler serverHandler = null!;

    /// <summary>
    /// Gets the singleton instance of the plugin.
    /// </summary>
    public static CustomItems Instance { get; private set; } = null!;

    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public override string Name { get; } = "Custom Items Reborn";

    /// <summary>
    /// Gets the author of the plugin.
    /// </summary>
    public override string Author { get; } = "CapyTeam (Original by jocker-119)";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public override Version Version { get; } = new Version(8, 0, 0);

    /// <summary>
    /// Gets the minimum required EXILED version.
    /// </summary>
    public override Version RequiredExiledVersion { get; } = new(9, 6, 2);

    /// <summary>
    /// Initializes the plugin, registering custom items, event handlers, and caching UI elements.
    /// </summary>
    /// <remarks>
    /// Sets up the plugin instance, loads item configurations, registers custom items, and initializes RueI.
    /// Caches all <see cref="Capybara"/> elements from the assembly and starts the hint coroutine if not running.
    /// Logs warnings for failed element initialization and debug info for successful caching.
    /// </remarks>
    public override void OnEnabled()
    {
        // Set the singleton instance
        Instance = this;
        serverHandler = new ServerHandler();

        // Load item configurations
        Config.LoadItems();

        Log.Debug("Registering items...");
        API.CustomItem.RegisterAll();

        // Subscribe to server events
        Server.ReloadedConfigs += serverHandler.OnReloadingConfigs;
        Server.WaitingForPlayers += serverHandler.WaitingPlayers;

        // Initialize RueI
        RueI.RueIMain.EnsureInit();

        // Clear cached UI elements
        HintsAPI.CachedElements.Clear();

        // Cache all Capybara elements from the assembly
        foreach (Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.BaseType != typeof(Capybara))
                continue;

            System.Reflection.ConstructorInfo? ctr = type.GetConstructor(Type.EmptyTypes);

            if (ctr == null)
            {
                Log.Warn($"Interface | CachedElements | Skipped {type.FullName}. | Constructor for element not found.");
                continue;
            }

            Capybara? element = ctr.Invoke(null) as Capybara;

            if (element == null)
            {
                Log.Warn($"Interface | CachedElements | Skipped {type.FullName}. | Element failed to initialize.");
                continue;
            }

            Log.Debug($"Interface | Added to cache [{element.Name}]");
            HintsAPI.CachedElements.Add(element);
        }

        Log.Warn($"Added [{HintsAPI.CachedElements.Count}] dynamic elements to cache.");

        // Start the hint coroutine if not already running
        if (Coroutine.HintCoroutine == null || !Coroutine.HintCoroutine.IsRunning)
            Coroutine.HintCoroutine = MEC.Timing.RunCoroutine(Coroutine.HintCoroutinelele());

        base.OnEnabled();
    }

    /// <summary>
    /// Cleans up the plugin, unregistering custom items and event handlers.
    /// </summary>
    /// <remarks>
    /// Unregisters all custom items and unsubscribes from server events before disabling the plugin.
    /// </remarks>
    public override void OnDisabled()
    {
        // Unregister custom items
        API.CustomItem.UnregisterAll();

        HintsAPI.CachedElements.Clear();

        MEC.Timing.KillCoroutines(Coroutine.HintCoroutine);

        // Unsubscribe from server events
        Server.ReloadedConfigs -= serverHandler.OnReloadingConfigs;
        Server.WaitingForPlayers -= serverHandler.WaitingPlayers;

        base.OnDisabled();
    }
}