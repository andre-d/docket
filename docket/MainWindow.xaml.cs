using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;

namespace docket
{
    public partial class MainWindow
    {
        private Screen _monitorSelection;
        public Screen[] MonitorList
        {
            get { return Screen.AllScreens; }
        }

        public void SetLabel(String labelText)
        {
            if (labelText == null)
            {
                StatusLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                StatusLabel.Visibility = Visibility.Visible;
                StatusLabel.Content = labelText;
            }
        }

        public Screen Monitor
        {
            get
            {
                if (Array.IndexOf(MonitorList, _monitorSelection) < 0)
                {
                    _monitorSelection = null;
                }
                return _monitorSelection ?? Screen.PrimaryScreen;
            }
            set
            {
                _monitorSelection = value;
                Dock();
            }
        }

        private void AddIcon(string path)
        {
            var icon = Utils.GetShellIcon(@path);
            if (icon == null)
            {
                return;
            }
            var stackPanel = IconTabs.SelectedContent as StackPanel;
            if (stackPanel != null)
                stackPanel.Children.Add(new IconItem(icon, path));
        }

        private void Dock()
        {
            var currentScreen = Monitor;
            var screenWidth = currentScreen.WorkingArea.Width;
            var windowWidth = Width;
            Left = currentScreen.WorkingArea.X + (screenWidth / 2.0 - (windowWidth / 2.0));
            Top = currentScreen.WorkingArea.Y;
        }

        public void RemoveItem(IconItem item)
        {
            SetLabel(null);
            var stackPanel = item.Parent as StackPanel;
            if (stackPanel != null) stackPanel.Children.Remove(item);
        }

        public void AddTab(String title)
        {
            var tabItem = new TabItem {Header = title};
            var panel = new StackPanel();
            tabItem.Content = panel;
            IconTabs.Items.Add(tabItem);
            tabItem.ContextMenu = new ContextMenu();
            var prefsItem = new MenuItem {Header = "Preferences"};
            prefsItem.Click += DocketPrefsItemOnClick;
            var quitItem = new MenuItem {Header = "Quit"};
            var renameTabItem = new MenuItem {Header = "Rename Tab"};
            renameTabItem.Click += (sender, args) =>
                {
                    var dialog = new InputDialog("Rename Tab", "Rename tab to?", tabItem.Header as string);
                    dialog.ShowDialog();
                    if (dialog.Response.Any())
                    {
                        tabItem.Header = dialog.Response;
                    }
                };
            var newTabItem = new MenuItem {Header = "Create New Tab"};
            newTabItem.Click += (sender, e) =>
            {
                var dialog = new InputDialog("Create New Tab", "Tab to create?", "");
                dialog.ShowDialog();
                if (dialog.Response.Any())
                {
                    AddTab(dialog.Response);
                }
            };
            var removeTabItem = new MenuItem {Header = "Delete Tab"};
            removeTabItem.Click += (sender, e) =>
                {
                    if (IconTabs.Items.Count <= 1)
                    {
                        AddTab("Apps");
                    }
                    IconTabs.Items.Remove(tabItem);
                };
            quitItem.Header = "Quit";
            quitItem.Click += QuitItemOnClick;
            tabItem.ContextMenu.Items.Add(newTabItem);
            tabItem.ContextMenu.Items.Add(renameTabItem);
            tabItem.ContextMenu.Items.Add(removeTabItem);
            tabItem.ContextMenu.Items.Add(new Separator());
            tabItem.ContextMenu.Items.Add(prefsItem);
            tabItem.ContextMenu.Items.Add(quitItem);
            tabItem.DragOver += TabItemOnDragOver;
            tabItem.AllowDrop = true;
        }

        private void TabItemOnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                IconTabs.SelectedItem = sender;
                e.Handled = true;
            }
        }

        private void TabItemOnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (var filePath in (paths ?? Enumerable.Empty<string>()))
                {
                    AddIcon(@filePath);
                }
            }
        }

        private void DocketPrefsItemOnClick(object sender, RoutedEventArgs e)
        {
            (new DockPrefs(this)).ShowDialog();
        }

        private void QuitItemOnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            AddTab("Utils");
            AddTab("Games");
            IconTabs.SelectedIndex = 0;
            AddIcon(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "icon.png"));
            AddIcon(@"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe");
            IconTabs.SelectedIndex = 1;
            AddIcon(@"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe");
            AddIcon(@"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe");
            IconTabs.SelectedIndex = 0;
            Dock();
            AddIcon(@"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe");
        }

        private void Reworked(object sender, EventArgs e)
        {
            Dock();
        }

        
    }
}
