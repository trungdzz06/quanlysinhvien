/*
 * ==============================================================================
 * Tên tệp tin: AccountController.cs
 * Tác giả: Nhóm phát triển phần mềm
 * Ngày tạo/sửa đổi: 05/05/2026
 * Chức năng: Điều phối luồng Đăng nhập, Đăng ký và Phân quyền người dùng.
 * ==============================================================================
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
	public class AccountController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AccountController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Login(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		[HttpGet]
		public IActionResult Register() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				/* 
                 * LOGIC PHỨC TẠP 1: KIỂM TRA TÍNH DUY NHẤT CỦA EMAIL TRÊN ĐA THỰC THỂ
                 * Vì tài khoản được tách thành bảng Sinh viên (Student) và Giảng viên (Teacher), 
                 * cần truy vấn song song để đảm bảo email không bị trùng lặp giữa các vai trò khác nhau.
                 */
				var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.Email);
				var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == model.Email);

				if (existingStudent != null || existingTeacher != null)
				{
					ModelState.AddModelError("Email", "Email này đã được sử dụng");
					return View(model);
				}

				if (model.Role == "Student")
				{
					var newStudent = new Student
					{
						MSSV = "SV" + DateTime.Now.Ticks.ToString().Substring(0, 8),
						FullName = model.FullName,
						Email = model.Email,
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
						TeacherId = "GV" + DateTime.Now.Ticks.ToString().Substring(0, 6),
						FullName = model.FullName,
						Email = model.Email,
						Status = "Active"
					};
					_context.Teachers.Add(newTeacher);
				}

				await _context.SaveChangesAsync();
				TempData["Success"] = "Đăng ký thành công!";
				return RedirectToAction(nameof(Login));
			}
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			if (ModelState.IsValid)
			{
				/* 
                 * LOGIC PHỨC TẠP 2: PHÂN QUYỀN GIẢ LẬP (MOCK) & THIẾT LẬP ĐỊNH DANH (CLAIMS)
                 * Hệ thống nhận diện quyền (Role) thông qua từ khóa trong Username để gán quyền tương ứng.
                 * Dữ liệu sau đó được đóng gói thành các "Claims" để lưu vào Cookie xác thực, 
                 * giúp duy trì phiên làm việc và phân quyền truy cập ở các trang khác.
                 */
				string role = "Student";
				string userName = "Sinh Viên";

				if (model.Username.Contains("admin")) { role = "Admin"; userName = "Admin"; }
				else if (model.Username.Contains("gv")) { role = "Teacher"; userName = "Giảng Viên"; }

				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, userName),
					new Claim(ClaimTypes.Role, role),
					new Claim("Username", model.Username)
				};

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

				await HttpContext.SignInAsync(
					CookieAuthenticationDefaults.AuthenticationScheme,
					new ClaimsPrincipal(claimsIdentity),
					new AuthenticationProperties { IsPersistent = model.RememberMe });

				return RedirectToLocal(returnUrl);
			}
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction(nameof(Login));
		}

		/* 
         * LOGIC PHỨC TẠP 3: BẢO MẬT CHUYỂN HƯỚNG (LOCAL REDIRECT)
         * Ngăn chặn lỗ hổng "Open Redirect" bằng cách kiểm tra đường dẫn trả về.
         * Chỉ cho phép chuyển hướng nếu URL thuộc nội bộ website, tránh bị hacker lợi dụng 
         * để điều hướng người dùng sang các trang web độc hại sau khi đăng nhập.
         */
		private IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
			return RedirectToAction("Dashboard", "Home");
		}
	}
}