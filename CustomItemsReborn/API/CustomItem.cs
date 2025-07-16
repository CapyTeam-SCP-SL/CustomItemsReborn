// -----------------------------------------------------------------------
// <copyright file="CustomItem.cs" company="CapyTeam SCP: SL">
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Enums;

namespace CustomItemsReborn.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pickups;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Map;
    using MEC;
    using UnityEngine;

    /// <summary>
    /// Base class for any custom item implementation.
    /// </summary>
    public abstract class CustomItem
    {
        private static readonly List<CustomItem> RegisteredItems = new();
        private static readonly Dictionary<ushort, CustomItem> SerialToItem = new();

        /// <summary>
        /// Gets the globally unique identifier for this item type.
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Gets the human-readable name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the underlying <see cref="ItemType"/> used for spawning.
        /// </summary>
        public abstract ItemType BaseType { get; }

        /// <summary>
        /// Gets the broadcast shown when a player picks up this item.
        /// </summary>
        public virtual string PickupBroadcast
        {
            get => string.Empty;
            set => PickupBroadcast = value;
        }

        /// <summary>
        /// Gets the hint shown when a player selects this item.
        /// </summary>
        public virtual string ChangeHint
        {
            get => string.Empty;
            set => ChangeHint =value;
        }

        /// <summary>
        /// Gets all registered custom items as read-only.
        /// </summary>
        public static IReadOnlyList<CustomItem> All => RegisteredItems;

        /// <summary>
        /// Gets all currently tracked serials for this custom item type.
        /// </summary>
        public IReadOnlyCollection<ushort> TrackedSerialsPublic => TrackedSerials;

        private readonly HashSet<ushort> TrackedSerials = new();

        /// <summary>
        /// Starts the global event handling system.
        /// </summary>
        public static void StartSystem()
        {
            Exiled.Events.Handlers.Player.PickingUpItem += OnInternalPickup;
            Exiled.Events.Handlers.Player.DroppingItem += OnInternalDrop;
            Exiled.Events.Handlers.Player.ChangedItem += OnInternalChanged;
            Exiled.Events.Handlers.Map.PickupDestroyed += OnInternalDestroyed;
            Exiled.Events.Handlers.Player.Died += OnInternalDied;
            Exiled.Events.Handlers.Server.RestartingRound += OnInternalRestart;
            
            foreach (var item in All)
            {
                try
                {
                    item.Initialize();
                    Log.Debug($"Initialized custom item: {item.Name} (ID: {item.Id})");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to initialize custom item {item.Name} (ID: {item.Id}): {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Stops the global event handling system and cleans up memory.
        /// </summary>
        public static void StopSystem()
        {
            Exiled.Events.Handlers.Player.PickingUpItem -= OnInternalPickup;
            Exiled.Events.Handlers.Player.DroppingItem -= OnInternalDrop;
            Exiled.Events.Handlers.Player.ChangedItem -= OnInternalChanged;
            Exiled.Events.Handlers.Map.PickupDestroyed -= OnInternalDestroyed;
            Exiled.Events.Handlers.Player.Died -= OnInternalDied;
            Exiled.Events.Handlers.Server.RestartingRound -= OnInternalRestart;

            foreach (var item in RegisteredItems)
                item.UnsubscribeEvents();
            RegisteredItems.Clear();
            SerialToItem.Clear();
        }

        /// <summary>
        /// Registers a single custom item instance.
        /// </summary>
        /// <param name="item">The instance to register.</param>
        public static void Register(CustomItem item)
        {
            if (RegisteredItems.Any(i => i.Id == item.Id))
                throw new InvalidOperationException($"CustomItem with Id '{item.Id}' already registered.");
            RegisteredItems.Add(item);
            item.SubscribeEvents();
        }

        /// <summary>
        /// Registers all non-abstract <see cref="CustomItem"/> implementations in the executing assembly.
        /// </summary>
        public static void RegisterAll()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && typeof(CustomItem).IsAssignableFrom(t));
            foreach (var t in types)
            {
                if (RegisteredItems.Any(i => i.GetType() == t)) continue;
                var instance = (CustomItem)Activator.CreateInstance(t);
                Register(instance);
            }
        }

        /// <summary>
        /// Registers a single generic <typeparamref name="T"/> without manual instantiation.
        /// </summary>
        /// <typeparam name="T">The custom item type.</typeparam>
        public static void Register<T>() where T : CustomItem, new()
        {
            if (RegisteredItems.Any(i => i is T)) return;
            Register(new T());
        }
        
        /// <summary>Unregisters a single instance.</summary>
        public static void Unregister(CustomItem item)
        {
            if (item == null) return;
            item.UnsubscribeEvents();
            RegisteredItems.Remove(item);
            foreach (var serial in item.TrackedSerialsPublic)
                SerialToItem.Remove(serial);
        }

        /// <summary>Unregisters a custom item by identifier.</summary>
        public static void Unregister(string id)
        {
            var item = Get(id);
            if (item != null) Unregister(item);
        }

        /// <summary>Unregisters a custom item by generic type.</summary>
        public static void Unregister<T>() where T : CustomItem => Unregister(typeof(T));

        /// <summary>Unregisters a custom item by runtime type.</summary>
        public static void Unregister(Type type)
        {
            var item = RegisteredItems.FirstOrDefault(i => i.GetType() == type);
            if (item != null) Unregister(item);
        }

        /// <summary>Unregisters all registered custom items.</summary>
        public static void UnregisterAll()
        {
            foreach (var item in RegisteredItems.ToList())
                Unregister(item);
        }
        
        /// <summary>
        /// Retrieves a custom item by its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The matching <see cref="CustomItem"/> or null.</returns>
        public static CustomItem Get(string id) => RegisteredItems.FirstOrDefault(ci => ci.Id == id);

        /// <summary>
        /// Retrieves a custom item by its identifier, cast to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>The matching <typeparamref name="T"/> or null.</returns>
        public static T Get<T>(string id) where T : CustomItem => Get(id) as T;

        /// <summary>
        /// Attempts to retrieve the <see cref="CustomItem"/> wrapper for the given serial.
        /// </summary>
        /// <param name="serial">The serial number.</param>
        /// <param name="customItem">The resolved custom item.</param>
        /// <returns>True if found; otherwise, false.</returns>
        public static bool TryGet(ushort serial, out CustomItem customItem) => SerialToItem.TryGetValue(serial, out customItem);

        /// <summary>
        /// Attempts to retrieve the <see cref="CustomItem"/> wrapper for the given <see cref="Item"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="customItem">The resolved custom item.</param>
        /// <returns>True if found; otherwise, false.</returns>
        public static bool TryGet(Item item, out CustomItem customItem) => TryGet(item.Serial, out customItem);

        /// <summary>
        /// Attempts to retrieve the <see cref="CustomItem"/> wrapper for the given <see cref="Pickup"/>.
        /// </summary>
        /// <param name="pickup">The pickup.</param>
        /// <param name="customItem">The resolved custom item.</param>
        /// <returns>True if found; otherwise, false.</returns>
        public static bool TryGet(Pickup pickup, out CustomItem customItem) => TryGet(pickup.Serial, out customItem);

        /// <summary>
        /// Gives <paramref name="amount"/> items of this type to <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="amount">The quantity.</param>
        /// <returns>The created item instance.</returns>
        public Item Give(Player player, int amount = 1)
        {
            var item = Item.Create(BaseType);
            TrackedSerials.Add(item.Serial);
            SerialToItem[item.Serial] = this;
            player.AddItem(item);
            return item;
        }

        /// <summary>
        /// Spawns a pickup of this type at the specified position and rotation.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="rotation">The euler rotation.</param>
        /// <returns>The created pickup.</returns>
        public Pickup Spawn(Vector3 position, Vector3 rotation = default)
        {
            var pickup = Pickup.CreateAndSpawn(BaseType, position, Quaternion.Euler(rotation));
            TrackedSerials.Add(pickup.Serial);
            SerialToItem[pickup.Serial] = this;
            return pickup;
        }
        
        /// <summary>Checks if <paramref name="item"/> is an instance of this custom item.</summary>
        public bool Check(Item item) => item != null && CustomItem.TryGet(item, out var ci) && ci.Id == Id;

        /// <summary>Checks if <paramref name="pickup"/> is an instance of this custom item.</summary>
        public bool Check(Pickup pickup) => pickup != null && CustomItem.TryGet(pickup, out var ci) && ci.Id == Id;

        /// <summary>Checks if <paramref name="serial"/> is an instance of this custom item.</summary>
        public bool Check(ushort serial) => SerialToItem.TryGetValue(serial, out var ci) && ci.Id == Id;

        /// <summary>Checks if <paramref name="player"/> is holding an instance of this custom item.</summary>
        public bool Check(Player player) => player?.CurrentItem != null && Check(player.CurrentItem);

        /// <summary>Checks if <paramref name="entity"/> (Item or Pickup) is an instance of this custom item.</summary>
        public bool Check(object entity) => entity switch
        {
            Item i   => Check(i),
            Pickup p => Check(p),
            _        => false
        };

        /// <summary>
        /// Initializes the custom item by spawning it in the game world.
        /// Override this method in derived classes to define specific spawn locations or conditions.
        /// </summary>
        public virtual void Initialize(){}

        /// <summary>
        /// Spawns the custom item in a specified room at the given position and rotation.
        /// </summary>
        /// <param name="roomType">The type of room to spawn the item in.</param>
        /// <param name="position">The local position relative to the room's transform.</param>
        /// <param name="rotation">The rotation of the spawned item.</param>
        /// <returns>The created pickup instance.</returns>
        protected Pickup SpawnInRoom(RoomType roomType, Vector3 position, Vector3 rotation = default)
        {
            // Find the room by type
            Room room = Room.Get(roomType);
            if (room == null)
            {
                Log.Error($"Failed to spawn {Name} (ID: {Id}): RoomType {roomType} not found.");
                return null;
            }

            // Calculate world position based on room's transform
            Vector3 worldPosition = room.Transform.TransformPoint(position);
            Quaternion worldRotation = Quaternion.Euler(rotation);

            // Spawn the item using the base Spawn method
            Pickup pickup = Spawn(worldPosition, rotation);
            if (pickup != null)
            {
                Log.Debug($"Spawned {Name} (ID: {Id}) at {worldPosition} in {roomType}.");
            }
            else
            {
                Log.Error($"Failed to spawn {Name} (ID: {Id}) at {worldPosition}.");
            }

            return pickup;
        }

        /// <summary>
        /// Spawns multiple instances of the custom item in a specified room.
        /// </summary>
        /// <param name="roomType">The type of room to spawn the items in.</param>
        /// <param name="positions">Array of local positions relative to the room's transform.</param>
        /// <param name="rotations">Array of rotations for the spawned items (optional).</param>
        /// <returns>An array of created pickup instances.</returns>
        protected Pickup[] SpawnMultipleInRoom(RoomType roomType, Vector3[] positions, Vector3[] rotations = null)
        {
            Pickup[] pickups = new Pickup[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 rotation = rotations != null && i < rotations.Length ? rotations[i] : Vector3.zero;
                pickups[i] = SpawnInRoom(roomType, positions[i], rotation);
            }
            return pickups;
        }

        /// <summary>
        /// Called when a player picks up this item.
        /// </summary>
        protected virtual void OnPickedUp(PickingUpItemEventArgs ev) { }

        /// <summary>
        /// Called when a player drops this item.
        /// </summary>
        protected virtual void OnDropped(DroppingItemEventArgs ev) { }

        /// <summary>
        /// Called when a player selects this item in the inventory.
        /// </summary>
        protected virtual void OnSelected(ChangedItemEventArgs ev) { }

        /// <summary>
        /// Override to subscribe to any additional events.
        /// </summary>
        protected virtual void SubscribeEvents() { }

        /// <summary>
        /// Override to unsubscribe from any additional events.
        /// </summary>
        protected virtual void UnsubscribeEvents() { }

        #region Internal Event Handlers

        private static void OnInternalPickup(PickingUpItemEventArgs ev)
        {
            foreach (var item in RegisteredItems)
            {
                if (!item.TrackedSerials.Contains(ev.Pickup.Serial)) continue;
                item.OnPickedUp(ev);
                if (!string.IsNullOrEmpty(item.PickupBroadcast))
                    ev.Player.Broadcast(3, item.PickupBroadcast);
            }
        }

        private static void OnInternalDrop(DroppingItemEventArgs ev)
        {
            foreach (var item in RegisteredItems)
            {
                if (!item.TrackedSerials.Contains(ev.Item.Serial)) continue;
                item.OnDropped(ev);
            }
        }

        private static void OnInternalChanged(ChangedItemEventArgs ev)
        {
            foreach (var item in RegisteredItems)
            {
                if (ev.Item == null || !item.TrackedSerials.Contains(ev.Item.Serial)) continue;
                item.OnSelected(ev);
                if (!string.IsNullOrEmpty(item.ChangeHint))
                    ev.Player.Hint(item.ChangeHint, 5f);
            }
        }

        private static void OnInternalDestroyed(PickupDestroyedEventArgs ev)
        {
            foreach (var item in RegisteredItems)
                item.TrackedSerials.Remove(ev.Pickup.Serial);
            SerialToItem.Remove(ev.Pickup.Serial);
        }

        private static void OnInternalDied(DiedEventArgs ev)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                foreach (var item in RegisteredItems)
                {
                    foreach (var i in ev.Player.Items)
                    {
                        item.TrackedSerials.Remove(i.Serial);
                        SerialToItem.Remove(i.Serial);
                    }
                }
            });
        }

        private static void OnInternalRestart()
        {
            foreach (var item in RegisteredItems)
                item.TrackedSerials.Clear();
            SerialToItem.Clear();
        }

        #endregion
    }
}