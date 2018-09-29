
namespace RegistryExplorer.Registry
{


    static class RegistryExplorerr
    {

        public const string RegistryFavoritePath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit\Favorites";


        public static System.Collections.Generic.List<RegKey> GetSubKeys(Microsoft.Win32.RegistryKey key)
        {
            int subKeyCount = key.SubKeyCount;
            if (subKeyCount == 0)
                return new System.Collections.Generic.List<RegKey>();

            System.Collections.Generic.List<RegKey> subKeys = new System.Collections.Generic.List<RegKey>(subKeyCount);            

            string[] subKeyNames = key.GetSubKeyNames();

            for (int i = 0; i < subKeyNames.Length; i++)
            {
                try
                {
                    string keyName = subKeyNames[i];
                    RegKey item = new RegKey(keyName, key.OpenSubKey(keyName));
                    subKeys.Add(item);
                }
                catch { }
            }

            return subKeys;
        }


        public static System.Collections.Generic.List<RegValue> GetValues(Microsoft.Win32.RegistryKey key)
        {
            int valueCount = key.ValueCount;
            if (valueCount == 0)
                return new System.Collections.Generic.List<RegValue>();

            System.Collections.Generic.List<RegValue> values = new System.Collections.Generic.List<RegValue>(valueCount);
            string[] valueNames = key.GetValueNames();

            for (int i = 0; i < valueNames.Length; i++) 
                values.Add(new RegValue(key, valueNames[i]));

            return values;
        }


    }


}