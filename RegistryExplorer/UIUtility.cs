
namespace RegistryExplorer
{


    static class UIUtility
    {


        public static void InformUser(System.Windows.Forms.IWin32Window owner, string message)
        {
            System.Windows.Forms.MessageBox.Show(owner, message, 
                System.Windows.Forms.Application.ProductName, 
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
        }


        public static void DisplayError(System.Windows.Forms.IWin32Window owner, string message)
        {
            DisplayError(owner, message, null);
        }


        public static void DisplayError(System.Windows.Forms.IWin32Window owner
            , string message
            , System.Windows.Forms.Control source)
        {
            System.Windows.Forms.MessageBox.Show(owner, message, "Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);

            if (source != null)
            {
                if (source is System.Windows.Forms.TextBox)
                    ((System.Windows.Forms.TextBox)source).SelectAll();
                source.Focus();
            }
        }

        public static void WarnUser(System.Windows.Forms.IWin32Window owner, string message)
        {
            System.Windows.Forms.MessageBox.Show(owner, message,
                System.Windows.Forms.Application.ProductName,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }
        

        public static bool ConfirmAction(System.Windows.Forms.IWin32Window owner, string message, string action, bool critical)
        {
            System.Windows.Forms.MessageBoxIcon icon = critical ? 
                System.Windows.Forms.MessageBoxIcon.Warning : System.Windows.Forms.MessageBoxIcon.Question;

            return System.Windows.Forms.MessageBox.Show(owner, message, "Confirm " + action,
                System.Windows.Forms.MessageBoxButtons.YesNo, 
                icon,
                System.Windows.Forms.MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes;
        }


    }


}
