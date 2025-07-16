using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class LuckyCoin : CustomItem
    {
        private readonly List<Player> _active = ListPool<Player>.Pool.Get();
        private readonly List<Player> _cooldown = ListPool<Player>.Pool.Get();
        private float _duration;
        private float _cooldownDuration;

        public override string Id => "LC-119";
        public override string Name => "Lucky Coin";
        public override ItemType BaseType => ItemType.Coin;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.LuckyCoin;
            _duration = config.Duration;
            _cooldownDuration = config.CooldownDuration;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnInRoom(RoomType.LczToilets, Vector3.zero);
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.FlippingCoin += OnFlippingCoin;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.FlippingCoin -= OnFlippingCoin;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            ListPool<Player>.Pool.Return(_active);
            ListPool<Player>.Pool.Return(_cooldown);
        }

        private void OnFlippingCoin(FlippingCoinEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem) || _cooldown.Contains(ev.Player))
                return;

            _active.Add(ev.Player);
            _cooldown.Add(ev.Player);

            Timing.CallDelayed(_duration, () => _active.Remove(ev.Player));
            Timing.CallDelayed(_cooldownDuration, () => _cooldown.Remove(ev.Player));

            ev.IsTails = UnityEngine.Random.value > 0.5f;
            if (ev.IsTails)
            {
                ev.Player.EnableEffect(EffectType.MovementBoost, _duration);
            }
            else
            {
                ev.Player.EnableEffect(EffectType.AntiScp207, _duration);
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (!_active.Contains(ev.Player))
                return;

            ev.Amount *= UnityEngine.Random.value > 0.5f ? 0f : 2f;
        }
    }
}