// -----------------------------------------------------------------------
// <copyright file="SpawnAPI.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace CustomItemsReborn.API;

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
    private const float Epsilon = 0.01f; // For float comparison

    /// <summary>
    /// Creates and spawns a pickup item in a specified room with given position and rotation.
    /// </summary>
    /// <param name="itemType">The type of item to spawn.</param>
    /// <param name="room">The room where the item will spawn.</param>
    /// <param name="relativePosition">The position relative to the room's origin with zero rotation.</param>
    /// <param name="rotation">The rotation of the pickup.</param>
    /// <param name="itemList">The set to store the pickup's serial number.</param>
    /// <returns>The spawned pickup, or null if creation fails.</returns>
    public Pickup? CreateAndSpawnPickup(ItemType itemType, RoomType room, Vector3 relativePosition, Quaternion rotation, HashSet<ushort> itemList)
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
    /// <returns>The relative rotation as a Vector3, or Vector3.zero if invalid.</returns>
    public static Vector3 PlayerLocalRotation(Player player)
    {
        if (player?.CurrentRoom == null)
        {
            Log.Warn($"PlayerLocalRotation: Player or room is null. Player: {player?.Nickname ?? "null"}");
            return Vector3.zero;
        }

        try
        {
            Quaternion playerRotation = player.CameraTransform.rotation;
            Quaternion roomRotation = player.CurrentRoom.Rotation;

            return new Vector3(
                Mathf.Round(Mathf.DeltaAngle(roomRotation.eulerAngles.x, playerRotation.eulerAngles.x)),
                Mathf.Round(Mathf.DeltaAngle(roomRotation.eulerAngles.y, playerRotation.eulerAngles.y)),
                Mathf.Round(Mathf.DeltaAngle(roomRotation.eulerAngles.z, playerRotation.eulerAngles.z)));
        }
        catch (Exception ex)
        {
            Log.Error($"Error calculating player local rotation: {ex.Message}");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Converts relative coordinates to global coordinates based on room rotation.
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

            return roomYaw switch
            {
                0f => roomPosition + relativeCoordinates,
                90f => new Vector3(
                    roomPosition.x + relativeCoordinates.z,
                    roomPosition.y + relativeCoordinates.y,
                    roomPosition.z - relativeCoordinates.x),
                180f => new Vector3(
                    roomPosition.x - relativeCoordinates.x,
                    roomPosition.y + relativeCoordinates.y,
                    roomPosition.z - relativeCoordinates.z),
                270f => new Vector3(
                    roomPosition.x - relativeCoordinates.z,
                    roomPosition.y + relativeCoordinates.y,
                    roomPosition.z + relativeCoordinates.x),
                _ => LogWarning(roomPosition, relativeCoordinates, roomYaw)
            };
        }
        catch (Exception ex)
        {
            Log.Error($"Error calculating global coordinates: {ex.Message}");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Converts global coordinates to relative coordinates based on room rotation.
    /// </summary>
    /// <param name="globalCoordinates">The global coordinates to convert.</param>
    /// <param name="room">The room to base the transformation on.</param>
    /// <returns>The relative coordinates, or Vector3.zero if the room is null.</returns>
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

            return roomYaw switch
            {
                0f => relative,
                90f => new Vector3(-relative.z, relative.y, relative.x),
                180f => new Vector3(-relative.x, relative.y, -relative.z),
                270f => new Vector3(relative.z, relative.y, -relative.x),
                _ => LogWarning(relative, roomYaw)
            };
        }
        catch (Exception ex)
        {
            Log.Error($"Error calculating relative coordinates: {ex.Message}");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Converts a player's global position to relative coordinates.
    /// </summary>
    /// <param name="player">The player to calculate coordinates for.</param>
    /// <returns>The relative coordinates, or Vector3.zero if invalid.</returns>
    public static Vector3 GetRelativeCoordinates(Player player)
    {
        if (player?.CurrentRoom == null)
        {
            Log.Warn($"GetRelativeCoordinates: Player or room is null. Player: {player?.Nickname ?? "null"}");
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

    private static Vector3 LogWarning(Vector3 relative, float roomYaw)
    {
        Log.Warn($"Unexpected room rotation yaw: {roomYaw}");
        return relative;
    }

    private static Vector3 LogWarning(Vector3 roomPosition, Vector3 relativeCoordinates, float roomYaw)
    {
        Log.Warn($"Unexpected room rotation yaw: {roomYaw}");
        return roomPosition + relativeCoordinates;
    }
}