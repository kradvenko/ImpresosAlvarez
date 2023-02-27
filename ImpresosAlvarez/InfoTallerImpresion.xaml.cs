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
    /// Lógica de interacción para InfoTallerImpresion.xaml
    /// </summary>
    public partial class InfoTallerImpresion : Window
    {
        List<Usuarios> UsuariosTaller;
        TrabajosImpresion ParentFormImpresion;
        public InfoTallerImpresion(TrabajosImpresion ParentForm)
        {
            InitializeComponent();
            this.ParentFormImpresion = ParentForm;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                UsuariosTaller = dbContext.Usuarios.Where(T => T.tipo == "IMPRESION" && T.estado == "ACTIVO").ToList();

                cbLamina.ItemsSource = UsuariosTaller;
                cbLamina.DisplayMemberPath = "nombre";
                cbLamina.SelectedValuePath = "id_usuario";
                cbNegativo.ItemsSource = UsuariosTaller;
                cbNegativo.DisplayMemberPath = "nombre";
                cbNegativo.SelectedValuePath = "id_usuario";
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
                        ParentFormImpresion.EnviarOrden(false, 0, 0, u);

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

                        ParentFormImpresion.EnviarOrden(true, int.Parse(cbLamina.SelectedValue.ToString()), int.Parse(cbNegativo.SelectedValue.ToString()), u);

                        this.Close();
                    }
                }
            }
        }
    }
}
