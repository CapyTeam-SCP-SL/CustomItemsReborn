using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using UnityEngine;
using CustomItemsReborn.API;

namespace CustomItemsReborn.Items
{
    public class Scp2818 : CustomItem
    {
        private const float ProjectileStartOffsetX = 0.0715f;
        private const float ProjectileStartOffsetY = 0.0225f;
        private const float ProjectileStartOffsetZ = 0.45f;
        private const float ProjectileScaleFactor = 0.15f;
        private const float NormalScaleFactor = 1f;
        private const float ScaleResetDelay = 0.01f;
        private const float MaxTravelDistanceSqr = 1000f;
        private const float MinDistanceToTarget = 0.5f;

        private float _tickFrequency;
        private float _maxDistancePerTick;
        private bool _despawnAfterUse;
        private float _damage;

        public override string Id => "SCP-2818";
        public override string Name => "SCP-2818";
        public override ItemType BaseType => ItemType.GunE11SR;

        public override void Initialize()
        {
            var config = CustomItems.Instance.Config.ItemConfigs.Scp2818;
            _tickFrequency = config.TickFrequency;
            _maxDistancePerTick = config.MaxDistancePerTick;
            _despawnAfterUse = config.DespawnAfterUse;
            _damage = config.Damage;
            PickupBroadcast = config.PickupBroadcast;
            ChangeHint = config.ChangeHint;

            SpawnMultipleInRoom(RoomType.HczHid, new[] { Vector3.zero });
            SpawnMultipleInRoom(RoomType.HczArmory, new[] { Vector3.zero });
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting += OnShoot;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShoot;
        }

        private void OnShoot(ShootingEventArgs ev)
        {
            if (!Check(ev.Player?.CurrentItem) || !ev.Player.IsAlive)
                return;

            try
            {
                RemoveScp2818Items(ev.Player);
                Vector3 direction = ev.Direction;

                if (!IsValidDirection(direction, ev.Player.Position))
                {
                    KillPlayer(ev.Player, "Invalid direction or excessive distance.");
                    ev.IsAllowed = false;
                    return;
                }

                Timing.RunCoroutine(LaunchProjectile(ev.Player, direction));
                ev.IsAllowed = false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnShoot for SCP-2818: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RemoveScp2818Items(Player player)
        {
            foreach (var item in player.Items.Where(item => Check(item)).ToList())
            {
                player.RemoveItem(item);
            }
        }

        private bool IsValidDirection(Vector3 direction, Vector3 playerPos)
        {
            return direction != Vector3.zero && (playerPos - direction).sqrMagnitude <= MaxTravelDistanceSqr;
        }

        private void KillPlayer(Player player, string reason)
        {
            player.Hurt(new UniversalDamageHandler(_damage, DeathTranslations.Warhead));
            Log.Debug($"Player {player.Nickname} killed due to: {reason}");
        }

        private IEnumerator<float> LaunchProjectile(Player player, Vector3 targetPos)
        {
            if (player == null || !player.IsAlive)
                yield break;

            RoleTypeId originalRole = player.Role;
            Transform camera = player.CameraTransform;

            player.Position = camera.TransformPoint(new Vector3(ProjectileStartOffsetX, ProjectileStartOffsetY, ProjectileStartOffsetZ));
            player.Scale = new Vector3(ProjectileScaleFactor, ProjectileScaleFactor, ProjectileScaleFactor);

            while (Vector3.Distance(player.Position, targetPos) > MinDistanceToTarget && player.IsAlive)
            {
                if (player.Role != originalRole)
                {
                    Log.Debug($"Projectile aborted for {player.Nickname}: Role changed from {originalRole}.");
                    break;
                }

                player.Position = Vector3.MoveTowards(player.Position, targetPos, _maxDistancePerTick);
                yield return Timing.WaitForSeconds(_tickFrequency);
            }

            if (player.IsAlive)
            {
                player.Scale = new Vector3(NormalScaleFactor, NormalScaleFactor, NormalScaleFactor);
                yield return Timing.WaitForSeconds(ScaleResetDelay);
            }

            if (_despawnAfterUse && player.IsAlive)
            {
                RemoveScp2818Items(player);
            }

            if (player.IsAlive && player.Role != RoleTypeId.Spectator)
            {
                KillPlayer(player, "Projectile impact.");
            }
        }
    }
}