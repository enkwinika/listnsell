using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using rexell.Models;


using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Dapper;

namespace rexell.Core
{
    public class commonCore
    {
        //private static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        private static SqlConnection GetOpenConnection(bool mars = false)
        {
            var cs = ConnectionString;
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }

            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }
        #region Listings

        /// <summary>
        /// Create a new listing
        /// </summary>
        public static AjaxResults CreateListing(ListingRequest request, int userId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    // Insert listing
                    var listingId = cn.QuerySingle<int>(@"
                        INSERT INTO Listings (UserId, Title, Description, CategoryId, Price, Condition, Location, Status, CreatedDate)
                        OUTPUT INSERTED.ListingId
                        VALUES (@UserId, @Title, @Description, @CategoryId, @Price, @Condition, @Location, 'Pending', getdate())",
                        new
                        {
                            UserId = userId,
                            request.Title,
                            request.Description,
                            request.CategoryId,
                            request.Price,
                            request.Condition,
                            request.Location
                        });

                    // Insert images
                    if (request.ImagePaths != null && request.ImagePaths.Any())
                    {
                        for (int i = 0; i < request.ImagePaths.Count; i++)
                        {
                            cn.Execute(@"
                                INSERT INTO ListingImages (ListingId, ImagePath, ImageOrder)
                                VALUES (@ListingId, @ImagePath, @ImageOrder)",
                                new { ListingId = listingId, ImagePath = request.ImagePaths[i], ImageOrder = i });
                        }
                    }

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing created successfully and is pending approval",
                        data = new { listingId }
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to create listing: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Get filtered listings with pagination
        /// </summary>
        public static AjaxResults GetListings(ListingFilterRequest filter)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var sql_delete = @"
                        WITH FilteredListings AS (
                            SELECT 
                                l.ListingId,
                                l.UserId,
                                l.Title,
                                l.Description,
                                c.CategoryName as Category,
                                l.CategoryId,
                                l.Price,
                                l.Condition,
                                l.Location,
                                l.Status,
                                l.ViewCount,
                                l.CreatedDate,
                                u.FirstName + ' ' + u.LastName as SellerName,
                                u.IsEmailVerified as IsVerified,
                                u.CreatedDate as SellerJoinDate,
                                u.ProfileImage,
                                (SELECT TOP 1 ImagePath FROM ListingImages WHERE ListingId = l.ListingId AND IsActive = 1 ORDER BY ImageOrder) as MainImage
                            FROM Listings l
                            INNER JOIN Users u ON l.UserId = u.UserId
                            INNER JOIN Categories c ON l.CategoryId = c.CategoryId
                            WHERE l.IsActive = 1 
                            AND l.Status = 'Approved'
                            AND (@SearchTerm IS NULL OR l.Title LIKE '%' + @SearchTerm + '%' OR l.Description LIKE '%' + @SearchTerm + '%')
                            AND (@MinPrice IS NULL OR l.Price >= @MinPrice)
                            AND (@MaxPrice IS NULL OR l.Price <= @MaxPrice)
                            AND (@Location IS NULL OR l.Location LIKE '%' + @Location + '%')
                        )
                        SELECT * FROM FilteredListings
                        ORDER BY 
                            CASE WHEN @SortBy = 'newest' THEN CreatedDate END DESC,
                            CASE WHEN @SortBy = 'price-low' THEN Price END ASC,
                            CASE WHEN @SortBy = 'price-high' THEN Price END DESC,
                            CASE WHEN @SortBy = 'popular' THEN ViewCount END DESC,
                            CreatedDate DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                        SELECT COUNT(*) FROM FilteredListings;";

                    var sql = @"
                        SELECT * FROM (
                            SELECT 
                                l.ListingId,
                                l.UserId,
                                l.Title,
                                l.Description,
                                c.CategoryName as Category,
                                l.CategoryId,
                                l.Price,
                                l.Condition,
                                l.Location,
                                l.Status,
                                l.ViewCount,
                                l.CreatedDate,
                                u.FirstName + ' ' + u.LastName as SellerName,
                                u.IsEmailVerified as IsVerified,
                                u.CreatedDate as SellerJoinDate,
                                u.ProfileImage,
                                (SELECT TOP 1 ImagePath FROM ListingImages WHERE ListingId = l.ListingId AND IsActive = 1 ORDER BY ImageOrder) as MainImage
                            FROM Listings l
                            INNER JOIN Users u ON l.UserId = u.UserId
                            INNER JOIN Categories c ON l.CategoryId = c.CategoryId
                            WHERE l.IsActive = 1 
                            AND l.Status = 'Approved'
                            AND (@SearchTerm IS NULL OR l.Title LIKE '%' + @SearchTerm + '%' OR l.Description LIKE '%' + @SearchTerm + '%')
                            AND (@MinPrice IS NULL OR l.Price >= @MinPrice)
                            AND (@MaxPrice IS NULL OR l.Price <= @MaxPrice)
                            AND (@Location IS NULL OR l.Location LIKE '%' + @Location + '%')
                        ) AS FilteredListings
                        ORDER BY 
                            CASE WHEN @SortBy = 'newest' THEN CreatedDate END DESC,
                            CASE WHEN @SortBy = 'price-low' THEN Price END ASC,
                            CASE WHEN @SortBy = 'price-high' THEN Price END DESC,
                            CASE WHEN @SortBy = 'popular' THEN ViewCount END DESC,
                            CreatedDate DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                        SELECT COUNT(*) FROM (
                            SELECT 
                                l.ListingId
                            FROM Listings l
                            INNER JOIN Users u ON l.UserId = u.UserId
                            INNER JOIN Categories c ON l.CategoryId = c.CategoryId
                            WHERE l.IsActive = 1 
                            AND l.Status = 'Approved'
                            AND (@SearchTerm IS NULL OR l.Title LIKE '%' + @SearchTerm + '%' OR l.Description LIKE '%' + @SearchTerm + '%')
                            AND (@MinPrice IS NULL OR l.Price >= @MinPrice)
                            AND (@MaxPrice IS NULL OR l.Price <= @MaxPrice)
                            AND (@Location IS NULL OR l.Location LIKE '%' + @Location + '%')
                        ) AS FilteredCount;";

                    using (var multi = cn.QueryMultiple(sql, new
                    {
                        filter.SearchTerm,
                        filter.MinPrice,
                        filter.MaxPrice,
                        filter.Location,
                        SortBy = filter.SortBy ?? "newest",
                        Offset = (filter.PageNumber - 1) * filter.PageSize,
                        filter.PageSize
                    }))
                    {
                        var listings = multi.Read<dynamic>().ToList();
                        var totalCount = multi.ReadSingle<int>();

                        // Apply category and condition filters in memory (or use temp tables for better performance) FilteredListings
                        if (filter.CategoryIds != null && filter.CategoryIds.Any())
                        {
                            listings = listings.Where(l => filter.CategoryIds.Contains((int)l.CategoryId)).ToList();
                            totalCount = listings.Count;
                        }

                        if (filter.Conditions != null && filter.Conditions.Any())
                        {
                            listings = listings.Where(l => filter.Conditions.Contains((string)l.Condition)).ToList();
                            totalCount = listings.Count;
                        }

                        var result = listings.Select(l => new
                        {
                            id = l.ListingId,
                            title = l.Title,
                            description = l.Description,
                            category = l.Category,
                            categoryId = l.CategoryId,
                            price = l.Price,
                            condition = l.Condition,
                            location = l.Location,
                            date = ((DateTime)l.CreatedDate).ToString("yyyy-MM-dd"),
                            viewCount = l.ViewCount,
                            image = l.MainImage ?? "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500",
                            seller = new
                            {
                                name = l.SellerName,
                                verified = l.IsVerified,
                                joinDate = ((DateTime)l.SellerJoinDate).ToString("MMM yyyy"),
                                profileImage = l.ProfileImage
                            }
                        }).ToList();

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "Listings retrieved successfully",
                            data = new
                            {
                                items = result,
                                totalCount,
                                pageNumber = filter.PageNumber,
                                pageSize = filter.PageSize,
                                totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve listings: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Get listing details by ID
        /// </summary>
        public static AjaxResults GetListingDetails(int listingId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    // Get listing details
                    var listing = cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT 
                            l.ListingId,
                            l.UserId,
                            l.Title,
                            l.Description,
                            c.CategoryName as Category,
                            l.CategoryId,
                            l.Price,
                            l.Condition,
                            l.Location,
                            l.Status,
                            l.ViewCount,
                            l.CreatedDate,
                            u.FirstName + ' ' + u.LastName as SellerName,
                            u.IsEmailVerified as IsVerified,
                            u.CreatedDate as SellerJoinDate,
                            u.ProfileImage
                        FROM Listings l
                        INNER JOIN Users u ON l.UserId = u.UserId
                        INNER JOIN Categories c ON l.CategoryId = c.CategoryId
                        WHERE l.ListingId = @ListingId AND l.IsActive = 1",
                        new { ListingId = listingId });

                    if (listing == null)
                    {
                        return new AjaxResults
                        {
                            code = "0",
                            title = "Not Found",
                            message = "Listing not found"
                        };
                    }

                    // Get images
                    var images = cn.Query<string>(@"
                        SELECT ImagePath 
                        FROM ListingImages 
                        WHERE ListingId = @ListingId AND IsActive = 1 
                        ORDER BY ImageOrder",
                        new { ListingId = listingId }).ToList();

                    if (!images.Any())
                    {
                        images.Add("https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800");
                    }

                    // Increment view count
                    cn.Execute("UPDATE Listings SET ViewCount = ViewCount + 1 WHERE ListingId = @ListingId",
                        new { ListingId = listingId });

                    // Get similar items
                    var similarItems = cn.Query<dynamic>(@"
                        SELECT TOP 4
                            l.ListingId,
                            l.Title,
                            l.Price,
                            l.Location,
                            l.CreatedDate,
                            (SELECT TOP 1 ImagePath FROM ListingImages WHERE ListingId = l.ListingId AND IsActive = 1 ORDER BY ImageOrder) as MainImage
                        FROM Listings l
                        WHERE l.CategoryId = @CategoryId 
                        AND l.ListingId != @ListingId 
                        AND l.Status = 'Approved'
                        AND l.IsActive = 1
                        ORDER BY NEWID()",
                        new { CategoryId = listing.CategoryId, ListingId = listingId }).ToList();

                    var result = new
                    {
                        id = listing.ListingId,
                        userId = listing.UserId,
                        title = listing.Title,
                        description = listing.Description,
                        category = listing.Category,
                        categoryId = listing.CategoryId,
                        price = listing.Price,
                        condition = listing.Condition,
                        location = listing.Location,
                        status = listing.Status,
                        viewCount = listing.ViewCount,
                        date = ((DateTime)listing.CreatedDate).ToString("yyyy-MM-dd"),
                        images = images,
                        seller = new
                        {
                            name = listing.SellerName,
                            verified = listing.IsVerified,
                            joinDate = ((DateTime)listing.SellerJoinDate).ToString("MMM yyyy"),
                            profileImage = listing.ProfileImage
                        },
                        similarItems = similarItems.Select(s => new
                        {
                            id = s.ListingId,
                            title = s.Title,
                            price = s.Price,
                            location = s.Location,
                            date = ((DateTime)s.CreatedDate).ToString("yyyy-MM-dd"),
                            image = s.MainImage ?? "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500"
                        })
                    };

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing details retrieved successfully",
                        data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve listing details: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Get user's own listings
        /// </summary>
        public static AjaxResults GetUserListings(int userId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var listings = cn.Query<dynamic>(@"
                        SELECT 
                            l.ListingId,
                            l.Title,
                            l.Price,
                            l.Status,
                            l.CreatedDate,
                            l.ViewCount,
                            (SELECT TOP 1 ImagePath FROM ListingImages WHERE ListingId = l.ListingId AND IsActive = 1 ORDER BY ImageOrder) as MainImage
                        FROM Listings l
                        WHERE l.UserId = @UserId AND l.IsActive = 1
                        ORDER BY l.CreatedDate DESC",
                        new { UserId = userId }).ToList();

                    var result = listings.Select(l => new
                    {
                        id = l.ListingId,
                        title = l.Title,
                        price = l.Price,
                        status = l.Status,
                        viewCount = l.ViewCount,
                        date = ((DateTime)l.CreatedDate).ToString("yyyy-MM-dd"),
                        image = l.MainImage ?? "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500"
                    });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "User listings retrieved successfully",
                        data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve user listings: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Update listing
        /// </summary>
        public static AjaxResults UpdateListing(int listingId, int userId, ListingRequest request)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    // Check ownership
                    var ownerId = cn.QuerySingleOrDefault<int?>(
                        "SELECT UserId FROM Listings WHERE ListingId = @ListingId",
                        new { ListingId = listingId });

                    if (ownerId == null || ownerId != userId)
                    {
                        return new AjaxResults
                        {
                            code = "0",
                            title = "Unauthorized",
                            message = "You don't have permission to edit this listing"
                        };
                    }

                    // Update listing
                    cn.Execute(@"
                        UPDATE Listings 
                        SET Title = @Title,
                            Description = @Description,
                            CategoryId = @CategoryId,
                            Price = @Price,
                            Condition = @Condition,
                            Location = @Location,
                            UpdatedDate = GETDATE()
                        WHERE ListingId = @ListingId",
                        new
                        {
                            ListingId = listingId,
                            request.Title,
                            request.Description,
                            request.CategoryId,
                            request.Price,
                            request.Condition,
                            request.Location
                        });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing updated successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to update listing: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Delete listing (soft delete)
        /// </summary>
        public static AjaxResults DeleteListing(int listingId, int userId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    // Check ownership
                    var ownerId = cn.QuerySingleOrDefault<int?>(
                        "SELECT UserId FROM Listings WHERE ListingId = @ListingId",
                        new { ListingId = listingId });

                    if (ownerId == null || ownerId != userId)
                    {
                        return new AjaxResults
                        {
                            code = "0",
                            title = "Unauthorized",
                            message = "You don't have permission to delete this listing"
                        };
                    }

                    // Soft delete
                    cn.Execute(@"
                        UPDATE Listings 
                        SET IsActive = 0, Status = 'Deleted', UpdatedDate = GETDATE()
                        WHERE ListingId = @ListingId",
                        new { ListingId = listingId });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing deleted successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to delete listing: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Mark listing as sold
        /// </summary>
        public static AjaxResults MarkAsSold(int listingId, int userId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    // Check ownership
                    var ownerId = cn.QuerySingleOrDefault<int?>(
                        "SELECT UserId FROM Listings WHERE ListingId = @ListingId",
                        new { ListingId = listingId });

                    if (ownerId == null || ownerId != userId)
                    {
                        return new AjaxResults
                        {
                            code = "0",
                            title = "Unauthorized",
                            message = "You don't have permission to modify this listing"
                        };
                    }

                    cn.Execute(@"
                        UPDATE Listings 
                        SET Status = 'Sold', SoldDate = GETDATE(), UpdatedDate = GETDATE()
                        WHERE ListingId = @ListingId",
                        new { ListingId = listingId });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing marked as sold"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to mark as sold: " + ex.Message
                };
            }
        }

        #endregion

        #region Admin Function

        #region Admin Management

        /// <summary>
        /// Get admin statistics
        /// </summary>
        public static AjaxResults GetAdminStats()
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var stats = cn.QueryFirstOrDefault<dynamic>(@"
                SELECT 
                    (SELECT COUNT(*) FROM Listings WHERE Status = 'Pending' AND IsActive = 1) as PendingCount,
                    (SELECT COUNT(*) FROM Listings WHERE IsActive = 1) as TotalListings,
                    (SELECT COUNT(DISTINCT ListingId) FROM Reports WHERE Status = 'Pending') as ReportsCount
            ");

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Admin stats retrieved successfully",
                        data = new
                        {
                            pendingCount = stats.PendingCount,
                            totalListings = stats.TotalListings,
                            reportsCount = stats.ReportsCount
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve admin stats: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Get admin listings with optional status filter
        /// </summary>
        public static AjaxResults GetAdminListings(string status = "")
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var sql = @"
                SELECT 
                    l.ListingId as Id,
                    l.Title,
                    l.Description,
                    l.Price,
                    l.Condition,
                    l.Location,
                    l.Status,
                    l.ViewCount,
                    l.CreatedDate as Date,
                    c.CategoryName as Category,
                    u.FirstName + ' ' + u.LastName as Seller,
                    u.UserId,
                    (SELECT TOP 1 ImagePath FROM ListingImages WHERE ListingId = l.ListingId AND IsActive = 1 ORDER BY ImageOrder) as Image
                FROM Listings l
                INNER JOIN Categories c ON l.CategoryId = c.CategoryId
                INNER JOIN Users u ON l.UserId = u.UserId
                WHERE l.IsActive = 1
                AND (@Status = '' OR l.Status = @Status)
                ORDER BY l.CreatedDate DESC";

                    var listings = cn.Query<dynamic>(sql, new { Status = status }).ToList();

                    var result = listings.Select(l => new
                    {
                        id = l.Id,
                        title = l.Title,
                        price = l.Price,
                        condition = l.Condition,
                        location = l.Location,
                        status = l.Status,
                        viewCount = l.ViewCount,
                        date = ((DateTime)l.Date).ToString("yyyy-MM-dd"),
                        category = l.Category,
                        seller = l.Seller,
                        userId = l.UserId,
                        image = l.Image ?? "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500"
                    });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Admin listings retrieved successfully",
                        data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve admin listings: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Approve listing (Admin only)
        /// </summary>
        public static AjaxResults ApproveListing(int listingId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Execute(@"
                UPDATE Listings 
                SET Status = 'Approved', 
                    UpdatedDate = GETDATE() 
                WHERE ListingId = @ListingId",
                        new { ListingId = listingId });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing approved successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to approve listing: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Reject listing with reason (Admin only)
        /// </summary>
        public static AjaxResults RejectListing(int listingId, string reason)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Execute(@"
                UPDATE Listings 
                SET Status = 'Rejected', 
                    RejectionReason = @Reason, 
                    UpdatedDate = GETDATE() 
                WHERE ListingId = @ListingId",
                        new { ListingId = listingId, Reason = reason });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing rejected"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to reject listing: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Delete listing permanently (Admin only)
        /// </summary>
        public static AjaxResults AdminDeleteListing(int listingId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    // Delete related records first
                    cn.Execute("DELETE FROM ListingImages WHERE ListingId = @ListingId", new { ListingId = listingId });
                    cn.Execute("DELETE FROM Reports WHERE ListingId = @ListingId", new { ListingId = listingId });
                    cn.Execute("DELETE FROM Messages WHERE ListingId = @ListingId", new { ListingId = listingId });

                    // Delete listing
                    cn.Execute("DELETE FROM Listings WHERE ListingId = @ListingId", new { ListingId = listingId });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing deleted permanently"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to delete listing: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Get reported listings
        /// </summary>
        public static AjaxResults GetReportedListings()
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var reports = cn.Query<dynamic>(@"
                SELECT 
                    r.ReportId,
                    r.ListingId,
                    l.Title as ListingTitle,
                    u.FirstName + ' ' + u.LastName as SellerName,
                    reporter.FirstName + ' ' + reporter.LastName as ReporterName,
                    r.ReasonText as LatestReason,
                    r.ReportedDate as ReportDate,
                    (SELECT TOP 1 ImagePath FROM ListingImages WHERE ListingId = l.ListingId AND IsActive = 1 ORDER BY ImageOrder) as ListingImage,
                    (SELECT COUNT(*) FROM Reports WHERE ListingId = r.ListingId AND Status = 'Pending') as ReportCount
                FROM Reports r
                INNER JOIN Listings l ON r.ListingId = l.ListingId
                INNER JOIN Users u ON l.UserId = u.UserId
                INNER JOIN Users reporter ON r.ReportedBy = reporter.UserId
                WHERE r.Status = 'Pending'
                ORDER BY r.ReportedDate DESC").ToList();

                    var result = reports.Select(r => new
                    {
                        reportId = r.ReportId,
                        listingId = r.ListingId,
                        listingTitle = r.ListingTitle,
                        sellerName = r.SellerName,
                        reporterName = r.ReporterName,
                        latestReason = r.LatestReason,
                        reportDate = ((DateTime)r.ReportDate).ToString("yyyy-MM-dd HH:mm"),
                        listingImage = r.ListingImage ?? "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500",
                        reportCount = r.ReportCount
                    });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Reported listings retrieved successfully",
                        data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve reported listings: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Dismiss report
        /// </summary>
        public static AjaxResults DismissReport(int reportId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Execute(@"
                UPDATE Reports 
                SET Status = 'Dismissed', 
                    ReviewedDate = GETDATE() 
                WHERE ReportId = @ReportId",
                        new { ReportId = reportId });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Report dismissed successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to dismiss report: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Get all users for admin management
        /// </summary>
        public static AjaxResults GetAllUsersForAdmin()
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var users = cn.Query<dynamic>(@"
                SELECT 
                    u.UserId as Id,
                    u.FirstName + ' ' + u.LastName as Name,
                    u.Email,
                    u.IsActive,
                    u.IsAdmin,
                    u.CreatedDate as JoinDate,
                    (SELECT COUNT(*) FROM Listings WHERE UserId = u.UserId AND IsActive = 1) as ListingsCount
                FROM Users u
                ORDER BY u.CreatedDate DESC").ToList();

                    var result = users.Select(u => new
                    {
                        id = u.Id,
                        name = u.Name,
                        email = u.Email,
                        isActive = u.IsActive,
                        isAdmin = u.IsAdmin ?? false,
                        joinDate = ((DateTime)u.JoinDate).ToString("MMM yyyy"),
                        listingsCount = u.ListingsCount
                    });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Users retrieved successfully",
                        data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve users: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Toggle user active status
        /// </summary>
        public static AjaxResults ToggleUserStatus(int userId, bool isActive)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Execute(@"
                UPDATE Users 
                SET IsActive = @IsActive 
                WHERE UserId = @UserId",
                        new { UserId = userId, IsActive = isActive });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = isActive ? "User activated successfully" : "User suspended successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to update user status: " + ex.Message
                };
            }
        }

        #endregion

        #endregion

        #region Categories

        /// <summary>
        /// Get all categories
        /// </summary>
        public static AjaxResults GetCategories()
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var categories = cn.Query<CategoryResponse>(@"
                        SELECT CategoryId, CategoryName 
                        FROM Categories 
                        WHERE IsActive = 1 
                        ORDER BY CategoryName").ToList();

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Categories retrieved successfully",
                        data = categories
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve categories: " + ex.Message
                };
            }
        }

        #endregion

        #region Messages

        /// <summary>
        /// Send message
        /// </summary>
        public static AjaxResults SendMessage(MessageRequest request, int senderId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Execute(@"
                        INSERT INTO Messages (ListingId, SenderId, ReceiverId, MessageText)
                        VALUES (@ListingId, @SenderId, @ReceiverId, @MessageText)",
                        new
                        {
                            request.ListingId,
                            SenderId = senderId,
                            request.ReceiverId,
                            request.MessageText
                        });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Message sent successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to send message: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Get user messages
        /// </summary>
        public static AjaxResults GetUserMessages(int userId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var messages = cn.Query<dynamic>(@"
                        SELECT 
                            m.MessageId,
                            m.ListingId,
                            l.Title as ListingTitle,
                            u.FirstName + ' ' + u.LastName as SenderName,
                            m.MessageText,
                            m.IsRead,
                            m.SentDate
                        FROM Messages m
                        INNER JOIN Listings l ON m.ListingId = l.ListingId
                        INNER JOIN Users u ON m.SenderId = u.UserId
                        WHERE m.ReceiverId = @UserId
                        ORDER BY m.SentDate DESC",
                        new { UserId = userId }).ToList();

                    var result = messages.Select(m => new
                    {
                        id = m.MessageId,
                        listingId = m.ListingId,
                        item = m.ListingTitle,
                        buyer = m.SenderName,
                        lastMessage = m.MessageText,
                        time = GetTimeAgo((DateTime)m.SentDate),
                        unread = !(bool)m.IsRead
                    });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Messages retrieved successfully",
                        data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to retrieve messages: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Mark message as read
        /// </summary>
        public static AjaxResults MarkMessageAsRead(int messageId, int userId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Execute(@"
                        UPDATE Messages 
                        SET IsRead = 1 
                        WHERE MessageId = @MessageId AND ReceiverId = @UserId",
                        new { MessageId = messageId, UserId = userId });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Message marked as read"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to mark message as read: " + ex.Message
                };
            }
        }

        #endregion

        #region Reports

        /// <summary>
        /// Report listing
        /// </summary>
        public static AjaxResults ReportListing(ReportRequest request, int userId)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    cn.Execute(@"
                        INSERT INTO Reports (ListingId, ReportedBy, ReasonText)
                        VALUES (@ListingId, @ReportedBy, @ReasonText)",
                        new
                        {
                            request.ListingId,
                            ReportedBy = userId,
                            request.ReasonText
                        });

                    return new AjaxResults
                    {
                        code = "1",
                        title = "Success",
                        message = "Listing reported successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to report listing: " + ex.Message
                };
            }
        }

        #endregion

        #region Helper Methods

        private static string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.Now - date;

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

            return $"{(int)(timeSpan.TotalDays / 30)} months ago";
        }

//        #endregion
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Net;
//using System.Net.Mail;
//using System.Security.Cryptography;
//using System.Text;
//using System.Web.Security;
//using Dapper;

//namespace Survey.Core
//    {
//        public class UserDataLayer
//        {
//            private static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

//            #region User Registration

            /// <summary>
            /// Register a new user
            /// </summary>
            public static AjaxResults UserRegister(RegisterRequest model)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        // Check if email already exists
                        var existingUser = cn.QuerySingleOrDefault<int?>(
                            "SELECT UserId FROM Users WHERE Email = @Email",
                            new { Email = model.email });

                        if (existingUser.HasValue)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Email address is already registered"
                            };
                        }

