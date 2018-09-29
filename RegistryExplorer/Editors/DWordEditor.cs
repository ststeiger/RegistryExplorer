
namespace RegistryExplorer.Editors
{
    partial class DWordEditor : ValueEditor
    {
        public DWordEditor(RegistryExplorer.Registry.RegValue value): base(value)
        {
            InitializeComponent();
            string data;
            if (value.Kind == Microsoft.Win32.RegistryValueKind.DWord)
                data = ((int)value.Data).ToString("x");
            else
                data = ((long)value.Data).ToString("x");
        }

        private void base_CheckedChanged(object sender, System.EventArgs e)
        {            
            txtData.HexNumber = rdoHex.Checked;            
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            if (Value.Kind == Microsoft.Win32.RegistryValueKind.DWord)
                SaveValue((int)txtData.UIntValue);
            else
                SaveValue((long)txtData.ULongValue);
        }
    }
}
