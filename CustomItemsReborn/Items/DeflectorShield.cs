using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pools;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;
using CustomItemsReborn.API;
using Exiled.API.Features.DamageHandlers;

namespace CustomItemsReborn.Items
{
    public class DeflectorShield : CustomItem
    {
        private readonly HashSet<Player> _activeShields = new();
        private float _duration;
        private float _multiplier;

        public override string Id => "DeflectorShield";
        public override string Name => "Deflector Shield";
        public override ItemType BaseType => ItemType.ArmorHeavy;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.DeflectorShield;
            _duration = config.Duration;
            _multiplier = config.Multiplier;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnInRoom(RoomType.Hcz049, Vector3.zero);
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem += OnUsedItem;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem -= OnUsedItem;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        }

        private void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!Check(ev.Item) || _activeShields.Contains(ev.Player))
                return;

            _activeShields.Add(ev.Player);

            if (_duration > 0)
                Timing.CallDelayed(_duration, () => _activeShields.Remove(ev.Player));
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (!_activeShields.Contains(ev.Player))
                return;

            if (ev.DamageHandler.CustomBase is FirearmDamageHandler firearmDamage)
            {
                ev.IsAllowed = false;

                if (ev.Attacker != null)
                {
                    ev.Attacker.Hurt(firearmDamage.Damage * _multiplier);
                }
            }
        }
    }
}