using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            if (MessageBox.Show("¿Desea guardar la orden?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            //ImprimirPDF("39865");
            //return;


            if (tbNombreTrabajo.Text.Length == 0)
            {
                MessageBox.Show("No ha ingresado el nombre del trabajo.");
                tbNombreTrabajo.Focus();
                return;
            }

            if (tbCantidad.Text.Length == 0)
            {
                MessageBox.Show("No ha ingresado una cantidad.");
                tbCantidad.Focus();
                return;
            }

            if (tbCotizacion.Text.Length == 0)
            {
                /*
                MessageBox.Show("No ha ingresado la cotización.");
                tbCotizacion.Focus();
                return;
                */
                tbCotizacion.Text = "0";
            }

            if (tbAnticipo.Text.Length == 0)
            {
                /*
                MessageBox.Show("No ha ingresado el anticipo.");
                tbAnticipo.Focus();
                return;
                */
                tbAnticipo.Text = "0";
            }

            if (tbCostoAnterior.Text.Length == 0)
            {
                MessageBox.Show("No ha ingresado el costo anterior.");
                tbCostoAnterior.Focus();
                return;
            }

            if (cbConFolio.Text == "SI")
            {
                if (tbDelNumero.Text.Length == 0)
                {
                    MessageBox.Show("No ha escrito el folio inicial.");
                    tbDelNumero.Focus();
                    return;
                }
                if (tbAlNumero.Text.Length == 0)
                {
                    MessageBox.Show("No ha escrito el folio final.");
                    tbAlNumero.Focus();
                    return;
                }
            }

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                using (DbContextTransaction transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //float numero = float.Parse(dbContext.NumeroNota.First().numero);
                        //float numero = float.Parse(dbContext.Valores.First().numero_orden);

                        Ordenes ordenModificar = dbContext.Ordenes.Where(O => O.id_orden == OrdenElegida.id_orden).First();

                        ordenModificar.id_cliente = OrdenElegida.id_cliente;
                        //ordenModificar.numero = ordenModificar.numero;
                        ordenModificar.telefono = tbTelefono.Text;
                        ordenModificar.solicitante = tbSolicita.Text;
                        //ordenModificar.fecha_solicita = dtpFecha.SelectedDate.Value.ToShortDateString();
                        //ordenModificar.quien_recibio = tbRecibe.Text;
                        //ordenModificar.inicio_diseno = "";
                        ordenModificar.nombre_trabajo = tbNombreTrabajo.Text;
                        ordenModificar.cantidad = int.Parse(tbCantidad.Text);
                        ordenModificar.color_tintas = tbColorTintas.Text;
                        ordenModificar.tipo_papel = tbTipoPapel.Text;
                        ordenModificar.con_folio = cbConFolio.Text;
                        ordenModificar.del_numero = tbDelNumero.Text;
                        ordenModificar.al_numero = tbAlNumero.Text;
                        ordenModificar.tamano = cbTamaño.Text;
                        ordenModificar.otros_1 = tbOtros1.Text;
                        ordenModificar.otros_2 = tbOtros2.Text;
                        ordenModificar.copia_1 = cbPrimeraCopia.Text;
                        ordenModificar.copia_2 = cbSegundaCopia.Text;
                        ordenModificar.copia_3 = cbTerceraCopia.Text;
                        ordenModificar.copia_4 = cbCuartaCopia.Text;
                        ordenModificar.otros_3 = tbOtros3.Text;
                        ordenModificar.otros_4 = tbNotas.Text;
                        ordenModificar.pegado = chbPegado.IsChecked == true ? "SI" : "NO";
                        ordenModificar.engrapado = chbEngrapado.IsChecked == true ? "SI" : "NO";
                        ordenModificar.perforacion = chbPerforacion.IsChecked == true ? "SI" : "NO";
                        ordenModificar.rojo = chbRojo.IsChecked == true ? "SI" : "NO";
                        ordenModificar.blanco = chbBlanco.IsChecked == true ? "SI" : "NO";
                        ordenModificar.especificaciones = tbDescripcion.Text;
                        //ordenModificar.estado = "RECEPCION";
                        //ordenModificar.tipo = "COTIZACION";
                        ordenModificar.total = float.Parse(tbCotizacion.Text);
                        ordenModificar.anticipo = float.Parse(tbAnticipo.Text);
                        ordenModificar.costo_anterior = float.Parse(tbCostoAnterior.Text);
                        //ordenModificar.muestra = "";
                        //ordenModificar.fecha_muestra = "";
                        ordenModificar.prioridad = cbPrioridad.Text;
                        //ordenModificar.pagado = "NO";
                        //ordenModificar.ruta = RutaOrden;
                        //ordenModificar.orden_anterior = tbOrdenAnterior.Text;
                        ordenModificar.fecha_negativo = dtpFechaNegativo.SelectedDate.HasValue ? dtpFechaNegativo.SelectedDate.Value.ToShortDateString() : "";
                        ordenModificar.tipo_maquina = cbTipoMaquina.Text;
                        //ordenModificar.autorizado = "NO";

                        
                        /*
                        Valores numOrden = dbContext.Valores.First();
                        numOrden.numero_orden = (numero + 1).ToString();
                        numOrden.ultimo_cambio = DateTime.Now.ToString();
                        */
                        dbContext.SaveChanges();

                        transaction.Commit();

                        //ImprimirPDF(numero.ToString());

                        MessageBox.Show("Orden modificada.");

                        this.Close();
                    }
                    catch (Exception exc)
                    {
                        transaction.Rollback();
                        MessageBox.Show(exc.Message);
                    }
                }
            }
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
