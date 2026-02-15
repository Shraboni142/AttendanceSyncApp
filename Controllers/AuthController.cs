using AttandanceSyncApp.Data;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Auth;
using AttandanceSyncApp.Services.Interfaces.Auth;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace AttandanceSyncApp.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IGoogleAuthService _googleAuthService;

        public AuthController() : base()
        {
            _googleAuthService = new GoogleAuthService();
        }

        public AuthController(IAuthService authService, IGoogleAuthService googleAuthService)
            : base(authService)
        {
            _googleAuthService = googleAuthService;
        }

        // ================= LOGIN =================

        public ActionResult Login()
        {
            if (IsAuthenticated)
            {
                if (IsAdmin)
                    return RedirectToAction("Index", "AdminDashboard");

                return RedirectToAction("Dashboard", "Attandance");
            }

            ViewBag.GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            return View();
        }

        // ================= REGISTER (GET) =================

        [HttpPost]
        public ActionResult Register(SignupRequest model)
        {
            using (var db = new ApplicationDbContext())
            {
                model.Status = "Pending";

                db.SignupRequests.Add(model);
                db.SaveChanges();
            }

            TempData["msg"] = "Please wait for admin approval";

            return RedirectToAction("Register");
        }


        // ============== SIGNUP REQUEST (POST) ==============

        [HttpPost]
        public ActionResult RegisterRequest(SignupRequest model)
        {
            using (var db = new AppDbContext())
            {
                db.SignupRequests.Add(model);
                db.SaveChanges();
            }

            TempData["msg"] = "Signup request sent for approval";
            return RedirectToAction("Register");
        }

        // ================= GOOGLE SIGN IN =================

        [HttpPost]
        public JsonResult GoogleSignIn(GoogleAuthDto googleAuth)
        {
            var sessionInfo = GetSessionInfo();

            var result = _authService.LoginWithGoogle(googleAuth, sessionInfo);

            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // ================= USER LOGIN =================

        [HttpPost]
        public JsonResult UserLogin(LoginDto loginDto)
        {
            var sessionInfo = GetSessionInfo();

            var result = _authService.LoginUser(loginDto.Email, loginDto.Password, sessionInfo);

            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // ============== MAIN SYSTEM REGISTER ==============

        [HttpPost]
        public JsonResult Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return Json(ApiResponse<UserDto>.Fail(errors));
            }

            var sessionInfo = GetSessionInfo();

            var result = _authService.RegisterUser(registerDto, sessionInfo);

            if (result.Success)
            {
                try
                {
                    var uow = new AuthUnitOfWork();

                    uow.UserApprovalStatus.Add(new UserApprovalStatus
                    {
                        UserId = result.Data.Id,
                        IsEmailVerified = true,
                        IsAdminApproved = false
                    });

                    uow.SaveChanges();
                }
                catch { }

                SetSessionCookie(result.Data.SessionToken);

                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // ================= LOGOUT =================

        [HttpPost]
        public JsonResult Logout()
        {
            var token = GetSessionToken();

            if (!string.IsNullOrEmpty(token))
                _authService.Logout(token);

            ClearSessionCookie();

            return Json(ApiResponse.Success("Logged out successfully"));
        }

        // ================= CURRENT USER =================

        [HttpGet]
        public JsonResult CurrentUser()
        {
            var token = GetSessionToken();

            if (string.IsNullOrEmpty(token))
                return Json(ApiResponse<UserDto>.Fail("Not authenticated"), JsonRequestBehavior.AllowGet);

            var result = _authService.GetCurrentUser(token);

            if (!result.Success)
                return Json(ApiResponse<UserDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);

            return Json(ApiResponse<UserDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UserLogin(string email, string password)
        {
            try
            {
                using (var db = new AttandanceSyncApp.Models.AppDbContext())
                {
                    var user = db.Employees
                        .FirstOrDefault(x =>
                            x.Email == email &&
                            x.Password == password &&
                            x.IsActive == true);

                    if (user != null)
                    {
                        Session["UserId"] = user.Id;
                        Session["UserName"] = user.Name;
                        Session["UserRole"] = user.Role;

                        return Json(new
                        {
                            success = true,
                            role = user.Role
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Invalid email or password"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }



    }
}
