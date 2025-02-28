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
    /// Lógica de interacción para CorreosCliente.xaml
    /// </summary>
    public partial class CorreosCliente : Window
    {
        Clientes ClienteElegido;
        List<Correos> ListaCorreos;
        Correos CorreoElegido;
        
        public CorreosCliente(Clientes ClienteElegido)
        {
            InitializeComponent();
            this.ClienteElegido = ClienteElegido;
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            CorreoElegido = (Correos)dgCorreos.SelectedItem;
            ControlCorreo correo = new ControlCorreo(null, "NUEVO", this, ClienteElegido.id_cliente);
            correo.ShowDialog();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCorreos.SelectedItem != null)
            {
                CorreoElegido = (Correos)dgCorreos.SelectedItem;
                ControlCorreo correo = new ControlCorreo(CorreoElegido, "MODIFICAR", this, 0);
                correo.ShowDialog();
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ClienteElegido != null)
            {
                try
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        ListaCorreos = dbContext.Correos.Where(T => T.id_cliente == ClienteElegido.id_cliente).ToList();
                        dgCorreos.ItemsSource = ListaCorreos;
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        public void ActualizarListaCorreos()
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    ListaCorreos = dbContext.Correos.Where(T => T.id_cliente == ClienteElegido.id_cliente).ToList();
                    dgCorreos.ItemsSource = ListaCorreos;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
