// -----------------------------------------------------------------------
// <copyright file="ServerHandler.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItems.Events;

using E = lelele.API.HintsAPI;
using global::CustomItems.API;
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