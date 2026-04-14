using SV22T1020670.BusinessLayers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.DomainModels;
using Microsoft.AspNetCore.Authorization;


namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.UserName = username;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập email và mật khẩu");
                return View();
            }

            //Kiểm tra thông tin đăng nhập
            var userAccount = await UserAccountService.EmployeeUserAccountDB.AuthenticateAsync(username, password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }

            //Tạo thông tin để ghi trong "giấy chứng nhận"
            WebUserData userData = new()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = [.. userAccount.RoleNames.Split(',')]
            };

            //Thiết lập phiên đăng nhập 
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userData.CreatePrincipal());

            //Quay về trang chủ
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // --- PHẦN ĐỔI MẬT KHẨU ---

        [Authorize] 
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // 1. Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            // 2. Lấy thông tin người dùng hiện tại từ Cookie
            var userData = User.GetUserData();
            if (userData == null)
            {
                return RedirectToAction("Login");
            }

            // 3. Gọi hàm xử lý từ Service/DAL
            // Hàm ChangePassword trả về bool: true (thành công), false (thất bại - do sai pass cũ)
            bool result = await UserAccountService.EmployeeUserAccountDB.ChangePassword(userData.UserId, oldPassword, newPassword);

            if (result)
            {
                ViewBag.Message = "Đổi mật khẩu thành công!";
                // Có thể logout bắt đăng nhập lại nếu muốn bảo mật cao hơn
                // return RedirectToAction("Logout"); 
                return View();
            }
            else
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác.");
                return View();
            }
        }

        // --- PHẦN QUÊN MẬT KHẨU ---

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Validate dữ liệu
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập Email.");
                return View();
            }

            // Gọi hàm ResetPassword từ DAL
            // Ở đây ta đặt mật khẩu mặc định là "1" như yêu cầu của bạn
            string defaultPassword = "1";
            bool result = await UserAccountService.EmployeeUserAccountDB.ResetPasswordAsync(email, defaultPassword);

            if (result)
            {
                // Reset thành công -> Thông báo và có thể chuyển về trang Login
                ViewBag.Message = $"Mật khẩu đã được reset thành công. Mật khẩu mới là: {defaultPassword}";
                // Hoặc return RedirectToAction("Login"); nếu muốn
                return View();
            }
            else
            {
                // Reset thất bại (do sai email hoặc lỗi hệ thống)
                ModelState.AddModelError("Error", "Email không tồn tại trong hệ thống.");
                return View();
            }
        }
    }
}