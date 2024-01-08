using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WalkGuideFront.Models.Filters
{
    public class CustomAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userToken = context.HttpContext.Session.GetString("_UserToken");

            if (string.IsNullOrEmpty(userToken))
                context.Result = new UnauthorizedResult();
        }
    }
}
