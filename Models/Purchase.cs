using System;
using System.Text;
using System.Text.RegularExpressions;

namespace zakupki.gov.ru.Models
{
    /// <summary>
    /// Закупка
    /// </summary>
    public class Purchase
    {
        #region Свойства

        /// <summary>
        /// Номер
        /// </summary>
        public string Number { get; set; }

        private string _determinig = string.Empty;
        /// <summary>
        /// Способ определения
        /// </summary>
        public string Determining
        {
            get
            {
                if (_determinig != null)
                {
                    if (_determinig.Contains("Электронный аукцион"))
                        return "ЭА";
                    else if (_determinig.Contains("Запрос котировок"))
                        return "КОТ";
                    else 
                        return _determinig;
                }
                return _determinig;
            }
            set => _determinig = value;
        }

        private string _platform;
        /// <summary>
        /// Электронная площадка
        /// </summary>
        public string Platform
        {
            get
            {
                if (_platform == null) return string.Empty;
                if (_platform.ToUpper().Contains("РЕСПУБЛИКИ ТАТАРСТАН"))
                    return "АГЗ РТ";
                if (_platform.ToUpper().Contains("ЕДИНАЯ ЭЛЕКТРОННАЯ ТОРГОВАЯ ПЛОЩАДКА"))
                    return "АО «ЕЭТП»";
                if (_platform.ToUpper().Contains("РОССИЙСКИЙ АУКЦИОННЫЙ ДОМ"))
                    return "АО «РАД»";
                if (_platform.ToUpper().Contains("ЭЛЕКТРОННЫЕ ТОРГОВЫЕ СИСТЕМЫ"))
                    return "ЭТП ТЭК-Торг";
                if (_platform.ToUpper().Contains("РТС-ТЕНДЕР"))
                    return "РТС-тендер";
                if (_platform.ToUpper().Contains("ГПБ"))
                    return "ЭТП Газпромбанк";
                return _platform;
            }
            set => _platform = value;
        }
        /// <summary>
        /// Дата и время окончания срока подачи заявок
        /// </summary>
        public string ApplicationDeadline { get; set; }
        /// <summary>
        /// Дата проведения аукциона
        /// </summary>
        public string DateAuction { get; set; }
        /// <summary>
        /// Время проведения аукциона
        /// </summary>
        public string TimeAuction { get; set; }
        /// <summary>
        /// Цена
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// Заказчик
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// Объект закупки
        /// </summary>
        public string ProcurementObject { get; set; }
        /// <summary>
        /// ФЗ
        /// </summary>
        public string FederalLaw { get; set; }
        /// <summary>
        /// Ограничения
        /// </summary>

        private string _restrictions;
        /// <summary>
        /// Ограничения
        /// </summary>
        public string Restrictions
        {
            get
            {
                string temp = string.Empty;
                if (_determinig.Contains("субъекты малого") || _restrictions.Contains("субъектов малого"))
                {
                    temp = "СМП";
                }
                return temp;
            }
            set => _restrictions = value;
        }

        private string _restrictions2;
        /// <summary>
        /// Ограничения 2 колонка
        /// </summary>
        public string Restrictions2
        {
            get
            {

                return _restrictions2;
            }
            set => _restrictions2 = value;
        }
        /// <summary>
        /// Исходящий номер письма
        /// </summary>
        public string OutputMailNum { get; set; }

        private string _timeZone;
        /// <summary>
        /// Часовой пояс
        /// </summary>
        public string TimeZone
        {
            get
            {
                var str = _timeZone.Split('(', ')')[1];
                return str.Trim();
            }

            set => _timeZone = value;
        }

        /// <summary>
        /// Текущий статус
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Платежные реквизиты
        /// </summary>
        private string _paymentDetails = string.Empty;
        public string PaymentDetails
        {
            get
            {
                string result = string.Empty;

                if (Company != null)
                {
                    if (!string.IsNullOrEmpty(Company.INN))
                    {
                        result += $"ИНН {Company.INN}\v";
                    }
                    if (!string.IsNullOrEmpty(Company.KPP))
                    {
                        result += $"КПП {Company.KPP}\v";
                    }
                    if (!string.IsNullOrEmpty(Company.OGRN))
                    {
                        result += $"КПП {Company.OGRN}\v";
                    }
                    result += "\v";

                    string[] detail = _paymentDetails.Split(',');
                    foreach (var item in detail)
                    {
                        result += item.Trim() + "\v";
                    }
                }
                return result;
            }
            set => _paymentDetails = value;
        }

        private string _prepaidExpense = string.Empty;
        /// <summary>
        /// Размер обеспечения исполнения контракта
        /// </summary>
        public string PrepaidExpense
        {
            get
            {

                string result;
                // Если длина меньше 5, то по-любому это только процент
                if (_prepaidExpense.Length < 5 && !string.IsNullOrEmpty(_prepaidExpense))
                {
                    int percent = int.Parse(Regex.Match(_prepaidExpense, @"\d+").Value);
                    // Извлекаем цену
                    var temp = Regex.Replace(Price, @"\s+", "");
                    var temp2 = Regex.Matches(temp, @"\d+\,\d+")[0];
                    double prc = double.Parse(temp2.ToString());
                    prc = RoundUp(prc / 100 * percent, 2);
                    double rub = Math.Truncate(prc);
                    double kop = (prc - rub) * 100;

                    // Дополнительный ноль к копейкам < 10 // D2 не работает почему-то
                    string k;
                    if (kop.ToString().Length < 2)
                        k = "0" + kop.ToString();
                    else
                        k = Math.Round(kop).ToString();

                    string rubString = Services.IntToRusString((int)rub);
                    string ext = "рублей";
                    if (rubString[rubString.Length - 1] != 'ь')
                    {
                        ext = "рубля";
                    }
                    else
                    if (rubString[rubString.Length - 1] != 'н')
                    {
                        ext = "рубль";
                    }

                    result = $"{rub} ({rubString}) {ext} {k} копеек";
                }
                else
                {
                    result = _prepaidExpense;
                }


                return result;
            }
            set => _prepaidExpense = value;
        }

        /// <summary>
        /// Огранизация
        /// </summary>
        public Company Company { get; set; }
        private static double RoundUp(double input, int places)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            return Math.Ceiling(input * multiplier) / multiplier;
        }
        #endregion
    }
}
