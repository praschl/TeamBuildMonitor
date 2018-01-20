using System;
using System.IO;
using System.Xml.Serialization;

using MiP.TeamBuilds.Properties;

namespace MiP.TeamBuilds.Configuration
{
    public class XmlConfig
    {
        private static readonly XmlSerializer _configSerializer = new XmlSerializer(typeof(Config));

        private static string ConfigurationPath
        {
            get
            {
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiP/TeamBuilds");

                Directory.CreateDirectory(configPath);

                return Path.Combine(configPath, "settings.xml");
            }
        }

        private static Config _instance;

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    var current = LoadSettings();

                    if (current.Version == 0)
                        MigrateFromExeConfig(current);

                    _instance = current;
                }

                return _instance;
            }
        }

        private static void MigrateFromExeConfig(Config current)
        {
            current.TfsUrl = Settings.Default.TfsUrl;
            current.MaxBuildAgeForDisplayDays = Settings.Default.MaxBuildAgeForDisplayDays;

            current.Version = 1;

            Save(current);
        }

        public static void Save()
        {
            Save(_instance);
        }

        private static void Save(Config config)
        {
            using (var fileStream = File.Open(ConfigurationPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                _configSerializer.Serialize(fileStream, config);
            }
        }

        private static Config LoadSettings()
        {
            if (!File.Exists(ConfigurationPath))
                return new Config();

            var config = Deserialize();

            return config;
        }

        private static Config Deserialize()
        {
            using (var fileStream = File.Open(ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (Config)_configSerializer.Deserialize(fileStream);
            }
        }
    }
}