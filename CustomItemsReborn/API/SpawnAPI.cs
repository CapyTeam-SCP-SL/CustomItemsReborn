// -----------------------------------------------------------------------
// <copyright file="SpawnAPI.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItems.API;

using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using UnityEngine;

/// <summary>
/// Provides utilities for spawning pickups and handling coordinate transformations in the game world.
/// </summary>
public class SpawnAPI
{
    /// <summary>
    /// Creates and spawns a pickup item in a specified room with given position and rotation.
    /// </summary>
    /// <param name="itemType">The type of item to spawn.</param>
    /// <param name="room">The room where the item will spawn.</param>
    /// <param name="relativePosition">The position relative to the room's origin with zero rotation.</param>
    /// <param name="rotation">The rotation of the pickup.</param>
    /// <param name="itemList">The list to store the pickup's serial number.</param>
    /// <returns>The spawned pickup, or null if creation fails.</returns>
    public Pickup? CreateAndSpawnPickup(ItemType itemType, RoomType room, Vector3 relativePosition, Quaternion rotation, List<ushort> itemList)
    {
        try
        {
            Room? targetRoom = Room.Get(room);
            if (targetRoom == null)
            {
                Log.Error($"Failed to spawn pickup: Room {room} not found.");
                return null;
            }

            Vector3 globalPosition = GetGlobalCoordinates(relativePosition, targetRoom);
            Pickup pickup = Pickup.CreateAndSpawn(itemType, globalPosition, rotation);

            itemList.Add(pickup.Serial);
            CustomItemsAPI.CreatedCustomItems.Add(pickup.Serial);

            Log.Debug($"Spawned pickup {pickup.Type} at {pickup.Position} in {targetRoom.Name} with rotation {pickup.Rotation}");
            return pickup;
        }
        catch (Exception ex)
        {
            Log.Error($"Error spawning pickup {itemType}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Calculates the local rotation of a player relative to their current room.
    /// </summary>
    /// <param name="player">The player to calculate the rotation for.</param>
    /// <returns>The relative rotation as a Vector3, or Vector3.zero if the room is null.</returns>
    public static Vector3 PlayerLocalRotation(Player player)
    {
        if (player == null)
        {
            Log.Warn("PlayerLocalRotation: Player is null.");
            return Vector3.zero;
        }

        if (player.CurrentRoom == null)
        {
            Log.Warn($"PlayerLocalRotation: Player {player.Nickname} has no current room.");
            return Vector3.zero;
        }

        try
        {
            Quaternion playerRotation = player.CameraTransform.rotation;
            Quaternion roomRotation = player.CurrentRoom.Rotation;

            float x = Mathf.DeltaAngle(roomRotation.eulerAngles.x, playerRotation.eulerAngles.x);
            float y = Mathf.DeltaAngle(roomRotation.eulerAngles.y, playerRotation.eulerAngles.y);
            float z = Mathf.DeltaAngle(roomRotation.eulerAngles.z, playerRotation.eulerAngles.z);

            return new Vector3(Mathf.Round(x), Mathf.Round(y), Mathf.Round(z));
        }
        catch (Exception ex)
        {
            Log.Error($"Error calculating player local rotation: {ex.Message}");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Converts relative coordinates (with zero rotation) to global coordinates based on room rotation.
    /// </summary>
    /// <param name="relativeCoordinates">The coordinates relative to the room's origin with zero rotation.</param>
    /// <param name="room">The room to base the transformation on.</param>
    /// <returns>The global coordinates, or Vector3.zero if the room is null.</returns>
    public static Vector3 GetGlobalCoordinates(Vector3 relativeCoordinates, Room room)
    {
        if (room == null)
        {
            Log.Warn("GetGlobalCoordinates: Room is null.");
            return Vector3.zero;
        }

        try
        {
            float roomYaw = Mathf.Round(room.Rotation.eulerAngles.y);
            Vector3 roomPosition = room.Position;

            switch (roomYaw)
            {
                case 0:
                    return roomPosition + relativeCoordinates;
                case 90:
                    return new Vector3(
                        roomPosition.x + relativeCoordinates.z,
                        roomPosition.y + relativeCoordinates.y,
                        roomPosition.z - relativeCoordinates.x);
                case 180:
                    return new Vector3(
                        roomPosition.x - relativeCoordinates.x,
                        roomPosition.y + relativeCoordinates.y,
                        roomPosition.z - relativeCoordinates.z);
                case 270:
                    return new Vector3(
                        roomPosition.x - relativeCoordinates.z,
                        roomPosition.y + relativeCoordinates.y,
                        roomPosition.z + relativeCoordinates.x);
                default:
                    Log.Warn($"Unexpected room rotation yaw: {roomYaw}");
                    return roomPosition + relativeCoordinates;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error calculating global coordinates: {ex.Message}");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Converts global coordinates to relative coordinates with zero rotation based on room rotation.
    /// </summary>
    /// <param name="globalCoordinates">The global coordinates to convert.</param>
    /// <param name="room">The room to base the transformation on.</param>
    /// <returns>The relative coordinates with zero rotation, or Vector3.zero if the room is null.</returns>
    public static Vector3 GetRelativeCoordinates(Vector3 globalCoordinates, Room room)
    {
        if (room == null)
        {
            Log.Warn("GetRelativeCoordinates: Room is null.");
            return Vector3.zero;
        }

        try
        {
            Vector3 relative = globalCoordinates - room.Position;
            float roomYaw = Mathf.Round(room.Rotation.eulerAngles.y);

            switch (roomYaw)
            {
                case 0:
                    return relative;
                case 90:
                    return new Vector3(-relative.z, relative.y, relative.x);
                case 180:
                    return new Vector3(-relative.x, relative.y, -relative.z);
                case 270:
                    return new Vector3(relative.z, relative.y, -relative.x);
                default:
                    Log.Warn($"Unexpected room rotation yaw: {roomYaw}");
                    return relative;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error calculating relative coordinates: {ex.Message}");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Converts a player's global position to relative coordinates with zero rotation.
    /// </summary>
    /// <param name="player">The player to calculate coordinates for.</param>
    /// <returns>The relative coordinates with zero rotation, or Vector3.zero if the room is null.</returns>
    public static Vector3 GetRelativeCoordinates(Player player)
    {
        if (player == null)
        {
            Log.Warn("GetRelativeCoordinates: Player is null.");
            return Vector3.zero;
        }

        if (player.CurrentRoom == null)
        {
            Log.Warn($"GetRelativeCoordinates: Player {player.Nickname} has no current room.");
            return Vector3.zero;
        }

        try
        {
            return GetRelativeCoordinates(player.Position, player.CurrentRoom);
        }
        catch (Exception ex)
        {
            Log.Error($"Error calculating player relative coordinates: {ex.Message}");
            return Vector3.zero;
        }
    }
}