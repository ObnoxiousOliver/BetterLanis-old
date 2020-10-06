using BetterLanis.Extensions;
using BetterLanis.UserSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BetterLanis.UserContols.Timetable
{
    /// <summary>
    /// Interaction logic for TimetableControl.xaml
    /// </summary>
    public partial class TimetableControl : UserControl
    {
        public TimetableControl()
        {
            InitializeComponent();
        }

        public void SetTimetable(Timetable timetable)
        {
            for (int y = 1; y <= 11; y++)
            {
                for (int x = 1; x <= 5; x++)
                {
                    TimetableLession l = new TimetableLession();
                    bool empty = true;

                    if(timetable.Lessions[x - 1, y - 1] != null)
                    { 
                        l = timetable.Lessions[x - 1, y - 1];
                        empty = false;
                    }

                    CreateLession(new TimetableElement(l) {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    }, x, y, l.Span, empty);
                }
            }

            for (int i = 0; i < timetable.Times.Length; i++)
            {
                var times = timetable.Times[i];

                if(times != null)
                {
                    CreateLession(new TimetableElement(times)
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    }, 0, i + 1);
                }
            }
        }

        private void CreateLession(TimetableElement timetableElement, int day, int lessionTime, int rowSpan = 1, bool empty = false)
        {
            TimetableContainer.Children.Add(timetableElement);

            Grid.SetColumn(timetableElement, day);
            Grid.SetRow(timetableElement, lessionTime);
            
            if(empty)
                Panel.SetZIndex(timetableElement, -100);

            timetableElement.RowSpan = rowSpan;
        }

        public void SetLocal(object sender, RoutedEventArgs e) {  /*Local.SetLocalStatic(sender);*/}
	}
}