// -----------------------------------------------------------------------
// <copyright file="ServerHandler.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.Events;

using E = CustomItemsReborn.API.HintsAPI;
using global::CustomItemsReborn.API;
using static CustomItems;
using Exiled.API.Features;

/// <summary>
/// Event Handlers.
/// </summary>
public class ServerHandler
{
    /// <summary>
    /// OnReloadingConfigs handler.
    /// </summary>
    public void OnReloadingConfigs() => Instance.Config.LoadItems();

    /// <summary>
    /// WaitingPlayers handler.
    /// </summary>
    public void WaitingPlayers() => E.ActiveHints.Clear();
    
    
}