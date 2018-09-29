
using System.Windows.Forms;


namespace RegistryExplorer
{


    partial class ExportDialog : Form
    {


        public ExportDialog()
        {
            InitializeComponent();
        }


        private void btnExport_Click(object sender, System.EventArgs e)
        {
            RegistryExplorer.Registry.RegKey key;
            if ((key = RegistryExplorer.Registry.RegKey.Parse(cmbBranch.Text)) == null)
            {
                UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_InvalidKey, cmbBranch);
                return;
            }

            RegistryExplorer.Export.RegExportFormat format = GetExportFormat();


            System.IO.Stream output;
            try
            {
                output = System.IO.File.OpenWrite(txtFile.Text);
            }
            catch
            {
                UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_FileOpenFail, txtFile);
                return;
            }

            bool success = ExportToFile(key, format, output);

            if (!success)
                UIUtility.DisplayError(this, RegistryExplorer.Properties.Resources.Error_ExportFail);
            else
            {
                UIUtility.InformUser(this, RegistryExplorer.Properties.Resources.Info_ExportSuccess);
                this.Close();
            }
        }


        private bool ExportToFile(
            RegistryExplorer.Registry.RegKey key, 
            RegistryExplorer.Export.RegExportFormat format, 
            System.IO.Stream output
            )
        {
            bool success = true;
            using (output)
            {
                DisableControls();
                try
                {
                    using (new BusyCursor(this))
                    {
                        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(output))
                        {
                            RegistryExplorer.Export.RegExporter.Export(
                                  key
                                , RegistryExplorer.Export.ExportProvider.Create(format, writer)
                            );
                        }
                    }
                }
                catch
                {
                    success = false;
                }
                EnableControls();
            }

            return success;
        }


        private RegistryExplorer.Export.RegExportFormat GetExportFormat()
        {
            RegistryExplorer.Export.RegExportFormat format;
            if (saveFileDialog1.FilterIndex == 1)
                format = RegistryExplorer.Export.RegExportFormat.NativeRegFormat;
            else if (saveFileDialog1.FilterIndex == 2)
                format = RegistryExplorer.Export.RegExportFormat.XmlFormat;
            else
                format = RegistryExplorer.Export.RegExportFormat.TextFormat;

            return format;
        }


        private void EnableControls()
        {
            btnBrowse.Enabled =
                btnExport.Enabled =
                btnCancel.Enabled =
                cmbBranch.Enabled = true;
        }


        private void DisableControls()
        {
            btnBrowse.Enabled =
                btnExport.Enabled =
                btnCancel.Enabled =
                cmbBranch.Enabled = false;
        }


        private void cmbBranch_TextChanged(object sender, System.EventArgs e)
        {
             SetExportButtonState();
        }


        private void SetExportButtonState()
        {
            btnExport.Enabled = (cmbBranch.Text != string.Empty && txtFile.Text != string.Empty);
        }


        private void btnBrowse_Click(object sender, System.EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtFile.Text = saveFileDialog1.FileName;
            }
        }


        private void txtFile_TextChanged(object sender, System.EventArgs e)
        {
            SetExportButtonState();
        }


    }


}
