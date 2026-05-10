/*
 * ==============================================================================
 * Tên tệp tin: Teacher.cs
 * Tổng quan: Lớp thực thể (Entity Class) đại diện cho hồ sơ Giảng viên.
 * Chức năng: Lưu trữ thông tin cá nhân, đơn vị công tác và quản lý các quan hệ 
 * giảng dạy hoặc chủ nhiệm lớp.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): Teacher
	/// Mục đích: Mô phỏng đối tượng Giảng viên trong cơ sở dữ liệu.
	/// Chức năng: Lưu giữ thông tin định danh và quản lý các lớp học phần được phân công.
	/// </summary>
	public class Teacher
	{
		/* * [Key]: Xác định TeacherId là Khóa chính (Primary Key).
         * Định dạng mã gợi nhớ giúp dễ nhận diện (Ví dụ: GV_001).
         */
		[Key]
		public string? TeacherId { get; set; }

		/* * [Required]: Họ tên giảng viên là bắt buộc để lập hồ sơ nhân sự. */
		[Required]
		[StringLength(100)]
		public string? FullName { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ ĐƠN VỊ CÔNG TÁC (Foreign Key)
         * FacultyId: Liên kết giảng viên với một Khoa chuyên môn quản lý.
         * virtual Faculty: Hỗ trợ Lazy Loading để truy xuất thông tin Khoa từ thực thể Giảng viên.
         */
		public string? FacultyId { get; set; }
		public virtual Faculty? Faculty { get; set; }

		/* * MODULE: LIÊN LẠC & TRẠNG THÁI
         * [EmailAddress] & [Phone]: Đảm bảo định dạng thông tin liên lạc hợp lệ.
         * Status: Trạng thái công tác (ví dụ: "Active" - Đang dạy, "Inactive" - Đã nghỉ).
         */
		[EmailAddress]
		public string? Email { get; set; }

		[Phone]
		public string? Phone { get; set; }

		public string? Status { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ GIẢNG DẠY (One-to-Many)
         * virtual ICollection<CourseClass>: Danh sách các lớp học phần mà giảng viên đang phụ trách.
         * Phục vụ cho việc thống kê giờ dạy và lập thời khóa biểu.
         */
		public virtual ICollection<CourseClass>? CourseClasses { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ CHỦ NHIỆM (One-to-Many)
         * virtual ICollection<Class>: Danh sách các lớp hành chính mà giảng viên này làm chủ nhiệm.
         * Điều này cho phép một giảng viên có thể quản lý nòng cốt cho nhiều lớp sinh viên.
         */
		public virtual ICollection<Class>? HomeRoomClasses { get; set; }
	}
}