using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace results_uploader.UI
{
    /// <summary>
    /// Interaction logic for EventYearWindow.xaml
    /// </summary>
    public partial class EventYearWindow : Window
    {
        private MainWindow mainWindow;

        private string type;

        public EventYearWindow(MainWindow mainWindow, string type)
        {
            InitializeComponent();
            if (type == "EVENT")
            {
                EventPanel.Visibility = Visibility.Visible;
                YearPanel.Visibility = Visibility.Collapsed;
            }
            else if (type == "YEAR")
            {
                YearPanel.Visibility = Visibility.Visible;
                EventPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                Close();
            }
            this.mainWindow = mainWindow;
            this.type = type;
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.AddEvent(NameBox.Text, SlugBox.Text, contactBox.Text);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddYear_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.AddYear(YearBox.Text, Convert.ToDateTime(DateBox.Text).ToString("yyyy/MM/dd HH:mm:ss"));
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.EventYearClosing();
        }
    }
}
