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
    public class JwtAuthorize : ActionFilterAttribute
    {
        public string Roles { get; set; }

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
                context.HttpContext.User = new ProfilePrincipal(new GenericIdentity(user.Name), user.Roles, user.Email, user.Picture);

                //check roles
                if (Roles != null)
                    if (!HasRolePermissions(user.Roles))
                        return Task.FromResult(context.Result = new HttpUnauthorizedResult());
                
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

        private bool HasRolePermissions(string[] userRoles)
        {
            var hasPermissions = false;
            var roles = Roles.Replace(" ", string.Empty).Split(',');
            
            foreach (var role in userRoles)
            {
                if (roles.Any(x => x == role))
                {
                    hasPermissions = true;
                    break;
                }
            }

            return hasPermissions;
        }
    }
}
