using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using Syncfusion.Windows.Controls.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

            //04/01/2023
            //Actualizaciones.Actualizacion1();
            //15/06/2025
            //Actualizaciones.Actualizacion2();
            //08/10/2025
            //Actualizaciones.Actualizacion3();
            //10/12/2025
            Actualizaciones.Actualizacion4();
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

                    gRecepcion.Visibility = Visibility.Hidden;
                    gImpresion.Visibility = Visibility.Hidden;
                    gTerminado.Visibility = Visibility.Hidden;
                    gDiseno.Visibility = Visibility.Hidden;
                    gInventario.Visibility = Visibility.Hidden;
                    break;
                case "RECEPCION":
                    btnRecepcion.Visibility = Visibility.Visible;
                    btnDiseno.Visibility = Visibility.Hidden;
                    btnImpresion.Visibility = Visibility.Hidden;
                    btnTerminado.Visibility = Visibility.Hidden;
                    btnInventario.Visibility = Visibility.Hidden;
                    btnContabilidad.Visibility = Visibility.Hidden;
                    btnAdministracion.Visibility = Visibility.Hidden;

                    gRecepcion.Visibility = Visibility.Visible;
                    gImpresion.Visibility = Visibility.Hidden;
                    gTerminado.Visibility = Visibility.Hidden;
                    gDiseno.Visibility = Visibility.Hidden;
                    gInventario.Visibility = Visibility.Hidden;
                    break;
                case "INVENTARIO":
                    btnRecepcion.Visibility = Visibility.Hidden;
                    btnDiseno.Visibility = Visibility.Hidden;
                    btnImpresion.Visibility = Visibility.Hidden;
                    btnTerminado.Visibility = Visibility.Hidden;
                    btnInventario.Visibility = Visibility.Visible;
                    btnContabilidad.Visibility = Visibility.Hidden;
                    btnAdministracion.Visibility = Visibility.Hidden;

                    gRecepcion.Visibility = Visibility.Hidden;
                    gImpresion.Visibility = Visibility.Hidden;
                    gTerminado.Visibility = Visibility.Hidden;
                    gDiseno.Visibility = Visibility.Hidden;
                    gInventario.Visibility = Visibility.Visible;
                    break;
                case "IMPRESION":
                    btnRecepcion.Visibility = Visibility.Hidden;
                    btnDiseno.Visibility = Visibility.Hidden;
                    btnImpresion.Visibility = Visibility.Visible;
                    btnTerminado.Visibility = Visibility.Hidden;
                    btnInventario.Visibility = Visibility.Hidden;
                    btnContabilidad.Visibility = Visibility.Hidden;
                    btnAdministracion.Visibility = Visibility.Hidden;

                    gRecepcion.Visibility = Visibility.Hidden;
                    gImpresion.Visibility = Visibility.Visible;
                    gTerminado.Visibility = Visibility.Hidden;
                    gDiseno.Visibility = Visibility.Hidden;
                    gInventario.Visibility = Visibility.Hidden;
                    break;
                case "TERMINADO":
                    btnRecepcion.Visibility = Visibility.Hidden;
                    btnDiseno.Visibility = Visibility.Hidden;
                    btnImpresion.Visibility = Visibility.Hidden;
                    btnTerminado.Visibility = Visibility.Visible;
                    btnInventario.Visibility = Visibility.Hidden;
                    btnContabilidad.Visibility = Visibility.Hidden;
                    btnAdministracion.Visibility = Visibility.Hidden;

                    gRecepcion.Visibility = Visibility.Hidden;
                    gImpresion.Visibility = Visibility.Hidden;
                    gTerminado.Visibility = Visibility.Visible;
                    gDiseno.Visibility = Visibility.Hidden;
                    gInventario.Visibility = Visibility.Hidden;
                    break;
                case "TALLER":
                    btnRecepcion.Visibility = Visibility.Hidden;
                    btnDiseno.Visibility = Visibility.Hidden;
                    btnImpresion.Visibility = Visibility.Visible;
                    btnTerminado.Visibility = Visibility.Visible;
                    btnInventario.Visibility = Visibility.Hidden;
                    btnContabilidad.Visibility = Visibility.Hidden;
                    btnAdministracion.Visibility = Visibility.Hidden;

                    gRecepcion.Visibility = Visibility.Hidden;
                    gImpresion.Visibility = Visibility.Hidden;
                    gTerminado.Visibility = Visibility.Hidden;
                    gDiseno.Visibility = Visibility.Hidden;
                    gInventario.Visibility = Visibility.Hidden;
                    break;
                default:
                    break;
            }

            try
            {
                string curFile = @"C:\Impresos\Facturacion\XML_4_0_Template_Cancelacion.xml";
                //MessageBox.Show(File.Exists(curFile) ? "File exists." : "File does not exist.");
                if (!File.Exists(curFile))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("http://impresosalvarez.atwebpages.com/XML_4_0_Template_Cancelacion.xml", @"C:\Impresos\Facturacion\XML_4_0_Template_Cancelacion.xml");
                    }
                }

                string orden2File = @"C:\Impresos\orden2File.png";
                if (!File.Exists(orden2File))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("http://impresosalvarez.atwebpages.com/orden2File.png", @"C:\Impresos\orden2File.png");
                    }
                }

                string excelFile = @"C:\Impresos\File.xlsx";
                if (!File.Exists(excelFile))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("http://impresosalvarez.atwebpages.com/File.xlsx", @"C:\Impresos\File.xlsx");
                    }
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
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
            gImpresion.Visibility = Visibility.Hidden;
            gInventario.Visibility = Visibility.Hidden;
            gTerminado.Visibility = Visibility.Hidden;
            gAdministracion.Visibility = Visibility.Hidden;
        }

        private void btnExistencias_Click(object sender, RoutedEventArgs e)
        {
            Existencias existencia = new Existencias();
            existencia.Show();
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            gRecepcion.Visibility = Visibility.Hidden;
            gImpresion.Visibility = Visibility.Hidden;
            gInventario.Visibility = Visibility.Visible;
            gTerminado.Visibility = Visibility.Hidden;
            gAdministracion.Visibility = Visibility.Hidden;
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

        private void btnImpresion_Click(object sender, RoutedEventArgs e)
        {
            gRecepcion.Visibility = Visibility.Hidden;
            gImpresion.Visibility = Visibility.Visible;
            gInventario.Visibility = Visibility.Hidden;
            gTerminado.Visibility = Visibility.Hidden;
            gAdministracion.Visibility = Visibility.Hidden;
        }

        private void btnTrabajosEnImpresion_Click(object sender, RoutedEventArgs e)
        {
            TrabajosImpresion pendientes = new TrabajosImpresion();
            pendientes.ShowDialog();
        }

        private void btnDiseno_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnDisenoPendientes_Click(object sender, RoutedEventArgs e)
        {
            DisenoOrdenesPendientes pendientes = new DisenoOrdenesPendientes();
            pendientes.ShowDialog();
        }

        private void btnNuevaOrden_Click(object sender, RoutedEventArgs e)
        {
            NuevaOrden orden = new NuevaOrden();
            orden.Show();
        }

        private void btnReimprimirOrden_Click(object sender, RoutedEventArgs e)
        {
            ReimprimirOrden Re = new ReimprimirOrden();
            Re.ShowDialog();
        }

        private void btnTrabajosEntregados_Click(object sender, RoutedEventArgs e)
        {
            TrabajosEntregados entregados = new TrabajosEntregados();
            entregados.Show();
        }

        private void btnServiciosProductos_Click(object sender, RoutedEventArgs e)
        {
            ControlServiciosProductos servicios = new ControlServiciosProductos();
            servicios.Show();
        }

        private void btnTrabajosEnTerminado_Click(object sender, RoutedEventArgs e)
        {
            TrabajosTerminado pendienste = new TrabajosTerminado();
            pendienste.ShowDialog();
        }

        private void btnTerminado_Click(object sender, RoutedEventArgs e)
        {
            gRecepcion.Visibility = Visibility.Hidden;
            gImpresion.Visibility = Visibility.Hidden;
            gInventario.Visibility = Visibility.Hidden;
            gTerminado.Visibility = Visibility.Visible;
            gAdministracion.Visibility = Visibility.Hidden;
        }

        private void btnControlOrdenes_Click(object sender, RoutedEventArgs e)
        {
            ControlOrdenes ordenes = new ControlOrdenes();
            ordenes.Show();
        }

        private void btnSesion_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login(this);
            login.ShowDialog();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnControlUsuarios_Click(object sender, RoutedEventArgs e)
        {
            ControlUsuarios usuarios = new ControlUsuarios();
            usuarios.ShowDialog();
        }

        private void btnAdministracion_Click(object sender, RoutedEventArgs e)
        {
            gRecepcion.Visibility = Visibility.Hidden;
            gImpresion.Visibility = Visibility.Hidden;
            gTerminado.Visibility = Visibility.Hidden;
            gDiseno.Visibility = Visibility.Hidden;
            gInventario.Visibility = Visibility.Hidden;
            gAdministracion.Visibility = Visibility.Visible;
        }

        private void btnSeguimiento_Click(object sender, RoutedEventArgs e)
        {
            SeguimientoOrdenes seg = new SeguimientoOrdenes(CurrentUser);
            seg.ShowDialog();
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            ControlClientes clientes = new ControlClientes();
            clientes.ShowDialog();
        }

        private void btnOrdenesSinFactura_Click(object sender, RoutedEventArgs e)
        {
            OrdenesSinPago ordenes = new OrdenesSinPago();
            ordenes.Show();
        }

        private void btnControlNotas_Click(object sender, RoutedEventArgs e)
        {
            ControlNotas notas = new ControlNotas();
            notas.ShowDialog();
        }

        private void btnClientesSinOrdenes_Click(object sender, RoutedEventArgs e)
        {
            ClientesSinOrdenes cso = new ClientesSinOrdenes();
            cso.ShowDialog();
        }
    }
}
