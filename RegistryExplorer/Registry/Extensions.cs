
namespace RegistryExplorer.Registry
{


    static class Extensions
    {


        public static object GetDefaultData(this Microsoft.Win32.RegistryValueKind valueKind)
        {

            switch (valueKind)
            {
                case Microsoft.Win32.RegistryValueKind.Binary:
                    return new byte[0];
                case Microsoft.Win32.RegistryValueKind.DWord:
                    return 0;
                case Microsoft.Win32.RegistryValueKind.ExpandString:
                    return string.Empty;
                case Microsoft.Win32.RegistryValueKind.MultiString:
                    return new string[0];
                case Microsoft.Win32.RegistryValueKind.QWord:
                    return (long)0;
                case Microsoft.Win32.RegistryValueKind.String:
                    return string.Empty;
                case Microsoft.Win32.RegistryValueKind.Unknown:
                default:
                    return null;
            } // End switch (valueKind) 

        } // End Function GetDefaultData 


        public static string ToDataType(this Microsoft.Win32.RegistryValueKind valueKind)
        {

            switch (valueKind)
            {
                case Microsoft.Win32.RegistryValueKind.Binary:
                    return "REG_BINARY";
                case Microsoft.Win32.RegistryValueKind.DWord:
                    return "REG_DWORD";
                case Microsoft.Win32.RegistryValueKind.ExpandString:
                    return "REG_EXPAND_SZ";
                case Microsoft.Win32.RegistryValueKind.MultiString:
                    return "REG_MULTI_SZ";
                case Microsoft.Win32.RegistryValueKind.QWord:
                    return "REG_QWORD";
                case Microsoft.Win32.RegistryValueKind.String:
                    return "REG_SZ";
                case Microsoft.Win32.RegistryValueKind.Unknown:
                    return "REG_UNKNOWN";
                default:
                    return string.Empty;
            } // End switch (valueKind) 

        } // End Function ToDataType 


        public static string ToHiveName(this Microsoft.Win32.RegistryHive regHive)
        {

            switch (regHive)
            {
                case Microsoft.Win32.RegistryHive.ClassesRoot:
                    return "HKEY_CLASSES_ROOT";
                case Microsoft.Win32.RegistryHive.CurrentConfig:
                    return "HKEY_CURRENT_CONFIG";
                case Microsoft.Win32.RegistryHive.CurrentUser:
                    return "HKEY_CURRENT_USER";
                case Microsoft.Win32.RegistryHive.DynData:
                    return "HKEY_DYN_DATA";
                case Microsoft.Win32.RegistryHive.LocalMachine:
                    return "HKEY_LOCAL_MACHINE";
                case Microsoft.Win32.RegistryHive.PerformanceData:
                    return "HKEY_PERFORMANCE_DATA";
                case Microsoft.Win32.RegistryHive.Users:
                    return "HKEY_USERS";
                default:
                    return string.Empty;
            } // End switch (regHive) 

        } // End Function ToHiveName 


        public static Microsoft.Win32.RegistryKey ToKey(this Microsoft.Win32.RegistryHive regHive)
        {

            switch (regHive)
            {
                case Microsoft.Win32.RegistryHive.ClassesRoot:
                    return Microsoft.Win32.Registry.ClassesRoot;
                case Microsoft.Win32.RegistryHive.CurrentConfig:
                    return Microsoft.Win32.Registry.CurrentConfig;
                case Microsoft.Win32.RegistryHive.CurrentUser:
                    return Microsoft.Win32.Registry.CurrentUser;
                case Microsoft.Win32.RegistryHive.DynData:
                    return Microsoft.Win32.Registry.DynData;
                case Microsoft.Win32.RegistryHive.LocalMachine:
                    return Microsoft.Win32.Registry.LocalMachine;
                case Microsoft.Win32.RegistryHive.PerformanceData:
                    return Microsoft.Win32.Registry.PerformanceData;
                case Microsoft.Win32.RegistryHive.Users:
                    return Microsoft.Win32.Registry.Users;
                default:
                    return null;
            } // End switch (regHive) 

        } // End Function ToKey 


    }


}