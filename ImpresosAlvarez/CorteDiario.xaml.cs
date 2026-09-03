using ImpresosAlvarez.Clases;
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
    /// Lógica de interacción para CorteDiario.xaml
    /// </summary>
    public partial class CorteDiario : Window
    {
        String Fecha;        
        public CorteDiario()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFecha.SelectedDate = DateTime.Now;
            Fecha = dpFecha.SelectedDate.Value.ToShortDateString();
            CargarFacturas();
            CargarCotizaciones();
        }

        private void CargarFacturas()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var facturas = dbContext.Facturas
                        .Join(
                            dbContext.Clientes,
                            f => f.id_cliente,
                            c => c.id_cliente,
                            (f, c) => new
                            {
                                f.id_factura,
                                f.id_cliente,
                                f.id_contribuyente,
                                f.subtotal,
                                f.total,
                                f.pagada,
                                f.estado,
                                f.fecha,
                                f.numero,
                                f.razon_cancelado,
                                f.amparada_por,
                                c.nombre
                            }
                        )
                        .Join(
                            dbContext.Contribuyentes,
                            f => f.id_contribuyente,
                            co => co.id_contribuyente,
                            (f, co) => new
                            {
                                f.id_factura,
                                f.id_cliente,
                                f.id_contribuyente,
                                f.subtotal,
                                f.total,
                                f.pagada,
                                f.estado,
                                f.fecha,
                                f.numero,
                                f.razon_cancelado,
                                f.amparada_por,
                                f.nombre,
                                NombreContribuyente = co.nombre.Substring(0, co.nombre.IndexOf(" ")),
                                id_entrega = 0,
                                entrego = "",
                                referencia = "",
                                observaciones = ""
                            }
                        )
                       .Where(F => F.fecha == Fecha)
                       .ToList()
                       .Select(f => new FacturaViewModel {
                           id_factura = f.id_factura,
                           id_cliente = f.id_cliente,
                           id_contribuyente = f.id_contribuyente,
                           subtotal = (decimal)f.subtotal,
                           total = (decimal)f.total,
                           pagada = f.pagada,
                           estado = f.estado,
                           fecha = f.fecha,
                           numero = f.numero,
                           nombre = f.nombre,
                           NombreContribuyente = f.NombreContribuyente,
                           id_entrega = f.id_entrega,
                           entrego = f.entrego,
                           referencia = f.referencia,
                           observaciones = f.observaciones
                       })
                       .ToList();

                dgFacturas.ItemsSource = facturas;
            }
        }

        private void CargarCotizaciones()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var cotizaciones = dbContext.Notas
                        .Join(
                            dbContext.Clientes,
                            f => f.id_cliente,
                            c => c.id_cliente,
                            (f, c) => new
                            {
                                f.id_nota,
                                f.id_cliente,
                                f.total,
                                f.pagada,
                                f.estado,
                                f.fecha,
                                f.numero,
                                f.solicita,
                                c.nombre,
                                NombreUnificado = c.nombre.Contains("VARIOS") ? (c.nombre + " / " + f.solicita) : c.nombre,
                                id_entrega = 0,
                                entrego = "",
                                referencia = "",
                                observaciones = ""
                            }
                        )
                       .Where(F => F.fecha == Fecha)
                       .ToList();

                dgCotizaciones.ItemsSource = cotizaciones;
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFecha.SelectedDate.HasValue)
            {
                Fecha = dpFecha.SelectedDate.Value.ToShortDateString();
                CargarFacturas();
                CargarCotizaciones();
            }

        }

        private void dgFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgFacturas.SelectedItem != null)
            {
                FacturaViewModel selectedFactura = dgFacturas.SelectedItem as FacturaViewModel;
                if (selectedFactura.entrego == null)
                {
                    selectedFactura.entrego = "";
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedFactura, "Factura", this, "");
                    detalleWindow.ShowDialog();
                }
                else
                {
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedFactura, "Factura", this, "EDITAR");
                    detalleWindow.ShowDialog();
                }                
            }
        }
        public void ActualizarFactura(FacturaViewModel facturaActualizada)
        {
            var facturas = dgFacturas.ItemsSource as List<FacturaViewModel>;
            if (facturas != null)
            {
                int index = facturas.FindIndex(f => f.id_factura == facturaActualizada.id_factura);
                if (index >= 0)
                {
                    facturas[index] = facturaActualizada;
                    dgFacturas.ItemsSource = null;
                    dgFacturas.ItemsSource = facturas;
                }
            }
        }
    }
}
