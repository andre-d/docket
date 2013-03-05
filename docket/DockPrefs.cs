using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace docket
{
    [XmlRootAttribute("DockPrefs")]
    public class DockPrefs
    {
        public int IconHeight { get; set; }

        public bool? AutoHide
        {
            get;
            set;
        }

        public bool? AutoHideWholeWidth
        {
            get;
            set;
        }

        public int? HideDelay
        {
            get;
            set;
        }

        public int? ShowDelay
        {
            get;
            set;
        }

        public int? ShowEaseTime
        {
            get;
            set;
        }

        public int? HideEaseTime
        {
            get;
            set;
        }

        public int SelectedTab
        {
            get;
            set;
        }

        public int MonitorNumber
        {
            get;
            set;
        }
        
        public List<string> Tabs;
        public List<List<IconItemPrefs>> TabItems;

        public void AddTab(string header)
        {
            if (!Tabs.Contains(header))
            {
                Tabs.Add(header);
                TabItems.Add(new List<IconItemPrefs>());
            }
        }

        public void AddItem(string tab, IconItemPrefs item)
        {
            TabItems[TabItems.Count - 1].Add(item);
        }

        public DockPrefs()
        {
            Tabs = new List<string>();
            TabItems = new List<List<IconItemPrefs>>();
        }
    }
}
