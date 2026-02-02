using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Auth;
using AttandanceSyncApp.Services.Interfaces.Auth;

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

        public ActionResult Login()
        {
            if (IsAuthenticated)
            {
                if (IsAdmin)
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                return RedirectToAction("Dashboard", "Attandance");
            }

            ViewBag.GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            return View();
        }

        public ActionResult Register()
        {
            if (IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Attandance");
            }

            ViewBag.GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            return View();
        }

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

        [HttpPost]
        public JsonResult GoogleSignUp(GoogleAuthDto googleAuth)
        {
            var sessionInfo = GetSessionInfo();
            var result = _authService.RegisterWithGoogle(googleAuth, sessionInfo);

            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        [HttpPost]
        public JsonResult AdminLogin(string email, string password)
        {
            var sessionInfo = GetSessionInfo();
            var result = _authService.LoginAdmin(email, password, sessionInfo);

            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

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

        // ================== MAIN REGISTER METHOD ==================

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
                // ===== NEW USER MANAGEMENT LAYER (ADD ONLY - NO CHANGE) =====
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
                catch
                {
                    // silently ignore - main registration must not break
                }
                // ============================================================

                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // ============================================================

        [HttpPost]
        public JsonResult Logout()
        {
            var token = GetSessionToken();

            if (!string.IsNullOrEmpty(token))
            {
                _authService.Logout(token);
            }

            ClearSessionCookie();

            return Json(ApiResponse.Success("Logged out successfully"));
        }

        [HttpGet]
        public JsonResult CurrentUser()
        {
            var token = GetSessionToken();

            if (string.IsNullOrEmpty(token))
            {
                return Json(ApiResponse<UserDto>.Fail("Not authenticated"), JsonRequestBehavior.AllowGet);
            }

            var result = _authService.GetCurrentUser(token);

            if (!result.Success)
            {
                return Json(ApiResponse<UserDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<UserDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GoogleCallback(string code, string state, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
                return View("Login");
            }

            return RedirectToAction("Login");
        }
    }
}
