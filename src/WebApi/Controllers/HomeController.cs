using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "hello", "web api" };
        }
    }
}
