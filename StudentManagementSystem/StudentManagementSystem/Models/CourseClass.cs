/*
 * ==============================================================================
 * Tên tệp tin: CourseClass.cs
 * Tổng quan: Lớp thực thể (Entity Class) đại diện cho các Lớp học phần (LHP).
 * Chức năng: Lưu trữ thông tin về môn học, giảng viên phụ trách, sĩ số và lịch học.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): CourseClass
	/// Mục đích: Mô phỏng đối tượng Lớp học phần trong cơ sở dữ liệu (ví dụ: Lớp Công nghệ phần mềm - Nhóm 01).
	/// Chức năng: Quản lý đăng ký học tập và là cầu nối giữa Môn học với Giảng viên.
	/// </summary>
	public class CourseClass
	{
		/* * [Key]: Xác định CourseClassId là Khóa chính duy nhất của lớp học phần. */
		[Key]
		public string? CourseClassId { get; set; }

		/* * [Required]: Mã môn học là thông tin bắt buộc để định danh lớp thuộc môn nào. */
		[Required]
		public string? SubjectCode { get; set; }

		/* * LOGIC PHỨC TẠP: CẤU HÌNH KHÓA NGOẠI (FOREIGN KEY)
         * [ForeignKey("SubjectCode")]: Chỉ định thuộc tính SubjectCode là khóa ngoại tham chiếu đến bảng Subject.
         * virtual Subject: Cho phép Entity Framework tải thông tin môn học liên quan (Lazy Loading).
         */
		[ForeignKey("SubjectCode")]
		public virtual Subject? Subject { get; set; }

		public string? TeacherId { get; set; }

		/* * [ForeignKey("TeacherId")]: Liên kết lớp học phần với một giảng viên phụ trách cụ thể.
         * virtual Teacher: Giúp truy xuất thông tin chi tiết của giảng viên từ thực thể lớp học phần.
         */
		[ForeignKey("TeacherId")]
		public virtual Teacher? Teacher { get; set; }

		/* Các thông tin về thời gian đào tạo và sĩ số */
		public string? Semester { get; set; }      // Ví dụ: "Học kỳ 1 - 2024"
		public int MaxStudents { get; set; }       // Sĩ số tối đa (Ví dụ: 50)
		public int CurrentStudents { get; set; }   // Sĩ số hiện tại đã đăng ký thành công
		public string? Status { get; set; }        // Trạng thái: "Mở đăng ký", "Đã đầy", "Đã hủy"
		public string? Schedule { get; set; }      // Thông tin lịch học (Thứ, Ca, Phòng)

		/* * LOGIC PHỨC TẠP: QUAN HỆ MỘT - NHIỀU (One-to-Many)
         * ICollection<Registration>: Một lớp học phần sẽ có một danh sách nhiều bản ghi đăng ký 
         * từ các sinh viên khác nhau. Danh sách này dùng để quản lý sĩ số và danh sách lớp.
         */
		public virtual ICollection<Registration>? Registrations { get; set; }
	}
}