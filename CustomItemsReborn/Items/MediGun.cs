using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using PlayerStatsSystem;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class MediGun : CustomItem
    {
        private bool _healZombies;
        private bool _healZombiesTeamCheck;
        private float _healingModifier;
        private int _zombieHealingRequired;

        public override string Id => "MG-119";
        public override string Name => "MediGun";
        public override ItemType BaseType => ItemType.GunCOM15;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.MediGun;
            _healZombies = config.HealZombies;
            _healZombiesTeamCheck = config.HealZombiesTeamCheck;
            _healingModifier = config.HealingModifier;
            _zombieHealingRequired = config.ZombieHealingRequired;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnMultipleInRoom(RoomType.LczToilets, new[] { Vector3.zero });
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
        }

        private void OnShooting(ShootingEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ev.IsAllowed = false;

            if (!Physics.Raycast(ev.Player.Position, ev.Player.Transform.forward, out var hit, 10f))
                return;

            var target = Player.Get(hit.transform.gameObject);
            if (target == null || !target.IsAlive)
                return;

            if (_healZombies && target.Role.Type == PlayerRoles.RoleTypeId.Scp0492)
            {
                if (_healZombiesTeamCheck && target.Role.Side != ev.Player.Role.Side)
                    return;

                target.Heal(_zombieHealingRequired);
            }
            else if (target.Role.Type != PlayerRoles.RoleTypeId.Scp0492)
            {
                target.Heal(10 * _healingModifier);
            }
        }
    }
}