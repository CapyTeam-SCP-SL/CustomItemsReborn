using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class GrenadeLauncher : CustomItem
    {
        private bool _useGrenades;
        private bool _ignoreModded;

        public override string Id => "GL-119";
        public override string Name => "Grenade Launcher";
        public override ItemType BaseType => ItemType.GunCrossvec;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.GrenadeLauncher;
            _useGrenades = config.UseGrenades;
            _ignoreModded = config.IgnoreModded;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnMultipleInRoom(RoomType.HczArmory, new[] { Vector3.zero });
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
            if (!Check(ev.Player.CurrentItem) || !ev.Player.IsAlive)
                return;

            ev.IsAllowed = false;

            ItemType grenadeType = ItemType.GrenadeHE;
            if (_useGrenades)
            {
                foreach (var item in ev.Player.Items.Where(item => item.Type == ItemType.GrenadeHE || (!_ignoreModded && item.Type == ItemType.GrenadeFlash)))
                {
                    grenadeType = item.Type;
                    ev.Player.RemoveItem(item);
                    break;
                }
            }

            var grenade = (ExplosiveGrenade)Item.Create(grenadeType);
            grenade.FuseTime = 0f;
            grenade.MaxRadius = 50f;

            grenade.SpawnActive(ev.Player.Position + ev.Player.Transform.forward, ev.Player);
        }
    }
}