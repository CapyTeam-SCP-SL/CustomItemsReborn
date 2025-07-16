using CustomPlayerEffects;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Scp939;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class TranquilizerGun : CustomItem
    {
        private readonly Dictionary<Player, float> tranquilizedPlayers = new();
        private readonly HashSet<Player> activeTranqs = new();
        private bool _resistantScps;
        private float _duration;
        private float _resistanceModifier;
        private float _resistanceFalloffDelay;
        private bool _dropItems;
        private int _scpResistChance;
        private byte _clipSize;

        public override string Id => "TG-119";
        public override string Name => "Tranquilizer Gun";
        public override ItemType BaseType => ItemType.GunCOM18;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.TranquilizerGun;
            _resistantScps = config.ResistantScps;
            _duration = config.Duration;
            _resistanceModifier = config.ResistanceModifier;
            _resistanceFalloffDelay = config.ResistanceFalloffDelay;
            _dropItems = config.DropItems;
            _scpResistChance = config.ScpResistChance;
            _clipSize = config.ClipSize;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnInRoom(RoomType.Lcz173, new Vector3(0, 1, 0));
            SpawnInRoom(RoomType.LczGlassBox, new Vector3(0, 1, 0));
        }

        protected override void SubscribeEvents()
        {
            Timing.RunCoroutine(ReduceResistances(), $"{nameof(TranquilizerGun)}-{Id}-reducer");
            Exiled.Events.Handlers.Player.PickingUpItem += OnDeniableEvent;
            Exiled.Events.Handlers.Player.ChangingItem += OnDeniableEvent;
            Exiled.Events.Handlers.Scp049.StartingRecall += OnDeniableEvent;
            Exiled.Events.Handlers.Scp106.Teleporting += OnDeniableEvent;
            Exiled.Events.Handlers.Scp096.Charging += OnDeniableEvent;
            Exiled.Events.Handlers.Scp096.Enraging += OnDeniableEvent;
            Exiled.Events.Handlers.Scp096.AddingTarget += OnDeniableEvent;
            Exiled.Events.Handlers.Scp939.PlacingAmnesticCloud += OnDeniableEvent;
            Exiled.Events.Handlers.Player.VoiceChatting += OnDeniableEvent;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.PickingUpItem -= OnDeniableEvent;
            Exiled.Events.Handlers.Player.ChangingItem -= OnDeniableEvent;
            Exiled.Events.Handlers.Scp049.StartingRecall -= OnDeniableEvent;
            Exiled.Events.Handlers.Scp106.Teleporting -= OnDeniableEvent;
            Exiled.Events.Handlers.Scp096.Charging -= OnDeniableEvent;
            Exiled.Events.Handlers.Scp096.Enraging -= OnDeniableEvent;
            Exiled.Events.Handlers.Scp096.AddingTarget -= OnDeniableEvent;
            Exiled.Events.Handlers.Scp939.PlacingAmnesticCloud -= OnDeniableEvent;
            Exiled.Events.Handlers.Player.VoiceChatting -= OnDeniableEvent;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            activeTranqs.Clear();
            tranquilizedPlayers.Clear();
            Timing.KillCoroutines($"{nameof(TranquilizerGun)}-{Id}-reducer");
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == ev.Player || !Check(ev.Attacker?.CurrentItem))
                return;

            if (ev.Player.Role.Team == Team.SCPs && _resistantScps)
            {
                int r = UnityEngine.Random.Range(1, 101);
                Log.Debug($"{Name}: SCP roll: {r} (must be greater than {_scpResistChance})");
                if (r <= _scpResistChance)
                {
                    Log.Debug($"{Name}: {r} is too low, no tranq.");
                    return;
                }
            }

            float duration = _duration;

            if (!tranquilizedPlayers.TryGetValue(ev.Player, out _))
                tranquilizedPlayers.Add(ev.Player, 1);

            tranquilizedPlayers[ev.Player] *= _resistanceModifier;
            Log.Debug($"{Name}: Resistance Duration Mod: {tranquilizedPlayers[ev.Player]}");

            duration -= tranquilizedPlayers[ev.Player];
            Log.Debug($"{Name}: Duration: {duration}");

            if (duration > 0f)
                Timing.RunCoroutine(DoTranquilize(ev.Player, duration));
        }

        private IEnumerator<float> DoTranquilize(Player player, float duration)
        {
            activeTranqs.Add(player);
            Vector3 oldPosition = player.Position;
            Item previousItem = player.CurrentItem;
            Vector3 previousScale = player.Scale;
            List<StatusEffectBase> activeEffects = ListPool<StatusEffectBase>.Pool.Get();
            player.CurrentItem = null;

            activeEffects.AddRange(player.ActiveEffects.Where(effect => effect.IsEnabled));

            try
            {
                if (_dropItems)
                {
                    foreach (Item item in player.Items.ToHashSet())
                    {
                        player.DropItem(item);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(DoTranquilize)}: {e}");
            }

            Ragdoll ragdoll = null;
            if (player.Role.Type != RoleTypeId.Scp106)
                ragdoll = Ragdoll.CreateAndSpawn(player.Role, player.DisplayNickname, "Tranquilized", player.Position, player.ReferenceHub.PlayerCameraReference.rotation, player);

            if (player.Role is Scp096Role scp)
                scp.RageManager.ServerEndEnrage();

            try
            {
                player.EnableEffect<Invisible>(duration);
                player.Scale = Vector3.one * 0.2f;
                player.IsGodModeEnabled = true;

                player.EnableEffect<AmnesiaVision>(duration);
                player.EnableEffect<AmnesiaItems>(duration);
                player.EnableEffect<Ensnared>(duration);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(DoTranquilize)}: {e}");
            }

            yield return Timing.WaitForSeconds(duration);

            try
            {
                if (ragdoll != null)
                    NetworkServer.Destroy(ragdoll.GameObject);

                if (player.GameObject == null)
                    yield break;

                player.IsGodModeEnabled = false;
                player.Scale = previousScale;

                if (!_dropItems)
                    player.CurrentItem = previousItem;

                foreach (StatusEffectBase effect in activeEffects.Where(effect => (effect.Duration - duration) > 0))
                    player.EnableEffect(effect, effect.Duration);

                activeTranqs.Remove(player);
                ListPool<StatusEffectBase>.Pool.Return(activeEffects);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(DoTranquilize)}: {e}");
            }

            if (Warhead.IsDetonated && player.Position.y < 900)
            {
                player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Warhead));
                yield break;
            }

            player.Position = oldPosition;
        }

        private IEnumerator<float> ReduceResistances()
        {
            while (true)
            {
                foreach (Player player in tranquilizedPlayers.Keys.ToList())
                    tranquilizedPlayers[player] = Mathf.Max(0, tranquilizedPlayers[player] / 2);

                yield return Timing.WaitForSeconds(_resistanceFalloffDelay);
            }
        }

        private void OnDeniableEvent(IDeniableEvent ev)
        {
            if (ev is IPlayerEvent eP && activeTranqs.Contains(eP.Player))
                ev.IsAllowed = false;
        }
    }
}