using System.Windows;

namespace docket
{
    public partial class DockPrefsDialog : Window
    {

        public DockPrefsDialog(MainWindow window)
        {
            InitializeComponent();
            DataContext = window;
        }
    }
}
