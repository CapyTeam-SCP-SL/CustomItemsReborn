// -----------------------------------------------------------------------
// <copyright file="Hint.cs" company="CapyTeam">
// Copyright (c) CapyTeam. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
namespace CustomItems.API.Capybaras
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using RueI.Displays;
    using RueI.Elements.Delegates;
    using RueI.Extensions.HintBuilding;
    using RueI.Parsing.Enums;

    /// <summary>
    /// Represents a dynamic UI element that displays centered text hints for a player using the RueI framework.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="Capybara"/> and is designed to render text-based hints in the center of a player's screen.
    /// It integrates with <see cref="HintsAPI"/> to manage active hints and their durations, formatting them for display using RueI's hint-building utilities.
    /// </remarks>
    public class Hint : Capybara
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hint"/> class.
        /// </summary>
        /// <remarks>
        /// Sets the initial position to <see cref="BasePosition"/> and assigns the content getter to <see cref="GetContent"/>.
        /// </remarks>
        public Hint()
        {
            Position = BasePosition;
            ContentGetter = GetContent;
        }


        /// <summary>
        /// Gets the name of the hint element.
        /// </summary>
        /// <value>
        /// A string representing the name of the hint, set to "Text".
        /// </value>
        public override string Name { get; } = "Text";

        /// <summary>
        /// Gets the description of the hint element.
        /// </summary>
        /// <value>
        /// A string describing the hint's purpose, set to "Displays text in the center of the screen."
        /// </value>
        public override string Description { get; } = "Displays text in the center of the screen.";

        /// <summary>
        /// Gets or sets the alignment style for the hint text.
        /// </summary>
        /// <value>
        /// The alignment style, defaulting to <see cref="HintBuilding.AlignStyle.Center"/>.
        /// </value>
        /// <remarks>
        /// This property controls how the hint text is aligned on the screen, using RueI's alignment options.
        /// </remarks>
        public override HintBuilding.AlignStyle Align { get; set; } = HintBuilding.AlignStyle.Center;

        /// <summary>
        /// Gets the base vertical position for the hint on the screen.
        /// </summary>
        /// <value>
        /// A float representing the vertical position in pixels, set to 750.
        /// </value>
        /// <remarks>
        /// This field determines the default position of the hint relative to the top of the screen.
        /// </remarks>
        public float BasePosition = 750;

        /// <summary>
        /// Generates the content for the hint element based on the player's active hints.
        /// </summary>
        /// <param name="core">The <see cref="DisplayCore"/> instance associated with the player's display hub.</param>
        /// <returns>A string containing the formatted hint content, or an empty string if no content is available.</returns>
        /// <remarks>
        /// This method retrieves the active hints for the player from <see cref="HintsAPI.ActiveHints"/>, formats them using RueI's hint-building utilities,
        /// and updates their durations. Hints with durations less than or equal to 0.1 seconds are removed. The content is built with a centered alignment
        /// and a font size of 100% to ensure visibility.
        /// </remarks>
        public override string GetContent(DisplayCore core)
        {
            // Attempt to get the Player instance from the DisplayCore hub
            if (!Player.TryGet(core.Hub, out Player pl)) return "";

            // Initialize a StringBuilder for formatting the hint content
            StringBuilder sb = new StringBuilder()
                .SetSize(100, MeasurementUnit.Percentage)
                .SetAlignment(Align);

            // Check if the player has active hints; if not, initialize an empty dictionary
            if (!HintsAPI.ActiveHints.TryGetValue(pl, out Dictionary<string, float>? activeHints))
            {
                HintsAPI.ActiveHints.Add(pl, new());
                return string.Empty;
            }

            // Iterate through the player's active hints, ordered by remaining duration
            foreach (KeyValuePair<Player, Dictionary<string, float>> entry in HintsAPI.ActiveHints.Where(x => x.Key == pl).ToList())
            {
                foreach (KeyValuePair<string, float> entry1 in entry.Value.OrderByDescending(x => x.Value))
                {
                    // Split the hint into lines and append each as a non-breaking line
                    foreach (string? line in entry1.Key.Split('\n'))
                    {
                        sb.AppendLine($"<nobr>{line}</nobr>");
                    }

                    // Decrease the hint's duration and remove it if it expires
                    HintsAPI.ActiveHints[entry.Key][entry1.Key] -= 0.5f;
                    if (HintsAPI.ActiveHints[entry.Key][entry1.Key] <= 0.1f)
                    {
                        HintsAPI.ActiveHints[entry.Key].Remove(entry1.Key);
                    }
                }
            }

            // Return the formatted hint content
            return sb.ToString();
        }
    }

}