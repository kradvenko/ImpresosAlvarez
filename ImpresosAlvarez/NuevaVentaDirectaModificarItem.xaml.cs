using ImpresosAlvarez.Clases;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
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
    /// Lógica de interacción para NuevaVentaDirectaModificarItem.xaml
    /// </summary>
    public partial class NuevaVentaDirectaModificarItem : Window
    {
        NuevaVentaDirecta Parent;
        VentaDirectaItem Item;
        public NuevaVentaDirectaModificarItem(NuevaVentaDirecta parent, VentaDirectaItem item)
        {
            InitializeComponent();
            Parent = parent;
            Item = item;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbCantidad.Text = Item.cantidad.ToString();
            tbPrecio.Text = Item.precio.ToString();
            tbTotal.Text = Item.total.ToString();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Item.cantidad = int.Parse(tbCantidad.Text);
            Item.precio = double.Parse(tbPrecio.Text);
            Item.total = Item.cantidad * Item.precio;
            Parent.ActualizarItems();
            this.Close();
        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(tbCantidad.Text, out int cantidad) && double.TryParse(tbPrecio.Text, out double precio))
                {
                    tbTotal.Text = (cantidad * precio).ToString();
                }
                else
                {
                    tbTotal.Text = "0";
                }
            }
        }

        private void tbPrecio_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(tbCantidad.Text, out int cantidad) && double.TryParse(tbPrecio.Text, out double precio))
                {
                    tbTotal.Text = (cantidad * precio).ToString();
                }
                else
                {
                    tbTotal.Text = "0";
                }
            }
        }
    }    
}
