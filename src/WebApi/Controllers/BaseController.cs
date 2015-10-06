using Microsoft.AspNet.Mvc;
using MongoDB.Driver;

namespace WebApi.Controllers
{
    public class BaseController : Controller
    {
        protected readonly MongoClient Client;
        protected readonly IMongoDatabase Db;

        public BaseController()
        {
            Client = new MongoClient("mongodb://user:readonly@ds033831.mongolab.com:33831/jwtdb"); //change the connection string with your db
            Db = Client.GetDatabase("jwtdb");
        }
    }
}
