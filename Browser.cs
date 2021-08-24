using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using zakupki.gov.ru.Models;

namespace zakupki.gov.ru
{
    enum FederalLaw
    {
        FL_44,
        FL_223
    }
    static class Browser
    {
        private static byte federalLawType;

        private static IWebDriver browser;
        static readonly string downloadsFolderPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Temp");

        private static readonly string url44 = "https://zakupki.gov.ru/epz/order/notice/ea44/view/common-info.html?regNumber=";
        private static readonly string doc44Url = "https://zakupki.gov.ru/epz/order/notice/ea44/view/documents.html?regNumber=";

        private static readonly string url223 = "https://zakupki.gov.ru/223/purchase/public/purchase/info/common-info.html?regNumber=";
        private static readonly string doc223Url = "https://zakupki.gov.ru/223/purchase/public/purchase/info/documents.html?regNumber=";

        #region События
        delegate void BrowserHandler(string message);
        static event BrowserHandler PageNotFound;

        public static event Action BrowserLoad;
        public static event Action FindIn44;
        public static event Action FindIn223;
        public static event Action DownloadDocuments;
        public static event Action ExtractDataEvent;
        public static event Action DisposeBrowser;
        public static event Action GetOrganisationData;
        #endregion

        private static void CreateTempFolder()
        {
            if (Directory.Exists(downloadsFolderPath))
                Directory.Delete(downloadsFolderPath, true);
            Directory.CreateDirectory(downloadsFolderPath);
        }
        private static void BrowserInit(bool isVisible = true)
        {
            CreateTempFolder();

            BrowserLoad?.Invoke();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            if (!isVisible) options.AddArgument("headless");
            //options.AddArgument("--start-maximized");
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            //   options.AddUserProfilePreference("profile.managed_default_content_settings.javascript", 2);
            options.AddUserProfilePreference("download.default_directory", downloadsFolderPath);
            options.AddUserProfilePreference("intl.accept_languages", "nl");
            options.AddUserProfilePreference("disable-popup-blocking", "true");


            browser = new ChromeDriver(service, options);
            browser.Manage().Window.Minimize();
        }
        private static void BrowserDispose()
        {
            DisposeBrowser?.Invoke();
            browser.Dispose();
        }
        public static Purchase ExtractData(string number)
        {
            // Запуск браузера
            BrowserInit();
            Purchase purchase = new Purchase();

            // Получение нужной страницы
            FindIn44?.Invoke();
            browser.Navigate().GoToUrl($"{url44}{number}");

            if (browser.Title.Contains("Страница не найдена"))
            {
                //Просто для красоты
                Task.Delay(300);

                PageNotFound?.Invoke("Страница с 44-ФЗ не найдена");
                FindIn223?.Invoke();
                browser.Navigate().GoToUrl($"{url223}{number}");
                if (browser.Title.Contains("Страница не найдена"))
                {
                    BrowserDispose();
                    return purchase;
                }
                else
                {
                    federalLawType = (byte)FederalLaw.FL_223;
                    purchase.FederalLaw = "223-ФЗ";
                }
            }
            else
            {
                federalLawType = (byte)FederalLaw.FL_44;
                purchase.FederalLaw = "44-ФЗ";
            }
            purchase.Number = number;

            // Сбор данных и документов.
            ExtractDataEvent?.Invoke();
            switch (federalLawType)
            {
                case (byte)FederalLaw.FL_44:
                    GetPurcaheAs44(purchase);
                    DownloadDocuments?.Invoke();
                    GetDocumentAs44(number);
                    GetCompanyDataAs44(purchase);
                    break;
                case (byte)FederalLaw.FL_223:
                    GetPurcaheAs223(purchase);
                    DownloadDocuments?.Invoke();
                    GetDocumentAs223(number);
                    break;
            }

            // Завершение
            BrowserDispose();
            return purchase;
        }

        private static Purchase GetCompanyDataAs44(Purchase purchase)
        {
            Company company = new Company();

            GetOrganisationData?.Invoke();
            // browser.Navigate().GoToUrl($"{doc44Url}{purchase.Number}");

            var links = browser.FindElements(By.CssSelector("span.cardMainInfo__content>a"));
            if (links.Count > 0)
            {
                browser.Navigate().GoToUrl($"{links[0].GetAttribute("href")}");
                var infoBlocks = browser.FindElements(By.CssSelector("div.registry-entry__body>div:nth-child(2)>div>div"));
                foreach (var block in infoBlocks)
                {
                    var innerBlocks = block.FindElements(By.TagName("div"));
                    if (innerBlocks.Count == 2)
                    {
                        if (innerBlocks[0].Text.Contains("ОГРН"))
                            company.OGRN = innerBlocks[1].Text;
                        if (innerBlocks[0].Text.Contains("ИНН"))
                            company.INN = innerBlocks[1].Text;
                        if (innerBlocks[0].Text.Contains("КПП"))
                            company.KPP = innerBlocks[1].Text;

                    }
                }
                company.Address = browser.FindElement(By.CssSelector("div.registry-entry__body>div:nth-child(1)>div.registry-entry__body-value")).Text;

            }

            purchase.Company = company;
            return purchase;
        }

