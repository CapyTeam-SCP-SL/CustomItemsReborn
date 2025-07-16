using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pools;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using CustomItemsReborn.API;
using Exiled.API.Extensions;
using LabApi.Loader.Features.Plugins;
using MEC;

namespace CustomItemsReborn.Items
{
    public class AntiMemeticPills : CustomItem
    {
        private readonly List<Player> _protected = ListPool<Player>.Pool.Get();

        private float _amnesiaVisionDuration;

        public override string Id => "AM-119";
        public override string Name => "Anti-Memetic Pills";
        public override ItemType BaseType => ItemType.SCP500;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.AntiMemeticPills;
            _amnesiaVisionDuration = config.AmnesiaVisionDuration;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnInRoom(RoomType.LczToilets, Vector3.zero);
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem += OnUsedItem;
            Exiled.Events.Handlers.Player.ReceivingEffect += OnReceivingEffect;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsedItem -= OnUsedItem;
            Exiled.Events.Handlers.Player.ReceivingEffect -= OnReceivingEffect;
            ListPool<Player>.Pool.Return(_protected);
        }

        private void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!Check(ev.Item) || !ev.Player.IsAlive)
                return;

            _protected.Add(ev.Player);
            ev.Player.RemoveItem(ev.Item);
            Timing.CallDelayed(_amnesiaVisionDuration, () => _protected.Remove(ev.Player));
        }

        private void OnReceivingEffect(ReceivingEffectEventArgs ev)
        {
            if (_protected.Contains(ev.Player) && ev.Effect.GetEffectType() == EffectType.AmnesiaVision)
                ev.IsAllowed = false;
        }
    }
}