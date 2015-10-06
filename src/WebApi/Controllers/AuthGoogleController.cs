using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using JWT;
using Microsoft.AspNet.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using WebApi.Models;
using WebApi.Shared;
using WebApi.ViewModels;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthGoogleController : BaseController
    {
        private readonly IMongoCollection<User> _collection;
        public AuthGoogleController()
        {
            _collection = Db.GetCollection<User>("users");
        }
        
        [HttpPost]
        [Route("/api/auth/google")]
        public async Task<ActionResult> Post([FromBody]OAuthClinetDetails clinetDetails)
        {
            if (!ModelState.IsValid) return new BadRequestResult();

            try
            {
                const string url = "https://accounts.google.com/o/oauth2/token";
                const string apiUrl = "https://www.googleapis.com/oauth2/v1/userinfo";
                GoogleProfileInfo profileInfo;

                using (var client = new WebClient())
                {
                    var data = new NameValueCollection();
                    data.Set("client_id", clinetDetails.ClientId);
                    data.Set("code", clinetDetails.Code);
                    data.Set("grant_type", "authorization_code");
                    data.Set("client_secret", "_C-7pAIh_WS7j1wlfVkqRHmM");

                    if (!string.IsNullOrEmpty(clinetDetails.RedirectUri))
                        data.Set("redirect_uri", clinetDetails.RedirectUri);

                    //first request to get access token
                    var response = client.UploadValues(url, "POST", data);
                    var responseString = client.Encoding.GetString(response);
                    var oauthTokenDetails = JsonConvert.DeserializeObject<OAuthTokenDetails>(responseString);

                    //second request to get profile info
                    var headers = new NameValueCollection();
                    headers.Set("Authorization", "Bearer " + oauthTokenDetails.AccessToken);
                    client.Headers = new WebHeaderCollection {headers};
                    var response2 = client.DownloadData(apiUrl);
                    var responseString2 = client.Encoding.GetString(response2);
                    profileInfo = JsonConvert.DeserializeObject<GoogleProfileInfo>(responseString2);
                }

                var builder = Builders<User>.Filter;
                var filter = builder.Eq("ProviderId", profileInfo.Id) & builder.Eq("Provider", "google");
                var user = await _collection.Find(filter).FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User()
                    {
                        Name = profileInfo.Name,
                        ProviderId = profileInfo.Id,
                        Provider = "google",
                        Picture = profileInfo.Picture,
                        Email = profileInfo.Email,
                        Roles = new[] {"User"}
                    };
                    await _collection.InsertOneAsync(user);
                }
                //map to view model excluding the sensitive information
                Mapper.CreateMap<User, UserDetails>();
                var userDetails = Mapper.Map<UserDetails>(user);

                //return jwt
                var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var issueTime = DateTime.Now;

                var iat = (int) issueTime.Subtract(utc0).TotalSeconds;
                var exp = (int) issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;
                // Expiration time is up to 1 hour, but lets play on safe side

                var payload = new
                {
                    sub = userDetails,
                    exp = exp,
                    iat = iat
                };

                return
                    new JsonResult(
                        new {token = JsonWebToken.Encode(payload, JwtConstants.SecretKey, JwtHashAlgorithm.HS256), code = 200});
            }
            catch (WebException webException)
            {
                var statusCode = HttpStatusCode.BadRequest;
                var response = webException.Response as HttpWebResponse;
                if (response != null) statusCode = response.StatusCode;
                return new JsonResult(new { error = webException.ToString(), code = statusCode });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.ToString(), code = 500 });
            }
        }
    }
}
