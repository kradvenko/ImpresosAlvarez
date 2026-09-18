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
using System.Windows.Shapes;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para PagosPendientesCliente.xaml
    /// </summary>
    public partial class PagosPendientesCliente : Window
    {
        class FacturaPendiente
        {
            public int id_factura { get; set; }
            public string numero { get; set; }
            public string fecha { get; set; }
            public double pagado { get; set; }
            public double total { get; set; }
        }
        
        class CotizacionPendiente
        {
            public int id_nota { get; set; }
            public string numero { get; set; }
            public string fecha { get; set; }
            public double pagado { get; set; }
            public double total { get; set; }
        }
        Clientes cliente;
        CorteDiario parentWindow;
        public PagosPendientesCliente(Clientes cliente, CorteDiario parentWindow)
        {
            InitializeComponent();
            this.cliente = cliente;
            this.parentWindow = parentWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var facturasPendientesPre = dbContext.Facturas                    
                    .Where(f => f.id_cliente == cliente.id_cliente && f.pagada == "NO" && f.estado == "ACTIVO")
                    .Select(f => new FacturaPendiente
                    {
                        id_factura = f.id_factura,
                        numero = f.numero,
                        fecha = f.fecha,
                        pagado = 0,
                        total = (double)f.total
                    })
                    .ToList();

                foreach (var factura in facturasPendientesPre)
                {
                    var pagos = dbContext.Pagos
                        .Where(p => p.id_factura == factura.id_factura)
                        .Sum(p => (double?)p.cantidad) ?? 0;
                    var pagado = pagos;
                    if (pagado > 0)
                    {
                        facturasPendientesPre.Where(f => f.id_factura == factura.id_factura).ToList().ForEach(f =>
                        {
                            f.pagado = pagado;
                        });
                    }
                }

                dgFacturas.ItemsSource = facturasPendientesPre;

                var cotizacionesPendientesPre = dbContext.Notas
                    .Where(c => c.id_cliente == cliente.id_cliente && c.pagada == "NO" && c.estado == "ACTIVO")
                    .Select(c => new CotizacionPendiente
                    {
                        id_nota = c.id_nota,
                        numero = c.numero,
                        fecha = c.fecha,
                        total = (double)c.total,
                        pagado = 0
                    })
                    .ToList();

                foreach (var cotizacion in cotizacionesPendientesPre)
                {
                    var pagos = dbContext.PagosNotas
                        .Where(p => p.id_nota == cotizacion.id_nota)
                        .Sum(p => (double?)p.cantidad) ?? 0;
                    var pagado = pagos;
                    if (pagado > 0)
                    {
                        cotizacionesPendientesPre.Where(c => c.id_nota == cotizacion.id_nota).ToList().ForEach(c =>
                        {
                            c.pagado = pagado;
                        });
                    }
                }

                dgCotizaciones.ItemsSource = cotizacionesPendientesPre  ;
            }
        }

        private void dgFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgFacturas.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una factura para agregarla a la lista de pagos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            parentWindow.AgregarFacturaPasada(((FacturaPendiente)dgFacturas.SelectedItem).id_factura);
            this.Close();
        }

        private void dgCotizaciones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCotizaciones.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una cotización para agregarla a la lista de pagos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }   
            parentWindow.AgregarCotizacionPasada(((CotizacionPendiente)dgCotizaciones.SelectedItem).id_nota);
            this.Close();
        }
    }
}
