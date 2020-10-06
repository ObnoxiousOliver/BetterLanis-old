using System.Collections.Generic;

namespace BetterLanis.UserContols.Timetable
{
    public class TimetableLession
    {
        public List<TimetableSubject> Subjects { get; set; } = new List<TimetableSubject>();
        public int Span { get; set; } = 1;
    }

    public class TimetableSubject
    {
        public string Subject { get; }
        public string Teacher { get; }
        public string Room { get; }
        public ABWeekSelector ABWeek { get; }
        public bool IsTimeIndicator { get; }

        public TimetableSubject(string subject, string room, string teacher, ABWeekSelector abWeek = ABWeekSelector.None, bool isTimeIndicator = false)
        {
            Subject = subject;
            Room = room;
            Teacher = teacher;
            ABWeek = abWeek;
            IsTimeIndicator = isTimeIndicator;
        }
    }
}