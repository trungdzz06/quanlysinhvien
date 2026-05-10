/*
 * ==============================================================================
 * Tên tệp tin: Class.cs
 * Tổng quan: Lớp thực thể (Entity Class) đại diện cho các Lớp hành chính trong hệ thống.
 * Chức năng: Lưu trữ thông tin định danh lớp học và thiết lập mối quan hệ với Khoa và Sinh viên.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): Class
	/// Mục đích: Mô tả đối tượng Lớp học (ví dụ: CNTT18-06) trong cơ sở dữ liệu.
	/// Chức năng: Là trung gian quản lý danh sách sinh viên thuộc cùng một đơn vị lớp.
	/// </summary>
	public class Class
	{
		/* * [Key]: Xác định ClassId là Khóa chính (Primary Key).
         * Kiểu dữ liệu string cho phép tùy biến mã lớp theo định dạng của nhà trường.
         */
		[Key]
		public string? ClassId { get; set; }

		/* * [Required]: Ràng buộc bắt buộc phải nhập tên lớp.
         * Đảm bảo tính toàn vẹn dữ liệu khi khởi tạo lớp mới.
         */
		[Required]
		public string? ClassName { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ KHÓA NGOẠI (Foreign Key)
         * FacultyId: Liên kết lớp học này với một Khoa cụ thể (ví dụ: Khoa CNTT).
         * virtual Faculty: Cho phép Entity Framework thực hiện cơ chế Lazy Loading để 
         * truy xuất thông tin chi tiết của Khoa từ thực thể Class.
         */
		public string? FacultyId { get; set; }
		public virtual Faculty? Faculty { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ MỘT - NHIỀU (One-to-Many)
         * ICollection<Student>: Định nghĩa rằng một lớp hành chính chứa danh sách nhiều sinh viên.
         * Thuộc tính virtual giúp hệ thống tự động tải danh sách sinh viên khi cần truy vấn 
         * sĩ số hoặc thông tin thành viên trong lớp.
         */
		public virtual ICollection<Student>? Students { get; set; }
	}
}