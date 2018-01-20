using System;
using System.IO;

using MiP.TeamBuilds.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiP.TeamBuilds.Configuration
{
    public static class JsonConfig
    {
        private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(),
                new IsoDateTimeConverter()
            },
            Formatting = Formatting.Indented
        };

        private static string ConfigurationPath
        {
            get
            {
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiP/TeamBuilds");

                Directory.CreateDirectory(configPath);

                return Path.Combine(configPath, "settings.json");
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
            string json = JsonConvert.SerializeObject(config, _serializerSettings);

            File.WriteAllText(ConfigurationPath, json);
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
            string json = File.ReadAllText(ConfigurationPath);

            return JsonConvert.DeserializeObject<Config>(json);
        }
    }
}