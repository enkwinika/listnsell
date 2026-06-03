using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public List<string> Images { get; set; }
        public DateTime DatePosted { get; set; }
        public string Status { get; set; }
        public int SellerId { get; set; }
        public int Views { get; set; }
    }
}