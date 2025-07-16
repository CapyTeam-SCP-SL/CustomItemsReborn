// -----------------------------------------------------------------------
// <copyright file="Commands.cs" company="CapyTeam SCP: SL">
// Copyright (c) CapyTeam SCP: SL. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using UnityEngine;
using CustomItemsReborn.API;
using CustomItemsReborn.Commands;
using Exiled.API.Enums;
using RemoteAdmin;

namespace CustomItemsReborn
{
    /// <summary>
    /// Base command for Remote Admin Console (RAC) commands related to CustomItemsReborn.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RciCommand : ICommand, IUsageProvider
    {
        public string Command => "rci";
        public string[] Aliases => new[] { "customitems" };
        public string Description => "Manages custom items and roles in the CustomItemsReborn plugin.";
        public string[] Usage => new[] { "list/info/spawn/give/role" };

        /// <summary>
        /// Dictionary mapping subcommand names to their implementations.
        /// </summary>
        private readonly Dictionary<string, BaseCommand> _subCommands = new()
        {
            { "list", new ListCommand() },
            { "info", new InfoCommand() },
            { "spawn", new SpawnCommand() },
            { "give", new GiveCommand() },
            { "role", new RoleCommand() }
        };

        /// <summary>
        /// Executes the rci command by delegating to the appropriate subcommand.
        /// </summary>
        /// <param name="arguments">The arguments provided with the command.</param>
        /// <param name="sender">The sender executing the command.</param>
        /// <param name="response">The response message to be returned.</param>
        /// <returns>True if the command executed successfully, false otherwise.</returns>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "<color=red>Please specify a subcommand: list, info, spawn, give, or role.</color>";
                return false;
            }

            string subCommand = arguments.At(0).ToLower();
            if (!_subCommands.TryGetValue(subCommand, out BaseCommand command))
            {
                response = "<color=red>Invalid subcommand. Available subcommands: list, info, spawn, give, role.</color>";
                return false;
            }

