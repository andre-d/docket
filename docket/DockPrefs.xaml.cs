using System.Windows;

namespace docket
{
    public partial class DockPrefs : Window
    {

        public DockPrefs(MainWindow window)
        {
            InitializeComponent();
            DataContext = window;
        }
    }
}
