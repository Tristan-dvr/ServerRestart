using System;
using UnityEngine;

namespace ServerRestart
{
    class RestartScheduleLogService : MonoBehaviour
    {
        private DateTime _nextRestartDate;

        private void OnEnable()
        {
            RestartService.OnScheduledRestartChanged += ScheduleLogMessages;
        }

        private void OnDisable()
        {
            RestartService.OnScheduledRestartChanged -= ScheduleLogMessages;
        }

        private void ScheduleLogMessages(DateTime time)
        {
            _nextRestartDate = time;
            PrintLogMessage();

            CancelInvoke();

            if (!Plugin.PrintLogs.Value) return;

            var repeatTime = Plugin.PrintLogsPeriod.Value;
            InvokeRepeating(nameof(PrintLogMessage), repeatTime, repeatTime);
        }

        private void PrintLogMessage()
        {
            var currentTime = DateTime.UtcNow;
            var timeLeft = _nextRestartDate - currentTime;
            if (_nextRestartDate != default)
                Log.Message($"Next restart {_nextRestartDate}. Time left: {timeLeft}");
            else
                Log.Message("No scheduled restarts");
        }
    }
}
