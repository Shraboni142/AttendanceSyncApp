using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Auth;
using AttandanceSyncApp.Services.Interfaces.Auth;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;


// ADD THIS LINE ↓↓↓
using AdminSessionDto = AttandanceSyncApp.Models.DTOs.Admin.SessionDto;



namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Handles authentication-related operations such as login, logout,
    /// and third-party authentication (e.g., Google OAuth).
    /// </summary>
    public class AuthController : BaseController
    {
        /// Google authentication service.
        private readonly IGoogleAuthService _googleAuthService;

        public AuthController()
        {
            _googleAuthService = new GoogleAuthService();
        }

        public AuthController(IAuthService authService, IGoogleAuthService googleAuthService)
            : base(authService)
        {
            _googleAuthService = googleAuthService;
        }




        // GET: Auth/Login
        public ActionResult Login()
        {
            if (IsAuthenticated) // check if the user is authenticated or not
            {
                // if the user is authenticated then check if the user is admin or not if admin recit to 'AdminDashboard'
                if (IsAdmin)
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                return RedirectToAction("Dashboard", "Attandance");
            }

            // if not then return view with the google client id configuration from the configuration manger for (google / email) login
            ViewBag.GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            return View();
        }

        // GET: Auth/Register
        public ActionResult Register()
        {
            using (var db = new AppDbContext())
            {
                ViewBag.Employees = db.Employees
                    .Where(e => e.IsActive)
                    .ToList();

                // ✅ ADD THIS (Email dropdown)
                ViewBag.EmployeeEmails = db.Employees
                    .Where(e => e.IsActive && e.Email != null && e.Email != "")
                    .Select(e => e.Email)
                    .Distinct()
                    .ToList();

                ViewBag.Companies = db.Companies
                    .Where(c => c.Status == "Active")
                    .ToList();

                ViewBag.Tools = db.Tools
                    .Where(t => t.IsActive)
                    .ToList();
            }

            return View();
        }

        // ✅ NEW REQUIREMENT IMPLEMENTATION
        // POST: Auth/GoogleSignIn
        // POST: Auth/GoogleSignIn
        [HttpPost]

   
        public JsonResult GoogleSignIn(GoogleAuthDto googleAuth)
        {
            try
            {
                if (googleAuth == null || string.IsNullOrEmpty(googleAuth.IdToken))
                {
                    return Json(ApiResponse<UserDto>.Fail("Invalid Google data."));
                }

                var sessionInfo = GetSessionInfo();

                // use existing service (NO new Session entity needed)
                var result = _authService.RegisterWithGoogle(googleAuth, sessionInfo);

                if (result.Success)
                {
                    SetSessionCookie(result.Data.SessionToken);

                    return Json(ApiResponse<UserDto>.Success(result.Data, "Login successful"));
                }

                return Json(ApiResponse<UserDto>.Fail(result.Message));
            }
            catch (Exception ex)
            {
                return Json(ApiResponse<UserDto>.Fail("Operation failed"));
            }
        }





        // POST: Auth/GoogleSignIn

        [HttpGet]
        public async Task<ActionResult> GoogleLoginCallback(string code)
        {
            try
            {
                var email = await _googleAuthService.GetEmailFromGoogle(code);

                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Google authentication failed.";
                    return RedirectToAction("Login");
                }

                using (SqlConnection con = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["AttandanceSyncConnection"].ConnectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE Email=@Email AND IsApproved=1",
                    con);

                    cmd.Parameters.AddWithValue("@Email", email);

                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        Session["UserEmail"] = email;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["Error"] = "Access denied.";
                        return RedirectToAction("Login");
                    }
                }
            }
            catch
            {
                TempData["Error"] = "Operation failed.";
                return RedirectToAction("Login");
            }
        }





        // POST: Auth/GoogleSignUp
        [HttpPost]
        public JsonResult GoogleSignUp(GoogleAuthDto googleAuth)
        {
            // Collect session-related information (IP, browser, device, etc.)
            var sessionInfo = GetSessionInfo();
            // Attempt to register the user using Google authentication data
            var result = _authService.RegisterWithGoogle(googleAuth, sessionInfo);

            // If registration is successful,
            // store the session token in a cookie and return success response
            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            // If registration fails, return error response
            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // POST: Auth/AdminLogin
        [HttpPost]
        public JsonResult AdminLogin(string email, string password)
        {
            // Collect session-related information
            var sessionInfo = GetSessionInfo();
            // Attempt to authenticate an admin user
            var result = _authService.LoginAdmin(email, password, sessionInfo);

            // If login is successful,
            // store the session token and return success response
            if (result.Success)
            {
                // set the session cookie for auto login for next few days
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            // If login fails, return error response
            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // POST: Auth/UserLogin
        [HttpPost]
        public JsonResult UserLogin(LoginDto loginDto)
        {
            // Collect session-related information
            var sessionInfo = GetSessionInfo();

            // Attempt to authenticate a regular user using email and password
            var result = _authService.LoginUser(loginDto.Email, loginDto.Password, sessionInfo);

            // If login is successful,
            // store the session token and return success response
            if (result.Success)
            {
                // set the session cookie for auto login for next few days
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            // If login fails, return error response
            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // POST: Auth/Register
        [HttpPost]
        public JsonResult Register(RegisterDto registerDto)
        {
            // Validate incoming registration data
            if (!ModelState.IsValid)
            {
                // Collect validation error messages
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                // Return validation failure response
                return Json(ApiResponse<UserDto>.Fail(errors));
            }

            // Collect session-related information
            var sessionInfo = GetSessionInfo();

            // Attempt to register a new user
            var result = _authService.RegisterUser(registerDto, sessionInfo);

            // If registration is successful,
            // store the session token and return success response
            if (result.Success)
            {
                // set the session cookie for auto login for next few days
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            // If registration fails, return error response
            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // POST: Auth/Logout
        [HttpPost]
        public JsonResult Logout()
        {
            // Retrieve the current session token
            var token = GetSessionToken();

            // If a session exists, invalidate it
            if (!string.IsNullOrEmpty(token))
            {
                _authService.Logout(token);
            }

            // Clear session cookie from the client
            ClearSessionCookie();

            // Return logout success response
            return Json(ApiResponse.Success("Logged out successfully"));
        }


        // GET: Auth/CurrentUser
        [HttpGet]
        public JsonResult CurrentUser()
        {
            // Retrieve the current session token
            var token = GetSessionToken();

            // If no token exists, user is not authenticated
            if (string.IsNullOrEmpty(token))
            {
                return Json(ApiResponse<UserDto>.Fail("Not authenticated"), JsonRequestBehavior.AllowGet);
            }

            // Attempt to retrieve the current user from the session token
            var result = _authService.GetCurrentUser(token);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<UserDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return authenticated user information
            return Json(ApiResponse<UserDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: Auth/GoogleCallback
        public ActionResult GoogleCallback(string code, string state, string error)
        {
            // If Google authentication returns an error,
            // display the error on the login page
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
                return View("Login");
            }

            // Google authentication is handled on the client side,
            // so simply redirect back to the login page
            return RedirectToAction("Login");
        }
    }
}
