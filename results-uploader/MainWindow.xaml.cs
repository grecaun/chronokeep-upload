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
using System.Text.RegularExpressions;
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
        private List<Result> results = new List<Result>();

        private Regex ageGroupRegex = new Regex(@"[^\d]*(?<start>\d{1,2})[^\d]{0,2}(?<end>\d{1,2})");

        private readonly int UploadCount = 20;

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
                    this.year = response.EventYear.Year;
                }
            }
            catch (APIException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            UpdateYears(this.year);
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

        public void ImportFinalize(List<Result> results)
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

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            Log.D("MainWindow", "Upload clicked.");
            if (api == null || slug == null || year == null)
            {
                MessageBox.Show("Something went wrong and one or more variables is not set properly. Upload failed.");
                return;
            }
            if (results.Count > 0)
            {
                bool ageGroups = false;
                bool calcRankings = false;
                bool setBibs = false;
                Dictionary<int, string> ageGroupDict = new Dictionary<int, string>();
                // figure out what the age groups are
                // if unable to figure out what they are, specify them as 10 year increments
                foreach (Result result in results)
                {
                    // If age groups aren't set when uploaded, then we can set age groups.
                    if (result.AgeGroup == null || result.AgeGroup.Length < 1)
                    {
                        Log.D("MainWindow","Age Groups not set.");
                        ageGroups = true;
                    }
                    else
                    {
                        GroupCollection groups = ageGroupRegex.Match(result.AgeGroup).Groups;
                        int start = -1;
                        int end = -1;
                        if (int.TryParse(groups[start].Value, out start) && int.TryParse(groups[end].Value, out end))
                        {
                            for (int i = start; i <= end; i++)
                            {
                                ageGroupDict[i] = result.AgeGroup;
                            }
                        }
                    }
                    // If there is a ranking that is less than 1, and its not a DNF or DNS entry, then we need to calculate rankings
                    if (result.Ranking < 1 && result.Type != Constants.Timing.RESULT_TYPE_DNF && result.Type != Constants.Timing.RESULT_TYPE_DNS)
                    {
                        Log.D("MainWindow", "Rankings not set.");
                        calcRankings = true;
                    }
                    if (result.Bib == null || result.Bib.Length < 1)
                    {
                        Log.D("MainWindow", "Bibs need to be set.");
                        setBibs = true;
                    }
                }
                if (calcRankings && ageGroups)
                {
                    Log.D("MainWindow", "Creating default age groups.");
                    foreach ((int start, int end) in DefaultAgeGroups())
                    {
                        for (int i = start; i <= end; i++)
                        {
                            ageGroupDict[i] = string.Format("{0}-{1}", start, end);
                        }
                    }
                }
                // calculate age group places and places if necessary
                if (calcRankings)
                {
                    Log.D("MainWindow", "Calculating rankings.");
                    // (dist, gender, age)
                    Dictionary<(string, string, string), int> ageRankDict = new Dictionary<(string, string, string), int>();
                    // (dist, gender)
                    Dictionary<(string, string), int> genderDict = new Dictionary<(string, string), int>();
                    // dist
                    Dictionary<string, int> currentRank = new Dictionary<string, int>();
                    results.Sort(Result.CompareTo);
                    foreach (Result result in results)
                    {
                        // only set ranking if its a finish time and not dnf/dns
                        if (result.Finish && result.Type != Constants.Timing.RESULT_TYPE_DNS && result.Type != Constants.Timing.RESULT_TYPE_DNF)
                        {
                            if (!currentRank.ContainsKey(result.Distance))
                            {
                                currentRank[result.Distance] = 1;
                            }
                            result.Ranking = currentRank[result.Distance];
                            if (!genderDict.ContainsKey((result.Distance, result.Gender)))
                            {
                                genderDict[(result.Distance, result.Gender)] = 1;
                            }
                            result.GenderRanking = genderDict[(result.Distance, result.Gender)];
                            if (!ageGroupDict.ContainsKey(result.Age))
                            {
                                ageGroupDict.Add(result.Age, "0-100");
                            }
                            result.AgeGroup = ageGroupDict[result.Age];
                            if (!ageRankDict.ContainsKey((result.Distance, result.Gender, ageGroupDict[result.Age])))
                            {
                                ageRankDict[(result.Distance, result.Gender, ageGroupDict[result.Age])] = 1;
                            }
                            result.AgeRanking = ageRankDict[(result.Distance, result.Gender, ageGroupDict[result.Age])];
                            // update rankings for the next result
                            currentRank[result.Distance] = result.Ranking + 1;
                            genderDict[(result.Distance, result.Gender)] = result.GenderRanking + 1;
                            ageRankDict[(result.Distance, result.Gender, ageGroupDict[result.Age])] = result.AgeRanking + 1;
                        }
                    }
                }
                // Set bibs for people if necessary
                if (setBibs)
                {
                    int bib = 1;
                    foreach (Result res in results)
                    {
                        if (res.Bib == null || res.Bib.Length < 1)
                        {
                            res.Bib = bib.ToString();
                            bib++;
                        }
                    }
                }
                // then upload in batches of UploadCount
                Log.D("MainWindow", "Uploading.");
                int leftovers = results.Count % UploadCount;
                for (int i = 0; i < results.Count - leftovers; i += UploadCount)
                {
                    try
                    {
                        List<APIResult> toUpload = new List<APIResult>();
                        foreach (Result res in results.GetRange(i, UploadCount))
                        {
                            toUpload.Add(res.toAPIResult());
                        }
                        AddResultsResponse response = await APIHandlers.UploadResults(api, slug, year, toUpload);
                        Log.D("MainWindow", "Response received.");
                        if (response.Count != UploadCount)
                        {
                            MessageBox.Show(string.Format("Something went wrong. {0} were uploaded when {1} were expected.", response.Count, UploadCount));
                        }
                    }
                    catch (APIException excep)
                    {
                        MessageBox.Show(excep.Message);
                        return;
                    }
                 }
                try
                {
                    List<APIResult> toUpload = new List<APIResult>();
                    foreach (Result res in results.GetRange(results.Count - leftovers, leftovers))
                    {
                        toUpload.Add(res.toAPIResult());
                    }
                    AddResultsResponse response = await APIHandlers.UploadResults(api, slug, year, toUpload);
                    if (response.Count != leftovers)
                    {
                        MessageBox.Show(string.Format("Something went wrong. {0} were uploaded when {1} were expected.", response.Count, leftovers));
                    }
                }
                catch(APIException excep)
                {
                    MessageBox.Show(excep.Message);
                    return;
                }
                MessageBox.Show("Upload complete.");
                results.Clear();
                updateListView.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Nothing to upload.");
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

        private (int, int)[] DefaultAgeGroups()
        {
            (int, int)[] output = { };
            output.Append((0, 19));
            output.Append((20, 29));
            output.Append((30, 39));
            output.Append((40, 49));
            output.Append((50, 59));
            output.Append((60, 69));
            output.Append((70, 70));
            output.Append((80, 100));
            return output;
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
            else if (uid != "-1")
            {
                year = uid;
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
