using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using JWT;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using WebApi.Shared;
using WebApi.ViewModels;

namespace WebApi.Controllers
{
    public class SecureController : BaseController
    {
        private readonly string _roles;

        public SecureController()
        {
        }

        public SecureController(string roles)
        {
            _roles = roles;
        }

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

                //check roles
                if (_roles != null)
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
            var roles = _roles.Replace(" ", string.Empty).Split(',');

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
