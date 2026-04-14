using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using SV22T1020670.DomainModels;
using SV22T1020670.Shop.AppCodes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020670.Shop.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // ========================= LOGIN / LOGOUT =========================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = "/")
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.UserName = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập Email và mật khẩu");
                return View();
            }
            var userAccount = await UserAccountService.CustomerUserAccountDB.AuthenticateAsync(username, password);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại. Sai email hoặc mật khẩu.");
                return View();
            }

            // Tạo Claims
            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.UserID.ToString(),
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = new List<string>() { "Customer" }
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userData.CreatePrincipal());

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ========================= ĐĂNG KÝ =========================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(Customer data, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName) || string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin bắt buộc.");
                return View(data);
            }

            if (data.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Mật khẩu xác nhận không khớp.");
                return View(data);
            }

            bool result = await UserAccountService.CustomerUserAccountDB.RegisterAsync(data);

            if (result)
            {
                // Đăng ký thành công -> Chuyển sang trang đăng nhập
                TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn Email khác.");
                return View(data);
            }
        }

        // ========================= CHANGE PASSWORD =========================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // 1. Kiểm tra dữ liệu nhập
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

            // 2. Lấy UserID từ Cookie
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }

            // 3. Gọi UserAccountService
            bool result = await UserAccountService.CustomerUserAccountDB.ChangePassword(userData.UserId, oldPassword, newPassword);

            if (result)
            {
                ViewBag.Message = "Đổi mật khẩu thành công!";
                ModelState.Clear(); 
                return View();
            }
            else
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác.");
                return View();
            }
        }

        // ========================= QUÊN MẬT KHẨU =========================

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
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập Email.");
                return View();
            }

            // Mật khẩu mới tạm thời
            string newPassword = "123456";

            // Gọi hàm ResetPasswordAsync đã có trong DAL của bạn
            bool result = await UserAccountService.CustomerUserAccountDB.ResetPasswordAsync(email, newPassword);

            if (result)
            {
                // Thông báo mật khẩu mới ngay trên màn hình (Dùng cho Demo/Test)
                ViewBag.Message = $"Mật khẩu đã được reset thành công. Mật khẩu mới của bạn là: {newPassword}";
                return View();
            }
            else
            {
                ModelState.AddModelError("Error", "Email này không tồn tại trong hệ thống.");
                return View();
            }
        }

        // ========================= PROFILE =========================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userData = User.GetUserData();
            if (userData == null) return RedirectToAction("Login");

            int.TryParse(userData.UserId, out int customerId);

            var customer = await CommonDataService.CustomerDB.GetAsync(customerId);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProfile(Customer data) 
        {
            // Validate
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên giao dịch không được để trống");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn Tỉnh/Thành");

            if (!ModelState.IsValid)
            {
                return View("Profile", data);
            }

            // Gọi CommonDataService để Update
            bool result = await CommonDataService.CustomerDB.UpdateAsync(data);

            if (result)
            {
                TempData["Message"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }
            else
            {
                ModelState.AddModelError("", "Không thể cập nhật thông tin.");
                return View("Profile", data);
            }
        }
    }
}