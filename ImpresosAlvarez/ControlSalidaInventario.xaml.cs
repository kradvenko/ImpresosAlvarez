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
    /// Lógica de interacción para ControlSalidaInventario.xaml
    /// </summary>
    public partial class ControlSalidaInventario : Window
    {
        Salidas Parent;
        String Modo;
        List<Insumos> _insumos;
        SalidasInventario SalidaInventario;
        Insumos InsumoElegido;
        public ControlSalidaInventario(Salidas Parent, String Modo, SalidasInventario SalidaInventario)
        {
            InitializeComponent();
            this.Parent = Parent;
            this.Modo = Modo;
            this.SalidaInventario = SalidaInventario;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFecha.SelectedDate = DateTime.Now;
            CargarInsumos();
            if (Modo == "MODIFICAR")
            {
                dpFecha.SelectedDate = SalidaInventario.fecha;
                tbPresupuesto.Text = SalidaInventario.presupuesto.ToString();
                tbFactura.Text = SalidaInventario.factura;
                tbOrdenTrabajo.Text = SalidaInventario.orden_trabajo;
                tbCantidad.Text = SalidaInventario.cantidad.ToString();
                tbInsumos.Text = SalidaInventario.descripcion;
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

        private void btnAgregarSalida_Click(object sender, RoutedEventArgs e)
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
                    SalidasInventario nueva = new SalidasInventario();
                    nueva.fecha = dpFecha.SelectedDate.Value;
                    nueva.presupuesto = float.Parse(tbPresupuesto.Text);
                    nueva.orden_trabajo = tbOrdenTrabajo.Text;
                    nueva.factura = tbFactura.Text;
                    nueva.cantidad = float.Parse(tbCantidad.Text);
                    nueva.id_insumo = InsumoElegido.id_insumo;
                    nueva.descripcion = InsumoElegido.descripcion;

                    dbContext.SalidasInventario.Add(nueva);

                    Insumos insumo = dbContext.Insumos.Where(I => I.id_insumo == InsumoElegido.id_insumo).First();
                    insumo.stock = insumo.stock - nueva.cantidad;

                    dbContext.SaveChanges();

                    Parent.CargarSalidas();

                    this.Close();
                }
            }
            else if (Modo == "MODIFICAR")
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
                    SalidasInventario modificar = dbContext.SalidasInventario.Where(S => S.id_salida == SalidaInventario.id_salida).FirstOrDefault();

                    modificar.fecha = dpFecha.SelectedDate.Value;
                    modificar.presupuesto = float.Parse(tbPresupuesto.Text);
                    modificar.orden_trabajo = tbOrdenTrabajo.Text;
                    modificar.factura = tbFactura.Text;
                    modificar.cantidad = float.Parse(tbCantidad.Text);
                    modificar.id_insumo = InsumoElegido.id_insumo;
                    modificar.descripcion = InsumoElegido.descripcion;

                    dbContext.SaveChanges();

                    Insumos insumoInicial = dbContext.Insumos.Where(I => I.id_insumo == SalidaInventario.id_insumo).First();
                    insumoInicial.stock = insumoInicial.stock + SalidaInventario.cantidad;

                    dbContext.SaveChanges();

                    Insumos insumo = dbContext.Insumos.Where(I => I.id_insumo == InsumoElegido.id_insumo).First();
                    insumo.stock = insumo.stock - modificar.cantidad;

                    dbContext.SaveChanges();

                    Parent.CargarSalidas();

                    this.Close();
                }
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
