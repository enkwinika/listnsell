using System;
using System.Net;
using System.Web.Mvc;
using NLog;
using rexell.Models;

namespace rexell.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;

            var exception = filterContext.Exception;
            var controllerName = filterContext.RouteData.Values["controller"]?.ToString();
            var actionName = filterContext.RouteData.Values["action"]?.ToString();

            // Log the exception
            Logger.Error(exception, $"Unhandled exception in {controllerName}.{actionName}");

            // Determine if this is an AJAX request
            bool isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();

            if (isAjaxRequest)
            {
                // Return JSON response for AJAX requests
                filterContext.Result = new JsonResult
                {
                    Data = new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "An unexpected error occurred. Please try again later."
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            {
                // Return error view for regular requests
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary
                    {
                        Model = new HandleErrorInfo(exception, controllerName, actionName)
                    }
                };

                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
