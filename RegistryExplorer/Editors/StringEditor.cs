
namespace RegistryExplorer.Editors
{


    partial class StringEditor : ValueEditor
    {


        public StringEditor(RegistryExplorer.Registry.RegValue value): base(value)
        {
            InitializeComponent();
            txtData.Text = value.Data.ToString();
        }


        private void btnOK_Click(object sender, System.EventArgs e)
        {
            SaveValue(txtData.Text);
        }


    }


}
