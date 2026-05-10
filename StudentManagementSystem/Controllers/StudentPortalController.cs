/*
 * ==============================================================================
 * Tên tệp tin: StudentPortalController.cs
 * Tổng quan: Module dành riêng cho sinh viên, cho phép xem điểm, thống kê học tập 
 *            và thực hiện đăng ký/hủy học phần trực tuyến.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
	/// <summary>
	/// LỚP (CLASS): StudentPortalController
	/// Mục đích: Cung cấp các tính năng tự phục vụ (self-service) cho sinh viên.
	/// Quyền truy cập: Chỉ dành cho người dùng có vai trò "Student".
	/// </summary>
	[Authorize(Roles = "Student")]
	public class StudentPortalController : Controller
	{
		private readonly ApplicationDbContext _context;

		public StudentPortalController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Trang chủ của cổng thông tin sinh viên, hiển thị tổng quan hồ sơ.
		/// Chức năng: Truy vấn thông tin cá nhân và tính toán các chỉ số học tập tổng quát.
		/// </summary>
		public async Task<IActionResult> Index()
		{
			// Lấy Username từ Claims (đã được lưu khi đăng nhập thành công)
			var username = User.FindFirst("Username")?.Value;

			// Tìm kiếm thông tin sinh viên dựa trên Email hoặc MSSV
			var student = await _context.Students
				.Include(s => s.Grades)
				.Include(s => s.Class)
				.FirstOrDefaultAsync(s => s.Email == username || s.MSSV == username);

			if (student == null) return NotFound();

			/* 
             * LOGIC PHỨC TẠP: THỐNG KÊ ĐIỂM TÍCH LŨY
             * Thuật toán thực hiện tính điểm trung bình (GPA) hệ 10 và hệ 4 ngay tại runtime 
             * để hiển thị kết quả học tập mới nhất cho sinh viên mà không cần lưu trữ tĩnh.
             */
			if (student.Grades != null && student.Grades.Any())
			{
				ViewBag.GPA10 = student.Grades.Average(g => g.TotalScore);
				ViewBag.GPA4 = ConvertToGPA4(ViewBag.GPA10);
				ViewBag.TotalCredits = student.Grades.Sum(g => 3); // Giả lập mỗi môn 3 tín chỉ
				ViewBag.Classification = GetClassification(ViewBag.GPA10);
			}
			else
			{
				ViewBag.GPA10 = 0;
				ViewBag.GPA4 = 0;
				ViewBag.TotalCredits = 0;
				ViewBag.Classification = "Chưa có dữ liệu";
			}

			return View(student);
		}

		/// <summary>
		/// HÀM (METHOD): Grades
		/// Mục đích: Hiển thị bảng điểm chi tiết theo học kỳ.
		/// Tham số: semester (Tên học kỳ muốn xem).
		/// </summary>
		public async Task<IActionResult> Grades(string semester)
		{
			var username = User.FindFirst("Username")?.Value;
			var student = await _context.Students
				.Include(s => s.Grades)
				.FirstOrDefaultAsync(s => s.Email == username || s.MSSV == username);

			if (student == null) return NotFound();

			var grades = student.Grades.AsQueryable();

			// Lọc danh sách điểm theo học kỳ nếu có yêu cầu
			if (!string.IsNullOrEmpty(semester))
			{
				grades = grades.Where(g => g.Semester == semester);
			}

			// Lấy danh sách các học kỳ duy nhất để đổ vào bộ lọc trên giao diện
			ViewBag.Semesters = student.Grades
				.Select(g => g.Semester)
				.Distinct()
				.ToList();

			return View(grades.ToList());
		}

		/// <summary>
		/// HÀM (METHOD): RegisterCourse [POST]
		/// Mục đích: Xử lý nghiệp vụ đăng ký vào một lớp học phần.
		/// Thuật toán: 
		/// 1. Kiểm tra tồn tại lớp học phần.
		/// 2. Kiểm tra sĩ số tối đa của lớp.
		/// 3. Kiểm tra tính trùng lặp (sinh viên đã đăng ký chưa).
		/// 4. Thực hiện Transaction: Lưu bản ghi đăng ký và tăng sĩ số hiện tại của lớp.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RegisterCourse(string courseClassId, string semester)
		{
			var username = User.FindFirst("Username")?.Value;
			var student = await _context.Students
				.FirstOrDefaultAsync(s => s.Email == username || s.MSSV == username);

			if (student == null) return NotFound();

			var courseClass = await _context.CourseClasses.FindAsync(courseClassId);

			/* 
             * LOGIC PHỨC TẠP: RÀNG BUỘC ĐĂNG KÝ HỌC PHẦN
             * Kiểm tra điều kiện "MaxStudents" để ngăn chặn việc đăng ký vượt quá giới hạn lớp học.
             * Sử dụng cơ chế SaveChangesAsync để đảm bảo tính toàn vẹn dữ liệu khi cập nhật đồng thời 2 bảng.
             */
			if (courseClass == null)
			{
				TempData["Error"] = "Lớp học phần không tồn tại!";
				return RedirectToAction(nameof(Register), new { semester });
			}

			if (courseClass.CurrentStudents >= courseClass.MaxStudents)
			{
				TempData["Error"] = "Lớp đã đầy!";
				return RedirectToAction(nameof(Register), new { semester });
			}

			var existing = await _context.Registrations
				.FirstOrDefaultAsync(r => r.StudentId == student.MSSV && r.CourseClassId == courseClassId);

			if (existing != null)
			{
				TempData["Error"] = "Bạn đã đăng ký lớp này rồi!";
				return RedirectToAction(nameof(Register), new { semester });
			}

			var registration = new Registration
			{
				StudentId = student.MSSV,
				CourseClassId = courseClassId,
				RegistrationDate = DateTime.Now,
				Status = "Đã đăng ký",
				Semester = semester,
				Year = DateTime.Now.Year
			};

			courseClass.CurrentStudents++;

			_context.Registrations.Add(registration);
			_context.Update(courseClass);
			await _context.SaveChangesAsync();

			TempData["Success"] = "Đăng ký thành công!";
			return RedirectToAction(nameof(Register), new { semester });
		}

		/// <summary>
		/// HÀM (METHOD): CancelRegistration [POST]
		/// Mục đích: Xử lý nghiệp vụ hủy học phần đã đăng ký.
		/// Logic: Xóa bản ghi đăng ký và giảm sĩ số hiện tại của lớp học phần tương ứng.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CancelRegistration(int registrationId, string semester)
		{
			var registration = await _context.Registrations
				.Include(r => r.CourseClass)
				.FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

			if (registration == null)
			{
				TempData["Error"] = "Không tìm thấy đăng ký!";
				return RedirectToAction(nameof(Register), new { semester });
			}

			registration.CourseClass.CurrentStudents--;

			_context.Registrations.Remove(registration);
			_context.Update(registration.CourseClass);
			await _context.SaveChangesAsync();

			TempData["Success"] = "Đã hủy đăng ký!";
			return RedirectToAction(nameof(Register), new { semester });
		}

		// --- Các phương thức bổ trợ (Helper Methods) ---

		private decimal ConvertToGPA4(decimal gpa10)
		{
			if (gpa10 >= 9.0m) return 4.0m;
			if (gpa10 >= 8.5m) return 3.7m;
			if (gpa10 >= 8.0m) return 3.5m;
			if (gpa10 >= 7.0m) return 3.0m;
			if (gpa10 >= 6.5m) return 2.5m;
			if (gpa10 >= 5.5m) return 2.0m;
			if (gpa10 >= 5.0m) return 1.5m;
			return 0.0m;
		}

		private string GetClassification(decimal gpa10)
		{
			if (gpa10 >= 9.0m) return "Xuất sắc";
			if (gpa10 >= 8.0m) return "Giỏi";
			if (gpa10 >= 6.5m) return "Khá";
			if (gpa10 >= 5.0m) return "Trung bình";
			return "Yếu";
		}

		// Chức năng lấy danh sách đăng ký học phần (Màn hình 14)
		public async Task<IActionResult> Register(string semester = "Học kỳ 1 - 2024")
		{
			var username = User.FindFirst("Username")?.Value;
			var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == username || s.MSSV == username);
			if (student == null) return NotFound();

			var availableClasses = await _context.CourseClasses.Include(c => c.Subject).Include(c => c.Teacher).Where(c => c.Semester == semester).ToListAsync();
			var registeredClasses = await _context.Registrations.Include(r => r.CourseClass).ThenInclude(c => c.Subject).Where(r => r.StudentId == student.MSSV && r.Semester == semester).ToListAsync();

			ViewBag.AvailableClasses = availableClasses;
			ViewBag.RegisteredClasses = registeredClasses;
			ViewBag.TotalCredits = registeredClasses.Sum(r => r.CourseClass.Subject.Credits);
			ViewBag.Semester = semester;
			return View();
		}
	}
}