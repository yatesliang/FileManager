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
            Collection<NoLogin> controllerFilter = actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<NoLogin>(false);
            Collection<NoLogin> actionFilter = actionContext.ActionDescriptor.GetCustomAttributes<NoLogin>(false);
            if (controllerFilter.Count == 1 || actionFilter.Count == 1)
            {
                return;
            }
            var userId = HttpContext.Current.Session["id"];
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