using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Dapper;
using rexell.Core;
using rexell.Models;

namespace rexell.Controllers
{
    public class MarketplaceController : Controller
    {
        #region Views

        public ActionResult Index()
        {
            ViewBag.Title = "ReXell - Buy & Sell Marketplace";
            return View();
        }

        #endregion

        #region Authentication API

        /// <summary>
        /// User Registration
        /// POST: /Marketplace/Register
        /// </summary>
        [HttpPost]
        public JsonResult Register(RegisterRequest model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Invalid request data"
                    });
                }

                var result = commonCore.UserRegister(model);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An unexpected error occurred. Please try again."
                });
            }
        }

        /// <summary>
        /// User Login
        /// POST: /Marketplace/LoginUser
        /// </summary>
        [HttpPost]
        public JsonResult LoginUser(LoginRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.username) || string.IsNullOrWhiteSpace(model.password))
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Email and password are required"
                    });
                }

                var result = commonCore.UserLogin(model);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An unexpected error occurred during login."
                });
            }
        }

        /// <summary>
        /// Verify Email with Code
        /// POST: /Marketplace/VerifyEmailCode
        /// </summary>
        [HttpPost]
        public JsonResult VerifyEmailCode(string email, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Email and verification code are required"
                    });
                }

                var result = commonCore.VerifyEmail(email, code);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An unexpected error occurred during verification."
                });
            }
        }

        /// <summary>
        /// Resend Verification Code
        /// POST: /Marketplace/ResendCode
        /// </summary>
        [HttpPost]
        public JsonResult ResendCode(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Email is required"
                    });
                }

                var result = commonCore.ResendVerificationCode(email);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An unexpected error occurred."
                });
            }
        }

        /// <summary>
        /// Request Password Reset
        /// POST: /Marketplace/ForgotPasswordRequest
        /// </summary>
        [HttpPost]
        public JsonResult ForgotPasswordRequest(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Email is required"
                    });
                }

                var result = commonCore.RequestPasswordReset(email);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An unexpected error occurred."
                });
            }
        }

        /// <summary>
        /// Reset Password with Code
        /// POST: /Marketplace/ResetPasswordWithCode
        /// </summary>
        [HttpPost]
        public JsonResult ResetPasswordWithCode(string email, string code, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Email, code, and new password are required"
                    });
                }

                var result = commonCore.ResetPassword(email, code, newPassword);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An unexpected error occurred during password reset."
                });
            }
        }

        /// <summary>
        /// Logout User
        /// POST: /Marketplace/Logout
        /// </summary>
        [HttpPost]
        public JsonResult Logout()
        {
            try
            {
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
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An error occurred during logout"
                });
            }
        }

        /// <summary>
        /// Check if user is authenticated
        /// GET: /Marketplace/CheckAuth
        /// </summary>
        [HttpGet]
        public JsonResult CheckAuth()
        {
            try
            {
                bool isAuthenticated = User.Identity.IsAuthenticated;

                return Json(new
                {
                    code = "1",
                    isAuthenticated = isAuthenticated,
                    username = isAuthenticated ? User.Identity.Name : null
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = "0",
                    isAuthenticated = false,
                    message = "Error checking authentication"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region Rexell API
        /// <summary>
        /// Get filtered listings
        /// POST: /Marketplace/GetListings
        /// </summary>
        [HttpPost]
        public JsonResult GetListings(ListingFilterRequest filter)
        {
            try
            {
                if (filter == null)
                {
                    filter = new ListingFilterRequest();
                }

                var result = commonCore.GetListings(filter);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An error occurred while retrieving listings"
                });
            }
        }

        /// <summary>
        /// Get listing details
        /// GET: /Marketplace/GetListingDetails
        /// </summary>
        [HttpGet]
        public JsonResult GetListingDetails(int id)
        {
            try
            {
                var result = commonCore.GetListingDetails(id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An error occurred while retrieving listing details"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Create new listing
        /// POST: /Marketplace/CreateListing
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult CreateListing(ListingRequest model)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Unauthorized",
                        message = "Please login to create a listing"
                    });
                }

                if (model == null)
                {
                    return Json(new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Invalid request data"
                    });
                }

                // Get user ID from authentication
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

                var result = commonCore.CreateListing(model, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "An error occurred while creating listing"
                });
            }
        }

        /// <summary>
        /// Upload listing images
        /// POST: /Marketplace/UploadImages
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult UploadImages()
        {
            try
            {
                var uploadedFiles = new List<string>();

                if (Request.Files.Count > 0)
                {
                    var uploadPath = Server.MapPath("~/Content/uploads/listings/");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    for (int i = 0; i < Request.Files.Count && i < 5; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            // Validate file size (max 10MB)
                            if (file.ContentLength > 10 * 1024 * 1024)
                            {
                                return Json(new AjaxResults
                                {
                                    code = "0",
                                    title = "Error",
                                    message = "File size must be less than 10MB"
                                });
                            }

                            // Validate file type
                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                            var extension = Path.GetExtension(file.FileName).ToLower();

                            if (!allowedExtensions.Contains(extension))
                            {
                                return Json(new AjaxResults
                                {
                                    code = "0",
                                    title = "Error",
                                    message = "Only image files are allowed (jpg, jpeg, png, gif)"
                                });
                            }

                            // Generate unique filename
                            var fileName = $"{Guid.NewGuid()}{extension}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            // Save file
                            file.SaveAs(filePath);

                            // Store relative path
                            uploadedFiles.Add($"/Content/uploads/listings/{fileName}");
                        }
                    }
                }

                return Json(new AjaxResults
                {
                    code = "1",
                    title = "Success",
                    message = "Images uploaded successfully",
                    data = uploadedFiles
                });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to upload images"
                });
            }
        }

        /// <summary>
        /// Get user's own listings
        /// GET: /Marketplace/GetMyListings
        /// </summary>
        [HttpGet]
        [Authorize]
        public JsonResult GetMyListings()
        {
            try
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

                var result = commonCore.GetUserListings(userId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve listings"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Update listing
        /// POST: /Marketplace/UpdateListing
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult UpdateListing(int id, ListingRequest model)
        {
            try
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

                var result = commonCore.UpdateListing(id, userId, model);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to update listing"
                });
            }
        }

        /// <summary>
        /// Delete listing
        /// POST: /Marketplace/DeleteListing
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult DeleteListing(int id)
        {
            try
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

                var result = commonCore.DeleteListing(id, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to delete listing"
                });
            }
        }

        /// <summary>
        /// Mark listing as sold
        /// POST: /Marketplace/MarkAsSold
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult MarkAsSold(int id)
        {
            try
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

                var result = commonCore.MarkAsSold(id, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to mark as sold"
                });
            }
        }

        #endregion

        #region Categories API

        /// <summary>
        /// Get all categories
        /// GET: /Marketplace/GetCategories
        /// </summary>
        [HttpGet]
        public JsonResult GetCategories()
        {
            try
            {
                var result = commonCore.GetCategories();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve categories"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Messages API

        /// <summary>
        /// Send message to seller
        /// POST: /Marketplace/SendMessage
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult SendMessage(MessageRequest model)
        {
            try
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

                var result = commonCore.SendMessage(model, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to send message"
                });
            }
        }

        /// <summary>
        /// Get user messages
        /// GET: /Marketplace/GetMessages
        /// </summary>
        [HttpGet]
        [Authorize]
        public JsonResult GetMessages()
        {
            try
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

                var result = commonCore.GetUserMessages(userId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve messages"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Reports API

        /// <summary>
        /// Report a listing
        /// POST: /Marketplace/ReportListing
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult ReportListing(ReportRequest model)
        {
            try
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

                var result = commonCore.ReportListing(model, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to report listing"
                });
            }
        }

        #endregion

        #region Helper Methods

        private int GetCurrentUserId()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    // Assuming you store user ID in the identity
                    // Modify based on your authentication implementation
                    var userIdClaim = User.Identity.Name; // Or use claims

                    // If you store email in Identity.Name, query the database
                    using (var db = new System.Data.SqlClient.SqlConnection(
                        System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                    {
                        var userId = db.QuerySingleOrDefault<int>(
                            "SELECT UserId FROM Users WHERE Email = @Email",
                            new { Email = User.Identity.Name });
                        return userId;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}