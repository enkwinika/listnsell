using System.Collections.Generic;
using System.Threading.Tasks;
using rexell.Models;

namespace rexell.Core
{
    public interface ICommonCore
    {
        // Listings
        Task<AjaxResults> CreateListingAsync(ListingRequest request, int userId);
        Task<AjaxResults> GetListingsAsync(ListingFilterRequest filter);
        Task<AjaxResults> GetListingDetailsAsync(int listingId);
        Task<AjaxResults> GetUserListingsAsync(int userId);
        Task<AjaxResults> UpdateListingAsync(int listingId, int userId, ListingRequest request);
        Task<AjaxResults> DeleteListingAsync(int listingId, int userId);
        Task<AjaxResults> MarkAsSoldAsync(int listingId, int userId);

        // Admin
        Task<AjaxResults> GetAdminStatsAsync();
        Task<AjaxResults> GetAdminListingsAsync(string status = "");
        Task<AjaxResults> ApproveListingAsync(int listingId);
        Task<AjaxResults> RejectListingAsync(int listingId, string reason);
        Task<AjaxResults> AdminDeleteListingAsync(int listingId);
        Task<AjaxResults> GetReportedListingsAsync();
        Task<AjaxResults> DismissReportAsync(int reportId);
        Task<AjaxResults> GetAllUsersForAdminAsync();
        Task<AjaxResults> ToggleUserStatusAsync(int userId, bool isActive);

        // Categories
        Task<AjaxResults> GetCategoriesAsync();

        // Messages
        Task<AjaxResults> SendMessageAsync(MessageRequest request, int senderId);
        Task<AjaxResults> GetUserMessagesAsync(int userId);
        Task<AjaxResults> MarkMessageAsReadAsync(int messageId, int userId);

        // Reports
        Task<AjaxResults> ReportListingAsync(ReportRequest request, int userId);

        // Authentication
        Task<AjaxResults> UserRegisterAsync(RegisterRequest model);
        Task<AjaxResults> GetUserByUsernameAsync(string email);
        Task<AjaxResults> UserLoginAsync(LoginRequest model);
        Task<AjaxResults> VerifyEmailAsync(string email, string code);
        Task<AjaxResults> ResendVerificationCodeAsync(string email);
        Task<AjaxResults> RequestPasswordResetAsync(string email);
        Task<AjaxResults> ResetPasswordAsync(string email, string code, string newPassword);
        
        // User Management
        Task<List<dynamic>> GetAllUsersAsync();
        Task<dynamic> GetUserByIdAsync(int userId);
    }
}
