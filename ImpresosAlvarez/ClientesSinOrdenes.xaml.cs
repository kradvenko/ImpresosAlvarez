using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Lógica de interacción para ClientesSinOrdenes.xaml
    /// </summary>
    public partial class ClientesSinOrdenes : Window
    {
        List<Clientes> ListaClientes;
        Clientes _clienteElegido;
        public ClientesSinOrdenes()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListaClientes = new List<Clientes>();
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImpresosBDConn"].ConnectionString))
                    {
                        using (SqlCommand comm = new SqlCommand(" " +
                            "Select * From Clientes " +
                            "Where id_cliente " +
                            "Not In( " +
                            "   Select id_cliente From Ordenes " +
                            ") " +
                            "", con))
                        {
                            con.Open();

                            SqlDataReader reader = comm.ExecuteReader();

                            while (reader.Read())
                            {
                                int idc = int.Parse(reader["id_cliente"].ToString());
                                Clientes clientes = new Clientes();
                                clientes = dbContext.Clientes.Where(C => C.id_cliente == idc).First();
                                ListaClientes.Add(clientes);
                            }

                            con.Close();
                        }
                    }
                }
                catch
                {

                }
                dgClientes.ItemsSource = ListaClientes;
                tbClientes.AutoCompleteSource = ListaClientes;
                lblTotalClientes.Content = "Total de clientes sin ordenes: " + ListaClientes.Count.ToString();
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Desea eliminar el cliente?", "ATENCIO", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (Clientes c in dgClientes.SelectedItems)
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        Clientes cliente = dbContext.Clientes.Where(C => C.id_cliente == c.id_cliente).First();
                        dbContext.Clientes.Remove(cliente);
                        dbContext.SaveChanges();
                    }
                }
                MessageBox.Show("Cliente(s) eliminado(s)");
                Window_Loaded(null, null);
            }
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListaClientes = new List<Clientes>();
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImpresosBDConn"].ConnectionString))
                        {
                            using (SqlCommand comm = new SqlCommand(" " +
                                "Select * From Clientes " +
                                "Where id_cliente = @IdCliente" +
                                "", con))
                            {
                                con.Open();
                                comm.Parameters.AddWithValue("@IdCliente", _clienteElegido.id_cliente);

                                SqlDataReader reader = comm.ExecuteReader();

                                while (reader.Read())
                                {
                                    int idc = int.Parse(reader["id_cliente"].ToString());
                                    Clientes clientes = new Clientes();
                                    clientes = dbContext.Clientes.Where(C => C.id_cliente == idc).First();
                                    ListaClientes.Add(clientes);
                                }

                                con.Close();
                            }
                        }
                    }
                    catch
                    {

                    }
                    dgClientes.ItemsSource = ListaClientes;
                }
            }
        }
    }
}
