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
    /// Lógica de interacción para NotasPendientesPorPagar.xaml
    /// </summary>
    public partial class NotasPendientesPorPagar : Window
    {
        class CotizacionPendiente
        {
            public int id_nota { get; set; }
            public string numero { get; set; }
            public string fecha { get; set; }
            public double pagado { get; set; }
            public double total { get; set; }
            public int id_cliente { get; set; }
            public string nombre { get; set; }
            public string estado { get; set; }
        }

        public NotasPendientesPorPagar()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var cotizacionesPendientesPre = dbContext.Notas
                    .Where(c => c.pagada == "NO" && c.estado == "ACTIVO")
                    .Select(c => new
                    {
                        id_nota = c.id_nota,
                        numero = c.numero,
                        fecha = c.fecha,
                        solicita = c.solicita,
                        total = (double)c.total,
                        pagado = 0,
                        id_cliente = (int)c.id_cliente
                    })
                    .Join(dbContext.Clientes,
                        c => c.id_cliente,
                        cl => cl.id_cliente,
                        (c, cl) => new CotizacionPendiente
                        {
                            id_nota = c.id_nota,
                            numero = c.numero,
                            fecha = c.fecha,
                            total = c.total,
                            pagado = c.pagado,
                            id_cliente = c.id_cliente,
                            nombre = cl.nombre.Contains("VARIOS") ? (cl.nombre + " / " + c.solicita) : cl.nombre
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
                            
                            if (c.pagado >= c.total)
                            {
                                c.estado = "PAGADA";
                            }
                            else if (c.pagado > 0 && c.pagado < c.total)
                            {
                                c.estado = "PAGO PARCIAL";
                            }
                        });
                    }
                    else
                    {
                        cotizacionesPendientesPre.Where(c => c.id_nota == cotizacion.id_nota).ToList().ForEach(c =>
                        {
                            c.estado = "SIN PAGO";
                        });
                    }
                }

                dgCotizaciones.ItemsSource = cotizacionesPendientesPre;
                lblTotalCotizaciones.Content = "Total Notas: " + cotizacionesPendientesPre.Count.ToString();
            }
        }
        private void dgCotizaciones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCotizaciones.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una cotización para agregarla a la lista de pagos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.Close();
        }
    }
}