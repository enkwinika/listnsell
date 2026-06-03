using System;
using System.Net;
using System.Web.Mvc;
using rexell.Models;

namespace rexell.Filters
{
    /// <summary>
    /// Validates model state and returns a JSON error response for invalid models
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                var errors = new System.Collections.Generic.List<string>();
                
                foreach (var modelState in filterContext.Controller.ViewData.ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errors.Add(!string.IsNullOrEmpty(error.ErrorMessage) 
                            ? error.ErrorMessage 
                            : error.Exception?.Message ?? "Validation error");
                    }
                }

                filterContext.Result = new JsonResult
                {
                    Data = new AjaxResults
                    {
                        code = "0",
                        title = "Validation Error",
                        message = string.Join("; ", errors)
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// Ensures user is authenticated before allowing access to action
    /// </summary>
    public class AuthenticateUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new AjaxResults
                    {
                        code = "0",
                        title = "Unauthorized",
                        message = "Please login to continue"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// Ensures user has admin role before allowing access to action
    /// </summary>
    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controllers.HomeController;
            
            if (controller == null || !filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new AjaxResults
                    {
                        code = "0",
                        title = "Unauthorized",
                        message = "Access denied"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
