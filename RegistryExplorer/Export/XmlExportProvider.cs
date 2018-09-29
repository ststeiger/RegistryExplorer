
namespace RegistryExplorer.Export
{


    class XmlExportProvider
        : ExportProvider
    {
        System.Xml.XmlTextWriter xmlWriter;
        bool firstKey;


        public XmlExportProvider(System.IO.TextWriter writer): base(writer)
        {
            xmlWriter = new System.Xml.XmlTextWriter(writer);
            xmlWriter.Formatting = System.Xml.Formatting.Indented;
            firstKey = true;
        }


        public override void BeginExport()
        {
            xmlWriter.WriteStartDocument();
        }


        public override void WriteKeyStart(string key)
        {
            if (firstKey)
            {
                xmlWriter.WriteStartElement("registry");
                xmlWriter.WriteAttributeString("branch", key);
                firstKey = false;
            }
            else
            {
                xmlWriter.WriteStartElement("key");
                xmlWriter.WriteAttributeString("name", key.Substring(key.LastIndexOf('\\') + 1));
            }
        }


        public override void WriteKeyEnd()
        {
            xmlWriter.WriteEndElement();
        }


        public override void WriteValue(string name, Microsoft.Win32.RegistryValueKind kind, object data)
        {
            xmlWriter.WriteStartElement("value");
            xmlWriter.WriteAttributeString("name", name);
            xmlWriter.WriteAttributeString("type", RegistryExplorer.Registry.Extensions.ToDataType(kind) );
            xmlWriter.WriteAttributeString("data", RegistryExplorer.Registry.RegValue.ToString(kind, data));
            xmlWriter.WriteEndElement();
        }


        public override void EndExport()
        {
            xmlWriter.WriteEndDocument();
        }


    }


}
