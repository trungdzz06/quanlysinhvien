/*
 * ==============================================================================
 * Tên tệp tin: Student.cs
 * Tổng quan: Lớp thực thể (Entity Class) đại diện cho thông tin Sinh viên trong hệ thống.
 * Chức năng: Định nghĩa cấu trúc bảng dữ liệu và các ràng buộc (Validation) cho Sinh viên.
 * Tác giả: Nhóm phát triển phần mềm
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): Student
	/// Mục đích: Mô phỏng đối tượng Sinh viên trong cơ sở dữ liệu.
	/// Chức năng: Lưu trữ thông tin cá nhân, quan hệ lớp học, khoa và kết quả học tập.
	/// </summary>
	public class Student
	{
		/* 
         * [Key]: Định nghĩa đây là Khóa chính của bảng.
         * MSSV: Mã số sinh viên, là định danh duy nhất cho mỗi cá nhân.
         */
		[Key]
		public string? MSSV { get; set; }

		/* 
         * [Required]: Trường thông tin bắt buộc phải nhập.
         * [StringLength(100)]: Giới hạn độ dài tối đa của chuỗi là 100 ký tự.
         */
		[Required]
		[StringLength(100)]
		public string? FullName { get; set; }

		/* [DataType(DataType.Date)]: Chỉ định kiểu dữ liệu là ngày tháng (không kèm giờ). */
		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; }

		public string? Gender { get; set; }

		[StringLength(200)]
		public string? Hometown { get; set; }

		/* 
         * LOGIC PHỨC TẠP: QUAN HỆ KHÓA NGOẠI (Foreign Key Relationships)
         * ClassId và FacultyId đóng vai trò liên kết Sinh viên với các thực thể Lớp và Khoa.
         * Thuộc tính 'virtual' cho phép Entity Framework sử dụng cơ chế Lazy Loading để 
         * tự động tải thông tin liên quan khi cần thiết.
         */
		public string? ClassId { get; set; }
		public virtual Class? Class { get; set; }

		public string? FacultyId { get; set; }
		public virtual Faculty? Faculty { get; set; }

		/* [Phone]: Ràng buộc định dạng phải là số điện thoại hợp lệ. */
		[Phone]
		public string? Phone { get; set; }

		/* [EmailAddress]: Ràng buộc định dạng phải là địa chỉ Email hợp lệ. */
		[EmailAddress]
		public string? Email { get; set; }

		/* 
         * Trạng thái hiện tại của sinh viên: 
         * Ví dụ: "Đang học", "Tạm ngừng", "Đã tốt nghiệp".
         */
		public string? Status { get; set; }

		/* 
         * LOGIC PHỨC TẠP: QUAN HỆ MỘT - NHIỀU (One-to-Many)
         * ICollection<Grade>: Một sinh viên có thể có nhiều đầu điểm (kết quả học tập).
         * Danh sách này cho phép truy xuất toàn bộ lịch sử điểm của sinh viên đó.
         */
		public virtual ICollection<Grade>? Grades { get; set; }
	}
}