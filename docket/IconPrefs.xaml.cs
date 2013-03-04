using System.Windows;

namespace docket
{
    public partial class IconPrefs : Window
    {

        public IconPrefs(IconItem item)
        {
            InitializeComponent();
            DataContext = item.Prefs;
        }
    }
}
