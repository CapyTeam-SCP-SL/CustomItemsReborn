// -----------------------------------------------------------------------
// <copyright file="CustomItemExtensions.cs" company="CapyTeam SCP: SL">
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pickups;
    using UnityEngine;

    /// <summary>
    /// Extension methods for <see cref="CustomItem"/> and related types.
    /// </summary>
    public static class CustomItemExtensions
    {
        #region Player Extensions

        /// <summary>
        /// Returns true if the player is currently holding an item of the specified type.
        /// </summary>
        public static bool HasCustomItem<T>(this Player player) where T : CustomItem
            => player.Items.Any(i => CustomItem.TryGet(i, out T _));

        /// <summary>
        /// Returns true if the player is currently holding an item with the specified ID.
        /// </summary>
        public static bool HasCustomItem(this Player player, string id)
            => player.Items.Any(i => CustomItem.TryGet(i, out var ci) && ci.Id == id);

        /// <summary>
        /// Returns the first held item of the specified type, or null.
        /// </summary>
        public static Item GetCustomItem<T>(this Player player) where T : CustomItem
            => player.Items.FirstOrDefault(i => CustomItem.TryGet(i, out T _));

        /// <summary>
        /// Returns the first held item with the specified ID, or null.
        /// </summary>
        public static Item GetCustomItem(this Player player, string id)
            => player.Items.FirstOrDefault(i => CustomItem.TryGet(i, out var ci) && ci.Id == id);

        /// <summary>
        /// Removes the first held item of the specified type from the player.
        /// </summary>
        public static bool RemoveCustomItem<T>(this Player player) where T : CustomItem
        {
            var item = player.GetCustomItem<T>();
            if (item == null) return false;
            player.RemoveItem(item);
            return true;
        }

        /// <summary>
        /// Removes the first held item with the specified ID from the player.
        /// </summary>
        public static bool RemoveCustomItem(this Player player, string id)
        {
            var item = player.GetCustomItem(id);
            if (item == null) return false;
            player.RemoveItem(item);
            return true;
        }

        /// <summary>
        /// Adds the specified amount of custom items to the player's inventory.
        /// </summary>
        public static Item[] GiveCustomItem<T>(this Player player, int amount = 1) where T : CustomItem, new()
        {
            var ci = new T();
            var items = new Item[amount];
            for (int i = 0; i < amount; i++)
                items[i] = ci.Give(player);
            return items;
        }

        #endregion

        #region Item Extensions

        /// <summary>
        /// Attempts to retrieve the <see cref="CustomItem"/> wrapper for this item.
        /// </summary>
        public static bool TryGetCustomItem(this Item item, out CustomItem customItem)
            => CustomItem.TryGet(item, out customItem);

        /// <summary>
        /// Returns true if this item is a tracked custom item.
        /// </summary>
        public static bool IsCustomItem(this Item item)
            => CustomItem.TryGet(item, out _);

        /// <summary>
        /// Returns the custom item ID, or null if not tracked.
        /// </summary>
        public static string GetCustomItemId(this Item item)
            => CustomItem.TryGet(item, out var ci) ? ci.Id : null;

        #endregion

        #region Pickup Extensions

        /// <summary>
        /// Attempts to retrieve the <see cref="CustomItem"/> wrapper for this pickup.
        /// </summary>
        public static bool TryGetCustomItem(this Pickup pickup, out CustomItem customItem)
            => CustomItem.TryGet(pickup, out customItem);

        /// <summary>
        /// Returns true if this pickup is a tracked custom item.
        /// </summary>
        public static bool IsCustomItem(this Pickup pickup)
            => CustomItem.TryGet(pickup, out _);

        /// <summary>
        /// Returns the custom item ID, or null if not tracked.
        /// </summary>
        public static string GetCustomItemId(this Pickup pickup)
            => CustomItem.TryGet(pickup, out var ci) ? ci.Id : null;

        #endregion

        #region Vector3 Extensions

        /// <summary>
        /// Spawns a custom item at the specified position and returns the created pickup.
        /// </summary>
        public static Pickup SpawnCustomItem<T>(this Vector3 position, Vector3 rotation = default) where T : CustomItem, new()
            => new T().Spawn(position, rotation);

        /// <summary>
        /// Spawns a custom item by ID at the specified position and returns the created pickup.
        /// </summary>
        public static Pickup SpawnCustomItem(this Vector3 position, string id, Vector3 rotation = default)
        {
            var ci = CustomItem.Get(id);
            return ci?.Spawn(position, rotation);
        }

        #endregion

        #region Collection Extensions

        /// <summary>
        /// Returns all custom items whose IDs start with the specified prefix.
        /// </summary>
        public static IEnumerable<CustomItem> GetCustomItemsByPrefix(string prefix)
            => CustomItem.All.Where(ci => ci.Id.StartsWith(prefix));

        /// <summary>
        /// Returns all custom items whose names contain the specified substring (case-insensitive).
        /// </summary>
        public static IEnumerable<CustomItem> GetCustomItemsByName(string substring)
            => CustomItem.All.Where(ci => ci.Name.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0);

        #endregion

        #region Serial Extensions

        /// <summary>
        /// Returns the custom item associated with the given serial, or null.
        /// </summary>
        public static CustomItem GetBySerial(ushort serial)
            => CustomItem.TryGet(serial, out var ci) ? ci : null;

        /// <summary>
        /// Returns true if the given serial is currently tracked as a custom item.
        /// </summary>
        public static bool IsTracked(ushort serial) => CustomItem.TryGet(serial, out _);

        #endregion

        #region Count Extensions

        /// <summary>
        /// Returns the count of tracked instances for the specified custom item type.
        /// </summary>
        public static int GetTrackedCount<T>() where T : CustomItem, new()
            => new T().TrackedSerialsPublic.Count;

        /// <summary>
        /// Returns the count of tracked instances for the specified custom item ID.
        /// </summary>
        public static int GetTrackedCount(string id)
            => CustomItem.Get(id)?.TrackedSerialsPublic.Count ?? 0;

        /// <summary>
        /// Returns all currently tracked serials for the specified custom item type.
        /// </summary>
        public static IEnumerable<ushort> GetTrackedSerials<T>() where T : CustomItem, new()
            => new T().TrackedSerialsPublic;

        /// <summary>
        /// Returns all currently tracked serials for the specified custom item ID.
        /// </summary>
        public static IEnumerable<ushort> GetTrackedSerials(string id)
            => CustomItem.Get(id)?.TrackedSerialsPublic ?? Enumerable.Empty<ushort>();

        #endregion
    }
}