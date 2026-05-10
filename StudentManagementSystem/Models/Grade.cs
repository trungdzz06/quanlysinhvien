/*
 * ==============================================================================
 * Tên tệp tin: Grade.cs
 * Tổng quan: Lớp thực thực thể (Entity Class) quản lý thông tin điểm số của sinh viên.
 * Chức năng: Lưu trữ các đầu điểm thành phần, tính toán điểm tổng kết và quy đổi điểm chữ.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): Grade
	/// Mục đích: Đại diện cho một bản ghi điểm của sinh viên cho một môn học cụ thể.
	/// Chức năng: Lưu giữ kết quả học tập và phục vụ cho việc tính toán GPA, xếp loại.
	/// </summary>
	public class Grade
	{
		/* * [Key]: Khóa chính tự động tăng để định danh duy nhất mỗi bản ghi điểm. */
		[Key]
		public int Id { get; set; }

		/* * LOGIC PHỨC TẠP: QUAN HỆ KHÓA NGOẠI (Foreign Key)
         * StudentId: Liên kết điểm số với một sinh viên cụ thể thông qua MSSV.
         * virtual Student: Thuộc tính điều hướng giúp truy xuất thông tin cá nhân của 
         * sinh viên sở hữu bảng điểm này.
         */
		public string? StudentId { get; set; }
		public virtual Student? Student { get; set; }

		/* Thông tin môn học: Lưu cả Mã và Tên để hỗ trợ truy vấn báo cáo nhanh (Denormalization nhẹ) */
		public string? SubjectCode { get; set; }
		public string? SubjectName { get; set; }

		/* * MODULE: RÀNG BUỘC DỮ LIỆU (DATA VALIDATION)
         * [Range(0, 10)]: Đảm bảo dữ liệu nhập vào luôn nằm trong thang điểm 10.
         * Ngăn chặn các lỗi nhập liệu sai sót từ phía giảng viên hoặc người quản trị.
         */
		[Range(0, 10)]
		public decimal Attendance { get; set; } // Điểm chuyên cần (thường chiếm 10%)

		[Range(0, 10)]
		public decimal Midterm { get; set; }    // Điểm kiểm tra giữa kỳ (thường chiếm 30%)

		[Range(0, 10)]
		public decimal Final { get; set; }      // Điểm thi kết thúc học phần (thường chiếm 60%)

		/* * KẾT QUẢ TỔNG KẾT:
         * TotalScore: Điểm trung bình sau khi nhân trọng số.
         * LetterGrade: Điểm quy đổi sang hệ chữ (A, B+, B, C+, C, D+, D, F).
         */
		public decimal TotalScore { get; set; }
		public string? LetterGrade { get; set; }

		/* Thông tin về thời điểm ghi nhận kết quả học tập */
		public string? Semester { get; set; }
		public int Year { get; set; }
	}
}