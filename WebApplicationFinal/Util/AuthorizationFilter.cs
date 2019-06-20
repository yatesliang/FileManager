using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System;
using System.Collections.ObjectModel;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace WebApplicationFinal.Util
{
    public class UserFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", HttpContext.Current.Request.Headers.GetValues("Origin")[0]);
            Collection<NoLogin> controllerFilter = actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<NoLogin>(false);
            Collection<NoLogin> actionFilter = actionContext.ActionDescriptor.GetCustomAttributes<NoLogin>(false);
            if (controllerFilter.Count == 1 || actionFilter.Count == 1)
            {
                return;
            }
            var userId = HttpContext.Current.Session["id"];
            HttpCookie cookie = HttpContext.Current.Request.Cookies["user_cookie"];
            var cookieId = cookie?.Value ?? null;
            Console.Write(userId);
            
            if (userId == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "User Not Login!");
            }
            base.OnActionExecuting(actionContext);
        }

    
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NoLogin: Attribute
    {
        public NoLogin()
        {

        }
    }
}