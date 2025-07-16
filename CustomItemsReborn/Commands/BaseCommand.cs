using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace CustomItemsReborn.Commands;

    public abstract class BaseCommand : ICommand
    {
        public abstract string Command { get; }
        public abstract string[] Aliases { get; }
        public abstract string Description { get; }

        protected virtual int MinArguments
        {
            get => 0;
        }

        protected virtual int MaxArguments
        {
            get => int.MaxValue;
        }

        protected virtual float CooldownSeconds
        {
            get => 0f;
        }

        protected virtual string Usage => $"{Command} <arguments>";

        protected virtual IReadOnlyDictionary<string, string> LocalizedResponses
        {
            get => new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                {"NoPermission", "<color=red>Недостаточно прав.</color>"},
                {"InvalidArgs", "<color=red>Неверное количество аргументов. Использование: {0}</color>"},
                {"Cooldown", "<color=yellow>Команда на перезарядке. Подождите {0} секунд.</color>"},
                {"Success", "<color=green>Команда выполнена успешно.</color>"}
            });
        }

        private readonly ConcurrentDictionary<string, DateTime> _cooldowns =
            new ConcurrentDictionary<string, DateTime>();

        protected internal bool CheckPermission(ICommandSender sender, string permission, out string response)
        {
            if (string.IsNullOrEmpty(permission) || sender.CheckPermission(permission))
            {
                response = string.Empty;
                return true;
            }

            response = LocalizedResponses["NoPermission"];
            Log.Debug($"Пользователь {sender.LogName} попытался выполнить команду {Command} без прав {permission}.");
            return false;
        }

        protected internal bool ValidateArguments(ArraySegment<string> arguments, out string response,
            int? minArgs = null, int? maxArgs = null, string usage = null)
        {
            minArgs ??= MinArguments;
            maxArgs ??= MaxArguments;
            usage ??= Usage;

            if (arguments.Count < minArgs || arguments.Count > maxArgs)
            {
                response = string.Format(LocalizedResponses["InvalidArgs"], usage);
                Log.Debug(
                    $"Команда {Command} вызвана с неверным количеством аргументов. Требуется: {minArgs}-{maxArgs}, получено: {arguments.Count}");
                return false;
            }

            response = string.Empty;
            return true;
        }

        protected internal bool CheckCooldown(ICommandSender sender, out string response)
        {
            if (CooldownSeconds <= 0)
            {
                response = string.Empty;
                return true;
            }

            string senderId = sender is PlayerCommandSender player ? player.SenderId : sender.LogName;
            if (_cooldowns.TryGetValue(senderId, out DateTime lastUsed))
            {
                double secondsSinceLastUse = (DateTime.UtcNow - lastUsed).TotalSeconds;
                if (secondsSinceLastUse < CooldownSeconds)
                {
                    response = string.Format(LocalizedResponses["Cooldown"],
                        Math.Ceiling(CooldownSeconds - secondsSinceLastUse));
                    return false;
                }
            }

            _cooldowns[senderId] = DateTime.UtcNow;
            response = string.Empty;
            return true;
        }

        protected internal string SanitizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.Trim()
                .Replace("<", "＜")
                .Replace(">", "＞")
                .Replace("&", "＆")
                .Replace("\"", "＂")
                .Replace("'", "＇");
        }

        protected internal string BuildResponse(string message, bool success = true, string color = null)
        {
            string sanitized = SanitizeText(message);
            color ??= success ? "green" : "red";
            return $"<color={color}>{sanitized}</color>";
        }

        public virtual string[] GetSuggestions(ArraySegment<string> arguments)
        {
            return Array.Empty<string>();
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!CheckCooldown(sender, out response))
                return false;

            if (!ValidateArguments(arguments, out response))
                return false;

            try
            {
                return OnExecute(arguments, sender, out response);
            }
            catch (Exception ex)
            {
                response = BuildResponse($"Ошибка выполнения команды: {ex.Message}", false);
                Log.Error($"Ошибка в команде {Command}: {ex}");
                return false;
            }
        }

        protected abstract bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response);
    }