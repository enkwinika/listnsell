using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Dapper;
using rexell.Core;
using rexell.Models;


namespace rexell.Controllers
{
    public class HomeController : Controller
    {
        #region Views

        public ActionResult Index()
        {
            ViewBag.Title = "ReXell - Buy & Sell Marketplace";
            return View();
        }

        /// <summary>
        /// Verify email address
        /// GET: /Home/VerifyEmail?code=xxx
        /// </summary>
        [HttpGet]
        public ActionResult VerifyEmail(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                ViewBag.Success = false;
                ViewBag.Message = "Invalid verification link.";
                return View();
            }

            var result = commonCore.VerifyEmailAddress(code);
            ViewBag.Success = result.code == "1";
            ViewBag.Message = result.message;
            ViewBag.Title = result.title;
            
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
                if(result.code == "1")
                {// Create user data string: userId|email|username
                    var userData = result.email;

                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                        1,                                      // version
                        result.email,                        // user name (will be in User.Identity.Name)
                        DateTime.Now,                           // issue time
                        DateTime.Now.AddHours(8),              // expiration time
                        true,                       // persistent?
                        userData,                               // user data
                        FormsAuthentication.FormsCookiePath     // cookie path
                    );

                    // Encrypt the ticket
                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                    // Create cookie
                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    authCookie.HttpOnly = true;
                    authCookie.Secure = FormsAuthentication.RequireSSL;
                    authCookie.Path = FormsAuthentication.FormsCookiePath;

                    if (ticket.IsPersistent)
                    {
                        authCookie.Expires = ticket.Expiration;
                    }

                    // Add cookie to response
                    Response.Cookies.Add(authCookie);

                    // Set session variables
                    //Session["UserId"] = result.userId;
                    Session["Email"] = result.email;
                    //Session["Username"] = result.username;

                    // Manually set the current user for this request
                    var identity = new GenericIdentity(result.email, "Forms");
                    var principal = new GenericPrincipal(identity, null);
                    HttpContext.User = principal;
                }
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

                var result = commonCore.GetUserByUsername(User.Identity.Name);
                return Json(new
                {
                    code = "1",
                    isAdmin = result.isAdmin,
                    isAuthenticated = isAuthenticated,
                    username = isAuthenticated ? User.Identity.Name : null,
                    name = isAuthenticated ? User.Identity.Name : null
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

        /// <summary>
        /// Submit Support Ticket
        /// POST: /Home/SubmitSupportTicket
        /// </summary>
        [HttpPost]
        public JsonResult SubmitSupportTicket(string title, string message)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new AjaxResults { code = "0", title = "Unauthorized", message = "You must be logged in to submit a support ticket" });
                }

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                {
                    return Json(new AjaxResults { code = "0", title = "Validation Error", message = "Title and message are required" });
                }

                if (title.Length > 200)
                {
                    return Json(new AjaxResults { code = "0", title = "Validation Error", message = "Title must be 200 characters or less" });
                }

                if (message.Length > 2000)
                {
                    return Json(new AjaxResults { code = "0", title = "Validation Error", message = "Message must be 2000 characters or less" });
                }

                string userEmail = User.Identity.Name;
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    var userId = connection.QueryFirstOrDefault<int?>("SELECT UserId as Id FROM Users WHERE Email = @Email", new { Email = userEmail });

                    if (userId == null)
                    {
                        return Json(new AjaxResults { code = "0", title = "Error", message = "User not found" });
                    }

