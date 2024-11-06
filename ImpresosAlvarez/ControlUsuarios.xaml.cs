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
    /// Lógica de interacción para ControlUsuarios.xaml
    /// </summary>
    public partial class ControlUsuarios : Window
    {
        public List<Usuarios> ListaUsuarios;
        public ControlUsuarios()
        {
            InitializeComponent();
        }

        private void dgUsuarios_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListaUsuarios = new List<Usuarios>();
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaUsuarios = dbContext.Usuarios.ToList();
                dgUsuarios.ItemsSource = ListaUsuarios;
            }
        }

        private void dgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem != null)
            {
                Usuarios ele = (Usuarios)dgUsuarios.SelectedItem;

                tbNombre.Text = ele.nombre;
                tbPass.Text = ele.pass;
                cbTipo.Text = ele.tipo;
                cbEstado.Text = ele.estado;
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem != null)
            {
                String Nombre = tbNombre.Text;
                String Pass = tbPass.Text;
                String Tipo = cbTipo.Text;
                String Estado = cbEstado.Text;

                Usuarios ele = (Usuarios)dgUsuarios.SelectedItem;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Usuarios u = dbContext.Usuarios.Where(T => T.id_usuario == ele.id_usuario).First();

                    u.nombre = Nombre;
                    u.pass = Pass;
                    u.tipo = Tipo;
                    u.estado = Estado;

                    dbContext.SaveChanges();

                    ListaUsuarios = dbContext.Usuarios.ToList();
                    dgUsuarios.ItemsSource = ListaUsuarios;

                    MessageBox.Show("Se ha actualizado el usuario");
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
