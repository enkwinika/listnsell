using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class ListingFilterRequest
    {
        public string SearchTerm { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<string> Conditions { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Location { get; set; }
        public string SortBy { get; set; } // newest, price-low, price-high, popular
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}