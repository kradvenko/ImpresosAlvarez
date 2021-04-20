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
    /// Lógica de interacción para Entradas.xaml
    /// </summary>
    public partial class Entradas : Window
    {
        List<EntradasInventario> EntradasDia;
        public Entradas()
        {
            InitializeComponent();
        }

        private void btnNuevaEntrada_Click(object sender, RoutedEventArgs e)
        {
            ControlEntradaInventario entrada = new ControlEntradaInventario(this, "NUEVO", null);
            entrada.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarEntradas();
        }

        public void CargarEntradas()
        {
            if (dpFecha.SelectedDate != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    DateTime Fecha = dpFecha.SelectedDate.Value.Date;
                    EntradasDia = dbContext.EntradasInventario.Where(E => E.fecha == Fecha).ToList();
                    dgEntradas.ItemsSource = null;
                    dgEntradas.ItemsSource = EntradasDia;
                }
            }
        }
    }
}
