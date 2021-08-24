using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WK.Libraries.BetterFolderBrowserNS;

namespace zakupki.gov.ru
{
    public partial class PathSettings : Window
    {
        public PathSettings()
        {
            InitializeComponent();
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child != null && child is T t)
                    yield return t;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
        private void SetFilePath(object sender, RoutedEventArgs e)
        {
            int i = 0;

            // Походу, MaterialDesign имеет вложенные Button, поэтому в найденной коллекции их больше в два раза.
            // поэтому elementIndex делим на 2 в каждой итерации
            int elementIndex = 0;

            var buttons = FindVisualChildren<Button>(Content).ToList();
            foreach (var btn in buttons)
            {
                if (btn == sender)
                {
                    OpenFileDialog ofd = new OpenFileDialog
                    {
                        RestoreDirectory = true
                    };

                    switch (elementIndex) 
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            ofd.Title = "Выберите шаблон документа";
                            ofd.Filter = "Документы MS Word (*.doc;*.docx;)|*.doc;*.docx;";
                            break;
                        case 6:
                        case 7:
                            ofd.Title = "Выберите PDF документ";
                            ofd.Filter = "Документы (*.pdf)|*.pdf";
                            break;
                        case 8:
                            ofd.Title = "Выберите таблицу отчета";
                            ofd.Filter = "Электронные табилцы MS Excel (*.xls; *.xlsx)|*.xls;*.xlsx";
                            break;
                        default:
                            ofd.Title = "Выберите файл";
                            break;
                    }

                    bool? result = ofd.ShowDialog();
                    if (result == true)
                    {
                        FindVisualChildren<TextBox>(Content).ToList()[elementIndex].Text = ofd.FileName;
                    }
                }
                i++;
                elementIndex = i / 2;
            }
        }

        private void SetFolderPath(object sender, RoutedEventArgs e)
        {
            var fd = new BetterFolderBrowser
            {
                Title = "Укажите папку для сохранения",
                Multiselect = false
            };
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TBoxFolderPath.Text = fd.SelectedPath;
            }
        }
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            var textBoxes = FindVisualChildren<TextBox>(Content).ToList();

            settings.FirstPartReferencePath = textBoxes[0].Text;
            settings.SecondPartReferencePath = textBoxes[1].Text;
            settings.Examination31Path = textBoxes[2].Text;
            settings.Examination66Path = textBoxes[3].Text;
            settings.ExaminationITNPath = textBoxes[4].Text;
            settings.PrivisionFilePath = textBoxes[5].Text;
            settings.FirstPDFFilePath = textBoxes[6].Text;
            settings.SecondPDFFilePath = textBoxes[7].Text;
            settings.ReportTablePath = textBoxes[8].Text;
            settings.FolderForResults = textBoxes[9].Text;

            settings.Save();
            Close();
            MessageBox.Show("Настройки приложения успешно сохранены", 
                "Соханение настроек приложения", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            var textBoxes = FindVisualChildren<TextBox>(Content).ToList();
            Settings settings = new Settings().Load();
            textBoxes[0].Text = settings.FirstPartReferencePath;
            textBoxes[1].Text = settings.SecondPartReferencePath;
            textBoxes[2].Text = settings.Examination31Path;
            textBoxes[3].Text = settings.Examination66Path;
            textBoxes[4].Text = settings.ExaminationITNPath;
            textBoxes[5].Text = settings.PrivisionFilePath;
            textBoxes[6].Text = settings.FirstPDFFilePath;
            textBoxes[7].Text = settings.SecondPDFFilePath;
            textBoxes[8].Text = settings.ReportTablePath;
            textBoxes[9].Text = settings.FolderForResults; 
        }

    }
}
