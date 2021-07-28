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
    /// Lógica de interacción para ControlConceptoFactura.xaml
    /// </summary>
    public partial class ControlConceptoFactura : Window
    {
        List<ProductosServicios> _servicios;
        ProductosServicios _ServicioSeleccionado;
        FacturaDigital _Parent;
        String _Modo;
        ConceptoFactura _Concepto;
        float Total;
        List<Unidades> _unidades;

        List<ProductoServicioAutocomplete> _serviciosAutocomplete;

        List<Insumos> Productos;
        public ControlConceptoFactura(FacturaDigital Parent, String Modo, ConceptoFactura Concepto)
        {
            InitializeComponent();
            _Parent = Parent;
            _Modo = Modo;
            _Concepto = Concepto;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _servicios = dbContext.ProductosServicios.ToList();

                _serviciosAutocomplete = new List<ProductoServicioAutocomplete>();

                foreach (ProductosServicios item in _servicios)
                {
                    ProductoServicioAutocomplete p = new ProductoServicioAutocomplete();
                    p.IdServicio = item.id_productoservicio;
                    p.Clave = item.clave;
                    p.Descripcion = p.Descripcion;
                    p.AutoCompleteText = item.clave + " " + item.descripcion;

                    _serviciosAutocomplete.Add(p);
                }

                tbServicios.AutoCompleteSource = _serviciosAutocomplete;

                _unidades = dbContext.Unidades.ToList();
                cbUnidad.ItemsSource = _unidades;
                cbUnidad.DisplayMemberPath = "unidad";
                cbUnidad.SelectedValuePath = "clave";

                Productos = dbContext.Insumos.ToList();

                tbProductos.AutoCompleteSource = Productos;
            }
            if (_Modo == "MODIFICAR")
            {
                tbCantidad.Text = _Concepto.Cantidad.ToString();
                tbDescripcion.Text = _Concepto.Descripcion;
                tbPrecioUnitario.Text = _Concepto.PrecioUnitario.ToString();
                tbImporte.Text = _Concepto.Importe.ToString();
                tbServicios.Text = _Concepto.Servicio.descripcion;
                lblClaveServicio.Content = _Concepto.Servicio.clave;
                _ServicioSeleccionado = _Concepto.Servicio;
                cbUnidad.Text = _Concepto.Unidad;

                tbProductos.Text = _Concepto.DescripcionInsumo;
            }
            else
            {
                cbUnidad.SelectedIndex = 0;
                tbDescripcion.Focus();
            }
        }

        private void btnAgregarConcepto_Click(object sender, RoutedEventArgs e)
        {
            if (_Modo == "NUEVO")
            {
                if (tbCantidad.Text.Length == 0)
                {
                    MessageBox.Show("No ha ingresado la cantidad.");
                    return;
                }

                int c;

                if (!int.TryParse(tbCantidad.Text, out c))
                {
                    MessageBox.Show("No ha ingresado una cantidad correcta.");
                    return;
                }

                if (tbDescripcion.Text.Length == 0)
                {
                    MessageBox.Show("No ha ingresado la descripción.");
                    return;
                }

                if (tbPrecioUnitario.Text.Length == 0)
                {
                    MessageBox.Show("No ha ingresado el precio.");
                    return;
                }

                double p;

                if (!double.TryParse(tbPrecioUnitario.Text, out p))
                {
                    MessageBox.Show("No ha ingresado una precio correcto.");
                    return;
                }

                if (_ServicioSeleccionado == null)
                {
                    MessageBox.Show("No ha seleccionado un servicio.");
                    return;
                }

                CalcularTotal();
                ConceptoFactura concepto = new ConceptoFactura();
                concepto.IdServicio = _ServicioSeleccionado.id_productoservicio;
                concepto.Cantidad = int.Parse(tbCantidad.Text);
                concepto.Descripcion = tbDescripcion.Text;
                concepto.PrecioUnitario = float.Parse(tbPrecioUnitario.Text);
                concepto.Importe = Total;
                concepto.Unidad = cbUnidad.Text;
                concepto.Clave = _ServicioSeleccionado.clave;
                concepto.claveUnidad = cbUnidad.SelectedValue.ToString();
                concepto.Servicio = _ServicioSeleccionado;

                if (tbProductos.Text != "")
                {
                    Insumos Ins = (Insumos)tbProductos.SelectedItem;
                    concepto.IdInsumo = Ins.id_insumo;
                    concepto.DescripcionInsumo = Ins.descripcion;
                }
                else
                {
                    concepto.IdInsumo = 0;
                    concepto.DescripcionInsumo = "";
                }

                _Parent.AgregarConcepto(concepto);

                this.Close();
            }
            else
            {
                if (tbCantidad.Text.Length == 0)
                {
                    MessageBox.Show("No ha ingresado la cantidad.");
                    return;
                }

                int c;

                if (!int.TryParse(tbCantidad.Text, out c))
                {
                    MessageBox.Show("No ha ingresado una cantidad correcta.");
                    return;
                }

                if (tbDescripcion.Text.Length == 0)
                {
                    MessageBox.Show("No ha ingresado la descripción.");
                    return;
                }

                if (tbPrecioUnitario.Text.Length == 0)
                {
                    MessageBox.Show("No ha ingresado el precio.");
                    return;
                }

                double p;

                if (!double.TryParse(tbPrecioUnitario.Text, out p))
                {
                    MessageBox.Show("No ha ingresado una precio correcto.");
                    return;
                }

                if (_ServicioSeleccionado == null)
                {
                    MessageBox.Show("No ha seleccionado un servicio.");
                    return;
                }

                CalcularTotal();
                _Concepto.IdServicio = _ServicioSeleccionado.id_productoservicio;
                _Concepto.Cantidad = int.Parse(tbCantidad.Text);
                _Concepto.Descripcion = tbDescripcion.Text;
                _Concepto.PrecioUnitario = float.Parse(tbPrecioUnitario.Text);
                _Concepto.Importe = Total;
                _Concepto.Unidad = cbUnidad.Text;
                _Concepto.Clave = _ServicioSeleccionado.clave;
                _Concepto.claveUnidad = cbUnidad.SelectedValue.ToString();
                _Concepto.Servicio = _ServicioSeleccionado;

                if (tbProductos.Text != "")
                {
                    Insumos Ins = (Insumos)tbProductos.SelectedItem;
                    _Concepto.IdInsumo = Ins.id_insumo;
                    _Concepto.DescripcionInsumo = Ins.descripcion;
                }
                else
                {
                    _Concepto.IdInsumo = 0;
                    _Concepto.DescripcionInsumo = "";
                }

                _Parent.ActualizarConceptos();

                this.Close();
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbClave_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbServicios.SelectedItem != null)
            {
                ProductoServicioAutocomplete pro = (ProductoServicioAutocomplete)tbServicios.SelectedItem;
                _ServicioSeleccionado = new ProductosServicios();
                _ServicioSeleccionado.descripcion = pro.Descripcion;
                _ServicioSeleccionado.clave = pro.Clave;
                _ServicioSeleccionado.id_productoservicio = pro.IdServicio;
                lblClaveServicio.Content = _ServicioSeleccionado.clave;
            }
        }

        private void tbCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void tbPrecioUnitario_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void CalcularTotal()
        {
            if (tbCantidad is null || tbPrecioUnitario is null || tbImporte is null)
            {
                return;
            }
            try
            {
                //Total = int.Parse(tbCantidad.Text) * float.Parse(tbPrecioUnitario.Text);
                //tbImporte.Text = (int.Parse(tbCantidad.Text) * float.Parse(tbPrecioUnitario.Text)).ToString();
                Total = float.Parse(tbImporte.Text);
                double unitario = Total / int.Parse(tbCantidad.Text);
                tbPrecioUnitario.Text = Math.Round(unitario, 6).ToString();
            }
            catch (Exception exc)
            {

            }
        }
        private void CalcularTotalUnitario()
        {
            if (tbCantidad is null || tbPrecioUnitario is null || tbImporte is null)
            {
                return;
            }
            try
            {
                Total = int.Parse(tbCantidad.Text) * float.Parse(tbPrecioUnitario.Text);
                tbImporte.Text = (int.Parse(tbCantidad.Text) * float.Parse(tbPrecioUnitario.Text)).ToString();
                Total = float.Parse(tbImporte.Text);
                //double unitario = Total / int.Parse(tbCantidad.Text);
                //tbPrecioUnitario.Text = Math.Round(unitario, 6).ToString();
            }
            catch (Exception exc)
            {

            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                    elementWithFocus.MoveFocus(request);
                    break;
                default:
                    break;
            }
        }

        private void tbImporte_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
        private void tbPrecioUnitario_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbPrecioUnitario.Text.Length == 0)
                {
                    tbPrecioUnitario.Text = "0";
                    return;
                }

                double p;

                if (!double.TryParse(tbPrecioUnitario.Text, out p))
                {
                    tbPrecioUnitario.Text = "0";
                    return;
                }

                CalcularTotalUnitario();
            }
        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbCantidad.Text.Length == 0)
                {
                    tbCantidad.Text = "1";
                    return;
                }
                else
                {
                    int c;

                    if (!int.TryParse(tbCantidad.Text, out c))
                    {
                        tbCantidad.Text = "1";
                        return;
                    }
                }

                CalcularTotal();
            }
        }

        private void tbImporte_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbImporte.Text.Length == 0)
                {
                    tbImporte.Text = "0";
                    return;
                }

                double p;

                if (!double.TryParse(tbImporte.Text, out p))
                {
                    tbImporte.Text = "0";
                    return;
                }

                double total = double.Parse(tbImporte.Text);
                int cantidad = int.Parse(tbCantidad.Text);
                double unitario = total / cantidad;
                unitario = Math.Round(unitario, 6);
                if (unitario > 0)
                {
                    tbPrecioUnitario.Text = unitario.ToString();
                    CalcularTotal();
                }
            }
        }

        private void tbProductos_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
