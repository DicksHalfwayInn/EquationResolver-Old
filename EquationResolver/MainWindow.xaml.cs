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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EquationResolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;
        public MainWindow()
        {
            InitializeComponent();

            vm = new MainWindowViewModel();

            DataContext= vm;
        }

        private void EquationEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                var textBox = sender as TextBox;

                var s = textBox.Text;

                vm.TextChanged(s);
            }
        }
    }
}
