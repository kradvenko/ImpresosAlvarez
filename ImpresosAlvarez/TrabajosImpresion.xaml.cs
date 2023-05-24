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
    /// Lógica de interacción para TrabajosImpresion.xaml
    /// </summary>
    public partial class TrabajosImpresion : Window
    {
        List<vOrdenesImpresion> TrabajosPendientes;
        public TrabajosImpresion()
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
                    InfoTallerImpresion info = new InfoTallerImpresion(this);
                    info.ShowDialog();
                }
            }
        }
        public void EnviarOrden(bool Aplica, int IdLamina, int IdNegativo, Usuarios UsuarioGuarda)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                vOrdenesImpresion elegida = (vOrdenesImpresion)dgTrabajos.SelectedItem;
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

                TrabajosPendientes = dbContext.vOrdenesImpresion.Where(T => T.estado == "IMPRESION").ToList();
                dgTrabajos.ItemsSource = TrabajosPendientes;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMostrarTodas_Click(object sender, RoutedEventArgs e)
        {
            tbBuscar.Text = "";
            CargarOrdenes();
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
                            TrabajosPendientes = dbContext.vOrdenesImpresion.AsNoTracking().Where(T => T.numero == x).ToList();
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
                        TrabajosPendientes = dbContext.vOrdenesImpresion.AsNoTracking().Where(T => T.numero == x).ToList();
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
                    TrabajosPendientes = dbContext.vOrdenesImpresion.AsNoTracking().Where(T => T.estado == "IMPRESION").ToList();
                    dgTrabajos.ItemsSource = TrabajosPendientes;
                }
            }
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrabajos.SelectedItem != null)
            {
                vOrdenesImpresion elegida = (vOrdenesImpresion)dgTrabajos.SelectedItem;
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
                vOrdenesImpresion elegida = (vOrdenesImpresion)dgTrabajos.SelectedItem;
                if (elegida.id_impresion is null)
                {
                    MessageBox.Show("No es una orden con un trabajo iniciado.");
                    return;
                }
                if (MessageBox.Show("Desea finalizar la orden y enviarla a terminado?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        Impresion imp = dbContext.Impresion.Where(T => T.id_impresion == elegida.id_impresion).First();
                        imp.fecha_fin = DateTime.Now.ToShortDateString();
                        imp.hora_fin = DateTime.Now.ToShortTimeString();

                        dbContext.SaveChanges();

                        Ordenes Orden = dbContext.Ordenes.Where(T => T.id_orden == elegida.id_orden).First();

                        Orden.estado = "TERMINADO";

                        dbContext.SaveChanges();

                        CargarOrdenes();
                    }
                }
            }
            else
            {
                MessageBox.Show("No ha elegido una orden.");
            }
        }
    }
}
