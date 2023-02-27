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
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        MainWindow parent;
        public Login(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        private void tbUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }

        private void tbClave_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Usuarios cu = dbContext.Usuarios.Where(U => U.pass == tbClave.Password).FirstOrDefault();

                    if (cu != null)
                    {
                        parent.CurrentUser = cu;
                        parent.SetMainWindow();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No se puede iniciar sesion.");
                    }
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbClave.Focus();
        }

        private void btnIngresoTaller_Click(object sender, RoutedEventArgs e)
        {
            /*
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                Usuarios cu = dbContext.Usuarios.Where(U => U.nombre == "TALLER" && U.pass == "TALLER").FirstOrDefault();

                if (cu != null)
                {
                    parent.CurrentUser = cu;
                    parent.SetMainWindow();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se puede iniciar sesion.");
                }
            }
            */
            Usuarios cu = new Usuarios();
            cu.id_usuario = 0;
            cu.nombre = "TALLER";
            cu.tipo = "TALLER";
            parent.CurrentUser = cu;
            parent.SetMainWindow();
            this.Close();
        }
    }
}
