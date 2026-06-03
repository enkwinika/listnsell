using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class SellerInfo
    {
        public string Name { get; set; }
        public bool IsVerified { get; set; }
        public string JoinDate { get; set; }
        public string ProfileImage { get; set; }
    }
}