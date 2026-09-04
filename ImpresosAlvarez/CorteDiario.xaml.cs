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
                           id_corte_diario = 0,
                           id_factura = f.id_factura,
                           id_nota = 0,
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

                foreach (FacturaViewModel factura in facturas)
                {
                    var corte = dbContext.CorteDiario.FirstOrDefault(c => c.id_factura == factura.id_factura);
                    if (corte != null)
                    {
                        factura.id_corte_diario = corte.id_corte_diario;
                        factura.entrego = corte.entrega;
                        factura.referencia = corte.referencia;
                        factura.observaciones = corte.observaciones;
                    }
                }

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
                       .ToList()
                       .Select(f => new FacturaViewModel
                       {
                           id_corte_diario = 0,
                           id_factura = 0,
                           id_nota = f.id_nota,
                           id_cliente = (int)f.id_cliente,
                           id_contribuyente = 0,
                           subtotal = 0,
                           total = (decimal)f.total,
                           pagada = f.pagada,
                           estado = f.estado,
                           fecha = f.fecha,
                           numero = f.numero,
                           nombre = f.nombre,
                           NombreContribuyente = f.NombreUnificado,
                           id_entrega = f.id_entrega,
                           entrego = f.entrego,
                           referencia = f.referencia,
                           observaciones = f.observaciones
                       })
                       .ToList();

                foreach (FacturaViewModel cotizacion in cotizaciones)
                {
                    var corte = dbContext.CorteDiario.FirstOrDefault(c => c.id_nota == cotizacion.id_nota);
                    if (corte != null)
                    {
                        cotizacion.id_corte_diario = corte.id_corte_diario;
                        cotizacion.entrego = corte.entrega;
                        cotizacion.referencia = corte.referencia;
                        cotizacion.observaciones = corte.observaciones;
                    }
                }

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

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            foreach (FacturaViewModel factura in dgFacturas.ItemsSource)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    if (factura.id_corte_diario == 0)
                    {
                        Entity.CorteDiario corte = new Entity.CorteDiario();

                        corte.numero = factura.numero;
                        corte.cliente = factura.nombre;
                        corte.contribuyente = factura.NombreContribuyente;
                        corte.subtotal = (double?)factura.subtotal;
                        corte.total = (double?)factura.total;
                        corte.referencia = factura.referencia;
                        corte.id_entrega = factura.id_entrega;
                        corte.entrega = factura.entrego;
                        corte.observaciones = factura.observaciones;
                        corte.tipo = "FACTURA";
                        corte.fecha_pago = Fecha;
                        if (factura.referencia == "FIRMO" || factura.referencia == "")
                        {
                            corte.aplicado = "NO";
                            corte.fecha_aplicado = "";
                        }
                        else
                        {
                            corte.aplicado = "SI";
                            corte.fecha_aplicado = Fecha;
                        }
                        corte.id_factura = factura.id_factura;
                        corte.id_nota = 0;

                        dbContext.CorteDiario.Add(corte);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        Entity.CorteDiario corteExistente = dbContext.CorteDiario.FirstOrDefault(c => c.id_corte_diario == factura.id_corte_diario);
                        if (corteExistente == null)
                        {

                        }
                        else
                        {
                            corteExistente.referencia = factura.referencia;
                            corteExistente.id_entrega = factura.id_entrega;
                            corteExistente.entrega = factura.entrego;
                            corteExistente.observaciones = factura.observaciones;
                            if (factura.referencia == "FIRMO" || factura.referencia == "")
                            {
                                corteExistente.aplicado = "NO";
                                corteExistente.fecha_aplicado = "";
                            }
                            else
                            {
                                corteExistente.aplicado = "SI";
                                corteExistente.fecha_aplicado = Fecha;
                            }
                            dbContext.SaveChanges();
                        }
                    }
                }
            }

            foreach (FacturaViewModel cotizacion in dgCotizaciones.ItemsSource)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    if (cotizacion.id_corte_diario == 0)
                    {
                        Entity.CorteDiario corte = new Entity.CorteDiario();
                        corte.numero = cotizacion.numero;
                        corte.cliente = cotizacion.nombre;
                        corte.contribuyente = "";
                        corte.subtotal = (double?)cotizacion.subtotal;
                        corte.total = (double?)cotizacion.total;
                        corte.referencia = cotizacion.referencia;
                        corte.id_entrega = cotizacion.id_entrega;
                        corte.entrega = cotizacion.entrego;
                        corte.observaciones = cotizacion.observaciones;
                        corte.tipo = "COTIZACION";
                        corte.fecha_pago = Fecha;
                        if (cotizacion.referencia == "FIRMO" || cotizacion.referencia == "")
                        {
                            corte.aplicado = "NO";
                            corte.fecha_aplicado = "";
                        }
                        else
                        {
                            corte.aplicado = "SI";
                            corte.fecha_aplicado = Fecha;
                        }
                        corte.id_factura = 0;
                        corte.id_nota = cotizacion.id_nota;
                        dbContext.CorteDiario.Add(corte);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        Entity.CorteDiario corteExistente = dbContext.CorteDiario.FirstOrDefault(c => c.id_corte_diario == cotizacion.id_corte_diario);
                        if (corteExistente == null)
                        {
                        }
                        else
                        {
                            corteExistente.referencia = cotizacion.referencia;
                            corteExistente.id_entrega = cotizacion.id_entrega;
                            corteExistente.entrega = cotizacion.entrego;
                            corteExistente.observaciones = cotizacion.observaciones;
                            if (cotizacion.referencia == "FIRMO" || cotizacion.referencia == "")
                            {
                                corteExistente.aplicado = "NO";
                                corteExistente.fecha_aplicado = "";
                            }
                            else
                            {
                                corteExistente.aplicado = "SI";
                                corteExistente.fecha_aplicado = Fecha;
                            }
                            dbContext.SaveChanges();
                        }
                    }
                }
            }
        }

        private void dgCotizaciones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCotizaciones.SelectedItem != null)
            {
                FacturaViewModel selectedCotizacion = dgCotizaciones.SelectedItem as FacturaViewModel;
                if (selectedCotizacion.entrego == null)
                {
                    selectedCotizacion.entrego = "";
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedCotizacion, "Cotizacion", this, "");
                    detalleWindow.ShowDialog();
                }
                else
                {
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedCotizacion, "Cotizacion", this, "EDITAR");
                    detalleWindow.ShowDialog();
                }
            }
        }
        public void ActualizarNota(FacturaViewModel notaActualizada)
        {
            var notas = dgCotizaciones.ItemsSource as List<FacturaViewModel>;
            if (notas != null)
            {
                int index = notas.FindIndex(f => f.id_nota == notaActualizada.id_nota);
                if (index >= 0)
                {
                    notas[index] = notaActualizada;
                    dgCotizaciones.ItemsSource = null;
                    dgCotizaciones.ItemsSource = notas;
                }
            }
        }
    }
}
