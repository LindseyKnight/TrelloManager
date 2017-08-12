using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Win32;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style.XmlAccess;
using Trello.Wpf.Models;
using Trello.Wpf.ViewModels;

namespace Trello.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Settings settings = GetSettings() ?? new Settings();
            Top = settings.Top ?? 100;
            Left = settings.Left ?? 100;
            Width = settings.Width ?? 1000;
            Height = settings.Height ?? 1200;
            MainWindowViewModel = new MainWindowViewModel(settings);
            InitializeComponent();
        }

        public MainWindowViewModel MainWindowViewModel { get; set; }

        private static Settings GetSettings()
        {
            try
            {
                using (Stream stream = new FileStream(GetSettingsPath(), FileMode.Open))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<Settings>(reader.ReadToEnd());
                }
            }
            catch (Exception)
            {
            }

            return new Settings();
        }

        private void Hyperlink_Navigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ExportToExcel_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { FileName = "Trello.xlsx", Filter = "Excel|*.xlsx" };
            if (saveFileDialog.ShowDialog() == true)
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Trello");
                    ExcelNamedStyleXml hyperlinkStyle = worksheet.Workbook.Styles.CreateNamedStyle("Hyperlink");
                    hyperlinkStyle.Style.Font.UnderLine = true;
                    hyperlinkStyle.Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ExcelRange headerCells = worksheet.Cells[1, 1, 1, 6];
                    headerCells.Style.Font.Bold = true;

                    worksheet.Cells[1, 1].Value = "Mine";
                    worksheet.Cells[1, 2].Value = "Title";
                    worksheet.Cells[1, 3].Value = "Url";
                    worksheet.Cells[1, 4].Value = "Created";
                    worksheet.Cells[1, 5].Value = "List";
                    worksheet.Cells[1, 6].Value = "Assigned To";

                    int index = 2;
                    foreach (CardViewModel card in MainWindowViewModel.Sort(MainWindowViewModel.Cards))
                    {
                        worksheet.Cells[index, 1].Value = card.AssignedToMe ? "Yes" : "No";
                        worksheet.Cells[index, 2].Value = card.Title;
                        worksheet.Cells[index, 3].Formula = string.Format("=hyperlink(\"{0}\")", card.Url);
                        worksheet.Cells[index, 3].StyleName = hyperlinkStyle.Name;
                        worksheet.Cells[index, 4].Value = card.CreationDate;
                        worksheet.Cells[index, 5].Value = card.List;
                        worksheet.Cells[index, 6].Value = card.AssignedTo;
                        index++;
                    }

                    worksheet.Column(4).Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells.AutoFitColumns(5, 115);
                    worksheet.Column(3).Width = 28.5;

                    Stream stream;
                    try
                    {
                        stream = File.Create(saveFileDialog.FileName);
                    }
                    catch (IOException)
                    {
                        stream = null;
                    }
                    if (stream == null)
                    {
                        try
                        {
                            string suffix = Guid.NewGuid().ToString("N").Substring(0, 5);
                            string fileName = Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), string.Format("{0}-{1}{2}",
                                Path.GetFileNameWithoutExtension(saveFileDialog.FileName),
                                suffix,
                                Path.GetExtension(saveFileDialog.FileName)));
                            stream = File.Create(Path.GetDirectoryName(fileName));
                        }
                        catch (IOException)
                        {
                            stream = null;
                        }
                    }
                    if (stream != null)
                    {
                        package.SaveAs(stream);
                        stream.Close();
                    }
                }
            }
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.Refresh();
        }

        private void FilterCards_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.FilterCards();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            File.WriteAllText(GetSettingsPath(), JsonConvert.SerializeObject(new Settings
            {
                Top = Top,
                Left = Left,
                Width = Width,
                Height = Height,
                TrelloName = MainWindowViewModel.TrelloFullName,
                GroupByList = MainWindowViewModel.GroupByList,
                GroupByMember = MainWindowViewModel.GroupByMember,
                GroupByPriority = MainWindowViewModel.GroupByPriority,
                ListFilters = MainWindowViewModel.ListFilters.Where(x => x.IsSelected).Select(x => x.FilterId).ToList(),
                MemberFilters = MainWindowViewModel.MemberFilters.Where(x => x.IsSelected).Select(x => x.FilterId).ToList(),
                PriorityFilters = MainWindowViewModel.PriorityFilters.Where(x => x.IsSelected).Select(x => x.FilterId).ToList(),
                StatusFilters = MainWindowViewModel.StatusFilters.Where(x => x.IsSelected).Select(x => x.FilterId).ToList(),
            }));
        }

        private static string GetSettingsPath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
        }
    }
}
