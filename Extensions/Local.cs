using BetterLanis.UserSettings;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BetterLanis.Extensions
{
    public class Local
    {
		public static void SetLocalStatic(object sender)
		{
			var type = sender.GetType();
			if (type == typeof(TextBlock))
			{
				var textBlock = (TextBlock)sender;
				var values = MainWindow.GetTagValues((string)textBlock.Tag);
				var localid = values["localid"];
			
				var content = GetLocalText(localid, values);
				if (content == null) return;
			
				textBlock.Text = content;
			
				if (values.ContainsKey("rawtext")) values["rawtext"] = content;
				else values.Add("rawtext", content);
			
				textBlock.Tag = JsonConvert.SerializeObject(values).Replace("{", "").Replace("}", "").Trim();
			}
			else if (type == typeof(Button))
			{
				var button = (Button)sender;
				var values = MainWindow.GetTagValues((string)button.Tag);
				var localid = values["localid"];
			
				var content = GetLocalText(localid, values);
				if (content == null) return;
			
				button.Content = content;
			
				if (values.ContainsKey("rawtext")) values["rawtext"] = content;
				else values.Add("rawtext", content);
			
				button.Tag = JsonConvert.SerializeObject(values).Replace("{", "").Replace("}", "").Trim();
			}
		}

		public static string GetLocalText(string localid, Dictionary<string, string> values)
		{
			var selectedLocal = "en_UK.json";
			if (File.Exists(DataPaths.LocalsPath + MainWindow.Config.SelectedLocal))
				selectedLocal = MainWindow.Config.SelectedLocal;
			else if (!File.Exists(DataPaths.LocalsPath + selectedLocal)) return null;
			var localDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(DataPaths.LocalsPath + selectedLocal));

			string addString = "";

			if (!localDic.ContainsKey(localid))
				return "";
			if (values.ContainsKey("addString"))
			{
				addString = values["addString"];
				if (selectedLocal.StartsWith("en_") && addString == "(Language)")
					addString = "";
			}
			return localDic[localid] + addString;
		}
	}
}