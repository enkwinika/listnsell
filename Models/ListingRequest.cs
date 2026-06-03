using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class ListingRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public List<string> ImagePaths { get; set; }
    }
}