using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterLanis.UserContols
{
    /// <summary>
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        public ToggleButton()
        {
            InitializeComponent();
        }

        private double animationDuration = 0.25;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;

                if (value)
                {
                    Slide.Background = (Brush)FindResource("AccentColor");

                    var storyboard = new Storyboard();

                    var thicknessAnimation = new ThicknessAnimation(
                        new Thickness(14, 0, 0, 0), 
                        new Duration(TimeSpan.FromSeconds(animationDuration)
                        ));
                    thicknessAnimation.EasingFunction = new ExponentialEase() 
                    { EasingMode = EasingMode.EaseOut };

                    Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath("Margin"));
                    storyboard.Children.Add(thicknessAnimation);

                    storyboard.Begin(Thumb);
                }
                else
                {
                    Slide.Background = (Brush)FindResource("ToggleButtonBackgroundColor");

                    var storyboard = new Storyboard();
                    var thicknessAnimation = new ThicknessAnimation(
                        new Thickness(0, 0, 0, 0), 
                        new Duration(TimeSpan.FromSeconds(animationDuration)
                        ));
                    thicknessAnimation.EasingFunction = new ExponentialEase()
                    { EasingMode = EasingMode.EaseOut };

                    Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath("Margin"));
                    storyboard.Children.Add(thicknessAnimation);

                    storyboard.Begin(Thumb);
                }
            }
        }

        private bool isChecked = false;

        private void Toggle(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }
    }

}