namespace CustomItems.API;

using Exiled.API.Enums;
using Exiled.API.Features;

using Exiled.API.Features.Pickups;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAPI
    {
    public Pickup CreateAndSpawnPickup(ItemType itemType, RoomType room, Vector3 pos, Quaternion rot,List<ushort> itemList)
    {
        var pickup = Pickup.CreateAndSpawn(itemType, GetGlobalCords(pos, Room.Get(room)), rot);

        itemList.Add(pickup.Serial);
        CustomItemsAPI.CreatedCustomItems.Add(pickup.Serial);
        Log.Debug($"Pickup created and spawned {pickup.Type} at {pickup.Position} in {pickup.Room.Name} with rotation {pickup.Rotation}");
        return pickup;
    }
        
    public static Vector3 PlayerLocalRotation(Player player)
    {
        if (player.CurrentRoom == null)
            return new Vector3(123f, 456f, 789f);
        Quaternion rotation1 = player.CameraTransform.rotation;
        double x = (double)(int)Math.Round((double)Mathf.DeltaAngle(player.CurrentRoom.Rotation.eulerAngles.x, rotation1.eulerAngles.x));
        Quaternion rotation2 = player.CurrentRoom.Rotation;
        double y = (double)(int)Math.Round((double)Mathf.DeltaAngle(rotation2.eulerAngles.y, rotation1.eulerAngles.y));
        rotation2 = player.CurrentRoom.Rotation;
        double z = (double)(int)Math.Round((double)Mathf.DeltaAngle(rotation2.eulerAngles.z, rotation1.eulerAngles.z));
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static Vector3 GetGlobalCords(Vector3 relCordsWirh0Rotation, Room room)
    {
        if ((double)Math.Abs(room.Rotation.eulerAngles.y - 0.0f) < 1.0)
            return new Vector3(room.Position.x + relCordsWirh0Rotation.x, room.Position.y + relCordsWirh0Rotation.y, room.Position.z + relCordsWirh0Rotation.z);
        Quaternion rotation = room.Rotation;
        if ((double)Math.Abs(rotation.eulerAngles.y - 90f) < 1.0)
            return new Vector3(room.Position.x + relCordsWirh0Rotation.z, room.Position.y + relCordsWirh0Rotation.y, room.Position.z - relCordsWirh0Rotation.x);
        rotation = room.Rotation;
        if ((double)Math.Abs(rotation.eulerAngles.y - 180f) < 1.0)
            return new Vector3(room.Position.x - relCordsWirh0Rotation.x, room.Position.y + relCordsWirh0Rotation.y, room.Position.z - relCordsWirh0Rotation.z);
        rotation = room.Rotation;
        return (double)Math.Abs(rotation.eulerAngles.y - 270f) < 1.0 ? new Vector3(room.Position.x - relCordsWirh0Rotation.z, room.Position.y + relCordsWirh0Rotation.y, room.Position.z + relCordsWirh0Rotation.x) : new Vector3(111f, 222f, 333f);
    }

    public static Vector3 GetRelCordsWith0Rotation(Vector3 relCords, float roomEulerY)
    {
        if ((double)Math.Abs(roomEulerY - 0.0f) < 1.0)
            return relCords;
        if ((double)Math.Abs(roomEulerY - 90f) < 1.0)
            return new Vector3(relCords.z * -1f, relCords.y, relCords.x);
        if ((double)Math.Abs(roomEulerY - 180f) < 1.0)
            return new Vector3(relCords.x * -1f, relCords.y, relCords.z * -1f);
        return (double)Math.Abs(roomEulerY - 270f) < 1.0 ? new Vector3(relCords.z, relCords.y, relCords.x * -1f) : Vector3.zero;
    }

    public static Vector3 GetRelCordsWith0Rotation(Vector3 cords, Room room)
    {
        Vector3 cordsWith0Rotation = new Vector3(cords.x - room.Position.x, cords.y - room.Position.y, cords.z - room.Position.z);
        if ((double)Math.Abs(room.Rotation.eulerAngles.y - 0.0f) < 1.0)
            return cordsWith0Rotation;
        Quaternion rotation = room.Rotation;
        if ((double)Math.Abs(rotation.eulerAngles.y - 90f) < 1.0)
            return new Vector3(cordsWith0Rotation.z * -1f, cordsWith0Rotation.y, cordsWith0Rotation.x);
        rotation = room.Rotation;
        if ((double)Math.Abs(rotation.eulerAngles.y - 180f) < 1.0)
            return new Vector3(cordsWith0Rotation.x * -1f, cordsWith0Rotation.y, cordsWith0Rotation.z * -1f);
        rotation = room.Rotation;
        return (double)Math.Abs(rotation.eulerAngles.y - 270f) < 1.0 ? new Vector3(cordsWith0Rotation.z, cordsWith0Rotation.y, cordsWith0Rotation.x * -1f) : Vector3.zero;
    }

    public static Vector3 GetRelCordsWith0Rotation(Player player)
    {
        if (player.CurrentRoom == null)
        {
            Log.Warn("GetRelCordsWith0Rotation(Player player) ERROR    -    player.CurrentRoom is null");
            return new Vector3(111f, 222f, 333f);
        }
        Room currentRoom = player.CurrentRoom;
        return GetRelCordsWith0Rotation(new Vector3(player.Position.x - currentRoom.Position.x, player.Position.y - currentRoom.Position.y, player.Position.z - currentRoom.Position.z), player.CurrentRoom.Rotation.eulerAngles.y);
    }

}
