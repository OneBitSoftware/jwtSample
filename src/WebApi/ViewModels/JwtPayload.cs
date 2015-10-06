namespace WebApi.ViewModels
{
    public class JwtPayload
    {
        public UserDetails Sub { get; set; }
        public string Exp { get; set; }
        public string Iat { get; set; }
    }
}
