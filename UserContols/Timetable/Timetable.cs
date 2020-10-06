using BetterLanis.Extensions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BetterLanis.UserContols.Timetable
{
    public class Timetable
    {
        public TimetableLession[,] Lessions { get; set; } = new TimetableLession[5, 11];
        public TimetableLession[] Times { get; set; } = new TimetableLession[11];

        public static async Task<Timetable> BuildTableFromWebElement(IWebElement webElement)
        {
            try
            {
                var timetable = new Timetable();
                var rows = (await Task.Run(() => webElement.FindElements(By.TagName("tr")))).ToList();
                for (int y = 0; y < rows.Count; y++)
                {
                    var lessionElements = (await Task.Run(() => rows[y].FindElements(By.TagName("td")))).ToList();

                    string time = string.Empty;
                    int timeIndex = 1;
                    if (lessionElements.Count > 0)
                    {
                        timeIndex = int.Parse((await Task.Run(() => lessionElements[0]
                            .FindElement(By.ClassName("hidden-xs"))
                            .FindElement(By.TagName("b")))).Text.Replace(". Stunde", ""));

                        time = (await Task.Run(() => lessionElements[0]
                            .FindElement(By.ClassName("VonBis"))
                            .FindElement(By.TagName("small")))).Text;

                        Console.WriteLine((timeIndex.ToString()+ ". ").PadRight(4) + time);

                    }

                    var timeLession = new TimetableLession();
                    timeLession.Subjects.Add(new TimetableSubject(timeIndex.ToString(), time, "", isTimeIndicator: true));
                    timetable.Times[timeIndex - 1] = timeLession;

                    for (int x = 1; x < lessionElements.Count; x++)
                    {
                        List<IWebElement> subjectElements;
                        var lession = new TimetableLession();
                        subjectElements = (await Task.Run(() => lessionElements[x].FindElements(By.TagName("div")))).ToList(); 
                        
                        if(subjectElements.Count > 0)
                        {
                            for (int i = 0; i < subjectElements.Count; i++)
                            {
                                string subject = (await Task.Run(() => subjectElements[i].FindElement(By.TagName("b")))).Text.Trim();
                                string teacher = (await Task.Run(() => subjectElements[i].FindElement(By.TagName("small")))).Text.Trim();
                                string room = 
                                    ("#" + subjectElements[i].Text
                                        .Remove(subjectElements[i].Text.LastIndexOf(teacher)))
                                        .Replace("#" + subject, "").Trim();

                                ABWeekSelector aBWeek = ABWeekSelector.None;
                                try
                                {
                                    var ab = (await Task.Run(() => subjectElements[i].FindElement(By.TagName("span")))).Text.Trim();
                                    if (ab == "A") aBWeek = ABWeekSelector.A;
                                    else if (ab == "B") aBWeek = ABWeekSelector.B;
                                } catch { }

                                lession.Subjects.Add(new TimetableSubject(subject, room, teacher, aBWeek));
                            }

                            lession.Span = int.Parse(lessionElements[x].GetAttribute("rowspan"));
                        }

                        timetable.Lessions[x -1, y -1] = lession;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("\n[===========================][===========================][===========================][===========================][===========================]");

                for (int y = 0; y < timetable.Lessions.GetLength(1); y++)
                {
                    for (int x = 0; x < timetable.Lessions.GetLength(0); x++)
                    {
                        if (y > 0)
                        {
                            for (int y2 = 0; y2 < y; y2++)
                            {
                                if (timetable.Lessions[x, y2] != null) 
                                { 
                                    if (y2 + timetable.Lessions[x, y2].Span > y)
                                    {
                                        //Console.WriteLine($"({x}|{y}) is obscured by {timetable.Lessions[x, y2].Subjects[0].Subject}");
                                        timetable.Lessions = MathE.MoveRowToRight(timetable.Lessions, y, x);
                                    }
                                }
                            }
                        }

                        try
                        {
                            if (timetable.Lessions[x, y] == null)
                                timetable.Lessions[x, y] = null;
                            else
                            { var subject = timetable.Lessions[x, y].Subjects[0]; }
                        }
                        catch { timetable.Lessions[x, y] = null; }

                        if (timetable.Lessions[x, y] == null)
                            Console.Write("[ " + StringE.PadBoth("empty", 25) + " ]");
                        else
                        {
                            var subject = timetable.Lessions[x, y].Subjects[0];

                            Console.Write("[ " + StringE.PadBoth($"{subject.Subject} {subject.Room} {subject.Teacher}", 25) + " ]");
                        }
                    }
                    Console.WriteLine("\n[===========================][===========================][===========================][===========================][===========================]");
                }

                Console.WriteLine();

                return timetable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ABWeekSelector GetABWeek()
        {
            var rest = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Parse("1/1/2020"), CalendarWeekRule.FirstDay, DayOfWeek.Monday) % 2;
            if (rest > 0) return ABWeekSelector.B;
            else          return ABWeekSelector.A;
        }
    }
}