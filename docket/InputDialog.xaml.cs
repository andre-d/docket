using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace docket
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(String Title, String Message, String DefaultText)
        {
            this.MsgTitle = Title;
            this.Message = Message;
            InitializeComponent();
            DataContext = this;
            this.Response = DefaultText;
        }

        public String MsgTitle { get; set; }
        public String Message { get; set; }
        public String Response {
            get { return TextBoxArea.Text; }
            set { TextBoxArea.Text = value; }
        }

        private void DialogLoadedEvent(object sender, RoutedEventArgs e)
        {
            TextBoxArea.CaretIndex = TextBoxArea.Text.Count();
            Keyboard.Focus(TextBoxArea);
        }
    }
}
