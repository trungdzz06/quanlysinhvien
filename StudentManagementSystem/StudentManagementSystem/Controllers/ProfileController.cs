using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using System.Security.Claims;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue("Username");

            if (userId == null) return RedirectToAction("Login", "Account");

            var model = new ProfileViewModel
            {
                UserId = userId,
                Role = role
            };

            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.MSSV == userId);
                if (student != null)
                {
                    model.FullName = student.FullName;
                    model.Email = student.Email;
                }
            }
            else if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == userId);
                if (teacher != null)
                {
                    model.FullName = teacher.FullName;
                    model.Email = teacher.Email;
                }
            }
            else if (role == "Admin")
            {
                model.FullName = User.FindFirstValue(ClaimTypes.Name) ?? "Quản trị viên";
                model.Email = "admin@dainam.edu.vn";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue("Username");

            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.UserId = userId;
                model.Role = role;
                // Reload FullName/Email in case of validation error
                if (role == "Student")
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.MSSV == userId);
                    if (student != null) { model.FullName = student.FullName; model.Email = student.Email; }
                }
                else if (role == "Teacher")
                {
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == userId);
                    if (teacher != null) { model.FullName = teacher.FullName; model.Email = teacher.Email; }
                }
                else if (role == "Admin")
                {
                    model.FullName = User.FindFirstValue(ClaimTypes.Name) ?? "Quản trị viên";
                    model.Email = "admin@dainam.edu.vn";
                }
                return View(model);
            }

            // Logic thay đổi mật khẩu
            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.MSSV == userId);
                if (student != null)
                {
                    if (string.IsNullOrEmpty(student.PasswordHash) || BCrypt.Net.BCrypt.Verify(model.OldPassword, student.PasswordHash))
                    {
                        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        _context.Update(student);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Đổi mật khẩu thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng.");
                    }
                }
            }
            else if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == userId);
                if (teacher != null)
                {
                    if (string.IsNullOrEmpty(teacher.PasswordHash) || BCrypt.Net.BCrypt.Verify(model.OldPassword, teacher.PasswordHash))
                    {
                        teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        _context.Update(teacher);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Đổi mật khẩu thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng.");
                    }
                }
            }
            else if (role == "Admin")
            {
                // Mật khẩu Admin hiện đang hardcode trong AccountController, không lưu DB
                ModelState.AddModelError("", "Tài khoản Admin hệ thống không thể đổi mật khẩu qua giao diện này.");
            }

            model.UserId = userId;
            model.Role = role;
            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.MSSV == userId);
                if (student != null) { model.FullName = student.FullName; model.Email = student.Email; }
            }
            else if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == userId);
                if (teacher != null) { model.FullName = teacher.FullName; model.Email = teacher.Email; }
            }
            else if (role == "Admin")
            {
                model.FullName = User.FindFirstValue(ClaimTypes.Name) ?? "Quản trị viên";
                model.Email = "admin@dainam.edu.vn";
            }
            return View(model);
        }
    }
}
