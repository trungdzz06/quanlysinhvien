/* ==============================================================================
 * Tên tệp tin: Program.cs
 * Tổng quan: Cấu hình dịch vụ (Services) và luồng xử lý (Middleware) của ứng dụng.
 * Chức năng: 
 * - Thiết lập kết nối cơ sở dữ liệu SQL Server.
 * - Cấu hình hệ thống xác thực người dùng bằng Cookie (Authentication).
 * - Định nghĩa lộ trình điều hướng mặc định (Routing).
 * Tác giả: Nhóm 4
 * Ngày tạo/sửa đổi: 10/05/2026
 * ==============================================================================
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using StudentManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ CÁC DỊCH VỤ (DEPENDENCY INJECTION) ---

// Hỗ trợ mô hình MVC (Model-View-Controller)
builder.Services.AddControllersWithViews();

// Cấu hình Database: Sử dụng Entity Framework Core với SQL Server
// Chuỗi kết nối "DefaultConnection" được lấy từ tệp appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Xác thực (Authentication): Sử dụng Cookie để duy trì đăng nhập
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";       // Đường dẫn trang đăng nhập
		options.LogoutPath = "/Account/Logout";     // Đường dẫn trang đăng xuất
		options.AccessDeniedPath = "/Account/AccessDenied"; // Trang báo lỗi khi không có quyền
		options.ExpireTimeSpan = TimeSpan.FromHours(2);      // Cookie hết hạn sau 2 giờ làm việc
		options.SlidingExpiration = true;           // Gia hạn thời gian nếu người dùng còn hoạt động
	});

var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---

// Xử lý lỗi và bảo mật HSTS khi ứng dụng chạy thực tế (Production)
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

// Chuyển hướng mọi yêu cầu HTTP sang HTTPS để bảo mật dữ liệu
app.UseHttpsRedirection();

// Cho phép truy cập các tệp tĩnh (CSS, JS, Hình ảnh) trong thư mục wwwroot
app.UseStaticFiles();

// Kích hoạt bộ máy điều hướng (Routing)
app.UseRouting();

// Middleware xác thực: Kiểm tra danh tính người dùng (Ai đang truy cập?)
app.UseAuthentication();

// Middleware phân quyền: Kiểm tra quyền hạn (Người dùng có được phép truy cập trang này không?)
app.UseAuthorization();

// --- 3. ĐỊNH NGHĨA LỘ TRÌNH (ROUTES) ---

// Mặc định khi chạy ứng dụng sẽ dẫn trực tiếp vào trang Đăng nhập (Account/Login)
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}");

// Khởi chạy ứng dụng
app.Run();