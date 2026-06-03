using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class AjaxResults
    {
        public string code { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string email { get; set; }
        public bool isAdmin { get; set; }
        public object data { get; set; }
    }
}