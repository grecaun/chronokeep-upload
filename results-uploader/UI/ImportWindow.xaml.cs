using results_uploader.Interfaces.IO;
using results_uploader.IO;
using results_uploader.Objects.API;
using results_uploader.Objects.Helpers;
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
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        internal static string[] human_fields = new string[]
        {
            "",
            "Bib",
            "First Name",
            "Last Name",
            "Age",
            "Gender",
            "Age Group",
            "Distance",
            "Time",
            "Chip Time",
            "Place",
            "Age Place",
            "Gender Place",
            "Type"
        };

        private static readonly int BIB = 1;
        private static readonly int FIRST = 2;
        private static readonly int LAST = 3;
        private static readonly int AGE = 4;
        private static readonly int GENDER = 5;
        private static readonly int AGEGROUP = 6;
        private static readonly int DISTANCE = 7;
        private static readonly int TIME = 8;
        private static readonly int CHIPTIME = 9;
        private static readonly int RANKING = 10;
        private static readonly int AGERANKING = 11;
        private static readonly int GENDERRANKING = 12;
        private static readonly int TYPE = 13;

        IDataImporter importer;
        bool init = true;
        List<Result> results = new List<Result>();
        MainWindow mainWindow;

        public ImportWindow(IDataImporter importer, MainWindow mainWindow)
        {
            InitializeComponent();
            this.importer = importer;
            if (importer.Data.Type == ImportData.FileType.EXCEL)
            {
                SheetRow.Height = new GridLength(35);
                //SheetsBox.ItemsSource = ((ExcelImporter)importer).SheetNames;
                SheetsBox.SelectedIndex = 0;
            }
            for (int i = 1; i < importer.Data.GetNumHeaders(); i++)
            {
                headerListBox.Items.Add(new HeaderListBoxItem(importer.Data.Headers[i], i));
            }
            this.mainWindow = mainWindow;
            init = false;
        }

        internal List<string> RepeatHeaders()
        {
            Log.D("UI.ImportWindow", "Checking for repeat headers in user selection.");
            int[] check = new int[human_fields.Length];
            bool repeat = false;
            List<string> output = new List<string>();
            foreach (ListBoxItem item in headerListBox.Items)
            {
                int val = ((HeaderListBoxItem)item).HeaderBox.SelectedIndex;
                if (val > 0)
                {
                    if (check[val] > 0)
                    {
                        output.Add(((HeaderListBoxItem)item).HeaderBox.SelectedItem.ToString());
                        repeat = true;
                    }
                    else
                    {
                        check[val] = 1;
                    }
                }
            }
            return repeat == true ? output : null;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            List<string> repeats = RepeatHeaders();
            if (repeats != null)
            {
                StringBuilder sb = new StringBuilder("Repeats for the following headers were found:");
                foreach (string s in repeats)
                {
                    sb.Append(" ");
                    sb.Append(s);
                }
                MessageBox.Show(sb.ToString());
                return;
            }
            int[] keys = new int[human_fields.Length + 1];
            for (int i = 0; i< keys.Length; i++)
            {
                keys[i] = 0;
            }
            foreach (HeaderListBoxItem item in headerListBox.Items)
            {
                Log.D("UI.ImportWindow", "Header is " + item.HeaderLabel.Content);
                if (item.HeaderBox.SelectedIndex != 0)
                {
                    keys[item.HeaderBox.SelectedIndex] = item.Index;
                }
            }
            importer.FetchData();
            ImportData data = importer.Data;
            for (int counter = 0; counter < data.Data.Count; counter++)
            {
                int age = int.Parse(data.Data[counter][keys[AGE]]);
                string gender = data.Data[counter][keys[GENDER]];
                if (gender == "M" || gender == "m" || gender == "Male" || gender == "male")
                {
                    gender = "M";
                }
                else if (gender == "F" || gender == "f" || gender == "Female" || gender == "female")
                {
                    gender = "F";
                }
                else
                {
                    gender = "U";
                }
                int seconds = 0, milliseconds = 0, chipseconds = 0, chipmilliseconds = 0;
                string[] tSplit1 = data.Data[counter][keys[TIME]].Split('.');
                string[] tSplit2 = tSplit1[0].Split(':');
                if (tSplit1.Length > 1)
                {
                    try
                    {
                        milliseconds = int.Parse(tSplit1[1]);
                    }
                    catch { }
                }
                if (tSplit2.Length == 3)
                {
                    try
                    {
                        seconds = int.Parse(tSplit2[0]) * 3600 + int.Parse(tSplit2[1]) * 60 + int.Parse(tSplit2[2]);
                    }
                    catch { }
                }
                else if (tSplit2.Length == 2)
                {
                    try
                    {
                        seconds = int.Parse(tSplit2[0]) * 60 + int.Parse(tSplit2[1]);
                    }
                    catch { }
                }
                else if (tSplit2.Length == 1)
                {
                    try
                    {
                        seconds = int.Parse(tSplit2[0]);
                    }
                    catch { }
                }
                if (keys[CHIPTIME] > 0)
                {
                    string[] cSplit1 = data.Data[counter][keys[CHIPTIME]].Split('.');
                    string[] cSplit2 = cSplit1[0].Split(':');
                    if (cSplit1.Length > 1)
                    {
                        try
                        {
                            chipmilliseconds = int.Parse(cSplit1[1]);
                        }
                        catch { }
                    }
                    if (cSplit2.Length == 3)
                    {
                        try
                        {
                            chipseconds = int.Parse(cSplit2[0]) * 3600 + int.Parse(cSplit2[1]) * 60 + int.Parse(cSplit2[2]);
                        }
                        catch { }
                    }
                    else if (cSplit2.Length == 2)
                    {
                        try
                        {
                            chipseconds = int.Parse(cSplit2[0]) * 60 + int.Parse(cSplit2[1]);
                        }
                        catch { }
                    }
                    else if (cSplit2.Length == 1)
                    {
                        try
                        {
                            chipseconds = int.Parse(cSplit2[0]);
                        }
                        catch { }
                    }
                }
                else
                {
                    chipseconds = seconds;
                    chipmilliseconds = milliseconds;
                }
                int type = Constants.Timing.RESULT_TYPE_NORMAL;
                string typeStr = data.Data[counter][keys[TYPE]];
                if (typeStr == null)
                {
                    type = Constants.Timing.RESULT_TYPE_NORMAL;
                }
                else if (typeStr.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                {
                    type = Constants.Timing.RESULT_TYPE_VIRTUAL;
                }
                else if (typeStr.StartsWith("U", StringComparison.OrdinalIgnoreCase))
                {
                    type = Constants.Timing.RESULT_TYPE_UNOFFICIAL;
                }
                else if (typeStr.StartsWith("E", StringComparison.OrdinalIgnoreCase))
                {
                    type = Constants.Timing.RESULT_TYPE_EARLY;
                }
                else if (typeStr.StartsWith("DNF", StringComparison.OrdinalIgnoreCase))
                {
                    type = Constants.Timing.RESULT_TYPE_DNF;
                }
                else if (typeStr.StartsWith("DNS", StringComparison.OrdinalIgnoreCase))
                {
                    type = Constants.Timing.RESULT_TYPE_DNS;
                }
                int rank = keys[RANKING] == 0 ? 0 : int.Parse(data.Data[counter][keys[RANKING]]);
                int ageRank = keys[AGERANKING] == 0 ? 0 : int.Parse(data.Data[counter][keys[AGERANKING]]);
                int genderRank = keys[GENDERRANKING] == 0 ? 0 : int.Parse(data.Data[counter][keys[AGERANKING]]);
                string bib = data.Data[counter][keys[BIB]] == null ? "" : data.Data[counter][keys[BIB]];
                if (data.Data[counter][keys[FIRST]] == null)
                {
                    MessageBox.Show("First name not set.");
                    return;
                }
                if (data.Data[counter][keys[LAST]] == null)
                {
                    MessageBox.Show("Last name not set.");
                    return;
                }
                if (data.Data[counter][keys[DISTANCE]] == null)
                {
                    MessageBox.Show("Distance not set.");
                    return;
                }
                results.Add(new Result(
                    bib,                                                // BIB
                    data.Data[counter][keys[FIRST]],                    // FIRST
                    data.Data[counter][keys[LAST]],                     // LAST
                    age,                                                // AGE
                    gender,                                             // GENDER
                    data.Data[counter][keys[AGEGROUP]],                 // AGEGROUP
                    data.Data[counter][keys[DISTANCE]],                 // DISTANCE
                    chipseconds,                                        // CHIPSECONDS
                    chipmilliseconds,                                   // CHIPMILLISECONDS
                    "Finish",                                           // SEGMENT
                    "Finish",                                           // LOCATION
                    1,                                                  // OCCURRENCE
                    rank,                                               // PLACE
                    ageRank,                                            // AGEPLACE
                    genderRank,                                         // GENDERPLACE
                    true,                                               // FINISH
                    type,                                               // TYPE
                    seconds,                                            // SECONDS
                    milliseconds));                                     // MILLISECONDS
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        internal static int GetHeaderBoxIndex(string s)
        {
            Log.D("ImportFileWindow", "Looking for a value for: " + s);
            if (s.IndexOf("Bib", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("pinney", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BIB;
            }
            if (s.IndexOf("First", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return FIRST;
            }
            if (s.IndexOf("Last", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return LAST;
            }
            if (string.Equals(s, "Age", StringComparison.OrdinalIgnoreCase))
            {
                return AGE;
            }
            if (string.Equals(s, "Gender", StringComparison.OrdinalIgnoreCase))
            {
                return GENDER;
            }
            if (string.Equals(s, "Age Group", StringComparison.OrdinalIgnoreCase))
            {
                return AGEGROUP;
            }
            if (s.IndexOf("Distance", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("Division", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return DISTANCE;
            }
            if (string.Equals(s, "Time", StringComparison.OrdinalIgnoreCase))
            {
                return TIME;
            }
            if (s.IndexOf("Chip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return CHIPTIME;
            }
            if (string.Equals(s, "Place", StringComparison.OrdinalIgnoreCase))
            {
                return RANKING;
            }
            if (s.IndexOf("Age Pl", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return AGERANKING;
            }
            if (s.IndexOf("Gender Pl", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return GENDERRANKING;
            }
            if (string.Equals(s, "Type", StringComparison.OrdinalIgnoreCase))
            {
                return TYPE;
            }
            return 0;
        }

        private void SheetsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) { return; }
            int selection = ((ComboBox)sender).SelectedIndex;
            Log.D("UI.ImportFile", "You've selected number " + selection);
            /*ExcelImporter excelImporter = (ExcelImporter)importer;
            excelImporter.ChangeSheet(selection + 1);
            excelImporter.FetchHeaders();
            headerListBox.Items.Clear();
            for (int i = 1; i < importer.Data.GetNumHeaders(); i++)
            {
                headerListBox.Items.Add(new HeaderListBoxItem(importer.Data.Headers[i], i));
            } //*/
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.ImportFinalize(results);
        }
    }
    internal class HeaderListBoxItem : ListBoxItem
    {
        public Label HeaderLabel { get; private set; }
        public ComboBox HeaderBox { get; private set; }
        public int Index { get; private set; }
        public HeaderListBoxItem(string s, int ix)
        {
            this.IsTabStop = false;
            Index = ix;
            Grid theGrid = new Grid();
            this.Content = theGrid;
            theGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            theGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            HeaderLabel = new Label
            {
                Content = s
            };
            theGrid.Children.Add(HeaderLabel);
            Grid.SetColumn(HeaderLabel, 0);
            HeaderBox = new ComboBox
            {
                ItemsSource = ImportWindow.human_fields,
                SelectedIndex = ImportWindow.GetHeaderBoxIndex(s.ToLower().Trim()),
            };
            theGrid.Children.Add(HeaderBox);
            Grid.SetColumn(HeaderBox, 1);
        }
    }
}
