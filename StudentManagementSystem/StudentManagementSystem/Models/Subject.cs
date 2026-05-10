/*
 * ==============================================================================
 * Tên tệp tin: Subject.cs
 * Tổng quan: Lớp thực thể (Entity Class) đại diện cho danh mục Môn học trong hệ thống.
 * Chức năng: Lưu trữ thông tin môn học, số tín chỉ và thiết lập liên kết với Khoa quản lý.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): Subject
	/// Mục đích: Mô phỏng đối tượng Môn học (ví dụ: Công nghệ phần mềm, Cơ sở dữ liệu).
	/// Chức năng: Là thông tin nền tảng để tạo ra các Lớp học phần (CourseClass).
	/// </summary>
	public class Subject
	{
		/* * [Key]: Xác định SubjectCode là Khóa chính (Primary Key). 
         * Sử dụng mã gợi nhớ (ví dụ: CNPM_01) để dễ dàng quản lý và truy vấn.
         */
		[Key]
		public string? SubjectCode { get; set; }

		/* * [Required]: Tên môn học là bắt buộc.
         * [StringLength(200)]: Giới hạn độ dài tên môn học để tối ưu hóa lưu trữ database.
         */
		[Required]
		[StringLength(200)]
		public string? SubjectName { get; set; }

		/* * MODULE: QUY ĐỊNH TÍN CHỈ
         * [Range(1, 6)]: Ràng buộc số tín chỉ của một môn học nằm trong khoảng từ 1 đến 6.
         * Điều này giúp ngăn chặn việc nhập sai dữ liệu đào tạo.
         */
		[Range(1, 6)]
		public int Credits { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ KHÓA NGOẠI (Foreign Key)
         * FacultyId: Liên kết môn học này với Khoa chuyên môn quản lý trực tiếp.
         * virtual Faculty: Thuộc tính điều hướng hỗ trợ Lazy Loading để lấy thông tin Khoa.
         */
		public string? FacultyId { get; set; }
		public virtual Faculty? Faculty { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ MỘT - NHIỀU (One-to-Many)
         * ICollection<CourseClass>: Một môn học (Subject) có thể được mở thành nhiều 
         * Lớp học phần (CourseClass) khác nhau trong cùng một kỳ hoặc nhiều kỳ học.
         * Ví dụ: Môn "Toán rời rạc" có thể có 3 lớp học phần chạy song song.
         */
		public virtual ICollection<CourseClass>? CourseClasses { get; set; }
	}
}