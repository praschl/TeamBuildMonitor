namespace MiP.TeamBuilds.Configuration
{
    public class Config
    {
        public int Version
        {
            get;
            set;
        }

        public string TfsUrl
        {
            get;
            set;
        }

        public int MaxBuildAgeForDisplayDays
        {
            get;
            set;
        }
    }
}