using BetterLanis.UserSettings;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using BetterLanis.Extensions;

namespace BetterLanis.UserContols
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
        }
        public string Hex
        {
            get { return hex; }
            private set
            {
                hex = value;

                var color = (Color)ColorConverter.ConvertFromString(value);
                Fill.Background = new SolidColorBrush(ColorE.ConvertFromHsv(GetHue(color), 1, 1));
                ColorIndicator.Background = new SolidColorBrush(color);
                Caret.Background = new SolidColorBrush(color);

                Red_Input.Text = color.R.ToString();
                Green_Input.Text = color.G.ToString();
                Blue_Input.Text = color.B.ToString();

                var hue = GetHue(color);
                var sat = GetSaturation(color);
                var val = GetValue(color);

                Hue_Input.Text = HueSlider.Value.ToString();
                Saturation_Input.Text = sat.ToString();
                Value_Input.Text = val.ToString();

                Hex_Input.Text = value.Replace("#", "");

                if (settingHex) settingHex = false;
                else OnColorChanged();
            }
        }

        private string hex = "#FF0000";

        bool settingHex;
        public void SetHex(string hex)
        {
            try { ColorConverter.ConvertFromString(hex); }
            catch { return; }

            var color = (Color)ColorConverter.ConvertFromString(hex);

            settingHex = true;
            HueSlider.Value = GetHue(color);
            SetCaretPosOnValues(GetSaturation(color), GetValue(color));

            settingHex = true;
            Hex = hex;
        }

        #region SHOW HIDE
        private void ShowColorPickerBoard()
        {
            var sb = new Storyboard();
        
            var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(Constants.tabTransitionDuration))
            { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("Opacity"));
                
            sb.Children.Add(opacityAnim);
            sb.Begin(ColorPickerBoard);
        }

        private void ColorIndicator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(!_Popup.IsOpen)
                _Popup.IsOpen = true;
            else
                _Popup.IsOpen = false;
        }
        private void Ok_Click(object sender, RoutedEventArgs e) => _Popup.IsOpen = false;
        #endregion

        private System.Drawing.Color ToSystemDrawingColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
        }
        private double GetHue(Color color)
        {
            return ToSystemDrawingColor(color).GetHue();
        }
        private double GetSaturation(Color color)
        {
            double ret;
            ColorE.ColorToHSV(ToSystemDrawingColor(color), out _, out ret, out _);
            return ret;
        }
        private double GetValue(Color color)
        {
            double ret;
            ColorE.ColorToHSV(ToSystemDrawingColor(color), out _, out _, out ret);
            return ret;
        }


        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) 
        {
            var color = (Color)ColorConverter.ConvertFromString(Hex);
            var sat = GetSaturation(color);
            var val = GetValue(color);

            Hex = ColorE.HexConverter(ColorE.ConvertFromHsv(HueSlider.Value, sat, val));
        }
        private void Caret_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || ColorPickerBoard.Opacity == 0)
            {
                caretPopup.IsOpen = false;
                return;
            }

            caretPopup.IsOpen = true;

            var pos = e.GetPosition(ColorSelectorField);

            Caret.Margin = new Thickness(MathE.Clamp(pos.X - 6, -6, 240), MathE.Clamp(pos.Y - 6, -6, 240), -6, -6);

            var sat = MathE.Clamp(pos.X, 0, 250) / 250;
            var val = (MathE.Clamp(pos.Y, 0, 250) / -250) + 1;
                        
            Hex = ColorE.HexConverter(ColorE.ConvertFromHsv(HueSlider.Value, sat, val));
        }
        private void SetCaretPosOnValues(double sat, double val)
        {
            Caret.Margin = new Thickness(MathE.Clamp(sat, 0, 1) * 245 - 6, MathE.Clamp(val, 0, 1) * -245 + 245 - 6, -6, -6);
        }

        private void NumberRegex(object sender, TextCompositionEventArgs e)
            => e.Handled = new Regex("[^0-9]").IsMatch(e.Text);
        private void NumberDecimalRegex(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9,.]").IsMatch(e.Text) ||
                (((TextBox)sender).Text.Count(x => x == '.' || x == ',') > 0 && 
                e.Text.Count(x => x == '.' || x == ',') > 0);
        }
        private void HexDecimalRegex(object sender, TextCompositionEventArgs e)
            => e.Handled = new Regex("[^0-9a-fA-F]").IsMatch(e.Text);

        private void UnfocusOnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                ColorPickerBoard.Focus();
            }
        }
        private void RGB_Unfocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;

            try
            {
                var value = byte.Parse(textBox.Text);
                textBox.Text = MathE.Clamp(value, 0, 255).ToString();
            }
            catch { textBox.Text = "255"; }

            var r = byte.Parse(Red_Input.Text);
            var g = byte.Parse(Green_Input.Text);
            var b = byte.Parse(Blue_Input.Text);

            var color = Color.FromRgb(r, g, b);

            HueSlider.Value = GetHue(color);
            SetCaretPosOnValues(GetSaturation(color), GetValue(color));

            Hex = ColorE.HexConverter(Color.FromRgb(r, g, b));
        }
        private void SV_Unfocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;

            try
            {
                var value = double.Parse(textBox.Text.Replace('.', ','));
                textBox.Text = MathE.Clamp(value, 0, 1).ToString().Replace(',', '.');
            }
            catch { textBox.Text = "1"; }

            var sat = double.Parse(Saturation_Input.Text);
            var val = double.Parse(Value_Input.Text);

            SetCaretPosOnValues(sat, val);
            Hex = ColorE.HexConverter(ColorE.ConvertFromHsv(HueSlider.Value, sat, val));
        }
        private void Hue_Unfocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;

            try
            {
                var value = double.Parse(textBox.Text.Replace('.', ','));
                textBox.Text = MathE.Clamp(value, 0, 360).ToString().Replace(',', '.');
            }
            catch { textBox.Text = "0"; }

            HueSlider.Value = double.Parse(Hue_Input.Text);
        }
        private void Hex_Unfocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;

            SetHex("#" + textBox.Text);
        }

        bool colorChanging;
        protected virtual async void OnColorChanged()
        {
            if (colorChanging) return;
            colorChanging = true;
            await Task.Delay(1000/5);
            colorChanging = false;

            var eventArgs = new ColorChangedEventArgs();
            eventArgs.hex = hex;
            ColorChanged?.Invoke(this, eventArgs);

        }

        public event EventHandler<ColorChangedEventArgs> ColorChanged;

        private void _Popup_Opened(object sender, EventArgs e)
        {
            ShowColorPickerBoard();
        }
    }

}
public class ColorChangedEventArgs : EventArgs
{
    public string hex;
}