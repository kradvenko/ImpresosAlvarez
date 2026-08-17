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
    public partial class AdministrarCategorias : Window
    {
        private ImpresosBDEntities db;

        public AdministrarCategorias()
        {
            InitializeComponent();
            db = new ImpresosBDEntities();
            CargarCategorias();
        }

        private void CargarCategorias()
        {
            try
            {
                var categorias = db.Categorias.OrderBy(c => c.nombre).ToList();
                dgCategorias.ItemsSource = categorias;
                lblTotal.Content = categorias.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las categorías: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            NuevaCategoria nueva = new NuevaCategoria();
            nueva.ShowDialog();
            CargarCategorias();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategorias.SelectedItem is Categorias categoriaSeleccionada)
            {
                EditarCategoria ventanaEditar = new EditarCategoria(categoriaSeleccionada.id_categoria);
                ventanaEditar.ShowDialog();
                CargarCategorias();
            }
            else
            {
                MessageBox.Show("Por favor seleccione una categoría para editar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategorias.SelectedItem is Categorias categoriaSeleccionada)
            {
                // Verificar si la categoría tiene insumos asociados
                var insumosAsociados = db.Insumos.Count(i => i.id_categoria == categoriaSeleccionada.id_categoria);
                if (insumosAsociados > 0)
                {
                    MessageBox.Show($"No se puede eliminar la categoría porque tiene {insumosAsociados} insumo(s) asociado(s).", 
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var resultado = MessageBox.Show($"¿Está seguro que desea eliminar la categoría '{categoriaSeleccionada.nombre}'?", 
                    "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        var categoria = db.Categorias.Find(categoriaSeleccionada.id_categoria);
                        if (categoria != null)
                        {
                            db.Categorias.Remove(categoria);
                            db.SaveChanges();
                            MessageBox.Show("Categoría eliminada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarCategorias();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar la categoría: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor seleccione una categoría para eliminar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarCategorias();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            db?.Dispose();
        }
    }
}