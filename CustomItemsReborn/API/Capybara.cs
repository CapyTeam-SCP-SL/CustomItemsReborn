// -----------------------------------------------------------------------
// <copyright file="Capybara.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.API
{
    using RueI.Displays;
    using RueI.Elements;
    using RueI.Extensions.HintBuilding;

    /// <summary>
    /// Represents a dynamic UI element for displaying custom content in the game, inheriting from <see cref="DynamicElement"/>.
    /// </summary>
    /// <remarks>
    /// This class is designed to work with the RueI library to create customizable UI elements, such as hints or overlays,
    /// that can be dynamically updated and displayed to players. Derived classes should override the virtual properties and
    /// methods to provide specific content and behavior.
    /// </remarks>
    public class Capybara : DynamicElement
    {

        /// <summary>
        /// Gets the name of the UI element.
        /// </summary>
        /// <remarks>
        /// This property provides a unique identifier or title for the element. Derived classes should override this to return
        /// a meaningful name. The default value is an empty string.
        /// </remarks>
        public virtual string Name { get; } = string.Empty;

        /// <summary>
        /// Gets the description of the UI element.
        /// </summary>
        /// <remarks>
        /// This property provides a brief description of the element's purpose or content. Derived classes should override this
        /// to return a meaningful description. The default value is an empty string.
        /// </remarks>
        public virtual string Description { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the UI element can be turned down or dismissed.
        /// </summary>
        /// <remarks>
        /// When set to <c>true</c>, the element can be hidden or dismissed by the player or system. Derived classes can override
        /// this to control visibility behavior. The default value is <c>true</c>.
        /// </remarks>
        public virtual bool CanBeTurnedDown { get; } = true;


        /// <summary>
        /// Gets or sets the alignment style for the UI element.
        /// </summary>
        /// <remarks>
        /// This property determines how the element is positioned on the screen, using the <see cref="HintBuilding.AlignStyle"/>
        /// enumeration from the RueI library. Derived classes can override or set this to customize alignment. The default value
        /// is the default alignment style provided by RueI.
        /// </remarks>
        public virtual HintBuilding.AlignStyle Align { get; set; } = default;

        /// <summary>
        /// Retrieves the content to be displayed by the UI element.
        /// </summary>
        /// <param name="hub">The <see cref="DisplayCore"/> instance managing the display context.</param>
        /// <returns>A string containing the content to be rendered, or an empty string by default.</returns>
        /// <remarks>
        /// This method is called by the RueI system to generate the content for the UI element. Derived classes should override
        /// this method to provide dynamic content based on the game state or player context. The default implementation returns
        /// an empty string.
        /// </remarks>
        public virtual string GetContent(DisplayCore hub)
        {
            return string.Empty;
        }
    }
}