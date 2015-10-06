using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Cors.Core;
using Microsoft.AspNet.Mvc;
using MongoDB.Driver;
using WebApi.Models;
using WebApi.Shared;
using WebApi.ViewModels;

namespace WebApi.Controllers
{
    //[EnableCors("allowAll")]
    [Route("api/[controller]")]
    public class AuthLocalController : BaseController
    {
        private readonly IMongoCollection<User> _collection;
        public AuthLocalController()
        {
            _collection = Db.GetCollection<User>("users");
        }

        [HttpPost]
        [Route("/api/auth/local")]
        public async Task<ActionResult> Post([FromBody]LoginUser loginUser)
        {
            //return new JsonResult(new { token = "123" });

            if (!ModelState.IsValid) return new BadRequestResult();

            var builder = Builders<User>.Filter;

            User user = null;
            try
            {
                if (!string.IsNullOrEmpty(loginUser.Name))
                {
                    var filter = builder.Eq("Name", loginUser.Name); //& builder.Eq("Password", hashedPassword);
                    user = await _collection.Find(filter).FirstOrDefaultAsync();
                }
                else if (!string.IsNullOrEmpty(loginUser.Email))
                {
                    loginUser.Name = loginUser.Email;
                    var filter = builder.Eq("Email", loginUser.Email);
                    user = await _collection.Find(filter).FirstOrDefaultAsync();
                }

                if (user == null)
                {
                    var hashedPassword = Utils.HashPassword(loginUser.Password);
                    user = new User() { Name = loginUser.Name, Password = hashedPassword, Email = loginUser.Email, Roles = new[] { "User" } };
                    await _collection.InsertOneAsync(user);
                }

                //verify password
                if (!Utils.VerifyHashedPassword(loginUser.Password, user.Password)) return new HttpUnauthorizedResult();

                //map to view model excluding the sensitive information
                Mapper.CreateMap<User, UserDetails>(); //.ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id.ToString()));
                var userDetails = Mapper.Map<UserDetails>(user);

                //return jwt
                var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var issueTime = DateTime.Now;

                var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
                var exp = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds; // Expiration time is up to 1 hour, but lets play on safe side

                var payload = new
                {
                    sub = userDetails,
                    exp = exp,
                    iat = iat
                };

                return new JsonResult(new { token = JWT.JsonWebToken.Encode(payload, JwtConstants.SecretKey, JWT.JwtHashAlgorithm.HS256) });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.ToString(), stack = ex.StackTrace.ToString() });
            } 
        }
    }
}
