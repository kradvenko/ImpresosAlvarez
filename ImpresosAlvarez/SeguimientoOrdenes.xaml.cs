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
    /// Lógica de interacción para SeguimientoOrdenes.xaml
    /// </summary>
    public partial class SeguimientoOrdenes : Window
    {
        List<Ordenes> ListaOrdenes;
        List<Clientes> ListaClientes;
        public SeguimientoOrdenes()
        {
            InitializeComponent();
        }

        private void cbAreas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String Area;
            ListaOrdenes = new List<Ordenes>();

            Area = cbAreas.SelectedValue.ToString();

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaOrdenes = dbContext.Ordenes.Where(A => A.estado == Area).OrderByDescending(A => A.id_orden).ToList();                
                dgOrdenes.ItemsSource = null;
                dgOrdenes.ItemsSource = ListaOrdenes;
            }
        }

        private void dgOrdenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrdenes.SelectedItem != null)
            {
                Ordenes OrdenElegida = new Ordenes();

                OrdenElegida = (Ordenes)dgOrdenes.SelectedItem;

                String Cliente;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Cliente = dbContext.Clientes.Where(C => C.id_cliente == OrdenElegida.id_cliente).First().nombre;
                }

                tbNombreCliente.Text = Cliente;
                tbTrabajo.Text = OrdenElegida.nombre_trabajo;
                tbFechaSolicito.Text = OrdenElegida.fecha_solicita;
                tbHoraSolicito.Text = OrdenElegida.hora_solicita;
                tbRecibio.Text = OrdenElegida.quien_recibio;
                tbCantidad.Text = OrdenElegida.cantidad.ToString();
                tbTintas.Text = OrdenElegida.color_tintas;
                tbPapel.Text = OrdenElegida.tipo_papel;
                cbTamanio.Text = OrdenElegida.tamano;
                if (OrdenElegida.con_folio == "SI")
                {
                    cbConFolio.IsChecked = true;
                } else
                {
                    cbConFolio.IsChecked = false;
                }
                tbDel.Text = OrdenElegida.del_numero;
                tbAl.Text = OrdenElegida.al_numero;
                tbTotal.Text = OrdenElegida.total.ToString();
                tbAnticipo.Text = OrdenElegida.anticipo.ToString();
                tbCostoAnterior.Text = OrdenElegida.costo_anterior.ToString();
                tbOtros1.Text = OrdenElegida.otros_1;
                tbOtros2.Text = OrdenElegida.otros_2;
                tbOtros3.Text = OrdenElegida.otros_3;
                tbOtros4.Text = OrdenElegida.otros_4;
                tbTelefono.Text = OrdenElegida.telefono;
                cbCopias1.Text = OrdenElegida.copia_1;
                cbCopias2.Text = OrdenElegida.copia_2;
                cbCopias3.Text = OrdenElegida.copia_3;
                cbCopias4.Text = OrdenElegida.copia_4;
                if (OrdenElegida.pegado == "SI")
                {
                    cbPegado.IsChecked = true;
                }
                else
                {
                    cbPegado.IsChecked = false;
                }
                if (OrdenElegida.engrapado == "SI")
                {
                    cbEngrapado.IsChecked = true;
                }
                else
                {
                    cbEngrapado.IsChecked = false;
                }
                if (OrdenElegida.perforacion == "SI")
                {
                    cbPerforacion.IsChecked = true;
                }
                else
                {
                    cbPerforacion.IsChecked = false;
                }
                if (OrdenElegida.rojo == "SI")
                {
                    cbRojo.IsChecked = true;
                }
                else
                {
                    cbRojo.IsChecked = false;
                }
                if (OrdenElegida.blanco == "SI")
                {
                    cbBlanco.IsChecked = true;
                }
                else
                {
                    cbBlanco.IsChecked = false;
                }
                tbNotas.Text = OrdenElegida.especificaciones;
            }
        }

        private void tbNumeroOrden_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double NumeroOrden;

                if (double.TryParse(tbNumeroOrden.Text, out NumeroOrden))
                {
                    NumeroOrden = double.Parse(tbNumeroOrden.Text);

                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        ListaOrdenes = dbContext.Ordenes.Where(A => A.numero == NumeroOrden).OrderByDescending(A => A.id_orden).ToList();
                        dgOrdenes.ItemsSource = null;
                        dgOrdenes.ItemsSource = ListaOrdenes;
                    }
                }
            }
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                Clientes ClienteElegido = (Clientes)tbClientes.SelectedItem;
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    ListaOrdenes = dbContext.Ordenes.Where(A => A.id_cliente == ClienteElegido.id_cliente).OrderByDescending(A => A.id_orden).ToList();
                    dgOrdenes.ItemsSource = null;
                    dgOrdenes.ItemsSource = ListaOrdenes;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaClientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = ListaClientes;
            }
        }
    }
}
