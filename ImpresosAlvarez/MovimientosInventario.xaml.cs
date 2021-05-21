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
    /// Lógica de interacción para MovimientosInventario.xaml
    /// </summary>
    public partial class MovimientosInventario : Window
    {
        List<EntradasInventario> Entradas;
        List<SalidasInventario> Salidas;

        Insumos Insumo;
        public MovimientosInventario(Insumos Insumo)
        {
            InitializeComponent();
            this.Insumo = Insumo;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgSalidas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSalidas.SelectedItem != null)
            {
                SalidasInventario salida = (SalidasInventario)dgSalidas.SelectedItem;

                if (salida.orden_trabajo.Length == 0)
                {
                    return;
                }

                Double numOrden = Double.Parse(salida.orden_trabajo);
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Ordenes orden = dbContext.Ordenes.Where(O => O.numero == numOrden).FirstOrDefault();

                    if (orden != null)
                    {
                        Clientes cliente = dbContext.Clientes.Where(C => C.id_cliente == orden.id_cliente).First();

                        lblCliente.Content = cliente.pseudonimo;
                        lblFechaSolicita.Content = orden.fecha_solicita;
                        lblTrabajo.Content = orden.nombre_trabajo;
                        lblPapel.Content = orden.tipo_papel;
                        lblCantidad.Content = orden.cantidad;
                        lblTintas.Content = orden.color_tintas;
                        lblTamano.Content = orden.tamano;
                    }
                    else
                    {
                        lblCliente.Content = "";
                        lblFechaSolicita.Content = "";
                        lblTrabajo.Content = "";
                        lblPapel.Content = "";
                        lblCantidad.Content = "";
                        lblTintas.Content = "";
                        lblTamano.Content = "";
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                Salidas = dbContext.SalidasInventario.Where(S => S.id_insumo == Insumo.id_insumo).Take(10).OrderByDescending(S => S.id_insumo).ToList();
                dgSalidas.ItemsSource = null;
                dgSalidas.ItemsSource = Salidas;

                Entradas = dbContext.EntradasInventario.Where(E => E.id_insumo == Insumo.id_insumo).Take(10).OrderByDescending(S => S.id_insumo).ToList();
                dgEntradas.ItemsSource = null;
                dgEntradas.ItemsSource = Entradas;
            }
        }
    }
}
