using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class MessageResponse
    {
        public int MessageId { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; }
        public string SenderName { get; set; }
        public string MessageText { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; }
        public string TimeAgo { get; set; }
    }
}