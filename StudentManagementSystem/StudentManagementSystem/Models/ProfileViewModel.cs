using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class ProfileViewModel
    {
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }
    }
}
