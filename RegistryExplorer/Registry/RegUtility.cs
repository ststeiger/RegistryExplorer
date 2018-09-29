
namespace RegistryExplorer.Registry
{


    static class RegUtility
    {
        public static string GetRegValueName(string value)
        {
            return value == string.Empty ? "(Default)" : value;
        }

        public static Microsoft.Win32.RegistryKey ParseRootKey(string path)
        {
            Microsoft.Win32.RegistryKey key;
            switch (path)
            {
                case "HKEY_CLASSES_ROOT": key = Microsoft.Win32.Registry.ClassesRoot; break;
                case "HKEY_CURRENT_USER": key = Microsoft.Win32.Registry.CurrentUser; break;
                case "HKEY_LOCAL_MACHINE": key = Microsoft.Win32.Registry.LocalMachine; break;
                case "HKEY_USERS": key = Microsoft.Win32.Registry.Users; break;
                default: key = Microsoft.Win32.Registry.CurrentConfig; break;
            }
            return key;
        }

        public static void SplitKey(string key, out string hive, out string branch)
        {
            int index = key.IndexOf('\\');
            hive = string.Empty;
            branch = string.Empty;
            if (index == -1)
                hive = key;
            else
            {
                hive = key.Substring(0, index);
                branch = key.Substring(index + 1);
            }
        }        

        public static bool DeleteKey(string key)
        {            
            try
            {
                RegKey child = RegKey.Parse(key);
                RegKey parent = RegKey.Parse(child.Parent, true);
                parent.Key.DeleteSubKeyTree(child.Name);
            }
            catch
            {
                return false;
            }            
            return true;
        }

        public static bool DeleteValue(string key, string value)
        {
            try
            {
                RegKey regKey = RegKey.Parse(key, true);
                regKey.Key.DeleteValue(value, false);
            }
            catch
            {
                return false;
            }
            return true;
        }
        
        public static string GetNewKeyName(Microsoft.Win32.RegistryKey key)
        {
            System.Collections.Generic.List<RegKey> subKeys = RegistryExplorerr.GetSubKeys(key);
            bool found = false;
            int suffix = 0;
            string title = string.Empty;
            while (!found)
            {
                suffix++;
                title = "New Key #" + suffix;
                found = !subKeys.Exists(subKey => subKey.Name == title);
            }
            return title;
        }

        public static string GetNewValueName(Microsoft.Win32.RegistryKey key)
        {
            System.Collections.Generic.List<RegValue> values = RegistryExplorerr.GetValues(key);
            bool found = false;
            int suffix = 0;
            string title = string.Empty;
            while (!found)
            {
                suffix++;
                title = "New Value #" + suffix;
                found = !values.Exists(val => val.Name == title);
            }
            return title;
        }
    }
}
