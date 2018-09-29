
namespace RegistryExplorer.Export
{


    class NativeExportProvider
        : ExportProvider
    {
        public NativeExportProvider(System.IO.TextWriter writer) 
            : base(writer)
        { }


        public override void BeginExport()
        {
            Writer.WriteLine("Windows Registry Editor Version 5.00");
        }


        public override void WriteKeyStart(string key)
        {
            Writer.WriteLine();
            Writer.WriteLine(string.Format("[{0}]", key));
        }

        public override void WriteKeyEnd() { }


        public override void WriteValue(string name, Microsoft.Win32.RegistryValueKind kind, object data)
        {
            string dataString;
            switch (kind)
            {
                case Microsoft.Win32.RegistryValueKind.Binary:
                    dataString = string.Format("hex:{0}", GetHexString((byte[])data));
                    break;
                case Microsoft.Win32.RegistryValueKind.DWord:
                    dataString = string.Format("dword:{0:x8}", (uint)((int)data));
                    break;
                case Microsoft.Win32.RegistryValueKind.QWord:
                    dataString = string.Format("qword:{0:x16}", (ulong)((long)data));
                    break;
                case Microsoft.Win32.RegistryValueKind.ExpandString:
                    dataString = string.Format("hex(2):{0}", GetHexString((string)data));
                    break;
                case Microsoft.Win32.RegistryValueKind.MultiString:
                    dataString = string.Format("hex(7):{0}", GetHexString((string[])data));
                    break;
                case Microsoft.Win32.RegistryValueKind.String:
                    dataString = string.Format("\"{0}\"", (string)data);
                    break;
                case Microsoft.Win32.RegistryValueKind.Unknown:
                default:
                    dataString = string.Empty;
                    break;
            }
            Writer.WriteLine("\"{0}\"={1}", name, dataString);
        }


        private string GetHexString(string[] data)
        {
            if (data.Length == 0)
                return string.Empty;
            System.Text.StringBuilder output = new System.Text.StringBuilder(data.Length * 10);
            System.Array.ForEach<string>(data, str => output.Append(GetHexString(str)).Append(','));
            output.Append("00,00");
            return output.ToString();
        }


        private string GetHexString(string data)
        {
            if (data.Length == 0)
                return string.Empty;
            /* length is calculated as follows
             * 4 char for each character (two 2 digit hex nums)
             * 1 char seperator ','
             * 5 char postfix (null termination 00,00)
             * 1 char string terminator */
            int length = (data.Length * 4) + data.Length * 2 + 5 + 1;
            System.Text.StringBuilder output = new System.Text.StringBuilder(length);
            const string format = "{0:x2},{1:x2},";
            System.Array.ForEach<char>(data.ToCharArray(), chr => output.Append(string.Format(format, (byte)chr, (byte)chr >> 8)));
            output.Append("00,00");
            return output.ToString(0, output.Length);
        }


        private string GetHexString(byte[] data)
        {
            if (data.Length == 0)
                return string.Empty;
            //------slow method----------
            //string[] output = Array.ConvertAll<byte, string>(data, byt => string.Format("{0:x2}", byt));
            //return String.Join(",", output);

            //-------fast method---------
            /* length is calculated as follows
             * 2 char for each byte
             * 1 char seperator ','
             * 1 char string terminator */
            int length = (data.Length * 2) + data.Length + 1;
            System.Text.StringBuilder output = new System.Text.StringBuilder(length);
            const string format = "{0:x2},";
            System.Array.ForEach<byte>(data, byt => output.Append(string.Format(format, byt)));
            return output.ToString(0, output.Length - 1);
        }


        public override void EndExport()
        {
            Writer.WriteLine();
        }


    }


}
