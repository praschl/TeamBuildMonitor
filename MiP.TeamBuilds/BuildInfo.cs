using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds
{
    public class BuildInfo
    {
        public int Id { get; set; }

        public string TeamProject { get; set; }

        public string BuildDefinitionName { get; set; }

        public string[] ServerItems { get; set; }

        public string RequestedBy { get; set; }
        public bool Finished { get; set; }
        public BuildStatus Status { get; set; }
    }
}
