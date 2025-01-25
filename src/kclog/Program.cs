using Indieteur.GlobalHooks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

// See https://github.com/Indieteur/GlobalHooks?tab=readme-ov-file for information on how the hook works
namespace kclog
{
    class Program
    {
        static GlobalKeyHook globalKeyHook;
        static GlobalMouseHook globalMouseHook;
        private readonly static ConcurrentQueue<string> keyQueue = new();
        private static Timer logTimer;
        private static string DbPath;

        static void Main()
        {
            globalKeyHook = new GlobalKeyHook();
            globalMouseHook = new GlobalMouseHook();
            globalKeyHook.OnKeyUp += GlobalKeyHook_OnKeyUp;
            globalMouseHook.OnButtonUp += GlobalMouseHook_OnButtonUp;
            globalMouseHook.OnMouseWheelScroll += GlobalMouseHook_OnMouseWheelScroll;

            Task.Run(RealMain);
            MessagePump.WaitForMessages();
        }

        static async Task RealMain()
        {
            var builder = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json", false, true);
            var app = builder.Build();
            DbPath = app["DbPath"];

            await InitializeDatabase();

            logTimer = new Timer(Convert.ToInt32(app["PulseTime"]));
            logTimer.Elapsed += ProcessKeyQueue;
            logTimer.AutoReset = true;
            logTimer.Enabled = true;

            ProcessKeyQueue(null, null);
        }

        // async void is generally a bad idea. Make sure we handle all exceptions
        private static async void ProcessKeyQueue(object sender, ElapsedEventArgs e)
        {
            var keys = new List<string>();
            while (keyQueue.TryDequeue(out string key))
            {
                keys.Add(key);
            }

            Shuffle(keys);

            await LogKeyPressToDatabase(keys);
        }

        private static void GlobalMouseHook_OnMouseWheelScroll(object sender, GlobalMouseEventArgs e)
        {
            //Console.WriteLine(e.wheelRotation);
        }

        private static void GlobalMouseHook_OnButtonUp(object sender, GlobalMouseEventArgs e)
        {
            keyQueue.Enqueue(e.Button.ToString());
        }

        private static void GlobalKeyHook_OnKeyUp(object sender, GlobalKeyEventArgs e)
        {
            var key = e.CharResult;
            key = GetKeyDescription(e.KeyCode, key);            
            keyQueue.Enqueue(key);
        }

        private static string GetKeyDescription(VirtualKeycodes virtualKeycode, string key)
        {
            key = virtualKeycode == VirtualKeycodes.LeftShift ? "L Shift" : key;
            key = virtualKeycode == VirtualKeycodes.RightShift ? "R Shift" : key;
            key = virtualKeycode == VirtualKeycodes.LeftAlt ? "L Alt" : key;
            key = virtualKeycode == VirtualKeycodes.RightAlt ? "R Alt" : key;
            key = virtualKeycode == VirtualKeycodes.LeftCtrl ? "L Ctrl" : key;
            key = virtualKeycode == VirtualKeycodes.RightCtrl ? "R Ctrl" : key;
            key = virtualKeycode == VirtualKeycodes.Esc ? "ESC" : key;
            key = virtualKeycode == VirtualKeycodes.Space ? "Space" : key;
            key = virtualKeycode == VirtualKeycodes.Capslock ? "Capslock" : key;
            key = virtualKeycode == VirtualKeycodes.Backspace ? "Backspace" : key;
            key = virtualKeycode == VirtualKeycodes.Insert ? "Insert" : key;
            key = virtualKeycode == VirtualKeycodes.Home ? "Home" : key;
            key = virtualKeycode == VirtualKeycodes.PageUp ? "PageUp" : key;
            key = virtualKeycode == VirtualKeycodes.Delete ? "Delete" : key;
            key = virtualKeycode == VirtualKeycodes.End ? "End" : key;
            key = virtualKeycode == VirtualKeycodes.PageDown ? "PageDown" : key;
            key = virtualKeycode == VirtualKeycodes.DownArrow ? "DownArrow" : key;
            key = virtualKeycode == VirtualKeycodes.UpArrow ? "UpArrow" : key;
            key = virtualKeycode == VirtualKeycodes.LeftArrow ? "LeftArrow" : key;
            key = virtualKeycode == VirtualKeycodes.RightArrow ? "RightArrow" : key;
            key = virtualKeycode == VirtualKeycodes.Numlock ? "Numlock" : key;
            key = virtualKeycode == VirtualKeycodes.F1 ? "F1" : key;
            key = virtualKeycode == VirtualKeycodes.F2 ? "F2" : key;
            key = virtualKeycode == VirtualKeycodes.F3 ? "F3" : key;
            key = virtualKeycode == VirtualKeycodes.F4 ? "F4" : key;
            key = virtualKeycode == VirtualKeycodes.F5 ? "F5" : key; 
            key = virtualKeycode == VirtualKeycodes.F6 ? "F6" : key;
            key = virtualKeycode == VirtualKeycodes.F7 ? "F7" : key;
            key = virtualKeycode == VirtualKeycodes.F8 ? "F8" : key;
            key = virtualKeycode == VirtualKeycodes.F9 ? "F9" : key;
            key = virtualKeycode == VirtualKeycodes.F10 ? "F10" : key;
            key = virtualKeycode == VirtualKeycodes.F11 ? "F11" : key;
            key = virtualKeycode == VirtualKeycodes.F12 ? "F12" : key;
            key = virtualKeycode == VirtualKeycodes.ScrollLock ? "ScrollLock" : key;
            key = virtualKeycode == VirtualKeycodes.PrintScreen ? "PrintScreen" : key;
            key = virtualKeycode == VirtualKeycodes.LeftWin ? "L Windows" : key;
            key = virtualKeycode == VirtualKeycodes.RightWin ? "R Windows" : key;
            key = virtualKeycode == VirtualKeycodes.Pause ? "Pause" : key;
            key = virtualKeycode == VirtualKeycodes.Tab ? "Tab" : key;
            return key;
        }

        private static async Task InitializeDatabase()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;");
            await connection.OpenAsync();

            var keysTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Keys (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Key TEXT NOT NULL,
                            Timestamp DATETIME NOT NULL
                        )";
            using var keysCommand = new SQLiteCommand(keysTableQuery, connection);
            await keysCommand.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        private static async Task LogKeyPressToDatabase(IList<string> keys)
        {
            using var connection = new SQLiteConnection($"Data Source={DbPath};Version=3;");
            await connection.OpenAsync();
            var insertQuery = "INSERT INTO Keys (Key, Timestamp) VALUES (@key, @timestamp)";
            using var insertCmd = new SQLiteCommand(insertQuery, connection);
            var now = DateTime.Now;
            var timestamp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            foreach (var key in keys)
            {
                insertCmd.Parameters.AddWithValue("@key", key);
                insertCmd.Parameters.AddWithValue("@timestamp", timestamp);
                await insertCmd.ExecuteNonQueryAsync();
            }
            await connection.CloseAsync();
        }

        static void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            var n = list.Count;

            while (n > 1)
            {
                var k = rng.Next(n);
                n--;
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}