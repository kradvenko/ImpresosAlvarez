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
    /// Lógica de interacción para ControlComplementos.xaml
    /// </summary>
    public partial class ControlComplementos : Window
    {
        List<Clientes> _clientes;
        Clientes _clienteElegido;
        String TipoBusqueda;
        public ControlComplementos()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = _clientes;
            }
        }
        private void BuscarComplementos()
        {
            if (TipoBusqueda == "CLIENTE")
            {
                if (tbClientes.SelectedItem != null)
                {
                    _clienteElegido = (Clientes)tbClientes.SelectedItem;

                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        //_facturas = dbContext.Facturas.Where(F => F.IdCliente == _clienteElegido.IdCliente).ToList();

                        var comps = dbContext.Parcialidades
                            .Join(
                                dbContext.Facturas,
                                parci => parci.id_factura,
                                fact => fact.id_factura,
                                (parci, fact) => new
                                {
                                    fact.id_contribuyente,
                                    fact.id_factura,
                                    fact.id_cliente,
                                    NumeroFactura = fact.numero,
                                    FechaFactura = fact.fecha,
                                    NumeroParcialidad = parci.parcialidad,
                                    FechaComplemento = parci.fecha,
                                    TotalComplemento = parci.pagado,
                                    EstadoComplemento = parci.estado
                                }
                            )
                            .Join(
                                dbContext.Contribuyentes,
                                f => f.id_contribuyente,
                                c => c.id_contribuyente,
                                (f, c) => new
                                {
                                    f.id_cliente,
                                    f.id_factura,
                                    f.NumeroFactura,
                                    FechaFactura = f.FechaFactura.Substring(0, 10),
                                    f.NumeroParcialidad,
                                    f.FechaComplemento,
                                    f.EstadoComplemento,
                                    TotalComplemento = Math.Round((double)f.TotalComplemento, 2),
                                    Contribuyente = c.nombre
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
                                    comb.NumeroFactura,
                                    comb.FechaFactura,
                                    comb.NumeroParcialidad,
                                    comb.FechaComplemento,
                                    comb.EstadoComplemento,
                                    comb.TotalComplemento,
                                    comb.Contribuyente,
                                    Cliente = cli.nombre
                                }
                            )
                            .Where(F => F.id_cliente == _clienteElegido.id_cliente)
                            .ToList();

                        dgComplementos.ItemsSource = comps;
                    }
                }
            }
            else if (TipoBusqueda == "FOLIO")
            {
                int folio = int.Parse(tbFolio.Text);
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    var comps = dbContext.Parcialidades
                            .Join(
                                dbContext.Facturas,
                                parci => parci.id_factura,
                                fact => fact.id_factura,
                                (parci, fact) => new
                                {
                                    fact.id_contribuyente,
                                    fact.id_factura,
                                    fact.id_cliente,
                                    NumeroFactura = fact.numero,
                                    FechaFactura = fact.fecha,
                                    NumeroParcialidad = parci.parcialidad,
                                    FechaComplemento = parci.fecha,
                                    TotalComplemento = parci.pagado,
                                    EstadoComplemento = parci.estado
                                }
                            )
                            .Join(
                                dbContext.Contribuyentes,
                                f => f.id_contribuyente,
                                c => c.id_contribuyente,
                                (f, c) => new
                                {
                                    f.id_cliente,
                                    f.id_factura,
                                    f.NumeroFactura,
                                    FechaFactura = f.FechaFactura.Substring(0, 10),
                                    f.NumeroParcialidad,
                                    f.FechaComplemento,
                                    f.EstadoComplemento,
                                    TotalComplemento = Math.Round((double)f.TotalComplemento, 2),
                                    Contribuyente = c.nombre
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
                                    comb.NumeroFactura,
                                    comb.FechaFactura,
                                    comb.NumeroParcialidad,
                                    comb.FechaComplemento,
                                    comb.EstadoComplemento,
                                    comb.TotalComplemento,
                                    comb.Contribuyente,
                                    Cliente = cli.nombre
                                }
                            )
                           .Where(F => F.NumeroFactura == folio.ToString())
                           .ToList();

                    dgComplementos.ItemsSource = comps;
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
                        var comps = dbContext.Parcialidades
                            .Join(
                                dbContext.Facturas,
                                parci => parci.id_factura,
                                fact => fact.id_factura,
                                (parci, fact) => new
                                {
                                    fact.id_contribuyente,
                                    fact.id_factura,
                                    fact.id_cliente,
                                    NumeroFactura = fact.numero,
                                    FechaFactura = fact.fecha,
                                    NumeroParcialidad = parci.parcialidad,
                                    FechaComplemento = parci.fecha,
                                    TotalComplemento = parci.pagado,
                                    EstadoComplemento = parci.estado
                                }
                            )
                            .Join(
                                dbContext.Contribuyentes,
                                f => f.id_contribuyente,
                                c => c.id_contribuyente,
                                (f, c) => new
                                {
                                    f.id_cliente,
                                    f.id_factura,
                                    f.NumeroFactura,
                                    FechaFactura = f.FechaFactura.Substring(0, 10),
                                    f.NumeroParcialidad,
                                    f.FechaComplemento,
                                    f.EstadoComplemento,
                                    TotalComplemento = Math.Round((double)f.TotalComplemento, 2),
                                    Contribuyente = c.nombre
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
                                    comb.NumeroFactura,
                                    comb.FechaFactura,
                                    comb.NumeroParcialidad,
                                    comb.FechaComplemento,
                                    comb.EstadoComplemento,
                                    comb.TotalComplemento,
                                    comb.Contribuyente,
                                    Cliente = cli.nombre
                                }
                            )
                            .Where(F => F.FechaFactura == fecha)
                            .ToList();
                        //MessageBox.Show(dpFecha.SelectedDate.ToString().Substring(0, 10));
                        dgComplementos.ItemsSource = comps;
                    }
                }
                catch (Exception exc)
                {

                }
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TipoBusqueda = "FECHA";
            BuscarComplementos();
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TipoBusqueda = "CLIENTE";
            BuscarComplementos();
        }

        private void tbFolio_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int folio = 0;
                if (tbFolio.Text.Length > 0)
                {
                    if (int.TryParse(tbFolio.Text, out folio))
                    {
                        folio = int.Parse(tbFolio.Text);
                    }
                }
                TipoBusqueda = "FOLIO";
                BuscarComplementos();
            }
        }

        private void btnVerTodas_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnVerTodos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    //_facturas = dbContext.Facturas.Where(F => F.Numero == folio.ToString()).ToList();
                    //String fecha = dpFecha.SelectedDate.Value.ToShortDateString();

                    var comps = dbContext.Parcialidades
                        .Join(
                            dbContext.Facturas,
                            parci => parci.id_factura,
                            fact => fact.id_factura,
                            (parci, fact) => new
                            {
                                fact.id_contribuyente,
                                fact.id_factura,
                                fact.id_cliente,
                                NumeroFactura = fact.numero,
                                FechaFactura = fact.fecha,
                                NumeroParcialidad = parci.parcialidad,
                                FechaComplemento = parci.fecha,
                                TotalComplemento = parci.pagado,
                                EstadoComplemento = parci.estado
                            }
                        )
                        .Join(
                            dbContext.Contribuyentes,
                            f => f.id_contribuyente,
                            c => c.id_contribuyente,
                            (f, c) => new
                            {
                                f.id_cliente,
                                f.id_factura,
                                f.NumeroFactura,
                                FechaFactura = f.FechaFactura.Substring(0, 10),
                                f.NumeroParcialidad,
                                FechaComplemento = f.FechaComplemento.Value,
                                f.EstadoComplemento,
                                TotalComplemento = Math.Round((double)f.TotalComplemento, 2),
                                Contribuyente = c.nombre
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
                                comb.NumeroFactura,
                                comb.FechaFactura,
                                comb.NumeroParcialidad,
                                comb.FechaComplemento,
                                comb.EstadoComplemento,
                                comb.TotalComplemento,
                                comb.Contribuyente,
                                Cliente = cli.nombre
                            }
                        )
                        .ToList();
                    dgComplementos.ItemsSource = comps;
                }
            }
            catch (Exception exc)
            {

            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnVerComplemento_Click(object sender, RoutedEventArgs e)
        {
            /*
            String fecha = dgComplementos.SelectedItem.GetType().GetProperty("FechaComplemento").GetValue(dgComplementos.SelectedItem, null).ToString();
            MessageBox.Show(fecha);
            */
            if (dgComplementos.SelectedItem != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    String idfactura = dgComplementos.SelectedItem.GetType().GetProperty("id_factura").GetValue(dgComplementos.SelectedItem, null).ToString();
                    Facturas f = dbContext.Facturas.Where(F => F.id_factura.ToString() == idfactura).First();

                    VerComplemento ver = new VerComplemento(this, f);
                    ver.ShowDialog();
                }
                /*
                Facturas f = (Facturas)dgFacturas.SelectedItem;
                VerFactura ver = new VerFactura(this, f);
                ver.ShowDialog();
                */
            }
        }
        public void ActualizarLista()
        {
            BuscarComplementos();
        }
    }
}
