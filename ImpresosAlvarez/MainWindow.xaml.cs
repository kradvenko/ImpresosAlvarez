using ImpresosAlvarez.Entity;
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
        public Usuarios CurrentUser;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Login login = new Login(this);
            login.ShowDialog();
        }

        public void SetMainWindow()
        {
            switch (CurrentUser.tipo)
            {
                case "ADMIN":
                    btnRecepcion.Visibility = Visibility.Visible;
                    btnDiseno.Visibility = Visibility.Visible;
                    btnImpresion.Visibility = Visibility.Visible;
                    btnTerminado.Visibility = Visibility.Visible;
                    btnInventario.Visibility = Visibility.Visible;
                    btnContabilidad.Visibility = Visibility.Visible;
                    btnAdministracion.Visibility = Visibility.Visible;
                    break;
                case "RECEPCION":
                    btnRecepcion.Visibility = Visibility.Visible;
                    break;
                case "INVENTARIO":
                    btnInventario.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
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

        private void btnRecepcion_Click(object sender, RoutedEventArgs e)
        {
            gRecepcion.Visibility = Visibility.Visible;
            gInventario.Visibility = Visibility.Hidden;
        }

        private void btnExistencias_Click(object sender, RoutedEventArgs e)
        {
            Existencias existencia = new Existencias();
            existencia.Show();
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            gRecepcion.Visibility = Visibility.Hidden;
            gInventario.Visibility = Visibility.Visible;
        }

        private void btnEntradas_Click(object sender, RoutedEventArgs e)
        {
            Entradas entrada = new Entradas();
            entrada.Show();
        }

        private void btnSalidas_Click(object sender, RoutedEventArgs e)
        {
            Salidas salida = new Salidas();
            salida.Show();
        }
    }
}
