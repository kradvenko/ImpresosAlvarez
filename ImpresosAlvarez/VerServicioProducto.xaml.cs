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
    /// Lógica de interacción para VerServicioProducto.xaml
    /// </summary>
    public partial class VerServicioProducto : Window
    {
        ProductosServicios ServicioElegido;
        ControlServiciosProductos ParentForm;
        String Modo;
        public VerServicioProducto(ProductosServicios ServicioElegido, ControlServiciosProductos ParentForm, String Modo)
        {
            InitializeComponent();
            this.ServicioElegido = ServicioElegido;
            this.ParentForm = ParentForm;
            this.Modo = Modo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Modo == "NUEVO")
            {

            }
            else
            {
                tbClave.Text = ServicioElegido.clave;
                tbDescripcion.Text = ServicioElegido.descripcion;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Modo == "NUEVO")
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    ProductosServicios nuevo = new ProductosServicios();
                    nuevo.clave = tbClave.Text;
                    nuevo.descripcion = tbDescripcion.Text;

                    dbContext.ProductosServicios.Add(nuevo);

                    dbContext.SaveChanges();

                    ParentForm.Cargar();

                    this.Close();
                }
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    ProductosServicios modificar = dbContext.ProductosServicios.Where(T => T.id_productoservicio == ServicioElegido.id_productoservicio).First();
                    modificar.clave = tbClave.Text;
                    modificar.descripcion = tbDescripcion.Text;

                    dbContext.SaveChanges();

                    ParentForm.Cargar();

                    this.Close();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
