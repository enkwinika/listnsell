using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NLog;
using rexell.Models;

namespace rexell.Core
{
    /// <summary>
    /// Async implementation of ICommonCore that wraps the existing static commonCore methods
    /// </summary>
    public class CommonCoreService : ICommonCore
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public CommonCoreService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        // Listings
        public async Task<AjaxResults> CreateListingAsync(ListingRequest request, int userId)
        {
            Logger.Info($"CreateListing called for userId: {userId}");
            return await Task.Run(() => commonCore.CreateListing(request, userId));
        }

        public async Task<AjaxResults> GetListingsAsync(ListingFilterRequest filter)
        {
            Logger.Debug("GetListings called");
            return await Task.Run(() => commonCore.GetListings(filter));
        }

        public async Task<AjaxResults> GetListingDetailsAsync(int listingId)
        {
            Logger.Debug($"GetListingDetails called for listingId: {listingId}");
            return await Task.Run(() => commonCore.GetListingDetails(listingId));
        }

        public async Task<AjaxResults> GetUserListingsAsync(int userId)
        {
            Logger.Debug($"GetUserListings called for userId: {userId}");
            return await Task.Run(() => commonCore.GetUserListings(userId));
        }

        public async Task<AjaxResults> UpdateListingAsync(int listingId, int userId, ListingRequest request)
        {
            Logger.Info($"UpdateListing called for listingId: {listingId}, userId: {userId}");
            return await Task.Run(() => commonCore.UpdateListing(listingId, userId, request));
        }

        public async Task<AjaxResults> DeleteListingAsync(int listingId, int userId)
        {
            Logger.Info($"DeleteListing called for listingId: {listingId}, userId: {userId}");
            return await Task.Run(() => commonCore.DeleteListing(listingId, userId));
        }

        public async Task<AjaxResults> MarkAsSoldAsync(int listingId, int userId)
        {
            Logger.Info($"MarkAsSold called for listingId: {listingId}, userId: {userId}");
            return await Task.Run(() => commonCore.MarkAsSold(listingId, userId));
        }

        // Admin
        public async Task<AjaxResults> GetAdminStatsAsync()
        {
            Logger.Debug("GetAdminStats called");
            return await Task.Run(() => commonCore.GetAdminStats());
        }

        public async Task<AjaxResults> GetAdminListingsAsync(string status = "")
        {
            Logger.Debug($"GetAdminListings called with status: {status}");
            return await Task.Run(() => commonCore.GetAdminListings(status));
        }

        public async Task<AjaxResults> ApproveListingAsync(int listingId)
        {
            Logger.Info($"ApproveListing called for listingId: {listingId}");
            return await Task.Run(() => commonCore.ApproveListing(listingId));
        }

        public async Task<AjaxResults> RejectListingAsync(int listingId, string reason)
        {
            Logger.Info($"RejectListing called for listingId: {listingId}");
            return await Task.Run(() => commonCore.RejectListing(listingId, reason));
        }

        public async Task<AjaxResults> AdminDeleteListingAsync(int listingId)
        {
            Logger.Info($"AdminDeleteListing called for listingId: {listingId}");
            return await Task.Run(() => commonCore.AdminDeleteListing(listingId));
        }

        public async Task<AjaxResults> GetReportedListingsAsync()
        {
            Logger.Debug("GetReportedListings called");
            return await Task.Run(() => commonCore.GetReportedListings());
        }

        public async Task<AjaxResults> DismissReportAsync(int reportId)
        {
            Logger.Info($"DismissReport called for reportId: {reportId}");
            return await Task.Run(() => commonCore.DismissReport(reportId));
        }

        public async Task<AjaxResults> GetAllUsersForAdminAsync()
        {
            Logger.Debug("GetAllUsersForAdmin called");
            return await Task.Run(() => commonCore.GetAllUsersForAdmin());
        }

        public async Task<AjaxResults> ToggleUserStatusAsync(int userId, bool isActive)
        {
            Logger.Info($"ToggleUserStatus called for userId: {userId}, isActive: {isActive}");
            return await Task.Run(() => commonCore.ToggleUserStatus(userId, isActive));
        }

        // Categories
        public async Task<AjaxResults> GetCategoriesAsync()
        {
            Logger.Debug("GetCategories called");
            return await Task.Run(() => commonCore.GetCategories());
        }

        // Messages
        public async Task<AjaxResults> SendMessageAsync(MessageRequest request, int senderId)
        {
            Logger.Info($"SendMessage called from senderId: {senderId}");
            return await Task.Run(() => commonCore.SendMessage(request, senderId));
        }

        public async Task<AjaxResults> GetUserMessagesAsync(int userId)
        {
            Logger.Debug($"GetUserMessages called for userId: {userId}");
            return await Task.Run(() => commonCore.GetUserMessages(userId));
        }

        public async Task<AjaxResults> MarkMessageAsReadAsync(int messageId, int userId)
        {
            Logger.Info($"MarkMessageAsRead called for messageId: {messageId}, userId: {userId}");
            return await Task.Run(() => commonCore.MarkMessageAsRead(messageId, userId));
        }

        // Reports
        public async Task<AjaxResults> ReportListingAsync(ReportRequest request, int userId)
        {
            Logger.Info($"ReportListing called from userId: {userId}");
            return await Task.Run(() => commonCore.ReportListing(request, userId));
        }

        // Authentication
        public async Task<AjaxResults> UserRegisterAsync(RegisterRequest model)
        {
            Logger.Info($"UserRegister called for email: {model?.email}");
            return await Task.Run(() => commonCore.UserRegister(model));
        }

        public async Task<AjaxResults> GetUserByUsernameAsync(string email)
        {
            Logger.Debug($"GetUserByUsername called for email: {email}");
            return await Task.Run(() => commonCore.GetUserByUsername(email));
        }

        public async Task<AjaxResults> UserLoginAsync(LoginRequest model)
        {
            Logger.Info($"UserLogin called for username: {model?.username}");
            return await Task.Run(() => commonCore.UserLogin(model));
        }

        public async Task<AjaxResults> VerifyEmailAsync(string email, string code)
        {
            Logger.Info($"VerifyEmail called for email: {email}");
            return await Task.Run(() => commonCore.VerifyEmail(email, code));
        }

        public async Task<AjaxResults> ResendVerificationCodeAsync(string email)
        {
            Logger.Info($"ResendVerificationCode called for email: {email}");
            return await Task.Run(() => commonCore.ResendVerificationCode(email));
        }

        public async Task<AjaxResults> RequestPasswordResetAsync(string email)
        {
            Logger.Info($"RequestPasswordReset called for email: {email}");
            return await Task.Run(() => commonCore.RequestPasswordReset(email));
        }

        public async Task<AjaxResults> ResetPasswordAsync(string email, string code, string newPassword)
        {
            Logger.Info($"ResetPassword called for email: {email}");
            return await Task.Run(() => commonCore.ResetPassword(email, code, newPassword));
        }

        // User Management
        public async Task<List<dynamic>> GetAllUsersAsync()
        {
            Logger.Debug("GetAllUsers called");
            return await Task.Run(() => commonCore.GetAllUsers());
        }

        public async Task<dynamic> GetUserByIdAsync(int userId)
        {
            Logger.Debug($"GetUserById called for userId: {userId}");
            return await Task.Run(() => commonCore.GetUserById(userId));
        }
    }
}
