// GetDefaultBrowser/RegistryKey

namespace RegistryExplorer
{


    static class ShellUtility
    {      
        
        static string GetDefaultBrowser()
        {
            string keyPath = @"htmlfile\shell\open\command";
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(keyPath);
            string browserPath = key.GetValue(string.Empty).ToString();
            browserPath = browserPath.Split('\"')[1];
            return browserPath;
        }


        public static void OpenWebPage(string url)
        {
            System.Diagnostics.Process.Start(GetDefaultBrowser(), url);
        }


    }


}
