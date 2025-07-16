using Exiled.API.Extensions;
using PlayerRoles;
using UnityEngine;
using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using MEC;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class Scp714 : CustomItem
    {
        private readonly List<Player> equippedPlayers = new();
        private readonly Dictionary<Player, CoroutineHandle> stamLimiters = new();
        private readonly Dictionary<Player, List<(EffectType, float)>> existingEffects = new();
        private List<RoleTypeId> _scp714Roles;
        private List<EffectType> _scp714Effects;
        private List<EffectType> _preventedEffects;
        private string _takeOffMessage;
        private string _putOnMessage;
        private float _scp049Damage;
        private float _pocketDimensionModifier;
        private float _stamLimitModifier;

        public override string Id => "SCP-714";
        public override string Name => "SCP-714";
        public override ItemType BaseType => ItemType.Coin;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.Scp714;
            _scp714Roles = config.Scp714Roles;
            _scp714Effects = config.Scp714Effects;
            _preventedEffects = config.PreventedEffects;
            _takeOffMessage = config.TakeOffMessage;
            _putOnMessage = config.PutOnMessage;
            _scp049Damage = config.Scp049Damage;
            _pocketDimensionModifier = config.PocketDimensionModifier;
            _stamLimitModifier = config.StamLimitModifier;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnInRoom(RoomType.Hcz049, Vector3.zero);
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Scp049.Attacking += OnAttacking;
            Exiled.Events.Handlers.Player.FlippingCoin += OnFlippingCoin;
            Exiled.Events.Handlers.Player.ReceivingEffect += OnReceivingEffect;
            Exiled.Events.Handlers.Player.DroppingItem += OnDropping;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Scp049.Attacking -= OnAttacking;
            Exiled.Events.Handlers.Player.FlippingCoin -= OnFlippingCoin;
            Exiled.Events.Handlers.Player.ReceivingEffect -= OnReceivingEffect;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDropping;
        }

        private void OnDropping(DroppingItemEventArgs ev)
        {
            if (equippedPlayers.Contains(ev.Player))
                SetRingState(ev.Player, false);
        }

        private void OnFlippingCoin(FlippingCoinEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            SetRingState(ev.Player, !equippedPlayers.Contains(ev.Player));
        }

        private void OnAttacking(AttackingEventArgs ev)
        {
            if (equippedPlayers.Contains(ev.Player) && _scp714Roles.Contains(ev.Player.Role.Type))
            {
                if (ev.Target != null)
                {
                    ev.IsAllowed = false;
                    ev.Target.Hurt(_scp049Damage);
                }
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (!equippedPlayers.Contains(ev.Player))
                return;

            if (ev.Attacker != null && _scp714Roles.Contains(ev.Attacker.Role))
                ev.IsAllowed = false;

            if (ev.DamageHandler.Type == DamageType.PocketDimension)
                ev.Amount *= _pocketDimensionModifier;
        }

        private void OnReceivingEffect(ReceivingEffectEventArgs ev)
        {
            if (equippedPlayers.Contains(ev.Player) && _preventedEffects.Contains(ev.Effect.GetEffectType()))
                ev.IsAllowed = false;
        }

        private void SetRingState(Player player, bool equipped)
        {
            if (equipped)
            {
                var activeEffects = ListPool<(EffectType, float)>.Pool.Get();
                foreach (var active in player.ActiveEffects)
                    activeEffects.Add((active.GetEffectType(), active.TimeLeft));

                existingEffects[player] = activeEffects;
                ListPool<(EffectType, float)>.Pool.Return(activeEffects);
                foreach (var effect in _scp714Effects)
                    player.EnableEffect(effect);
                stamLimiters[player] = Timing.RunCoroutine(LimitStamina(player));
                equippedPlayers.Add(player);
                player.ShowHint(_putOnMessage);
            }
            else
            {
                foreach (var effect in _scp714Effects)
                    player.DisableEffect(effect);
                equippedPlayers.Remove(player);
                player.ShowHint(_takeOffMessage);
                Timing.KillCoroutines(stamLimiters[player]);
                stamLimiters.Remove(player);
                if (existingEffects.TryGetValue(player, out var existingEffect))
                {
                    foreach (var (type, dur) in existingEffect)
                        player.EnableEffect(type, dur);
                    existingEffects.Remove(player);
                }
            }
        }

        private IEnumerator<float> LimitStamina(Player player)
        {
            while (equippedPlayers.Contains(player))
            {
                if (player.Stamina > player.StaminaStat.MaxValue * _stamLimitModifier)
                    player.Stamina = player.StaminaStat.MaxValue * _stamLimitModifier;

                yield return Timing.WaitForSeconds(0.15f);
            }
        }
    }
}