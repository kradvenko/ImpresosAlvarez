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
            TipoBusqueda = "CLIENTE";
            BuscarNotas();
        }

        private void tbFolio_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TipoBusqueda = "FOLIO";
                BuscarNotas();
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TipoBusqueda = "FECHA";
            BuscarNotas();
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
            if (MessageBox.Show("Desea cancelar la nota?", "Atención", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    String idnota = dgNotas.SelectedItem.GetType().GetProperty("id_nota").GetValue(dgNotas.SelectedItem, null).ToString();
                    Notas n = dbContext.Notas.Where(F => F.id_nota.ToString() == idnota).First();

                    n.estado = "CANCELADO";

                    dbContext.SaveChanges();

                    MessageBox.Show("Se ha cancelado la nota.");
                    BuscarNotas();
                }
            }
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
                        var notes = dbContext.Notas
                            .Join(
                                dbContext.Clientes,
                                nota => nota.id_cliente,
                                cli => cli.id_cliente,
                                (nota, cli) => new
                                {
                                    nota.id_cliente,
                                    nota.id_nota,
                                    nota.total,
                                    nota.pagada,
                                    nota.estado,
                                    nota.fecha,
                                    nota.numero,                                    
                                    Cliente = cli.nombre + " | " + cli.pseudonimo
                                }
                            )
                            .Where(N => N.id_cliente == _clienteElegido.id_cliente)
                            .ToList();

                        dgNotas.ItemsSource = notes;
                    }
                }
            }
            else if (TipoBusqueda == "FOLIO")
            {
                int folio = int.Parse(tbFolio.Text);
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    var notes = dbContext.Notas
                        .Join(
                            dbContext.Clientes,
                            nota => nota.id_cliente,
                            cli => cli.id_cliente,
                            (nota, cli) => new
                            {
                                nota.id_cliente,
                                nota.id_nota,
                                nota.total,
                                nota.pagada,
                                nota.estado,
                                nota.fecha,
                                nota.numero,                                    
                                Cliente = cli.nombre + " | " + cli.pseudonimo
                            }
                        )
                        .Where(N => N.numero == folio.ToString())
                        .ToList();

                    dgNotas.ItemsSource = notes;
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

                        var notes = dbContext.Notas
                        .Join(
                            dbContext.Clientes,
                            nota => nota.id_cliente,
                            cli => cli.id_cliente,
                            (nota, cli) => new
                            {
                                nota.id_cliente,
                                nota.id_nota,
                                nota.total,
                                nota.pagada,
                                nota.estado,
                                nota.fecha,
                                nota.numero,
                                Cliente = cli.nombre + " | " + cli.pseudonimo
                            }
                        )                        
                        .Where(F => F.fecha == fecha)
                        .ToList();
                        
                        dgNotas.ItemsSource = notes;

                    }
                }
                catch (Exception exc)
                {

                }
            }
        }
    }
}
