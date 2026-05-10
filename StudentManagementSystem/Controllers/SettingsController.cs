/*
 * ==============================================================================
 * Tên tệp tin: SettingsController.cs
 * Tổng quan: Module quản lý các thiết lập hệ thống, bao gồm cấu hình học kỳ 
 *            và quản lý sao lưu dữ liệu (Backup).
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
		/// <summary>
		/// HÀM (METHOD): Index
		/// Mục đích: Hiển thị trang quản lý thiết lập tổng thể.
		/// Chức năng: Cung cấp thông tin về trạng thái hệ thống như thời gian sao lưu cuối và học kỳ hiện tại.
		/// </summary>
		/// <returns>View Index kèm dữ liệu trạng thái qua ViewBag.</returns>
		public IActionResult Index()
		{
			// Ý nghĩa các biến cục bộ/ViewBag:
			// LastBackupTime: Giả lập thời gian sao lưu gần nhất để hiển thị cho người dùng.
			ViewBag.LastBackupTime = DateTime.Now.AddHours(-12);
			ViewBag.CurrentSemester = "Học kỳ 1 - 2024";
			return View();
		}

		/// <summary>
		/// HÀM (METHOD): Backup [POST]
		/// Mục đích: Khởi chạy quy trình sao lưu cơ sở dữ liệu.
		/// Logic: Sử dụng ValidateAntiForgeryToken để ngăn chặn các cuộc tấn công giả mạo yêu cầu từ phía người dùng.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Backup()
		{
			/* 
             * LOGIC PHỨC TẠP: QUY TRÌNH SAO LƯU (MOCK)
             * Trong thực tế, hàm này sẽ gọi đến các Service tầng thấp hơn để thực hiện 
             * lệnh SQL BACKUP DATABASE hoặc nén các tệp tin lưu trữ. 
             * Ở đây, hệ thống phản hồi thông báo thành công kèm dấu thời gian thực tế.
             */
			TempData["Success"] = "Đã tạo bản sao lưu thành công lúc " + DateTime.Now.ToString("HH:mm dd/MM/yyyy");
			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// HÀM (METHOD): UpdateSemester [POST]
		/// Mục đích: Cập nhật học kỳ hiện hành cho toàn bộ hệ thống.
		/// Tham số: semester (Chuỗi tên học kỳ mới).
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateSemester(string semester)
		{
			/* 
             * LOGIC PHỨC TẠP: CẬP NHẬT TRẠNG THÁI HỆ THỐNG
             * Việc thay đổi học kỳ hiện tại có tầm ảnh hưởng lớn, tác động đến các module 
             * đăng ký môn học và nhập điểm. Do đó, yêu cầu kiểm tra tính hợp lệ của chuỗi đầu vào.
             */
			if (string.IsNullOrEmpty(semester))
			{
				TempData["Error"] = "Tên học kỳ không được để trống";
				return RedirectToAction(nameof(Index));
			}

			// Giả lập lưu vào cấu hình hệ thống
			TempData["Success"] = "Đã cập nhật học kỳ hiện tại: " + semester;
			return RedirectToAction(nameof(Index));
		}
	}
}