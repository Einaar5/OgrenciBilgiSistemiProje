namespace OgrenciBilgiSistemiProje.Models
{
    public class StudentAttendanceReportView
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public List<LessonAttendanceReport> LessonReports { get; set; } = new List<LessonAttendanceReport>();
    }

    public class LessonAttendanceReport
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; } = "";
        public int AbsenceCount { get; set; }
        public double AbsenceRate { get; set; }
        public List<DateTime> AbsenceDates { get; set; } = new List<DateTime>();
    }
}