using System.Drawing;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace docket
{
    [XmlRootAttribute("IconItemPrefs")]
    public class IconItemPrefs
    {
        [XmlIgnore] private Icon _icon;

        [XmlIgnore]
        public Icon Icon
        {
            get { return _icon ?? (_icon = Utils.GetShellIcon(@Path)); }
            set { _icon = value; }
        }

        public IconItemPrefs(string path)
        {
            Path = path;
            Arguments = "";
            RunIn = "";
            Label = "";
        }

        public IconItemPrefs()
            : this("")
        {
        }

        private static void Save()
        {
            MainWindow.Save();
        }

        private bool _shouldRunAsAdmin;

        public bool ShouldRunAsAdmin
        {
            get { return _shouldRunAsAdmin; }
            set
            {
                _shouldRunAsAdmin = value;
                Save();
            }
        }

        private string _arguments;

        public string Arguments
        {
            get { return _arguments; }
            set
            {
                _arguments = value;
                Save();
            }
        }

        private string _path;

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                Save();
            }
        }

        private string _label;

        public string Label
        {
            get { return (_label.Any()) ? _label : System.IO.Path.GetFileNameWithoutExtension(Path); }
            set
            {
                _label = value;
                Save();
            }
        }

        private string _runin;

        public string RunIn
        {
            get { return _runin; }
            set
            {
                _runin = value;
                Save();
            }
        }
    }
}
