using ImpresosAlvarez.Clases;
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
    /// Lógica de interacción para ControlEntradaInventario.xaml
    /// </summary>
    public partial class ControlEntradaInventario : Window
    {
        Entradas Parent;
        String Modo;
        List<Insumos> _insumos;
        Entrada EntradaInventario;
        Insumos InsumoElegido;
        
        public ControlEntradaInventario(Entradas Parent, String Modo, Entrada EntradaInventario)
        {
            InitializeComponent();
            this.Parent = Parent;
            this.Modo = Modo;
            this.EntradaInventario = EntradaInventario;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInsumos();
            if (Modo == "MODIFICAR")
            {

            }
        }

        public void CargarInsumos()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _insumos = dbContext.Insumos.ToList();
                tbInsumos.AutoCompleteSource = _insumos;
            }
        }

        private void btnAgregarEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (Modo == "NUEVO")
            {
                if (dpFecha.SelectedDate == null)
                {
                    MessageBox.Show("No ha elegido una fecha.");
                    dpFecha.Focus();
                    return;
                }
                float cantidad;
                if (!float.TryParse(tbCantidad.Text, out cantidad))
                {
                    MessageBox.Show("No ha ingresado una cantidad correcta.");
                    tbCantidad.Focus();
                    return;
                }
                if (tbInsumos.SelectedItem == null)
                {
                    MessageBox.Show("No ha elegido un insumo.");
                    tbInsumos.Focus();
                    return;
                }

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    EntradasInventario nueva = new EntradasInventario();
                    nueva.fecha = dpFecha.SelectedDate.Value;
                    nueva.proveedor = tbProveedor.Text;
                    nueva.factura = tbFactura.Text;
                    nueva.cantidad = float.Parse(tbCantidad.Text);
                    nueva.id_insumo = InsumoElegido.id_insumo;
                    nueva.descripcion_insumo = InsumoElegido.descripcion;

                    dbContext.EntradasInventario.Add(nueva);

                    Insumos insumo = dbContext.Insumos.Where(I => I.id_insumo == InsumoElegido.id_insumo).First();
                    insumo.stock = insumo.stock + nueva.cantidad;

                    dbContext.SaveChanges();

                    Parent.CargarEntradas();

                    this.Close();
                }
            }
            else if (Modo == "MODIFICAR")
            {

            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbInsumos_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbInsumos.SelectedItem != null)
            {
                InsumoElegido = (Insumos)tbInsumos.SelectedItem;
            }
        }
    }
}
