/*
 * ==============================================================================
 * Tên tệp tin: SubjectController.cs
 * Tổng quan: Module quản lý danh mục môn học (Subjects) trong chương trình đào tạo.
 * Cung cấp các chức năng CRUD: Xem danh sách, Thêm, Sửa và Xóa môn học.
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
	/// LỚP (CLASS): SubjectController
	/// Mục đích: Quản lý thông tin các môn học trong hệ thống, bao gồm mã môn, tên môn và số tín chỉ.
	/// Quyền truy cập: Chỉ dành cho tài khoản có vai trò "Admin".
	/// </summary>
	[Authorize(Roles = "Admin")]
	public class SubjectController : Controller
	{
		private readonly ApplicationDbContext _context;

		public SubjectController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Hiển thị danh sách toàn bộ môn học, hỗ trợ lọc theo Khoa và tìm kiếm theo tên/mã.
		/// </summary>
		public async Task<IActionResult> Index(string facultyId, string searchTerm)
		{
			/* * LOGIC PHỨC TẠP: TRUY VẤN LINQ ĐỘNG (DYNAMIC LINQ)
             * Sử dụng AsQueryable để xây dựng câu lệnh SQL tối ưu. Các điều kiện lọc 
             * chỉ được thực thi dưới Database khi gọi .ToListAsync() ở cuối hàm.
             */
			var subjects = _context.Subjects
				.Include(s => s.Faculty)
				.AsQueryable();

			// Lọc môn học theo đơn vị quản lý (Khoa)
			if (!string.IsNullOrEmpty(facultyId))
			{
				subjects = subjects.Where(s => s.FacultyId == facultyId);
			}

			// Tìm kiếm tương đối theo Tên môn học hoặc Mã môn học
			if (!string.IsNullOrEmpty(searchTerm))
			{
				subjects = subjects.Where(s => s.SubjectName.Contains(searchTerm) ||
											  s.SubjectCode.Contains(searchTerm));
			}

			// Lấy danh sách Khoa để đổ vào Dropdown lọc trên giao diện
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			ViewBag.CurrentFacultyId = facultyId;
			ViewBag.CurrentSearchTerm = searchTerm;
			return View(await subjects.ToListAsync());
		}

		/// <summary>
		/// HÀM (METHOD): Create [GET]
		/// Mục đích: Hiển thị form để Admin nhập thông tin môn học mới.
		/// </summary>
		public async Task<IActionResult> Create()
		{
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View();
		}

		/// <summary>
		/// HÀM (METHOD): Create [POST]
		/// Mục đích: Kiểm tra dữ liệu và lưu môn học mới vào cơ sở dữ liệu.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Subject subject)
		{
			// Kiểm tra các ràng buộc dữ liệu (Validation) được định nghĩa trong Model
			if (ModelState.IsValid)
			{
				if (await _context.Subjects.AnyAsync(s => s.SubjectCode == subject.SubjectCode))
				{
					ModelState.AddModelError("SubjectCode", "Mã môn học này đã tồn tại trong hệ thống.");
				}
				else
				{
					_context.Add(subject);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã thêm môn học thành công!";
					return RedirectToAction(nameof(Index));
				}
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(subject);
		}

		/// <summary>
		/// HÀM (METHOD): Edit [GET]
		/// Mục đích: Lấy dữ liệu môn học hiện tại để Admin chỉnh sửa.
		/// </summary>
		public async Task<IActionResult> Edit(string id)
		{
			var subject = await _context.Subjects.FindAsync(id);
			if (subject == null)
			{
				return NotFound();
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(subject);
		}

		/// <summary>
		/// HÀM (METHOD): Edit [POST]
		/// Mục đích: Cập nhật các thay đổi của môn học và xử lý lỗi đồng thời.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, Subject subject)
		{
			if (id != subject.SubjectCode)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(subject);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã cập nhật môn học!";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					/* * LOGIC PHỨC TẠP: DbUpdateConcurrencyException
                     * Xử lý trường hợp dữ liệu bị thay đổi đồng thời bởi một tác vụ khác 
                     * trong khi phiên làm việc hiện tại đang diễn ra.
                     */
					throw;
				}
			}

			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(subject);
		}

		/// <summary>
		/// HÀM (METHOD): Delete [POST]
		/// Mục đích: Xóa môn học khỏi hệ thống.
		/// Bảo mật: Sử dụng ValidateAntiForgeryToken để chống tấn công giả mạo yêu cầu (CSRF).
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			var subject = await _context.Subjects.FindAsync(id);
			if (subject != null)
			{
				// LOGIC NGHIỆP VỤ: Kiểm tra xem môn học đã được mở lớp chưa
				bool isUsed = await _context.CourseClasses.AnyAsync(c => c.SubjectCode == id);
				if (isUsed)
				{
					TempData["Error"] = "❌ LỖI: Không thể xóa môn học này vì đã có lớp học phần được mở. Vui lòng xóa các lớp học phần trước!";
					return RedirectToAction(nameof(Index));
				}

				try
				{
					_context.Subjects.Remove(subject);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã xóa môn học thành công!";
				}
				catch (DbUpdateException)
				{
					TempData["Error"] = "❌ LỖI: Ràng buộc cơ sở dữ liệu. Không thể xóa môn học này!";
				}
			}

			return RedirectToAction(nameof(Index));
		}
	}
}