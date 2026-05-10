/*
 * ==============================================================================
 * Tên tệp tin: StudentController.cs
 * Tổng quan: Module quản trị thông tin sinh viên dành cho người điều hành hệ thống.
 *            Cung cấp các chức năng Xem danh sách, Chi tiết, Cập nhật và Xóa.
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
	/// LỚP (CLASS): StudentController
	/// Mục đích: Quản lý toàn bộ vòng đời dữ liệu của Sinh viên trong hệ thống.
	/// Chức năng: Tiếp nhận và xử lý các yêu cầu CRUD từ giao diện quản trị.
	/// Quyền truy cập: Chỉ tài khoản có vai trò "Admin" mới được phép thực thi.
	/// </summary>
	[Authorize(Roles = "Admin")]
	public class StudentController : Controller
	{
		// Biến cục bộ: Lưu trữ ngữ cảnh cơ sở dữ liệu để thực hiện các truy vấn.
		private readonly ApplicationDbContext _context;

		public StudentController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Hiển thị danh sách sinh viên kèm bộ lọc tìm kiếm.
		/// Tham số: searchTerm (Từ khóa tìm kiếm theo MSSV/Tên), classFilter (Mã lớp để lọc).
		/// Giá trị trả về: View kèm danh sách sinh viên đã được lọc.
		/// </summary>
		public async Task<IActionResult> Index(string searchTerm, string classFilter)
		{
			/* 
             * LOGIC PHỨC TẠP: XÂY DỰNG TRUY VẤN ĐỘNG (QUERY BUILDING)
             * Sử dụng .AsQueryable() để trì hoãn việc thực thi câu lệnh SQL. 
             * Các điều kiện lọc (Where) sẽ được cộng dồn và chỉ thực hiện một truy vấn duy nhất 
             * xuống cơ sở dữ liệu khi gọi .ToListAsync(), giúp tối ưu hóa hiệu năng.
             */
			var students = _context.Students
				.Include(s => s.Class)
				.Include(s => s.Faculty)
				.AsQueryable();

			// Tìm kiếm theo mã số hoặc tên nếu có nhập từ khóa
			if (!string.IsNullOrEmpty(searchTerm))
			{
				students = students.Where(s => s.MSSV.Contains(searchTerm) ||
											  s.FullName.Contains(searchTerm));
			}

			// Lọc theo lớp học cụ thể nếu có chọn
			if (!string.IsNullOrEmpty(classFilter))
			{
				students = students.Where(s => s.ClassId == classFilter);
			}

			// Chuẩn bị dữ liệu danh sách lớp cho dropdown list trên giao diện
			ViewBag.Classes = await _context.Classes.ToListAsync();
			return View(await students.ToListAsync());
		}

		/// <summary>
		/// HÀM (METHOD): Detail [GET]
		/// Mục đích: Hiển thị thông tin chi tiết của một sinh viên để xem hoặc chỉnh sửa.
		/// Tham số: id (Mã số sinh viên - MSSV).
		/// </summary>
		public async Task<IActionResult> Detail(string id)
		{
			if (id == null)
			{
				return NotFound();
			}

			// Truy vấn sinh viên kèm theo thông tin Lớp và Khoa liên kết (Eager Loading)
			var student = await _context.Students
				.Include(s => s.Class)
				.Include(s => s.Faculty)
				.FirstOrDefaultAsync(m => m.MSSV == id);

			if (student == null)
			{
				return NotFound();
			}

			// Cung cấp dữ liệu danh mục để hiển thị trong các ô chọn (Combobox)
			ViewBag.Classes = await _context.Classes.ToListAsync();
			ViewBag.Faculties = await _context.Faculties.ToListAsync();

			return View(student);
		}

		/// <summary>
		/// HÀM (METHOD): Detail [POST]
		/// Mục đích: Tiếp nhận dữ liệu cập nhật và lưu vào cơ sở dữ liệu.
		/// Logic: Kiểm tra tính hợp lệ của Model và xử lý xung đột dữ liệu.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Detail(string id, Student student)
		{
			if (id != student.MSSV)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					/* 
                     * LOGIC PHỨC TẠP: CẬP NHẬT VÀ XỬ LÝ ĐỒNG THỜI (CONCURRENCY)
                     * Sử dụng khối try-catch để bắt lỗi DbUpdateConcurrencyException. 
                     * Trường hợp có hai người cùng sửa một bản ghi tại một thời điểm, 
                     * hệ thống sẽ kiểm tra sự tồn tại của sinh viên để đưa ra phản hồi phù hợp.
                     */
					_context.Update(student);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã lưu thông tin sinh viên thành công!";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!StudentExists(student.MSSV))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
			}

			ViewBag.Classes = await _context.Classes.ToListAsync();
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(student);
		}

		/// <summary>
		/// HÀM (METHOD): Delete [POST]
		/// Mục đích: Xóa bỏ bản ghi sinh viên khỏi hệ thống.
		/// Bảo mật: Sử dụng ValidateAntiForgeryToken để chống tấn công giả mạo (CSRF).
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			var student = await _context.Students.FindAsync(id);
			if (student != null)
			{
				// Thực hiện lệnh xóa bản ghi
				_context.Students.Remove(student);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Đã xóa sinh viên thành công!";
			}

			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// Hàm bổ trợ (Helper): Kiểm tra nhanh sự tồn tại của sinh viên theo ID.
		/// </summary>
		private bool StudentExists(string id)
		{
			return _context.Students.Any(e => e.MSSV == id);
		}
	}
}