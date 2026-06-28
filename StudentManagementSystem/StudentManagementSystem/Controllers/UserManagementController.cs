/*
 * ==============================================================================
 * Tên tệp tin: UserManagementController.cs
 * Tổng quan: Điều khiển trung tâm cho việc giám sát tài khoản (Sinh viên & Giảng viên).
 * Tác giả: Nhóm 4
 * Ngày sửa đổi: 26/05/2026
 * Phiên bản: v2
 * ==============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using System.Security.Claims;

namespace StudentManagementSystem.Controllers
{
    // [Authorize(Roles = "Admin")]: Rào chắn bảo mật, ngăn chặn mọi người dùng không có quyền Admin.
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH: Hiển thị danh sách tất cả các tài khoản bao gồm cả Sinh viên và Giảng viên
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ dữ liệu Sinh viên và ánh xạ (project) sang ViewModel chung (UserRoleViewModel)
            var students = await _context.Students.Select(s => new UserRoleViewModel
            {
                UserId = s.MSSV,
                FullName = s.FullName,
                Email = s.Email,
                Role = "Student",
                Status = s.Status
            }).ToListAsync();

            // Lấy toàn bộ dữ liệu Giảng viên và ánh xạ sang ViewModel chung (UserRoleViewModel)
            var teachers = await _context.Teachers.Select(t => new UserRoleViewModel
            {
                UserId = t.TeacherId,
                FullName = t.FullName,
                Email = t.Email,
                Role = "Teacher",
                Status = t.Status
            }).ToListAsync();

            // Gộp cả 2 danh sách sinh viên và giảng viên lại thành một danh sách tài khoản duy nhất để hiển thị lên bảng
            var allUsers = students.Concat(teachers).ToList();

            return View(allUsers);
        }

        // 2. CẬP NHẬT VAI TRÒ (POST): Chuyển đổi vai trò của tài khoản giữa Sinh viên và Giảng viên
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công giả mạo yêu cầu (CSRF)
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            // Kiểm tra tính hợp lệ của tham số đầu vào
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                TempData["Error"] = "Dữ liệu yêu cầu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Tìm kiếm đối tượng người dùng ở cả 2 bảng Sinh viên và Giảng viên kèm theo các liên kết tương ứng
            var student = await _context.Students
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.MSSV == userId);

            var teacher = await _context.Teachers
                .Include(t => t.CourseClasses)
                .Include(t => t.HomeRoomClasses)
                .FirstOrDefaultAsync(t => t.TeacherId == userId);

            // Nếu không tìm thấy người dùng này trong hệ thống, trả về thông báo lỗi
            if (student == null && teacher == null)
            {
                TempData["Error"] = $"Không tìm thấy người dùng với mã: {userId}.";
                return RedirectToAction(nameof(Index));
            }

            // Xác định vai trò hiện tại của tài khoản
            string currentRole = student != null ? "Student" : "Teacher";

            // Nếu vai trò mới trùng với vai trò hiện tại, không cần thay đổi
            if (currentRole == newRole)
            {
                TempData["Success"] = $"Người dùng {userId} đã ở vai trò {newRole}.";
                return RedirectToAction(nameof(Index));
            }

            // Ngăn chặn việc cấp quyền Quản trị viên (Admin) thông qua phương thức này để bảo mật
            if (newRole == "Admin")
            {
                TempData["Error"] = "Không thể gán vai trò Quản trị viên (Admin) cho tài khoản này.";
                return RedirectToAction(nameof(Index));
            }

            // TRƯỜNG HỢP 1: Chuyển đổi từ Sinh viên sang Giảng viên
            if (currentRole == "Student" && newRole == "Teacher")
            {
                // Kiểm tra xem sinh viên đã đăng ký học phần nào chưa
                var hasRegistrations = await _context.Registrations.AnyAsync(r => r.StudentId == userId);
                // Ràng buộc nghiệp vụ: Sinh viên đã có điểm hoặc đã đăng ký học phần thì không được chuyển vai trò
                if ((student.Grades != null && student.Grades.Any()) || hasRegistrations)
                {
                    TempData["Error"] = $"Không thể chuyển vai trò: Sinh viên {student.FullName} đã có điểm số hoặc đã đăng ký học phần!";
                    return RedirectToAction(nameof(Index));
                }

                // Tạo đối tượng Giảng viên mới sao chép thông tin từ Sinh viên cũ
                var newTeacher = new Teacher
                {
                    // Tạo mã giảng viên mới (GV + phần số cũ hoặc sinh ngẫu nhiên theo ticks)
                    TeacherId = "GV" + (userId.StartsWith("SV") && userId.Length > 2 ? userId.Substring(2) : Math.Abs(DateTime.Now.Ticks).ToString().Substring(0, 6)),
                    FullName = student.FullName,
                    Email = student.Email,
                    Phone = student.Phone,
                    PasswordHash = student.PasswordHash,
                    FacultyId = student.FacultyId,
                    Status = "Active"
                };

                // Tránh trùng lặp mã giảng viên nếu có xung đột xảy ra
                if (await _context.Teachers.AnyAsync(t => t.TeacherId == newTeacher.TeacherId))
                {
                    newTeacher.TeacherId = "GV" + Math.Abs(DateTime.Now.Ticks).ToString().Substring(0, 6);
                }

                // Thêm giảng viên mới và xóa sinh viên cũ khỏi DbContext
                _context.Teachers.Add(newTeacher);
                _context.Students.Remove(student);
            }
            // TRƯỜNG HỢP 2: Chuyển đổi từ Giảng viên sang Sinh viên
            else if (currentRole == "Teacher" && newRole == "Student")
            {
                // Ràng buộc nghiệp vụ: Giảng viên đang phụ trách giảng dạy hoặc chủ nhiệm lớp thì không được chuyển vai trò
                if ((teacher.CourseClasses != null && teacher.CourseClasses.Any()) || 
                    (teacher.HomeRoomClasses != null && teacher.HomeRoomClasses.Any()))
                {
                    TempData["Error"] = $"Không thể chuyển vai trò: Giảng viên {teacher.FullName} đang phụ trách lớp học phần hoặc lớp chủ nhiệm!";
                    return RedirectToAction(nameof(Index));
                }

                // Tạo đối tượng Sinh viên mới sao chép từ Giảng viên cũ
                var newStudent = new Student
                {
                    // Tạo mã sinh viên mới (SV + phần số cũ hoặc sinh ngẫu nhiên theo ticks)
                    MSSV = "SV" + (userId.StartsWith("GV") && userId.Length > 2 ? userId.Substring(2) : Math.Abs(DateTime.Now.Ticks).ToString().Substring(0, 8)),
                    FullName = teacher.FullName,
                    Email = teacher.Email,
                    Phone = teacher.Phone,
                    PasswordHash = teacher.PasswordHash,
                    FacultyId = teacher.FacultyId,
                    Status = "Đang học",
                    DateOfBirth = DateTime.Now.AddYears(-20),
                    Gender = "Nam"
                };

                // Tránh trùng lặp mã sinh viên nếu có xung đột xảy ra
                if (await _context.Students.AnyAsync(s => s.MSSV == newStudent.MSSV))
                {
                    newStudent.MSSV = "SV" + Math.Abs(DateTime.Now.Ticks).ToString().Substring(0, 8);
                }

                // Thêm sinh viên mới và xóa giảng viên cũ khỏi DbContext
                _context.Students.Add(newStudent);
                _context.Teachers.Remove(teacher);
            }

            // Thực hiện lưu các thay đổi vai trò (thêm/xóa) xuống cơ sở dữ liệu
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã chuyển đổi quyền cho người dùng {userId} thành {newRole} thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}