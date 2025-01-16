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
    /// Lógica de interacción para UsuarioIniciaTermina.xaml
    /// </summary>
    public partial class UsuarioIniciaTermina : Window
    {
        List<Usuarios> UsuariosTaller;
        TrabajosTerminado ParentFormTerminado;
        TrabajosImpresion ParentFormImpresion;
        vOrdenesTerminado OrdenElegidaTerminado;
        vOrdenesImpresion OrdenElegidaImpresion;
        String Modo;
        String TipoUsuario;
        public UsuarioIniciaTermina(vOrdenesTerminado OrdenElegidaTerminado, TrabajosTerminado ParentFormTerminado, String Modo)
        {
            InitializeComponent();
            this.OrdenElegidaTerminado = OrdenElegidaTerminado;
            this.ParentFormTerminado = ParentFormTerminado;
            this.Modo = Modo;
            this.TipoUsuario = "TERMINADO";
        }

        public UsuarioIniciaTermina(vOrdenesImpresion OrdenElegidaImpresion, TrabajosImpresion ParentFormTerminado, String Modo)
        {
            InitializeComponent();
            this.OrdenElegidaImpresion = OrdenElegidaImpresion;
            this.ParentFormImpresion = ParentFormTerminado;
            this.Modo = Modo;
            this.TipoUsuario = "IMPRESION";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                UsuariosTaller = dbContext.Usuarios.Where(T => T.tipo == TipoUsuario && T.estado == "ACTIVO").ToList();
                
                cbUsuarios.ItemsSource = UsuariosTaller;
                cbUsuarios.DisplayMemberPath = "nombre";
                cbUsuarios.SelectedValuePath = "id_usuario";
            }

            if (ParentFormImpresion != null)
            {
                lblTipoTerminado.Visibility = Visibility.Hidden;
                tbTipoTerminado.Visibility = Visibility.Hidden;
            }
            else if (ParentFormTerminado != null)
            {
                
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            Usuarios u = new Usuarios();
            if (cbUsuarios.SelectedItem != null)
            {
                u = (Usuarios)cbUsuarios.SelectedItem;
            }
            else
            {
                MessageBox.Show("No ha elegido un usuario.");
                return;
            }
            if (Modo == "INICIA")
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    

                    if (ParentFormImpresion != null)
                    {
                        Ordenes orden = dbContext.Ordenes.Where(T => T.id_orden == OrdenElegidaImpresion.id_orden).First();
                        orden.inicio_impresion = DateTime.Now.ToShortDateString();
                        dbContext.SaveChanges();

                        Impresion imp = new Impresion();
                        imp.id_orden = OrdenElegidaImpresion.id_orden;
                        imp.id_usuario = u.id_usuario;
                        imp.fecha_inicio = DateTime.Now.ToShortDateString();
                        imp.hora_inicio = DateTime.Now.ToShortTimeString();
                        imp.fecha_fin = "";
                        imp.hora_fin = "";
                        imp.notas = "";
                        dbContext.Impresion.Add(imp);

                        dbContext.SaveChanges();

                        ParentFormImpresion.CargarOrdenes();
                    }
                    else if (ParentFormTerminado != null)
                    {
                        Ordenes orden = dbContext.Ordenes.Where(T => T.id_orden == OrdenElegidaTerminado.id_orden).First();
                        orden.inicio_terminado = DateTime.Now.ToShortDateString();
                        dbContext.SaveChanges();
                        
                        Terminado ter = new Terminado();
                        ter.id_orden = OrdenElegidaTerminado.id_orden;
                        ter.id_usuario = u.id_usuario;
                        ter.fecha_inicio = DateTime.Now.ToShortDateString();
                        ter.hora_inicio = DateTime.Now.ToShortTimeString();
                        ter.fecha_fin = "";
                        ter.hora_fin = "";
                        ter.descripcion = "";
                        ter.tipo_terminado = tbTipoTerminado.Text;
                        dbContext.Terminado.Add(ter);

                        dbContext.SaveChanges();

                        ParentFormTerminado.CargarOrdenes();
                    }
                    this.Close();
                }
            }
            else if (Modo == "FINALIZA")
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    if (ParentFormImpresion != null)
                    {
                        Impresion imp = dbContext.Impresion.Where(T => T.id_orden == OrdenElegidaTerminado.id_orden).First();
                        imp.fecha_fin = DateTime.Now.ToShortDateString();
                        imp.hora_fin = DateTime.Now.ToShortTimeString();
                        dbContext.Impresion.Add(imp);

                        dbContext.SaveChanges();

                        ParentFormImpresion.CargarOrdenes();
                    }
                    else if (ParentFormTerminado != null)
                    {
                        Terminado ter = dbContext.Terminado.Where(T => T.id_orden == OrdenElegidaTerminado.id_orden).First();
                        ter.fecha_fin = DateTime.Now.ToShortDateString();
                        ter.hora_fin = DateTime.Now.ToShortTimeString();
                        dbContext.Terminado.Add(ter);

                        dbContext.SaveChanges();

                        ParentFormTerminado.CargarOrdenes();
                    }
                    this.Close();
                }
            }
        }
    }
}
