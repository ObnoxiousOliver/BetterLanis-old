using BetterLanis.EasterEggs;
using BetterLanis.Extensions;
using BetterLanis.Extensions.Animations;
using BetterLanis.Login.SchoolList;
using BetterLanis.UserContols;
using BetterLanis.UserContols.Timetable;
using BetterLanis.UserSettings;
using BetterLanis.UserSettings.News;
using BetterLanis.UserSettings.ThemeResources;
using BetterLanis.UserSettings.User;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace BetterLanis
{
    public partial class MainWindow : Window
	{
		public static MainWindow Instance { get; private set; }

		public static ChromeDriver Driver { get; set; }
		public static bool DriverIsHeadless { get; set; } = false;
		public static Preferences Config { get; set; }
		public static UserData LoggedInUser { get; set; }

		public MainWindow()
		{
			DataPaths.CreateDataPaths();
			InstallDefaultFiles();

			if (File.Exists(DataPaths.ConfigPath + @"config.json"))
			{
				try { Config = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(DataPaths.ConfigPath + @"config.json")); }
				catch { Config = new Preferences(); }
			} else Config = new Preferences();
			Config.SavePrefrences();

			InitializeComponent();
			Closed += App_Closed;
			PreviewKeyDown += App_PreviewKeyDown;
			
			HideTabs();
			LoadLocals();
			LoadThemes();

            CreateDriver().GetAwaiter();
			
			try
			{
				if (File.Exists(DataPaths.UsersDataPath + "login.lcfg"))
				{
					var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(
						CryptoData.Decrypt(File.ReadAllText(DataPaths.UsersDataPath + "login.lcfg")));
			
					if (bool.Parse(dic["autoLogin"]))
						AutoLogin(dic["user"]);
				}
            }
            catch { }
			
			CheckNews();

			Instance = this;
		}

		public void HideTabs()
		{
			var l = new List<TabControl>()
			{
				ReloadTabControl,
				MainTabControl,
				SettingsTabControl,
				LoginTabControl,
				AccountTabContol,
				UserItemsTabControl
			};
			
			foreach (var i in l)
			{
                foreach (var item in i.Items)
                {
					var tab = (TabItem)item;
					tab.Visibility = Visibility.Collapsed;
                }
			}
		}
		private void GoToLanisWebsite(object sender, MouseButtonEventArgs e)
		{
			if (loginSchoolId == null)
				Process.Start("https://portal.lanis-system.de/");
			else
				Process.Start("https://login.schulportal.hessen.de/?i=" + loginSchoolId);
		}

		private void InstallDefaultFiles()
		{
			//DEFAULT LANGUAGE PACKS
			var languagePacks = Directory.GetFiles(DataPaths.DefaultLocalsPath, "*.json");
			foreach (var lpk in languagePacks)
			{
				File.WriteAllText(DataPaths.LocalsPath + lpk.Replace(DataPaths.DefaultLocalsPath, ""), File.ReadAllText(lpk));
			}

			//DEFAULT THEME PRESETS
			var themePresets = Directory.GetFiles(DataPaths.DefaultThemesPath, "*.json");
			foreach (var thp in themePresets)
			{
				File.WriteAllText(DataPaths.ThemesPath + thp.Replace(DataPaths.DefaultThemesPath, ""), File.ReadAllText(thp));
			}
		}

		public async Task CreateDriver()
		{
			if(Driver != null)
            {
				await Task.Run(() => Driver.Close());
				await Task.Run(() => Driver.Quit());
				Driver = null;
			}

			var chromeOptions = new ChromeOptions();
			//chromeOptions.AddArguments("headless");
			DriverIsHeadless = true;

			Driver = new ChromeDriver(chromeOptions);

			await Task.Run(() => Driver.Navigate().GoToUrl("https://portal.lanis-system.de/"));
		}
		public async Task SetHeadless(bool headless)
        {
			Console.WriteLine(DriverIsHeadless);
			if (DriverIsHeadless == headless) return;
			if (!headless)
            {
				var chromeOptions = new ChromeOptions();

				var cookies = Driver.Manage().Cookies.AllCookies;
                OpenQA.Selenium.Html5.IWebStorage webStorage = null;
				if (Driver.HasWebStorage)
					webStorage = Driver.WebStorage;

				var url = Driver.Url;

				await Task.Run(() => Driver.Quit());
				Driver = null;

				Driver = new ChromeDriver(chromeOptions);

				await Task.Run(() => Driver.Navigate().GoToUrl(url));

				foreach (var cookie in cookies)
				{
					await Task.Run(() => Driver.Manage().Cookies.AddCookie(cookie));
				}

				if(webStorage != null)
				{
					Driver.GetType().GetProperty("WebStorage", BindingFlags.Public|BindingFlags.NonPublic)
						.SetValue(Driver.WebStorage, webStorage);
				}

				await Task.Run(() => Driver.Navigate().GoToUrl(url));
            } 
			else
            {
				await CreateDriver();

				if(LoggedInUser != null)
                {
					loginSchoolId = LoggedInUser.LoginData.SchoolId;
					LoginLanis(0, LoggedInUser.LoginData.Username, LoggedInUser.LoginData.Password, true);
                }
			}
			DriverIsHeadless = headless;
		}
		public async void ReloadAll()
		{
			var animDuratiom = Constants.tabTransitionDuration;

			var opacityAnim1 = new DoubleAnimation(0, TimeSpan.FromSeconds(animDuratiom));
			ReloadTabControl.BeginAnimation(OpacityProperty, opacityAnim1);

			await Task.Delay(TimeSpan.FromSeconds(animDuratiom));

			ReloadTabControl.SelectedIndex = 1;
			await Task.Delay(1);
			ReloadTabControl.SelectedIndex = 0;

			var opacityAnim2 = new DoubleAnimation(1, TimeSpan.FromSeconds(animDuratiom));
			ReloadTabControl.BeginAnimation(OpacityProperty, opacityAnim2);
		}

		public static Dictionary<string, string> GetTagValues(string tag)
		{
			tag = $"{{ {tag} }}";
			var valuesDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(tag);
			return valuesDic;
		}

		public static async void SwitchTabContolTab(TabControl tabControl, int tabIndex)
        {
			if (tabControl.SelectedIndex == tabIndex) return;

			var animDuratiom = Constants.tabTransitionDuration;

			var opacityAnim1 = new DoubleAnimation(0, TimeSpan.FromSeconds(animDuratiom));
			tabControl.BeginAnimation(OpacityProperty, opacityAnim1);

			await Task.Delay(TimeSpan.FromSeconds(animDuratiom));

			tabControl.SelectedIndex = tabIndex;

			var opacityAnim2 = new DoubleAnimation(1, TimeSpan.FromSeconds(animDuratiom));
			tabControl.BeginAnimation(OpacityProperty, opacityAnim2);
		}

		private void Expander_LostFocus(object sender, RoutedEventArgs e) => ((Expander)sender).IsExpanded = false;

		#region Application
		private async void App_Closed(object sender, EventArgs e)
		{
			if (Driver != null)
				try {
					await Task.Run(() => Driver.Close());
					await Task.Run(() => Driver.Quit());
				} catch { }
		}

		private void App_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			Secrets.CheckAltF4(e);
		}
		#endregion

		#region Header
		//HEADER COLLAPSE ANIMATIONS
		bool expanding = false;
		private async void HeaderGrid_MouseEnter(object sender, MouseEventArgs e)
		{
            var heightAnim = new GridLengthAnimation
            {
                To = new GridLength(80),
                From = MainGrid.RowDefinitions[0].Height,
                Duration = TimeSpan.FromSeconds(0.3),
                DecelerationRatio = 0.5,
                AccelerationRatio = 0.5
            };
            MainGrid.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, heightAnim);

			if (MainGrid.RowDefinitions[0].Height.Value >= 80) return;
			expanding = true;
			await Task.Delay(TimeSpan.FromSeconds(0.4));
			expanding = false;
		}
		private void HeaderGrid_MouseLeave(object sender, MouseEventArgs e)
		{
            var heightAnim = new GridLengthAnimation
            {
                To = new GridLength(10),
                From = MainGrid.RowDefinitions[0].Height
            };

            if (!expanding)
			{
				heightAnim.Duration = TimeSpan.FromSeconds(1);
				heightAnim.BeginTime = TimeSpan.FromSeconds(2.5);
			}
			else
				heightAnim.Duration = TimeSpan.FromSeconds(0.3);

			heightAnim.DecelerationRatio = 0.5;
			heightAnim.AccelerationRatio = 0.5;
			MainGrid.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, heightAnim);
		}
		private void HeaderGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (MainGrid.RowDefinitions[0].Height.Value < 80) return;
			HeaderGrid_MouseLeave(null, null);
		}
		#endregion

		#region News
		private static readonly List<Article> articles = new List<Article>();
		public async void CheckNews()
		{
			try
			{
				var jsonString = File.ReadAllText(DataPaths.VersionPath + @"\Version.json");
				var newsPost = JsonConvert.DeserializeObject<Post>(jsonString);

				Version.Text = newsPost.Version;

				var supportedLocals = new List<string>();

				foreach (var article in newsPost.Articles)
				{
					articles.Add(article);
					supportedLocals.Add(article.LocalName);
					newsLocalNames.Add(article.Local, article.LocalName);
				}

				NewsLanguageSelector.ItemsSource = supportedLocals;

				var sl = Config.SelectedLocal.Remove(Config.SelectedLocal.LastIndexOf('.'));

				if (newsLocalNames.ContainsKey(sl))
					NewsLanguageSelector.SelectedItem = newsLocalNames[sl];
				else
					NewsLanguageSelector.SelectedIndex = 0;

				if (!newsPost.Viewed)
                {
					await Task.Delay(1000);
					ShowNews();
                }
			}
			catch { }
		}

		public void SetNews(Article article)
		{
			if (article == null) return;

			NewsTextContainer.Children.Clear();

            var image = new System.Windows.Media.Imaging.BitmapImage();
			image.BeginInit();
			image.UriSource = new Uri(DataPaths.VersionPath + "Version.png");
			image.EndInit();

			NewsImage.Source = image;

			NewsHeader.Text = article.Header;

			var textBlock = new TextBlock()
			{
				FontFamily = new FontFamily("/BetterLanis;component/resources/Fonts/Heebo/#Heebo Medium"),
				TextWrapping = TextWrapping.Wrap,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Top,
				Width = double.NaN,
				Height = double.NaN,
				Background = null,
				FontSize = 14,
			};

			textBlock.SetResourceReference(ForegroundProperty, "SecondTextColor");
			NewsTextContainer.Children.Add(textBlock);

			foreach (var text in article.Text)
			{
				var span = new Run(text);

				if (text.Trim().StartsWith("<subheader>"))
				{

					span.SetResourceReference(TextElement.ForegroundProperty, "TextColor");
					span.Text = text.Replace("<subheader>", "").Trim() + "\n";
					span.FontSize = 18;
					span.FontFamily = new FontFamily("/BetterLanis;component/resources/Fonts/Heebo/#Heebo");
					span.FontWeight = FontWeights.Bold;

					textBlock.Inlines.Add(span);

					textBlock.Inlines.Add(new Run(" \n") { FontSize = 2 });
				}
                else
                {
					span.SetResourceReference(TextElement.ForegroundProperty, "SecondTextColor");
					span.Text = text.Trim() + "\n";
					span.FontSize = 14;

					textBlock.Inlines.Add(span);

					textBlock.Inlines.Add(new Run(" \n") { FontSize = 12});
				}

			}
		}

		public void ShowNews()
        {
			var yOffsetAnim = new DoubleAnimation(Constants.transitionSwipeDistance, 0,
				TimeSpan.FromSeconds(Constants.newsFadeDuration))
			{ EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut } };

			var opacityAnim = new DoubleAnimation(0, 1,
				TimeSpan.FromSeconds(Constants.newsFadeDuration));

			var blurAnim = new DoubleAnimation(0, 15,
				TimeSpan.FromSeconds(Constants.newsFadeDuration));

			NewsBorderTranslateTransform.BeginAnimation(TranslateTransform.YProperty, yOffsetAnim);
			News.BeginAnimation(OpacityProperty, opacityAnim);
			MainGridBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);

			Panel.SetZIndex(News, 0);
		}

		public async void HideNews()
		{
			var yOffsetAnim = new DoubleAnimation(Constants.transitionSwipeDistance,
				TimeSpan.FromSeconds(Constants.newsFadeDuration))
			{ EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn } };

			var opacityAnim = new DoubleAnimation(0,
				TimeSpan.FromSeconds(Constants.newsFadeDuration));

			var blurAnim = new DoubleAnimation(0,
				TimeSpan.FromSeconds(Constants.newsFadeDuration));

			NewsBorderTranslateTransform.BeginAnimation(TranslateTransform.YProperty, yOffsetAnim);
			News.BeginAnimation(OpacityProperty, opacityAnim);
			MainGridBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);

			await Task.Delay(TimeSpan.FromSeconds(Constants.newsFadeDuration));
			Panel.SetZIndex(News, -100);
		}

		#region Events
		private readonly Dictionary<string, string> newsLocalNames = new Dictionary<string, string>();
        private void NewsLanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Article selectedArticle = null;

			foreach (var article in articles)
			{
				if (article.LocalName == (string)NewsLanguageSelector.SelectedItem)
                {
					selectedArticle = article;
					break;
                }
			}

			SetNews(selectedArticle);
		}
		private void NewsButton_Click(object sender, RoutedEventArgs e) => ShowNews();
		private void HideNewsButton_MouseDown(object sender, MouseButtonEventArgs e) => HideNews();
		#endregion
		#endregion

		#region Settings

		#region Appearance
		public void LoadThemes()
		{
			List<string> themes = new List<string>();

			foreach (var theme in Directory.GetFiles(DataPaths.ThemesPath, "*.json"))
			{
				themes.Add(theme.Replace(".json", "").Replace(DataPaths.ThemesPath, ""));
			}

			ThemeSelectorComboBox.ItemsSource = themes;

			ThemeSelectorComboBox.SelectedItem = Config.ThemePreset;
		}
		
        private void ColorPicker_ColorChanged(object sender, ColorChangedEventArgs e)
        {
			if(sender.GetType() != typeof(ColorPicker))
            {
				Console.WriteLine(((FrameworkElement)sender).Name + " is not a ColorPicker Control");
				return;
			}

			var colorPicker = (ColorPicker)sender;

			var values = GetTagValues((string)colorPicker.Tag);

            if (!values.ContainsKey("colorid"))
            {
				Console.WriteLine(colorPicker.Name + " does not contain the Tag 'colorid'");
				return;
            }

			ThemeResources.SetResource(values["colorid"], e.hex);
        }
		private void ColorPicker_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!(bool)e.NewValue) return;

			if (sender.GetType() != typeof(ColorPicker))
			{
				Console.WriteLine(((FrameworkElement)sender).Name + " is not a ColorPicker Control");
				return;
			}

			var colorPicker = (ColorPicker)sender;

			var values = GetTagValues((string)colorPicker.Tag);

			if (!values.ContainsKey("colorid"))
			{
				Console.WriteLine(colorPicker.Name + " does not contain the Tag 'colorid'");
				return;
			}

			var hex = ThemeResources.GetResource(values["colorid"]);
			if (hex == null)
			{
				Console.WriteLine(values["colorid"] + "was not found in Resources");
				return;
			}

			colorPicker.SetHex(hex);
		}
		private void ColorPickerStackpanel_Loaded(object sender, RoutedEventArgs e)
		{
            for (int i = 0; i < ColorPickerStackpanel.Children.Count; i++)
            {
				var child = ColorPickerStackpanel.Children[i];
				if (child.GetType() == typeof(WrapPanel))
					Panel.SetZIndex(child, ColorPickerStackpanel.Children.Count - i);
				else	
					Panel.SetZIndex(child, int.MinValue);
			}
		}

		#region Events
		private void ThemeSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var path = DataPaths.ThemesPath + (string)ThemeSelectorComboBox.SelectedItem + ".json";

			if (!File.Exists(path)) return;

			ThemeResources.SetResources(
				JsonConvert.DeserializeObject<ThemePreset>(
					File.ReadAllText(path)));

			ColorPickerStackpanel.Visibility = Visibility.Hidden;
			ColorPickerStackpanel.Visibility = Visibility.Visible;

			Config.ThemePreset = (string)ThemeSelectorComboBox.SelectedItem;
			Config.SavePrefrences();
		}
		#endregion
		#endregion

		#region Language
		private void LoadLocals()
		{
			var locals = Directory.GetFiles(DataPaths.LocalsPath, "*.json");

			foreach (var local in locals)
			{
				var json = File.ReadAllText(local);

                var button = new LocalButton(json)
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Width = double.NaN,
                    fileName = local.Replace(DataPaths.LocalsPath, ""),
					IsSelected = local.Replace(DataPaths.LocalsPath, "") == Config.SelectedLocal
                };
                button.LocalSelected += SelectLocal;

				LocalsList.Children.Add(button);
			}
		}
		public void SelectLocal(object sender, LocalSelectedEventArgs e)
		{
			if (Config.SelectedLocal == e.LocalFileName) return;

			Config.SelectedLocal = e.LocalFileName;
			Config.SavePrefrences();

			foreach (var element in LocalsList.Children)
			{
				var button = (LocalButton)element;

				button.IsSelected = button.fileName == e.LocalFileName;
			}

			ReloadAll();
		}
		#endregion

		#region Events
		bool settingsAnimating = false;
		private async void SettingsButton_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (settingsAnimating) return;

			var animDuration = Constants.tabTransitionDuration;

			settingsAnimating = true;

			if (MainTabControl.SelectedIndex == 0)
            {
				var sb1 = new Storyboard();

				var marginAnim1 = new ThicknessAnimation(
					new Thickness(-Constants.transitionSwipeDistance, 0, Constants.transitionSwipeDistance, 0),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseIn }
				};

				var opacityAnim1 = new DoubleAnimation(
					0,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
				};

				Storyboard.SetTargetProperty(marginAnim1, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim1, new PropertyPath("Opacity"));
				sb1.Children.Add(marginAnim1);
				sb1.Children.Add(opacityAnim1);
				sb1.Begin(MainTabControl);

				await Task.Delay(TimeSpan.FromSeconds(animDuration));

				var sb2 = new Storyboard();

				var marginAnim2 = new ThicknessAnimation(
					new Thickness(Constants.transitionSwipeDistance, 0, -Constants.transitionSwipeDistance, 0),
					new Thickness(0),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
				};

				var opacityAnim2 = new DoubleAnimation(
					1,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTargetProperty(marginAnim2, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim2, new PropertyPath("Opacity"));
				sb2.Children.Add(marginAnim2);
				sb2.Children.Add(opacityAnim2);

				sb2.Begin(MainTabControl);

				MainTabControl.SelectedIndex = 1;
			}
            else
            {
				var sb1 = new Storyboard();

				var marginAnim1 = new ThicknessAnimation(
					new Thickness(Constants.transitionSwipeDistance, 0, -Constants.transitionSwipeDistance, 0),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseIn }
				};

				var opacityAnim1 = new DoubleAnimation(
					0,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
				};

				Storyboard.SetTargetProperty(marginAnim1, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim1, new PropertyPath("Opacity"));
				sb1.Children.Add(marginAnim1);
				sb1.Children.Add(opacityAnim1);
				sb1.Begin(MainTabControl);

				await Task.Delay(TimeSpan.FromSeconds(animDuration));

				var sb2 = new Storyboard();

				var marginAnim2 = new ThicknessAnimation(
					new Thickness(-Constants.transitionSwipeDistance, 0, Constants.transitionSwipeDistance, 0),
					new Thickness(0),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
				};

				var opacityAnim2 = new DoubleAnimation(
					1,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTargetProperty(marginAnim2, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim2, new PropertyPath("Opacity"));
				sb2.Children.Add(marginAnim2);
				sb2.Children.Add(opacityAnim2);

				sb2.Begin(MainTabControl);

				MainTabControl.SelectedIndex = 0;
			}

			settingsAnimating = false;
		}

		private async void SwitchSettingsTab(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var tags = GetTagValues((string)button.Tag);
			var targetTabIndex = int.Parse(tags["tabIndex"]);

			if (targetTabIndex == SettingsTabControl.SelectedIndex) return;

			//ANIMATE TAB INDICATOR			
			var margin = SelectedSettingsTabIndicator.Margin;

			var point = button.TranslatePoint(new Point(0, 0), SettingsTabButtons);
			margin.Top = point.Y + 1;

			bool up = margin.Top < SelectedSettingsTabIndicator.Margin.Top;

			var sb = new Storyboard();
            var marginAnim = new ThicknessAnimation(
                margin,
                new Duration(TimeSpan.FromSeconds(Constants.tabTransitionDuration * 2)))
            { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut } };

            Storyboard.SetTargetProperty(marginAnim, new PropertyPath("Margin"));
			sb.Children.Add(marginAnim);

			sb.Begin(SelectedSettingsTabIndicator);

			//ANIMATE TAB TRANSITION
			var animDuration = Constants.tabTransitionDuration;

            if (!up)
            {
				var sb1 = new Storyboard();

				var marginAnim1 = new ThicknessAnimation(
					new Thickness(0, -Constants.transitionSwipeDistance, 0, Constants.transitionSwipeDistance),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseIn }
				};

				var opacityAnim1 = new DoubleAnimation(
					0,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
				};

				Storyboard.SetTargetProperty(marginAnim1, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim1, new PropertyPath("Opacity"));
				sb1.Children.Add(marginAnim1);
				sb1.Children.Add(opacityAnim1);
				sb1.Begin(SettingsTabControl);

				await Task.Delay(TimeSpan.FromSeconds(animDuration));

				var sb2 = new Storyboard();

				var marginAnim2 = new ThicknessAnimation(
					new Thickness(0, Constants.transitionSwipeDistance, 0, -Constants.transitionSwipeDistance),
					new Thickness(0),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
				};

				var opacityAnim2 = new DoubleAnimation(
					1,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTargetProperty(marginAnim2, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim2, new PropertyPath("Opacity"));
				sb2.Children.Add(marginAnim2);
				sb2.Children.Add(opacityAnim2);

				sb2.Begin(SettingsTabControl);
            }
			else
			{
				var sb1 = new Storyboard();

				var marginAnim1 = new ThicknessAnimation(
					new Thickness(0, Constants.transitionSwipeDistance, 0, -Constants.transitionSwipeDistance),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseIn }
				};

				var opacityAnim1 = new DoubleAnimation(
					0,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
				};

				Storyboard.SetTargetProperty(marginAnim1, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim1, new PropertyPath("Opacity"));
				sb1.Children.Add(marginAnim1);
				sb1.Children.Add(opacityAnim1);
				sb1.Begin(SettingsTabControl);

				await Task.Delay(TimeSpan.FromSeconds(animDuration));

				var sb2 = new Storyboard();

				var marginAnim2 = new ThicknessAnimation(
					new Thickness(0, -Constants.transitionSwipeDistance, 0, Constants.transitionSwipeDistance),
					new Thickness(0),
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
				};

				var opacityAnim2 = new DoubleAnimation(
					1,
					TimeSpan.FromSeconds(animDuration)
					)
				{
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTargetProperty(marginAnim2, new PropertyPath("Margin"));
				Storyboard.SetTargetProperty(opacityAnim2, new PropertyPath("Opacity"));
				sb2.Children.Add(marginAnim2);
				sb2.Children.Add(opacityAnim2);

				sb2.Begin(SettingsTabControl);
			}

			SettingsTabControl.SelectedIndex = targetTabIndex;
		}
		#endregion
		#endregion

		#region Local
		private void SetLocal(object sender, RoutedEventArgs e) => Local.SetLocalStatic(sender);
		#endregion

		#region Login

		string loginSchoolId;
		private async void LoginLanis(int exCount = 0, string overwriteUsername = null, string overwritePassword = null, bool justLogin = false)
		{
			string username = this.username.Text;
			string password = this.password.Password;

			if (overwriteUsername != null && overwritePassword != null)
            {
				username = overwriteUsername;
				password = overwritePassword;
				RememberLoginDataToggle.IsChecked = true;
            }

			var returnLogin = false;

			double animDuration = Constants.loginWarningWiggleDuration;

			SomethingWentWrong.Visibility = Visibility.Hidden;

			if (username == "")
			{
				//USERNAME EMPTY WARNING

				UsernamePasswordWrong.Visibility = Visibility.Hidden;

				var sb = new Storyboard();

                var paddingAnim = new ThicknessAnimation(
                    new Thickness(20, 0, 0, 0),
                    new Thickness(15, 0, 0, 0),
                    new Duration(TimeSpan.FromSeconds(animDuration)
                    ))
                { EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

				sb.Children.Add(paddingAnim);

				UsernameEmpty.Visibility = Visibility.Visible;
				sb.Begin(UsernameEmpty);
				returnLogin = true;
			} else UsernameEmpty.Visibility = Visibility.Hidden;

			if (password == "")
			{
				//PASSWORD EMPTY WARNING

				UsernamePasswordWrong.Visibility = Visibility.Hidden;

				var sb = new Storyboard();

                var paddingAnim = new ThicknessAnimation(
                    new Thickness(20, 0, 0, 0),
                    new Thickness(15, 0, 0, 0),
                    new Duration(TimeSpan.FromSeconds(animDuration)
                    ))
                { EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

				sb.Children.Add(paddingAnim);

				if (username == "") await Task.Delay(100);

				PasswordEmpty.Visibility = Visibility.Visible;
				sb.Begin(PasswordEmpty);
				returnLogin = true;
			} else PasswordEmpty.Visibility = Visibility.Hidden;

			if (returnLogin)
			{
				SwitchTabContolTab(MainTabControl, 0);
				return;
			}

			if (loginSchoolId == null)
			{
				//NO SCHOOLSELECTED WARNING

				UsernamePasswordWrong.Visibility = Visibility.Hidden;

				var sb = new Storyboard();

                var paddingAnim = new ThicknessAnimation(
                    new Thickness(20, 0, 0, 0),
                    new Thickness(15, 0, 0, 0),
                    new Duration(TimeSpan.FromSeconds(animDuration)
                    ))
                { EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

				sb.Children.Add(paddingAnim);
				sb.Begin(NoSchoolSelected);

				SwitchTabContolTab(MainTabControl, 0);
				return;
			}

			LoginButton.IsEnabled = false;

			LogoutStatusText.Text = "Logging In...";

			SwitchTabContolTab(LoginTabControl ,2);

			if (Driver.HasWebStorage)
			{
				await Task.Run(() => Driver.WebStorage.LocalStorage.Clear());
				await Task.Run(() => Driver.WebStorage.SessionStorage.Clear());
			}
			await Task.Run(() => Driver.Manage().Cookies.DeleteAllCookies());

			if(exCount == 0)
				await Task.Run(() => Driver.Navigate().GoToUrl("https://login.schulportal.hessen.de/?i=" + loginSchoolId));

			try {
				await Task.Run(() => Driver.FindElementByXPath("//*[@id=\"username2\"]").SendKeys(username));
				await Task.Run(() => Driver.FindElementByXPath("//*[@id=\"inputPassword\"]").SendKeys(password));

				await Task.Run(() => Driver.FindElementByXPath("//*[@id=\"tlogin\"]").Click());
			} catch (Exception ex)
			{
				Console.WriteLine("\nFAILED LOGGING IN! Exeption:\n\n" + ex + "\n\nRETRYING...");
				LogoutStatusText.Text = "Failed Logging In! Retrying...";
				if (exCount == 3)
				{
					//SOMETHING WENT WRONG

					UsernamePasswordWrong.Visibility = Visibility.Hidden;

					var sb = new Storyboard();

                    var paddingAnim = new ThicknessAnimation(
                        new Thickness(20, 0, 0, 0),
                        new Thickness(15, 0, 0, 0),
                        new Duration(TimeSpan.FromSeconds(animDuration)
                        ))
                    { EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseOut } };
                    Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

					sb.Children.Add(paddingAnim);

					SomethingWentWrong.Visibility = Visibility.Visible;
					sb.Begin(SomethingWentWrong);

					LoginButton.IsEnabled = true;
					SwitchTabContolTab(LoginTabControl, 0);
					SwitchTabContolTab(MainTabControl, 0);
					return;
				}
				await Task.Delay(1000);
				LoginLanis(exCount + 1, username, password);
				SwitchTabContolTab(MainTabControl, 0);
				return;
			}


			if (Driver.Url.Contains("/schulportallogin.php"))
			{
				//LOGIN SUCCESSFULL
				if (justLogin) 
					return; 

				LoggedInUser = new UserData()
				{
					LoginData = new LoginData
					{
						Password = password,
						Username = username,
						SchoolId = loginSchoolId
					}
				};

				string saveUsername = "";
				string savePassword = "";
				string saveLoginId = "";

				if (RememberLoginDataToggle.IsChecked)
                {
					saveUsername = username;
					savePassword = password;
					saveLoginId = loginSchoolId;

					LogoutStatusText.Text = "Saving Login Data...";
				}
				SaveLoginData(RememberLoginDataToggle.IsChecked, saveUsername, savePassword, saveLoginId);
				ClearSchoolList();
				#region RESET LOGIN MENU
				this.username.Text = "";
				this.password.Password = "";
				passwordShown.Text = "";
				SchoolListSearch.Text = "";
				RememberLoginDataToggle.IsChecked = false;
				loginSchoolId = null;
				NoSchoolSelected.Visibility = Visibility.Visible;
				SelectedSchool.Visibility = Visibility.Hidden;

				UsernameEmpty.Visibility = Visibility.Hidden;
				PasswordEmpty.Visibility = Visibility.Hidden;
				UsernamePasswordWrong.Visibility = Visibility.Hidden;
				SomethingWentWrong.Visibility = Visibility.Hidden;
				HidePassword_MouseDown(null, null);
				#endregion

				BuildMenu();
			}
			else
			{
                try
                {
					await Task.Run(() => Driver.FindElementByXPath("//*[@id=\"login\"]/div/div/form/div[1]/span"));
				}
                catch 
				{
					//SOMETHING WENT WRONG

					UsernamePasswordWrong.Visibility = Visibility.Hidden;

					var sb1 = new Storyboard();

                    var paddingAnim1 = new ThicknessAnimation(
                        new Thickness(20, 0, 0, 0),
                        new Thickness(15, 0, 0, 0),
                        new Duration(TimeSpan.FromSeconds(animDuration)
                        ))
                    { EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseOut } };
                    Storyboard.SetTargetProperty(paddingAnim1, new PropertyPath("Padding"));

					sb1.Children.Add(paddingAnim1);

					SomethingWentWrong.Visibility = Visibility.Visible;
					sb1.Begin(SomethingWentWrong);

					LoginButton.IsEnabled = true;
					SwitchTabContolTab(LoginTabControl, 0);
					SwitchTabContolTab(MainTabControl, 0);
					return;
				}

				//USERNAME OR PASSWORD WRONG

				var sb2 = new Storyboard();

                var paddingAnim2 = new ThicknessAnimation(
                    new Thickness(20, 0, 0, 0),
                    new Thickness(15, 0, 0, 0),
                    new Duration(TimeSpan.FromSeconds(animDuration)
                    ))
                { EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTargetProperty(paddingAnim2, new PropertyPath("Padding"));

				sb2.Children.Add(paddingAnim2);

				UsernamePasswordWrong.Visibility = Visibility.Visible;
				sb2.Begin(UsernamePasswordWrong);
				SwitchTabContolTab(LoginTabControl, 0);
			}

			LoginButton.IsEnabled = true;
		}
		private void AutoLogin(string user)
        {
			if (!File.Exists(DataPaths.UsersDataPath + user + @"\lData.blud")) return;

			var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(CryptoData.Decrypt(File.ReadAllText(DataPaths.UsersDataPath + user + @"\lData.blud")));
			loginSchoolId = dic["schoolId"];
			LoginLanis(0, dic["username"], dic["password"]);
        }

		private void SaveLoginData(bool autologin, string username, string password, string schoolid)
		{
			var userDataPath = DataPaths.UsersDataPath + $"{schoolid}.{username.ToLower()}" + @"\";
			var loginConfigPath = DataPaths.UsersDataPath + "login.lcfg";
			var loginDataPath = userDataPath + $"lData.blud";
			Directory.CreateDirectory(userDataPath);

			var loginData = new Dictionary<string, string>
			{
				{ "schoolId", schoolid },
				{ "username", username.ToLower() },
				{ "password", password }
			};

			string loginDataJsonString = JsonConvert.SerializeObject(loginData);
			string loginDataEncryptedString = CryptoData.Encrypt(loginDataJsonString);

			var autoLoginData = new Dictionary<string, string>()
			{
				{ "autoLogin", autologin.ToString() },
				{" user", "" }
			};

			if (autologin)
				autoLoginData["user"] = $"{schoolid}.{username.ToLower()}";

			else if (File.Exists(loginConfigPath))
				autoLoginData["user"] = JsonConvert.DeserializeObject<Dictionary<string, string>>(
					CryptoData.Decrypt(File.ReadAllText(loginConfigPath)))["user"];

			string autoLoginDataJsonString = JsonConvert.SerializeObject(autoLoginData);
			string autoLoginDataEncryptedString = CryptoData.Encrypt(autoLoginDataJsonString);

			File.WriteAllText(loginConfigPath, autoLoginDataEncryptedString);
			File.WriteAllText(loginDataPath, loginDataEncryptedString);
		}

		public async void LogoutLanis()
        {
			LogoutStatusText.Text = "Deleting Login Data...";
			SaveLoginData(false, "", "", "");

			LoggedInUser = null;

			SwitchTabContolTab(AccountTabContol, 0);
			LoginTabControl.SelectedIndex = 2;

			LoggingIn.Visibility = Visibility.Hidden;
			LoggingOut.Visibility = Visibility.Visible;

			LogoutStatusText.Text = "Creating new Chrome Driver...";
			await CreateDriver();

			SwitchTabContolTab(LoginTabControl, 0);
			LoggingIn.Visibility = Visibility.Visible;
			LoggingOut.Visibility = Visibility.Hidden;

		}

		#region Events
		private async void CenterBorderInside(object sender, SizeChangedEventArgs e)
        {
			bool isOddX = false;
			bool isOddY = false;

			var grid = (Grid)sender;
			Border border = null;
			foreach (var child in grid.Children) if (child.GetType() == typeof(Border)) border = (Border)child;

			if (border == null) return;

			double previousWidth = grid.ActualWidth;
			double previousHeight = grid.ActualHeight;
			await Task.Delay(300);

			if (previousWidth != grid.ActualWidth || previousHeight != grid.ActualHeight)
				return;

			if (grid.ActualWidth % 2 != 0) isOddX = true;
			if (grid.ActualHeight % 2 != 0) isOddY = true;

			var margin = border.Margin;

            if (isOddX) margin.Left = 1;
			else margin.Left = 0;

			if (isOddY) margin.Top = 11;
			else margin.Top = 10;

			border.Margin = margin;
		}
		private void LoginButton_Click(object sender, RoutedEventArgs e) => LoginLanis();
		private void SelectSchoolButton_Click(object sender, RoutedEventArgs e) 
		{ 
			SwitchTabContolTab(LoginTabControl, 1);
			if(SchoolSelect.Children.Count <= 0)
				BuildSchoolList();
		}

		//USERNAME REGEX +NO WHITESPACES
		private void username_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var reg = new Regex("[0-zÀ-Ÿ.]");
			e.Handled = !reg.IsMatch(e.Text);
		}
		private void username_TextChange(object sender, TextChangedEventArgs e)
		{
			if (!username.Text.Contains(" ")) return;

			var caretIndex = username.CaretIndex - 1;
			username.Text = username.Text.Replace(" ", "");
			username.CaretIndex = caretIndex;
		}

		//USERNAME>PASSWORD>LOGIN FLOW + CAPS LOCK DETECTION
		private void username_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (password.Visibility == Visibility.Hidden)
					passwordShown.Focus();
				else password.Focus();
			}
		}
		private void password_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Tab) return;

			if (e.Key == Key.CapsLock)
			{
				if (Keyboard.GetKeyStates(Key.CapsLock) != KeyStates.Down)
				{
					//CAPS LOCK WARNING

					var sb = new Storyboard();

                    var paddingAnim = new ThicknessAnimation(
                        new Thickness(0),
                        new Duration(TimeSpan.FromSeconds(0.25)
                        ))
                    { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut } };
                    Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

					sb.Children.Add(paddingAnim);

					sb.Begin(CapsLockWarning);

					var padding = password.Padding;
					padding.Right = 30;
					password.Padding = padding;
				}
				else
				{
					var sb = new Storyboard();

                    var paddingAnim = new ThicknessAnimation(
                        new Thickness(30, 0, 0, 0),
                        new Duration(TimeSpan.FromSeconds(0.25)
                        ))
                    { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut } };
                    Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

					sb.Children.Add(paddingAnim);

					sb.Begin(CapsLockWarning);

					var padding = password.Padding;
					padding.Right = 3;
					password.Padding = padding;
				}
			}

			if (e.Key == Key.Enter)
            {
				if (loginSchoolId == null)
                {
					SwitchTabContolTab(LoginTabControl, 1);
					if (SchoolSelect.Children.Count <= 0)
						BuildSchoolList();
                }
				else
					LoginLanis();
            }
		}
		private void passwordShown_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (loginSchoolId == null)
				{
					SwitchTabContolTab(LoginTabControl, 1);
					if (SchoolSelect.Children.Count <= 0)
						BuildSchoolList();
				}
				else
					LoginLanis();
			}
		}
		private void password_GotFocus(object sender, RoutedEventArgs e)
		{
			if (Keyboard.GetKeyStates(Key.CapsLock) != KeyStates.None)
			{
				//CAPS LOCK WARNING

				var sb = new Storyboard();

                var paddingAnim = new ThicknessAnimation(
                    new Thickness(0),
                    new Duration(TimeSpan.FromSeconds(0.25)
                    ))
                { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

				sb.Children.Add(paddingAnim);

				sb.Begin(CapsLockWarning);

				var padding = password.Padding;
				padding.Right = 30;
				password.Padding = padding;
			}
		}
		private void password_LostFocus(object sender, RoutedEventArgs e)
		{
			if (Keyboard.GetKeyStates(Key.CapsLock) != KeyStates.None)
			{
				//CAPS LOCK WARNING

				var sb = new Storyboard();

                var paddingAnim = new ThicknessAnimation(
                    new Thickness(30, 0, 0, 0),
                    new Duration(TimeSpan.FromSeconds(0.25)
                    ))
                { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTargetProperty(paddingAnim, new PropertyPath("Padding"));

				sb.Children.Add(paddingAnim);

				sb.Begin(CapsLockWarning);

				var padding = password.Padding;
				padding.Right = 3;
				password.Padding = padding;
			}
		}

		//HIDE/SHOW PASSWORD
		private void ShowPassword_MouseDown(object sender, MouseButtonEventArgs e)
		{
			password.Visibility = Visibility.Hidden;
			passwordShown.Visibility = Visibility.Visible;

			ShowPassword.Visibility = Visibility.Hidden;
			HidePassword.Visibility = Visibility.Visible;

			passwordShown.Text = password.Password;

			if (password.IsFocused)
			{
				passwordShown.Focus();
				passwordShown.CaretIndex = passwordShown.Text.Length;
			}
		}
		private void HidePassword_MouseDown(object sender, MouseButtonEventArgs e)
		{
			password.Visibility = Visibility.Visible;
			passwordShown.Visibility = Visibility.Hidden;

			ShowPassword.Visibility = Visibility.Visible;
			HidePassword.Visibility = Visibility.Hidden;

			if (passwordShown.IsFocused)
			{
				password.Focus();
				password.GetType().GetMethod("Select",
					BindingFlags.Instance | BindingFlags.NonPublic)
					.Invoke(password, new object[] { password.Password.Length, 0 });
			}
		}
		private void passwordShown_TextChanged(object sender, TextChangedEventArgs e) => password.Password = passwordShown.Text;

		bool loggingInWaitAnimating = false;
		private async void LoggingInWait_Loaded(object sender, RoutedEventArgs e)
        {
			if (loggingInWaitAnimating) return;

			var animDuration = 0.5;

			LoggingInWait1.Margin = new Thickness(0, 40, 0, 0);
			LoggingInWait2.Margin = new Thickness(0, 40, 0, 0);
			LoggingInWait3.Margin = new Thickness(0, 40, 0, 0);

			if (!LoggingInWait.IsLoaded)
			{
				loggingInWaitAnimating = false;
				return;
			}

			loggingInWaitAnimating = true;

			LoggingInWait1.BeginAnimation(MarginProperty, new ThicknessAnimation(
				new Thickness(0), TimeSpan.FromSeconds(animDuration))
				{ EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut } });

			await Task.Delay(TimeSpan.FromSeconds(animDuration / 3));

			LoggingInWait2.BeginAnimation(MarginProperty, new ThicknessAnimation(
				new Thickness(0), TimeSpan.FromSeconds(animDuration))
				{ EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut } });

			await Task.Delay(TimeSpan.FromSeconds(animDuration / 3));

			LoggingInWait3.BeginAnimation(MarginProperty, new ThicknessAnimation(
				new Thickness(0), TimeSpan.FromSeconds(animDuration))
			{ EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut } });

			await Task.Delay(TimeSpan.FromSeconds(animDuration / 3));

			LoggingInWait1.BeginAnimation(MarginProperty, new ThicknessAnimation(
				new Thickness(0,40,0,0), TimeSpan.FromSeconds(animDuration))
			{ EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseIn } });

			await Task.Delay(TimeSpan.FromSeconds(animDuration / 3));

			LoggingInWait2.BeginAnimation(MarginProperty, new ThicknessAnimation(
				new Thickness(0, 40, 0, 0), TimeSpan.FromSeconds(animDuration))
			{ EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseIn } });

			await Task.Delay(TimeSpan.FromSeconds(animDuration / 3));

			LoggingInWait3.BeginAnimation(MarginProperty, new ThicknessAnimation(
				new Thickness(0, 40, 0, 0), TimeSpan.FromSeconds(animDuration))
			{ EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseIn } });

			await Task.Delay(TimeSpan.FromSeconds(animDuration / 3));

			loggingInWaitAnimating = false;

			LoggingInWait_Loaded(sender, e); 
		}
		private void LoginBorder_Loaded(object sender, RoutedEventArgs e) => BuildSchoolList();
		private void LoginBorder_Unloaded(object sender, RoutedEventArgs e) => ClearSchoolList();
		#endregion

		#region School Select
		District[] schoollist;
		bool buildingList = false;
		public async void BuildSchoolList()
		{
			buildingList = true;

			if (schoollist == null)
			{
				var client = new WebClient();
				var schoolListUrl = new Uri("https://portal.lanis-system.de/exporteur.php?a=schoollist");

				string jsonString = await client.DownloadStringTaskAsync(schoolListUrl);

				schoollist = JsonConvert.DeserializeObject<District[]>(jsonString);
			}

			foreach (var district in schoollist)
			{
				foreach (var school in district.Schools)
				{
                    var schoolButton = new SchoolButton(district.Name, school.Name, school.Local, school.Id)
                    {
                        Width = SchoolSelect.Width
                    };

                    schoolButton.SchoolSelected += new EventHandler<SchoolSelectedEventArgs>(SchoolSelected);

					SchoolSelect.Children.Add(schoolButton);
				}

				ResultsTextBox.Text = GetTagValues((string)ResultsTextBox.Tag)["rawtext"]
					.Replace("[#results]", SchoolSelect.Children.Count.ToString());

				await Task.Delay(50);
			}

			buildingList = false;
		}
		public void ClearSchoolList()
		{
			SchoolSelect.Children.Clear();

			ResultsTextBox.Text = SchoolSelect.Children.Count.ToString();
		}
		public async void SearchSchoolList(string keyword)
		{
			while (buildingList)
				await Task.Delay(50);

			int visibleCount = 0;

			foreach (var child in SchoolSelect.Children)
			{
				var schoolButton = (SchoolButton)child;

				var keywords = keyword.Trim().ToLower().Split(new char[] { ' ' });

				bool visible = true;

				foreach (var key in keywords)
				{
					if (!schoolButton.SchoolName.ToLower().Contains(key) &&
						!schoolButton.SchoolLocal.ToLower().Contains(key) &&
						!schoolButton.DistrictName.ToLower().Contains(key))
						visible = false;
				}

				if (visible == true || keyword == "")
				{
					schoolButton.Visibility = Visibility.Visible;
					visibleCount++;
				}
				else
				{
					schoolButton.Visibility = Visibility.Collapsed;
				}
			}

			ResultsTextBox.Text = GetTagValues((string)ResultsTextBox.Tag)["rawtext"]
				.Replace("[#results]", visibleCount.ToString());
		}
		public void SchoolSelected(object sender, SchoolSelectedEventArgs e)
		{
			SwitchTabContolTab(LoginTabControl, 0);

			loginSchoolId = e.Id;

			NoSchoolSelected.Visibility = Visibility.Hidden;
			SelectedSchool.Visibility = Visibility.Visible;

			SelectedSchoolName.Text = e.Name;
			SelectedSchoolDistrict.Text = e.District;
			SelectedSchoolLocal.Text = e.Local;
		}

		#region Events
		private void SchoolSelect_Loaded(object sender, RoutedEventArgs e) => SchoolListSearch.Focus();
		private async void SchoolListSearch_TextChanged(object sender, TextChangedEventArgs e) 
		{
			var beginText = SchoolListSearch.Text;
			await Task.Delay(250);

			if (SchoolListSearch.Text == beginText)
				SearchSchoolList(SchoolListSearch.Text); 
		}
		private void BackToLogin_MouseDown(object sender, MouseButtonEventArgs e) => SwitchTabContolTab(LoginTabControl, 0);
		private void ResultsTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			SetLocal(sender, e);

			ResultsTextBox.Text = GetTagValues((string)ResultsTextBox.Tag)["rawtext"]
					.Replace("[#results]", SchoolSelect.Children.Count.ToString());
		}
        private async void SchoolListSearch_KeyDown(object sender, KeyEventArgs e)
        {
			if(e.Key == Key.Enter)
            {
				await Task.Delay(250);

                foreach (var obj in SchoolSelect.Children)
                {
					var button = (SchoolButton)obj;

                    try
                    {
						if (button.Visibility == Visibility.Visible)
						{
							SchoolSelected(button, new SchoolSelectedEventArgs()
							{
								Id = button.schoolId,
								Name = button.SchoolName,
								District = button.DistrictName,
								Local = button.SchoolLocal,
							});
							return;
                        }
                    } catch { }
                }
            }
        }
		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			int height = -1;
			int i = 0;
            foreach (var child in SchoolSelect.Children)
			{
				var element = (FrameworkElement)child;

				if(element.Visibility != Visibility.Collapsed)
                {
					i++;

					if (height == -1) height = (int)element.Height;

					var posInStack = height * i;
					var inView = MathE.InRange(posInStack, e.VerticalOffset, e.VerticalOffset + e.ViewportHeight + height);

					if (inView && element.Visibility != Visibility.Visible)
						element.Visibility = Visibility.Visible;
					else if (!inView && element.Visibility != Visibility.Hidden)
						element.Visibility = Visibility.Hidden;
                }
			}
		}
        #endregion

        #endregion
        #endregion

        #region Main Menu
        readonly List<IWebElement> appElements = new List<IWebElement>();
		private async void BuildMenu()
        {
			LogoutStatusText.Text = "Building Menu...";
			try
			{
				await Task.Run(() => Driver.FindElementByXPath("//*[@id=\"topapps\"]").Click());
				await Task.Delay(50);
				await Task.Run(() => Driver.FindElementByXPath("//*[@id=\"topappssearch\"]").SendKeys(" "));
				var elementsInTopApps = await Task.Run(() => Driver.FindElementByXPath("//*[@id=\"menueband\"]/li[2]/ul").FindElements(By.XPath("//*[@id=\"menueband\"]/li[2]/ul/li")));


				for (int i = 2; i < elementsInTopApps.Count; i++)
                {
					try 
					{ appElements.Add(await Task.Run(() => Driver.FindElement(By.XPath($"//*[@id=\"menueband\"]/li[2]/ul/li[{i+1}]/a")))); }
                    catch 
					{ Console.WriteLine("NO ELEMENT FOUND"); }
                }
			} catch (Exception ex)
			{
				Console.WriteLine("\n\nFAILED BUILDING MENU! Exeption:\n\n" + ex + "\n\nRETRYING...");
				LogoutStatusText.Text = "Failed Building Menu! Retrying...";
				await Task.Delay(200);
				BuildMenu();
				return;
			}

			string userRawText = (await Task.Run(() => Driver.FindElementByXPath("/html/body/div[1]/div[4]/div/div[2]/ul[2]/li[1]/a"))).Text;

			int indexOfComma = userRawText.IndexOf(',') + 1;
			string firstName = userRawText.Substring(indexOfComma, userRawText.IndexOf('(') - indexOfComma).Trim();
			string lastName = userRawText.Substring(0, userRawText.IndexOf(',')).Trim().Trim();
			string schoolclass = userRawText.Substring(userRawText.IndexOf('(') + 1).Replace(")", "").Trim();

			LoggedInUser.FirstName = firstName;
			LoggedInUser.LastName = lastName;
			LoggedInUser.Class = schoolclass;

			var buildQueue = new Dictionary<string, object[]>();
			var buttonNames = new List<string>();
            foreach (var element in appElements)
            {
				if (!buttonNames.Contains(element.Text + "*") && !buttonNames.Contains(element.Text))
                {
					var button = new Button
					{
						Height = 30,
						Margin = new Thickness(0, 0, 0, 5),
						Padding = new Thickness(0, 1, 0, 0),
						FontFamily = new FontFamily("Heebo Medium"),
						FontSize = 14,
						Content = element.Text,
						Tag = "\"appUrl\":\"" + element.GetProperty("href") + "\"",
						Cursor = Cursors.Hand
                    };

					buttonNames.Add((string)button.Content);

                    button.SetResourceReference(ForegroundProperty, "ButtonTextColor");
					button.SetResourceReference(BackgroundProperty, "ButtonBackgroundColor");
					button.SetResourceReference(BorderBrushProperty, "OutlineColor");

					switch (button.Content as string)
					{
						case "Stundenplan":
							buildQueue.Add("Timetable", new object[] { element.GetProperty("href") });

							button.Content = "Timetable";
							button.Tag = "\"localid\":\"timetable\"";

							button.Click += TimetableButton_Click;
							button.Loaded += SetLocal;

							AppButtonList.Children.Add(button);
							break;
						default:
							button.ToolTip = "Opens Browser Mode";
							button.Content = button.Content + "*";

							button.Click += NotSupportedAppSelected;

							NotSupportedAppButtonList.Visibility = Visibility.Visible;
							NotSupportedAppButtonList.Children.Add(button);
							break;
                    }
                }
            }

            foreach (var queue in buildQueue)
            {
                switch (queue.Key)
                {
					case "Timetable":
						LogoutStatusText.Text = "Building Timetable...";
						await BuildTimetable(queue.Value[0] as string);
						break;
                }
            }

			LogoutStatusText.Text = "Done";
			KeepAlive();
			SwitchTabContolTab(AccountTabContol, 1);
			await Task.Delay(TimeSpan.FromSeconds(Constants.tabTransitionDuration));
			LoginTabControl.SelectedIndex = 0;
		}
        private async Task BuildTimetable(string url)
        {
			var urlBefore = Driver.Url;

			await Task.Run(() => Driver.Navigate().GoToUrl(url));

			var element = Driver.FindElementByXPath("//*[@id=\"all\"]/div[1]/div[2]/div[3]/table");
			var timetable = await Timetable.BuildTableFromWebElement(element);
			await Task.Run(() => Driver.Navigate().GoToUrl(urlBefore));

			_Timetable.SetTimetable(timetable);
		}

		bool keepAlive = false;
		private async void KeepAlive()
        {
			if (keepAlive) return;
			keepAlive = true;

			TryKeepAlive:
			TryClickKeepAlive();
			await Task.Delay(TimeSpan.FromSeconds(90));
			if (!keepAlive) return;

			goto TryKeepAlive;
		}
		private async void TryClickKeepAlive()
        {
			try
			{
				await Task.Run(() => Driver.FindElementByXPath("/html/body/div[4]/div[2]/div/div[3]/button").Click());
			}
			catch { Console.WriteLine("Keep Alive Button not found."); }
		}

		#region Events
		#region UserItemsClick
		public void TimetableButton_Click(object sender, RoutedEventArgs e) => SwitchTabContolTab(UserItemsTabControl, 2);
        #endregion
        private void LogoutButton_Click(object sender, RoutedEventArgs e) => LogoutLanis();
		private async void NotSupportedAppSelected(object sender, RoutedEventArgs e)
        {
			var button = (Button)sender;
			Driver.Navigate().GoToUrl(GetTagValues((string)button.Tag)["appUrl"]);
			await SetHeadless(false);
			SwitchTabContolTab(UserItemsTabControl, 1);
		}
        private void UsernameText_Loaded(object sender, RoutedEventArgs e)
        {
			if(LoggedInUser != null)
				UsernameText.Text = $"{LoggedInUser.FirstName} {LoggedInUser.LastName} ,{LoggedInUser.Class}";
        }
		private void LoginAgain_Click(object sender, RoutedEventArgs e)
		{
			var url = Driver.Url;
			Driver.Url = "about:blank";
			Driver.Manage().Cookies.DeleteAllCookies();
			if (Driver.HasWebStorage)
			{
				Driver.WebStorage.LocalStorage.Clear();
				Driver.WebStorage.SessionStorage.Clear();
			}

			Console.WriteLine(LoggedInUser.LoginData.Username, LoggedInUser.LoginData.Password);
			loginSchoolId = LoggedInUser.LoginData.SchoolId;
			LoginLanis(overwriteUsername: LoggedInUser.LoginData.Username, overwritePassword: LoggedInUser.LoginData.Password);
			Driver.Url = url;
		}
        private async void ReturnFromBrowserMode_Click(object sender, RoutedEventArgs e)
        {
			await SetHeadless(true);
        }
        #endregion

        #endregion
    }
}