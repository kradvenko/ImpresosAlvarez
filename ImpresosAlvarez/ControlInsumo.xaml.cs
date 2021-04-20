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
    /// Lógica de interacción para ControlInsumo.xaml
    /// </summary>
    public partial class ControlInsumo : Window
    {
        List<Categorias> categorias;

        Existencias Parent;
        String Modo;
        Insumos Insumo;

        public ControlInsumo(Existencias Parent, String Modo, Insumos Insumo)
        {
            InitializeComponent();
            this.Parent = Parent;
            this.Modo = Modo;
            this.Insumo = Insumo;
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

            if (Modo == "MODIFICAR")
            {
                cbCategorias.SelectedValue = Insumo.id_categoria;
                tbDescripcion.Text = Insumo.descripcion;
                tbStock.Text = Insumo.stock.ToString();
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (Modo == "MODIFICAR")
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Insumos InsumoM = dbContext.Insumos.Where(I => I.id_insumo == Insumo.id_insumo).FirstOrDefault();
                    InsumoM.id_categoria = int.Parse(cbCategorias.SelectedValue.ToString());
                    InsumoM.descripcion = tbDescripcion.Text;
                    InsumoM.stock = int.Parse(tbStock.Text);
                    InsumoM.estado = "ACTIVO";

                    //dbContext.Insumos.Add(Insumo);

                    dbContext.SaveChanges();

                    Parent.ActualizarListaInsumos();
                    this.Close();
                }
            }
            else if (Modo == "NUEVO")
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Insumo = new Insumos();
                    Insumo.id_categoria = int.Parse(cbCategorias.SelectedValue.ToString());                    
                    Insumo.descripcion = tbDescripcion.Text;
                    Insumo.stock = int.Parse(tbStock.Text);
                    Insumo.estado = "ACTIVO";

                    dbContext.Insumos.Add(Insumo);

                    dbContext.SaveChanges();

                    Parent.ActualizarListaInsumos();
                    this.Close();
                }
            }
        }
    }
}
