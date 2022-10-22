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
    /// Lógica de interacción para ControlServiciosProductos.xaml
    /// </summary>
    public partial class ControlServiciosProductos : Window
    {
        List<ProductosServicios> ListaProductos;
        public ControlServiciosProductos()
        {
            InitializeComponent();
        }

        private void tbBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            String busqueda = "";
            busqueda = tbBuscar.Text;

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaProductos = dbContext.ProductosServicios.Where(T => T.descripcion.Contains(busqueda)).ToList();
                dgServiciosProductos.ItemsSource = ListaProductos;
            }
        }

        public void Cargar()
        {
            tbBuscar.Text = "";
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaProductos = dbContext.ProductosServicios.ToList();
                dgServiciosProductos.ItemsSource = ListaProductos;
            }
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            VerServicioProducto nuevo = new VerServicioProducto(null, this, "NUEVO");
            nuevo.ShowDialog();
        }

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (dgServiciosProductos.SelectedItem != null)
            {
                ProductosServicios elegido = (ProductosServicios)dgServiciosProductos.SelectedItem;

                VerServicioProducto modificar = new VerServicioProducto(elegido, this, "MODIFICAR");
                modificar.ShowDialog();
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaProductos = dbContext.ProductosServicios.ToList();
                dgServiciosProductos.ItemsSource = ListaProductos;
            }
        }
    }
}
