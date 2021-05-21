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
    /// Lógica de interacción para Salidas.xaml
    /// </summary>
    public partial class Salidas : Window
    {
        List<SalidasInventario> SalidasDia;
        public Salidas()
        {
            InitializeComponent();
        }

        private void btnNuevaSalida_Click(object sender, RoutedEventArgs e)
        {
            ControlSalidaInventario salida = new ControlSalidaInventario(this, "NUEVO", null);
            salida.ShowDialog();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFecha.SelectedDate = DateTime.Now;
        }

        public void CargarSalidas()
        {
            if (dpFecha.SelectedDate != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    DateTime Fecha = dpFecha.SelectedDate.Value.Date;
                    SalidasDia = dbContext.SalidasInventario.Where(E => E.fecha == Fecha).ToList();
                    dgSalidas.ItemsSource = null;
                    dgSalidas.ItemsSource = SalidasDia;
                }
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarSalidas();
        }

        private void Label_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void tbOrden_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Double numOrden = 0;

                if (double.TryParse(tbOrden.Text, out numOrden))
                {
                    numOrden = double.Parse(tbOrden.Text);
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
                    }
                }
            }
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

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (dgSalidas.SelectedItem != null)
            {
                SalidasInventario salidaInventario = (SalidasInventario)dgSalidas.SelectedItem;
                ControlSalidaInventario salida = new ControlSalidaInventario(this, "MODIFICAR", salidaInventario);
                salida.ShowDialog();
            }
        }
    }
}
