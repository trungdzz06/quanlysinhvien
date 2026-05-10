/*
 * ==============================================================================
 * Tên tệp tin: HomeController.cs
 * Tổng quan: Controller trung tâm quản lý trang tổng quan (Dashboard). 
 * Cung cấp các số liệu thống kê tổng quát và dữ liệu trực quan hóa.
 * Tác giả: Nhóm phát triển phần mềm
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace StudentManagementSystem.Controllers
{
	/// <summary>
	/// LỚP (CLASS): HomeController
	/// Mục đích: Xử lý các yêu cầu hiển thị trang chủ và báo cáo số liệu nhanh.
	/// Quyền truy cập: [Authorize] - Yêu cầu người dùng phải đăng nhập để xem Dashboard.
	/// </summary>
	[Authorize]
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// HÀM (METHOD): Dashboard
		/// Mục đích: Tổng hợp các chỉ số KPI của hệ thống (Sinh viên, Lớp học, Khoa).
		/// Chức năng: Truy vấn dữ liệu thực tế từ Database và chuẩn bị cấu trúc JSON cho biểu đồ.
		/// </summary>
		/// <returns>View Dashboard kèm các thông số thống kê qua ViewBag.</returns>
		public async Task<IActionResult> Dashboard()
		{
			/* * MODULE: THỐNG KÊ TỔNG QUAN
             * Sử dụng CountAsync để đếm số lượng bản ghi một cách bất đồng bộ, 
             * giúp tối ưu hóa luồng xử lý của Web Server khi có nhiều người truy cập.
             */
			ViewBag.TotalStudents = await _context.Students.CountAsync();
			ViewBag.TotalClasses = await _context.Classes.CountAsync();
			ViewBag.TotalFaculties = await _context.Faculties.CountAsync();
			ViewBag.TotalSubjects = 85; // Dữ liệu giả lập (Mock data) cho mục Môn học

			/* * LOGIC PHỨC TẠP: XỬ LÝ DỮ LIỆU BIỂU ĐỒ (CHART DATA)
             * Quy trình:
             * 1. Truy vấn danh sách Khoa (Faculties).
             * 2. Sử dụng Projection (Select) để chỉ lấy các trường cần thiết: Tên khoa và số lượng sinh viên.
             * 3. StudentCount được tính toán thông qua quan hệ Collection giữa Faculty và Student.
             * Kết quả này thường được chuyển đổi thành JSON ở phía View để Chart.js hoặc Google Charts xử lý.
             */
			var facultyStats = await _context.Faculties
				.Select(f => new
				{
					Name = f.FacultyName,
					StudentCount = f.Students.Count
				})
				.ToListAsync();

			// Gán dữ liệu thống kê vào ViewBag để truyền ra giao diện
			ViewBag.FacultyStats = facultyStats;

			return View();
		}
	}
}