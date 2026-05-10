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

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Hiển thị danh sách giảng viên toàn trường kèm bộ lọc theo Khoa.
		/// Tham số: facultyId (Lọc theo đơn vị công tác), role (Lọc theo chức vụ - nếu cần mở rộng).
		/// </summary>
		public async Task<IActionResult> Index(string facultyId, string role)
		{
			/* * LOGIC PHỨC TẠP: TRUY VẤN LINQ KÈM LIÊN KẾT (EAGER LOADING)
             * Sử dụng .Include(t => t.Faculty) để nạp trước thông tin Khoa của giảng viên. 
             * Điều này giúp tối ưu hóa số lượng câu lệnh SQL khi hiển thị tên Khoa trên danh sách, 
             * ngăn chặn lỗi hiệu năng (N+1 Select problem).
             */
			var teachers = _context.Teachers
				.Include(t => t.Faculty)
				.AsQueryable();

			// Áp dụng lọc theo Khoa nếu tham số facultyId được truyền từ giao diện
			if (!string.IsNullOrEmpty(facultyId))
			{
				teachers = teachers.Where(t => t.FacultyId == facultyId);
			}

			// Lấy danh sách Khoa đổ vào ViewBag để hiển thị trên thẻ <select> ở View
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(await teachers.ToListAsync());
		}

		/// <summary>
		/// HÀM (METHOD): Create [GET]
		/// Mục đích: Hiển thị form trống để khởi tạo hồ sơ giảng viên mới.
		/// </summary>
		public async Task<IActionResult> Create()
		{
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View();
		}

		/// <summary>
		/// HÀM (METHOD): Create [POST]
		/// Mục đích: Tiếp nhận thông tin từ form, gán trạng thái mặc định và lưu vào CSDL.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Teacher teacher)
		{
			// Kiểm tra các ràng buộc dữ liệu (Required, Email, Phone...) từ Model
			if (ModelState.IsValid)
			{
				// Logic nghiệp vụ: Giảng viên mới tạo mặc định sẽ ở trạng thái "Active" (Đang công tác)
				teacher.Status = "Active";
				_context.Add(teacher);
				await _context.SaveChangesAsync();

				// TempData: Thông báo thành công chỉ hiển thị một lần sau khi Redirect
				TempData["Success"] = "Đã thêm giảng viên thành công!";
				return RedirectToAction(nameof(Index));
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(teacher);
		}

		/// <summary>
		/// HÀM (METHOD): Edit [GET]
		/// Mục đích: Truy xuất thông tin giảng viên hiện tại để chỉnh sửa.
		/// Tham số: id (Mã giảng viên).
		/// </summary>
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

		/// <summary>
		/// HÀM (METHOD): Edit [POST]
		/// Mục đích: Cập nhật các thay đổi trong hồ sơ giảng viên.
		/// Logic: Xử lý lỗi đồng thời (Concurrency) để đảm bảo tính toàn vẹn dữ liệu.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
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
					_context.Update(teacher);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã cập nhật thông tin giảng viên!";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					/* * LOGIC PHỨC TẠP: XỬ LÝ LỖI TRUY CẬP ĐỒNG THỜI
                     * Xảy ra khi có hai Admin cùng cập nhật hồ sơ của một giảng viên 
                     * tại cùng một thời điểm. Hệ thống sẽ ném lỗi để tránh ghi đè dữ liệu sai lệch.
                     */
					throw;
				}
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(teacher);
		}
	}
}