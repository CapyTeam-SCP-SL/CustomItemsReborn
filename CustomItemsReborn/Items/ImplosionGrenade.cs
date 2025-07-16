using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Map;
using MEC;
using PlayerRoles;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class ImplosionGrenade : CustomItem
    {
        private float _damageModifier;
        private int _suctionCount;
        private float _suctionPerTick;
        private float _suctionTickRate;
        private List<RoleTypeId> _blackListedRoles;

        public override string Id => "IG-119";
        public override string Name => "Implosion Grenade";
        public override ItemType BaseType => ItemType.GrenadeHE;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.ImplosionGrenade;
            _damageModifier = config.DamageModifier;
            _suctionCount = config.SuctionCount;
            _suctionPerTick = config.SuctionPerTick;
            _suctionTickRate = config.SuctionTickRate;
            _blackListedRoles = config.BlackListedRoles;
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

            Vector3 pos = ev.Projectile.Position;

            foreach (Player player in Player.List.Where(p => p.IsAlive && Vector3.Distance(p.Position, pos) <= 10f && !_blackListedRoles.Contains(p.Role.Type)))
            {
                Timing.RunCoroutine(PullPlayer(player, pos));
            }

            ev.Projectile.FuseTime = 0f;
                Timing.CallDelayed(0.1f, () => ev.Projectile.Destroy());
        }

        private IEnumerator<float> PullPlayer(Player player, Vector3 target)
        {
            int count = _suctionCount;

            while (count > 0)
            {
                if (!player.IsAlive)
                    yield break;

                Vector3 playerPos = player.Position;
                Vector3 direction = (target - playerPos).normalized;
                float distance = Vector3.Distance(playerPos, target);

                if (distance > 0.5f)
                {
                    player.Position = Vector3.MoveTowards(playerPos, target, _suctionPerTick);
                    player.Hurt(distance * _damageModifier);
                }

                count--;
                yield return Timing.WaitForSeconds(_suctionTickRate);
            }
        }
    }
}