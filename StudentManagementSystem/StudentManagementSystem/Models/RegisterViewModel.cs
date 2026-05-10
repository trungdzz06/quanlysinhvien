/*
 * ==============================================================================
 * Tên tệp tin: RegisterViewModel.cs
 * Tổng quan: Lớp trung gian (View Model) quản lý dữ liệu đăng ký tài khoản mới.
 * Chức năng: Kiểm soát tính hợp lệ của dữ liệu đầu vào và đảm bảo các quy tắc 
 * đăng ký đặc thù của Đại học Đại Nam.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): RegisterViewModel
	/// Mục đích: Đóng gói thông tin đăng ký bao gồm vai trò, họ tên, email và mật khẩu.
	/// Chức năng: Sử dụng Data Annotations để thực hiện xác thực phía máy chủ (Server-side validation).
	/// </summary>
	public class RegisterViewModel
	{
		/* * MODULE: VAI TRÒ (ROLE)
         * Phân loại người dùng ngay từ bước đăng ký (Sinh viên hoặc Giảng viên).
         */
		[Required(ErrorMessage = "Vui lòng chọn vai trò")]
		public string? Role { get; set; } // "Student", "Teacher"

		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[StringLength(100)]
		public string? FullName { get; set; }

		/* * LOGIC PHỨC TẠP: XÁC THỰC EMAIL TỔ CHỨC
         * [EmailAddress]: Kiểm tra định dạng email tiêu chuẩn.
         * [RegularExpression]: Chỉ cho phép các email có đuôi "@dainam.edu.vn".
         * Điều này giúp lọc người dùng không thuộc tổ chức ngay từ bước nhập liệu.
         */
		[Required(ErrorMessage = "Vui lòng nhập email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@dainam\.edu\.vn$", ErrorMessage = "Phải sử dụng email của Đại học Đại Nam")]
		public string? Email { get; set; }

		/* * MODULE: QUẢN LÝ MẬT KHẨU
         * [StringLength]: Thiết lập độ dài tối thiểu để tăng cường tính bảo mật.
         * [DataType(DataType.Password)]: Đảm bảo các trình duyệt ẩn mật khẩu khi nhập.
         */
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
		[DataType(DataType.Password)]
		public string? Password { get; set; }

		/* * LOGIC PHỨC TẠP: XÁC NHẬN MẬT KHẨU (PASSWORD CONFIRMATION)
         * [Compare]: So khớp thuộc tính ConfirmPassword với thuộc tính Password.
         * Nếu hai giá trị không trùng nhau, hệ thống sẽ tự động thông báo lỗi mà không cần viết code logic thủ công.
         */
		[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
		[Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
		[DataType(DataType.Password)]
		public string? ConfirmPassword { get; set; }
	}
}