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
    /// Interaction logic for APIWindow.xaml
    /// </summary>
    public partial class APIWindow : Window
    {
        private MainWindow mainWindow;

        public APIWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            typeBox.Items.Add(new ComboBoxItem
            {
                Uid = Constants.ResultsAPI.CHRONOKEEP,
                Content = "Chronokeep"
            });
            typeBox.Items.Add(new ComboBoxItem
            {
                Uid = Constants.ResultsAPI.CHRONOKEEP_SELF,
                Content = "Chronokeep (Self Hosted)"
            });
            typeBox.SelectedIndex = 0;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.AddAPI(((ComboBoxItem)typeBox.SelectedItem).Uid, urlBox.Text, nicknameBox.Text, keyBox.Text);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)typeBox.SelectedItem).Uid == Constants.ResultsAPI.CHRONOKEEP)
            {
                urlBox.Text = Constants.ResultsAPI.CHRONOKEEP_URL;
                urlBox.IsEnabled = false;
            }
            else
            {
                urlBox.Text = "";
                urlBox.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.APIClosing();
        }
    }
}
