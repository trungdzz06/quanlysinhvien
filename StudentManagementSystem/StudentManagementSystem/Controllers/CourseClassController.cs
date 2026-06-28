/*
 * ==============================================================================
 * Tên tệp tin: CourseClassController.cs
 * Tổng quan: Module quản lý các lớp học phần (Course Class) theo từng học kỳ.
 * Hỗ trợ nghiệp vụ mở lớp, phân công giảng viên và quản lý sĩ số.
 * Tác giả: Nhóm phát triển phần mềm
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

		// 1. DANH SÁCH: Hiển thị danh sách lớp học phần có lọc theo học kỳ và tìm kiếm tương đối
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

		// 2. TẠO MỚI (POST): Tiếp nhận dữ liệu từ form để mở một lớp học phần mới
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CourseClass courseClass)
		{
			if (ModelState.IsValid)
			{
				// Kiểm tra mã lớp học phần đã tồn tại chưa
				bool isDuplicate = await _context.CourseClasses.AnyAsync(c => c.CourseClassId == courseClass.CourseClassId);
				if (isDuplicate)
				{
					ModelState.AddModelError("CourseClassId", $"❌ Mã lớp học phần '{courseClass.CourseClassId}' đã tồn tại trong hệ thống. Vui lòng chọn mã khác!");
				}
				else
				{
					// Kiểm tra Giảng viên và Môn học có thuộc cùng một Khoa hay không
					var subject = await _context.Subjects.FindAsync(courseClass.SubjectCode);
					var teacher = await _context.Teachers.FindAsync(courseClass.TeacherId);
					
					if (subject != null && teacher != null && subject.FacultyId != teacher.FacultyId)
					{
						ModelState.AddModelError("TeacherId", "LỖI NGHIỆP VỤ: Giảng viên được chọn không thuộc khoa quản lý môn học này!");
					}
					else
					{
						// Mặc định sĩ số ban đầu là 0 khi vừa mở lớp
						courseClass.CurrentStudents = 0;
						_context.Add(courseClass);
						await _context.SaveChangesAsync();
						TempData["Success"] = "Đã mở lớp học phần thành công!";
						return RedirectToAction(nameof(Index));
					}
				}
			}

			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(courseClass);
		}

		// 3. CẬP NHẬT (POST): Tiếp nhận thông tin chỉnh sửa của lớp học phần
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
				// Kiểm tra Giảng viên và Môn học có thuộc cùng một Khoa hay không
				var subject = await _context.Subjects.FindAsync(courseClass.SubjectCode);
				var teacher = await _context.Teachers.FindAsync(courseClass.TeacherId);
				
				if (subject != null && teacher != null && subject.FacultyId != teacher.FacultyId)
				{
					ModelState.AddModelError("TeacherId", "LỖI NGHIỆP VỤ: Giảng viên được chọn không thuộc khoa quản lý môn học này!");
				}
				else
				{
					try
					{
						/* * LOGIC PHỨC TẠP: THEO DÕI TRẠNG THÁI THỰC THỂ
						 * EF Core sẽ so sánh dữ liệu cũ và mới để tạo ra câu lệnh UPDATE tối ưu.
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
			}

			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(courseClass);
		}

		// 4. CHI TIẾT (GET): Hiển thị thông tin lớp học phần kèm danh sách sinh viên thực tế đã đăng ký học môn này
		public async Task<IActionResult> Details(string id)
		{
			if (id == null) return NotFound();

			var courseClass = await _context.CourseClasses
				.Include(c => c.Subject)
				.Include(c => c.Teacher)
				.FirstOrDefaultAsync(m => m.CourseClassId == id);
				
			if (courseClass == null) return NotFound();

			// Lấy danh sách đăng ký của lớp này
			var registrations = await _context.Registrations
				.Include(r => r.Student)
				.Where(r => r.CourseClassId == id && (r.Status == "Đã đăng ký" || r.Status == "Đã chốt"))
				.ToListAsync();

			ViewBag.Registrations = registrations;

			return View(courseClass);
		}

		// 5. XÓA (POST): Xóa lớp học phần ra khỏi hệ thống nếu thỏa mãn điều kiện
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			var courseClass = await _context.CourseClasses.FindAsync(id);
			if (courseClass != null)
			{
				// Kiểm tra xem đã có sinh viên nào đăng ký lớp này chưa
				bool hasRegistrations = await _context.Registrations.AnyAsync(r => r.CourseClassId == id);
				if (hasRegistrations)
				{
					TempData["Error"] = "❌ LỖI: Không thể xóa lớp học phần này vì đã có sinh viên đăng ký học!";
					return RedirectToAction(nameof(Index));
				}

				try 
				{
					_context.CourseClasses.Remove(courseClass);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã xóa lớp học phần thành công!";
				}
				catch (DbUpdateException)
				{
					TempData["Error"] = "❌ LỖI: Ràng buộc dữ liệu. Không thể xóa lớp này!";
				}
			}
			return RedirectToAction(nameof(Index));
		}

		// 6. XÓA SINH VIÊN KHỎI LỚP (POST): Admin thực hiện rút tên sinh viên ra khỏi lớp học phần
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveStudent(int registrationId)
		{
			var registration = await _context.Registrations
				.Include(r => r.CourseClass)
				.FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

			if (registration == null)
			{
				TempData["Error"] = "❌ LỖI: Không tìm thấy thông tin đăng ký!";
				return RedirectToAction(nameof(Index));
			}

			string classId = registration.CourseClassId;

			if (registration.CourseClass != null)
			{
				if (registration.CourseClass.CurrentStudents > 0)
				{
					registration.CourseClass.CurrentStudents--;
				}
				if (registration.CourseClass.CurrentStudents < registration.CourseClass.MaxStudents && registration.CourseClass.Status == "Đã đầy")
				{
					registration.CourseClass.Status = "Mở đăng ký";
				}
				_context.Update(registration.CourseClass);
			}

			_context.Registrations.Remove(registration);
			await _context.SaveChangesAsync();

			TempData["Success"] = "Đã xóa sinh viên khỏi lớp học phần thành công!";
			return RedirectToAction(nameof(Details), new { id = classId });
		}

		// 7. TẠO MỚI (GET): Hiển thị form trống mở lớp học phần mới
		public async Task<IActionResult> Create()
		{
			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View();
		}

		// 8. CẬP NHẬT (GET): Tải dữ liệu lớp học phần và hiển thị lên form chỉnh sửa
		public async Task<IActionResult> Edit(string id)
		{
			var courseClass = await _context.CourseClasses.FindAsync(id);
			if (courseClass == null) return NotFound();
			ViewBag.Subjects = await _context.Subjects.ToListAsync();
			ViewBag.Teachers = await _context.Teachers.Where(t => t.Status == "Active").ToListAsync();
			return View(courseClass);
		}

		// 9. XUẤT THỜI KHÓA BIỂU (GET): Xuất file CSV chứa lịch học và thời khóa biểu các lớp theo học kỳ
		[HttpGet]
		public async Task<IActionResult> ExportTKB(string semester)
		{
			var query = _context.CourseClasses
				.Include(c => c.Subject)
				.Include(c => c.Teacher)
				.AsQueryable();

			if (!string.IsNullOrEmpty(semester))
			{
				query = query.Where(c => c.Semester == semester);
			}

			var classes = await query.OrderBy(c => c.Semester).ThenBy(c => c.CourseClassId).ToListAsync();

			var csv = new System.Text.StringBuilder();
			csv.Append('\uFEFF'); // UTF-8 BOM - bắt buộc để Excel hiển thị tiếng Việt đúng
			csv.AppendLine($"THỜI KHÓA BIỂU - {(string.IsNullOrEmpty(semester) ? "Tất cả học kỳ" : semester)}");
			csv.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
			csv.AppendLine();
			csv.AppendLine("Mã Lớp HP,Môn Học,Mã Môn,Giảng Viên,Học Kỳ,Lịch Học,Sĩ Số Tối Đa,Sĩ Số Hiện Tại,Trạng Thái");

			foreach (var c in classes)
			{
				csv.AppendLine($"\"{c.CourseClassId}\",\"{c.Subject?.SubjectName ?? "N/A"}\",\"{c.SubjectCode}\",\"{c.Teacher?.FullName ?? "Chưa phân công"}\",\"{c.Semester}\",\"{c.Schedule ?? "Chưa xếp lịch"}\",{c.MaxStudents},{c.CurrentStudents},\"{c.Status}\"");
			}

			string fileName = $"ThoiKhoaBieu_{(string.IsNullOrEmpty(semester) ? "TatCa" : semester.Replace(" ", "_").Replace("-", ""))}_{DateTime.Now:yyyyMMdd}.csv";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
			return File(buffer, "text/csv; charset=utf-8", fileName);
		}

		// 10. XUẤT DANH SÁCH LỚP (GET): Xuất file CSV chứa danh sách tất cả sinh viên đăng ký trong một lớp học phần
		[HttpGet]
		public async Task<IActionResult> ExportClassStudents(string id)
		{
			if (string.IsNullOrEmpty(id)) return NotFound();

			var courseClass = await _context.CourseClasses
				.Include(c => c.Subject)
				.Include(c => c.Teacher)
				.FirstOrDefaultAsync(c => c.CourseClassId == id);

			if (courseClass == null) return NotFound();

			var registrations = await _context.Registrations
				.Include(r => r.Student)
					.ThenInclude(s => s!.Class)
				.Where(r => r.CourseClassId == id && (r.Status == "Đã đăng ký" || r.Status == "Đã chốt"))
				.OrderBy(r => r.Student!.MSSV)
				.ToListAsync();

			var csv = new System.Text.StringBuilder();
			csv.Append('\uFEFF');
			csv.AppendLine($"DANH SÁCH SINH VIÊN LỚP HỌC PHẦN");
			csv.AppendLine($"Mã lớp: {courseClass.CourseClassId} | Môn học: {courseClass.Subject?.SubjectName} | Giảng viên: {courseClass.Teacher?.FullName}");
			csv.AppendLine($"Học kỳ: {courseClass.Semester} | Lịch học: {courseClass.Schedule} | Tổng SV: {registrations.Count}/{courseClass.MaxStudents}");
			csv.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
			csv.AppendLine();
			csv.AppendLine("STT,Mã Sinh Viên,Họ và Tên,Ngày Sinh,Giới Tính,Lớp Hành Chính,Trạng Thái Đăng Ký,Ngày Đăng Ký");

			int stt = 1;
			foreach (var reg in registrations)
			{
				csv.AppendLine($"{stt},\"{reg.Student?.MSSV}\",\"{reg.Student?.FullName}\",\"{reg.Student?.DateOfBirth:dd/MM/yyyy}\",\"{reg.Student?.Gender}\",\"{reg.Student?.Class?.ClassName ?? "N/A"}\",\"{reg.Status}\",\"{reg.RegistrationDate:dd/MM/yyyy}\"");
				stt++;
			}

			string fileName = $"DanhSachSV_{id}_{DateTime.Now:yyyyMMdd}.csv";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
			return File(buffer, "text/csv; charset=utf-8", fileName);
		}
	}
}