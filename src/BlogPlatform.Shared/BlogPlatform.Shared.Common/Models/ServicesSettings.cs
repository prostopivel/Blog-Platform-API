namespace BlogPlatform.Shared.Common.Models
{
    public class ServicesSettings
    {
        public string Auth { get; set; } = string.Empty;
        public string AuthGrpc {  get; set; } = string.Empty;
        public string Blog { get; set; } = string.Empty;
        public string BlogGrpc { get; set; } = string.Empty;
        public string Analytics { get; set; } = string.Empty;
        public string AnalyticsGrpc { get; set; } = string.Empty;
    }
}
