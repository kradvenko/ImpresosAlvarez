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
    /// Lógica de interacción para ControlNotas.xaml
    /// </summary>
    public partial class ControlNotas : Window
    {
        List<Clientes> _clientes;
        Clientes _clienteElegido;
        String TipoBusqueda;
        public ControlNotas()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = _clientes;

                /*
                _contribuyentes = dbContext.Contribuyentes.ToList();
                cbContribuyentes.ItemsSource = _contribuyentes;
                cbContribuyentes.SelectedValuePath = "IdContribuyente";
                cbContribuyentes.DisplayMemberPath = "Nombre";
                cbContribuyentes.SelectedIndex = 0;
                */
            }
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void tbFolio_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnVerNota_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCancelarNota_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BuscarNotas()
        {
            if (TipoBusqueda == "CLIENTE")
            {
                if (tbClientes.SelectedItem != null)
                {
                    _clienteElegido = (Clientes)tbClientes.SelectedItem;

                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        //_facturas = dbContext.Facturas.Where(F => F.IdCliente == _clienteElegido.IdCliente).ToList();

                        var facts = dbContext.Facturas
                            .Join(
                                dbContext.Contribuyentes,
                                f => f.id_contribuyente,
                                c => c.id_contribuyente,
                                (f, c) => new
                                {
                                    f.id_cliente,
                                    f.id_factura,
                                    f.numero,
                                    Fecha = f.fecha.Substring(0, 10),
                                    f.estado,
                                    Total = Math.Round((double)f.total, 2),
                                    c.nombre
                                }
                            )
                            .Join(
                                dbContext.Clientes,
                                comb => comb.id_cliente,
                                cli => cli.id_cliente,
                                (comb, cli) => new
                                {
                                    comb.id_cliente,
                                    comb.id_factura,
                                    comb.numero,
                                    comb.Fecha,
                                    comb.estado,
                                    comb.nombre,
                                    comb.Total,
                                    Cliente = cli.nombre
                                }
                            )
                            .Where(F => F.id_cliente == _clienteElegido.id_cliente)
                            .ToList();

                        var notes = dbContext.Notas
                            .Join

                        dgNotas.ItemsSource = facts;
                    }
                }
            }
            else if (TipoBusqueda == "FOLIO")
            {
                int folio = int.Parse(tbFolio.Text);
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    var facts = dbContext.Facturas
                            .Join(
                                dbContext.Contribuyentes,
                                f => f.id_contribuyente,
                                c => c.id_contribuyente,
                                (f, c) => new
                                {
                                    f.id_cliente,
                                    f.id_factura,
                                    f.numero,
                                    Fecha = f.fecha.Substring(0, 10),
                                    f.estado,
                                    Total = Math.Round((double)f.total, 2),
                                    c.nombre
                                }
                            )
                            .Join(
                                dbContext.Clientes,
                                comb => comb.id_cliente,
                                cli => cli.id_cliente,
                                (comb, cli) => new
                                {
                                    comb.id_cliente,
                                    comb.id_factura,
                                    comb.numero,
                                    comb.Fecha,
                                    comb.estado,
                                    comb.nombre,
                                    comb.Total,
                                    Cliente = cli.nombre
                                }
                            )
                           .Where(F => F.numero == folio.ToString())
                           .ToList();

                    dgNotas.ItemsSource = facts;
                }
            }
            else if (TipoBusqueda == "FECHA")
            {
                try
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        //_facturas = dbContext.Facturas.Where(F => F.Numero == folio.ToString()).ToList();
                        String fecha = dpFecha.SelectedDate.Value.ToShortDateString();

                        var facts = dbContext.Facturas
                            .Join(
                                dbContext.Contribuyentes,
                                f => f.id_contribuyente,
                                c => c.id_contribuyente,
                                (f, c) => new
                                {
                                    f.id_cliente,
                                    f.id_factura,
                                    f.numero,
                                    Fecha = f.fecha.Substring(0, 10),
                                    f.estado,
                                    Total = Math.Round((double)f.total, 2),
                                    c.nombre
                                }
                            )
                            .Join(
                                dbContext.Clientes,
                                comb => comb.id_cliente,
                                cli => cli.id_cliente,
                                (comb, cli) => new
                                {
                                    comb.id_cliente,
                                    comb.id_factura,
                                    comb.numero,
                                    comb.Fecha,
                                    comb.estado,
                                    comb.nombre,
                                    comb.Total,
                                    Cliente = cli.nombre
                                }
                            )
                               .Where(F => F.Fecha == fecha)
                               .ToList();
                        //MessageBox.Show(dpFecha.SelectedDate.ToString().Substring(0, 10));
                        dgNotas.ItemsSource = facts;

                    }
                }
                catch (Exception exc)
                {

                }
            }
        }
    }
}
