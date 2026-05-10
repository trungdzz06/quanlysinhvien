/*
 * ==============================================================================
 * Tên tệp tin: ReportController.cs
 * Tổng quan: Module quản lý và tổng hợp báo cáo kết quả học tập của sinh viên.
 *            Hỗ trợ lọc dữ liệu theo khoa, lớp và thống kê phân loại học lực.
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
	/// LỚP (CLASS): ReportController
	/// Mục đích: Cung cấp các chức năng thống kê và báo cáo dành riêng cho quản trị viên (Admin).
	/// Chức năng: Truy vấn dữ liệu điểm, tính toán GPA và phân loại học lực sinh viên.
	/// Quyền truy cập: Chỉ người dùng có vai trò "Admin" mới có thể truy cập.
	/// </summary>
	[Authorize(Roles = "Admin")]
	public class ReportController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ReportController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Xử lý hiển thị trang danh sách báo cáo điểm sinh viên.
		/// Tham số: facultyId (Mã khoa), classId (Mã lớp), semester (Học kỳ).
		/// Giá trị trả về: View kèm theo dữ liệu thống kê (ReportViewModel).
		/// </summary>
		public async Task<IActionResult> Index(string facultyId, string classId, string semester)
		{
			/* 
             * LOGIC PHỨC TẠP: TRUY VẤN ĐỘNG (DYNAMIC QUERY) & THỐNG KÊ TỔNG HỢP
             * 1. Sử dụng AsQueryable để xây dựng câu lệnh SQL động dựa trên các tham số lọc 
             *    (khoa/lớp) mà không cần tải toàn bộ dữ liệu vào bộ nhớ ngay lập tức.
             * 2. Duyệt danh sách sinh viên để tính điểm trung bình (GPA) dựa trên tập dữ liệu Grades đi kèm.
             */
			var students = _context.Students
				.Include(s => s.Grades)
				.AsQueryable();

			// Áp dụng bộ lọc nếu người dùng chọn
			if (!string.IsNullOrEmpty(facultyId))
			{
				students = students.Where(s => s.FacultyId == facultyId);
			}

			if (!string.IsNullOrEmpty(classId))
			{
				students = students.Where(s => s.ClassId == classId);
			}

			var studentList = await students.ToListAsync();
			var reports = new List<StudentGradeReport>();

			// Các biến cục bộ dùng để đếm số lượng phân loại học lực
			int excellent = 0, good = 0, fair = 0, average = 0, poor = 0;

			foreach (var student in studentList)
			{
				var grades = student.Grades;
				if (grades != null && grades.Any())
				{
					/*
                     * THUẬT TOÁN: TÍNH TOÁN VÀ PHÂN LOẠI
                     * Tính GPA hệ 10 bằng cách lấy trung bình cộng điểm các môn, 
                     * sau đó quy đổi sang hệ 4 và thực hiện đếm phân loại bằng Switch-case.
                     */
					decimal gpa10 = grades.Average(g => g.TotalScore);
					decimal gpa4 = ConvertToGPA4(gpa10);
					string classification = GetClassification(gpa10);

					switch (classification)
					{
						case "Xuất sắc": excellent++; break;
						case "Giỏi": good++; break;
						case "Khá": fair++; break;
						case "Trung bình": average++; break;
						case "Yếu": poor++; break;
					}

					reports.Add(new StudentGradeReport
					{
						MSSV = student.MSSV,
						FullName = student.FullName,
						GPA10 = gpa10,
						GPA4 = gpa4,
						Classification = classification
					});
				}
			}

			var viewModel = new ReportViewModel
			{
				FacultyId = facultyId,
				ClassId = classId,
				Semester = semester,
				ExcellentCount = excellent,
				GoodCount = good,
				FairCount = fair,
				AverageCount = average,
				PoorCount = poor,
				StudentReports = reports
			};

			// Chuẩn bị dữ liệu cho các DropdownList ở giao diện
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			ViewBag.Classes = await _context.Classes.ToListAsync();

			return View(viewModel);
		}

		/// <summary>
		/// HÀM (METHOD): ConvertToGPA4
		/// Mục đích: Quy đổi điểm trung bình từ hệ 10 sang hệ 4.
		/// Logic: Sử dụng các ngưỡng điểm chuẩn để trả về giá trị số tương ứng.
		/// </summary>
		private decimal ConvertToGPA4(decimal gpa10)
		{
			if (gpa10 >= 9.0m) return 4.0m;
			if (gpa10 >= 8.5m) return 3.7m;
			if (gpa10 >= 8.0m) return 3.5m;
			if (gpa10 >= 7.0m) return 3.0m;
			if (gpa10 >= 6.5m) return 2.5m;
			if (gpa10 >= 5.5m) return 2.0m;
			if (gpa10 >= 5.0m) return 1.5m;
			return 0.0m;
		}

		/// <summary>
		/// HÀM (METHOD): GetClassification
		/// Mục đích: Xác định danh hiệu học tập dựa trên điểm hệ 10.
		/// </summary>
		private string GetClassification(decimal gpa10)
		{
			if (gpa10 >= 9.0m) return "Xuất sắc";
			if (gpa10 >= 8.0m) return "Giỏi";
			if (gpa10 >= 6.5m) return "Khá";
			if (gpa10 >= 5.0m) return "Trung bình";
			return "Yếu";
		}

		// --- Các Module chức năng mở rộng (Mock) ---

		public IActionResult ExportPDF(string facultyId, string classId, string semester)
		{
			TempData["Info"] = "Chức năng xuất PDF đang được phát triển";
			return RedirectToAction(nameof(Index), new { facultyId, classId, semester });
		}

		public IActionResult ExportExcel(string facultyId, string classId, string semester)
		{
			TempData["Info"] = "Chức năng xuất Excel đang được phát triển";
			return RedirectToAction(nameof(Index), new { facultyId, classId, semester });
		}
	}
}