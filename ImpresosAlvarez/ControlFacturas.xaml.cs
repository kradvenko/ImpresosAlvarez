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
using ImpresosAlvarez.Entity;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para ControlFacturas.xaml
    /// </summary>
    public partial class ControlFacturas : Window
    {
        private class FacturaView
        {
            public int id_cliente { get; set; }
            public int id_factura { get; set; }
            public float Numero { get; set; }
            public string Fecha { get; set; }
            public string estado { get; set; }
            public double Total { get; set; }
            public string nombre { get; set; }
            public string Cliente { get; set; }
            public string Pagada { get; set; }
        }

        List<Clientes> _clientes;
        Clientes _clienteElegido;
        String TipoBusqueda;

        List<Facturas> _facturas;
        public ControlFacturas()
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
        public void ActualizarLista()
        {
            BuscarFacturas();
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TipoBusqueda = "CLIENTE";
            BuscarFacturas();
        }

        private List<FacturaView> ToFacturaViewList(IEnumerable<dynamic> facts)
        {
            var list = new List<FacturaView>();

            foreach (var f in facts)
            {
                // Intentar parsear campo 'numero' (maneja nombres 'numero' o 'Numero')
                string numeroStr = null;
                try
                {
                    // si el objeto anónimo contiene 'numero' o 'Numero'
                    var prop = (f.GetType().GetProperty("numero") ?? f.GetType().GetProperty("Numero"));
                    if (prop != null)
                        numeroStr = (prop.GetValue(f, null) ?? "").ToString();
                }
                catch
                {
                    numeroStr = "";
                }

                float numero = 0f;
                float.TryParse(numeroStr, out numero);

                list.Add(new FacturaView
                {
                    id_cliente = (int)f.id_cliente,
                    id_factura = (int)f.id_factura,
                    Numero = numero,
                    Fecha = (string)f.Fecha,
                    estado = (string)f.estado,
                    Total = (double)f.Total,
                    nombre = (string)f.nombre,
                    Cliente = (f.GetType().GetProperty("Cliente") != null) ? (string)f.Cliente : null,
                    Pagada = (string)f.pagada
                });
            }

            return list;
        }

        private void BuscarFacturas()
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
                                    f.pagada,
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
                                    comb.pagada,
                                    Cliente = cli.nombre
                                }
                            )
                            .Where(F => F.id_cliente == _clienteElegido.id_cliente)
                            .OrderByDescending(F => F.numero)
                            .ToList();

                        List<FacturaView> facturaViews = ToFacturaViewList(facts)
                        .OrderByDescending(f => f.Numero)
                        .ToList();

                        float TotalPorPagar = 0f;
                        int TotalFacturasSinPagar = 0;

                        foreach (FacturaView item in facturaViews)
                        {
                            if (item.Pagada == "NO" && item.estado != "CANCELADO")
                            {
                                TotalPorPagar += (float)item.Total;
                                TotalFacturasSinPagar++;
                            }
                        }

                        lblNumeroFacturas.Content = "Facturas sin pagar: " + TotalFacturasSinPagar;
                        lblTotalPorPagar.Content = "Total por pagar: " + TotalPorPagar;

                        dgFacturas.ItemsSource = facturaViews;
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
                                    f.pagada,
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
                                    comb.pagada,
                                    Cliente = cli.nombre
                                }
                            )
                           .Where(F => F.numero == folio.ToString())
                           .OrderByDescending(F => F.numero)
                           .ToList();

                    List<FacturaView> facturaViews = ToFacturaViewList(facts)
                        .OrderByDescending(f => f.Numero)
                        .ToList();

                    float TotalPorPagar = 0f;
                    int TotalFacturasSinPagar = 0;

                    foreach (FacturaView item in facturaViews)
                    {
                        if (item.Pagada == "NO" && item.estado != "CANCELADO")
                        {
                            TotalPorPagar += (float)item.Total;
                            TotalFacturasSinPagar++;
                        }
                    }

                    lblNumeroFacturas.Content = "Facturas sin pagar: " + TotalFacturasSinPagar;
                    lblTotalPorPagar.Content = "Total por pagar: " + TotalPorPagar;

                    dgFacturas.ItemsSource = facturaViews;
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
                                    f.pagada,
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
                                    comb.pagada,
                                    Cliente = cli.nombre
                                }
                            )
                               .Where(F => F.Fecha == fecha)
                               .OrderByDescending(F => F.numero)
                               .ToList();
                        //MessageBox.Show(dpFecha.SelectedDate.ToString().Substring(0, 10));
                        List<FacturaView> facturaViews = ToFacturaViewList(facts)
                        .OrderByDescending(f => f.Numero)
                        .ToList();

                        float TotalPorPagar = 0f;
                        int TotalFacturasSinPagar = 0;

                        foreach (FacturaView item in facturaViews)
                        {
                            if (item.Pagada == "NO" && item.estado != "CANCELADO")
                            {
                                TotalPorPagar += (float)item.Total;
                                TotalFacturasSinPagar++;
                            }
                        }

                        lblNumeroFacturas.Content = "Facturas sin pagar: " + TotalFacturasSinPagar;
                        lblTotalPorPagar.Content = "Total por pagar: " + TotalPorPagar;

                        dgFacturas.ItemsSource = facturaViews;

                    }
                }
                catch (Exception exc)
                {

                }
            }
        }

        private void tbFolio_KeyUp(object sender, KeyEventArgs e)
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
            BuscarFacturas();
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TipoBusqueda = "FECHA";
            BuscarFacturas();
        }

        private void btnVerTodas_Click(object sender, RoutedEventArgs e)
        {
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
                                f.pagada,
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
                                comb.pagada,
                                Cliente = cli.nombre
                            }
                        )
                        .OrderByDescending(F => F.numero)
                       .ToList();

                List<FacturaView> facturaViews = ToFacturaViewList(facts)
                        .OrderByDescending(f => f.Numero)
                        .ToList();
                float TotalPorPagar = 0f;
                int TotalFacturasSinPagar = 0;

                foreach (FacturaView item in facturaViews)
                {
                    if (item.Pagada == "NO" && item.estado != "CANCELADO")
                    {
                        TotalPorPagar += (float)item.Total;
                        TotalFacturasSinPagar++;
                    }
                }

                lblNumeroFacturas.Content = "Facturas sin pagar: " + TotalFacturasSinPagar;
                lblTotalPorPagar.Content = "Total por pagar: " + TotalPorPagar;

                dgFacturas.ItemsSource = facturaViews;
            }
        }

        private void btnVerFactura_Click(object sender, RoutedEventArgs e)
        {
            if (dgFacturas.SelectedItem != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    String idfactura = dgFacturas.SelectedItem.GetType().GetProperty("id_factura").GetValue(dgFacturas.SelectedItem, null).ToString();
                    Facturas f = dbContext.Facturas.Where(F => F.id_factura.ToString() == idfactura).First();

                    if (f.estado == "PENDIENTE")
                    {
                        MessageBox.Show("No es posible ver la factura pendiente.");
                    }
                    else
                    {
                        VerFactura ver = new VerFactura(this, f);
                        ver.ShowDialog();
                    }
                    
                }
                /*
                Facturas f = (Facturas)dgFacturas.SelectedItem;
                VerFactura ver = new VerFactura(this, f);
                ver.ShowDialog();
                */
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            if (dgFacturas.SelectedItem != null)
            {                
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    String idfactura = dgFacturas.SelectedItem.GetType().GetProperty("id_factura").GetValue(dgFacturas.SelectedItem, null).ToString();                    

                    Facturas f = dbContext.Facturas.Where(F => F.id_factura.ToString() == idfactura).First();

                    if (f.estado == "CANCELADO")
                    {
                        f.estado = "ACTIVO";

                        dbContext.SaveChanges();
                        BuscarFacturas();
                    }
                    else
                    {

                    }
                }
                /*
                Facturas f = (Facturas)dgFacturas.SelectedItem;
                VerFactura ver = new VerFactura(this, f);
                ver.ShowDialog();
                */
            }
        }

        private void btnCancelarFactura_Click(object sender, RoutedEventArgs e)
        {
            if (dgFacturas.SelectedItem != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    String idfactura = dgFacturas.SelectedItem.GetType().GetProperty("id_factura").GetValue(dgFacturas.SelectedItem, null).ToString();

                    Facturas f = dbContext.Facturas.Where(F => F.id_factura.ToString() == idfactura).First();

                    if (f.estado == "ACTIVO" || f.estado == "PENDIENTE")
                    {
                        f.estado = "CANCELADO";

                        var listOrders = dbContext.FacturaOrden.Where(F => F.id_factura.ToString() == idfactura).ToList();

                        foreach (var item in listOrders)
                        {
                            dbContext.FacturaOrden.Remove(item);
                        }

                        dbContext.SaveChanges();
                        BuscarFacturas();
                    }
                    else
                    {

                    }
                }
                /*
                Facturas f = (Facturas)dgFacturas.SelectedItem;
                VerFactura ver = new VerFactura(this, f);
                ver.ShowDialog();
                */
            }
        }
    }
}
