using System.Security.Principal;

namespace WebApi.Shared
{
    public class ProfilePrincipal : GenericPrincipal
    {
        public string Email { get; set; }
        public string Id { get; set; }
        public string Picture { get; set; }
        
        public ProfilePrincipal(IIdentity identity, string[] roles) : base(identity, roles)
        {
        }

        public ProfilePrincipal(IIdentity identity, string id, string[] roles, string email, string picture) : base(identity, roles)
        {
            Email = email;
            Picture = picture;
            Id = id;
        }
    }
}
