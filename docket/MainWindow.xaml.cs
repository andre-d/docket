using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml.Serialization;
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
        private bool _isHideSafe;
        private System.Timers.Timer _timer;
        readonly bool _doneLoading;
        private Screen _monitorSelection;
        public Screen[] MonitorList
        {
            get { return Screen.AllScreens; }
        }

        public int IconHeight { get; set; }

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
                SerializeToXml();
                Dock();
            }
        }

        private void AddIcon(string path)
        {
            AddIcon(new IconItem(path));
        }

        private void AddIcon(IconItemPrefs prefs)
        {
            AddIcon(new IconItem(prefs));
        }

        private void AddIcon(IconItem iconItem)
        {
            var stackPanel = IconTabs.SelectedContent as StackPanel;
            if (stackPanel != null)
                stackPanel.Children.Add(iconItem);
            SerializeToXml();
        }

        private void Dock()
        {
            var currentScreen = Monitor;
            var screenWidth = currentScreen.WorkingArea.Width;
            var windowWidth = ActualWidth;
            Left = currentScreen.WorkingArea.X + (screenWidth / 2.0 - (windowWidth / 2.0));
            Top = currentScreen.WorkingArea.Y;
        }

        public void RemoveItem(IconItem item)
        {
            SetLabel(null);
            var stackPanel = item.Parent as StackPanel;
            if (stackPanel != null) stackPanel.Children.Remove(item);
            SerializeToXml();
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
                    SerializeToXml();
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
                    SerializeToXml();
                    _isHideSafe = true;
                };
            quitItem.Header = "Quit";
            quitItem.Click += QuitItemOnClick;
            tabItem.ContextMenu.Items.Add(newTabItem);
            tabItem.ContextMenu.Items.Add(renameTabItem);
            tabItem.ContextMenu.Items.Add(removeTabItem);
            tabItem.ContextMenu.Items.Add(new Separator());
            tabItem.ContextMenu.Items.Add(prefsItem);
            tabItem.ContextMenu.Items.Add(quitItem);
            tabItem.ContextMenuClosing += TabItemOnContextMenuClosing;
            tabItem.ContextMenuOpening += TabItemOnContextMenuOpening;
            tabItem.DragOver += TabItemOnDragOver;
            tabItem.AllowDrop = true;
            SerializeToXml();
        }

        private void TabItemOnContextMenuOpening(object sender, ContextMenuEventArgs contextMenuEventArgs)
        {
            _isHideSafe = false;
        }

        private void TabItemOnContextMenuClosing(object sender, ContextMenuEventArgs contextMenuEventArgs)
        {
            _isHideSafe = true;
        }

        private void TabItemOnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                IconTabs.SelectedItem = sender;
                SerializeToXml();
                e.Handled = true;
            }
        }

        public static void Save()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null) mainWindow.SerializeToXml();
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

        private void LoadFromXml()
        {
            var deserializer = new XmlSerializer(typeof(DockPrefs));
            TextReader textReader = new StreamReader(@"docket.xml");
            var prefs = deserializer.Deserialize(textReader) as DockPrefs;
            if (prefs == null)
            {
                return;
            }
            textReader.Close();
            if (prefs.MonitorNumber < MonitorList.Count())
            {
                Monitor = MonitorList[prefs.MonitorNumber];
            }
            if (prefs.IconHeight > 0)
            {
                IconHeight = prefs.IconHeight;
            }
            IconTabs.SelectedIndex = 0;
            foreach (var tab in prefs.Tabs)
            {
                AddTab(tab);
                foreach (var iconItemPrefs in prefs.TabItems[IconTabs.SelectedIndex])
                {
                    AddIcon(iconItemPrefs);
                }
                IconTabs.SelectedIndex++;
            }
            IconTabs.SelectedIndex = prefs.SelectedTab;
        }

        public void SerializeToXml()
        {
            if (!_doneLoading)
            {
                return;
            }
            var serializer = new XmlSerializer(typeof(DockPrefs));
            TextWriter textWriter = new StreamWriter(@"docket.xml", false);
            var dockPrefs = new DockPrefs { MonitorNumber = Array.IndexOf(MonitorList, Monitor), SelectedTab = IconTabs.SelectedIndex, IconHeight = IconHeight};
            foreach (TabItem tabItem in IconTabs.Items)
            {
                var header = tabItem.Header;
                var stackPanel = tabItem.Content as StackPanel;
                if (stackPanel != null)
                {
                    dockPrefs.AddTab(header.ToString());
                    foreach (var item in stackPanel.Children.OfType<IconItem>())
                    {
                        dockPrefs.AddItem(header.ToString(), item.Prefs);
                    }
                }
            }
            serializer.Serialize(textWriter, dockPrefs);
            textWriter.Close();
        }

        private void DocketPrefsItemOnClick(object sender, RoutedEventArgs e)
        {
            (new DockPrefsDialog(this)).ShowDialog();
        }

        private void QuitItemOnClick(object sender, RoutedEventArgs e)
        {
            SerializeToXml();
            Application.Current.Shutdown();
        }

        private void _hide()
        {
            Visibility = Visibility.Hidden;
        }

        private void _show()
        {
            Visibility = Visibility.Visible;
        }

        private void __CheckAutoHide()
        {
            if (!_isHideSafe)
            {
                return;
            }
            var pos = Win32.GetMousePosition();
            var windowRectangle = new System.Drawing.Rectangle(
            (int)(Left - (ActualWidth / 2.0)), (int)Top,
            (int)(ActualWidth * 2), (int)((Visibility == Visibility.Hidden) ? 2 : (ActualHeight * 1.5)));

            if (!windowRectangle.Contains(pos.X, pos.Y))
            {
                if(Visibility != Visibility.Hidden)
                    _hide();
            }
            else
            {
                if (Visibility == Visibility.Hidden)
                    _show();
            }
           
        }

        private void _CheckAutoHide(object sender, EventArgs eventArgs)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(__CheckAutoHide));
        }

        public MainWindow()
        {
            InitializeComponent();
            UseLayoutRounding = true;
            _isHideSafe = true;
            _timer = new System.Timers.Timer();
            _timer.Interval = 50;
            _timer.Elapsed += _CheckAutoHide;
            _timer.Enabled = true;
            ShowInTaskbar = false;
            IconHeight = 32;
            DataContext = this;
            try
            {
                LoadFromXml();
            }catch(FileNotFoundException)
            {
            }
            _doneLoading = true;
            if (IconTabs.Items.Count <= 0)
            {
                AddTab("Apps");
                IconTabs.SelectedIndex = 0;
            }
            SerializeToXml();
            Dock();

        }

        private void Reworked(object sender, EventArgs e)
        {
            Dock();
        }

        private void IconTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SerializeToXml();
        }

        
    }
}
