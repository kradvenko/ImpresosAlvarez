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
                            "Select id_cliente From Ordenes " +
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
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