            return command.Execute(new ArraySegment<string>(arguments.Array, 1, arguments.Count - 1), sender, out response);
        }
    }

    /// <summary>
    /// Lists all installed and enabled custom items.
    /// </summary>
    internal class ListCommand : BaseCommand
    {
        public override string Command => "list";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Lists all installed and enabled custom items.";

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!CheckPermission(sender, "citems.list", out response))
                return false;

            if (!ValidateArguments(arguments, out response))
                return false;

            StringBuilder sb = new();
            sb.AppendLine("Installed Custom Items:");
            foreach (var item in CustomItem.All)
            {
                sb.AppendLine($"- {item.Name} (ID: {item.Id})");
            }

            response = BuildResponse(sb.ToString());
            return true;
        }
    }

    /// <summary>
    /// Displays detailed information about a specific custom item.
    /// </summary>
    internal class InfoCommand : BaseCommand
    {
        public override string Command => "info";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Displays detailed information about a specific custom item.";
        protected override int MinArguments => 1;
        protected override int MaxArguments => 1;
        protected override string Usage => "info <item name/id>";

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!CheckPermission(sender, "citems.info", out response))
                return false;

            if (!ValidateArguments(arguments, out response))
                return false;

            string itemIdentifier = arguments.At(0);
            CustomItem item = CustomItem.Get(itemIdentifier);

            if (item == null)
            {
                response = BuildResponse($"Item '{itemIdentifier}' not found.", false);
                return false;
            }

            StringBuilder sb = new();
            sb.AppendLine($"Item: {item.Name} (ID: {item.Id})");
            sb.AppendLine($"Base Type: {item.BaseType}");
            // Note: Description is not directly accessible in CustomItem; adjust if needed
            sb.AppendLine("Description: Custom item managed by CustomItemsReborn plugin.");

            response = BuildResponse(sb.ToString());
            return true;
        }
    }

    /// <summary>
    /// Spawns a custom item at a specified location or player's position.
    /// </summary>
    internal class SpawnCommand : BaseCommand
    {
        public override string Command => "spawn";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Spawns a custom item at a specified location or player's position.";
        protected override int MinArguments => 2;
        protected override int MaxArguments => 5;
        protected override string Usage => "spawn <item name/id> <location/player/coordinates>";

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!CheckPermission(sender, "citems.spawn", out response))
                return false;

            if (!ValidateArguments(arguments, out response))
                return false;

            string itemIdentifier = arguments.At(0);
            CustomItem item = CustomItem.Get(itemIdentifier);

            if (item == null)
            {
                response = BuildResponse($"Item '{itemIdentifier}' not found.", false);
                return false;
            }

            string location = arguments.At(1).ToLower();
            Vector3 position;

            // Check if the location is a predefined spawn point
            if (Enum.TryParse<RoomType>(location, true, out RoomType roomType))
            {
                position = Room.Get(roomType)?.Position ?? Vector3.zero;
            }
            // Check if the location is a player's name
            else if (Player.Get(location) is Player targetPlayer)
            {
                position = targetPlayer.Position;
            }
            // Check if the location is coordinates (x, y, z)
            else if (arguments.Count >= 4 &&
                     float.TryParse(arguments.At(1), out float x) &&
                     float.TryParse(arguments.At(2), out float y) &&
                     float.TryParse(arguments.At(3), out float z))
            {
                position = new Vector3(x, y, z);
            }
            else
            {
                response = BuildResponse("Invalid location. Use a valid RoomType, player name, or coordinates (x y z).", false);
                return false;
            }

            item.Spawn(position);
            response = BuildResponse($"Spawned {item.Name} at {location}.");
            return true;
        }
    }

    /// <summary>
    /// Gives a custom item to a specified player or the command issuer.
    /// </summary>
    internal class GiveCommand : BaseCommand
    {
        public override string Command => "give";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Gives a custom item to a specified player or the command issuer.";
        protected override int MinArguments => 1;
        protected override int MaxArguments => 2;
        protected override string Usage => "give <item name/id> [player]";

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!CheckPermission(sender, "citems.give", out response))
                return false;

            if (!ValidateArguments(arguments, out response))
                return false;

            string itemIdentifier = arguments.At(0);
            CustomItem item = CustomItem.Get(itemIdentifier);

            if (item == null)
            {
                response = BuildResponse($"Item '{itemIdentifier}' not found.", false);
                return false;
            }

            Player target = null;
            if (arguments.Count == 2)
            {
                target = Player.Get(arguments.At(1));
                if (target == null)
                {
                    response = BuildResponse($"Player '{arguments.At(1)}' not found.", false);
                    return false;
                }
            }
            else if (sender is PlayerCommandSender playerSender)
            {
                target = Player.Get(playerSender.SenderId);
            }

            if (target == null)
            {
                response = BuildResponse("No target player specified and sender is not a player.", false);
                return false;
            }

            item.Give(target);
            response = BuildResponse($"Gave {item.Name} to {target.Nickname}.");
            return true;
        }
    }

    /// <summary>
    /// Assigns a role to a specified player.
    /// </summary>
    internal class RoleCommand : BaseCommand
    {
        public override string Command => "role";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Assigns a role to a specified player.";
        protected override int MinArguments => 2;
        protected override int MaxArguments => 2;
        protected override string Usage => "role <player> <role>";
        protected override float CooldownSeconds => 3f;

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!CheckPermission(sender, "citems.role", out response))
                return false;

            if (!CheckCooldown(sender, out response))
                return false;

            if (!ValidateArguments(arguments, out response))
                return false;

            string playerIdentifier = arguments.At(0);
            Player target = Player.Get(playerIdentifier);

            if (target == null)
            {
                response = BuildResponse($"Player '{playerIdentifier}' not found.", false);
                return false;
            }

            string roleName = arguments.At(1);
            if (!Enum.TryParse<RoleTypeId>(roleName, true, out RoleTypeId roleType))
            {
                response = BuildResponse($"Invalid role '{roleName}'. Use a valid RoleTypeId (e.g., Scp173, Scientist).", false);
                return false;
            }

            target.Role.Set(roleType);
            response = BuildResponse($"Assigned role {roleName} to {target.Nickname}.");
            return true;
        }

        public override string[] GetSuggestions(ArraySegment<string> arguments)
        {
            if (arguments.Count == 1)
            {
                return Player.List.Select(p => p.Nickname).ToArray();
            }
            else if (arguments.Count == 2)
            {
                return Enum.GetNames(typeof(RoleTypeId));
            }
            return Array.Empty<string>();
        }
    }
}