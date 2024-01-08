using Microsoft.AspNetCore.Mvc;
using WalkGuideFront.Models.Filters;

namespace WalkGuideFront.Models.Attributes
{
    public class CustomAuthorizationAttribute : TypeFilterAttribute
    {
        public CustomAuthorizationAttribute() : base(typeof(CustomAuthorizationFilter)) { }
    }
}
