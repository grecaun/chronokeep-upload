using Microsoft.Win32;
using results_uploader.Interfaces.IO;
using results_uploader.IO;
using results_uploader.Network.API;
using results_uploader.Objects;
using results_uploader.Objects.API;
using results_uploader.Objects.Helpers;
using results_uploader.UI;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace results_uploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<ResultsAPI> aPIs = new List<ResultsAPI>();
        private ResultsAPI api = null;
        private string slug = null;
        private string year = null;

        private APIWindow apiWindow;
        private EventYearWindow eventYearWindow;
        private ImportWindow importWindow;

        private const string apiPath = ".\\readers.txt";
        
        private GetEventsResponse events;
        private GetEventYearsResponse eventYears;
        private List<APIResult> results = new List<APIResult>();

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                if (File.Exists(apiPath))
                {
                    using (StreamReader sr = new StreamReader(apiPath))
                    {
                        while (sr.Peek() >= 0)
                        {
                            string reader = sr.ReadLine();
                            if (reader != null)
                            {
                                string[] parts = reader.Split(',');
                                if (parts.Length == 4)
                                {
                                    aPIs.Add(new ResultsAPI(parts[0], parts[1], parts[2], parts[3]));
                                }
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Log.E("MainWindow", string.Format("An exception occured while trying to open api file. {0}", ex.ToString()));
            }
            UpdateView();
        }

        public void AddAPI(string type, string url, string auth_token, string nickname)
        {
            if (type != Constants.ResultsAPI.CHRONOKEEP && type != Constants.ResultsAPI.CHRONOKEEP_SELF)
            {
                return;
            }
            aPIs.Add(new ResultsAPI(type, url.Replace(',', '_'), auth_token.Replace(',', ' '), nickname.Replace(',', ' ')));
            UpdateView();
        }

        public void APIClosing()
        {
            apiWindow = null;
        }

        public void EventYearClosing()
        {
            eventYearWindow = null;
        }

        public async void AddEvent(string name, string s, string contactEmail)
        {
            if (api == null)
            {
                return;
            }
            try
            {
                ModifyEventResponse response = await APIHandlers.AddEvent(api, new APIEvent
                {
                    Name = name,
                    Slug = s,
                    ContactEmail = contactEmail,
                    Website = "",
                    Image = "",
                    AccessRestricted = false,
                    Type = Constants.ResultsAPI.CHRONOKEEP_EVENT_TYPE_DISTANCE
                });
                if (response != null && response.Event != null)
                {
                    slug = response.Event.Slug;
                }
            }
            catch (APIException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            UpdateEvents(slug);
        }

        public async void AddYear(string y, string date)
        {
            if (api == null || slug == null)
            {
                return;
            }
            try
            {
                EventYearResponse response =  await APIHandlers.AddEventYear(api, slug, new APIEventYear
                {
                    Year = y,
                    DateTime = date,
                    Live = false
                });
                if (response != null && response.EventYear != null)
                {
                    year = response.EventYear.Year;
                }
            }
            catch (APIException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            UpdateYears(year);
        }

        private void UpdateView()
        {
            APIBox.Items.Clear();
            APIBox.Items.Add(new ComboBoxItem
            {
                Uid = "-1",
                Content = ""
            });
            APIBox.SelectedIndex = 0;
            for (int i = 0; i < aPIs.Count; i++)
            {
                APIBox.Items.Add(new ComboBoxItem
                {
                    Uid = i.ToString(),
                    Content = aPIs[i].Nickname
                });
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            apiWindow = new APIWindow(this);
            apiWindow.Show();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = int.Parse(((ComboBoxItem)APIBox.SelectedItem).Uid);
                if (index >= 0)
                {
                    aPIs.RemoveAt(index);
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                Log.E("MainWindow", string.Format("An exception occured while trying to remove known api.", ex.ToString()));
            }
        }

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Log.D("MainWindow", "Choose file clicked.");
            OpenFileDialog bib_dialog = new OpenFileDialog() { Filter = "CSV Files (*.csv,*.txt)|*.csv;*.txt|All files|*" };
            if (bib_dialog.ShowDialog() == true)
            {
                IDataImporter importer = new CSVImporter(bib_dialog.FileName);
                importer.FetchHeaders();
                importWindow = new ImportWindow(importer, this);
                importWindow.Show();
            }
        }

        public void ImportFinalize(List<APIResult> results)
        {
            Log.D("Main Window", "Import finalized. Results count: " + results.Count);
            this.results = results;
            updateListView.ItemsSource = this.results;
            importWindow = null;
            if (this.results.Count > 0)
            {
                updateScrollView.Visibility = Visibility.Visible;
                Upload.Visibility = Visibility.Visible;
                WindowFrame.Height = 490;
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            Log.D("MainWindow", "Upload clicked.");
            if (results.Count > 0)
            {
                // do stuff
            }
            else
            {
                
            }
        }

        private void EventBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Log.D("MainWindow", "Event changed.");
            if (EventBox.SelectedItem == null)
            {
                return;
            }
            string uid = ((ComboBoxItem)EventBox.SelectedItem).Uid;
            if (uid == "NEW")
            {
                eventYearWindow = new EventYearWindow(this, "EVENT");
                eventYearWindow.Show();
            }
            else if (uid != "-1")
            {
                slug = uid;
                UpdateYears(null);
            }
        }

        private void YearBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Log.D("MainWindow", "Year changed.");
            if (YearBox.SelectedItem == null)
            {
                return;
            }
            string uid = ((ComboBoxItem)YearBox.SelectedItem).Uid;
            if (uid == "NEW")
            {
                eventYearWindow = new EventYearWindow(this, "YEAR");
                eventYearWindow.Show();
            }
        }

        private async void UpdateYears(string year)
        {
            if (api == null)
            {
                return;
            }
            YearBox.Items.Clear();
            YearBox.Items.Add(new ComboBoxItem
            {
                Uid = "-1",
                Content = ""
            });
            YearBox.Items.Add(new ComboBoxItem
            {
                Content = "New Year",
                Uid = "NEW"
            });
            int ix = 0;
            if (slug != null)
            {
                try
                {
                    eventYears = await APIHandlers.GetEventYears(api, slug);
                }
                catch (APIException ex)
                {
                    EventPanel.Visibility = Visibility.Collapsed;
                    WindowFrame.Height = 130;
                    MessageBox.Show(ex.Message);
                    return;
                }
                if (eventYears.EventYears != null)
                {
                    for (int i = 0; i < eventYears.EventYears.Count; i++)
                    {
                        YearBox.Items.Add(new ComboBoxItem
                        {
                            Uid = eventYears.EventYears[i].Year,
                            Content = eventYears.EventYears[i].Year
                        });
                        if (eventYears.EventYears[i].Year == year)
                        {
                            ix = i + 2;
                        }
                    }
                }
            }
            YearBox.SelectedIndex = ix;
        }

        private async void UpdateEvents(string slug)
        {
            if (api == null)
            {
                return;
            }
            try
            {
                events = await APIHandlers.GetEvents(api);
            }
            catch (APIException ex)
            {
                EventPanel.Visibility = Visibility.Collapsed;
                WindowFrame.Height = 130;
                MessageBox.Show(ex.Message);
                return;
            }
            EventPanel.Visibility = Visibility.Visible;
            WindowFrame.Height = 250;
            EventBox.Items.Clear();
            EventBox.Items.Add(new ComboBoxItem
            {
                Uid = "-1",
                Content = ""
            });
            EventBox.Items.Add(new ComboBoxItem
            {
                Content = "New Event",
                Uid = "NEW"
            });
            int ix = 0;
            if (events.Events != null)
            {
                for (int i = 0; i < events.Events.Count; i++)
                {
                    EventBox.Items.Add(new ComboBoxItem
                    {
                        Content = events.Events[i].Name,
                        Uid = events.Events[i].Slug
                    });
                    if (events.Events[i].Slug == slug)
                    {
                        ix = i + 2;
                    }
                }
            }
            EventBox.SelectedIndex = ix;
            UpdateYears(null);
        }

        private void APIBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("API clicked...");
            Log.D("MainWindow", "API changed.");
            if (APIBox.SelectedItem == null)
            {
                return;
            }
            int index = int.Parse(((ComboBoxItem)APIBox.SelectedItem).Uid);
            if (index > -1)
            {
                api = aPIs[index];
                UpdateEvents(null);
            }
            else
            {
                EventPanel.Visibility = Visibility.Collapsed;
                WindowFrame.Height = 130;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(apiPath, false))
                {
                    foreach (ResultsAPI api in aPIs)
                    {
                        sw.WriteLine(string.Format("{0},{1},{2},{3}", api.Type, api.URL, api.Nickname, api.AuthToken));
                    }
                };
            }
            catch (Exception ex)
            {
                Log.E("MainWindow", string.Format("An exception occured while trying to save api file. {0}", ex.ToString()));
            }
            try
            {
                if (apiWindow != null) apiWindow.Close();
            }
            catch { }
            try
            {
                if (eventYearWindow != null) eventYearWindow.Close();
            }
            catch { }
        }
    }
}
