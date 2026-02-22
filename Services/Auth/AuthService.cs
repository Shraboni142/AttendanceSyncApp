using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Auth;
using System;
using System.Linq;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Services.Auth
{
    /// <summary>
    /// Service for handling user authentication and registration.
    /// Supports Google OAuth, traditional email/password, and admin authentication.
    /// Manages user sessions and account linking.
    /// </summary>
    public class AuthService : IAuthService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// Google authentication service for OAuth validation.
        private readonly IGoogleAuthService _googleAuthService;

        /// Session management service for user sessions.
        private readonly ISessionService _sessionService;

        /// <summary>
        /// Initializes a new AuthService with required dependencies.
        /// </summary>
        public AuthService(
            IAuthUnitOfWork unitOfWork,
            IGoogleAuthService googleAuthService,
            ISessionService sessionService)
        {
            _unitOfWork = unitOfWork;
            _googleAuthService = googleAuthService;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Generates Google OAuth login URL
        /// </summary>
        public string GetGoogleLoginUrl()
        {
            var clientId = "950626062908-qjitgrv3dat4tvjh0krktasibvnctapc.apps.googleusercontent.com";

            var redirectUri = "https://localhost:44340/Auth/GoogleLoginCallback";

            var url =
                "https://accounts.google.com/o/oauth2/v2/auth" +
                "?client_id=" + Uri.EscapeDataString(clientId) +
                "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                "&response_type=code" +   // ✅ FIXED
                "&scope=openid email profile" +
                "&access_type=offline" +
                "&prompt=select_account";

            return url;
        }



        /// <summary>
        /// Authenticates a user via Google OAuth.
        /// </summary>
        public ServiceResult<UserDto> LoginWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo)
        {
            try
            {
                var googleResult = _googleAuthService.ValidateIdToken(googleAuth.IdToken);

                if (!googleResult.Success)
                    return ServiceResult<UserDto>.FailureResult(googleResult.Message);

                var googleUser = googleResult.Data;

                var user = _unitOfWork.Users.GetByEmail(googleUser.Email);

                if (user == null)
                {
                    return ServiceResult<UserDto>.FailureResult(
                        "Your account is not approved by admin.");
                }

                if (!user.IsActive)
                {
                    return ServiceResult<UserDto>.FailureResult(
                        "Your account is inactive.");
                }

                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleUser.GoogleId;
                    user.ProfilePicture = googleUser.Picture;
                    user.UpdatedAt = DateTime.Now;

                    _unitOfWork.Users.Update(user);
                    _unitOfWork.SaveChanges();
                }

                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);

                if (!sessionResult.Success)
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);

                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);

                return ServiceResult<UserDto>.SuccessResult(
                    userDto,
                    "Login successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult(ex.Message);
            }
        }

        public ServiceResult<UserDto> LoginAdmin(string email, string password, SessionDto sessionInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return ServiceResult<UserDto>.FailureResult("Email and password are required");
                }

                var user = _unitOfWork.Users.GetByEmail(email);

                if (user == null)
                {
                    return ServiceResult<UserDto>.FailureResult("Invalid email or password");
                }

                if (user.Role != "ADMIN")
                {
                    return ServiceResult<UserDto>.FailureResult("Admin access required");
                }

                if (!user.IsActive)
                {
                    return ServiceResult<UserDto>.FailureResult("Account is deactivated");
                }

                if (!EncryptionHelper.VerifyPassword(password, user.Password))
                {
                    return ServiceResult<UserDto>.FailureResult("Invalid email or password");
                }

                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);

                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);

                return ServiceResult<UserDto>.SuccessResult(userDto, "Admin login successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Login failed: {ex.Message}");
            }
        }

        public ServiceResult<UserDto> LoginUser(string email, string password, SessionDto sessionInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return ServiceResult<UserDto>.FailureResult("Email and password are required");
                }

                var user = _unitOfWork.Users.GetByEmail(email.Trim().ToLower());

                if (user == null)
                {
                    return ServiceResult<UserDto>.FailureResult("Invalid email or password");
                }

                if (string.IsNullOrEmpty(user.Password))
                {
                    return ServiceResult<UserDto>.FailureResult("This account uses Google sign-in. Please sign in with Google.");
                }

                if (!user.IsActive)
                {
                    return ServiceResult<UserDto>.FailureResult("Account is deactivated");
                }

                if (!EncryptionHelper.VerifyPassword(password, user.Password))
                {
                    return ServiceResult<UserDto>.FailureResult("Invalid email or password");
                }

                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);

                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);

                return ServiceResult<UserDto>.SuccessResult(userDto, "Login successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Login failed: {ex.Message}");
            }
        }

        public ServiceResult<UserDto> RegisterWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo)
        {
            try
            {
                // Validate Google token using existing service
                var googleResult = _googleAuthService.ValidateIdToken(googleAuth.IdToken);

                if (!googleResult.Success)
                    return ServiceResult<UserDto>.FailureResult("Invalid Google login.");

                var googleUser = googleResult.Data;

                // ONLY check email exists in Users table
                var user = _unitOfWork.Users.GetByEmail(googleUser.Email);

                if (user == null)
                {
                    return ServiceResult<UserDto>.FailureResult(
                        "Access denied. Your email is not registered in the system.");
                }

                // ===============================
                // STEP 3: AUTO TOOL ACCESS 🔥
                // ===============================

                var hasAccess = _unitOfWork.UserTools
                    .HasActiveAssignment(user.Id, 1); // 1 = Attendance Tool ID

                if (!hasAccess)
                {
                    var userTool = new UserTool
                    {
                        UserId = user.Id,
                        ToolId = 1,
                        IsActive = true,
                        AssignedAt = DateTime.Now,
                        CreatedAt = DateTime.Now
                    };

                    _unitOfWork.UserTools.Add(userTool);
                    _unitOfWork.SaveChanges();
                }

                // ===============================
                // Create session
                // ===============================

                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);

                if (!sessionResult.Success)
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);

                // Map user to DTO
                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);

                return ServiceResult<UserDto>.SuccessResult(
                    userDto,
                    "Login successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult(ex.Message);
            }
        }



        public ServiceResult<UserDto> RegisterUser(RegisterDto registerDto, SessionDto sessionInfo)
        {
            try
            {
                if (registerDto == null)
                {
                    return ServiceResult<UserDto>.FailureResult("Registration data is required");
                }

                if (string.IsNullOrEmpty(registerDto.Name) ||
                    string.IsNullOrEmpty(registerDto.Email) ||
                    string.IsNullOrEmpty(registerDto.Password))
                {
                    return ServiceResult<UserDto>.FailureResult("Name, email, and password are required");
                }

                if (registerDto.Password.Length < 8)
                {
                    return ServiceResult<UserDto>.FailureResult("Password must be at least 8 characters");
                }

                if (_unitOfWork.Users.EmailExists(registerDto.Email.Trim().ToLower()))
                {
                    return ServiceResult<UserDto>.FailureResult("An account with this email already exists");
                }

                var user = new User
                {
                    Name = registerDto.Name.Trim(),
                    Email = registerDto.Email.Trim().ToLower(),
                    Password = EncryptionHelper.HashPassword(registerDto.Password),
                    Role = "USER",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.Users.Add(user);
                _unitOfWork.SaveChanges();

                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);

                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);

                return ServiceResult<UserDto>.SuccessResult(userDto, "Registration successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Registration failed: {ex.Message}");
            }
        }

        public ServiceResult Logout(string sessionToken)
        {
            return _sessionService.EndSession(sessionToken);
        }

        public ServiceResult<UserDto> GetCurrentUser(string sessionToken)
        {
            try
            {
                var sessionResult = _sessionService.GetActiveSession(sessionToken);

                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var user = _unitOfWork.Users.GetById(sessionResult.Data.UserId);

                if (user == null || !user.IsActive)
                {
                    return ServiceResult<UserDto>.FailureResult("User not found or inactive");
                }

                var userDto = MapToUserDto(user, sessionToken);

                return ServiceResult<UserDto>.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public bool ValidateSession(string sessionToken)
        {
            var result = _sessionService.GetActiveSession(sessionToken);
            return result.Success;
        }

        private UserDto MapToUserDto(User user, string sessionToken)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ProfilePicture = user.ProfilePicture,
                SessionToken = sessionToken,
                IsActive = user.IsActive
            };
        }
    }
}
