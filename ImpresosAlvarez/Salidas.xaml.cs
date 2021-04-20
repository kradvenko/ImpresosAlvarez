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
    /// Lógica de interacción para Salidas.xaml
    /// </summary>
    public partial class Salidas : Window
    {
        List<SalidasInventario> SalidasDia;
        public Salidas()
        {
            InitializeComponent();
        }

        private void btnNuevaSalida_Click(object sender, RoutedEventArgs e)
        {
            ControlSalidaInventario salida = new ControlSalidaInventario(this, "NUEVO", null);
            salida.ShowDialog();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void CargarSalidas()
        {
            if (dpFecha.SelectedDate != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    DateTime Fecha = dpFecha.SelectedDate.Value.Date;
                    SalidasDia = dbContext.SalidasInventario.Where(E => E.fecha == Fecha).ToList();
                    dgSalidas.ItemsSource = null;
                    dgSalidas.ItemsSource = SalidasDia;
                }
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarSalidas();
        }
    }
}
