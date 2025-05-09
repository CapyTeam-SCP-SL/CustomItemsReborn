// -----------------------------------------------------------------------
// <copyright file="Coroutine.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.API.Other
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using CustomItemsReborn.API;
    using MEC;
    using RueI.Displays;
    using UnityEngine;
    using Display = RueI.Displays.Display;

    /// <summary>
    /// Manages a coroutine for updating player hints using RueI and HintsAPI.
    /// </summary>
    public class Coroutine
    {
        /// <summary>
        /// Stores the coroutine handle for managing hint updates.
        /// </summary>
        public static CoroutineHandle HintCoroutine;

        /// <summary>
        /// Runs an infinite coroutine to update player hints every 0.5 seconds.
        /// </summary>
        /// <returns>An iterator yielding a 0.5-second delay between iterations.</returns>
        /// <remarks>
        /// The coroutine iterates through all connected players, updates their displays in <see cref="HintsAPI.DisplayHashSet"/>,
        /// synchronizes <see cref="Capybara"/> elements from <see cref="HintsAPI.CachedPlayerElements"/>,
        /// and shows hints. For dead players in spectator mode, it displays the spectated player's hint.
        /// Errors are logged to prevent crashes.
        /// </remarks>
        public static IEnumerator<float> HintCoroutinelele()
        {
            while (true)
            {
                try
                {
                    // Loop through all players in the HashSet
                    foreach (Player? pl in Player.List)
                    {
                        // Skip if player is null or not connected
                        if (pl == null || !pl.IsConnected) continue;

                        // Add player's display to DisplayHashSet if not present
                        if (!HintsAPI.DisplayList.Any(x => x.ReferenceHub == pl.ReferenceHub))
                            HintsAPI.DisplayList.Add(new(pl.ReferenceHub));

                        // Find the player's display
                        Display playerDisplay = HintsAPI.DisplayList.Find(x => x.ReferenceHub == pl.ReferenceHub);

                        // Skip if display is not found
                        if (playerDisplay == null) continue;

                        // Initialize CachedPlayerElements for the player if absent
                        if (!HintsAPI.CachedPlayerElements.ContainsKey(pl))
                            HintsAPI.CachedPlayerElements.Add(pl, HintsAPI.CachedElements);

                        // Synchronize Capybara elements for the player
                        if (HintsAPI.CachedPlayerElements.TryGetValue(pl, out List<Capybara>? savedElements))
                        {
                            // Clear current display elements
                            playerDisplay.Elements.Clear();

                            // Add saved Capybara elements to the display
                            foreach (Capybara element in savedElements)
                            {
                                playerDisplay.Elements.Add(element);
                            }
                        }

                        // Handle dead players in spectator mode
                        if (pl.IsDead
                            && pl.Role is SpectatorRole spectator
                            && spectator.SpectatedPlayer != null
                            && spectator.SpectatedPlayer.CurrentHint != null)
                        {
                            // Show the spectated player's hint
                            pl.ShowHint(spectator.SpectatedPlayer.CurrentHint.Content, 2f);
                        }
                        else
                        {
                            // Combine and show hints from the player's display elements
                            pl.ShowHint(ElemCombiner.Combine(playerDisplay.Coordinator, playerDisplay.Elements), 2f);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Log any errors to prevent coroutine crashes
                    Log.Error(e);
                }

                // Wait 0.5 seconds before the next iteration
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}