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
    public class AuthFacebookController : BaseController
    {
        private readonly IMongoCollection<User> _collection;
        public AuthFacebookController()
        {
            _collection = Db.GetCollection<User>("users");
        }

        [HttpPost]
        [Route("/api/auth/facebook")]
        public async Task<ActionResult> Post([FromBody]OAuthClinetDetails clinetDetails)
        {
            if (!ModelState.IsValid) return new BadRequestResult();

            try
            {
                const string url = "https://graph.facebook.com/v2.3/oauth/access_token";
                const string apiUrl = "https://graph.facebook.com/me?fields=id,email,name,locale,picture";
                FacebookProfileInfo profileInfo;

                using (var client = new WebClient())
                {
                    var data = new NameValueCollection();
                    data.Set("client_id", clinetDetails.ClientId);
                    data.Set("code", clinetDetails.Code);
                    data.Set("redirect_uri", clinetDetails.RedirectUri);
                    data.Set("client_secret", "4ddbdcef0e15f38348218f4a54fd53ce");

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
                    profileInfo = JsonConvert.DeserializeObject<FacebookProfileInfo>(responseString2);
                }

                var builder = Builders<User>.Filter;
                var filter = builder.Eq("ProviderId", profileInfo.Id) & builder.Eq("Provider", "facebook");
                var user = await _collection.Find(filter).FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User()
                    {
                        Name = profileInfo.Name,
                        ProviderId = profileInfo.Id,
                        Provider = "facebook",
                        Picture = profileInfo.Picture.Data.Url,
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


        [HttpPost]
        [Route("/api/auth/facebook/mobile")]
        public async Task<ActionResult> Post([FromBody]AccessTokenDetails tokenDetails)
        {
            if (!ModelState.IsValid) return new BadRequestResult();

            try
            {
                const string apiUrl = "https://graph.facebook.com/me?fields=id,email,name,locale,picture";
                FacebookProfileInfo profileInfo;

                using (var client = new WebClient())
                {
                    //second request to get profile info
                    var headers = new NameValueCollection();
                    headers.Set("Authorization", "Bearer " + tokenDetails.AccessToken);
                    client.Headers = new WebHeaderCollection { headers };
                    var response2 = client.DownloadData(apiUrl);
                    var responseString2 = client.Encoding.GetString(response2);
                    profileInfo = JsonConvert.DeserializeObject<FacebookProfileInfo>(responseString2);
                }

                var builder = Builders<User>.Filter;
                var filter = builder.Eq("ProviderId", profileInfo.Id) & builder.Eq("Provider", "facebook");
                var user = await _collection.Find(filter).FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User()
                    {
                        Name = profileInfo.Name,
                        ProviderId = profileInfo.Id,
                        Provider = "facebook",
                        Picture = profileInfo.Picture.Data.Url,
                        Email = profileInfo.Email,
                        Roles = new[] { "User" }
                    };
                    await _collection.InsertOneAsync(user);
                }
                //map to view model excluding the sensitive information
                Mapper.CreateMap<User, UserDetails>();
                var userDetails = Mapper.Map<UserDetails>(user);

                //return jwt
                var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var issueTime = DateTime.Now;

                var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
                var exp = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;
                // Expiration time is up to 1 hour, but lets play on safe side

                var payload = new
                {
                    sub = userDetails,
                    exp = exp,
                    iat = iat
                };

                return
                    new JsonResult(
                        new { token = JsonWebToken.Encode(payload, JwtConstants.SecretKey, JwtHashAlgorithm.HS256), code = 200 });
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
