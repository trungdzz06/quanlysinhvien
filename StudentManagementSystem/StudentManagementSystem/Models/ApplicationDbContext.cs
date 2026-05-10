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
				new Subject { SubjectCode = "TRR_01", SubjectName = "Toán rời rạc", Credits = 2, FacultyId = "CB" }
			);

			// --- Seed Teachers: Hồ sơ giảng viên quản lý theo đơn vị Khoa ---
			modelBuilder.Entity<Teacher>().HasData(
				new Teacher { TeacherId = "GV_001", FullName = "Phạm Thị Tố Nga", FacultyId = "CNTT", Email = "ptnga@dainam.edu.vn", Status = "Active" },
				new Teacher { TeacherId = "GV_002", FullName = "Nguyễn Văn A", FacultyId = "CNTT", Email = "nva@dainam.edu.vn", Status = "Active" },
				new Teacher { TeacherId = "GV_003", FullName = "Trần Bảo Châu", FacultyId = "CNTT", Email = "tbc@dainam.edu.vn", Status = "Active" }
			);

			// --- Seed CourseClasses: Các lớp học phần mở trong học kỳ cụ thể ---
			modelBuilder.Entity<CourseClass>().HasData(
				new CourseClass { CourseClassId = "LHP_CNPM_01", SubjectCode = "CNPM_01", TeacherId = "GV_001", Semester = "Học kỳ 1 - 2024", MaxStudents = 50, CurrentStudents = 45, Status = "Mở đăng ký", Schedule = "Thứ 3 (Ca 1)" },
				new CourseClass { CourseClassId = "LHP_CSDL_02", SubjectCode = "CSDL_01", TeacherId = "GV_002", Semester = "Học kỳ 1 - 2024", MaxStudents = 50, CurrentStudents = 50, Status = "Đã đầy", Schedule = "Thứ 5 (Ca 2)" },
				new CourseClass { CourseClassId = "LHP_LTW_01", SubjectCode = "LTW_01", TeacherId = "GV_003", Semester = "Học kỳ 1 - 2024", MaxStudents = 40, CurrentStudents = 0, Status = "Chờ duyệt", Schedule = "Thứ 2 (Ca 3)" }
			);

			// --- Seed Classes: Lớp hành chính để quản lý sinh viên theo khóa ---
			modelBuilder.Entity<Class>().HasData(
				new Class { ClassId = "CNTT18-06", ClassName = "CNTT 18-06", FacultyId = "CNTT" },
				new Class { ClassId = "CNTT18-07", ClassName = "CNTT 18-07", FacultyId = "CNTT" }
			);

			// --- Seed Students: Thông tin sinh viên mẫu để kiểm thử chức năng ---
			modelBuilder.Entity<Student>().HasData(
				new Student { MSSV = "18710205", FullName = "Nguyễn Quang Trung", DateOfBirth = new DateTime(2000, 2, 20), Gender = "Nam", Hometown = "Hà Nội", ClassId = "CNTT18-06", FacultyId = "CNTT", Email = "trung@xxx.edu", Status = "Đang học" },
				new Student { MSSV = "18710204", FullName = "Đào Ngọc Nam", DateOfBirth = new DateTime(2000, 5, 15), Gender = "Nam", Hometown = "Hà Nội", ClassId = "CNTT18-06", FacultyId = "CNTT", Email = "nam@xxx.edu", Status = "Đang học" }
			);
		}
	}
}