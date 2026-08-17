using ImpresosAlvarez.Entity;
using System;
using System.Linq;
using System.Windows;

namespace ImpresosAlvarez
{
    public partial class NuevaCategoria : Window
    {
        private ImpresosBDEntities db;

        public NuevaCategoria()
        {
            InitializeComponent();
            db = new ImpresosBDEntities();
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatos())
                return;

            try
            {
                var categoria = new Categorias
                {
                    nombre = txtNombre.Text.Trim()
                };

                db.Categorias.Add(categoria);
                db.SaveChanges();

                MessageBox.Show("Categoría agregada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
                txtNombre.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la categoría: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Verificar si ya existe una categoría con el mismo nombre
            string nombreCategoria = txtNombre.Text.Trim();
            if (db.Categorias.Any(c => c.nombre.ToLower() == nombreCategoria.ToLower()))
            {
                MessageBox.Show("Ya existe una categoría con ese nombre.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
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
