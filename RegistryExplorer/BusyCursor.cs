
namespace RegistryExplorer
{


    class BusyCursor
        : System.IDisposable
    {
        System.Windows.Forms.Form source;

        public BusyCursor(System.Windows.Forms.Form source)
        {
            this.source = source;
            source.Cursor = System.Windows.Forms.Cursors.WaitCursor;
        }

        public void Dispose()
        {
            source.Cursor = System.Windows.Forms.Cursors.Default;
        }

    }


}
