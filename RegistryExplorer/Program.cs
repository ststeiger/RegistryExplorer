
namespace RegistryExplorer
{


    static class Program
    {


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [System.STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            // System.Windows.Forms.Application.Run(new Form1());
            System.Windows.Forms.Application.Run(new RegistryExplorer.MainForm());
        }


    }


}
