using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
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
    /// Lógica de interacción para TrabajosEntregados.xaml
    /// </summary>
    public partial class TrabajosEntregados : Window
    {
        String Fecha;
        public TrabajosEntregados()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFecha.SelectedDate = DateTime.Now;
            Fecha = dpFecha.SelectedDate.Value.ToShortDateString();
            CargarOrdenes();
            CargarFacturas();
            CargarCotizaciones();
            CargarComplementos();
        }

        private void CargarOrdenes()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var ordenes = dbContext.Ordenes
                        .Join(
                            dbContext.Clientes,
                            o => o.id_cliente,
                            c => c.id_cliente,
                            (o, c) => new
                            {
                                o.id_orden,
                                o.numero,
                                o.telefono,
                                o.solicitante,
                                o.fecha_solicita,
                                o.quien_recibio,
                                o.inicio_diseno,
                                o.nombre_trabajo,
                                o.cantidad,
                                o.color_tintas,
                                o.tipo_papel,
                                o.con_folio,
                                o.del_numero,
                                o.al_numero,
                                o.tamano,
                                o.otros_1,
                                o.otros_2,
                                o.copia_1,
                                o.copia_2,
                                o.copia_3,
                                o.copia_4,
                                o.otros_3,
                                o.tipo,
                                o.total,
                                o.muestra,
                                o.fecha_muestra,
                                o.prioridad,
                                o.anticipo,
                                o.fecha_entregado,
                                o.pagado,
                                o.inicio_impresion,
                                o.inicio_terminado,
                                o.inicio_por_entregar,
                                o.costo_anterior,
                                o.autorizado,
                                o.hora_solicita,
                                o.ruta,
                                c.id_cliente,
                                c.nombre
                            }
                        )
                       .Where(F => F.fecha_entregado == Fecha)
                       .ToList();

                dgOrdenes.ItemsSource = ordenes;
            }
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
                                c.nombre
                            }
                        )
                       .Where(F => F.fecha == Fecha)
                       .ToList();

                dgCotizaciones.ItemsSource = cotizaciones;
            }
        }

        private void CargarComplementos()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var cotizaciones = dbContext.Parcialidades
                        .Join(
                            dbContext.Facturas,
                            p => p.id_factura,
                            f => f.id_factura,
                            (p, f) => new
                            {
                                p.id_parcialidad,
                                p.id_factura,
                                FechaP = ( ( SqlFunctions.DatePart("dd", p.fecha) > 9 ? (SqlFunctions.DatePart("dd", p.fecha).ToString()) : ("0" + SqlFunctions.DatePart("dd", p.fecha)) ) + "/" + (SqlFunctions.DatePart("mm", p.fecha) > 9 ? (SqlFunctions.DatePart("mm", p.fecha).ToString()) : ("0" + SqlFunctions.DatePart("mm", p.fecha))) + "/" + SqlFunctions.DatePart("yyyy", p.fecha)),
                                p.anterior,
                                p.pagado,
                                p.insoluto,
                                p.parcialidad,
                                p.folio,
                                f.numero
                            }
                        )
                       .Where(F => F.FechaP == Fecha)
                       .ToList();

                dgComplementos.ItemsSource = cotizaciones;
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Fecha = dpFecha.SelectedDate.Value.ToShortDateString();
            CargarOrdenes();
            CargarFacturas();
            CargarCotizaciones();
            CargarComplementos();
        }
    }
}
