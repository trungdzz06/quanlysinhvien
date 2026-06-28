/*
 * ==============================================================================
 * Tên tệp tin: TeacherController.cs
 * Tổng quan: Module quản lý nhân sự giảng viên trong hệ thống.
 * Cung cấp các chức năng: Xem danh sách, Thêm mới và Chỉnh sửa thông tin giảng viên.
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
	/// LỚP (CLASS): TeacherController
	/// Mục đích: Quản lý hồ sơ giảng viên, phân bổ giảng viên theo khoa và trạng thái công tác.
	/// Quyền truy cập: Chỉ dành cho người dùng có vai trò "Admin".
	/// </summary>
	[Authorize(Roles = "Admin")]
	public class TeacherController : Controller
	{
		private readonly ApplicationDbContext _context;

		public TeacherController(ApplicationDbContext context)
		{
			_context = context;
		}

		// 1. DANH SÁCH: Hiển thị danh sách giảng viên toàn trường kèm bộ lọc theo Khoa và phân trang
		public async Task<IActionResult> Index(string facultyId, string role, int page = 1)
		{
			int pageSize = 10; // Giới hạn hiển thị tối đa 10 giảng viên trên mỗi trang

			// Truy vấn nạp kèm thông tin Khoa (Faculty)
			var teachers = _context.Teachers
				.Include(t => t.Faculty)
				.AsQueryable();

			// Áp dụng lọc theo Khoa nếu tham số facultyId được chọn từ giao diện
			if (!string.IsNullOrEmpty(facultyId))
			{
				teachers = teachers.Where(t => t.FacultyId == facultyId);
			}

			// Tính toán tổng số trang
			int totalItems = await teachers.CountAsync();
			int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			// Ràng buộc số trang hợp lệ
			if (page < 1) page = 1;
			if (page > totalPages && totalPages > 0) page = totalPages;

			// Phân trang dữ liệu ở mức database
			var pagedTeachers = await teachers
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Lưu trạng thái và dữ liệu vào ViewBag để hiển thị trên View
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.FacultyId = facultyId;
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			
			return View(pagedTeachers);
		}

		// 2. TẠO MỚI (GET): Hiển thị form trống cho phép thêm giảng viên mới
		public async Task<IActionResult> Create()
		{
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View();
		}

		// 2. TẠO MỚI (POST): Tiếp nhận thông tin từ form và thực hiện lưu giảng viên mới
		[HttpPost]
		[ValidateAntiForgeryToken] // Chống tấn công giả mạo yêu cầu (CSRF)
		public async Task<IActionResult> Create(Teacher teacher)
		{
			// Kiểm tra các ràng buộc dữ liệu phía Server (Validation)
			if (ModelState.IsValid)
			{
				// Kiểm tra mã giảng viên (TeacherId) đã tồn tại trong DB chưa
				if (await _context.Teachers.AnyAsync(t => t.TeacherId == teacher.TeacherId))
				{
					ModelState.AddModelError("TeacherId", "Mã giảng viên này đã tồn tại trong hệ thống.");
				}
				else
				{
					// Giảng viên mới tạo mặc định ở trạng thái "Active" và cấp mật khẩu mã hóa mặc định
					teacher.Status = "Active";
					teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dainam@123");
					_context.Add(teacher);
					await _context.SaveChangesAsync();

					TempData["Success"] = "Đã thêm giảng viên thành công!";
					return RedirectToAction(nameof(Index));
				}
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(teacher);
		}

		// 3. CHI TIẾT (GET): Chỉ xem thông tin chi tiết của giảng viên (chế độ chỉ đọc)
		[HttpGet]
		public async Task<IActionResult> Info(string id)
		{
			if (id == null) return NotFound();
			// Tìm giảng viên và nạp kèm thông tin Khoa
			var teacher = await _context.Teachers
				.Include(t => t.Faculty)
				.FirstOrDefaultAsync(t => t.TeacherId == id);
			if (teacher == null) return NotFound();

			return View(teacher);
		}

		// 4. CẬP NHẬT (GET): Tải dữ liệu hồ sơ giảng viên hiện tại và hiển thị lên form sửa
		public async Task<IActionResult> Edit(string id)
		{
			var teacher = await _context.Teachers.FindAsync(id);
			if (teacher == null)
			{
				return NotFound();
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(teacher);
		}

		// 5. XÓA (POST): Xác nhận và xóa cứng hồ sơ giảng viên khỏi cơ sở dữ liệu
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken] // Chống tấn công giả mạo yêu cầu (CSRF)
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			var teacher = await _context.Teachers.FindAsync(id);
			if (teacher == null)
			{
				return NotFound();
			}

			// Ràng buộc nghiệp vụ: Giảng viên đang được phân công dạy lớp học phần thì không được xóa
			bool hasClasses = await _context.CourseClasses.AnyAsync(c => c.TeacherId == id);
			if (hasClasses)
			{
				TempData["Error"] = "❌ LỖI: Không thể xóa giảng viên này vì đang được phân công giảng dạy lớp học phần. Vui lòng cập nhật trạng thái thay vì xóa.";
				return RedirectToAction(nameof(Index));
			}

			try
			{
				_context.Teachers.Remove(teacher);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Đã xóa hồ sơ giảng viên thành công!";
			}
			catch (DbUpdateException)
			{
				TempData["Error"] = "❌ LỖI: Ràng buộc cơ sở dữ liệu. Không thể xóa giảng viên này!";
			}

			return RedirectToAction(nameof(Index));
		}

		// 4. CẬP NHẬT (POST): Tiếp nhận dữ liệu chỉnh sửa hồ sơ giảng viên và lưu lại
		[HttpPost]
		[ValidateAntiForgeryToken] // Chống tấn công giả mạo yêu cầu (CSRF)
		public async Task<IActionResult> Edit(string id, Teacher teacher)
		{
			if (id != teacher.TeacherId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Lấy lại PasswordHash cũ từ cơ sở dữ liệu để tránh bị ghi đè thành null do không có trên form
					var existingTeacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.TeacherId == id);
					teacher.PasswordHash = existingTeacher?.PasswordHash;

					_context.Update(teacher);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã cập nhật thông tin giảng viên!";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					throw; // Ném lỗi tranh chấp dữ liệu đồng thời
				}
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(teacher);
		}

		// 6. RESET MẬT KHẨU (POST): Đặt lại mật khẩu mặc định 'Dainam@123' cho giảng viên
		[HttpPost]
		[ValidateAntiForgeryToken] // Chống tấn công giả mạo yêu cầu (CSRF)
		public async Task<IActionResult> ResetPassword(string id)
		{
			var teacher = await _context.Teachers.FindAsync(id);
			if (teacher == null) return NotFound();

			// Cập nhật lại mật khẩu mặc định và Hash lại
			teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dainam@123");
			_context.Update(teacher);
			await _context.SaveChangesAsync();

			TempData["Success"] = $"Đã reset mật khẩu cho GV {teacher.FullName} thành 'Dainam@123'.";
			return RedirectToAction(nameof(Index));
		}
	}
}