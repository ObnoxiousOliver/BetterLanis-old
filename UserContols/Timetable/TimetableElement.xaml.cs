using System;
using System.Windows;
using System.Windows.Controls;

namespace BetterLanis.UserContols.Timetable
{
    /// <summary>
    /// Interaction logic for TimetableElement.xaml
    /// </summary>
    public partial class TimetableElement : UserControl
    {
        public int RowSpan { 
            get { return Grid.GetRowSpan(this); }
            set { Grid.SetRowSpan(this, value); }
        }

        public TimetableElement(TimetableLession lession)
        {
            InitializeComponent();

            foreach (var subject in lession.Subjects)
            {
                var subjectControl = new TimetableSubjectControl(subject)
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                SubjectHolder.Children.Add(subjectControl);
            }

            RowSpan = (int)MathE.Clamp(lession.Span, 1, double.PositiveInfinity);
        }

        private void ContentGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ContentGrid.Focus();
    }
}