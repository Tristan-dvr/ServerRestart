using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace ServerRestart
{
    [BepInProcess("valheim_server.exe")]
    [BepInDependency("org.bepinex.plugins.servercharacters", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(Guid, Name, Version)]
    internal class Plugin : BaseUnityPlugin
    {
        public const string Guid = "org.tristan.serverrestart";
        public const string Name = "Server Restart";
        public const string Version = "1.1.2";

        public static ConfigEntry<string> RestartTimes;
        public static ConfigEntry<string> Message1Hour;
        public static ConfigEntry<string> Message30Mins;
        public static ConfigEntry<string> Message10Mins;
        public static ConfigEntry<string> Message5Min;
        public static ConfigEntry<string> Message4Min;
        public static ConfigEntry<string> Message3Min;
        public static ConfigEntry<string> Message2Min;
        public static ConfigEntry<string> Message1Min;
        public static ConfigEntry<string> AnounceFormat;

        public static ConfigEntry<string> ChatName;
        public static ConfigEntry<bool> SendMessagesToChat;
        public static ConfigEntry<string> ChatFormat;

        public static ConfigEntry<string> DiscordUrl;
        public static ConfigEntry<string> DiscordName;
        public static ConfigEntry<string> DiscordFormat;

        public static ConfigEntry<bool> ShutDownServer;
        public static ConfigEntry<bool> EnableMaintenance;
        public static ConfigEntry<int> MaintenanceMinutes;

        public static ConfigEntry<bool> PrintLogs;
        public static ConfigEntry<int> PrintLogsPeriod;

        private static RestartService _service;

        private void Awake()
        {
            Log.CreateInstance(Logger);

            RestartTimes = Config.Bind("1. Restart", "Schedule (utc)", "23:00:00,11:00:00", "Restart times divied by ,");
            ShutDownServer = Config.Bind("1. Restart", "Shut down", true, "Should plugin shut down server process. Disable if you use hosting restart schedule or another plugin");

            Message1Hour = Config.Bind("2. Messages", "1 hour", "Server restart in 1 hour");
            Message30Mins = Config.Bind("2. Messages", "30 minutes", "Server restart in 30 minutes");
            Message10Mins = Config.Bind("2. Messages", "10 minutes", "Server restart in 10 minutes");
            Message5Min = Config.Bind("2. Messages", "5 minutes", "Server restart in 5 minutes");
            Message4Min = Config.Bind("2. Messages", "4 minutes", "Server restart in 4 minutes");
            Message3Min = Config.Bind("2. Messages", "3 minutes", "Server restart in 3 minutes");
            Message2Min = Config.Bind("2. Messages", "2 minutes", "Server restart in 2 minutes");
            Message1Min = Config.Bind("2. Messages", "1 minute", "Server restart in 1 minute");
            AnounceFormat = Config.Bind("2. Messages", "Anounce format", "{0}", "Format of center screen message");

            ChatName = Config.Bind("2. Messages", "Chat name", "Restart");
            SendMessagesToChat = Config.Bind("2. Messages", "Send to chat", true);
            ChatFormat = Config.Bind("2. Messages", "Chat format", "{0}", "Format of chat message");

            EnableMaintenance = Config.Bind("3. ServerCharacters", "Maintenance mode", false, "Should enable maintenance mode for ServerCharacters");
            MaintenanceMinutes = Config.Bind("3. ServerCharacters", "Maintenance time", 7, "Enable maintenance before scheduled server restart time");

            DiscordUrl = Config.Bind("4. Discord", "Webhook", "");
            DiscordName = Config.Bind("4. Discord", "Display name", "Restart");
            DiscordFormat = Config.Bind("4. Discord", "Discord format", "{0}", "Format of discord message");

            PrintLogs = Config.Bind("5. Logs", "Print logs", false);
            PrintLogsPeriod = Config.Bind("5. Logs", "Period", 600, "Period for displaying date of next restart and remaining time");

            Helper.WatchConfigFileChanges(Config, OnConfigChanged);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Guid);
        }

        private void OnConfigChanged()
        {
            Log.Message("Config reloaded");
            Config.Reload();
            
            _service?.ScheduleNextRestart();
        }

        [HarmonyPatch]
        class Patch
        {
            [HarmonyPostfix, HarmonyPatch(typeof(Game), nameof(Game.Start))]
            private static void Game_Start(Game __instance)
            {
                _service = __instance.gameObject.AddComponent<RestartService>();
                
                __instance.gameObject.AddComponent<MaintenanceService>();
                __instance.gameObject.AddComponent<RestartMessages>();
                __instance.gameObject.AddComponent<RestartScheduleLogService>();
            }

            [HarmonyPrefix, HarmonyPatch(typeof(ZNet), nameof(ZNet.CheckForIncommingServerConnections))]
            private static bool ZNet_CheckForIncommingServerConnections()
            {
                if (_service.RestartStarted || (Game.instance != null && Game.instance.IsShuttingDown()))
                    return false;

                return true;
            }

            [HarmonyPriority(Priority.Last)]
            [HarmonyFinalizer, HarmonyPatch(typeof(Game), nameof(Game.OnApplicationQuit))]
            private static void Game_OnApplicationQuit()
            {
                Thread.Sleep(5000);
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
