
namespace RegistryExplorer.Export
{


    class RegExporter
    {


        public static void Export(RegistryExplorer.Registry.RegKey key
            , ExportProvider provider)
        {
            provider.BeginExport();
            ExportKey(key, provider);
            provider.EndExport();
        }

        
        static void ExportKey(RegistryExplorer.Registry.RegKey key
            , ExportProvider provider)
        {
            provider.WriteKeyStart(key.Key.Name);
            ExportValues(key, provider);
            foreach (RegistryExplorer.Registry.RegKey subKey in RegistryExplorer.Registry.RegistryExplorerr.GetSubKeys(key.Key))
                ExportKey(subKey, provider);
            provider.WriteKeyEnd();
        }


        static void ExportValues(RegistryExplorer.Registry.RegKey key, ExportProvider provider)
        {
            foreach (RegistryExplorer.Registry.RegValue value in RegistryExplorer.Registry.RegistryExplorerr.GetValues(key.Key))
                provider.WriteValue(value.Name, value.Kind, value.Data);
        }


    }


}
