/*
 * ==============================================================================
 * Tên tệp tin: AccountController.cs
 * Tác giả: Nhóm phát triển phần mềm
 * Ngày tạo/sửa đổi: 05/05/2026
 * Chức năng: Điều phối luồng Đăng nhập, Đăng ký và Phân quyền người dùng.
 * ==============================================================================
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using System.Security.Claims;

namespace StudentManagementSystem.Controllers
{
	[Authorize(Roles = "Admin")] // Chỉ Admin mới vào được các chức năng quản lý tài khoản
	public class AccountController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AccountController(ApplicationDbContext context) => _context = context;

		[AllowAnonymous]
		[HttpGet]
		public IActionResult Register() => View();

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.Email);
			var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == model.Email);

			if (existingStudent != null || existingTeacher != null)
			{
				ModelState.AddModelError("Email", "Email này đã được sử dụng");
				return View(model);
			}

			string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

			if (model.Role == "Student")
			{
				var newStudent = new Student
				{
					MSSV = "SV" + Math.Abs(DateTime.Now.Ticks).ToString().Substring(0, 8),
					FullName = model.FullName,
					Email = model.Email,
					PasswordHash = passwordHash,
					Status = "Đang học",
					DateOfBirth = DateTime.Now.AddYears(-20),
					Gender = "Nam"
				};
				_context.Students.Add(newStudent);
			}
			else if (model.Role == "Teacher")
			{
				var newTeacher = new Teacher
				{
					TeacherId = "GV" + Math.Abs(DateTime.Now.Ticks).ToString().Substring(0, 6),
					FullName = model.FullName,
					Email = model.Email,
					PasswordHash = passwordHash,
					Status = "Active"
				};
				_context.Teachers.Add(newTeacher);
			}

			await _context.SaveChangesAsync();
			TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập.";
			return RedirectToAction(nameof(Login));
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult Login() => View();

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			// Kiểm tra Sinh viên
			var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.Username || s.MSSV == model.Username);
			if (student != null && BCrypt.Net.BCrypt.Verify(model.Password, student.PasswordHash))
			{
				await SignInUser(student.FullName, "Student", student.MSSV);
				return RedirectToAction("Index", "StudentPortal");
			}

			// Kiểm tra Giảng viên
			var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == model.Username || t.TeacherId == model.Username);
			if (teacher != null && BCrypt.Net.BCrypt.Verify(model.Password, teacher.PasswordHash))
			{
				await SignInUser(teacher.FullName, "Teacher", teacher.TeacherId);
				return RedirectToAction("Dashboard", "Home");
			}

			// Tài khoản Admin hệ thống (Hardcoded hoặc lấy từ bảng cấu hình)
			if (model.Username == "admin@dainam.edu.vn" && model.Password == "Admin@123")
			{
				await SignInUser("Quản trị viên", "Admin", "ADMIN01");
				return RedirectToAction("Dashboard", "Home");
			}

			ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
			return View(model);
		}

		private async Task SignInUser(string name, string role, string userId)
		{
			var claims = new List<Claim> {
			new Claim(ClaimTypes.Name, name),
			new Claim(ClaimTypes.Role, role),
			new Claim("Username", userId)
		};
			var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
		}
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> UpdateOldPasswords()
        {
            // 1. Dùng thư viện BCrypt tạo ra 1 mã Hash chuẩn cho chữ "Dainam@123"
            string defaultHash = BCrypt.Net.BCrypt.HashPassword("Dainam@123");

            // 2. Quét toàn bộ Sinh viên chưa có mật khẩu (PasswordHash bị NULL)
            var students = await _context.Students
                                         .Where(s => string.IsNullOrEmpty(s.PasswordHash))
                                         .ToListAsync();
            foreach (var student in students)
            {
                student.PasswordHash = defaultHash;
            }

            // 3. Quét toàn bộ Giảng viên chưa có mật khẩu
            var teachers = await _context.Teachers
                                         .Where(t => string.IsNullOrEmpty(t.PasswordHash))
                                         .ToListAsync();
            foreach (var teacher in teachers)
            {
                teacher.PasswordHash = defaultHash;
            }

            // 4. Lưu toàn bộ thay đổi xuống SQL Server
            await _context.SaveChangesAsync();

            // In thông báo ra màn hình
            return Content($"✅ QUÉT THÀNH CÔNG! Đã cấp mật khẩu 'Dainam@123' cho {students.Count} Sinh viên và {teachers.Count} Giảng viên cũ.");
        }
        [AllowAnonymous]
        [HttpPost]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Login");
		}
	}
}