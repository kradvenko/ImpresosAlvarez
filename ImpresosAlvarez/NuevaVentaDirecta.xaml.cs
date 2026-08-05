using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using Syncfusion.Windows.Controls.Input;
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
    /// Lógica de interacción para NuevaVentaDirecta.xaml
    /// </summary>
    public partial class NuevaVentaDirecta : Window
    {
        List<Categorias> categorias;
        List<Insumos> insumos;
        Insumos Insumo;
        List<VentaDirectaItem> ventaDirectaItems;
        MainWindow ParentWindow;
        public NuevaVentaDirecta(MainWindow parentWindow)
        {
            InitializeComponent();
            ParentWindow = parentWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    categorias = dbContext.Categorias.ToList();
                    cbCategorias.ItemsSource = categorias;
                    cbCategorias.SelectedValuePath = "id_categoria";
                    cbCategorias.DisplayMemberPath = "nombre";
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            ventaDirectaItems = new List<VentaDirectaItem>();
        }

        private void cbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Categorias c = (Categorias)cbCategorias.SelectedItem;
                    insumos = dbContext.Insumos.Where(I => I.id_categoria == c.id_categoria).ToList();
                    dgInsumos.ItemsSource = null;
                    dgInsumos.ItemsSource = insumos;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (dgVenta.SelectedItem != null)
            {
                VentaDirectaItem ventaDirectaItem = (VentaDirectaItem)dgVenta.SelectedItem;
                NuevaVentaDirectaModificarItem modificarItemWindow = new NuevaVentaDirectaModificarItem(this, ventaDirectaItem);
                modificarItemWindow.ShowDialog();
            }
        }

        private void btnVender_Click(object sender, RoutedEventArgs e)
        {
            if (dgVenta.Items.Count > 0)
            {
                if (MessageBox.Show("¿Desea realizar la venta?", "Confirmación", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            VentaDirecta ventaDirecta = new VentaDirecta
                            {
                                id_usuario = ParentWindow.CurrentUser.id_usuario,
                                fecha = DateTime.Now.ToString(),
                                fecha_ingreso = DateTime.Now,
                                notas = "",
                                estado = "ACTIVO",
                                total = ventaDirectaItems.Sum(item => item.total)
                            };
                            dbContext.VentaDirecta.Add(ventaDirecta);
                            dbContext.SaveChanges();
                            foreach (var item in ventaDirectaItems)
                            {
                                DetalleVentaDirecta detalleVentaDirecta = new DetalleVentaDirecta
                                {
                                    id_venta_directa = ventaDirecta.id_venta_directa,
                                    id_insumo = item.id_insumo,
                                    cantidad = item.cantidad,
                                    precio = item.precio,
                                    costo = item.total
                                };
                                dbContext.DetalleVentaDirecta.Add(detalleVentaDirecta);
                            }
                            dbContext.SaveChanges();
                        }
                        MessageBox.Show("Venta realizada con éxito.");
                        this.Close();
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("No hay productos en la venta.");
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgVenta.SelectedItem != null)
            {
                VentaDirectaItem ventaDirectaItem = (VentaDirectaItem)dgVenta.SelectedItem;
                ventaDirectaItems.Remove(ventaDirectaItem);
                dgVenta.ItemsSource = null;
                dgVenta.ItemsSource = ventaDirectaItems;
            }
        }

        private void dgInsumos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgInsumos.SelectedItem != null)
            {
                Insumo = (Insumos)dgInsumos.SelectedItem;
                VentaDirectaItem ventaDirectaItem = new VentaDirectaItem(
                    Insumo.id_insumo,
                    Insumo.descripcion,
                    Insumo.precio ?? 0,
                    1
                );
                ventaDirectaItem.total = ventaDirectaItem.precio * ventaDirectaItem.cantidad;
                ventaDirectaItems.Add(ventaDirectaItem);
                dgVenta.ItemsSource = null;
                dgVenta.ItemsSource = ventaDirectaItems;
                ActualizarTotal();
            }
        }

        public void ActualizarItems()
        {
            dgVenta.ItemsSource = null;
            dgVenta.ItemsSource = ventaDirectaItems;
            ActualizarTotal();
        }

        public void ActualizarTotal()
        {
            double total = ventaDirectaItems.Sum(item => item.total);
            lblTotal.Content = "Total: " + total.ToString("C2");
        }
    }
}
