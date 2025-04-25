namespace OgrenciBilgiSistemiProje.Models
{
    public class AttendanceReportView
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public List<StudentAttendanceReport> StudentReports { get; set; } = new List<StudentAttendanceReport>();
    }

    public class StudentAttendanceReport
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int AttendanceCount { get; set; } // Geldiği ders sayısı
        public int AbsenceCount { get; set; } // Gelmediği ders sayısı
        public int TotalLessons { get; set; }
    }
}