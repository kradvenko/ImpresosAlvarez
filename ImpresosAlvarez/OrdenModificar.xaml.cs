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
    /// Lógica de interacción para OrdenModificar.xaml
    /// </summary>
    public partial class OrdenModificar : Window
    {
        ControlOrdenes ParentForm;
        Ordenes OrdenElegida;
        Clientes ClienteOrden;
        public OrdenModificar(ControlOrdenes Parent, Ordenes Orden)
        {
            InitializeComponent();
            this.OrdenElegida = Orden;
            this.ParentForm = Parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {                
                ClienteOrden = dbContext.Clientes.Where(F => F.id_cliente.ToString() == OrdenElegida.id_cliente.ToString()).First();
            }
            lblFolioOrden.Content = OrdenElegida.numero;
            lblCliente.Content = ClienteOrden.nombre;
            tbTelefono.Text = OrdenElegida.telefono;
            tbSolicita.Text = OrdenElegida.solicitante;
            dtpFecha.SelectedDate = DateTime.Parse(OrdenElegida.fecha_solicita);
            tbRecibe.Text = OrdenElegida.quien_recibio;
            tbOrdenAnterior.Text = OrdenElegida.orden_anterior;
            if (OrdenElegida.fecha_negativo.Length > 0)
            {
                dtpFechaNegativo.SelectedDate = DateTime.Parse(OrdenElegida.fecha_negativo);
            }
            tbNombreTrabajo.Text = OrdenElegida.nombre_trabajo;
            tbCantidad.Text = OrdenElegida.cantidad.ToString();
            tbColorTintas.Text = OrdenElegida.color_tintas;
            tbTipoPapel.Text = OrdenElegida.tipo_papel;
            cbConFolio.Text = OrdenElegida.con_folio;
            if (OrdenElegida.con_folio == "SI")
            {
                tbDelNumero.IsEnabled = true;
                tbAlNumero.IsEnabled = true;
            }
            else
            {
                tbDelNumero.IsEnabled = false;
                tbAlNumero.IsEnabled = false;
            }
            tbDelNumero.Text = OrdenElegida.del_numero;
            tbAlNumero.Text = OrdenElegida.al_numero;
            cbTamaño.Text = OrdenElegida.tamano;
            cbPrimeraCopia.Text = OrdenElegida.copia_1;
            cbSegundaCopia.Text = OrdenElegida.copia_2;
            cbTerceraCopia.Text = OrdenElegida.copia_3;
            cbCuartaCopia.Text = OrdenElegida.copia_4;
            tbOtros1.Text = OrdenElegida.otros_1;
            tbOtros2.Text = OrdenElegida.otros_2;
            if (OrdenElegida.pegado == "SI")
            {
                chbPegado.IsChecked = true;
            }
            else
            {
                chbPegado.IsChecked = false;
            }
            if (OrdenElegida.engrapado == "SI")
            {
                chbEngrapado.IsChecked = true;
            }
            else
            {
                chbEngrapado.IsChecked = false;
            }
            if (OrdenElegida.perforacion == "SI")
            {
                chbPerforacion.IsChecked = true;
            }
            else
            {
                chbPerforacion.IsChecked = false;
            }
            if (OrdenElegida.rojo == "SI")
            {
                chbRojo.IsChecked = true;
            }
            else
            {
                chbRojo.IsChecked = false;
            }
            if (OrdenElegida.blanco == "SI")
            {
                chbBlanco.IsChecked = true;
            }
            else
            {
                chbRojo.IsChecked = false;
            }
            tbOtros3.Text = OrdenElegida.otros_3;
            cbTipoMaquina.Text = OrdenElegida.tipo_maquina;
            tbDescripcion.Text = OrdenElegida.especificaciones;
            tbCotizacion.Text = OrdenElegida.total.ToString();
            tbAnticipo.Text = OrdenElegida.anticipo.ToString();
            tbCostoAnterior.Text = OrdenElegida.costo_anterior.ToString();
            tbNotas.Text = OrdenElegida.otros_4;
        }

        private void btnOrdenesAnteriores_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void cbConFolio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tbDelNumero_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
