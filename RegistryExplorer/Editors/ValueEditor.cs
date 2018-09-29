using System.Windows.Forms;


namespace RegistryExplorer.Editors
{    
    partial class ValueEditor : Form
    {
        protected RegistryExplorer.Registry.RegValue Value { get; private set; }

        private ValueEditor()
        {
            InitializeComponent();
        }

        public ValueEditor(RegistryExplorer.Registry.RegValue value):this()
        {            
            Value = value;
            txtName.Text = value.Name;
            txtName.Modified = false;
        }

        protected void SaveValue(object data)
        {
            Microsoft.Win32.Registry.SetValue(Value.ParentKey.Name, Value.Name, data, Value.Kind);
        }
    }
}
