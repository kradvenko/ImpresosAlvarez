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
    /// Lógica de interacción para VerCotizacionesArticuloInfo.xaml
    /// </summary>
    public partial class VerCotizacionesArticuloInfo : Window
    {
        int Cantidad = 0;
        double PrecioUnitario = 0;
        double Importe = 0;

        VerCotizaciones _parent;
        CotizacionLlenado _articulo;

        List<Insumos> Productos;
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
                if (tbProductos.Text != "")
                {
                    Insumos Ins = (Insumos)tbProductos.SelectedItem;
                    _articulo.IdInsumo = Ins.id_insumo;
                    _articulo.DescripcionInsumo = Ins.descripcion;
                }
                else
                {
                    _articulo.IdInsumo = 0;
                    _articulo.DescripcionInsumo = "";
                }
                _parent.AgregarArticulo(_articulo);
                this.Close();
            }
            else
            {
                _articulo.Cantidad = Cantidad;
                _articulo.Descripcion = tbDescripcion.Text;
                _articulo.PrecioUnitario = PrecioUnitario;
                _articulo.Importe = Importe;
                if (tbProductos.Text != "")
                {
                    Insumos Ins = (Insumos)tbProductos.SelectedItem;
                    _articulo.IdInsumo = Ins.id_insumo;
                    _articulo.DescripcionInsumo = Ins.descripcion;
                }
                else
                {
                    _articulo.IdInsumo = 0;
                    _articulo.DescripcionInsumo = "";
                }
                _parent.ActualizarArticulo(_articulo);
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CalcularTotales(int opt)
        {
            if (opt == 1)
            {
                PrecioUnitario = Math.Round(PrecioUnitario, 4);
                Importe = PrecioUnitario * Cantidad;
                tbImporte.Text = Importe.ToString();
            }
            else if (opt == 2)
            {
                PrecioUnitario = Math.Round(PrecioUnitario, 4);
                Importe = PrecioUnitario * Cantidad;
                tbImporte.Text = Importe.ToString();
            }
            else if (opt == 3)
            {
                PrecioUnitario = Importe / Cantidad;
                PrecioUnitario = Math.Round(PrecioUnitario, 4);
                tbPrecioUnitario.Text = PrecioUnitario.ToString();
            }
            
            //lblPrecioUnitario.Content = PrecioUnitario.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                Productos = dbContext.Insumos.ToList();

                tbProductos.AutoCompleteSource = Productos;
            }

            if (_articulo != null)
            {
                Cantidad = _articulo.Cantidad;
                tbCantidad.Text = Cantidad.ToString(); ;
                tbDescripcion.Text = _articulo.Descripcion;
                PrecioUnitario = _articulo.PrecioUnitario;
                tbPrecioUnitario.Text = PrecioUnitario.ToString();
                //lblPrecioUnitario.Content = PrecioUnitario.ToString();
                Importe = _articulo.Importe;
                tbImporte.Text = Importe.ToString();
                if (_articulo.IdInsumo != 0)
                {
                    tbProductos.Text = _articulo.DescripcionInsumo;
                }
            }
            tbCantidad.Focus();
        }

        private void tbPrecioUnitario_KeyUp(object sender, KeyEventArgs e)
        {
            if (double.TryParse(tbPrecioUnitario.Text, out PrecioUnitario))
            {
                PrecioUnitario = double.Parse(tbPrecioUnitario.Text);
                CalcularTotales(2);
            }
            else
            {
                tbPrecioUnitario.Text = PrecioUnitario.ToString();
            }
        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {
            if (int.TryParse(tbCantidad.Text, out Cantidad))
            {
                Cantidad = int.Parse(tbCantidad.Text);
                CalcularTotales(1);
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
                CalcularTotales(3);
            }
            else
            {
                //lblPrecioUnitario.Content = PrecioUnitario.ToString();
                tbPrecioUnitario.Text = PrecioUnitario.ToString();
            }
        }

        private void tbProductos_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
