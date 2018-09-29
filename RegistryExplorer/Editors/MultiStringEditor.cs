
namespace RegistryExplorer.Editors
{


    partial class MultiStringEditor : ValueEditor
    {


        public MultiStringEditor(RegistryExplorer.Registry.RegValue value)
            : base(value)
        {
            InitializeComponent();
            txtData.Text = string.Join("\r\n",((string[])value.Data));
        }


        private void btnOK_Click(object sender, System.EventArgs e)
        {
            SaveValue(txtData.Text.Split(new string[]{"\r\n"}, System.StringSplitOptions.RemoveEmptyEntries));
        }


    }


}
