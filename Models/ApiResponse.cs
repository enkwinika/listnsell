using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rexell.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        //public T Data { get; set; }
        public string Message { get; set; }
        public int? TotalCount { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}