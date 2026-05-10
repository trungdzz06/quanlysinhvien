/*
 * ==============================================================================
 * Tên tệp tin: ReportViewModel.cs
 * Tổng quan: Tập hợp các lớp trung gian (View Models) phục vụ cho tính năng báo cáo,
 * thống kê kết quả học tập và phân loại học lực sinh viên toàn trường.
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 05/05/2026
 * ==============================================================================
 */

using System.Collections.Generic;

namespace StudentManagementSystem.Models
{
	/// <summary>
	/// LỚP (CLASS): ReportViewModel
	/// Mục đích: Đóng gói toàn bộ dữ liệu thống kê của một đợt báo cáo (theo Khoa, Lớp hoặc Học kỳ).
	/// Chức năng: Cung cấp các con số tổng hợp để vẽ biểu đồ phân loại học lực.
	/// </summary>
	public class ReportViewModel
	{
		/* --- MODULE: CÁC TIÊU CHÍ LỌC BÁO CÁO --- */
		public string? FacultyId { get; set; } // Mã Khoa thực hiện báo cáo
		public string? ClassId { get; set; }   // Mã Lớp thực hiện báo cáo
		public string? Semester { get; set; }  // Học kỳ ghi nhận dữ liệu

		/* --- MODULE: DỮ LIỆU THỐNG KÊ TỔNG HỢP (KPIs) --- 
         * Các thuộc tính này dùng để đếm số lượng sinh viên đạt từng mức xếp loại,
         * phục vụ trực tiếp cho việc hiển thị biểu đồ tròn (Pie Chart) hoặc cột (Bar Chart).
         */
		public int ExcellentCount { get; set; } // Số lượng sinh viên Xuất sắc
		public int GoodCount { get; set; }      // Số lượng sinh viên Giỏi
		public int FairCount { get; set; }      // Số lượng sinh viên Khá
		public int AverageCount { get; set; }   // Số lượng sinh viên Trung bình
		public int PoorCount { get; set; }      // Số lượng sinh viên Yếu/Kém

		/* Danh sách chi tiết từng sinh viên trong báo cáo */
		public List<StudentGradeReport>? StudentReports { get; set; }
	}

	/// <summary>
	/// LỚP (CLASS): StudentGradeReport
	/// Mục đích: Mô tả kết quả học tập chi tiết của một sinh viên cụ thể trong danh sách báo cáo.
	/// Chức năng: Lưu trữ điểm GPA đã được quy đổi và xếp loại học lực tương ứng.
	/// </summary>
	public class StudentGradeReport
	{
		public string? MSSV { get; set; }     // Mã số sinh viên
		public string? FullName { get; set; } // Họ và tên sinh viên

		/* * LOGIC PHỨC TẠP: QUY ĐỔI ĐIỂM TRUNG BÌNH (GPA)
         * GPA10: Điểm trung bình tích lũy theo thang điểm 10 (Hệ Việt Nam).
         * GPA4: Điểm trung bình tích lũy đã quy đổi sang thang điểm 4 (Hệ quốc tế).
         */
		public decimal GPA10 { get; set; }
		public decimal GPA4 { get; set; }

		/* * LOGIC PHỨC TẠP: XẾP LOẠI HỌC LỰC
         * Dựa trên kết quả GPA để đưa ra danh hiệu: Xuất sắc, Giỏi, Khá, Trung bình, Yếu.
         */
		public string? Classification { get; set; }
	}
}