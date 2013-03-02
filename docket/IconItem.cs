using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Image = System.Windows.Controls.Image;

namespace docket
{
    public class IconItem : StackPanel
    {
        private const double Clickopacity = .5;
        private const double Hoveropacity = .75;

        private bool _mouseClickedDown;
        private readonly string _statusText;
        public System.Windows.Controls.Image IconImage;

        public bool ShouldRunAsAdmin { get; set; }
        public string Arguments { get; set; }
        public string Path { get; set;  }
        public string RunIn { get; set; }

        static IconItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (IconItem), new FrameworkPropertyMetadata(typeof (IconItem)));
        }

        public IconItem(Icon icon, string path)
        {
            RunIn = "";
            Arguments = "";
            Path = path;

            IconImage = new Image();
            _statusText = System.IO.Path.GetFileNameWithoutExtension(path);
            IconImage.Source = Utils.ConvertBitmapToBitmapImage(Utils.TrimBitmap(icon.ToBitmap()));
            IconImage.Margin = new Thickness(5, 5, 5, 5);
            IconImage.Drop += FileDropped;
            IconImage.MouseUp += IconClicked;
            IconImage.MouseDown += IconClickedDown;
            IconImage.IsMouseDirectlyOverChanged += MouseOverEvent;
            IconImage.DragEnter += HoverIn;
            IconImage.DragLeave += HoverOut;
            IconImage.Width = 60;
            Effect = new DropShadowEffect();
            RenderOptions.SetBitmapScalingMode(IconImage, BitmapScalingMode.HighQuality);
            Children.Add(IconImage);
            ContextMenu = new ContextMenu();
            var prefsItem = new MenuItem {Header = "Icon Preferences"};
            prefsItem.Click += PrefsItemOnClick;
            var removeItem = new MenuItem {Header = "Remove"};
            removeItem.Click += RemoveItemOnClick;
            ContextMenu.Items.Add(prefsItem);
            ContextMenu.Items.Add(removeItem);
        }

        private void RemoveItemOnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null) mainWindow.RemoveItem(this);
        }

        private void PrefsItemOnClick(object sender, RoutedEventArgs e)
        {
            (new IconPrefs(this)).ShowDialog();
        }

        private void HoverOut(object sender, DragEventArgs e)
        {
            HoverHandle(false);
        }

        private void HoverIn(object sender, DragEventArgs e)
        {
            HoverHandle(true);
        }

        private void HoverHandle(bool v)
        {
            IconImage.Opacity = v ? Hoveropacity : 1;
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.SetLabel(v ? _statusText : null);
            }
        }

        private void MouseOverEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            HoverHandle((bool) e.NewValue);
        }

        private void IconClickedDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                IconImage.Opacity = Clickopacity;
                _mouseClickedDown = true;
                e.Handled = true;
            }
        }

        private void IconClicked(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _mouseClickedDown)
            {
                IconImage.Opacity = Hoveropacity;
                e.Handled = true;
                _mouseClickedDown = false;
                Execute();
            }
        }

        private void Execute(String args = null)
        {
            var startInfo = new ProcessStartInfo {FileName = Path};
            if (args != null)
            {
                startInfo.Arguments = args;
            }
            if (ShouldRunAsAdmin)
            {
                startInfo.Verb = "runas";
            }
            if (RunIn.Length > 0)
            {
                startInfo.WorkingDirectory = @RunIn;
            }
            try
            {
                Process.Start(startInfo);
            }
            catch (Win32Exception)
            {}
        }

        private void FileDropped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                foreach (var filePath in (paths ?? Enumerable.Empty<string>()))
                {
                    var arg = Arguments;
                    if (!arg.Contains("{0}"))
                    {
                        arg += " {0}";
                    }
                    Execute(String.Format(arg, String.Format("\"{0}\"", @filePath)));
                }
                e.Handled = true;
            }
        }

    }
}
