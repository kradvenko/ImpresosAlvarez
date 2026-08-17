using ImpresosAlvarez.Entity;
using System;
using System.Linq;
using System.Windows;

namespace ImpresosAlvarez
{
    public partial class EditarCategoria : Window
    {
        private ImpresosBDEntities db;
        private int idCategoria;

        public EditarCategoria(int idCategoriaEditar)
        {
            InitializeComponent();
            db = new ImpresosBDEntities();
            idCategoria = idCategoriaEditar;
            CargarCategoria();
        }

        private void CargarCategoria()
        {
            try
            {
                var categoria = db.Categorias.Find(idCategoria);
                if (categoria != null)
                {
                    txtNombre.Text = categoria.nombre;
                    txtNombre.Focus();
                    txtNombre.SelectAll();
                }
                else
                {
                    MessageBox.Show("No se encontró la categoría.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la categoría: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatos())
                return;

            try
            {
                var categoria = db.Categorias.Find(idCategoria);
                if (categoria != null)
                {
                    categoria.nombre = txtNombre.Text.Trim();
                    db.SaveChanges();

                    MessageBox.Show("Categoría actualizada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la categoría: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre de la categoría es requerido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            // Verificar si ya existe otra categoría con el mismo nombre
            string nombreCategoria = txtNombre.Text.Trim();
            if (db.Categorias.Any(c => c.nombre.ToLower() == nombreCategoria.ToLower() && c.id_categoria != idCategoria))
            {
                MessageBox.Show("Ya existe otra categoría con ese nombre.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            return true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            db?.Dispose();
        }
    }
}