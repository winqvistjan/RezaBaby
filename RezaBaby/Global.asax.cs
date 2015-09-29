using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using RezaBaby.Controllers;

namespace RezaBaby
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //
        public void Application_Error(Object sender, EventArgs e)
        {
            if (Server != null)
            {
                //Get the context         
                HttpContext appContext = ((MvcApplication)sender).Context;
                Exception ex = Server.GetLastError().GetBaseException();
                //Log the error using the logging framework         
                //Logger.Error(ex);
                //
                System.Diagnostics.Debug.WriteLine(ex);
                Response.Redirect("/ErrorManager/ServerError");
                //
                //Clear the last error on the server so that custom errors are not fired       
                Server.ClearError();
                //Redirect user
                //Context.Server.TransferRequest("~/Error/");
                //forward the user to the error manager controller.         
                //IController errorController = new ErrorManagerController();
                //RouteData routeData = new RouteData();
                //routeData.Values["controller"] = "ErrorManagerController";
                //routeData.Values["action"] = "ServerError";
                //errorController.Execute(
                //    new RequestContext(new HttpContextWrapper(appContext), routeData));     
            }
        }

        //
    }
}
