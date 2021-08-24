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

namespace zakupki.gov.ru
{
    /// <summary>
    /// Логика взаимодействия для License.xaml
    /// </summary>
    public partial class License : Window
    {
        public License()
        {
            InitializeComponent();
        }

        private void ActivateApplication(object sender, RoutedEventArgs e)
        {
            string activationKey = TB_Key.Text;
     //       string licenseKey = TB_License.Text;
     //       int length = activationKey.Length;
      //      if (length == 24 || length == 44 || length == 64 || length == 88 || length == 108 || length == 128)
      //      {
                if (!Cryptographer.CheckActivationApp(activationKey))
                {
                    MessageBox.Show("Введен не верный код активации", "Ошибка активации", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Settings settings = new Settings().Load();
                    settings.License = activationKey;
                    settings.Save();
                    Close();
                    MessageBox.Show("Приложение успешно активировано. Перезапустите его", "Активация успешна", MessageBoxButton.OK, MessageBoxImage.Information);
                }
       //     }
       //     else
       //     {
       //         MessageBox.Show("Введен не верный код активации", "Ошибка активации", MessageBoxButton.OK, MessageBoxImage.Warning);
       //     }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TB_License.Text = Cryptographer.GetLicenseKey();
        }


    }
}
