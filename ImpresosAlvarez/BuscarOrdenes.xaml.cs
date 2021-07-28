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
using System.Data.Entity;
using ImpresosAlvarez.Entity;
using ImpresosAlvarez.Clases;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para BuscarOrdenes.xaml
    /// </summary>
    public partial class BuscarOrdenes : Window
    {
        FacturaDigital Parent;
        private int IdCliente;
        List<Obtener_Ordenes_Para_Factura_Result> Ordenes;
        public BuscarOrdenes(int IdCliente, FacturaDigital Parent)
        {
            InitializeComponent();
            this.IdCliente = IdCliente;
            this.Parent = Parent;
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrdenes.SelectedItems.Count > 0)
            {
                foreach (var item in dgOrdenes.SelectedItems)
                {
                    Obtener_Ordenes_Para_Factura_Result orden = (Obtener_Ordenes_Para_Factura_Result)item;
                    AgregarOrden(orden);
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                Ordenes = dbContext.Obtener_Ordenes_Para_Factura(IdCliente).ToList();
                dgOrdenes.ItemsSource = Ordenes;
            }
        }

        private void dgOrdenes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgOrdenes.SelectedItems.Count > 0)
            {
                foreach (var item in dgOrdenes.SelectedItems)
                {
                    Obtener_Ordenes_Para_Factura_Result orden = (Obtener_Ordenes_Para_Factura_Result)item;
                    AgregarOrden(orden);
                }
            }
        }
        private void AgregarOrden(Obtener_Ordenes_Para_Factura_Result orden)
        {
            /*
            if (dgOrdenes.SelectedItem is null)
            {
                MessageBox.Show("No ha elegido una orden.");
                return;
            }
            */
            //Obtener_Ordenes_Para_Factura_Result orden = (Obtener_Ordenes_Para_Factura_Result)dgOrdenes.SelectedItem;
            if (orden.autorizado == "NO")
            {
                MessageBox.Show("La orden no ha sido autorizada.");
                return;
            }
            if (Parent.OrdenAgregada(orden.id_orden))
            {
                MessageBox.Show("La orden ya ha sido agregada.");
                return;
            }
            int numCopias = 0;
            String copias = "";
            if (orden.copia_1.Length > 0)
            {
                numCopias++;
            }
            if (orden.copia_2.Length > 0)
            {
                numCopias++;
            }
            if (orden.copia_3.Length > 0)
            {
                numCopias++;
            }
            if (orden.copia_4.Length > 0)
            {
                numCopias++;
            }
            if (numCopias > 0)
            {
                if (numCopias == 1)
                {
                    copias = "1 COPIA";
                }
                else
                {
                    copias = numCopias.ToString() + " COPIAS";
                }
            }
            ConceptoFactura nuevaOrden = new ConceptoFactura();
            nuevaOrden.IdOrden = orden.id_orden;
            nuevaOrden.Cantidad = (int)orden.cantidad;
            nuevaOrden.Descripcion = orden.nombre_trabajo + " " + orden.color_tintas + " " + orden.tipo_papel + " " + orden.del_numero + " " + orden.al_numero + " " + orden.tamano + " " + copias;
            nuevaOrden.PrecioUnitario = (float)orden.total / nuevaOrden.Cantidad;
            nuevaOrden.PrecioUnitario = (float)Math.Round(nuevaOrden.PrecioUnitario, 6);
            nuevaOrden.Importe = (float)orden.total;
            nuevaOrden.Servicio = new ProductosServicios();
            nuevaOrden.Unidad = "";
            nuevaOrden.Clave = "";
            Parent.AgregarOrden(nuevaOrden);
            this.Close();
        }
    }
}
