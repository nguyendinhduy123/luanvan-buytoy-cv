using buytoy.Areas.Admin.Repository;
using buytoy.Models;
using buytoy.Models.ViewModels;
using buytoy.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace buytoy.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        public AccountController(IEmailSender emailSender, UserManager<AppUserModel> userManage,
            SignInManager<AppUserModel> signInManager, DataContext context)
        {
            _userManage = userManage;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _dataContext = context;

        }
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                    TempData["success"] = "Đăng nhập thành công";
                    var receiver = "0903682500a@gmail.com";
                    var subject = "Đăng nhập trên thiết bị thành công.";
                    var message = "Đăng nhập thành công, trải nghiệm dịch vụ nhé.";

                    await _emailSender.SendEmailAsync(receiver, subject, message);
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Sai tài khoản hặc mật khẩu");
            }
            return View(loginVM);
        }
        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var Orders = await _dataContext.Orders
                .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
            ViewBag.UserEmail = userEmail;
            return View(Orders);
        }
        [HttpPost]
        public IActionResult CancelOrder(string ordercode)
        {
            var order = _dataContext.Orders.FirstOrDefault(o => o.OrderCode == ordercode);
            if (order != null && order.Status == 1)
            {
                order.Status = 3; // Đã hủy
                _dataContext.Orders.Update(order);
                _dataContext.SaveChanges();

                TempData["success"] = "Đơn hàng đã được hủy.";
            }

            return RedirectToAction("History");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create(UserModel user)
        {
            if (!user.AcceptPolicy)
            {
                TempData["warning"] = "Bạn cần đồng ý với điều khoản để tiếp tục.";
                return View(user);
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManage.Users
    .Where(u => u.Email == user.Email)
    .FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    return View(user);
                }

                var newUser = new AppUserModel
                {
                    UserName = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    ReceiveNews = user.ReceiveNews,
                    CreatedDate = DateTime.Now
                };

                IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    TempData["success"] = "Tài khoản đã được tạo thành công.";
                    return RedirectToAction("Login", "Account");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(); // optional, depends on your auth scheme
            TempData["success"] = "Bạn đã đăng xuất thành công.";
            return Redirect(returnUrl);
        }
        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Không tìm thấy địa chỉ email.";
                return RedirectToAction("ForgotPass", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                // Cập nhật token cho người dùng
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();

                var receiver = checkMail.Email;
                var subject = "Yêu cầu thay đổi mật khẩu";
                var message = "Bạn đã yêu cầu thay đổi mật khẩu. Vui lòng nhấn vào liên kết sau để đặt lại mật khẩu của bạn: " +
                    $"<a href='{Request.Scheme}://{Request.Host}/Account/NewPass?email={checkMail.Email}&token={token}'>Đặt lại mật khẩu</a>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "Email hướng dẫn thay đổi mật khẩu đã được gửi đến địa chỉ email của bạn.";
            return RedirectToAction("ForgotPass", "Account");
        }

        public IActionResult ForgotPass()
        {
            return View();
        }

        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email không tồn tại hoặc mã xác nhận không hợp lệ.";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }

        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                string newtoken = Guid.NewGuid().ToString();
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManage.UpdateAsync(checkuser);

                TempData["success"] = "Mật khẩu đã được cập nhật thành công.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email không tồn tại hoặc mã xác nhận không hợp lệ.";
                return RedirectToAction("ForgotPass", "Account");
            }
        }

        public async Task<IActionResult> UpdateAccount()
        {
            var user = await _userManage.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new UpdateAccountViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                ReceiveNews = user.ReceiveNews,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedDate = user.CreatedDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(UpdateAccountViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManage.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Dữ liệu không hợp lệ.";
                return View(model);
            }

            // Không cho đổi username & email nhưng vẫn check trùng để tránh bug nếu cố tình thay đổi từ client
            var sameUsername = await _userManage.Users.AnyAsync(u => u.UserName == model.Username && u.Id != userId);
            var sameEmail = await _userManage.Users.AnyAsync(u => u.Email == model.Email && u.Id != userId);
            if (sameUsername)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }
            if (sameEmail)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            // Cập nhật thông tin
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.ReceiveNews = model.ReceiveNews;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;


            // Nếu nhập mật khẩu cũ và mật khẩu mới
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var checkPassword = await _userManage.CheckPasswordAsync(user, model.CurrentPassword);
                if (!checkPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                var changePassResult = await _userManage.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePassResult.Succeeded)
                {
                    foreach (var error in changePassResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            var updateResult = await _userManage.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                TempData["success"] = "Thông tin đã được cập nhật.";
                return RedirectToAction("UpdateAccount");
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }



        //public async Task LoginByGoogle()
        //{
        //    // Use Google authentication scheme for challenge
        //    await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
        //        new AuthenticationProperties
        //        {
        //            RedirectUri = Url.Action("GoogleResponse")
        //        });
        //}

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                TempData["error"] = "Đăng nhập Google thất bại.";
                return RedirectToAction("Login");
            }

            // Lấy thông tin từ Google
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["error"] = "Không lấy được email từ Google.";
                return RedirectToAction("Login");
            }

            var emailName = email.Split('@')[0]; // dùng làm UserName

            // Tìm user theo cả Email và UserName (nếu có trùng, dùng user mới nhất)
            var existingUser = await _userManage.Users
                .Where(u => u.Email == email && u.UserName == emailName)
                .OrderByDescending(u => u.Id)
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                // Tạo mật khẩu giả lập
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "123456789");

                // Tạo tài khoản mới (không kiểm tra trùng)
                var newUser = new AppUserModel
                {
                    UserName = emailName,
                    Email = email,
                    PasswordHash = hashedPassword,
                    CreatedDate = DateTime.Now
                };

                var createResult = await _userManage.CreateAsync(newUser);
                if (!createResult.Succeeded)
                {
                    TempData["error"] = "Tạo tài khoản Google thất bại.";
                    return RedirectToAction("Login");
                }

                await _signInManager.SignInAsync(newUser, isPersistent: false);
                TempData["success"] = "Đăng nhập Google thành công.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Đăng nhập user đã tồn tại
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                TempData["success"] = "Đăng nhập Google thành công.";
                return RedirectToAction("Index", "Home");
            }
        }



        [HttpGet]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManage.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            var user = await _userManage.GetUserAsync(User);
            if (user == null || avatarFile == null || avatarFile.Length == 0)
            {
                TempData["error"] = "Vui lòng chọn ảnh hợp lệ.";
                return RedirectToAction("Profile");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            user.Avatar = "/uploads/avatars/" + fileName;
            await _userManage.UpdateAsync(user);

            TempData["success"] = "Ảnh đại diện đã được cập nhật.";
            return RedirectToAction("Profile");
        }

    }
}


