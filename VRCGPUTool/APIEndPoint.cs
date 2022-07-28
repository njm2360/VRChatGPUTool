namespace VRCGPUTool
{
    internal static class APIEndpoints
    {
        public const string FeedbackEndpoint = "https://a3reoytkul.execute-api.ap-northeast-1.amazonaws.com/api";
        public const string GitHubReleaseAPIEndpoint = "https://api.github.com/repos/njm2360/VRChatGPUTool/releases/latest";
        public const string ProvideDataAPIEndPoint = "";
    }
    internal class GitHubApiRes
    {
        public string tag_name { get; set; }
        public string body { get; set; }
    }

    internal class ReportApiRes
    {
        public string body { get; set; }
    }

    internal class ProvDataApiRes
    {
        public string body { get; set; }
    }
}