Student Information System (SIS) - OBS
<br>
📝 Overview
Student Information System (SIS), also known as OBS (Öğrenci Bilgi Sistemi) in Turkish, is a comprehensive web-based platform designed to streamline academic processes for students, teachers, and administrators in educational institutions. Built with ASP.NET Core MVC, this system provides real-time management of academic records, course materials, and institutional communication.

🌟 Key Features
👨‍🎓 For Students:
Grade Tracking: View all exam results and course grades

Course Management: Access registered courses and materials

Attendance Monitoring: Check attendance records in real-time

Messaging System: Communicate directly with instructors

👨‍🏫 For Teachers:
Grade Management: Enter and adjust student grades

Quiz System: Create and weight different assessment types

Attendance Recording: Mark and review student attendance

Notification Center: Send announcements to classes/departments

🖥️ For Administrators:
User Management: Handle student and faculty accounts

Course Scheduling: Organize academic timetables

Department Oversight: Manage academic departments

System Analytics: Generate academic reports

🛠️ Technical Specifications
Framework: ASP.NET Core 6.0 MVC

Database: SQL Server with Entity Framework Core

Authentication: Role-based (Student/Teacher/Admin)

Frontend: Bootstrap 5 with responsive design


📚 Database Schema
The system utilizes a relational database structure with key entities:

Users (Students/Teachers)

Courses/Lessons

Departments

Grades/Assessments

Attendance Records

Notifications/Messages

🚀 Getting Started
Clone the repository

Configure database connection in appsettings.json

Run migrations: dotnet ef database update

Launch application: dotnet run

bash
git clone https://github.com/your-repo/obs-system.git
cd obs-system
dotnet restore
dotnet ef database update
dotnet run





🌍 Turkish Version (Türkçe)
Öğrenci Bilgi Sistemi (OBS)
📝 Genel Bakış
Öğrenci Bilgi Sistemi (OBS), eğitim kurumlarında öğrenciler, öğretmenler ve yöneticiler için akademik süreçleri dijitalleştiren kapsamlı bir web platformudur. ASP.NET Core MVC ile geliştirilen bu sistem, gerçek zamanlı akademik kayıt yönetimi, ders materyalleri ve kurumsal iletişim imkanları sunar.

🌟 Temel Özellikler
👨‍🎓 Öğrenciler İçin:
Not Takibi: Sınav sonuçlarını ve ders notlarını görüntüleme

Ders Yönetimi: Kayıtlı derslere ve materyallere erişim

Devamsızlık Takibi: Anlık devamsızlık bilgilerini görüntüleme

Mesajlaşma: Öğretmenlerle doğrudan iletişim

👨‍🏫 Öğretmenler İçin:
Not Yönetimi: Öğrenci notlarını girme ve güncelleme

Sınav Sistemi: Farklı ağırlıklarda değerlendirmeler oluşturma

Yoklama Alma: Öğrenci devamsızlıklarını işaretleme

Duyuru Merkezi: Sınıf/bölümlere duyuru gönderme

🖥️ Yöneticiler İçin:
Kullanıcı Yönetimi: Öğrenci ve öğretmen hesaplarını yönetme

Ders Programlama: Akademik takvim oluşturma

Bölüm Yönetimi: Akademik birimleri düzenleme

Sistem Analitiği: Akademik raporlar oluşturma

🛠️ Teknik Özellikler
Framework: ASP.NET Core 6.0 MVC

Veritabanı: Entity Framework Core ile SQL Server

Kimlik Doğrulama: Rol tabanlı (Öğrenci/Öğretmen/Yönetici)

Arayüz: Duyarlı (responsive) Bootstrap 5 tasarımı

📚 Veritabanı Yapısı
Sistemdeki temel veri modelleri:

Kullanıcılar (Öğrenciler/Öğretmenler)

Dersler

Bölümler

Notlar/Değerlendirmeler

Devamsızlık Kayıtları

Duyurular/Mesajlar

🚀 Başlarken
Depoyu klonlayın

appsettings.json dosyasında veritabanı bağlantısını yapılandırın

Migration'ları çalıştırın: dotnet ef database update

Uygulamayı başlatın: dotnet run

bash
git clone https://github.com/your-repo/obs-system.git
cd obs-system
dotnet restore
dotnet ef database update
dotnet run
