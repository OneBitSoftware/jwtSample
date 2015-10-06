using MongoDB.Bson;

namespace WebApi.Models
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string[] Roles { get; set; }
        public string Password { get; set; }
        public string ProviderId { get; set; }
        public string Provider { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
    }
}
