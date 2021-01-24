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

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*Login login = new Login();
            login.ShowDialog();*/
        }

        private void btnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }        

        private void btnNotas_Click(object sender, RoutedEventArgs e)
        {
            VerCotizaciones cotizaciones = new VerCotizaciones();
            cotizaciones.Show();
        }

        private void btnFacturaDigital_Click(object sender, RoutedEventArgs e)
        {
            FacturaDigital factura = new FacturaDigital();
            factura.Show();
        }

        private void btnComplemento_Click(object sender, RoutedEventArgs e)
        {
            ComplementoPago complemento = new ComplementoPago();
            complemento.Show();
        }

        private void btnControlFacturas_Click(object sender, RoutedEventArgs e)
        {
            ControlFacturas control = new ControlFacturas();
            control.Show();
        }

        private void btnControlComplementos_Click(object sender, RoutedEventArgs e)
        {
            ControlComplementos control = new ControlComplementos();
            control.Show();
        }

        private void btnCalculadora_Click(object sender, RoutedEventArgs e)
        {
            Calculadora calc = new Calculadora();
            calc.Show();
        }
    }
}
