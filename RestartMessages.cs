using DiscordTools;
using System;
using System.Collections;
using UnityEngine;

namespace ServerRestart
{
    public class RestartMessages : MonoBehaviour
    {
        private static UserInfo User = new UserInfo
        {
            Gamertag = "",
            NetworkUserId = "Steam_0",
        };

        public static event Action<string> OnMessageSent;

        private void OnEnable()
        {
            RestartService.OnScheduledRestartChanged += ScheduleRestartMessages;
        }

        private void OnDisable()
        {
            RestartService.OnScheduledRestartChanged -= ScheduleRestartMessages;
        }

        private void ScheduleRestartMessages(DateTime date)
        {
            StopAllCoroutines();
            if (date == default) return;

            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromHours(1)), Plugin.Message1Hour.Value));
            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromMinutes(30)), Plugin.Message30Mins.Value));
            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromMinutes(10)), Plugin.Message10Mins.Value));
            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromMinutes(5)), Plugin.Message5Min.Value));
            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromMinutes(4)), Plugin.Message4Min.Value));
            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromMinutes(3)), Plugin.Message3Min.Value));
            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromMinutes(2)), Plugin.Message2Min.Value));
            StartCoroutine(ScheduleMessage(date.Subtract(TimeSpan.FromMinutes(1)), Plugin.Message1Min.Value));
        }

        private IEnumerator ScheduleMessage(DateTime date, string message)
        {
            if (DateTime.UtcNow > date || string.IsNullOrEmpty(message)) yield break;

            Log.Debug($"Schedule message '{message}' at {date}");
            yield return new WaitUntil(() => DateTime.UtcNow >= date);
            Log.Debug($"Sending message '{message}'");

            SendMessageToAll(message);
            SendMessageToDiscord(message);
            SendMessageToChat(message);

            OnMessageSent?.Invoke(message);
        }

        private void SendMessageToAll(string message)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ShowMessage", new object[]
            {
                (int)MessageHud.MessageType.Center,
                string.Format(Plugin.AnounceFormat.Value, message),
            });
        }

        private void SendMessageToDiscord(string message)
        {
            var webhookUrl = Plugin.DiscordUrl.Value;
            if (string.IsNullOrEmpty(Plugin.DiscordUrl.Value)) return;

            var displayName = Plugin.DiscordName.Value;
            ThreadingUtil.RunThread(() =>
            {
                DiscordTool.SendMessageToDiscord(webhookUrl, displayName, string.Format(Plugin.DiscordFormat.Value, message));
            });
        }

        private void SendMessageToChat(string message)
        {
            if (!Plugin.SendMessagesToChat.Value) return;

            User.Name = Plugin.ChatName.Value;
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,
                "ChatMessage",
                new Vector3(0f, 200f, 0f),
                (int)Talker.Type.Shout,
                User,
                string.Format(Plugin.ChatFormat.Value, message),
                User.NetworkUserId);
        }
    } 
}
