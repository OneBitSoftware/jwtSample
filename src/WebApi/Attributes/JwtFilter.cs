using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using JWT;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using WebApi.Shared;
using WebApi.ViewModels;

namespace WebApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JwtFilter : ActionFilterAttribute
    {
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string[] jwtArray;
            context.HttpContext.Request.Headers.TryGetValue("Authorization", out jwtArray);
            if (jwtArray == null || !jwtArray.Any()) return Task.FromResult(context.Result = new HttpUnauthorizedResult());
            var jwt = jwtArray[0].Replace("Bearer ", string.Empty);

            try
            {
                var jsonPayload = JsonWebToken.Decode(jwt, JwtConstants.SecretKey);
                var user = JsonConvert.DeserializeObject<JwtPayload>(jsonPayload).Sub;
                //var user = new User() {Name = "Velkata", Roles = new[] {"Admin"}};
                context.HttpContext.User = new ProfilePrincipal(new GenericIdentity(user.Name), user.Roles, user.Email, user.Picture);
                return base.OnActionExecutionAsync(context, next);
            }
            catch (SignatureVerificationException)
            {
                return Task.FromResult(context.Result = new HttpUnauthorizedResult());
            }
            catch (Exception ex)
            {
                return Task.FromResult(context.Result = new HttpStatusCodeResult(500));
            }
        }
    }
}
