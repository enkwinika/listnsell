using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Dapper;
using NLog;
using rexell.Core;
using rexell.Filters;
using rexell.Models;
using rexell.Services;

namespace rexell.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommonCore _commonCore;
        private readonly IFileUploadService _fileUploadService;
        private int? _currentUserId;

        public HomeController(ICommonCore commonCore, IFileUploadService fileUploadService)
        {
            _commonCore = commonCore ?? throw new ArgumentNullException(nameof(commonCore));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            Logger.Debug("HomeController instance created");
        }
        #region Views

        public ActionResult Index()
        {
            ViewBag.Title = "ReXell - Buy & Sell Marketplace";
            Logger.Debug("Index page accessed");
            return View();
        }

        public ActionResult Helper()
        {
            Logger.Debug("Helper page accessed");
            return View();
        }

        #endregion

        #region Authentication API

        /// <summary>
        /// User Registration
        /// POST: /Marketplace/Register
        /// </summary>
        [HttpPost]
        [ValidateModel]
        public async Task<JsonResult> Register(RegisterRequest model)
        {
            Logger.Info($"Registration attempt for email: {model?.email}");
            
            if (model == null)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Invalid request data"
                });
            }

            var result = await _commonCore.UserRegisterAsync(model);
            Logger.Info($"Registration result for {model.email}: {result.code}");
            return Json(result);
        }

        /// <summary>
        /// User Login
        /// POST: /Marketplace/LoginUser
        /// </summary>
        [HttpPost]
        [ValidateModel]
        public async Task<JsonResult> LoginUser(LoginRequest model)
        {
            Logger.Info($"Login attempt for username: {model?.username}");
            
            if (model == null || string.IsNullOrWhiteSpace(model.username) || string.IsNullOrWhiteSpace(model.password))
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Email and password are required"
                });
            }

            var result = await _commonCore.UserLoginAsync(model);
            if(result.code == "1")
            {
                var userData = result.email;

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    result.email,
                    DateTime.Now,
                    DateTime.Now.AddHours(8),
                    true,
                    userData,
                    FormsAuthentication.FormsCookiePath
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                authCookie.HttpOnly = true;
                authCookie.Secure = FormsAuthentication.RequireSSL;
                authCookie.Path = FormsAuthentication.FormsCookiePath;

                if (ticket.IsPersistent)
                {
                    authCookie.Expires = ticket.Expiration;
                }

                Response.Cookies.Add(authCookie);
                Session["Email"] = result.email;

                var identity = new GenericIdentity(result.email, "Forms");
                var principal = new GenericPrincipal(identity, null);
                HttpContext.User = principal;
                
                Logger.Info($"User logged in successfully: {result.email}");
            }
            else
            {
                Logger.Warn($"Login failed for user: {model.username}");
            }
            return Json(result);
        }

        /// <summary>
        /// Verify Email with Code
        /// POST: /Marketplace/VerifyEmailCode
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> VerifyEmailCode(string email, string code)
        {
            Logger.Info($"Email verification attempt for: {email}");
            
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Email and verification code are required"
                });
            }

            var result = await _commonCore.VerifyEmailAsync(email, code);
            Logger.Info($"Email verification result for {email}: {result.code}");
            return Json(result);
        }

        /// <summary>
        /// Resend Verification Code
        /// POST: /Marketplace/ResendCode
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ResendCode(string email)
        {
            Logger.Info($"Resend verification code for: {email}");
            
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Email is required"
                });
            }

            var result = await _commonCore.ResendVerificationCodeAsync(email);
            return Json(result);
        }

        /// <summary>
        /// Request Password Reset
        /// POST: /Marketplace/ForgotPasswordRequest
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ForgotPasswordRequest(string email)
        {
            Logger.Info($"Password reset request for: {email}");
            
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Email is required"
                });
            }

            var result = await _commonCore.RequestPasswordResetAsync(email);
            return Json(result);
        }

        /// <summary>
        /// Reset Password with Code
        /// POST: /Marketplace/ResetPasswordWithCode
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ResetPasswordWithCode(string email, string code, string newPassword)
        {
            Logger.Info($"Password reset with code for: {email}");
            
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Email, code, and new password are required"
                });
            }

            var result = await _commonCore.ResetPasswordAsync(email, code, newPassword);
            Logger.Info($"Password reset result for {email}: {result.code}");
            return Json(result);
        }

        /// <summary>
        /// Logout User
        /// POST: /Marketplace/Logout
        /// </summary>
        [HttpPost]
        public JsonResult Logout()
        {
            var email = User.Identity.Name;
            Logger.Info($"User logout: {email}");
            
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            return Json(new AjaxResults
            {
                code = "1",
                title = "Logged Out",
                message = "You have been successfully logged out"
            });
        }

        /// <summary>
        /// Check if user is authenticated
        /// GET: /Marketplace/CheckAuth
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckAuth()
        {
            bool isAuthenticated = User.Identity.IsAuthenticated;

            if (isAuthenticated)
            {
                var result = await _commonCore.GetUserByUsernameAsync(User.Identity.Name);
                return Json(new
                {
                    code = "1",
                    isAdmin = result.isAdmin,
                    isAuthenticated = isAuthenticated,
                    username = User.Identity.Name
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                code = "1",
                isAdmin = false,
                isAuthenticated = false,
                username = (string)null
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Rexell API
        /// <summary>
        /// Get filtered listings
        /// POST: /Marketplace/GetListings
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetListings(ListingFilterRequest filter)
        {
            Logger.Debug("GetListings called");
            
            if (filter == null)
            {
                filter = new ListingFilterRequest();
            }

            var result = await _commonCore.GetListingsAsync(filter);
            return Json(result);
        }

        /// <summary>
        /// Get listing details
        /// GET: /Marketplace/GetListingDetails
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetListingDetails(int id)
        {
            Logger.Debug($"GetListingDetails called for id: {id}");
            var result = await _commonCore.GetListingDetailsAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Create new listing
        /// POST: /Marketplace/CreateListing
        /// </summary>
        [HttpPost]
        [AuthenticateUser]
        [ValidateModel]
        public async Task<JsonResult> CreateListing(ListingRequest model)
        {
            Logger.Info("CreateListing called");
            
            if (model == null)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Invalid request data"
                });
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "User not found"
                });
            }

            var result = await _commonCore.CreateListingAsync(model, userId);
            Logger.Info($"Listing created with result: {result.code}");
            return Json(result);
        }

        /// <summary>
        /// Upload listing images
        /// POST: /Marketplace/UploadImages
        /// </summary>
        [HttpPost]
        [AuthenticateUser]
        public async Task<JsonResult> UploadImages()
        {
            Logger.Info("UploadImages called");
            var result = await _fileUploadService.UploadImagesAsync(Request.Files);
            return Json(result);
        }

        /// <summary>
        /// Get user's own listings
        /// GET: /Marketplace/GetMyListings
        /// </summary>
        [HttpGet]
        [AuthenticateUser]
        public async Task<JsonResult> GetMyListings()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Unauthorized",
                    message = "Please login to view your listings"
                }, JsonRequestBehavior.AllowGet);
            }

            Logger.Debug($"GetMyListings called for userId: {userId}");
            var result = await _commonCore.GetUserListingsAsync(userId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Update listing
        /// POST: /Marketplace/UpdateListing
        /// </summary>
        [HttpPost]
        [AuthenticateUser]
        [ValidateModel]
        public async Task<JsonResult> UpdateListing(int id, ListingRequest model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Unauthorized",
                    message = "Please login to update listings"
                });
            }

            Logger.Info($"UpdateListing called for listingId: {id}");
            var result = await _commonCore.UpdateListingAsync(id, userId, model);
            return Json(result);
        }

        /// <summary>
        /// Delete listing
        /// POST: /Marketplace/DeleteListing
        /// </summary>
        [HttpPost]
        [AuthenticateUser]
        public async Task<JsonResult> DeleteListing(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Unauthorized",
                    message = "Please login to delete listings"
                });
            }

            Logger.Info($"DeleteListing called for listingId: {id}");
            var result = await _commonCore.DeleteListingAsync(id, userId);
            return Json(result);
        }

        /// <summary>
        /// Mark listing as sold
        /// POST: /Marketplace/MarkAsSold
        /// </summary>
        [HttpPost]
        [AuthenticateUser]
        public async Task<JsonResult> MarkAsSold(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Unauthorized",
                    message = "Please login"
                });
            }

            Logger.Info($"MarkAsSold called for listingId: {id}");
            var result = await _commonCore.MarkAsSoldAsync(id, userId);
            return Json(result);
        }

        #endregion

        #region Admin Function

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<JsonResult> GetAdminStats()
        {
            Logger.Debug("GetAdminStats called");
            var result = await _commonCore.GetAdminStatsAsync();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<JsonResult> GetAdminListings(string status = "")
        {
            Logger.Debug($"GetAdminListings called with status: {status}");
            var result = await _commonCore.GetAdminListingsAsync(status);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<JsonResult> ApproveListing(int id)
        {
            Logger.Info($"ApproveListing called for id: {id}");
            var result = await _commonCore.ApproveListingAsync(id);
            return Json(result);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<JsonResult> RejectListing(int id, string reason)
        {
            Logger.Info($"RejectListing called for id: {id}");
            var result = await _commonCore.RejectListingAsync(id, reason);
            return Json(result);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<JsonResult> AdminDeleteListing(int id)
        {
            Logger.Info($"AdminDeleteListing called for id: {id}");
            var result = await _commonCore.AdminDeleteListingAsync(id);
            return Json(result);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<JsonResult> GetReportedListings()
        {
            Logger.Debug("GetReportedListings called");
            var result = await _commonCore.GetReportedListingsAsync();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<JsonResult> DismissReport(int id)
        {
            Logger.Info($"DismissReport called for id: {id}");
            var result = await _commonCore.DismissReportAsync(id);
            return Json(result);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<JsonResult> GetAllUsers()
        {
            Logger.Debug("GetAllUsers called");
            var result = await _commonCore.GetAllUsersForAdminAsync();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<JsonResult> ToggleUserStatus(int userId, bool isActive)
        {
            Logger.Info($"ToggleUserStatus called for userId: {userId}, isActive: {isActive}");
            var result = await _commonCore.ToggleUserStatusAsync(userId, isActive);
            return Json(result);
        }


        #endregion


        #region Categories API

        /// <summary>
        /// Get all categories
        /// GET: /Marketplace/GetCategories
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetCategories()
        {
            Logger.Debug("GetCategories called");
            var result = await _commonCore.GetCategoriesAsync();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Messages API

        /// <summary>
        /// Send message to seller
        /// POST: /Marketplace/SendMessage
        /// </summary>
        [HttpPost]
        [AuthenticateUser]
        [ValidateModel]
        public async Task<JsonResult> SendMessage(MessageRequest model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Unauthorized",
                    message = "Please login to send messages"
                });
            }

            Logger.Info($"SendMessage called from userId: {userId}");
            var result = await _commonCore.SendMessageAsync(model, userId);
            return Json(result);
        }

        /// <summary>
        /// Get user messages
        /// GET: /Marketplace/GetMessages
        /// </summary>
        [HttpGet]
        [AuthenticateUser]
        public async Task<JsonResult> GetMessages()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Unauthorized",
                    message = "Please login to view messages"
                }, JsonRequestBehavior.AllowGet);
            }

            Logger.Debug($"GetMessages called for userId: {userId}");
            var result = await _commonCore.GetUserMessagesAsync(userId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Reports API

        /// <summary>
        /// Report a listing
        /// POST: /Marketplace/ReportListing
        /// </summary>
        [HttpPost]
        [AuthenticateUser]
        [ValidateModel]
        public async Task<JsonResult> ReportListing(ReportRequest model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Unauthorized",
                    message = "Please login to report listings"
                });
            }

            Logger.Info($"ReportListing called from userId: {userId}");
            var result = await _commonCore.ReportListingAsync(model, userId);
            return Json(result);
        }

        #endregion

        #region Helper Methods

        private int GetCurrentUserId()
        {
            if (_currentUserId.HasValue)
                return _currentUserId.Value;

            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    using (var db = new SqlConnection(
                        ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                    {
                        var userId = db.QuerySingleOrDefault<int>(
                            "SELECT UserId FROM Users WHERE Email = @Email",
                            new { Email = User.Identity.Name });
                        
                        _currentUserId = userId;
                        return userId;
                    }
                }
                _currentUserId = 0;
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error getting current user ID for {User.Identity.Name}");
                _currentUserId = 0;
                return 0;
            }
        }

        #endregion
    }
}