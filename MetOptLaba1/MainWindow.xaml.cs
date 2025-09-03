using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetOptLaba1
{
    public partial class MainWindow : Window
    {
        // В тетрадочке дизайн первого таба. Гит ещё сделай
        public MainWindow()
        {
            InitializeComponent();
        }

        private void grid_Loaded(object sender, RoutedEventArgs e)
        {
            target_func_grid.ItemsSource = new List<aba> { new aba { Id = 1, Name = "aba" }, new aba { Id = 2, Name = "ABAaaaaaaaaaaaaaaaaaaa" } };
        }
    }

    class aba
    {
        public int Id {  get; set; }
        public string Name { get; set; }
    }
}
