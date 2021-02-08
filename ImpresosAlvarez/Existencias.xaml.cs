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

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para Existencias.xaml
    /// </summary>
    public partial class Existencias : Window
    {
        public Existencias()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnNuevoInsumo_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
