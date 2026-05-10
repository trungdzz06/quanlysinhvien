/*
 * ==============================================================================
 * Tên tệp tin: LoginViewModel.cs
 * Tổng quan: Lớp trung gian (View Model) dùng để truyền tải dữ liệu từ trang Đăng nhập
 * về phía Controller. Định nghĩa các quy tắc kiểm tra dữ liệu đầu vào.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): LoginViewModel
	/// Mục đích: Đóng gói thông tin tài khoản người dùng khi thực hiện đăng nhập vào hệ thống.
	/// Chức năng: Tách biệt dữ liệu hiển thị (View) và dữ liệu thực thể (Entity) để bảo mật.
	/// </summary>
	public class LoginViewModel
	{
		/* * MODULE: TÀI KHOẢN ĐĂNG NHẬP
         * [Required]: Bắt buộc người dùng phải nhập thông tin, nếu trống sẽ hiển thị ErrorMessage.
         * [Display]: Định nghĩa nhãn (label) hiển thị tương ứng trên giao diện người dùng.
         */
		[Required(ErrorMessage = "Vui lòng nhập email hoặc mã số")]
		[Display(Name = "Email / Mã số")]
		public string? Username { get; set; }

		/* * MODULE: MẬT KHẨU
         * [DataType(DataType.Password)]: Chỉ định kiểu dữ liệu là mật khẩu, giúp trình duyệt 
         * tự động ẩn ký tự (hiển thị dạng dấu chấm hoặc sao) khi người dùng nhập liệu.
         */
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu")]
		public string? Password { get; set; }

		/* * LOGIC TIỆN ÍCH: GHI NHỚ ĐĂNG NHẬP
         * RememberMe: Kiểu bool dùng để xác định liệu hệ thống có nên lưu Cookie phiên đăng nhập
         * lâu dài (Persistent Cookie) hay không.
         */
		[Display(Name = "Nhớ mật khẩu")]
		public bool RememberMe { get; set; }
	}
}