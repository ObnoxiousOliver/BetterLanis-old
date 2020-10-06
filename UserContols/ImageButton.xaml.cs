using BetterLanis.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BetterLanis.UserContols
{
    /// <summary>
    /// Interaction logic for ImageButton.xaml
    /// </summary>
    public partial class ImageButton : UserControl
    {
        public ImageButton()
        {
            InitializeComponent();
        }

        public Brush Image
        { 
            get { return ImageRec.OpacityMask; } 
            set { ImageRec.OpacityMask = value; }
        }

        public CornerRadius CornerRadius
        {
            get { return Border.CornerRadius; }
            set { Border.CornerRadius = value; }
        }

        public double ImageWidth
        {
            get { return ImageRec.Width; }
            set { ImageRec.Width = value;}
        }
        public double ImageHeight
        {
            get { return ImageRec.Height; }
            set { ImageRec.Height = value; }
        }
    }
}