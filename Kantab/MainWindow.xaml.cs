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
using Kantab.Classes;
using Kantab.Classes.ViewModels;

namespace Kantab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private KantabServer ks;
        public MainWindow() {
            PointerData dt = new PointerData();
            DataContext = dt;
            InitializeComponent();
            ks = new KantabServer(null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ks.BeginHttpServer();
        }
    }
}
