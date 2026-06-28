namespace StudentManagementSystem.Models
{
	public class UserRoleViewModel
	{
		public string UserId { get; set; }      // MSSV hoặc TeacherId
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Role { get; set; }        // "Student", "Teacher", hoặc "Admin"
		public string Status { get; set; }      // Trạng thái tài khoản
	}
}