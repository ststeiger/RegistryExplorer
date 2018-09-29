
using System.Windows.Forms;


namespace RegistryExplorer
{


    partial class AddToFavoritesDialog : Form
    {
        public AddToFavoritesDialog()
        {
            InitializeComponent();
        }

        private void txtName_TextChanged(object sender, System.EventArgs e)
        {
            btnOK.Enabled = (txtName.TextLength > 0);
        }
    }


}
