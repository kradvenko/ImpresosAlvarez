using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
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
    /// Lógica de interacción para OrdenesSinPago.xaml
    /// </summary>
    public partial class OrdenesSinPago : Window
    {
        public OrdenesSinPago()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void cbCanceladas_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void cbRecepcion_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void cbDiseño_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void cbImpresion_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void cbTerminado_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void cbPorEntregar_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void cbEntregadas_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes();
        }

        private void BuscarOrdenes()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                //_facturas = dbContext.Facturas.Where(F => F.IdCliente == _clienteElegido.IdCliente).ToList();
                /*
                var ords = dbContext.Ordenes
                    .Join(
                        dbContext.Clientes,
                        o => o.id_cliente,
                        c => c.id_cliente,
                        (o, c) => new
                        {
                            o.id_orden,
                            o.numero,
                            o.id_cliente,
                            o.fecha_solicita,
                            o.quien_recibio,
                            o.nombre_trabajo,
                            o.cantidad,
                            o.estado,
                            o.fecha_entregado,
                            c.nombre
                        }
                    )
                    .GroupJoin(
                        dbContext.FacturaOrden,
                        comb => comb.id_orden,
                        fo => fo.id_orden,
                        (comb, fo) => new
                        {
                            comb.id_orden,
                            comb.numero,
                            comb.id_cliente,
                            comb.fecha_solicita,
                            comb.quien_recibio,
                            comb.nombre_trabajo,
                            comb.cantidad,
                            comb.estado,
                            comb.fecha_entregado,                            
                            Cliente = comb.nombre,
                            sub = fo
                        }
                    )
                    .Join(
                        dbContext.Facturas,
                        comb => comb.id_factura,
                        f => f.id_factura,
                        (comb, f) => new
                        {
                            comb.id_orden,
                            comb.numero,
                            comb.id_cliente,
                            comb.fecha_solicita,
                            comb.quien_recibio,
                            comb.nombre_trabajo,
                            comb.cantidad,
                            comb.estado,
                            comb.fecha_entregado,
                            comb.Cliente,
                            comb.
                        }
                    )
                    .Where(F => F)
                    .ToList();
                */

                var ords = dbContext.Ordenes
                    .Join(
                        dbContext.Clientes,
                        o => o.id_cliente,
                        c => c.id_cliente,
                        (o, c) => new
                        {
                            o.id_orden,
                            o.numero,
                            o.id_cliente,
                            o.fecha_solicita,
                            o.quien_recibio,
                            o.nombre_trabajo,
                            o.cantidad,
                            o.estado,
                            o.fecha_entregado,
                            c.nombre
                        }
                    )
                    .GroupJoin(
                        dbContext.FacturaOrden,
                        orden => orden.id_orden,
                        fo => fo.id_orden,
                        (orden, fo) => new
                        {
                            orden.id_orden,
                            orden.numero,
                            orden.id_cliente,
                            orden.fecha_solicita,
                            orden.quien_recibio,
                            orden.nombre_trabajo,
                            orden.cantidad,
                            orden.estado,
                            orden.fecha_entregado,
                            orden.nombre,
                            sub = fo
                        }
                    )
                    .SelectMany(
                        joinedSet => joinedSet.sub.DefaultIfEmpty(),
                        (orden, sub) => new
                        {
                            orden.id_orden,
                            orden.numero,
                            orden.id_cliente,
                            orden.fecha_solicita,
                            orden.quien_recibio,
                            orden.nombre_trabajo,
                            orden.cantidad,
                            orden.estado,
                            orden.fecha_entregado,
                            orden.nombre,
                            sub
                        }
                    )
                    .Where(T => T.sub.id_facturaorden == null)
                    .GroupJoin(
                        dbContext.NotaOrden,
                        orden => orden.id_orden,
                        no => no.id_orden,
                        (orden, no) => new
                        {
                            orden.id_orden,
                            orden.numero,
                            orden.id_cliente,
                            orden.fecha_solicita,
                            orden.quien_recibio,
                            orden.nombre_trabajo,
                            orden.cantidad,
                            orden.estado,
                            orden.fecha_entregado,
                            orden.nombre,
                            subn = no
                        }
                    )
                    .SelectMany(
                        joinedSet => joinedSet.subn.DefaultIfEmpty(),
                        (orden, subn) => new
                        {
                            orden.id_orden,
                            orden.numero,
                            orden.id_cliente,
                            orden.fecha_solicita,
                            orden.quien_recibio,
                            orden.nombre_trabajo,
                            orden.cantidad,
                            orden.estado,
                            orden.fecha_entregado,
                            orden.nombre,
                            subn
                        }
                    )
                    .Where(T => T.subn.id_notaorden == null)
                    .OrderByDescending(F => F.id_orden)
                    .ToList();

                var ordsf = ords;

                if (cbCanceladas.IsChecked == false)
                {
                    ordsf = ords.Where(E => E.estado != "CANCELADA").ToList();
                }

                if (cbRecepcion.IsChecked == false)
                {
                    ordsf = ordsf.Where(E => E.estado != "RECEPCION").ToList();
                }

                if (cbDiseño.IsChecked == false)
                {
                    ordsf = ordsf.Where(E => E.estado != "DISEÑO").ToList();
                }

                if (cbImpresion.IsChecked == false)
                {
                    ordsf = ordsf.Where(E => E.estado != "IMPRESION").ToList();
                }

                if (cbTerminado.IsChecked == false)
                {
                    ordsf = ordsf.Where(E => E.estado != "TERMINADO").ToList();
                }

                if (cbPorEntregar.IsChecked == false)
                {
                    ordsf = ordsf.Where(E => E.estado != "POR ENTREGAR").ToList();
                }

                if (cbEntregadas.IsChecked == false)
                {
                    ordsf = ordsf.Where(E => E.estado != "ENTREGADO").ToList();
                }

                int ordenesCanceladas = ords.Where(O => O.estado == "CANCELADA").ToList().Count();
                int ordenesRecepcion = ords.Where(O => O.estado == "RECEPCION").ToList().Count();
                int ordenesDiseño = ords.Where(O => O.estado == "DISEÑO").ToList().Count();
                int ordenesImpresion = ords.Where(O => O.estado == "IMPRESION").ToList().Count();
                int ordenesTerminado = ords.Where(O => O.estado == "TERMINADO").ToList().Count();
                int ordenesPorEntregar = ords.Where(O => O.estado == "POR ENTREGAR").ToList().Count();
                int ordenesEntregadas = ords.Where(O => O.estado == "ENTREGADO").ToList().Count();

                dgOrdenes.ItemsSource = ordsf;
                lblTotal.Content = "TOTAL ORDENES SIN FACTURA: " + ordsf.Count;
                lblTotalCanceladas.Content = "CANCELADAS " + ordenesCanceladas;
                lblTotalRecepcion.Content = "RECEPCIÓN " + ordenesRecepcion;
                lblTotalDiseño.Content = "DISEÑO " + ordenesDiseño;
                lblTotalImpresion.Content = "IMPRESIÓN " + ordenesImpresion;
                lblTotalTerminado.Content = "TERMINADO " + ordenesTerminado;
                lblTotalPorEntregar.Content = "POR ENTREGAR " + ordenesPorEntregar;
                lblTotalEntregadas.Content = "ENTREGADO " + ordenesEntregadas;
            }
        }
    }
}
