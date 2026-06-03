using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class MessageRequest
    {
        public int ListingId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageText { get; set; }
    }
}