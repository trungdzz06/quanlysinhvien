/*
 * ==============================================================================
 * Tên tệp tin: ReportController.cs
 * Tổng quan: Module quản lý và tổng hợp báo cáo kết quả học tập của sinh viên.
 *            Hỗ trợ lọc dữ liệu theo khoa, lớp và thống kê phân loại học lực.
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
				var studentGrades = student.Grades ?? new List<Grade>();
				if (!string.IsNullOrEmpty(semester))
				{
					studentGrades = studentGrades.Where(g => g.Semester == semester).ToList();
				}

				if (studentGrades.Any())
				{
					/*
                     * THUẬT TOÁN: TÍNH TOÁN VÀ PHÂN LOẠI
                     * Tính GPA hệ 10 bằng cách lấy trung bình cộng điểm các môn, 
                     * sau đó quy đổi sang hệ 4 và thực hiện đếm phân loại bằng Switch-case.
                     */
					decimal gpa10 = studentGrades.Average(g => g.TotalScore);
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
			ViewBag.Semesters = await _context.CourseClasses
				.Select(c => c.Semester)
				.Where(s => s != null)
				.Distinct()
				.ToListAsync();

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

		// --- Các Module chức năng mở rộng ---

		[HttpGet]
		public async Task<IActionResult> ExportPDF(string facultyId, string classId, string semester)
			{
			var students = _context.Students
				.Include(s => s.Grades)
				.Include(s => s.Class)
				.Include(s => s.Faculty)
				.AsQueryable();

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

			foreach (var student in studentList)
			{
				var studentGrades = student.Grades ?? new List<Grade>();
				if (!string.IsNullOrEmpty(semester))
				{
					studentGrades = studentGrades.Where(g => g.Semester == semester).ToList();
				}

				if (studentGrades.Any())
				{
					decimal gpa10 = studentGrades.Average(g => g.TotalScore);
					decimal gpa4 = ConvertToGPA4(gpa10);
					string classification = GetClassification(gpa10);

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

			ViewBag.SelectedFaculty = !string.IsNullOrEmpty(facultyId) ? (await _context.Faculties.FindAsync(facultyId))?.FacultyName : "Tất cả";
			ViewBag.SelectedClass = !string.IsNullOrEmpty(classId) ? (await _context.Classes.FindAsync(classId))?.ClassName : "Tất cả";
			ViewBag.Semester = semester;

			return View("PrintReport", reports);
		}

		[HttpGet]
		public async Task<IActionResult> ExportExcel(string facultyId, string classId, string semester)
		{
			var students = _context.Students
				.Include(s => s.Grades)
				.Include(s => s.Class)
				.Include(s => s.Faculty)
				.AsQueryable();

			if (!string.IsNullOrEmpty(facultyId))
				students = students.Where(s => s.FacultyId == facultyId);

			if (!string.IsNullOrEmpty(classId))
				students = students.Where(s => s.ClassId == classId);

			var studentList = await students.ToListAsync();

			string selectedFacultyName = !string.IsNullOrEmpty(facultyId)
				? (await _context.Faculties.FindAsync(facultyId))?.FacultyName ?? facultyId
				: "Tất cả";
			string selectedClassName = !string.IsNullOrEmpty(classId)
				? (await _context.Classes.FindAsync(classId))?.ClassName ?? classId
				: "Tất cả";

			var csvBuilder = new System.Text.StringBuilder();
			csvBuilder.Append('\uFEFF'); // UTF-8 BOM
			csvBuilder.AppendLine("BÁO CÁO KẾT QUẢ HỌC TẬP SINH VIÊN");
			csvBuilder.AppendLine($"Khoa: {selectedFacultyName} | Lớp: {selectedClassName} | Học kỳ: {(string.IsNullOrEmpty(semester) ? "Tất cả" : semester)}");
			csvBuilder.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} | Người lập: Hệ thống Quản lý Sinh viên");
			csvBuilder.AppendLine();
			csvBuilder.AppendLine("STT,MSSV,Họ Tên,Lớp,Khoa,Điểm TBC (Hệ 10),Tích lũy (Hệ 4),Xếp loại");

			int stt = 1;
			int excellent = 0, good = 0, fair = 0, average = 0, poor = 0;

			foreach (var student in studentList)
			{
				var studentGrades = student.Grades ?? new List<Grade>();
				if (!string.IsNullOrEmpty(semester))
					studentGrades = studentGrades.Where(g => g.Semester == semester).ToList();

				if (studentGrades.Any())
				{
					decimal gpa10 = studentGrades.Average(g => g.TotalScore);
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

					csvBuilder.AppendLine($"{stt},\"{student.MSSV}\",\"{student.FullName}\",\"{student.Class?.ClassName ?? "N/A"}\",\"{student.Faculty?.FacultyName ?? "N/A"}\",{gpa10.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{gpa4.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},\"{classification}\"");
					stt++;
				}
			}

			int totalWithGrades = stt - 1;
			csvBuilder.AppendLine();
			csvBuilder.AppendLine("TỔNG KẾT PHÂN LOẠI HỌC LỰC");
			csvBuilder.AppendLine($"Tổng SV có điểm:,{totalWithGrades}");
			csvBuilder.AppendLine($"Xuất sắc (>= 9.0):,{excellent},{(totalWithGrades > 0 ? $"{excellent * 100 / totalWithGrades}%" : "N/A")}");
			csvBuilder.AppendLine($"Giỏi (8.0 - 8.9):,{good},{(totalWithGrades > 0 ? $"{good * 100 / totalWithGrades}%" : "N/A")}");
			csvBuilder.AppendLine($"Khá (6.5 - 7.9):,{fair},{(totalWithGrades > 0 ? $"{fair * 100 / totalWithGrades}%" : "N/A")}");
			csvBuilder.AppendLine($"Trung bình (5.0 - 6.4):,{average},{(totalWithGrades > 0 ? $"{average * 100 / totalWithGrades}%" : "N/A")}");
			csvBuilder.AppendLine($"Yếu (< 5.0):,{poor},{(totalWithGrades > 0 ? $"{poor * 100 / totalWithGrades}%" : "N/A")}");

			string fileName = $"BaoCaoDiem_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
			return File(buffer, "text/csv; charset=utf-8", fileName);
		}
	}
}