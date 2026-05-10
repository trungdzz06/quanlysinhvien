/*
 * ==============================================================================
 * Tên tệp tin: AccountController.cs
 * Tổng quan: Module quản lý tài khoản người dùng, điều hướng đăng nhập và đăng ký.
 * Tác giả: Nhóm 4
 * Ngày tạo: 05/05/2026
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
	/// <summary>
	/// LỚP (CLASS): AccountController
	/// Mục đích: Xử lý các yêu cầu HTTP liên quan đến định danh người dùng.
	/// Chức năng: Đóng vai trò là Controller điều phối các hành động Login, Register và Logout.
	/// </summary>
	public class AccountController : Controller
	{
		// Biến cục bộ: Lưu trữ kết nối với cơ sở dữ liệu để truy xuất thông tin SV/GV.
		private readonly ApplicationDbContext _context;

		public AccountController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Register [POST]
		/// Mục đích: Tiếp nhận dữ liệu từ form để tạo tài khoản mới.
		/// Tham số: RegisterViewModel (Chứa thông tin đăng ký).
		/// Giá trị trả về: Chuyển hướng đến Login nếu thành công hoặc quay lại View kèm thông báo lỗi.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				/* 
                 * LOGIC PHỨC TẠP: KIỂM TRA ĐA BẢNG
                 * Hệ thống thực hiện truy vấn trên cả hai thực thể Student và Teacher để 
                 * đảm bảo tính duy nhất của Email trên toàn hệ thống trước khi tạo mới.
                 */
				var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.Email);
				var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == model.Email);

				if (existingStudent != null || existingTeacher != null)
				{
					ModelState.AddModelError("Email", "Email này đã được sử dụng");
					return View(model);
				}

				// Cấu trúc thuật toán: Tự động phát sinh mã định danh (MSSV/GV) dựa trên Ticks thời gian.
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

		/// <summary>
		/// HÀM (METHOD): Login [POST]
		/// Mục đích: Xác thực thông tin người dùng và thiết lập phiên làm việc (Session).
		/// Cấu trúc: Sử dụng Cookie Authentication để duy trì trạng thái đăng nhập.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			if (ModelState.IsValid)
			{
				/* 
                 * LOGIC PHỨC TẠP: MOCK AUTHENTICATION & PHÂN QUYỀN (CLAIMS)
                 * Thuật toán này phân tích chuỗi Username để gán quyền (Role) tương ứng.
                 * Dữ liệu sau đó được đóng gói vào các "Claims" - một dạng chứng chỉ số 
                 * lưu trong Cookie để hệ thống nhận diện quyền truy cập ở các trang sau.
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

		/// <summary>
		/// HÀM (METHOD): RedirectToLocal
		/// Mục đích: Kiểm tra tính an toàn của URL chuyển hướng.
		/// Logic: Ngăn chặn lỗ hổng bảo mật Open Redirect bằng cách chỉ cho phép điều hướng nội bộ.
		/// </summary>
		private IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
			return RedirectToAction("Dashboard", "Home");
		}

		// --- Các hàm đơn giản khác ---
		[HttpGet] public IActionResult Login(string returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(); }
		[HttpGet] public IActionResult Register() => View();
		[HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return RedirectToAction(nameof(Login)); }
	}
}