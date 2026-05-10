/*
 * ==============================================================================
 * Tên tệp tin: CourseClassController.cs
 * Tổng quan: Module quản lý các lớp học phần (Course Class) theo từng học kỳ.
 * Hỗ trợ nghiệp vụ mở lớp, phân công giảng viên và quản lý sĩ số.
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
	/// LỚP (CLASS): CourseClassController
	/// Mục đích: Điều phối các hoạt động liên quan đến vòng đời của một lớp học phần.
	/// Quyền truy cập: Chỉ dành cho tài khoản có quyền "Admin".
	/// </summary>
	[Authorize(Roles = "Admin")]
	public class CourseClassController : Controller
	{
		private readonly ApplicationDbContext _context;

		public CourseClassController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Hiển thị danh sách lớp học phần kèm bộ lọc theo học kỳ và tìm kiếm.
		/// </summary>
		public async Task<IActionResult> Index(string semester, string searchTerm)
		{
			/* * LOGIC PHỨC TẠP: TRUY VẤN LIÊN KẾT ĐA TẦNG (EAGER LOADING)
             * Sử dụng .Include để nạp dữ liệu từ bảng Subject (Môn học) và Teacher (Giảng viên).
             * Điều này giúp tránh lỗi "Lazy Loading" khi truy cập các thuộc tính liên quan 
             * ở phía View, từ đó giảm số lượng truy vấn thừa (N+1 query problem).
             */
			var classes = _context.CourseClasses
				.Include(c => c.Subject)
				.Include(c => c.Teacher)
				.AsQueryable();

			if (!string.IsNullOrEmpty(semester))
			{
				classes = classes.Where(c => c.Semester == semester);
			}

			if (!string.IsNullOrEmpty(searchTerm))
			{
				classes = classes.Where(c => c.CourseClassId.Contains(searchTerm) ||
											c.Subject.SubjectName.Contains(searchTerm));
			}

			/* * LOGIC PHỨC TẠP: TRUY VẤN DISTINCT (DUY NHẤT)
             * Lấy danh sách các học kỳ hiện có trong Database để đổ vào bộ lọc, 
             * giúp quản trị viên dễ dàng chuyển đổi qua lại giữa các kỳ học.
             */
			ViewBag.Semesters = await _context.CourseClasses
				.Select(c => c.Semester)
				.Distinct()
				.ToListAsync();

			return View(await classes.ToListAsync());
		}

		/// <summary>
		/// HÀM (METHOD): Create [POST]
		/// Mục đích: Khởi tạo một lớp học phần mới vào hệ thống.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CourseClass courseClass)
		{
			if (ModelState.IsValid)
			{
				// Mặc định sĩ số ban đầu là 0 khi vừa mở lớp
				courseClass.CurrentStudents = 0;
				_context.Add(courseClass);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Đã mở lớp học phần thành công!";
				return RedirectToAction(nameof(Index));
			}

			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(courseClass);
		}

		/// <summary>
		/// HÀM (METHOD): Edit [POST]
		/// Mục đích: Cập nhật thông tin lớp học phần (đổi giảng viên, thay đổi sĩ số tối đa).
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, CourseClass courseClass)
		{
			if (id != courseClass.CourseClassId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					/* * LOGIC PHỨC TẠP: THEO DÕI TRẠNG THÁI THỰC THỂ
                     * EF Core sẽ so sánh dữ liệu cũ và mới để tạo ra câu lệnh UPDATE tối ưu.
                     * Nếu có xung đột về dữ liệu (Concurrency), hệ thống sẽ ném ra ngoại lệ 
                     * để bảo vệ tính toàn vẹn của dữ liệu.
                     */
					_context.Update(courseClass);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã cập nhật lớp học phần!";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					throw; // Chuyển tiếp lỗi để hệ thống xử lý tập trung
				}
			}

			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(courseClass);
		}

		/// <summary>
		/// HÀM (METHOD): Delete [POST]
		/// Mục đích: Xóa lớp học phần khỏi hệ thống.
		/// Lưu ý: Cần đảm bảo không có sinh viên nào đang đăng ký lớp này trước khi xóa (Ràng buộc FK).
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			var courseClass = await _context.CourseClasses.FindAsync(id);
			if (courseClass != null)
			{
				_context.CourseClasses.Remove(courseClass);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Đã xóa lớp học phần thành công!";
			}
			return RedirectToAction(nameof(Index));
		}

		// --- Các hàm phụ trợ hỗ trợ hiển thị giao diện (GET) ---
		public async Task<IActionResult> Create()
		{
			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View();
		}

		public async Task<IActionResult> Edit(string id)
		{
			var courseClass = await _context.CourseClasses.FindAsync(id);
			if (courseClass == null) return NotFound();
			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(courseClass);
		}
	}
}