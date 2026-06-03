using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Security.Principal;
using System.Web.Security;
using NLog;

namespace rexell
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            Logger.Info("Application starting...");
            
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            Logger.Info("Application started successfully");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                Logger.Error(exception, "Unhandled application error");
            }
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    if (authTicket != null && !authTicket.Expired)
                    {
                        // Parse user data: userId|email|username
                        string[] userData = authTicket.UserData.Split('|');

                        if (userData.Length >= 3)
                        {
                            string userId = userData[0];
                            string email = userData[1];
                            string username = userData[2];

                            // Create custom principal
                            GenericIdentity identity = new GenericIdentity(username, "Forms");
                            GenericPrincipal principal = new GenericPrincipal(identity, null);

                            // Set the principal
                            Context.User = principal;
                            System.Threading.Thread.CurrentPrincipal = principal;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                    System.Diagnostics.Debug.WriteLine("Auth error: " + ex.Message);
                }
            }
        }
    }
}
