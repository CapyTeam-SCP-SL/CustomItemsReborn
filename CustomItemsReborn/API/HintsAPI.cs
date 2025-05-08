// -----------------------------------------------------------------------
// <copyright file="HintsAPI.cs" company="CapyTeam">
// Copyright (c) CapyTeam. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItems.API;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using MEC;
using UnityEngine;


/// <summary>
/// Provides utilities for managing player hints, nicknames, custom info, and progress bars in the game.
/// </summary>
public static class HintsAPI
{
    /// <summary>
    /// Placeholder for UI display elements (assumed to be a custom type).
    /// </summary>
    public static List<Display> DisplayList { get; } = new();

    /// <summary>
    /// Caches UI elements per player (assumed to be a custom type).
    /// </summary>
    public static Dictionary<Player, List<Capybara>> CachedPlayerElements { get; } = new();

    /// <summary>
    /// Caches global UI elements (assumed to be a custom type).
    /// </summary>
    public static List<Capybara> CachedElements { get; } = new();


    /// <summary>
    /// Tracks active hints and their durations for each player.
    /// </summary>
    public static Dictionary<Player, Dictionary<string, float>> ActiveHints { get; } = new();

    /// <summary>
    /// Displays a hint to a player with a specified duration.
    /// </summary>
    /// <param name="player">The player to show the hint to.</param>
    /// <param name="hint">The hint message.</param>
    /// <param name="time">The duration of the hint in seconds.</param>
    public static void Hint(this Player player, string hint, float time = 3f)
    {
        if (player == null || string.IsNullOrEmpty(hint))
        {
            Log.Warn("Hint: Player or hint is null/empty.");
            return;
        }

        try
        {
            if (!ActiveHints.ContainsKey(player))
                ActiveHints.Add(player, new Dictionary<string, float>());

            ActiveHints[player][hint] = time;
            player.ShowHint(hint, time);
        }
        catch (Exception ex)
        {
            Log.Error($"Error displaying hint for player {player.Nickname}: {ex.Message}");
        }
    }
}


/// <summary>
/// Placeholder class for item hint management (assumed external dependency).
/// </summary>
public static class HintToItemAPI
{
    public static Dictionary<ushort, string> ItemSerialToHint { get; } = new();
}


/// <summary>
/// Placeholder class for ragdoll management (assumed external dependency).
/// </summary>
public static class TralalelotralalaAPI
{
    public static Dictionary<Player, PlayerRoles.Ragdolls.RagdollData> PlayerTakingRagdoll { get; } = new();
}

/// <summary>
/// Extension method for random item selection from a list.
/// </summary>
public static class ListExtensions
{
    public static T RandomItem<T>(this List<T> list)
    {
        return list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count)] : default;
    }
}