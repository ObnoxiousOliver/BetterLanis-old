using BetterLanis.Extensions;
using BetterLanis.Extensions.Animations;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace BetterLanis.EasterEggs
{
	public class Secrets
    {
		static bool altf4Closing = false;
		public async static void CheckAltF4(KeyEventArgs e)
        {
			if (e.Key == Key.System && e.SystemKey == Key.F4 && !altf4Closing)
			{
				e.Handled = true;

				altf4Closing = true;

				//EASTER EGG (ALT + F4 ANIMATION)
				var translateAnim = new DoubleAnimation(200, 0, TimeSpan.FromSeconds(0.75))
				{
					EasingFunction = new ExponentialEase()
					{
						EasingMode = EasingMode.EaseOut,
						Exponent = 4
					}
				};
				var scaleAnim = new DoubleAnimation(1, 1.5, TimeSpan.FromSeconds(0.4))
				{
					BeginTime = TimeSpan.FromSeconds(0.6),
					EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseInOut }
				};

				var blackbarDistanceAnim = new GridLengthAnimation
				{
					From = new GridLength(MainWindow.Instance.Window.ActualHeight),
					To = new GridLength(0),
					Duration = TimeSpan.FromSeconds(.25),
					BeginTime = TimeSpan.FromSeconds(0.75),
					DecelerationRatio = 0.5
				};

				MainWindow.Instance.AltF4BlackbarDistance.BeginAnimation(RowDefinition.HeightProperty, blackbarDistanceAnim);

				MainWindow.Instance.AltF4Translate.BeginAnimation(TranslateTransform.YProperty, translateAnim);
				MainWindow.Instance.AltF4Scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
				MainWindow.Instance.AltF4Scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

				var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(Constants.newsFadeDuration));

				var blurAnim = new DoubleAnimation(15, TimeSpan.FromSeconds(Constants.newsFadeDuration));

				MainWindow.Instance.AltF4.BeginAnimation(MainWindow.OpacityProperty, opacityAnim);
				MainWindow.Instance.MainGridBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);
				MainWindow.Instance.NewsGridBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);

				Panel.SetZIndex(MainWindow.Instance.AltF4, 0);

				await Task.Delay(TimeSpan.FromSeconds(1.3));
				MainWindow.Instance.Window.Visibility = Visibility.Hidden;
				await Task.Delay(TimeSpan.FromSeconds(1));
				altf4Closing = false;
				MainWindow.Instance.Close();
			}
		}
    }
}