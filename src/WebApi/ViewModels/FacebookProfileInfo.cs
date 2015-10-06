namespace WebApi.ViewModels
{
    public class FacebookProfileInfo : ProfileInfo
    {
        public FacebookData Picture { get; set; }
    }

    public class FacebookData
    {
        public FacebookDataUrl Data { get; set; }
    }

    public class FacebookDataUrl
    {
        public string Url { get; set; }
    }
}
