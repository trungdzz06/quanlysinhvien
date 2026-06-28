/*
 * ==============================================================================
 * Tên tệp tin: GradeController.cs
 * Tổng quan: Module quản lý điểm số, cho phép Giảng viên và Admin nhập, cập nhật 
 * và tính toán điểm tổng kết tự động cho sinh viên.
 * Tác giả: Nhóm phát triển phần mềm
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using System.Linq;

namespace StudentManagementSystem.Controllers
{
	/// <summary>
	/// LỚP (CLASS): GradeController
	/// Mục đích: Xử lý các nghiệp vụ liên quan đến bảng điểm của sinh viên.
	/// Quyền truy cập: Admin và Teacher (Giảng viên).
	/// </summary>
	[Authorize(Roles = "Admin,Teacher")]
	public class GradeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public GradeController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Entry [GET]
		/// Mục đích: Hiển thị giao diện nhập điểm cho một Lớp học phần cụ thể.
		/// Tham số: courseClassId (Mã lớp học phần).
		/// </summary>
		public async Task<IActionResult> Entry(string courseClassId)
		{
			var username = User.FindFirst("Username")?.Value;
			
			// 1. Tải danh sách Lớp học phần khả dụng cho dropdown
			var ccQuery = _context.CourseClasses
				.Include(c => c.Subject)
				.Include(c => c.Teacher)
				.AsQueryable();

			// Nếu là Giảng viên, chỉ hiển thị các lớp học phần được phân công dạy
			if (User.IsInRole("Teacher"))
			{
				var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == username || t.TeacherId == username);
				if (teacher != null)
				{
					ccQuery = ccQuery.Where(c => c.TeacherId == teacher.TeacherId);
				}
			}

			ViewBag.CourseClasses = await ccQuery.ToListAsync();

			var studentsList = new List<Student>();

			if (!string.IsNullOrEmpty(courseClassId))
			{
				// 2. Tìm thông tin Lớp học phần được chọn
				var courseClass = await _context.CourseClasses
					.Include(c => c.Subject)
					.FirstOrDefaultAsync(c => c.CourseClassId == courseClassId);

				if (courseClass != null)
				{
					// 3. Truy vấn danh sách sinh viên thực tế đã đăng ký hoặc đã chốt ở lớp học phần này
					var studentIds = await _context.Registrations
						.Where(r => r.CourseClassId == courseClassId && (r.Status == "Đã đăng ký" || r.Status == "Đã chốt"))
						.Select(r => r.StudentId)
						.ToListAsync();

					studentsList = await _context.Students
						.Include(s => s.Grades)
						.Where(s => studentIds.Contains(s.MSSV))
						.ToListAsync();

					ViewBag.SelectedCourseClass = courseClassId;
					ViewBag.SelectedSubjectCode = courseClass.SubjectCode;
					ViewBag.SelectedSubjectName = courseClass.Subject?.SubjectName;
					ViewBag.SelectedSemester = courseClass.Semester;
				}
			}

			return View(studentsList);
		}

		/// <summary>
		/// HÀM (METHOD): Save [POST]
		/// Mục đích: Lưu trữ hoặc cập nhật hàng loạt điểm số của sinh viên từ form nhập liệu.
		/// Logic: Thực hiện tính toán điểm tổng kết, quy đổi điểm chữ và tối ưu hóa truy vấn.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Save(List<Grade> grades, string subjectCode, string courseClassId)
		{
			if (grades == null || !grades.Any() || string.IsNullOrEmpty(subjectCode) || string.IsNullOrEmpty(courseClassId))
				return RedirectToAction(nameof(Entry), new { courseClassId = courseClassId });

			var courseClass = await _context.CourseClasses.FirstOrDefaultAsync(c => c.CourseClassId == courseClassId);
			string semester = courseClass?.Semester ?? "N/A";
			int year = DateTime.Now.Year;

			var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectCode == subjectCode);
			string subjectName = subject?.SubjectName ?? "N/A";

			/* * LOGIC PHỨC TẠP: TỐI ƯU HIỆU NĂNG (BULK PROCESSING)
             * 1. Thay vì kiểm tra từng sinh viên trong vòng lặp (gây lỗi N+1 Query), hệ thống lấy
             * toàn bộ điểm cũ của danh sách sinh viên này trong 1 truy vấn duy nhất.
             * 2. Sử dụng .ToDictionaryAsync để chuyển dữ liệu sang cấu trúc Dictionary (Key-Value),
             * giúp việc tra cứu điểm cũ đạt tốc độ O(1), cực nhanh khi xử lý số lượng lớn.
             */
			var studentIds = grades.Select(g => g.StudentId).ToList();
			var existingGrades = await _context.Grades
				.Where(g => g.SubjectCode == subjectCode && studentIds.Contains(g.StudentId) && g.Semester == semester)
				.ToDictionaryAsync(g => g.StudentId);

			foreach (var grade in grades)
			{
				/* * THUẬT TOÁN TÍNH ĐIỂM:
                 * Tổng điểm = Chuyên cần(10%) + Giữa kỳ(30%) + Cuối kỳ(60%).
                 * Kết quả được làm tròn đến 2 chữ số thập phân để đảm bảo độ chính xác.
                 */
				grade.TotalScore = Math.Round(grade.Attendance * 0.1m + grade.Midterm * 0.3m + grade.Final * 0.6m, 2);
				grade.LetterGrade = CalculateLetterGrade(grade.TotalScore);

				grade.SubjectCode = subjectCode;
				grade.SubjectName = subjectName;
				grade.Semester = semester;
				grade.Year = year;

				// Kiểm tra bằng Dictionary: Nếu đã có điểm thì Cập nhật, chưa có thì Thêm mới
				if (existingGrades.TryGetValue(grade.StudentId, out var existing))
				{
					existing.Attendance = grade.Attendance;
					existing.Midterm = grade.Midterm;
					existing.Final = grade.Final;
					existing.TotalScore = grade.TotalScore;
					existing.LetterGrade = grade.LetterGrade;
					existing.SubjectName = grade.SubjectName;
				}
				else
				{
					_context.Grades.Add(grade);
				}
			}

			// Lưu tất cả thay đổi (Insert/Update) vào Database trong một Transaction duy nhất
			await _context.SaveChangesAsync();
			TempData["Success"] = "Đã lưu và cập nhật điểm thành công!";

			return RedirectToAction(nameof(Entry), new { courseClassId = courseClassId });
		}

		/// <summary>
		/// HÀM (METHOD): CalculateLetterGrade (Hàm bổ trợ)
		/// Mục đích: Quy đổi điểm số từ hệ 10 sang thang điểm chữ (A, B, C, D, F).
		/// </summary>
		private string CalculateLetterGrade(decimal score)
		{
			if (score >= 9.0m) return "A";
			if (score >= 8.5m) return "B+";
			if (score >= 8.0m) return "B";
			if (score >= 7.0m) return "C+";
			if (score >= 6.5m) return "C";
			if (score >= 5.5m) return "D+";
			if (score >= 5.0m) return "D";
			return "F";
		}
	}
}