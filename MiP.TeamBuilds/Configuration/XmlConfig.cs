using System;
using System.IO;
using System.Xml.Serialization;

using MiP.TeamBuilds.Properties;

namespace MiP.TeamBuilds.Configuration
{
    public class XmlConfig
    {
        public static Config Instance = Initialize();


        private static readonly XmlSerializer _configSerializer = new XmlSerializer(typeof(Config));

        private static Config Initialize()
        {
            return new Config();
        }

        public static string ConfigurationPath
        {
            get
            {
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TFSMergePro");

                Directory.CreateDirectory(configPath);

                return Path.Combine(configPath, "settings.json");
            }
        }

        private static Config _current;

        public static Config Current
        {
            get
            {
                if (_current == null)
                {
                    _current = LoadSettings();

                    MigrateFromExeConfig();
                }

                return _current;
            }
        }

        private static void MigrateFromExeConfig()
        {
            _current.TfsUrl = Settings.Default.TfsUrl;
            _current.MaxBuildAgeForDisplayDays = Settings.Default.MaxBuildAgeForDisplayDays;

            _current.Version = 1;

            Save();
        }

        private static void Save()
        {
            using (FileStream writer = File.Open(ConfigurationPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                _configSerializer.Serialize(writer, _current);
            }
        }

        private static Config LoadSettings()
        {
            if (!File.Exists(ConfigurationPath))
                return new Config();

            Config config = Deserialize();

            return config;
        }

        private static Config Deserialize()
        {
            using (FileStream reader = File.Open(ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (Config)_configSerializer.Deserialize(reader);
            }
        }
    }
}