using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Lógica de interacción para ReporteVentasDirectas.xaml
    /// </summary>
    public partial class ReporteVentasDirectas : Window
    {
        private ImpresosBDEntities db;

        public ReporteVentasDirectas()
        {
            InitializeComponent();
            db = new ImpresosBDEntities();
            CargarVentasDirectas();
        }

        private void CargarVentasDirectas(DateTime? fechaFiltro = null)
        {
            try
            {
                var ventas = db.VentaDirecta.ToList();

                if (fechaFiltro.HasValue)
                {
                    ventas = ventas.Where(v =>
                    {
                        if (string.IsNullOrEmpty(v.fecha))
                            return false;

                        DateTime fechaVenta;
                        // Intentar parsear la fecha en diferentes formatos comunes
                        if (DateTime.TryParseExact(v.fecha, new[] { "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss" },
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaVenta))
                        {
                            return fechaVenta.Date == fechaFiltro.Value.Date;
                        }
                        else if (DateTime.TryParse(v.fecha, out fechaVenta))
                        {
                            return fechaVenta.Date == fechaFiltro.Value.Date;
                        }

                        return false;
                    }).ToList();
                }
                else
                {
                    ventas = ventas.OrderByDescending(v => v.id_venta_directa).ToList();
                }

                dgVentasDirectas.ItemsSource = ventas;
                CalcularTotalGeneral(ventas);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las ventas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDetalleVenta(int idVentaDirecta)
        {
            try
            {
                var detalles = (from d in db.DetalleVentaDirecta
                                where d.id_venta_directa == idVentaDirecta
                                join i in db.Insumos on d.id_insumo equals i.id_insumo into insumos
                                from insumo in insumos.DefaultIfEmpty()
                                select new
                                {
                                    d.id_detalle_venta_directa,
                                    d.id_insumo,
                                    DescripcionInsumo = insumo != null ? insumo.descripcion : "Sin descripción",
                                    d.cantidad,
                                    d.precio,
                                    d.costo,
                                    Subtotal = (d.cantidad ?? 0) * (d.precio ?? 0)
                                }).ToList();

                dgDetalleVentas.ItemsSource = detalles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el detalle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalcularTotalGeneral(List<VentaDirecta> ventas)
        {
            double total = ventas.Sum(v => v.total ?? 0);
            lblTotalGeneral.Content = total.ToString("C2");
        }

        private void dgVentasDirectas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgVentasDirectas.SelectedItem is VentaDirecta ventaSeleccionada)
            {
                CargarDetalleVenta(ventaSeleccionada.id_venta_directa);
            }
            else
            {
                dgDetalleVentas.ItemsSource = null;
            }
        }

        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            if (dpFecha.SelectedDate.HasValue)
            {
                CargarVentasDirectas(dpFecha.SelectedDate.Value);
            }
            else
            {
                MessageBox.Show("Por favor seleccione una fecha para filtrar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            dpFecha.SelectedDate = null;
            CargarVentasDirectas();
            dgDetalleVentas.ItemsSource = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            db?.Dispose();
        }
    }
}
