using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace BetterLanis.UserSettings.ThemeResources
{
    class ThemeResources
    {
        public static void SetResources(ThemePreset theme)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            foreach (var valueRawName in typeof(ThemePreset).GetFields(bindingFlags).Select(field => field.Name).ToList())
            {
                var valueName = valueRawName.Remove(valueRawName.IndexOf(">")).Replace("<", "");

                var value = (string)typeof(ThemePreset).GetProperty(valueName).GetValue(theme, null);


                if (Application.Current.Resources.Contains(valueName))
                {
                    Application.Current.Resources[valueName] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
                }
            }
        }

        public static void SetResource(string name, string hexValue)
        {
            if (Application.Current.Resources.Contains(name))
            {
                if(Application.Current.Resources[name].GetType() == typeof(SolidColorBrush))
                {
                    try
                    {
                        var color = (Color)ColorConverter.ConvertFromString(hexValue);
                        Application.Current.Resources[name] = new SolidColorBrush(color);
                    } catch { }
                }
            }
        }
        public static string GetResource(string name)
        {
            if (Application.Current.Resources.Contains(name))
                if (Application.Current.Resources[name].GetType() == typeof(SolidColorBrush))
                    return ColorE.HexConverter(((SolidColorBrush)Application.Current.Resources[name]).Color);
            return null;
        }
    }
}