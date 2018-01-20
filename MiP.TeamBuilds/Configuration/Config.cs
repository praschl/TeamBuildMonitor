using System.Xml.Serialization;

namespace MiP.TeamBuilds.Configuration
{
    public class Config
    {
        [XmlAttribute("version")]
        public int Version
        {
            get;
            set;
        }

        [XmlAttribute("tfsUrl")]
        public string TfsUrl
        {
            get;
            set;
        }

        [XmlAttribute("maxBuildAgeForDisplayDays")]
        public int MaxBuildAgeForDisplayDays
        {
            get;
            set;
        }
    }
}