        private static Purchase GetPurcaheAs44(Purchase purchase)
        {
            var infoBlocks = browser.FindElements(By.ClassName("blockInfo__section"));

            foreach (IWebElement block in infoBlocks)
            {
                if (block.Text.Contains("Способ определения поставщика"))
                {
                    purchase.Determining = block.FindElement(By.ClassName("section__info")).Text;
                }
                if (block.Text.Contains("Размещение осуществляет"))
                {
                    var temp = block.FindElements(By.TagName("a"));
                    if (temp.Count > 0)
                    {
                        purchase.Customer = temp[0].Text;
                    }
                }
                if (block.Text.Contains("Дата и время окончания срока"))
                {
                    purchase.ApplicationDeadline = block.FindElement(By.ClassName("section__info")).Text;
                }
                if (block.Text.Contains("Наименование объекта закупки"))
                {
                    purchase.ProcurementObject = block.FindElement(By.ClassName("section__info")).Text;
                }
                if (block.Text.Contains("Дата проведения аукциона"))
                {
                    purchase.DateAuction = block.FindElement(By.ClassName("section__info")).Text;
                }
                if (block.Text.Contains("Время проведения аукциона"))
                {
                    purchase.TimeAuction = block.FindElement(By.ClassName("section__info")).Text;
                }
                if (block.Text.Contains("Наименование электронной площадки"))
                {
                    purchase.Platform = block.FindElement(By.ClassName("section__info")).Text;
                }
                if (block.Text.Contains("Ограничения и запреты"))
                {
                    purchase.Restrictions = block.FindElement(By.ClassName("section__info")).Text;

                }
                if (block.Text.Contains("Платежные реквизиты для обеспечения исполнения контракта"))
                {
                    purchase.PaymentDetails = block.FindElement(By.ClassName("section__info")).Text;
                }
                if (block.Text.Contains("Размер обеспечения исполнения контракта"))
                {
                    purchase.PrepaidExpense = block.FindElement(By.ClassName("section__info")).Text;
                }
            }

            purchase.Price = browser.FindElement(By.ClassName("cost")).Text;
            purchase.TimeZone = purchase.ApplicationDeadline;

            purchase.Status = browser.FindElement(By.ClassName("cardMainInfo__state")).Text;

            return purchase;
        }
        private static void GetDocumentAs44(string num)
        {
            browser.Navigate().GoToUrl($"{doc44Url}{num}");
            var fileHeader = browser.FindElements(By.ClassName("blockFilesTabDocs"));
            if (fileHeader.Count > 0)
            {
                var filesURL = fileHeader[0].FindElements(By.CssSelector("span>a"));
                if (filesURL.Count > 0)
                {
                    DownloadDocuments?.Invoke();
                    foreach (var link in filesURL)
                    {
                        //var l = link.GetAttribute("href");
                        //  link.Click();
                        browser.SwitchTo().Window(browser.WindowHandles.Last());
                        browser.Navigate().GoToUrl(link.GetAttribute("href"));
                        browser.SwitchTo().Window(browser.WindowHandles.First());
                    }
                }
                // Не все файлы успевают догрузиться. Добавил задержку
                Thread.Sleep(2000);
            }
        }

        private static Purchase GetPurcaheAs223(Purchase purchase)
        {
            var rows = browser.FindElements(By.TagName("tr"));

            foreach (var row in rows)
            {
                var cols = row.FindElements(By.TagName("td"));

                if (cols.Count > 0)
                {
                    if (cols[0].Text.Contains("Наименование электронной площадки"))
                    {
                        purchase.Platform = cols[1].Text;
                    }
                    if (cols[0].Text.Contains("Способ размещения закупки"))
                    {
                        purchase.Determining = cols[1].Text;
                    }
                    if (cols[0].Text.Contains("Наименование закупки"))
                    {
                        purchase.ProcurementObject = cols[1].Text;
                    }
                    if (cols[0].Text.Contains("Наименование организации"))
                    {
                        purchase.Customer = cols[1].Text;
                    }
                    if (cols[0].Text.Contains("Дата и время окончания подачи заявок"))
                    {
                        purchase.ApplicationDeadline = cols[1].Text;
                    }
                    if (cols[0].Text.Contains("Дата начала срока подачи заявок"))
                    {
                        purchase.DateAuction = cols[1].Text;
                    }
                }
            }

            purchase.TimeZone = browser.FindElement(By.ClassName("public")).Text;
            return purchase;
        }
        private static void GetDocumentAs223(string num)
        {
            browser.Navigate().GoToUrl($"{doc223Url}{num}");
            var rows = browser.FindElements(By.TagName("tr"));
            DownloadDocuments?.Invoke();
            if (rows.Count > 0)
            {
                foreach (var row in rows)
                {
                    var tdList = row.FindElements(By.TagName("td"));
                    if (tdList.Count == 5)
                    {
                        if (tdList[2].Text.Contains("действующая"))
                        {
                            var link = tdList[1].FindElement(By.TagName("a"));
                            browser.SwitchTo().Window(browser.WindowHandles.Last());
                            browser.Navigate().GoToUrl(link.GetAttribute("href"));
                            browser.SwitchTo().Window(browser.WindowHandles.First());
                        }
                    }
                }

                // Не все файлы успевают догрузиться. Добавил задержку
                Thread.Sleep(2000);
            }
        }

    }
}
