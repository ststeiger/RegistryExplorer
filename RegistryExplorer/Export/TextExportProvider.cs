
namespace RegistryExplorer.Export
{


    class TextExportProvider
        : ExportProvider
    {
        System.Collections.Generic.Stack<int> counters;
        int counter;


        public TextExportProvider(System.IO.TextWriter writer) : base(writer) 
        {
            counters = new System.Collections.Generic.Stack<int>();
            counter = 1;
        }


        public override void BeginExport()
        { }


        public override void WriteKeyStart(string key)
        {
            Writer.WriteLine("Key Name:\t{0}", key);            
            counters.Push(counter);
            counter = 1;
        }


        public override void WriteKeyEnd()
        {
            counter = counters.Pop();
            Writer.WriteLine();
        }


        public override void WriteValue(string name, Microsoft.Win32.RegistryValueKind kind, object data)
        {
            Writer.WriteLine("Value {0}", counter++);
            Writer.WriteLine("    Name:\t{0}", name);
            Writer.WriteLine("    Type:\t{0}", RegistryExplorer.Registry.Extensions.ToDataType(kind) );
            Writer.WriteLine("    Data:\t{0}", RegistryExplorer.Registry.RegValue.ToString(kind, data));
            Writer.WriteLine();
        }


        public override void EndExport()
        {
            Writer.WriteLine();
        }


    }


}
