using Exiled.API.Features.Pickups;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using PlayerStatsSystem;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class SniperRifle : CustomItem
    {
        private float _damageMultiplier;
        private byte _clipSize;

        public override string Id => "SR-119";
        public override string Name => "Sniper Rifle";
        public override ItemType BaseType => ItemType.GunE11SR;

        public AttachmentName[] Attachments { get; } = new[]
        {
            AttachmentName.ExtendedBarrel,
            AttachmentName.ScopeSight,
        };

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.SniperRifle;
            _damageMultiplier = config.DamageMultiplier;
            _clipSize = config.ClipSize;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnMultipleInRoom(RoomType.HczHid, new[] { new Vector3(0, 1, 0) });
            SpawnMultipleInRoom(RoomType.HczArmory, new[] { new Vector3(0, 1, 0) });

            foreach (var pickup in Pickup.List)
            {
                if (pickup.Type == BaseType && Check(pickup))
                {
                    if (Item.Get(pickup.Serial) is Firearm firearm)
                    {
                        foreach (var attachment in Attachments)
                            firearm.AddAttachment(attachment);
                        firearm.MagazineAmmo = _clipSize;
                    }
                }
            }
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker == ev.Player || ev.DamageHandler.Base is not FirearmDamageHandler firearmDamageHandler)
                return;

            if (!Check(ev.Attacker.CurrentItem) || firearmDamageHandler.WeaponType != BaseType)
                return;

            ev.Amount *= _damageMultiplier;
        }
    }
}