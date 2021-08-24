using Microsoft.Office.Interop.Excel;
using System;
using System.Threading.Tasks;
using System.Windows;
using zakupki.gov.ru.Models;
using Application = Microsoft.Office.Interop.Excel.Application;
using Excel = Microsoft.Office.Interop.Excel;

namespace zakupki.gov.ru
{
    static class ExcelWorker
    {
        private static Application ex;
        private static Workbook wb;
        private static Worksheet sheet;
        private static int currnetRow;
        public static string PathToExcelFile { get; set; }
        /// <summary>
        /// Запуск Excel и октрытие табилцы
        /// </summary>
        /// <param name="visible">Видимая (для тестов)</param>
        /// <returns></returns>
        public static bool ExcelAppInit(bool visible = false)
        {
            if (!string.IsNullOrEmpty(PathToExcelFile))
            {
                try
                {
                    ex = new Application
                    {
                        Visible = visible,
                        DisplayAlerts = false
                    };
                    wb = ex.Workbooks.Open(PathToExcelFile);
                    sheet = (Worksheet)wb.Worksheets.get_Item(1);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void ExcelAppDispose(bool DocSave)
        {
            if (ex != null)
            {
                if (DocSave)
                    wb.Save();
                ex.Quit();
            }
        }
        public static string GetSendMailNunber()
        {
            dynamic number = string.Empty;
            Range lastCell = sheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell, Type.Missing);

            for (int i = lastCell.Row; i > 1; i--)
            {
                var cellData = sheet.Cells[i, 1].Value;

                if (cellData != null)
                {
                    number = sheet.Cells[i + 1, 2].Value;
                    if (number == null) number = string.Empty;

                    if (int.TryParse(cellData.ToString(), out int counter))
                    {
                        sheet.Cells[i + 1, 1].Value = ++counter;
                    }
                    else
                    {
                        sheet.Cells[i + 1, 1].Value = 1;
                    }
                    currnetRow = i;
                    break;
                }
            }
            return number.ToString();
        }
        internal static void WriteDataToTable(Purchase purchase)
        {
            int row = currnetRow + 1;
            //   cells.NumberFormat = "@";

            string hyperLink = string.Empty;
            if (purchase.FederalLaw == "44-ФЗ")
                hyperLink = "https://zakupki.gov.ru/epz/order/notice/ea44/view/common-info.html?regNumber=" + purchase.Number;
            if (purchase.FederalLaw == "223-ФЗ")
                hyperLink = "https://zakupki.gov.ru/223/purchase/public/purchase/info/common-info.html?regNumber=" + purchase.Number;

            sheet.Hyperlinks.Add(sheet.Cells[row, 3], hyperLink, Type.Missing, "Процедура №" + purchase.Number, "№ " + purchase.Number);
            sheet.Cells[row, 4].Value = string.Format("{0:dd.MM.yyyy}", DateTime.Now.Date);
            sheet.Cells[row, 5].Value = purchase.Price;
            sheet.Cells[row, 6].Value = purchase.DateAuction;
            sheet.Cells[row, 7].Value = purchase.TimeAuction;
            sheet.Cells[row, 8].Value = purchase.Determining;
            sheet.Cells[row, 9].Value = purchase.FederalLaw;
            sheet.Cells[row, 10].Value = purchase.Platform;
            sheet.Cells[row, 11].Value = purchase.TimeZone;
            sheet.Cells[row, 12].Value = purchase.ApplicationDeadline;
            sheet.Cells[row, 15].Value = purchase.Customer;
            sheet.Cells[row, 16].Value = purchase.ProcurementObject;
            sheet.Cells[row, 17].Value = purchase.Restrictions;
        }
    }
}
