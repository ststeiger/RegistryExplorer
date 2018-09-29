
namespace RegistryExplorer.Registry
{


    class RegValue : System.IComparable<RegValue>
    {
        public Microsoft.Win32.RegistryKey ParentKey { get; private set; }
        public Microsoft.Win32.RegistryValueKind Kind { get; set; }
        string name;
        public object Data { get; set; }


        public string Name
        {
            get
            {
                if (IsDefault)
                    return "(Default)";
                else
                    return name;
            }
            set { name = value; }
        }

        

        public bool IsDefault
        {
            get { return name == string.Empty; }
        }

        

        public RegValue(string name, Microsoft.Win32.RegistryValueKind kind, object data)
        {
            this.name = name;
            Kind = kind;
            Data = data;
        }


        public RegValue(Microsoft.Win32.RegistryKey parentKey, string valueName) :
            this(valueName, parentKey.GetValueKind(valueName), parentKey.GetValue(valueName))
        {
            ParentKey = parentKey;
        }


        public override string ToString()
        {
            return ToString(Kind, Data);
        }


        public static string ToString(object valueData)
        {
            if (valueData is byte[])
                return System.Text.Encoding.ASCII.GetString((byte[])valueData);
            else
                return valueData.ToString();
        }


        public static string ToString(Microsoft.Win32.RegistryValueKind valueKind, object valueData)
        {
            string data;
            switch (valueKind)
            {
                case Microsoft.Win32.RegistryValueKind.Binary:
                    data = System.Text.Encoding.ASCII.GetString((byte[])valueData);
                    break;
                case Microsoft.Win32.RegistryValueKind.MultiString:
                    data = string.Join(" ", (string[])valueData);
                    break;
                case Microsoft.Win32.RegistryValueKind.DWord:
                    data = ((uint)((int)valueData)).ToString();
                    break;
                case Microsoft.Win32.RegistryValueKind.QWord:
                    data = ((ulong)((long)valueData)).ToString();
                    break;
                case Microsoft.Win32.RegistryValueKind.String:
                case Microsoft.Win32.RegistryValueKind.ExpandString:
                    data = valueData.ToString();
                    break;
                case Microsoft.Win32.RegistryValueKind.Unknown:
                default:
                    data = string.Empty;
                    break;
            }
            return data;
        }


        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }


        /* Implemented the IComparable interface
         * because Sort method of List<T> calls the CompareTo function
         * of the objects to compare them and I want the objects
         * of RegValue type to be sorted on basis of value name */
        public int CompareTo(RegValue other)
        {
            return Name.CompareTo(other.Name);
        }


    }


}
