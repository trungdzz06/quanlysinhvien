/*
 * ==============================================================================
 * Tên tệp tin: Faculty.cs
 * Tổng quan: Lớp thực thể (Entity Class) đại diện cho các Khoa đào tạo trong trường.
 * Chức năng: Lưu trữ thông tin định danh khoa và quản lý các quan hệ cấp bậc với 
 * Lớp hành chính và Sinh viên.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): Faculty
	/// Mục đích: Mô phỏng đơn vị quản lý cao nhất trong sơ đồ tổ chức (ví dụ: Khoa CNTT, Khoa Kinh tế).
	/// Chức năng: Đóng vai trò là "gốc" để phân loại sinh viên và lớp học theo chuyên môn.
	/// </summary>
	public class Faculty
	{
		/* * [Key]: Xác định FacultyId là Khóa chính (Primary Key).
         * Thường sử dụng các mã viết tắt như "CNTT", "KT" để thuận tiện cho việc truy vấn.
         */
		[Key]
		public string? FacultyId { get; set; }

		/* * [Required]: Yêu cầu bắt buộc phải có tên Khoa khi khởi tạo dữ liệu. */
		[Required]
		public string? FacultyName { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ CẤU TRÚC PHÂN CẤP (One-to-Many)
         * 1. virtual ICollection<Class>: Một Khoa sẽ quản lý nhiều Lớp hành chính. 
         * Giúp truy xuất nhanh danh sách các lớp thuộc khoa đó quản lý.
         * * 2. virtual ICollection<Student>: Một Khoa chứa danh sách tất cả sinh viên thuộc khoa.
         * Thiết lập mối quan hệ này giúp thực hiện các báo cáo thống kê số lượng sinh viên 
         * theo khoa một cách dễ dàng và tối ưu hiệu năng (Eager Loading).
         * * Thuộc tính 'virtual' cho phép Entity Framework sử dụng Lazy Loading để chỉ tải dữ liệu 
         * các bộ sưu tập này khi lập trình viên thực sự truy cập vào chúng.
         */
		public virtual ICollection<Class>? Classes { get; set; }
		public virtual ICollection<Student>? Students { get; set; }
	}
}