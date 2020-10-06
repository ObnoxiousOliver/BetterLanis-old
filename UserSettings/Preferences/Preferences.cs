using Newtonsoft.Json;
using System;
using System.IO;

namespace BetterLanis.UserSettings
{
    [Serializable]
    public class Preferences
    {
        public string ThemePreset { get; set; } = "Dark";
        public bool RememberLoginData { get; set; } = false;
        public string SelectedLocal { get; set; } = "en_UK.json";

        public void SavePrefrences()
        {
            File.WriteAllText(DataPaths.ConfigPath + @"config.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}