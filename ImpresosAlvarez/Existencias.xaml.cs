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
    /// Lógica de interacción para Existencias.xaml
    /// </summary>
    public partial class Existencias : Window
    {
        List<Categorias> categorias;
        List<Insumos> insumos;
        public Existencias()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    categorias = dbContext.Categorias.ToList();
                    cbCategorias.ItemsSource = categorias;
                    cbCategorias.SelectedValuePath = "id_categoria";
                    cbCategorias.DisplayMemberPath = "nombre";
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Categorias c = (Categorias)cbCategorias.SelectedItem;
                    insumos = dbContext.Insumos.Where(I => I.id_categoria == c.id_categoria).ToList();
                    dgInsumos.ItemsSource = null;
                    dgInsumos.ItemsSource = insumos;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void ActualizarListaInsumos()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                Categorias c = (Categorias)cbCategorias.SelectedItem;
                insumos = dbContext.Insumos.Where(I => I.id_categoria == c.id_categoria).ToList();
                dgInsumos.ItemsSource = null;
                dgInsumos.ItemsSource = insumos;
            }
        }

        private void btnNuevoInsumo_Click(object sender, RoutedEventArgs e)
        {
            ControlInsumo nuevo = new ControlInsumo(this, "NUEVO", null);
            nuevo.ShowDialog();
        }

        private void btnModificarInsumo_Click(object sender, RoutedEventArgs e)
        {
            if (dgInsumos.SelectedItem != null)
            {
                Insumos i = (Insumos)dgInsumos.SelectedItem;
                ControlInsumo mod = new ControlInsumo(this, "MODIFICAR", i);
                mod.ShowDialog();
            } 
            else
            {
                MessageBox.Show("No ha elegido un insumo.");
            }
        }

        private void btnVerMovimientos_Click(object sender, RoutedEventArgs e)
        {
            if (dgInsumos.SelectedItem != null)
            {
                Insumos Insumo = (Insumos)dgInsumos.SelectedItem;
                MovimientosInventario movimientos = new MovimientosInventario(Insumo);
                movimientos.ShowDialog();
            }
        }
    }
}
