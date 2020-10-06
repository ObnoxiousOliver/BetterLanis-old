using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BetterLanis.UserContols
{
    /// <summary>
    /// Interaction logic for LocalButton.xaml
    /// </summary>
    public partial class LocalButton : UserControl
    {
        public LocalButton(string json)
        {
            InitializeComponent();

            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            LocalName.Text = values["localName"];

            var uri = new Uri("https://www.worldometers.info/img/flags/small/" + values["flagFilename"]);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();

            FlagImage.Source = bitmap;
            MouseDown += OnLocalSelected;
        }

        public bool IsSelected 
        { 
            get { return isSelected; } 
            set 
            { 
                isSelected = value;

                if (value)
                    ButtonGrid.Background = (Brush)FindResource("AccentColor");
                else
                    ButtonGrid.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            } 
        }
        private bool isSelected;

        public string fileName;

        protected virtual void OnLocalSelected(object sender, MouseButtonEventArgs e)
        {
            var eventArgs = new LocalSelectedEventArgs();
            eventArgs.LocalFileName = fileName;
            LocalSelected?.Invoke(this, eventArgs);
        }

        public event EventHandler<LocalSelectedEventArgs> LocalSelected;
    }

    public class LocalSelectedEventArgs : EventArgs
    {
        public string LocalFileName { get; set; }
    }
}