using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Map;
using MEC;
using UnityEngine;
using CustomItemsReborn.API;
using LabApi.Features.Wrappers;
using Door = Exiled.API.Features.Doors.Door;

namespace CustomItemsReborn.Items
{
    public class EmpGrenade : CustomItem
    {
        private readonly List<Exiled.API.Features.TeslaGate> _disabledTeslas = new();
        private bool _openLockedDoors;
        private bool _openKeycardDoors;
        private List<DoorType> _blackListedDoorTypes;
        private bool _disableTeslaGates;
        private float _duration;

        public override string Id => "EM-119";
        public override string Name => "EMP Grenade";
        public override ItemType BaseType => ItemType.GrenadeHE;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.EmpGrenade;
            _openLockedDoors = config.OpenLockedDoors;
            _openKeycardDoors = config.OpenKeycardDoors;
            _blackListedDoorTypes = config.BlackListedDoorTypes;
            _disableTeslaGates = config.DisableTeslaGates;
            _duration = config.Duration;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnMultipleInRoom(RoomType.HczArmory, new[] { Vector3.zero });
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += OnExploded;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= OnExploded;
        }

        private void OnExploded(ExplodingGrenadeEventArgs ev)
        {
            if (!Check(ev.Projectile))
                return;

            if (_openLockedDoors || _openKeycardDoors)
            {
                foreach (Door door in Door.List.Where(door => Vector3.Distance(door.Position, ev.Projectile.Position) <= 10f))
                {
                    if (_blackListedDoorTypes.Contains(door.Type))
                        continue;

                    if (_openLockedDoors && door.IsLocked)
                        door.Unlock();

                    if (_openKeycardDoors && door.KeycardPermissions != KeycardPermissions.None)
                        door.ChangeLock(DoorLockType.None);
                }
            }

            if (_disableTeslaGates)
            {
                foreach (var tesla in Exiled.API.Features.TeslaGate.List.Where(tesla => Vector3.Distance(tesla.Position, ev.Projectile.Position) <= 10f))
                {
                    tesla.Base.enabled = false;
                    _disabledTeslas.Add(tesla);
                    Timing.CallDelayed(_duration, () => ReEnableTesla(tesla));
                }
            }

            ev.Projectile.FuseTime = 0f;
                Timing.CallDelayed(0.1f, () => ev.Projectile.Destroy());
        }

        private void ReEnableTesla(Exiled.API.Features.TeslaGate tesla)
        {
            if (_disabledTeslas.Remove(tesla))
                tesla.Base.enabled = true;
        }
    }
}