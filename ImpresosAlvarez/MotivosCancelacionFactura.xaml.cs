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
    /// Lógica de interacción para MotivosCancelacionFactura.xaml
    /// </summary>
    public partial class MotivosCancelacionFactura : Window
    {
        String UUIDCancelado;
        VerFactura ParentForm;
        VerComplemento ParentFormComplemento;
        Entity.FacturaDigital FacturaElegida;
        public MotivosCancelacionFactura(VerFactura ParentForm)
        {
            InitializeComponent();
            this.ParentForm = ParentForm;
        }

        public MotivosCancelacionFactura(VerComplemento ParentForm)
        {
            InitializeComponent();
            this.ParentFormComplemento = ParentForm;
        }

        private void cbMotivos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbMotivos.SelectedItem != null && lblInfo != null)
                {
                    switch (cbMotivos.SelectedIndex)
                    {
                        case 0:
                            tbInfo.Text = "01-Este supuesto aplica cuando la factura generada contiene un error en la clave del producto, valor unitario, descuento o cualquier otro dato, por lo que se debe reexpedir. En este caso, primero se sustituye la factura y cuando se solicita la cancelación, se incorpora el folio de la factura que sustituye a la cancelada.";
                            btnCancelacionFactura.Visibility = Visibility.Visible;
                            UUIDCancelado = "";
                            FacturaElegida = null;
                            lblUUIDCancelada.Content = "";
                            break;
                        case 1:
                            tbInfo.Text = "02-Este supuesto aplica cuando la factura generada contiene un error en la clave del producto, valor unitario, descuento o cualquier otro dato y no se requiera relacionar con otra factura generada.";
                            btnCancelacionFactura.Visibility = Visibility.Hidden;
                            UUIDCancelado = "";
                            FacturaElegida = null;
                            lblUUIDCancelada.Content = "";
                            break;
                        case 2:
                            tbInfo.Text = "03-Este supuesto aplica cuando se facturó una operación que no se concreta.";
                            btnCancelacionFactura.Visibility = Visibility.Hidden;
                            UUIDCancelado = "";
                            FacturaElegida = null;
                            lblUUIDCancelada.Content = "";
                            break;
                        case 3:
                            tbInfo.Text = "04-Este supuesto aplica cuando se incluye una venta en la factura global de operaciones con el público en general y posterior a ello, el cliente solicita su factura nominativa, lo que conlleva a cancelar la factura global y reexpedirla, así como generar la factura nominativa al cliente.";
                            btnCancelacionFactura.Visibility = Visibility.Visible;
                            UUIDCancelado = "";
                            FacturaElegida = null;
                            lblUUIDCancelada.Content = "";
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception exc)
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void bntCancelarFactura_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbMotivos.SelectedItem != null)
                {
                    switch (cbMotivos.SelectedIndex)
                    {
                        case 0:
                            if (FacturaElegida == null)
                            {
                                MessageBox.Show("No ha elegido la factura que va a sustituir a la cancelada.");
                                return;
                            }
                            if (ParentForm != null)
                            {
                                ParentForm.Cancelacion("01");
                            }
                            else if (ParentFormComplemento != null)
                            {
                                ParentFormComplemento.Cancelacion("01");
                            }
                            break;
                        case 1:
                            if (ParentForm != null)
                            {
                                ParentForm.Cancelacion("02");
                            }
                            else if (ParentFormComplemento != null)
                            {
                                ParentFormComplemento.Cancelacion("02");
                            }
                            break;
                        case 2:
                            if (ParentForm != null)
                            {
                                ParentForm.Cancelacion("03");
                            }
                            else if (ParentFormComplemento != null)
                            {
                                ParentFormComplemento.Cancelacion("03");
                            }
                            break;
                        case 3:
                            if (FacturaElegida == null)
                            {
                                MessageBox.Show("No ha elegido la factura que va a sustituir a la cancelada.");
                                return;
                            }
                            if (ParentForm != null)
                            {
                                ParentForm.Cancelacion("04");
                            }
                            else if (ParentFormComplemento != null)
                            {
                                ParentFormComplemento.Cancelacion("04");
                            }
                            break;
                        default:
                            break;
                    }
                    this.Close();
                }
            }
            catch (Exception exc)
            {

            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCancelacionFactura_Click(object sender, RoutedEventArgs e)
        {
            BuscarFacturaRelacionadaCancelacion cancel = new BuscarFacturaRelacionadaCancelacion(this);
            cancel.ShowDialog();
        }

        public void ElegirFactura(Entity.FacturaDigital FacturaElegida)
        {
            string[] parts = FacturaElegida.cadena_original.Split('|');
            lblUUIDCancelada.Content = parts[4];
            UUIDCancelado = parts[4];
            this.FacturaElegida = FacturaElegida;

            if (ParentForm != null)
            {
                ParentForm.ElegirFacturaRelacionada(UUIDCancelado, FacturaElegida);
            }
            else if (ParentFormComplemento != null)
            {
                ParentFormComplemento.ElegirFacturaRelacionada(UUIDCancelado, FacturaElegida);
            }
        }
    }
}
