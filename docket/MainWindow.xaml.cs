using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private bool IsHideSafe
        {
            get { return _isHideSafe; }
            set {
            _showhideStopWatch.Restart();
            _isHideSafe = value;
        } }

        private bool _hidden;
        private readonly System.Timers.Timer _timer;
        private readonly Stopwatch _showhideStopWatch;
        private readonly Stopwatch _tabSwitchStopwatch;

        readonly bool _doneLoading;
        private Screen _monitorSelection;
        public Screen[] MonitorList
        {
            get { return Screen.AllScreens; }
        }

        private int _iconHeight;
        public int IconHeight { get { return _iconHeight; } set { _iconHeight = value; SerializeToXml(); } }

        private int? _hideDelay;
        public int HideDelay { get { return _hideDelay ?? 500; }
            set { _hideDelay = value; SerializeToXml(); }
        }

        private int? _hideEaseTime;
        public int HideEaseTime
        {
            get { return _hideEaseTime ?? 250; }
            set { _hideEaseTime = value; SerializeToXml(); }
        }

        private int? _showEaseTime;
        public int ShowEaseTime
        {
            get { return _showEaseTime ?? 250; }
            set { _showEaseTime = value; SerializeToXml(); }
        }

        private int? _showDelay;
        public int ShowDelay
        {
            get { return _showDelay ?? 250; }
            set { _showDelay = value; SerializeToXml(); }
        }

        private bool? _autoHide;
        public bool AutoHide
        {
            get { return _autoHide ?? true; }
            set { _autoHide = value; SerializeToXml(); }
        }

        private bool? _autoHideWholeWidth;
        public bool AutoHideWholeWidth
        {
            get { return _autoHideWholeWidth ?? true; }
            set
            {
                _autoHideWholeWidth = value;
                SerializeToXml();
            }
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
            Width = screenWidth;
            Top = currentScreen.WorkingArea.Top;
            Left = currentScreen.WorkingArea.Left;
        }

        public void RemoveItem(IconItem item)
        {
            SetLabel(null);
            var stackPanel = item.Parent as StackPanel;
            if (stackPanel != null) stackPanel.Children.Remove(item);
            SerializeToXml();
            IsHideSafe = true;
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
                    IsHideSafe = true;
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
            tabItem.DragLeave += TabItemOnDragLeave;
            tabItem.AllowDrop = true;
            SerializeToXml();
        }

        private void TabItemOnDragLeave(object sender, DragEventArgs dragEventArgs)
        {
            _tabSwitchStopwatch.Stop();
        }

        private void TabItemOnContextMenuOpening(object sender, ContextMenuEventArgs contextMenuEventArgs)
        {
            IsHideSafe = false;
        }

        private void TabItemOnContextMenuClosing(object sender, ContextMenuEventArgs contextMenuEventArgs)
        {
            IsHideSafe = true;
        }

        private void TabItemOnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                if (!_tabSwitchStopwatch.IsRunning)
                {
                    _tabSwitchStopwatch.Reset();
                    _tabSwitchStopwatch.Start();
                }
                else if (_tabSwitchStopwatch.ElapsedMilliseconds > 250)
                {
                    IconTabs.SelectedItem = sender;
                    SerializeToXml();
                }
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
            textReader.Close();
            if (prefs == null)
            {
                return;
            }
            _showDelay = prefs.ShowDelay;
            _showEaseTime = prefs.ShowEaseTime;
            _hideDelay = prefs.HideDelay;
            _hideEaseTime = prefs.HideEaseTime;
            _autoHide = prefs.AutoHide;
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
            dockPrefs.ShowDelay = ShowDelay;
            dockPrefs.HideDelay = HideDelay;
            dockPrefs.HideEaseTime = HideEaseTime;
            dockPrefs.ShowEaseTime = ShowEaseTime;
            dockPrefs.AutoHide = AutoHide;
            dockPrefs.AutoHideWholeWidth = AutoHideWholeWidth;
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
            _hidden = true;
            var anim = new DoubleAnimation(0, -IconTabs.ActualHeight, TimeSpan.FromMilliseconds(HideEaseTime)) { EasingFunction = new SineEase() };
            IconTabs.RenderTransform = new TranslateTransform();
            IconTabs.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);
            anim.Completed += (sender, args) => { Visibility = Visibility.Hidden; };
        }

        private void _show()
        {
            _hidden = false;
            Visibility = Visibility.Visible;
            var anim = new DoubleAnimation(-IconTabs.ActualHeight, 0, TimeSpan.FromMilliseconds(ShowEaseTime)) { EasingFunction = new SineEase() };
            IconTabs.RenderTransform =  new TranslateTransform();
            IconTabs.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);
        }

        private void __CheckAutoHide()
        {
            if (!AutoHide && _hidden)
            {
                _show();
                return;
            }
            if (!AutoHide || (!_hidden && !IsHideSafe))
            {
                return;
            }
            var pos = Win32.GetMousePosition();
            var p = IconTabs.TransformToVisual(this).Transform(new Point());
            var windowRectangle = new System.Drawing.Rectangle(
            (int)p.X, (int)Top,
            (int)IconTabs.ActualWidth, (int)((_hidden) ? 5 : ActualHeight));
            if (AutoHideWholeWidth)
            {
                windowRectangle.X = (int)Left;
                windowRectangle.Width = (int)ActualWidth;
            }
            if (!windowRectangle.Contains(pos.X, pos.Y))
            {
                if (!_hidden)
                {
                    if (_showhideStopWatch.ElapsedMilliseconds > HideDelay)
                    {

                        _hide();
                    }
                }
                else
                {
                    _showhideStopWatch.Restart();
                }
            }
            else
            {
                if (_hidden)
                {
                    if (_showhideStopWatch.ElapsedMilliseconds > ShowDelay)
                    {
                        _show();
                    }
                }
                else
                {
                    _showhideStopWatch.Restart();
                }
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
            LabelContainer.UseLayoutRounding = true;
            _tabSwitchStopwatch = new Stopwatch();
            _showhideStopWatch = new Stopwatch();
            IsHideSafe = true;
            _showhideStopWatch.Start();
            _timer = new System.Timers.Timer {Interval = 15};
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
