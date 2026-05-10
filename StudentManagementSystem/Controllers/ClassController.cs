/*
 * ==============================================================================
 * Tên tệp tin: ClassController.cs
 * Tổng quan: Module quản lý các lớp hành chính (Lớp sinh viên) trong hệ thống.
 * Cung cấp các chức năng: Xem danh sách, Lọc theo khoa, Tạo mới, Sửa và Xóa.
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
	/// LỚP (CLASS): ClassController
	/// Mục đích: Điều hướng và xử lý các thao tác quản trị đối với thực thể Lớp học (Class).
	/// Quyền truy cập: Chỉ dành cho người dùng có vai trò "Admin".
	/// </summary>
	[Authorize(Roles = "Admin")]
	public class ClassController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ClassController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Hiển thị danh sách các lớp học có trong hệ thống kèm bộ lọc.
		/// Tham số: facultyId (Lọc theo khoa), searchTerm (Tìm kiếm theo mã hoặc tên lớp).
		/// </summary>
		public async Task<IActionResult> Index(string facultyId, string searchTerm)
		{
			/* * LOGIC PHỨC TẠP: XÂY DỰNG TRUY VẤN LINQ ĐỘNG
             * 1. Sử dụng .Include(c => c.Faculty) để thực hiện Eager Loading, giúp lấy thông tin 
             * tên khoa ngay trong một lần truy vấn duy nhất thay vì truy vấn lẻ tẻ (N+1 problem).
             * 2. .AsQueryable() cho phép hệ thống cộng dồn các điều kiện Where mà chưa thực thi ngay, 
             * giúp tối ưu hóa SQL phát sinh khi gửi xuống Database.
             */
			var classes = _context.Classes
				.Include(c => c.Faculty)
				.AsQueryable();

			// Áp dụng lọc theo Khoa nếu người dùng chọn
			if (!string.IsNullOrEmpty(facultyId))
			{
				classes = classes.Where(c => c.FacultyId == facultyId);
			}

			// Áp dụng tìm kiếm theo từ khóa (Mã lớp hoặc Tên lớp)
			if (!string.IsNullOrEmpty(searchTerm))
			{
				classes = classes.Where(c => c.ClassId.Contains(searchTerm) ||
											c.ClassName.Contains(searchTerm));
			}

			// Đổ dữ liệu danh sách Khoa vào ViewBag để hiển thị trên Dropdown lọc ở giao diện
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(await classes.ToListAsync());
		}

		/// <summary>
		/// HÀM (METHOD): Edit [GET]
		/// Mục đích: Lấy dữ liệu lớp học cụ thể và hiển thị lên form chỉnh sửa.
		/// Tham số: id (Mã lớp cần sửa).
		/// </summary>
		public async Task<IActionResult> Edit(string id)
		{
			if (id == null) return NotFound();

			var classModel = await _context.Classes.FindAsync(id);
			if (classModel == null) return NotFound();

			// Cung cấp dữ liệu danh mục Khoa và Giảng viên để chọn lại nếu cần
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(classModel);
		}

		/// <summary>
		/// HÀM (METHOD): Edit [POST]
		/// Mục đích: Lưu các thay đổi của lớp học sau khi người dùng chỉnh sửa.
		/// Logic: Kiểm tra tính hợp lệ và xử lý lỗi xung đột dữ liệu.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, Class classModel)
		{
			if (id != classModel.ClassId) return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(classModel);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã cập nhật lớp thành công!";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					/* * LOGIC PHỨC TẠP: XỬ LÝ LỖI ĐỒNG THỜI (CONCURRENCY)
                     * Nếu trong quá trình lưu, lớp học bị xóa bởi một admin khác hoặc có lỗi 
                     * kết nối, hệ thống sẽ kiểm tra sự tồn tại thực tế của ID để phản hồi lỗi 404.
                     */
					if (!_context.Classes.Any(e => e.ClassId == classModel.ClassId)) return NotFound();
					else throw;
				}
			}
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(classModel);
		}

		/// <summary>
		/// HÀM (METHOD): Create [GET]
		/// Mục đích: Hiển thị form trống để tạo lớp học mới.
		/// </summary>
		public async Task<IActionResult> Create()
		{
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			// Lọc danh sách giảng viên đang hoạt động để gán làm chủ nhiệm lớp
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View();
		}

		/// <summary>
		/// HÀM (METHOD): Create [POST]
		/// Mục đích: Tiếp nhận dữ liệu lớp mới từ form và lưu vào CSDL.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Class classModel)
		{
			if (ModelState.IsValid)
			{
				_context.Add(classModel);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Đã thêm lớp hành chính thành công!";
				return RedirectToAction(nameof(Index));
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(classModel);
		}

		/// <summary>
		/// HÀM (METHOD): Delete [POST]
		/// Mục đích: Xóa lớp học khỏi hệ thống.
		/// Bảo mật: Sử dụng Anti-Forgery Token để chống lại các cuộc tấn công CSRF.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			var classModel = await _context.Classes.FindAsync(id);
			if (classModel != null)
			{
				_context.Classes.Remove(classModel);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Đã xóa lớp thành công!";
			}

			return RedirectToAction(nameof(Index));
		}
	}
}