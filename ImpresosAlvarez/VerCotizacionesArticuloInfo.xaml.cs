using ImpresosAlvarez.Clases;
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
    /// Lógica de interacción para VerCotizacionesArticuloInfo.xaml
    /// </summary>
    public partial class VerCotizacionesArticuloInfo : Window
    {
        int Cantidad = 0;
        double PrecioUnitario = 0;
        double Importe = 0;

        VerCotizaciones _parent;
        CotizacionLlenado _articulo;
        public VerCotizacionesArticuloInfo(VerCotizaciones parent, CotizacionLlenado articulo)
        {
            InitializeComponent();
            _articulo = articulo;
            _parent = parent;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_articulo == null)
            {
                _articulo = new CotizacionLlenado();
                _articulo.Cantidad = Cantidad;
                _articulo.Descripcion = tbDescripcion.Text;
                _articulo.PrecioUnitario = PrecioUnitario;
                _articulo.Importe = Importe;
                _parent.AgregarArticulo(_articulo);
                this.Close();
            }
            else
            {
                _articulo.Cantidad = Cantidad;
                _articulo.Descripcion = tbDescripcion.Text;
                _articulo.PrecioUnitario = PrecioUnitario;
                _articulo.Importe = Importe;
                _parent.ActualizarArticulo(_articulo);
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CalcularTotales()
        {
            PrecioUnitario = Importe / Cantidad;
            PrecioUnitario = Math.Round(PrecioUnitario, 2);
            lblPrecioUnitario.Content = PrecioUnitario.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_articulo != null)
            {
                Cantidad = _articulo.Cantidad;
                tbCantidad.Text = Cantidad.ToString(); ;
                tbDescripcion.Text = _articulo.Descripcion;
                PrecioUnitario = _articulo.PrecioUnitario;
                lblPrecioUnitario.Content = PrecioUnitario.ToString();
                Importe = _articulo.Importe;
                tbImporte.Text = Importe.ToString();
            }
            tbCantidad.Focus();
        }

        private void tbPrecioUnitario_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {
            if (int.TryParse(tbCantidad.Text, out Cantidad))
            {
                Cantidad = int.Parse(tbCantidad.Text);
                CalcularTotales();
            }
            else
            {
                tbCantidad.Text = Cantidad.ToString();
            }
        }

        private void tbImporte_KeyUp(object sender, KeyEventArgs e)
        {
            if (double.TryParse(tbImporte.Text, out Importe))
            {
                Importe = double.Parse(tbImporte.Text);
                CalcularTotales();
            }
            else
            {
                lblPrecioUnitario.Content = PrecioUnitario.ToString();
            }
        }
    }
}
