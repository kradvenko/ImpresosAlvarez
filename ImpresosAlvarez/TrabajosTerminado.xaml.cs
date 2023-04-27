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
    /// Lógica de interacción para TrabajosTerminado.xaml
    /// </summary>
    public partial class TrabajosTerminado : Window
    {
        List<vOrdenesTerminado> TrabajosPendientes;
        public TrabajosTerminado()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarOrdenes();
        }

        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrabajos.SelectedItem != null)
            {
                if (MessageBox.Show("Desea enviar a entrega?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    InfoTallerTerminado info = new InfoTallerTerminado(this);
                    info.ShowDialog();
                }
            }
        }
        public void EnviarOrden(bool Aplica, int IdLamina, int IdNegativo, Usuarios UsuarioGuarda)
        {            
            if (dgTrabajos.SelectedItem != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    vOrdenesTerminado elegida = (vOrdenesTerminado)dgTrabajos.SelectedItem;
                    Ordenes orden = dbContext.Ordenes.Where(T => T.id_orden == elegida.id_orden).First();
                    orden.estado = "POR ENTREGAR";
                    orden.inicio_por_entregar = DateTime.Now.ToShortDateString();
                    dbContext.SaveChanges();

                    if (Aplica)
                    {
                        Entity.InfoTaller info = new Entity.InfoTaller();
                        info.IdUsuarioLamina = IdLamina;
                        info.IdUsuarioNegativo = IdNegativo;
                        info.Fecha = DateTime.Now;
                        info.IdUsuarioGuarda = UsuarioGuarda.id_usuario;
                        dbContext.InfoTaller.Add(info);
                        dbContext.SaveChanges();
                    }

                    TrabajosPendientes = dbContext.vOrdenesTerminado.Where(T => T.estado == "TERMINADO").ToList();
                    dgTrabajos.ItemsSource = TrabajosPendientes;
                }
            }
            else
            {
                MessageBox.Show("No ha elegido una orden.");
                return;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrabajos.SelectedItem != null)
            {
                vOrdenesTerminado elegida = (vOrdenesTerminado)dgTrabajos.SelectedItem;
                UsuarioIniciaTermina inicia = new UsuarioIniciaTermina(elegida, this, "INICIA");
                inicia.ShowDialog();
            }
            else
            {
                MessageBox.Show("No ha elegido una orden.");
            }
        }

        private void btnTerminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrabajos.SelectedItem != null)
            {
                vOrdenesTerminado elegida = (vOrdenesTerminado)dgTrabajos.SelectedItem;
                if (elegida.id_terminado is null)
                {
                    MessageBox.Show("No es una orden con un trabajo iniciado.");
                    return;
                }
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Terminado ter = dbContext.Terminado.Where(T => T.id_terminado == elegida.id_terminado).First();
                    ter.fecha_fin = DateTime.Now.ToShortDateString();
                    ter.hora_fin = DateTime.Now.ToShortTimeString();

                    dbContext.SaveChanges();

                    CargarOrdenes();
                }
            }
            else
            {
                MessageBox.Show("No ha elegido una orden.");
            }
        }
        public void CargarOrdenes()
        {
            if (tbBuscar.Text.Length > 0)
            {
                int x;
                if (int.TryParse(tbBuscar.Text, out x))
                {
                    x = int.Parse(tbBuscar.Text);
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        TrabajosPendientes = dbContext.vOrdenesTerminado.AsNoTracking().Where(T => T.numero == x).ToList();
                        dgTrabajos.ItemsSource = TrabajosPendientes;
                    }
                }
                else
                {
                    MessageBox.Show("No ha escrito un número de orden correcto.");
                    tbBuscar.Text = "";
                }
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    TrabajosPendientes = dbContext.vOrdenesTerminado.AsNoTracking().Where(T => T.estado == "TERMINADO").ToList();
                    dgTrabajos.ItemsSource = TrabajosPendientes;
                }
            }
        }

        private void tbBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbBuscar.Text.Length > 0)
                {
                    int x;
                    if (int.TryParse(tbBuscar.Text, out x))
                    {
                        x = int.Parse(tbBuscar.Text);
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            TrabajosPendientes = dbContext.vOrdenesTerminado.AsNoTracking().Where(T => T.numero == x).ToList();
                            dgTrabajos.ItemsSource = TrabajosPendientes;
                        }
                    }
                    else
                    {
                        MessageBox.Show("No ha escrito un número de orden correcto.");
                        tbBuscar.Text = "";
                    }
                }
                else
                {
                    CargarOrdenes();
                }
            }
        }

        private void btnMostrarTodas_Click(object sender, RoutedEventArgs e)
        {
            tbBuscar.Text = "";
            CargarOrdenes();
        }
    }
}
