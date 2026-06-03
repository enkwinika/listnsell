using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class ListingResponse
    {
        public int ListingId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> Images { get; set; }
        public SellerInfo Seller { get; set; }
    }
}