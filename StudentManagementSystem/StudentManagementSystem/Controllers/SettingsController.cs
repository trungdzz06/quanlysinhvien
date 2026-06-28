/*
 * ==============================================================================
 * Tên tệp tin: SettingsController.cs
 * Tổng quan: Module quản lý các thiết lập hệ thống, bao gồm cấu hình học kỳ 
 *            và quản lý sao lưu dữ liệu (Backup).
 * Tác giả: Nhóm phát triển phần mềm
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentManagementSystem.Controllers
{
	/// <summary>
	/// LỚP (CLASS): SettingsController
	/// Mục đích: Cung cấp giao diện và chức năng cấu hình hệ thống dành cho quản trị viên.
	/// Quyền truy cập: Yêu cầu vai trò "Admin" để thực hiện các thay đổi quan trọng.
	/// </summary>
	[Authorize(Roles = "Admin")]
	public class SettingsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _env;

		public SettingsController(ApplicationDbContext context, IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}

		private string GetSettingsFilePath()
		{
			return Path.Combine(_env.WebRootPath, "systemsettings.json");
		}

		private SystemSettings LoadSettings()
		{
			string path = GetSettingsFilePath();
			if (!System.IO.File.Exists(path))
			{
				var defaultSettings = new SystemSettings
				{
					CurrentSemester = "Học kỳ 1 - 2024",
					LastBackupTime = DateTime.Now.AddHours(-12)
				};
				string json = JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true });
				System.IO.File.WriteAllText(path, json);
				return defaultSettings;
			}

			try
			{
				string json = System.IO.File.ReadAllText(path);
				return JsonSerializer.Deserialize<SystemSettings>(json) ?? new SystemSettings();
			}
			catch
			{
				return new SystemSettings { CurrentSemester = "Học kỳ 1 - 2024", LastBackupTime = DateTime.Now.AddHours(-12) };
			}
		}

		private void SaveSettings(SystemSettings settings)
		{
			string path = GetSettingsFilePath();
			string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
			System.IO.File.WriteAllText(path, json);
		}

		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Hiển thị trang quản lý thiết lập tổng thể.
		/// </summary>
		public IActionResult Index()
		{
			var settings = LoadSettings();
			ViewBag.LastBackupTime = settings.LastBackupTime;
			ViewBag.CurrentSemester = settings.CurrentSemester;
			return View();
		}

		/// <summary>
		/// HÀM (METHOD): Backup [POST]
		/// Mục đích: Khởi chạy quy trình sao lưu cơ sở dữ liệu sang tệp tin JSON lưu trữ tại máy chủ.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Backup()
		{
			try
			{
				// 1. Lấy tất cả dữ liệu từ database
				var backupData = new DatabaseBackupData
				{
					Students = await _context.Students.AsNoTracking().ToListAsync(),
					Teachers = await _context.Teachers.AsNoTracking().ToListAsync(),
					Classes = await _context.Classes.AsNoTracking().ToListAsync(),
					Subjects = await _context.Subjects.AsNoTracking().ToListAsync(),
					Grades = await _context.Grades.AsNoTracking().ToListAsync(),
					Registrations = await _context.Registrations.AsNoTracking().ToListAsync(),
					CourseClasses = await _context.CourseClasses.AsNoTracking().ToListAsync(),
					Faculties = await _context.Faculties.AsNoTracking().ToListAsync()
				};

				// 2. Định nghĩa thư mục lưu trữ backup
				string backupsFolder = Path.Combine(_env.WebRootPath, "backups");
				if (!Directory.Exists(backupsFolder))
				{
					Directory.CreateDirectory(backupsFolder);
				}

				// 3. Đặt tên tệp tin backup theo thời gian
				string fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
				string filePath = Path.Combine(backupsFolder, fileName);

				// 4. Lưu dữ liệu dạng JSON
				string json = JsonSerializer.Serialize(backupData, new JsonSerializerOptions { WriteIndented = true });
				await System.IO.File.WriteAllTextAsync(filePath, json);

				// 5. Cập nhật mốc thời gian backup trong file cấu hình
				var settings = LoadSettings();
				settings.LastBackupTime = DateTime.Now;
				SaveSettings(settings);

				TempData["Success"] = $"Đã sao lưu cơ sở dữ liệu thành công vào file '{fileName}' lúc {settings.LastBackupTime:HH:mm dd/MM/yyyy}!";
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Lỗi sao lưu cơ sở dữ liệu: {ex.Message}";
			}

			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// HÀM (METHOD): UpdateSemester [POST]
		/// Mục đích: Cập nhật học kỳ hiện hành cho toàn bộ hệ thống.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateSemester(string semester)
		{
			if (string.IsNullOrEmpty(semester))
			{
				TempData["Error"] = "Tên học kỳ không được để trống";
				return RedirectToAction(nameof(Index));
			}

			var settings = LoadSettings();
			settings.CurrentSemester = semester;
			SaveSettings(settings);

			TempData["Success"] = "Đã cập nhật học kỳ hiện tại: " + semester;
			return RedirectToAction(nameof(Index));
		}
	}

	public class SystemSettings
	{
		public string CurrentSemester { get; set; } = "Học kỳ 1 - 2024";
		public DateTime LastBackupTime { get; set; } = DateTime.Now.AddHours(-12);
	}

	public class DatabaseBackupData
	{
		public System.Collections.Generic.List<Student> Students { get; set; } = new();
		public System.Collections.Generic.List<Teacher> Teachers { get; set; } = new();
		public System.Collections.Generic.List<Class> Classes { get; set; } = new();
		public System.Collections.Generic.List<Subject> Subjects { get; set; } = new();
		public System.Collections.Generic.List<Grade> Grades { get; set; } = new();
		public System.Collections.Generic.List<Registration> Registrations { get; set; } = new();
		public System.Collections.Generic.List<CourseClass> CourseClasses { get; set; } = new();
		public System.Collections.Generic.List<Faculty> Faculties { get; set; } = new();
	}
}