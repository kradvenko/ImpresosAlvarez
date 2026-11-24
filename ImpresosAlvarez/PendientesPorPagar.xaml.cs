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
    /// Lógica de interacción para PendientesPorPagar.xaml
    /// </summary>
    public partial class PendientesPorPagar : Window
    {
        class FacturaPendiente
        {
            public int id_factura { get; set; }
            public string numero { get; set; }
            public string fecha { get; set; }
            public double pagado { get; set; }
            public double total { get; set; }
            public int id_cliente { get; set; }
            public string nombre { get; set; }
            public string estado { get; set; }
        }

        class CotizacionPendiente
        {
            public int id_nota { get; set; }
            public string numero { get; set; }
            public string fecha { get; set; }
            public double pagado { get; set; }
            public double total { get; set; }
            public int id_cliente { get; set; }
            public string nombre { get; set; }
        }
        
        public PendientesPorPagar()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var facturasPendientesPre = dbContext.Facturas
                    .Where(f => f.pagada == "NO" && f.estado == "ACTIVO")
                    .Select(f => new
                    {
                        id_factura = f.id_factura,
                        numero = f.numero,
                        fecha = f.fecha,
                        pagado = 0,
                        total = (double)f.total,
                        id_cliente = f.id_cliente
                    })
                    .Join(dbContext.Clientes,
                        f => f.id_cliente,
                        c => c.id_cliente,
                        (f, c) => new FacturaPendiente
                        {
                            id_factura = f.id_factura,
                            numero = f.numero,
                            fecha = f.fecha,
                            pagado = f.pagado,
                            total = f.total,
                            id_cliente = f.id_cliente,
                            nombre = c.nombre
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
                            if (f.pagado >= f.total)
                            {
                                f.estado = "PAGADA";                                
                            }
                            else
                            {
                                f.estado = "PAGO PARCIAL";
                            }
                        });
                    }
                    else
                    {
                        facturasPendientesPre.Where(f => f.id_factura == factura.id_factura).ToList().ForEach(f =>
                        {
                            f.estado = "SIN PAGO";
                        });
                    }
                }

                dgFacturas.ItemsSource = facturasPendientesPre;       
                lblTotalFacturas.Content = "Total Facturas: " + facturasPendientesPre.Count.ToString();
            }
        }

        private void dgFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgFacturas.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una factura para agregarla a la lista de pagos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.Close();
        }        
    }
}
