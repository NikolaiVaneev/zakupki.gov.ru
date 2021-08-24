using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WK.Libraries.BetterFolderBrowserNS;
using zakupki.gov.ru.Models;


namespace zakupki.gov.ru
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string activateKey;

        #region Служебные методы
        /// <summary>
        /// Записать сообщение в лог
        /// </summary>
        /// <param name="message">Сообщение</param>
        private async void AddLogMessage(string message)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)(() => Logs.AppendText($"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}")));
                Dispatcher.BeginInvoke((Action)(() => Logs.ScrollToEnd()));
            });
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddLogMessage("Приложение запущено");
            LoadSettings();
        }
        private void SettingsShow(object sender, RoutedEventArgs e)
        {
            PathSettings settings = new PathSettings();
            settings.Show();
        }
        #endregion

        #region Настройки
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            AddLogMessage("Сохранение настроек");
            Settings settings = new Settings
            {
                //LineWidth = int.Parse(TB_LineWidth.Text),
            };
            settings.Save();
            AddLogMessage("Настройки сохранены");
        }
        private void LoadSettings()
        {
            AddLogMessage("Загрузка настроек приложения");
            Settings settings = new Settings().Load();
            activateKey = settings.License;
            AddLogMessage("Настройки загружены");

            // HACK : Проверка лицензии отключается на время тестирования
            // Сделал отдельную переменную, чтобы код не комментить каждый раз
            bool appTesting = false;
            if (!appTesting)
            {
                if (!Cryptographer.CheckActivationApp(activateKey))
                {
                    Code.IsEnabled = false;
                    License license = new License();
                    license.Show();
                }
            }
            



        }
        #endregion

        #region Подписки на события браузера
        private void Browser_BrowserLoad()
        {
            AddLogMessage("Запуск браузера");
            Browser.BrowserLoad -= Browser_BrowserLoad;
        }
        private void Browser_FindIn223()
        {
            AddLogMessage("Поиск по 223-ФЗ");
            Browser.FindIn223 -= Browser_FindIn223;
        }
        private void Browser_FindIn44()
        {
            AddLogMessage("Поиск по 44-ФЗ");
            Browser.FindIn44 -= Browser_FindIn44;
        }
        private void Browser_ExtractDataEvent()
        {
            AddLogMessage("Извлечение данных");
            Browser.ExtractDataEvent -= Browser_ExtractDataEvent;
        }
        private void Browser_DownloadDocuments()
        {
            AddLogMessage("Поиск и загрузка документов");
            Browser.DownloadDocuments -= Browser_DownloadDocuments;
        }
        private void Browser_DisposeBrowser()
        {
            AddLogMessage("Закрытие браузера");
            Browser.DisposeBrowser -= Browser_DisposeBrowser;
        }
        private void Browser_GetOrganisationData()
        {
            AddLogMessage("Получение данных заказчика");
            Browser.GetOrganisationData -= Browser_GetOrganisationData;
        }
        #endregion

        private async void Start(object sender, RoutedEventArgs e)
        {
            string purchaseCode = Code.Text;
            Purchase purchase = new Purchase();
            string OutputMessageNumber;

            Browser.BrowserLoad += Browser_BrowserLoad;
            Browser.FindIn44 += Browser_FindIn44;
            Browser.FindIn223 += Browser_FindIn223;
            Browser.ExtractDataEvent += Browser_ExtractDataEvent;
            Browser.DownloadDocuments += Browser_DownloadDocuments;
            Browser.DisposeBrowser += Browser_DisposeBrowser;
            Browser.GetOrganisationData += Browser_GetOrganisationData;

            await Task.Run(() =>
            {
                try
                {
                    Dispatcher.BeginInvoke((Action)(() => BtnStart.IsEnabled = false));

                    #region Работа браузера
                    purchase = Browser.ExtractData(purchaseCode);
                    if (purchase.Number == null)
                    {
                        AddLogMessage("Не удалось найти данные");
                        return;
                    }
                    #endregion

                    #region Получение исходящего
                    AddLogMessage("Получение исходящего номера");
                    Settings settings = new Settings().Load();
                    ExcelWorker.PathToExcelFile = settings.ReportTablePath;
                    // HACK : в процессе тестирования поставить true
                    if (ExcelWorker.ExcelAppInit(false))
                    {
                        OutputMessageNumber = ExcelWorker.GetSendMailNunber();
                        if (string.IsNullOrEmpty(OutputMessageNumber))
                        {
                            AddLogMessage("Исходящий номер не присвоен. Укажите его в таблице");
                            ExcelWorker.ExcelAppDispose(false);
                            return;
                        }
                    }
                    else
                    {
                        AddLogMessage("Не удалось открыть таблицу заявок");
                        return;
                    }
                    purchase.OutputMailNum = OutputMessageNumber;
                    AddLogMessage($"Исходящий номер - {OutputMessageNumber}");
                    #endregion

                    FoldersCreate(purchase);
                    DocumentsCreate(purchase);
                    ExcelWorker.ExcelAppDispose(true);
                    AddLogMessage($"Работа приложения завершена");
                    MessageBox.Show("Процес создания документов завершен", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    AddLogMessage($"В процессе выполнения произошла ошибка{Environment.NewLine}{ex}");
                    ExcelWorker.ExcelAppDispose(false);
                }
                finally
                {

                    Dispatcher.BeginInvoke((Action)(() => BtnStart.IsEnabled = true));
                }
            });
        }



        private void FoldersCreate(Purchase purchase)
        {
            Settings settings = new Settings().Load();
            string pathToFolder = settings.FolderForResults;
            // TODO : Надо как-то генерировать название конечной папки
            string projectFolderName = purchase.Number;
            if (string.IsNullOrEmpty(pathToFolder))
            {
                var fd = new BetterFolderBrowser
                {
                    Title = "Укажите папку для сохранения",
                    Multiselect = false
                };
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    pathToFolder = fd.SelectedFolder;
                }
                else
                {
                    AddLogMessage("Выбор папки отменен пользвателем");
                    return;
                }
            }
            else
            {
                pathToFolder += "\\";
            }

            AddLogMessage("Создание папок");
            string ADPath = Path.Combine(pathToFolder, projectFolderName, "АД");
            string contractPath = Path.Combine(pathToFolder, projectFolderName, "Контракт");
            string AppPath = Path.Combine(pathToFolder, projectFolderName, "Заявка");

            Directory.CreateDirectory(Path.Combine(pathToFolder, projectFolderName));
            Directory.CreateDirectory(ADPath);
            Directory.CreateDirectory(contractPath);
            Directory.CreateDirectory(AppPath);
        }
        private void DocumentsCreate(Purchase purchase)
        {
            Settings settings = new Settings().Load();
            string pathToFolder = settings.FolderForResults;
            // TODO : Надо как-то генерировать название конечной папки
            string projectFolderName = purchase.Number;
            string ADPath = Path.Combine(pathToFolder, projectFolderName, "АД");
            string AppPath = Path.Combine(pathToFolder, projectFolderName, "Заявка");

            AddLogMessage("Формирование пакета документов");

            if (!string.IsNullOrEmpty(settings.FirstPartReferencePath) && File.Exists(settings.FirstPartReferencePath))
            {
                WordWorker.CreateDocument(settings.FirstPartReferencePath, Path.Combine(AppPath, Path.GetFileName(settings.FirstPartReferencePath)), purchase);
                AddLogMessage("Заявка на участие (1 часть) создана");
            }

            if (!string.IsNullOrEmpty(settings.SecondPartReferencePath) && File.Exists(settings.SecondPartReferencePath))
            {
                WordWorker.CreateDocument(settings.SecondPartReferencePath, Path.Combine(AppPath, Path.GetFileName(settings.SecondPartReferencePath)), purchase);
                AddLogMessage("Заявка на участие (2 часть) создана");
            }

            if (!string.IsNullOrEmpty(settings.Examination31Path) && File.Exists(settings.Examination31Path))
            {
                WordWorker.CreateDocument(settings.Examination31Path, Path.Combine(AppPath, Path.GetFileName(settings.Examination31Path)), purchase);
                AddLogMessage("Декларация о соответствии (1 часть)");
            }

            if (!string.IsNullOrEmpty(settings.Examination66Path) && File.Exists(settings.Examination66Path))
            {
                WordWorker.CreateDocument(settings.Examination66Path, Path.Combine(AppPath, Path.GetFileName(settings.Examination66Path)), purchase);
                AddLogMessage("Декларация о соответствии (2 часть)");
            }

            if (!string.IsNullOrEmpty(settings.ExaminationITNPath) && File.Exists(settings.ExaminationITNPath))
            {
                WordWorker.CreateDocument(settings.ExaminationITNPath, Path.Combine(AppPath, Path.GetFileName(settings.ExaminationITNPath)), purchase);
                AddLogMessage("Сведения об ИНН");
            }


            if (!string.IsNullOrEmpty(settings.PrivisionFilePath) && File.Exists(settings.PrivisionFilePath))
            {
           
                // string savePath = Path.Combine(AppPath, Path.GetFileName(settings.PrivisionFilePath));
                string savePath = Path.Combine(AppPath, $"ОИК_{purchase.Number.Substring(purchase.Number.Length - 4)}{Path.GetExtension(settings.PrivisionFilePath)}");
                WordWorker.CreateDocument(settings.PrivisionFilePath, savePath, purchase);
                AddLogMessage("Обеспечение исполнения контракта");

            }

            // Копирование PDF
            if (!string.IsNullOrEmpty(settings.FirstPDFFilePath))
                File.Copy(settings.FirstPDFFilePath, Path.Combine(AppPath, Path.GetFileName(settings.FirstPDFFilePath)), true);
            if (!string.IsNullOrEmpty(settings.SecondPDFFilePath))
                File.Copy(settings.SecondPDFFilePath, Path.Combine(AppPath, Path.GetFileName(settings.SecondPDFFilePath)), true);

            // Копирование скаченных файлов
            var tempFolderPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Temp");
            foreach (var newPath in Directory.GetFiles(tempFolderPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                File.Copy(newPath, newPath.Replace(tempFolderPath, ADPath), true);
            }

            if (!string.IsNullOrEmpty(settings.ReportTablePath) && File.Exists(settings.ReportTablePath))
            {
                AddLogMessage("Запись в таблицу");
                ExcelWorker.WriteDataToTable(purchase);
            }

            AddLogMessage("Успешно завершено");
        }

        private void ShowInfo(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Продукт \"Zakupki application\" является программным модулем для анализа по заданным " +
                "критериям сайта zakupki.gov.ru с последующим формированием заявок по шаблонам, с целью подачи их " +
                "для участия в процедурах по 44-ФЗ и 223-ФЗ. Исключительные права на данный продукт принадлежат ее " +
                "правообладателю - ГК \"Проф.Ком\" (тм)." + Environment.NewLine +
                "Несанкционированное использование данного программного " +
                "продукта не в интересах правообладателя наносит экономический вред правообладателю, со всеми " +
                "вытекающими последствиями в соответствии с УК и КоАП РФ." + Environment.NewLine +
                "Тел.: +7 (996) 062-72-72, +7 (909) 563-64-64",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

}