                    var sql = @"INSERT INTO SupportTickets (UserId, UserEmail, Title, Message, Status, CreatedAt, UpdatedAt) VALUES (@UserId, @UserEmail, @Title, @Message, @Status, @CreatedAt, @UpdatedAt)";
                    connection.Execute(sql, new { UserId = userId.Value, UserEmail = userEmail, Title = title, Message = message, Status = "Open", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
                }

                // Send email notifications (don't fail if this errors)
                try
                {
                    SendSupportTicketEmails(userEmail, title, message);
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the ticket submission
                    System.Diagnostics.Debug.WriteLine($"Email notification failed: {emailEx.Message}");
                }

                return Json(new AjaxResults { code = "1", title = "Success", message = "Support ticket submitted successfully. We'll respond within 24 hours." });
            }
            catch (Exception ex)
            {
                // Return detailed error for debugging
                return Json(new AjaxResults { code = "0", title = "Error", message = $"Error: {ex.Message}. Inner: {ex.InnerException?.Message}" });
            }
        }

        private void SendSupportTicketEmails(string userEmail, string ticketTitle, string ticketMessage)
        {
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            var smtpEnableSSL = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSSL"]);
            var fromAddress = ConfigurationManager.AppSettings["EmailFromAddress"];
            var fromName = ConfigurationManager.AppSettings["EmailFromName"];
            var adminEmail = ConfigurationManager.AppSettings["AdminEmail"];

            // Accept all SSL certificates (for self-signed or internal certificates)
            System.Net.ServicePointManager.ServerCertificateValidationCallback = 
                delegate { return true; };

            using (var smtpClient = new System.Net.Mail.SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = smtpEnableSSL;

                var adminMail = new System.Net.Mail.MailMessage { From = new System.Net.Mail.MailAddress(fromAddress, fromName), Subject = $"New Support Ticket: {ticketTitle}", Body = $"<html><body style='font-family:Arial,sans-serif;'><div style='max-width:600px;margin:0 auto;padding:20px;background:#f8f9fa;'><div style='background:linear-gradient(135deg,#cb0c9f 0%,#7928ca 100%);padding:30px;border-radius:10px 10px 0 0;'><h1 style='color:white;margin:0;'>New Support Ticket</h1></div><div style='background:white;padding:30px;border-radius:0 0 10px 10px;'><p><strong>From:</strong> {userEmail}</p><p><strong>Subject:</strong> {ticketTitle}</p><p><strong>Message:</strong></p><p style='white-space:pre-wrap;background:#f8f9fa;padding:15px;border-radius:5px;'>{ticketMessage}</p></div></div></body></html>", IsBodyHtml = true };
                adminMail.To.Add(adminEmail);

                var userMail = new System.Net.Mail.MailMessage { From = new System.Net.Mail.MailAddress(fromAddress, fromName), Subject = "Support Ticket Received", Body = $"<html><body style='font-family:Arial,sans-serif;'><div style='max-width:600px;margin:0 auto;padding:20px;background:#f8f9fa;'><div style='background:linear-gradient(135deg,#cb0c9f 0%,#7928ca 100%);padding:30px;border-radius:10px 10px 0 0;'><h1 style='color:white;margin:0;'>Thank You!</h1></div><div style='background:white;padding:30px;border-radius:0 0 10px 10px;'><p>We've received your support ticket:</p><p><strong>Subject:</strong> {ticketTitle}</p><p style='white-space:pre-wrap;background:#f8f9fa;padding:15px;border-radius:5px;'>{ticketMessage}</p><p style='background:#fff3cd;padding:15px;border-radius:5px;margin-top:20px;'>⏱️ We typically respond within 24 hours.</p><p style='margin-top:20px;'>Best regards,<br><strong>listNsell Support</strong></p></div></div></body></html>", IsBodyHtml = true };
                userMail.To.Add(userEmail);

                smtpClient.Send(adminMail);
                smtpClient.Send(userMail);
            }
        }

        [HttpGet]
        public JsonResult GetSupportTickets()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { code = "0", message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
                }

                var result = commonCore.GetUserByUsername(User.Identity.Name);
                if (!result.isAdmin)
                {
                    return Json(new { code = "0", message = "Admin access required" }, JsonRequestBehavior.AllowGet);
                }

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    var ticketsData = connection.Query(@"
                        SELECT Id, UserId, UserEmail, Title, Message, Status, AdminResponse, RespondedAt, RespondedBy, CreatedAt, UpdatedAt 
                        FROM SupportTickets 
                        ORDER BY CreatedAt DESC").ToList();

                    // Convert to proper objects for JSON serialization
                    var tickets = ticketsData.Select(t => new
                    {
                        Id = (int)t.Id,
                        UserId = (int)t.UserId,
                        UserEmail = (string)t.UserEmail,
                        Title = (string)t.Title,
                        Message = (string)t.Message,
                        Status = (string)t.Status,
                        AdminResponse = t.AdminResponse as string,
                        RespondedAt = t.RespondedAt as DateTime?,
                        RespondedBy = t.RespondedBy as string,
                        CreatedAt = (DateTime)t.CreatedAt,
                        UpdatedAt = (DateTime)t.UpdatedAt
                    }).ToList();

                    return Json(new { code = "1", tickets = tickets }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = "0", message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RespondToTicket(int ticketId, string responseMessage)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new AjaxResults { code = "0", title = "Unauthorized", message = "You must be logged in" });
                }

                var result = commonCore.GetUserByUsername(User.Identity.Name);
                if (!result.isAdmin)
                {
                    return Json(new AjaxResults { code = "0", title = "Unauthorized", message = "Admin access required" });
                }

                if (string.IsNullOrWhiteSpace(responseMessage))
                {
                    return Json(new AjaxResults { code = "0", title = "Validation Error", message = "Response message is required" });
                }

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    
                    var ticket = connection.QueryFirstOrDefault(@"
                        SELECT Id, UserId, UserEmail, Title, Message, Status, CreatedAt 
                        FROM SupportTickets 
                        WHERE Id = @TicketId", new { TicketId = ticketId });

                    if (ticket == null)
                    {
                        return Json(new AjaxResults { code = "0", title = "Error", message = "Ticket not found" });
                    }

                    connection.Execute(@"
                        UPDATE SupportTickets 
                        SET AdminResponse = @Response, 
                            RespondedAt = @RespondedAt, 
                            RespondedBy = @RespondedBy, 
                            Status = 'Responded', 
                            UpdatedAt = @UpdatedAt 
                        WHERE Id = @TicketId", 
                        new { 
                            Response = responseMessage, 
                            RespondedAt = DateTime.Now, 
                            RespondedBy = User.Identity.Name, 
                            UpdatedAt = DateTime.Now, 
                            TicketId = ticketId 
                        });
                }

                try
                {
                    SendTicketResponseEmail(ticketId, responseMessage);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email failed: {emailEx.Message}");
                }

                return Json(new AjaxResults { code = "1", title = "Success", message = "Response sent successfully" });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResults { code = "0", title = "Error", message = $"Error: {ex.Message}" });
            }
        }

        private void SendTicketResponseEmail(int ticketId, string responseMessage)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                connection.Open();
                var ticket = connection.QueryFirstOrDefault(@"
                    SELECT UserEmail, Title, Message 
                    FROM SupportTickets 
                    WHERE Id = @TicketId", new { TicketId = ticketId });

                if (ticket == null) return;

                var fromAddress = ConfigurationManager.AppSettings["EmailFromAddress"];
                var fromName = ConfigurationManager.AppSettings["EmailFromName"];
                var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                var smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
                var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
                var smtpEnableSSL = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSSL"]);

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                using (var smtpClient = new System.Net.Mail.SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = smtpEnableSSL;

                    var mail = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress(fromAddress, fromName),
                        Subject = $"Re: {ticket.Title}",
                        Body = $"<html><body style='font-family:Arial,sans-serif;'><div style='max-width:600px;margin:0 auto;padding:20px;background:#f8f9fa;'><div style='background:linear-gradient(135deg,#cb0c9f 0%,#7928ca 100%);padding:30px;border-radius:10px 10px 0 0;'><h1 style='color:white;margin:0;'>Support Response</h1></div><div style='background:white;padding:30px;border-radius:0 0 10px 10px;'><p>Hi,</p><p>We've reviewed your support ticket and here's our response:</p><div style='background:#f8f9fa;padding:20px;border-radius:8px;margin:20px 0;'><p style='margin:0 0 10px 0;'><strong>Your Question:</strong></p><p style='white-space:pre-wrap;'>{ticket.Message}</p></div><div style='background:#e8f5e9;padding:20px;border-radius:8px;border-left:4px solid #4caf50;margin:20px 0;'><p style='margin:0 0 10px 0;'><strong>Our Response:</strong></p><p style='white-space:pre-wrap;'>{responseMessage}</p></div><p>If you have any further questions, please don't hesitate to contact us again.</p><p style='margin-top:30px;'>Best regards,<br><strong>listNsell Support Team</strong></p></div></div></body></html>",
                        IsBodyHtml = true
                    };
                    mail.To.Add(ticket.UserEmail);
                    smtpClient.Send(mail);
                }
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
        ////[Authorize]
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
        ////[Authorize]
        public JsonResult UploadImages()
        {
            try
            {
                var uploadedFiles = new List<string>();

                if (Request.Files.Count > 0)
                {
                    var uploadPath = Server.MapPath("~/ListingImages/");

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
                            uploadedFiles.Add($"/ListingImages/{fileName}");
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
        ////[Authorize]
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
        //[Authorize]
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
        ////[Authorize]
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
        //[Authorize]
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

        #region Admin Function

        // Admin Statistics
        [HttpGet]
        public JsonResult GetAdminStats()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            var result = commonCore.GetAdminStats();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Get Admin Listings
        [HttpGet]
        public JsonResult GetAdminListings(string status = "")
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            var result = commonCore.GetAdminListings(status);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Approve Listing
        [HttpPost]
        public JsonResult ApproveListing(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" });
            }

            var result = commonCore.ApproveListing(id);
            return Json(result);
        }

        // Reject Listing
        [HttpPost]
        public JsonResult RejectListing(int id, string reason)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" });
            }

            var result = commonCore.RejectListing(id, reason);
            return Json(result);
        }

        // Admin Delete Listing
        [HttpPost]
        public JsonResult AdminDeleteListing(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" });
            }

            var result = commonCore.AdminDeleteListing(id);
            return Json(result);
        }

        // Get Reported Listings
        [HttpGet]
        public JsonResult GetReportedListings()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            var result = commonCore.GetReportedListings();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Dismiss Report
        [HttpPost]
        public JsonResult DismissReport(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" });
            }

            var result = commonCore.DismissReport(id);
            return Json(result);
        }

        // Get All Users
        [HttpGet]
        public JsonResult GetAllUsers()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            var result = commonCore.GetAllUsersForAdmin();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Toggle User Status
        [HttpPost]
        public JsonResult ToggleUserStatus(int userId, bool isActive)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null)
            {
                return Json(new { code = "0", message = "Unauthorized" });
            }

            var result = commonCore.ToggleUserStatus(userId, isActive);
            return Json(result);
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
        //[Authorize]
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
        //[Authorize]
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
        //[Authorize]
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


        [HttpPost]
        public JsonResult PostHelpData(AjaxResults model)
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            AjaxResults res = new AjaxResults();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(model.message, connection);
                connection.Open();
                string table = "";
                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        table = "<table class='table'>";
                        while (reader.Read())
                        {
                            table += "<tr>";
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                table += "<td>" + reader.GetValue(i) + "<td/>";
                            }
                            table += "<tr/>";
                        }
                        table += "<table/>";
                        reader.Close();
                    }
                    res.message = table;
                }
                catch (Exception er)
                {
                    res.message = er.Message;
                }
            }
            string results = new JavaScriptSerializer().Serialize(res);
            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}