                        // Validate input
                        if (string.IsNullOrWhiteSpace(model.firstname) || model.firstname.Length < 2)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "First name must be at least 2 characters"
                            };
                        }

                        if (string.IsNullOrWhiteSpace(model.lastname) || model.lastname.Length < 2)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Last name must be at least 2 characters"
                            };
                        }

                        if (string.IsNullOrWhiteSpace(model.password) || model.password.Length < 6)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Password must be at least 6 characters"
                            };
                        }

                        // Hash password
                        string passwordHash = HashPassword(model.password);

                        // Generate verification code
                        string verificationCode = GenerateVerificationCode();
                        DateTime codeExpiry = DateTime.Now.AddHours(24);

                        // Insert user
                        var userId = cn.QuerySingle<int>(@"
                        INSERT INTO Users (FirstName, LastName, Email, Phone, PasswordHash, 
                                         IsEmailVerified, VerificationCode, VerificationCodeExpiry, IsActive, CreatedDate)
                        OUTPUT INSERTED.UserId
                        VALUES (@FirstName, @LastName, @Email, @Phone, @PasswordHash, 
                                0, @VerificationCode, @VerificationCodeExpiry, 1, GETDATE())",
                            new
                            {
                                FirstName = model.firstname,
                                LastName = model.lastname,
                                Email = model.email,
                                Phone = model.phone ?? "",
                                PasswordHash = passwordHash,
                                VerificationCode = verificationCode,
                                VerificationCodeExpiry = codeExpiry
                            });

                        // Send verification email
                        bool emailSent = SendVerificationEmail(model.email, verificationCode, model.firstname);

                        if (!emailSent)
                        {
                            // Log warning but don't fail registration
                            System.Diagnostics.Debug.WriteLine($"Failed to send verification email to {model.email}");
                        }

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "Registration successful! Please check your email for verification code.",
                            data = new { userId, email = model.email }
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Registration failed. Please try again."
                    };
                }
            }

        #endregion

        #region User Login


        /// <summary>
        /// Request password reset
        /// </summary>
        public static AjaxResults GetUserByUsername(string email)
        {
            try
            {
                using (var cn = GetOpenConnection())
                {
                    var user = cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT UserId, FirstName, IsActive, isAdmin
                        FROM Users 
                        WHERE Email = @Email",
                        new { Email = email });

                    return new AjaxResults
                    {
                        isAdmin = user.isAdmin,
                        code = "1",
                        title = "Success",
                        message = "If the email exists, a reset code has been sent."
                    };
                }
            }
            catch (Exception ex)
            {
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Login failed. Please try again."
                };
            }
        }


        /// <summary>
        /// User login
        /// </summary>
        public static AjaxResults UserLogin(LoginRequest model)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        // Get user by email/username
                        var user = cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT UserId, FirstName, LastName, Email, PasswordHash, IsEmailVerified, IsActive, isAdmin  
                        FROM Users 
                        WHERE Email = @Username OR Email = @Username",
                            new { Username = model.username });

                        if (user == null)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Invalid email or password"
                            };
                        }

                        // Check if account is active
                        if (!user.IsActive)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Account is disabled. Please contact support."
                            };
                        }

                        // Verify password
                        if (!VerifyPassword(model.password, user.PasswordHash))
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Invalid email or password"
                            };
                        }

                        // Check email verification
                        if (!user.IsEmailVerified)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Please verify your email before logging in. Check your inbox for verification code."
                            };
                        }

                        // Update last login date
                        cn.Execute("UPDATE Users SET LastLoginDate = GETDATE() WHERE UserId = @UserId",
                            new { UserId = user.UserId });

                        // Create authentication ticket
                        //FormsAuthentication.SetAuthCookie(user.Email, false);

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "Login successful!",
                            email = user.Email,
                            isAdmin = user.isAdmin,
                            data = new
                            {
                                isAdmin = user.isAdmin,
                                userId = user.UserId,
                                name = $"{user.FirstName} {user.LastName}",
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = ex.Message
                    };
                }
            }

            #endregion

            #region Email Verification

            /// <summary>
            /// Verify email with code
            /// </summary>
            public static AjaxResults VerifyEmail(string email, string code)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        var user = cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT UserId, VerificationCode, VerificationCodeExpiry, IsEmailVerified 
                        FROM Users 
                        WHERE Email = @Email",
                            new { Email = email });

                        if (user == null)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "User not found"
                            };
                        }

                        if (user.IsEmailVerified)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Email is already verified"
                            };
                        }

                        if (string.IsNullOrEmpty(user.VerificationCode))
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "No verification code found. Please request a new one."
                            };
                        }

                        if (DateTime.Now > user.VerificationCodeExpiry)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Verification code has expired. Please request a new one."
                            };
                        }

                        if (user.VerificationCode.ToString() != code.Trim())
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Invalid verification code"
                            };
                        }

                        // Mark email as verified
                        cn.Execute(@"
                        UPDATE Users 
                        SET IsEmailVerified = 1, 
                            VerificationCode = NULL, 
                            VerificationCodeExpiry = NULL 
                        WHERE UserId = @UserId",
                            new { UserId = user.UserId });

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "Email verified successfully! You can now log in."
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Email verification error: {ex.Message}");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Verification failed. Please try again."
                    };
                }
            }

            /// <summary>
            /// Resend verification code
            /// </summary>
            public static AjaxResults ResendVerificationCode(string email)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        var user = cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT UserId, FirstName, IsEmailVerified 
                        FROM Users 
                        WHERE Email = @Email",
                            new { Email = email });

                        if (user == null)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "User not found"
                            };
                        }

                        if (user.IsEmailVerified)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Email is already verified"
                            };
                        }

                        // Generate new verification code
                        string verificationCode = GenerateVerificationCode();
                        DateTime codeExpiry = DateTime.Now.AddHours(24);

                        // Update verification code
                        cn.Execute(@"
                        UPDATE Users 
                        SET VerificationCode = @VerificationCode, 
                            VerificationCodeExpiry = @VerificationCodeExpiry 
                        WHERE UserId = @UserId",
                            new
                            {
                                UserId = user.UserId,
                                VerificationCode = verificationCode,
                                VerificationCodeExpiry = codeExpiry
                            });

                        // Send verification email
                        bool emailSent = SendVerificationEmail(email, verificationCode, user.FirstName);

                        if (!emailSent)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Failed to send verification email. Please try again."
                            };
                        }

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "Verification code sent! Please check your email."
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Resend verification error: {ex.Message}");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Failed to resend verification code. Please try again."
                    };
                }
            }

            #endregion

            #region Password Reset

            /// <summary>
            /// Request password reset
            /// </summary>
            public static AjaxResults RequestPasswordReset(string email)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        var user = cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT UserId, FirstName, IsActive 
                        FROM Users 
                        WHERE Email = @Email",
                            new { Email = email });

                        if (user == null)
                        {
                            // Don't reveal if email exists for security
                            return new AjaxResults
                            {
                                code = "1",
                                title = "Success",
                                message = "If the email exists, a reset code has been sent."
                            };
                        }

                        if (!user.IsActive)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Account is disabled. Please contact support."
                            };
                        }

                        // Generate reset code
                        string resetCode = GenerateVerificationCode();
                        DateTime resetExpiry = DateTime.Now.AddHours(1); // Reset codes expire in 1 hour

                        // Update reset code
                        cn.Execute(@"
                        UPDATE Users 
                        SET ResetPasswordCode = @ResetCode, 
                            ResetPasswordExpiry = @ResetExpiry 
                        WHERE UserId = @UserId",
                            new
                            {
                                UserId = user.UserId,
                                ResetCode = resetCode,
                                ResetExpiry = resetExpiry
                            });

                        // Send reset email
                        bool emailSent = SendPasswordResetEmail(email, resetCode, user.FirstName);

                        if (!emailSent)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to send password reset email to {email}");
                        }

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "If the email exists, a reset code has been sent."
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Password reset request error: {ex.Message}");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Failed to process request. Please try again."
                    };
                }
            }

            /// <summary>
            /// Reset password with code
            /// </summary>
            public static AjaxResults ResetPassword(string email, string code, string newPassword)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        var user = cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT UserId, ResetPasswordCode, ResetPasswordExpiry 
                        FROM Users 
                        WHERE Email = @Email",
                            new { Email = email });

                        if (user == null)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "User not found"
                            };
                        }

                        if (string.IsNullOrEmpty(user.ResetPasswordCode))
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "No reset code found. Please request a new one."
                            };
                        }

                        if (DateTime.Now > user.ResetPasswordExpiry)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Reset code has expired. Please request a new one."
                            };
                        }

                        if (user.ResetPasswordCode.ToString() != code.Trim())
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Invalid reset code"
                            };
                        }

                        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                        {
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Password must be at least 6 characters"
                            };
                        }

                        // Hash new password
                        string passwordHash = HashPassword(newPassword);

                        // Update password and clear reset code
                        cn.Execute(@"
                        UPDATE Users 
                        SET PasswordHash = @PasswordHash, 
                            ResetPasswordCode = NULL, 
                            ResetPasswordExpiry = NULL 
                        WHERE UserId = @UserId",
                            new
                            {
                                UserId = user.UserId,
                                PasswordHash = passwordHash
                            });

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "Password reset successfully! You can now log in with your new password."
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Password reset error: {ex.Message}");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Failed to reset password. Please try again."
                    };
                }
            }

            #endregion

            #region User Management

            /// <summary>
            /// Get all users (Admin only)
            /// </summary>
            public static List<dynamic> GetAllUsers()
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        var users = cn.Query<dynamic>(@"
                        SELECT 
                            UserId, 
                            FirstName, 
                            LastName, 
                            Email, 
                            Phone, 
                            IsEmailVerified, 
                            IsActive, 
                            CreatedDate, 
                            LastLoginDate 
                        FROM Users 
                        ORDER BY CreatedDate DESC").ToList();

                        return users;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Get all users error: {ex.Message}");
                    return new List<dynamic>();
                }
            }

            /// <summary>
            /// Get user by ID
            /// </summary>
            public static dynamic GetUserById(int userId)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        return cn.QuerySingleOrDefault<dynamic>(@"
                        SELECT 
                            UserId, 
                            FirstName, 
                            LastName, 
                            Email, 
                            Phone, 
                            IsEmailVerified, 
                            IsActive, 
                            CreatedDate, 
                            LastLoginDate,
                            ProfileImage
                        FROM Users 
                        WHERE UserId = @UserId",
                            new { UserId = userId });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Get user by ID error: {ex.Message}");
                    return null;
                }
            }

            /// <summary>
            /// Get user ID by email
            /// </summary>
            public static int GetUserIdByEmail(string email)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        return cn.QuerySingleOrDefault<int>(
                            "SELECT UserId FROM Users WHERE Email = @Email AND IsActive = 1",
                            new { Email = email });
                    }
                }
                catch
                {
                    return 0;
                }
            }

            /// <summary>
            /// Update user profile
            /// </summary>
            public static AjaxResults UpdateUserProfile(int userId, string firstName, string lastName, string phone)
            {
                try
                {
                   using (var cn = GetOpenConnection())
                    {
                        cn.Execute(@"
                        UPDATE Users 
                        SET FirstName = @FirstName, 
                            LastName = @LastName, 
                            Phone = @Phone 
                        WHERE UserId = @UserId",
                            new
                            {
                                UserId = userId,
                                FirstName = firstName,
                                LastName = lastName,
                                Phone = phone
                            });

                        return new AjaxResults
                        {
                            code = "1",
                            title = "Success",
                            message = "Profile updated successfully"
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Update profile error: {ex.Message}");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "Failed to update profile"
                    };
                }
            }

            #endregion

            #region Helper Methods

            /// <summary>
            /// Hash password using SHA256
            /// </summary>
            private static string HashPassword(string password)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString();
                }
            }

            /// <summary>
            /// Verify password against hash
            /// </summary>
            private static bool VerifyPassword(string password, string hash)
            {
                string passwordHash = HashPassword(password);
                return passwordHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Generate random 6-digit verification code
            /// </summary>
            private static string GenerateVerificationCode()
            {
                Random random = new Random();
                return random.Next(100000, 999999).ToString();
            }

            /// <summary>
            /// Send verification email
            /// </summary>
            private static bool SendVerificationEmail(string email, string code, string firstName)
            {
                try
                {
                    // Email configuration - UPDATE THESE WITH YOUR SMTP SETTINGS
                    string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                    int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                    string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "your-email@gmail.com";
                    string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "your-app-password";
                    string fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? smtpUsername;
                    string fromName = ConfigurationManager.AppSettings["FromName"] ?? "ReXell Marketplace";

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(fromEmail, fromName);
                        mail.To.Add(email);
                        mail.Subject = "Verify Your Email - ReXell Marketplace";
                        mail.IsBodyHtml = true;
                        mail.Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #e91e63;'>Welcome to ReXell, {firstName}!</h2>
                                <p>Thank you for registering. Please verify your email address to complete your registration.</p>
                                <div style='background-color: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                    <p style='margin: 0; font-size: 14px; color: #666;'>Your verification code is:</p>
                                    <h1 style='margin: 10px 0; color: #e91e63; font-size: 32px; letter-spacing: 5px;'>{code}</h1>
                                </div>
                                <p style='color: #666; font-size: 14px;'>This code will expire in 24 hours.</p>
                                <p style='color: #666; font-size: 14px;'>If you didn't create an account, please ignore this email.</p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                                <p style='color: #999; font-size: 12px;'>ReXell Marketplace - Buy & Sell with Confidence</p>
                            </div>
                        </body>
                        </html>
                    ";

                        using (SmtpClient smtp = new SmtpClient(smtpHost, smtpPort))
                        {
                            smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Email send error: {ex.Message}");
                    return false;
                }
            }

            /// <summary>
            /// Send password reset email
            /// </summary>
            private static bool SendPasswordResetEmail(string email, string code, string firstName)
            {
                try
                {
                    string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                    int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                    string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "your-email@gmail.com";
                    string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "your-app-password";
                    string fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? smtpUsername;
                    string fromName = ConfigurationManager.AppSettings["FromName"] ?? "ReXell Marketplace";

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(fromEmail, fromName);
                        mail.To.Add(email);
                        mail.Subject = "Password Reset - ReXell Marketplace";
                        mail.IsBodyHtml = true;
                        mail.Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #e91e63;'>Password Reset Request</h2>
                                <p>Hi {firstName},</p>
                                <p>We received a request to reset your password. Use the code below to reset your password:</p>
                                <div style='background-color: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                    <p style='margin: 0; font-size: 14px; color: #666;'>Your reset code is:</p>
                                    <h1 style='margin: 10px 0; color: #e91e63; font-size: 32px; letter-spacing: 5px;'>{code}</h1>
                                </div>
                                <p style='color: #666; font-size: 14px;'>This code will expire in 1 hour.</p>
                                <p style='color: #666; font-size: 14px;'>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                                <p style='color: #999; font-size: 12px;'>ReXell Marketplace - Buy & Sell with Confidence</p>
                            </div>
                        </body>
                        </html>
                    ";

                        using (SmtpClient smtp = new SmtpClient(smtpHost, smtpPort))
                        {
                            smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Password reset email error: {ex.Message}");
                    return false;
                }
            }

            #endregion
        }
    }