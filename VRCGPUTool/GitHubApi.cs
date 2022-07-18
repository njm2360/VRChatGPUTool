namespace VRCGPUTool
{
    internal class GitHubApi
    {
        public string tag_name { get; set; }
        public string body { get; set; }
    }

    internal class RepoApi
    {
        public string body { get; set; }
        public string ver { get; set; } 
    }
}
