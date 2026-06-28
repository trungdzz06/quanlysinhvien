/*
 * ==============================================================================
 * Tên tệp tin: StudentController.cs
 * Tổng quan: Điều khiển vòng đời tài khoản sinh viên (CRUD) và bảo mật.
 * Phiên bản: v2
 * Tác giả: Nhóm 4
 * Ngày sửa đổi: 26/05/2026
 * ==============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using BCrypt.Net;

namespace StudentManagementSystem.Controllers
{
    // [Authorize] là lớp rào chắn đầu tiên: Chỉ Admin có quyền truy cập toàn bộ Action trong class này.
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH: Sử dụng IQueryable để tối ưu hóa truy vấn SQL (chỉ thực thi khi cần).
        public async Task<IActionResult> Index(string searchTerm, string classFilter, int page = 1)
        {
            int pageSize = 10; // Giới hạn 10 sinh viên mỗi trang để tối ưu hiển thị

            var students = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Faculty)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                students = students.Where(s => s.MSSV.Contains(searchTerm) || s.FullName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(classFilter))
            {
                students = students.Where(s => s.ClassId == classFilter);
            }

            // Tính toán tổng số trang
            int totalItems = await students.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Xử lý lỗi nếu người dùng cố tình nhập số trang không hợp lệ trên URL
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Thực thi truy vấn phân trang: Skip() bỏ qua dữ liệu cũ, Take() lấy dữ liệu mới
            var pagedStudents = await students
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.ClassFilter = classFilter;
            ViewBag.Classes = await _context.Classes.ToListAsync();
            
            return View(pagedStudents);
        }

        // 2. TẠO MỚI: Cấp tài khoản với cơ chế bảo mật Password Hashing.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Classes = await _context.Classes.ToListAsync();
            ViewBag.Faculties = await _context.Faculties.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF Protection: Chặn các yêu cầu giả mạo từ website lạ.
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra sự tồn tại của khóa chính để tránh lỗi xung đột trong DB.
                if (await _context.Students.AnyAsync(s => s.MSSV == student.MSSV))
                {
                    ModelState.AddModelError("MSSV", "Mã số sinh viên này đã tồn tại.");
                }
                else
                {
                    // LOGIC QUAN TRỌNG: Mã hóa mật khẩu bằng BCrypt.
                    // Không bao giờ lưu mật khẩu ở dạng plain-text vào database.
                    student.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dainam@123");

                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã cấp tài khoản cho SV: {student.FullName}.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Classes = await _context.Classes.ToListAsync();
            ViewBag.Faculties = await _context.Faculties.ToListAsync();
            return View(student);
        }

        // 3. CẬP NHẬT: Kỹ thuật bảo toàn dữ liệu ẩn (Hidden Data Persistence).
        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            ViewBag.Classes = await _context.Classes.ToListAsync();
            ViewBag.Faculties = await _context.Faculties.ToListAsync();
            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Info(string id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.MSSV == id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Detail(string id, Student student)
        {
            if (id != student.MSSV) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // GIẢI THÍCH LOGIC PHỨC TẠP:
                    // Khi update qua form, trường PasswordHash không tồn tại trong form gửi lên.
                    // Nếu dùng _context.Update(student) trực tiếp, trường này sẽ bị set thành null.
                    // .AsNoTracking() giúp lấy bản ghi cũ từ DB ra để sao chép lại PasswordHash cũ.
                    var existingStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.MSSV == id);
                    student.PasswordHash = existingStudent?.PasswordHash;

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật hồ sơ sinh viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.MSSV)) return NotFound();
                    else throw;
                }
            }
            return View(student);
        }

        // 4. XÓA: Xóa bỏ dữ liệu dựa trên khóa chính.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                // LOGIC NGHIỆP VỤ (Business Logic): Xử lý liên kết trước khi xóa sinh viên
                // 1. Giảm sĩ số (CurrentStudents) trong các lớp học phần đã đăng ký
                var registrations = await _context.Registrations.Where(r => r.StudentId == id).Include(r => r.CourseClass).ToListAsync();
                foreach (var reg in registrations)
                {
                    if (reg.CourseClass != null && reg.CourseClass.CurrentStudents > 0)
                    {
                        reg.CourseClass.CurrentStudents--;
                        
                        // Tự động chuyển trạng thái nếu lớp đang "Đã đầy"
                        if (reg.CourseClass.CurrentStudents < reg.CourseClass.MaxStudents && reg.CourseClass.Status == "Đã đầy")
                        {
                            reg.CourseClass.Status = "Mở đăng ký";
                        }
                        
                        _context.Update(reg.CourseClass);
                    }
                }
                _context.Registrations.RemoveRange(registrations);

                // 2. Xóa các bản ghi điểm (Grades) liên quan
                var grades = await _context.Grades.Where(g => g.StudentId == id).ToListAsync();
                _context.Grades.RemoveRange(grades);

                // 3. Xóa sinh viên
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sinh viên và các dữ liệu liên kết thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. RESET MẬT KHẨU: Chức năng hỗ trợ Admin xử lý khi SV quên thông tin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            // Cập nhật lại mật khẩu mặc định và Hash lại.
            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dainam@123");
            _context.Update(student);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã reset mật khẩu cho SV {student.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// HÀM (METHOD): ExportStudents
        /// Mục đích: Xuất file CSV danh sách sinh viên, hỗ trợ lọc theo lớp / từ khóa tìm kiếm.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportStudents(string searchTerm, string classFilter)
        {
            var students = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Faculty)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                students = students.Where(s => s.MSSV.Contains(searchTerm) || s.FullName.Contains(searchTerm));

            if (!string.IsNullOrEmpty(classFilter))
                students = students.Where(s => s.ClassId == classFilter);

            var list = await students.OrderBy(s => s.ClassId).ThenBy(s => s.MSSV).ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.Append('\uFEFF'); // UTF-8 BOM
            csv.AppendLine("DANH SÁCH SINH VIÊN");
            csv.AppendLine($"Bộ lọc: {(string.IsNullOrEmpty(classFilter) ? "Tất cả lớp" : classFilter)} | Từ khóa: {(string.IsNullOrEmpty(searchTerm) ? "(không)" : searchTerm)}");
            csv.AppendLine($"Tổng số: {list.Count} sinh viên | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine();
            csv.AppendLine("STT,Mã Sinh Viên,Họ và Tên,Ngày Sinh,Giới Tính,Lớp,Khoa,Email,Trạng Thái");

            int stt = 1;
            foreach (var s in list)
            {
                csv.AppendLine($"{stt},\"{s.MSSV}\",\"{s.FullName}\",\"{s.DateOfBirth:dd/MM/yyyy}\",\"{s.Gender}\",\"{s.Class?.ClassName ?? "N/A"}\",\"{s.Faculty?.FacultyName ?? "N/A"}\",\"{s.Email}\",\"{s.Status}\"");
                stt++;
            }

            string fileName = $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(buffer, "text/csv; charset=utf-8", fileName);
        }

        private bool StudentExists(string id) => _context.Students.Any(e => e.MSSV == id);
    }
}