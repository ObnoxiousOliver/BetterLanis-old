using BetterLanis.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterLanis.UserContols.Timetable
{
    public enum ABWeekSelector { None,A,B }

    /// <summary>
    /// Interaction logic for TimetableSubjectControl.xaml
    /// </summary>
    public partial class TimetableSubjectControl : UserControl
    {

        public string Subject 
        { 
            get { return subject; }
            set { 
                _Subject.Text = value; 
                subject = value;
            } 
        }
        string subject = "";

        public string Room
        {
            get { return _Room.Text; }
            set { _Room.Text = value; }
        }

        public string Teacher
        {
            get { return _Teacher.Text; }
            set { _Teacher.Text = value; }
        }

        public ABWeekSelector ABWeek
        {
            get 
            {
                if (_ABWeek.Text == "A") return ABWeekSelector.A;
                if (_ABWeek.Text == "B") return ABWeekSelector.B;
                else return ABWeekSelector.None;
            }
            set {
                if (value == ABWeekSelector.A) 
                {
                    ABWeekBorder.Visibility = Visibility.Visible; 
                    _ABWeek.Text = "A"; 
                }
                else if (value == ABWeekSelector.B)
                {
                    ABWeekBorder.Visibility = Visibility.Visible;
                    _ABWeek.Text = "B";
                }
                else ABWeekBorder.Visibility = Visibility.Collapsed;

                ABWeekBorder_Loaded();
            }
        }

        public TimetableSubjectControl(TimetableSubject _subject)
        {
            InitializeComponent();

            Subject = _subject.Subject;
            Room = _subject.Room;
            Teacher = _subject.Teacher;
            ABWeek = _subject.ABWeek;

            if (_subject.IsTimeIndicator)
            {
                _Subject.Tag = "\"localid\":\"timeIndicator\"";
                _Subject.Loaded += SetLocal;
            }
        }

        public void SetLocal(object sender, RoutedEventArgs e) 
        {
            Local.SetLocalStatic(sender);
            ((TextBlock)sender).Text = ((TextBlock)sender).Text.Replace("[#lessionIndex]", subject);
        }

        private void ABWeekBorder_Loaded(object sender = null, RoutedEventArgs e = null)
        {
            if(ABWeek == Timetable.GetABWeek())
            {
                ABWeekBorder.SetResourceReference(Border.BackgroundProperty, "AccentColor");
                _ABWeek.SetResourceReference(TextBlock.ForegroundProperty, "AccentButtonTextColor");
            }
            else
            {
                ABWeekBorder.SetResourceReference(Border.BackgroundProperty, "TimetableBadgeBackgroundColor");
                _ABWeek.SetResourceReference(TextBlock.ForegroundProperty, "TimetableDetailsTextColor");
            }
        }
    }
}