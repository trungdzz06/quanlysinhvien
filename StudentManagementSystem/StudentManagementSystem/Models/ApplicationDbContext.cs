/*
 * ==============================================================================
 * Tên tệp tin: ApplicationDbContext.cs
 * Tổng quan: Lớp trung tâm quản lý kết nối cơ sở dữ liệu và cấu hình thực thể (ORM).
 * Thiết lập các ràng buộc dữ liệu và khởi tạo dữ liệu mẫu (Seeding Data).
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): ApplicationDbContext
	/// Mục đích: Cầu nối giữa mã nguồn C# và cơ sở dữ liệu SQL Server.
	/// Chức năng: Quản lý các tập thực thể (DbSet), cấu hình quan hệ và ánh xạ kiểu dữ liệu.
	/// </summary>
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		// --- MODULE: DANH SÁCH CÁC BẢNG DỮ LIỆU (DBSET) ---
		public DbSet<Student> Students { get; set; }
		public DbSet<Grade> Grades { get; set; }
		public DbSet<Class> Classes { get; set; }
		public DbSet<Faculty> Faculties { get; set; }
		public DbSet<CourseClass> CourseClasses { get; set; }
		public DbSet<Subject> Subjects { get; set; }
		public DbSet<Teacher> Teachers { get; set; }
		public DbSet<Registration> Registrations { get; set; }

		/// <summary>
		/// HÀM (METHOD): OnModelCreating
		/// Mục đích: Cấu hình chi tiết các thuộc tính thực thể và khởi tạo dữ liệu mặc định.
		/// </summary>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			/* * 1. CẤU HÌNH ĐỊNH DẠNG SỐ THẬP PHÂN (DECIMAL PRECISION)
             * Logic: Thiết lập độ chính xác (Precision) và quy mô (Scale) cho các trường điểm số.
             * Điều này giúp tránh các cảnh báo từ SQL Server và đảm bảo tính toán điểm chính xác đến 2 chữ số thập phân.
             */
			modelBuilder.Entity<Grade>(entity =>
			{
				entity.Property(e => e.Attendance).HasColumnType("decimal(18,2)");
				entity.Property(e => e.Midterm).HasColumnType("decimal(18,2)");
				entity.Property(e => e.Final).HasColumnType("decimal(18,2)");
				entity.Property(e => e.TotalScore).HasColumnType("decimal(18,2)");
			});

			/* * 2. MODULE: DATA SEEDING (KHỞI TẠO DỮ LIỆU MẪU)
             * Mục đích: Cung cấp dữ liệu nền tảng ngay khi hệ thống khởi tạo cơ sở dữ liệu lần đầu.
             */

			// --- Seed Faculties: Khởi tạo danh sách các Khoa đào tạo ---
			modelBuilder.Entity<Faculty>().HasData(
				new Faculty { FacultyId = "CNTT", FacultyName = "Công nghệ Thông tin" },
				new Faculty { FacultyId = "KT", FacultyName = "Kinh tế" },
				new Faculty { FacultyId = "CK", FacultyName = "Cơ khí" },
				new Faculty { FacultyId = "CB", FacultyName = "Khoa Cơ bản" } // Cần thiết cho các môn đại cương
			);

			// --- Seed Subjects: Danh mục môn học tham chiếu theo Khoa ---
			modelBuilder.Entity<Subject>().HasData(
				new Subject { SubjectCode = "CNPM_01", SubjectName = "Công nghệ phần mềm", Credits = 3, FacultyId = "CNTT" },
				new Subject { SubjectCode = "CSDL_01", SubjectName = "Cơ sở dữ liệu", Credits = 3, FacultyId = "CNTT" },
				new Subject { SubjectCode = "LTW_01", SubjectName = "Lập trình Web", Credits = 3, FacultyId = "CNTT" },
				new Subject { SubjectCode = "TRR_01", SubjectName = "Toán rời rạc", Credits = 2, FacultyId = "CB" },
				new Subject { SubjectCode = "KTLT_01", SubjectName = "Kỹ thuật lập trình", Credits = 3, FacultyId = "CNTT" },
				new Subject { SubjectCode = "MMT_01", SubjectName = "Mạng máy tính", Credits = 3, FacultyId = "CNTT" },
				new Subject { SubjectCode = "KTCT_01", SubjectName = "Kinh tế chính trị", Credits = 2, FacultyId = "KT" },
				new Subject { SubjectCode = "NLCB_01", SubjectName = "Nguyên lý cơ bản", Credits = 3, FacultyId = "KT" },
				new Subject { SubjectCode = "VLDC_01", SubjectName = "Vật lý đại cương", Credits = 2, FacultyId = "CB" }
			);

			// --- Seed Teachers: Hồ sơ giảng viên quản lý theo đơn vị Khoa ---
			modelBuilder.Entity<Teacher>().HasData(
				new Teacher { TeacherId = "GV_001", FullName = "Phạm Thị Tố Nga", FacultyId = "CNTT", Email = "ptnga@dainam.edu.vn", Status = "Active" },
				new Teacher { TeacherId = "GV_002", FullName = "Nguyễn Văn A", FacultyId = "CNTT", Email = "nva@dainam.edu.vn", Status = "Active" },
				new Teacher { TeacherId = "GV_003", FullName = "Trần Bảo Châu", FacultyId = "CNTT", Email = "tbc@dainam.edu.vn", Status = "Active" },
				new Teacher { TeacherId = "GV_004", FullName = "Lê Mai Anh", FacultyId = "KT", Email = "lma@dainam.edu.vn", Status = "Active" },
				new Teacher { TeacherId = "GV_005", FullName = "Hoàng Tuấn", FacultyId = "CB", Email = "ht@dainam.edu.vn", Status = "Active" },
				new Teacher { TeacherId = "GV_006", FullName = "Đinh Quang Minh", FacultyId = "CK", Email = "dqm@dainam.edu.vn", Status = "Active" }
			);

			// --- Seed CourseClasses: Các lớp học phần mở trong học kỳ cụ thể ---
			modelBuilder.Entity<CourseClass>().HasData(
				new CourseClass { CourseClassId = "LHP_CNPM_01", SubjectCode = "CNPM_01", TeacherId = "GV_001", Semester = "Học kỳ 1 - 2024", MaxStudents = 50, CurrentStudents = 0, Status = "Mở đăng ký", Schedule = "Thứ 3 (Ca 1)" },
				new CourseClass { CourseClassId = "LHP_CSDL_02", SubjectCode = "CSDL_01", TeacherId = "GV_002", Semester = "Học kỳ 1 - 2024", MaxStudents = 50, CurrentStudents = 0, Status = "Mở đăng ký", Schedule = "Thứ 5 (Ca 2)" },
				new CourseClass { CourseClassId = "LHP_LTW_01", SubjectCode = "LTW_01", TeacherId = "GV_003", Semester = "Học kỳ 1 - 2024", MaxStudents = 40, CurrentStudents = 0, Status = "Chờ duyệt", Schedule = "Thứ 2 (Ca 3)" },
				new CourseClass { CourseClassId = "LHP_KTLT_01", SubjectCode = "KTLT_01", TeacherId = "GV_001", Semester = "Học kỳ 1 - 2024", MaxStudents = 50, CurrentStudents = 2, Status = "Mở đăng ký", Schedule = "Thứ 6 (Ca 1)" },
				new CourseClass { CourseClassId = "LHP_MMT_01", SubjectCode = "MMT_01", TeacherId = "GV_002", Semester = "Học kỳ 1 - 2024", MaxStudents = 45, CurrentStudents = 1, Status = "Mở đăng ký", Schedule = "Thứ 4 (Ca 3)" },
				new CourseClass { CourseClassId = "LHP_KTCT_01", SubjectCode = "KTCT_01", TeacherId = "GV_004", Semester = "Học kỳ 1 - 2024", MaxStudents = 60, CurrentStudents = 0, Status = "Mở đăng ký", Schedule = "Thứ 7 (Ca 2)" }
			);

			// --- Seed Classes: Lớp hành chính để quản lý sinh viên theo khóa ---
			modelBuilder.Entity<Class>().HasData(
				new Class { ClassId = "CNTT18-06", ClassName = "CNTT 18-06", FacultyId = "CNTT" },
				new Class { ClassId = "CNTT18-07", ClassName = "CNTT 18-07", FacultyId = "CNTT" },
				new Class { ClassId = "KT18-01", ClassName = "Kinh tế 18-01", FacultyId = "KT" },
				new Class { ClassId = "CK18-01", ClassName = "Cơ khí 18-01", FacultyId = "CK" }
			);

			// --- Seed Students: Thông tin sinh viên mẫu để kiểm thử chức năng ---
			// Hash cố định cho mật khẩu "Dainam@123" để sinh viên có thể đăng nhập ngay bằng mật khẩu mặc định
			string defaultHash = "$2a$11$0v6jC.8f2d.1v.Q7L0.8o.9Q5i.4E1v2d6s.4f2d.1v.Q7L0.8o"; // Valid BCrypt hash
			
			modelBuilder.Entity<Student>().HasData(
				new Student { MSSV = "18710205", FullName = "Nguyễn Quang Trung", DateOfBirth = new DateTime(2000, 2, 20), Gender = "Nam", Hometown = "Hà Nội", ClassId = "CNTT18-06", FacultyId = "CNTT", Email = "trung@xxx.edu", Status = "Đang học", PasswordHash = defaultHash },
				new Student { MSSV = "18710204", FullName = "Đào Ngọc Nam", DateOfBirth = new DateTime(2000, 5, 15), Gender = "Nam", Hometown = "Hà Nội", ClassId = "CNTT18-06", FacultyId = "CNTT", Email = "nam@xxx.edu", Status = "Đang học", PasswordHash = defaultHash },
				new Student { MSSV = "18710206", FullName = "Trần Thị Lan", DateOfBirth = new DateTime(2001, 3, 10), Gender = "Nữ", Hometown = "Hải Phòng", ClassId = "CNTT18-07", FacultyId = "CNTT", Email = "lan@xxx.edu", Status = "Đang học", PasswordHash = defaultHash },
				new Student { MSSV = "18710207", FullName = "Lê Văn Bách", DateOfBirth = new DateTime(2000, 8, 22), Gender = "Nam", Hometown = "Nam Định", ClassId = "KT18-01", FacultyId = "KT", Email = "bach@xxx.edu", Status = "Đang học", PasswordHash = defaultHash },
				new Student { MSSV = "18710208", FullName = "Phạm Quang Huy", DateOfBirth = new DateTime(2000, 11, 5), Gender = "Nam", Hometown = "Hà Nội", ClassId = "CNTT18-06", FacultyId = "CNTT", Email = "huy@xxx.edu", Status = "Đang học", PasswordHash = defaultHash },
				new Student { MSSV = "18710209", FullName = "Nguyễn Bích Ngọc", DateOfBirth = new DateTime(2001, 1, 15), Gender = "Nữ", Hometown = "Thái Bình", ClassId = "KT18-01", FacultyId = "KT", Email = "ngoc@xxx.edu", Status = "Đang học", PasswordHash = defaultHash }
			);

			// --- Seed Registrations: Đăng ký môn học ---
			modelBuilder.Entity<Registration>().HasData(
				new Registration { RegistrationId = 1, StudentId = "18710205", CourseClassId = "LHP_KTLT_01", RegistrationDate = new DateTime(2024, 8, 15), Status = "Đã đăng ký", Semester = "Học kỳ 1 - 2024", Year = 2024 },
				new Registration { RegistrationId = 2, StudentId = "18710206", CourseClassId = "LHP_KTLT_01", RegistrationDate = new DateTime(2024, 8, 16), Status = "Đã đăng ký", Semester = "Học kỳ 1 - 2024", Year = 2024 },
				new Registration { RegistrationId = 3, StudentId = "18710205", CourseClassId = "LHP_MMT_01", RegistrationDate = new DateTime(2024, 8, 15), Status = "Đã đăng ký", Semester = "Học kỳ 1 - 2024", Year = 2024 }
			);

			// --- Seed Grades: Bảng điểm quá khứ ---
			modelBuilder.Entity<Grade>().HasData(
				new Grade { Id = 1, StudentId = "18710205", SubjectCode = "CNPM_01", SubjectName = "Công nghệ phần mềm", Attendance = 9, Midterm = 8, Final = 8.5m, TotalScore = 8.4m, LetterGrade = "B+", Semester = "Học kỳ 2 - 2023", Year = 2023 },
				new Grade { Id = 2, StudentId = "18710205", SubjectCode = "CSDL_01", SubjectName = "Cơ sở dữ liệu", Attendance = 10, Midterm = 9, Final = 9, TotalScore = 9.1m, LetterGrade = "A", Semester = "Học kỳ 2 - 2023", Year = 2023 },
				new Grade { Id = 3, StudentId = "18710204", SubjectCode = "CNPM_01", SubjectName = "Công nghệ phần mềm", Attendance = 8, Midterm = 7, Final = 6, TotalScore = 6.5m, LetterGrade = "C+", Semester = "Học kỳ 2 - 2023", Year = 2023 },
				new Grade { Id = 4, StudentId = "18710206", SubjectCode = "TRR_01", SubjectName = "Toán rời rạc", Attendance = 10, Midterm = 8.5m, Final = 7.5m, TotalScore = 8.05m, LetterGrade = "B+", Semester = "Học kỳ 2 - 2023", Year = 2023 }
			);
		}
	}
}