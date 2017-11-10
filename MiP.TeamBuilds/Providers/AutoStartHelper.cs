using Microsoft.Win32;
using System.Reflection;

namespace MiP.TeamBuilds.Providers
{
    public static class AutoStartHelper
    {
        private const string RegistryKey_Run = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string AutoStartValueName = "MiP_TeamBuilds";

        public static bool IsStartupItem()
        {
            using (var key = GetReadable())
                return IsStartupItem(key);
        }

        public static bool IsStartupItem(RegistryKey key)
        {
            return key?.GetValue(AutoStartValueName) != null;
        }

        public static void SetAutoStart(bool autostart)
        {
            using (var key = GetWriteable())
            {
                if (key == null)
                    return;

                bool isCurrentlyAutostarting = IsStartupItem(key);
                if (autostart == isCurrentlyAutostarting)
                    return;

                if (autostart)
                {
                    string path = Assembly.GetEntryAssembly().Location;
                    key.SetValue(AutoStartValueName, path);
                }
                else
                {
                    key.DeleteValue(AutoStartValueName, false);
                }
            }
        }

        private static RegistryKey GetReadable()
        {
            return GetKey(false);
        }

        private static RegistryKey GetWriteable()
        {
            return GetKey(true);
        }

        private static RegistryKey GetKey(bool writeable)
        {
            return Registry.CurrentUser.OpenSubKey(RegistryKey_Run, writeable);
        }
    }
}
