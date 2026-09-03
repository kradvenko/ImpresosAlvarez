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
                                entrego = "",
                                referencia = "",
                                observaciones = ""
                            }
                        )
                       .Where(F => F.fecha == Fecha)
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

        }
    }
}
