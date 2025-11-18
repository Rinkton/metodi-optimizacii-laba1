using System;
using System.Windows;

namespace MetOptLaba1
{
    public class UserError
    {
        public static void Show(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
