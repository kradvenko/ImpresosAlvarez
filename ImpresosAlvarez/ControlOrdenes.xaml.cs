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
    /// Lógica de interacción para ControlOrdenes.xaml
    /// </summary>
    public partial class ControlOrdenes : Window
    {
        Clientes _clienteElegido;
        String TipoBusqueda = "";
        List<Clientes> _clientes;
        public ControlOrdenes()
        {
            InitializeComponent();
        }

        private void BuscarOrdenes()
        {
            if (TipoBusqueda == "CLIENTE")
            {
                if (tbClientes.SelectedItem != null)
                {
                    _clienteElegido = (Clientes)tbClientes.SelectedItem;

                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        //_facturas = dbContext.Facturas.Where(F => F.IdCliente == _clienteElegido.IdCliente).ToList();

                        var comps = dbContext.Ordenes
                            .Join(
                                dbContext.Clientes,
                                orden => orden.id_cliente,
                                cli => cli.id_cliente,
                                (orden, cli) => new
                                {
                                    orden.id_cliente,
                                    orden.nombre_trabajo,
                                    orden.id_orden,
                                    orden.numero,
                                    orden.estado,
                                    orden.fecha_solicita,
                                    Cliente = cli.nombre
                                }
                            )
                            .Where(F => F.id_cliente == _clienteElegido.id_cliente)
                            .ToList();

                        dgOrdenes.ItemsSource = comps;
                    }
                }
            }
            else if (TipoBusqueda == "NUMERO")
            {
                int folio = int.Parse(tbNumero.Text);
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    //_facturas = dbContext.Facturas.Where(F => F.IdCliente == _clienteElegido.IdCliente).ToList();

                    var comps = dbContext.Ordenes
                        .Join(
                            dbContext.Clientes,
                            orden => orden.id_cliente,
                            cli => cli.id_cliente,
                            (orden, cli) => new
                            {
                                orden.id_cliente,
                                orden.nombre_trabajo,
                                orden.id_orden,
                                orden.numero,
                                orden.estado,
                                orden.fecha_solicita,
                                Cliente = cli.nombre
                            }
                        )
                        .Where(F => F.numero == folio)
                        .ToList();

                    dgOrdenes.ItemsSource = comps;
                }
            }
            else if (TipoBusqueda == "FECHA")
            {
                try
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        //_facturas = dbContext.Facturas.Where(F => F.Numero == folio.ToString()).ToList();
                        String fecha = dpFecha.SelectedDate.Value.ToString("dd/MM/yyyy");
                        //MessageBox.Show(dpFecha.SelectedDate.Value.Date.ToString());
                        var comps = dbContext.Ordenes
                            .Join(
                                dbContext.Clientes,
                                orden => orden.id_cliente,
                                cli => cli.id_cliente,
                                (orden, cli) => new
                                {
                                    orden.id_cliente,
                                    orden.nombre_trabajo,
                                    orden.id_orden,
                                    orden.numero,
                                    orden.estado,
                                    orden.fecha_solicita,
                                    Cliente = cli.nombre
                                }
                            )
                            .Where(F => F.fecha_solicita == fecha)
                            .ToList();
                        //MessageBox.Show(dpFecha.SelectedDate.ToString().Substring(0, 10));
                        dgOrdenes.ItemsSource = comps;
                    }
                }
                catch (Exception exc)
                {

                }
            }
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                TipoBusqueda = "CLIENTE";
                BuscarOrdenes();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = _clientes;
            }
        }

        private void tbNumero_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbNumero.Text.Length > 0)
                {
                    int x;
                    if (int.TryParse(tbNumero.Text, out x))
                    {
                        TipoBusqueda = "NUMERO";
                        BuscarOrdenes();
                    }
                }
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFecha.SelectedDate != null)
            {
                TipoBusqueda = "FECHA";
                BuscarOrdenes();
            }
        }

        private void btnVerOrden_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrdenes.SelectedItem != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    String idorden = dgOrdenes.SelectedItem.GetType().GetProperty("id_orden").GetValue(dgOrdenes.SelectedItem, null).ToString();
                    Ordenes f = dbContext.Ordenes.Where(F => F.id_orden.ToString() == idorden).First();

                    OrdenModificar ver = new OrdenModificar(this, f);
                    ver.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Elija una orden.");
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
