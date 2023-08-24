using BepInEx.Bootstrap;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ServerRestart
{
    class MaintenanceService : MonoBehaviour
    {
        private string _maintenanceFilePath;

        private void Awake()
        {
            var serverCharacters = Chainloader.PluginInfos.Values
                .FirstOrDefault(p => p.Metadata.GUID == "org.bepinex.plugins.servercharacters");

            if (serverCharacters == null) return;

            var serverCharactersFolder = Path.GetDirectoryName(serverCharacters.Location);
            Log.Debug($"ServerCharacters mod found at {serverCharactersFolder}");
            _maintenanceFilePath = Path.Combine(serverCharactersFolder, "maintenance");
        }

        private void OnEnable()
        {
            RestartService.OnScheduledRestartChanged += OnRestartDateChanged;
        }

        private void OnDisable()
        {
            RestartService.OnScheduledRestartChanged -= OnRestartDateChanged;
        }

        private void OnRestartDateChanged(DateTime date)
        {
            if (!Plugin.EnableMaintenance.Value) return;

            if (string.IsNullOrEmpty(_maintenanceFilePath))
            {
                Log.Error("Cannot enable maintenance. ServerCharacters mod not found!");
                return;
            }

            StopAllCoroutines();
            RemoveMaintenance();
            if (date == default) return;

            StartCoroutine(ScheduleMaintenance(date.Subtract(TimeSpan.FromMinutes(Plugin.MaintenanceMinutes.Value))));
        }

        private void RemoveMaintenance()
        {
            if (!File.Exists(_maintenanceFilePath)) return;

            File.Delete(_maintenanceFilePath);
            Log.Info("Maintenance disabled");
        }

        private IEnumerator ScheduleMaintenance(DateTime date)
        {
            yield return new WaitUntil(() => DateTime.UtcNow >= date);
            using (File.Create(_maintenanceFilePath)) { }
            Log.Info("Maintenance started");
        }
    }
}
