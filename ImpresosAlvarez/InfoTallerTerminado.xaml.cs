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
    /// Lógica de interacción para InfoTallerTerminado.xaml
    /// </summary>
    public partial class InfoTallerTerminado : Window
    {
        List<Usuarios> UsuariosTaller;
        TrabajosTerminado ParentFormTerminado;
        public InfoTallerTerminado(TrabajosTerminado ParentForm)
        {
            InitializeComponent();
            this.ParentFormTerminado = ParentForm;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                UsuariosTaller = dbContext.Usuarios.Where(T => T.tipo == "IMPRESION" && T.estado == "ACTIVO").ToList();
                cbUsuarios.ItemsSource = UsuariosTaller;
                cbUsuarios.DisplayMemberPath = "nombre";
                cbUsuarios.SelectedValuePath = "id_usuario";
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnNoAplica_Click(object sender, RoutedEventArgs e)
        {
            if (cbUsuarios.SelectedItem == null)
            {
                MessageBox.Show("No ha elegido un usuario.");
                return;
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Usuarios u = (Usuarios)cbUsuarios.SelectedItem;

                    if (u != null)
                    {
                        ParentFormTerminado.EnviarOrden(false, 0, 0, u);

                        this.Close();
                    }
                }
            }
        }

        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (cbUsuarios.SelectedItem == null)
            {
                MessageBox.Show("No ha elegido un usuario.");
                return;
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Usuarios u = (Usuarios)cbUsuarios.SelectedItem;

                    if (u != null)
                    {
                        Usuarios Lamina;
                        Usuarios Negativo;

                        ParentFormTerminado.EnviarOrden(true, 0, 0, u);

                        this.Close();
                    }
                }
            }
        }
    }
}