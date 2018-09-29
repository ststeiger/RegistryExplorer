
using System.Windows.Forms;


namespace RegistryExplorer
{


    partial class RemoveFavoritesDialog : Form
    {
        RegistryExplorer.Collections.EventDictionary<string, string> favorites;


        public RemoveFavoritesDialog(RegistryExplorer.Collections.EventDictionary<string, string> favorites)
        {
            InitializeComponent();
            this.favorites = favorites;
        }


        private void RemoveFavoritesDialog_Load(object sender, System.EventArgs e)
        {
            foreach (string item in favorites.Keys)
                lstKeys.Items.Add(item);
        }


        private void btOK_Click(object sender, System.EventArgs e)
        {
            RegistryExplorer.Registry.RegKey regKey = RegistryExplorer.Registry.RegKey.Parse(
                RegistryExplorer.Registry.RegistryExplorerr.RegistryFavoritePath, true
            );

            foreach (object item in lstKeys.SelectedItems)
            {
                string key = item.ToString();
                regKey.Key.DeleteValue(key);
                favorites.Remove(key);
            }

        }


    }


}
