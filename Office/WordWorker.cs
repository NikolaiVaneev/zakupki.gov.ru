using Word = Microsoft.Office.Interop.Word;

using System;
using zakupki.gov.ru.Models;
using System.Threading.Tasks;


namespace zakupki.gov.ru
{
    static class WordWorker
    {
        /// <summary>
        /// Создание документа по шаблону
        /// </summary>
        /// <param name="source">Путь до шаблона</param>
        /// <param name="outputPath">Куда сохранить</param>
        /// <param name="purchase">Объект</param>
        public static async void CreateDocument(string source, string outputPath, Purchase purchase)
        {
            await Task.Run(() =>
            {
                Word.Application app = new Word.Application
                {
                    Visible = false
                };
                Word.Document doc = app.Documents.Add(source);
                FindAndReplace(app, "[ИСХ]", purchase.OutputMailNum);
                FindAndReplace(app, "[ЗАКАЗЧИК]", purchase.Customer);
                FindAndReplace(app, "[ДАТА]", Services.GetStringDate(DateTime.Now));
                FindAndReplace(app, "[ОПИСАНИЕ]", purchase.ProcurementObject);
                FindAndReplace(app, "[НОМЕР]", purchase.Number);

                if (purchase.PrepaidExpense.Length < 255)
                    FindAndReplace(app, "[ОИК]", purchase.PrepaidExpense);
                else
                    FindAndReplace(app, "[ОИК]", string.Empty);

                FindAndReplace(app, "[РЕКВИЗИТЫ]", purchase.PaymentDetails);
                doc.SaveAs2(outputPath);

                app.Quit();
            });
        }
        private static void FindAndReplace(Word.Application doc, object findText, object replaceWithText)
        {
            //options
            object matchCase = false;
            object matchWholeWord = true;
            object matchWildCards = false;
            object matchSoundsLike = false;
            object matchAllWordForms = false;
            object forward = true;
            object format = false;
            object matchKashida = false;
            object matchDiacritics = false;
            object matchAlefHamza = false;
            object matchControl = false;
            object read_only = false;
            object visible = true;
            object replace = 2;
            object wrap = 1;
            //execute find and replace
            doc.Selection.Find.Execute(ref findText, ref matchCase, ref matchWholeWord,
                ref matchWildCards, ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText, ref replace,
                ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);

            //var find = doc.Selection.Find;
            // find.Text = findText.ToString(); // текст поиска
            // find.Replacement.Text = replaceWithText.ToString(); // текст замены

            //doc.Selection.Find.Execute(FindText: Type.Missing, MatchCase: false, MatchWholeWord: false, MatchWildcards: false,
            //                MatchSoundsLike: Type.Missing, MatchAllWordForms: false, Forward: true, Wrap: Word.WdFindWrap.wdFindContinue,
            //                Format: false, ReplaceWith: Type.Missing, Replace: Word.WdReplace.wdReplaceAll);
        }
    }
}
