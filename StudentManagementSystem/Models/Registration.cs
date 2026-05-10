/*
 * ==============================================================================
 * Tên tệp tin: Registration.cs
 * Tổng quan: Lớp thực thể (Entity Class) quản lý việc đăng ký lớp học phần của sinh viên.
 * Chức năng: Lưu trữ thông tin chi tiết về các học phần mà sinh viên đã đăng ký trong từng kỳ.
 * Tác giả: Nhóm phát triển phần mềm
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): Registration
	/// Mục đích: Đại diện cho một bản ghi đăng ký môn học trong cơ sở dữ liệu.
	/// Chức năng: Kết nối giữa Sinh viên và Lớp học phần, đồng thời lưu trữ trạng thái đăng ký.
	/// </summary>
	public class Registration
	{
		/* 
         * [Key]: Định nghĩa Khóa chính (Primary Key) cho bảng Registration.
         * Hệ thống sẽ tự động tăng giá trị này khi có bản ghi mới.
         */
		[Key]
		public int RegistrationId { get; set; }

		/* 
         * LOGIC PHỨC TẠP: QUAN HỆ KHÓA NGOẠI (Foreign Keys)
         * StudentId và CourseClassId là các tham chiếu để xác định ai đăng ký và đăng ký lớp nào.
         * Thuộc tính 'virtual' cho phép Entity Framework thực hiện cơ chế Lazy Loading (tải dữ liệu khi cần).
         */
		public string? StudentId { get; set; }
		public virtual Student? Student { get; set; }

		public string? CourseClassId { get; set; }
		public virtual CourseClass? CourseClass { get; set; }

		/* Lưu trữ thời điểm chính xác sinh viên thực hiện thao tác đăng ký. */
		public DateTime RegistrationDate { get; set; }

		/* 
         * Trạng thái của bản ghi đăng ký. 
         * Ví dụ: "Đã đăng ký", "Đã hủy", "Đã hoàn thành".
         */
		public string? Status { get; set; }

		/* 
         * Thông tin về thời gian đào tạo:
         * Semester: Học kỳ thực hiện đăng ký (ví dụ: Học kỳ 1 - 2024).
         * Year: Năm học tương ứng.
         */
		public string? Semester { get; set; }/*
 * ==============================================================================
 * Tên tệp tin: Registration.cs
 * Tổng quan: Lớp thực thể (Entity Class) quản lý việc đăng ký lớp học phần của sinh viên.
 * Chức năng: Lưu trữ thông tin chi tiết về các học phần mà sinh viên đã đăng ký trong từng kỳ.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
	{
		/// <summary>
		/// LỚP (CLASS): Registration
		/// Mục đích: Đại diện cho một bản ghi đăng ký môn học trong cơ sở dữ liệu.
		/// Chức năng: Kết nối giữa Sinh viên và Lớp học phần, đồng thời lưu trữ trạng thái đăng ký.
		/// </summary>
		public class Registration
		{
			/* * [Key]: Định nghĩa Khóa chính (Primary Key) cho bảng Registration.
			 * Hệ thống sẽ tự động tăng giá trị này khi có bản ghi mới được tạo.
			 */
			[Key]
			public int RegistrationId { get; set; }

			/* * LOGIC PHỨC TẠP: QUAN HỆ KHÓA NGOẠI (Foreign Keys)
			 * 1. StudentId: Liên kết với thực thể Sinh viên (Student).
			 * 2. CourseClassId: Liên kết với thực thể Lớp học phần (CourseClass).
			 * Thuộc tính 'virtual' hỗ trợ Lazy Loading, giúp truy xuất thông tin chi tiết của 
			 * sinh viên hoặc lớp học phần ngay từ bản ghi đăng ký.
			 */
			public string? StudentId { get; set; }
			public virtual Student? Student { get; set; }

			public string? CourseClassId { get; set; }
			public virtual CourseClass? CourseClass { get; set; }

			/* MODULE: THÔNG TIN GIAO DỊCH
			 * RegistrationDate: Lưu trữ thời điểm chính xác sinh viên thực hiện thao tác đăng ký. 
			 */
			public DateTime RegistrationDate { get; set; }

			/* * Trạng thái của bản ghi đăng ký. 
			 * Ví dụ: "Đã đăng ký", "Đã hủy", "Chờ thanh toán", "Đã xác nhận".
			 */
			public string? Status { get; set; }

			/* * MODULE: THỜI GIAN ĐÀO TẠO
			 * Semester: Học kỳ thực hiện đăng ký (ví dụ: Học kỳ 1 - 2024).
			 * Year: Năm học tương ứng của kỳ đăng ký đó.
			 */
			public string? Semester { get; set; }
			public int Year { get; set; }
		}
	}
	public int Year { get; set; }
	}
}