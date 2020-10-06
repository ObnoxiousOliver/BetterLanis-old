using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterLanis.UserSettings
{
    class DataPaths
    {
		public static string LocalAppdataPath { get; set; }

		public static string DefaultLocalsPath { get; set; }
		public static string DefaultThemesPath { get; set; }

		public static string VersionPath { get; set; }
		public static string LocalsPath { get; set; }
		public static string ThemesPath { get; set; }
		public static string ConfigPath { get; set; }
		public static string UsersDataPath { get; set; }

		public static void CreateDataPaths()
        {
			LocalAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\BetterLanis\";
			ConfigPath = LocalAppdataPath + @"Config\";
			UsersDataPath = LocalAppdataPath + @"Data\";

			DefaultLocalsPath = AppDomain.CurrentDomain.BaseDirectory + @"BuildInLocals\";
			DefaultThemesPath = AppDomain.CurrentDomain.BaseDirectory + @"BuildInThemes\";
			VersionPath = AppDomain.CurrentDomain.BaseDirectory + @"Version\";
			LocalsPath = LocalAppdataPath + @"Locals\";
			ThemesPath = LocalAppdataPath + @"Themes\";

			Directory.CreateDirectory(LocalAppdataPath);
			Directory.CreateDirectory(ConfigPath);
			Directory.CreateDirectory(UsersDataPath);
			Directory.CreateDirectory(LocalsPath);
			Directory.CreateDirectory(ThemesPath);
		}
	}
}