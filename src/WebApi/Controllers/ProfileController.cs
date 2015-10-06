using Microsoft.AspNet.Mvc;
using WebApi.Attributes;
using WebApi.Shared;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ProfileController : SecureController
    {
        //public ProfileController() : base("Admin")
        //{
        //}
        
        [HttpGet]
        //[JwtAuthorize(Roles = "Admin, Admin2, User")]
        public ActionResult Get()
        {
            var user = (ProfilePrincipal)Context.User;
            return new JsonResult(new { name = user.Identity.Name, email = user.Email, picture = user.Picture});
        }

        [HttpGet("{id}")]
        [JwtFilter]
        public string Get(int id)
        {
            return "success " + Context.User.Identity.Name;
        }
    }
}
