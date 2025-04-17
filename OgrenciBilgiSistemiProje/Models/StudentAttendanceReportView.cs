namespace OgrenciBilgiSistemiProje.Models
{
    public class StudentAttendanceReportView
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public List<LessonAttendanceReport> LessonReports { get; set; } = new List<LessonAttendanceReport>();
    }

    public class LessonAttendanceReport
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public int AbsenceCount { get; set; } // Toplam devamsız saat sayısı (her saat için)
        public double AbsenceRate { get; set; } // Yüzde cinsinden devamsızlık oranı
        public List<DateTime> AbsenceDatesHour1 { get; set; } = new List<DateTime>();
        public List<DateTime> AbsenceDatesHour2 { get; set; } = new List<DateTime>();
        public List<DateTime> AbsenceDatesHour3 { get; set; } = new List<DateTime>();
    }
}