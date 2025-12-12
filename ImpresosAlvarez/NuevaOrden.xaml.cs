using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using ImpresosAlvarez.Entity;
using System.Data.Entity;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using System.Diagnostics;
using iText.IO.Image;
using iText.Kernel.Colors;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para NuevaOrden.xaml
    /// </summary>
    public partial class NuevaOrden : Window
    {
        List<Clientes> _clientes;
        List<Usuarios> _usuarios;
        List<Ordenes> _anteriores;

        Clientes ClienteElegido;

        public String RutaOrden;
        public NuevaOrden()
        {
            InitializeComponent();
        }

        private void btnOrdenesAnteriores_Click(object sender, RoutedEventArgs e)
        {
            if (ClienteElegido != null)
            {
                OrdenesAnteriores ant = new OrdenesAnteriores(ClienteElegido, this);
                ant.ShowDialog();
            }
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                ClienteElegido = (Clientes)tbClientes.SelectedItem;
                tbTelefono.Text = ClienteElegido.telefono1;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            String TipoOrden = cbTipoOrden.Text;
            if (MessageBox.Show("¿Desea guardar la orden de TIPO " + TipoOrden + "?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }
            
            //ImprimirPDF("46711", "LONA/VINIL");
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
                        float numero = float.Parse(dbContext.Valores.First().numero_orden);

                        Ordenes nuevaOrden = new Ordenes();

                        nuevaOrden.id_cliente = ClienteElegido.id_cliente;
                        nuevaOrden.numero = numero;
                        nuevaOrden.telefono = tbTelefono.Text;
                        nuevaOrden.solicitante = tbSolicita.Text;
                        nuevaOrden.fecha_solicita = dtpFecha.SelectedDate.Value.ToShortDateString();
                        nuevaOrden.quien_recibio = tbRecibe.Text;
                        nuevaOrden.inicio_diseno = "";

                        /*ORDEN NORMAL*/
                        nuevaOrden.nombre_trabajo = tbNombreTrabajo.Text;
                        nuevaOrden.cantidad = int.Parse(tbCantidad.Text);
                        nuevaOrden.color_tintas = tbColorTintas.Text;
                        nuevaOrden.tipo_papel = tbTipoPapel.Text;
                        nuevaOrden.con_folio = cbConFolio.Text;
                        nuevaOrden.del_numero = tbDelNumero.Text;
                        nuevaOrden.al_numero = tbAlNumero.Text;
                        nuevaOrden.tamano = cbTamaño.Text;
                        nuevaOrden.otros_1 = tbOtros1.Text;
                        nuevaOrden.otros_2 = tbOtros2.Text;
                        nuevaOrden.copia_1 = cbPrimeraCopia.Text;
                        nuevaOrden.copia_2 = cbSegundaCopia.Text;
                        nuevaOrden.copia_3 = cbTerceraCopia.Text;
                        nuevaOrden.copia_4 = cbCuartaCopia.Text;
                        nuevaOrden.otros_3 = tbOtros3.Text;
                        nuevaOrden.otros_4 = tbNotas.Text;
                        nuevaOrden.pegado = chbPegado.IsChecked == true ? "SI" : "NO";
                        nuevaOrden.engrapado = chbEngrapado.IsChecked == true ? "SI" : "NO";
                        nuevaOrden.perforacion = chbPerforacion.IsChecked == true ? "SI" : "NO";
                        nuevaOrden.rojo = chbRojo.IsChecked == true ? "SI" : "NO";
                        nuevaOrden.blanco = chbBlanco.IsChecked == true ? "SI" : "NO";
                        nuevaOrden.especificaciones = tbDescripcion.Text;

                        /*ORDEN LONA/VINIL*/
                        nuevaOrden.lona_medida = tbLonaMedida.Text;
                        if (chbLonaNormal.IsChecked == true)
                        {
                            nuevaOrden.lona_normal = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_normal = "NO";
                        }
                        if (chbLonaTraslucida.IsChecked == true)
                        {
                            nuevaOrden.lona_traslucida = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_traslucida = "NO";
                        }
                        if (chbLonaUv.IsChecked == true)
                        {
                            nuevaOrden.lona_impresion_uv = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_impresion_uv = "NO";
                        }
                        if (chbLonaEcosolvente.IsChecked == true)
                        {
                            nuevaOrden.lona_impresion_ecosolvente = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_impresion_ecosolvente = "NO";
                        }
                        if (chbLonaAcabadoBYO.IsChecked == true)
                        {
                            nuevaOrden.lona_acabado_byo = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_acabado_byo = "NO";
                        }
                        if (chbLonaAcabadoBastilla.IsChecked == true)
                        {
                            nuevaOrden.lona_acabado_bastilla = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_acabado_bastilla = "NO";
                        }
                        if (chbLonaAcabadoSobrante.IsChecked == true)
                        {
                            nuevaOrden.lona_acabado_sobrante = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_acabado_sobrante = "NO";
                        }
                        if (chbLonaAcabadoBolsa.IsChecked == true)
                        {
                            nuevaOrden.lona_acabado_bolsa = "SI";
                        }
                        else
                        {
                            nuevaOrden.lona_acabado_bolsa = "NO";
                        }
                        nuevaOrden.lona_acabado_otro = tbLonaAcabadoOtro.Text;
                        nuevaOrden.lona_acabado_observaciones = tbLonaObservaciones.Text;
                        nuevaOrden.vinil_medida = tbVinilMedida.Text;
                        if (chbVinilBrillante.IsChecked == true)
                        {
                            nuevaOrden.vinil_tipo_brillante = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_tipo_brillante = "NO";
                        }
                        if (chbVinilMate.IsChecked == true)
                        {
                            nuevaOrden.vinil_tipo_mate = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_tipo_mate = "NO";
                        }
                        if (chbVinilUv.IsChecked == true)
                        {
                            nuevaOrden.vinil_impresion_uv = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_impresion_uv = "NO";
                        }
                        if (chbVinilEcosolvente.IsChecked == true)
                        {
                            nuevaOrden.vinil_impresion_ecosolvente = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_impresion_ecosolvente = "NO";
                        }
                        if (chbVinilEcono.IsChecked == true)
                        {
                            nuevaOrden.vinil_econo = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_econo = "NO";
                        }
                        if (chbVinilEcoGris.IsChecked == true)
                        {
                            nuevaOrden.vinil_ecogris = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_ecogris = "NO";
                        }
                        if (chbVinilAlta.IsChecked == true)
                        {
                            nuevaOrden.vinil_alta = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_alta = "NO";
                        }
                        if (chbVinilMicro.IsChecked == true)
                        {
                            nuevaOrden.vinil_micro = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_micro = "NO";
                        }
                        if (chbVinilTrans.IsChecked == true)
                        {
                            nuevaOrden.vinil_trans = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_trans = "NO";
                        }
                        if (chbVinilEstBlanco.IsChecked == true)
                        {
                            nuevaOrden.vinil_est_blanco = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_est_blanco = "NO";
                        }
                        if (chbVinilEstTrans.IsChecked == true)
                        {
                            nuevaOrden.vinil_est_trans = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_est_trans= "NO";
                        }
                        if (chbVinilReflejante.IsChecked == true)
                        {
                            nuevaOrden.vinil_reflejante = "SI";
                        }
                        else
                        {
                            nuevaOrden.vinil_reflejante = "NO";
                        }
                        nuevaOrden.vinil_observaciones = tbVinilObservaciones.Text;
                        nuevaOrden.otro_material = tbOtroMaterial.Text;
                        nuevaOrden.envio_a = cbEnvioA.Text;

                        /*PARA TODOS LOS TIPOS DE ORDENES*/
                        nuevaOrden.tipo_orden = TipoOrden;
                        nuevaOrden.estado = "RECEPCION";
                        nuevaOrden.tipo = "COTIZACION";
                        nuevaOrden.total = float.Parse(tbCotizacion.Text);
                        nuevaOrden.anticipo = float.Parse(tbAnticipo.Text);
                        nuevaOrden.costo_anterior = float.Parse(tbCostoAnterior.Text);
                        nuevaOrden.muestra = "";
                        nuevaOrden.fecha_muestra = "";
                        nuevaOrden.prioridad = cbPrioridad.Text;
                        nuevaOrden.pagado = "NO";
                        nuevaOrden.ruta = RutaOrden;
                        nuevaOrden.orden_anterior = tbOrdenAnterior.Text;
                        nuevaOrden.fecha_negativo = dtpFechaNegativo.SelectedDate.HasValue ? dtpFechaNegativo.SelectedDate.Value.ToShortDateString() : "";
                        nuevaOrden.tipo_maquina = cbTipoMaquina.Text;
                        nuevaOrden.autorizado = "NO";

                        dbContext.Ordenes.Add(nuevaOrden);

                        Valores numOrden = dbContext.Valores.First();
                        numOrden.numero_orden = (numero + 1).ToString();
                        numOrden.ultimo_cambio = DateTime.Now.ToString();

                        dbContext.SaveChanges();

                        transaction.Commit();

                        ImprimirPDF(numero.ToString(), cbTipoOrden.Text);

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dtpFecha.SelectedDate = DateTime.Now;

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = _clientes;

                /*
                _usuarios = dbContext.Usuarios.Where(T => T.tipo == "Recepcion" && T.estado == "ACTIVO").ToList();
                cbRecibe.SelectedValuePath = "id_usuario";
                cbRecibe.DisplayMemberPath = "nombre";
                cbRecibe.ItemsSource = _usuarios;
                */

                lblFolioOrden.Content = dbContext.Valores.First().numero_orden;
            }
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public void ImprimirPDF(String NumeroOrden, String Tipo)
        {
            if (Tipo == "NORMAL")
            {
                Ordenes OrdenElegida;
                Clientes ClienteOrden;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    OrdenElegida = dbContext.Ordenes.Where(O => O.numero.ToString() == NumeroOrden).First();

                    if (OrdenElegida == null)
                    {
                        return;
                    }
                    else
                    {
                        ClienteOrden = dbContext.Clientes.Where(C => C.id_cliente == OrdenElegida.id_cliente).First();
                    }
                }

                String rutaPDF = "";

                Document document = null;

                System.IO.Directory.CreateDirectory(@"C:\Impresos\Ordenes");
                rutaPDF = @"C:\Impresos\Ordenes\Orden_" + NumeroOrden + ".pdf";

                //PdfDocument pdf = new PdfDocument(new PdfReader(@"AlvarezCotizacionL.pdf"), new PdfWriter(rutaPDF));
                PdfDocument pdf = new PdfDocument(new PdfWriter(rutaPDF));
                document = new Document(pdf, PageSize.LETTER);

                document.SetMargins(10, 10, 10, 10);

                float[] columnWidths = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
                PdfFont f = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                float fs = 9;


                //PRIMER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden de trabajo")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Recibe")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.quien_recibio)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.prioridad)));

                //SEGUNDO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Nombre de la empresa")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ClienteOrden.pseudonimo)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Teléfono")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.telefono)));

                //TERCER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Solicitante")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.solicitante)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha solicita")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_solicita)));

                //CUARTO RENGLON                

                table.AddCell(new Cell(1, 5)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //QUINTO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Descripción")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.nombre_trabajo)));


                //SEXTO RENGLON
                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Cantidad")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Color de tintas")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.color_tintas)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Papel")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tipo_papel)));


                //SEPTIMO RENGLON                


                //OCTAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Con folio")));

                String ConFolio = "Si No [X]";
                if (OrdenElegida.con_folio == "SI")
                {
                    ConFolio = "Si [X] No";
                }

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ConFolio)));

                String Folios = "";
                if (OrdenElegida.con_folio == "SI")
                {
                    Folios = "Del " + OrdenElegida.del_numero + " Al " + OrdenElegida.al_numero;
                }

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Folios)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden anterior")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.orden_anterior)));
                /*
                table.AddCell(new Cell(2, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));
                */

                //NOVENO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tamaño del papel")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tamano)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha anterior ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_negativo)));

                /*
                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()                
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                */

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DECIMO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Copias")));

                String Copias = "";

                if (OrdenElegida.copia_1.Length > 0)
                {
                    Copias = " Copia 1 " + OrdenElegida.copia_1;
                }

                if (OrdenElegida.copia_2.Length > 0)
                {
                    Copias = Copias + " Copia 2 " + OrdenElegida.copia_2;
                }

                if (OrdenElegida.copia_3.Length > 0)
                {
                    Copias = Copias + " Copia 3 " + OrdenElegida.copia_3;
                }

                if (OrdenElegida.copia_4.Length > 0)
                {
                    Copias = Copias + " Copia 4 " + OrdenElegida.copia_4;
                }

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Copias)));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DUODECIMO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Acabado")));

                String Acabado = "";

                if (OrdenElegida.pegado == "SI")
                {
                    Acabado = " PEGADO ";
                }

                if (OrdenElegida.engrapado == "SI")
                {
                    Acabado = Acabado + " ENGRAPADO ";
                }

                if (OrdenElegida.perforacion == "SI")
                {
                    Acabado = Acabado + " PERFORACION ";
                }

                if (OrdenElegida.rojo == "SI")
                {
                    Acabado = Acabado + " ROJO ";
                }

                if (OrdenElegida.blanco == "SI")
                {
                    Acabado = Acabado + " BLANCO ";
                }

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Acabado)));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //CATORCEAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Especificaciones")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("NOMBRE DEL DISEÑADOR: _________________________________")));

                //QUINCEAVO RENGLON

                if (OrdenElegida.ruta != null)
                {

                    table.AddCell(new Cell(1, 2)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("Ruta")));

                    table.AddCell(new Cell(1, 8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(OrdenElegida.ruta)));
                }


                //DIECISEISAVO RENGLON

                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                   .SetFont(f)
                   .SetFontSize(fs)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .Add(new Paragraph(OrdenElegida.especificaciones)));

                // 19 RENGLON

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Negativo nuevo [  ]")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Ya existe negativo [  ]")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tirar placas y negativos [  ]")));

                // 19

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Tipo de máquina " + OrdenElegida.tipo_maquina)));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 6)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));

                //////////////////////////////////////////
                ///
                table.AddCell(new Cell(8, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(20)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));

                /////////////////////////////////// SEGUNDA IMPRESION
                //PRIMER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden de trabajo")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Recibe")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.quien_recibio)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.prioridad)));

                //SEGUNDO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Nombre de la empresa")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ClienteOrden.pseudonimo)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Teléfono")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.telefono)));

                //TERCER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Solicitante")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.solicitante)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha solicita")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_solicita)));

                //CUARTO RENGLON                

                table.AddCell(new Cell(1, 5)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //QUINTO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Descripción")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.nombre_trabajo)));


                //SEXTO RENGLON
                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Cantidad")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Color de tintas")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.color_tintas)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Papel")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tipo_papel)));


                //SEPTIMO RENGLON                


                //OCTAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Con folio")));

                ConFolio = "Si No [X]";
                if (OrdenElegida.con_folio == "SI")
                {
                    ConFolio = "Si [X] No";
                }

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ConFolio)));

                Folios = "";
                if (OrdenElegida.con_folio == "SI")
                {
                    Folios = "Del " + OrdenElegida.del_numero + " Al " + OrdenElegida.al_numero;
                }

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Folios)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden anterior")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.orden_anterior)));
                /*
                table.AddCell(new Cell(2, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));
                */

                //NOVENO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tamaño del papel")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tamano)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha anterior ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_negativo)));

                /*
                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()                
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                */

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DECIMO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Copias")));

                Copias = "";

                if (OrdenElegida.copia_1.Length > 0)
                {
                    Copias = " Copia 1 " + OrdenElegida.copia_1;
                }

                if (OrdenElegida.copia_2.Length > 0)
                {
                    Copias = Copias + " Copia 2 " + OrdenElegida.copia_2;
                }

                if (OrdenElegida.copia_3.Length > 0)
                {
                    Copias = Copias + " Copia 3 " + OrdenElegida.copia_3;
                }

                if (OrdenElegida.copia_4.Length > 0)
                {
                    Copias = Copias + " Copia 4 " + OrdenElegida.copia_4;
                }

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Copias)));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DUODECIMO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Acabado")));

                Acabado = "";

                if (OrdenElegida.pegado == "SI")
                {
                    Acabado = " PEGADO ";
                }

                if (OrdenElegida.engrapado == "SI")
                {
                    Acabado = Acabado + " ENGRAPADO ";
                }

                if (OrdenElegida.perforacion == "SI")
                {
                    Acabado = Acabado + " PERFORACION ";
                }

                if (OrdenElegida.rojo == "SI")
                {
                    Acabado = Acabado + " ROJO ";
                }

                if (OrdenElegida.blanco == "SI")
                {
                    Acabado = Acabado + " BLANCO ";
                }

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Acabado)));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //CATORCEAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Especificaciones")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("NOMBRE DEL DISEÑADOR: _________________________________")));

                //QUINCEAVO RENGLON

                if (OrdenElegida.ruta != null)
                {

                    table.AddCell(new Cell(1, 2)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("Ruta")));

                    table.AddCell(new Cell(1, 8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(OrdenElegida.ruta)));
                }


                //DIECISEISAVO RENGLON

                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                   .SetFont(f)
                   .SetFontSize(fs)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .Add(new Paragraph(OrdenElegida.especificaciones)));

                // 19 RENGLON

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Negativo nuevo [  ]")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Ya existe negativo [  ]")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tirar placas y negativos [  ]")));

                // 19

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Tipo de máquina " + OrdenElegida.tipo_maquina)));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 6)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));


                //////////////////////////////////////////


                /*

                ImageData imageData = ImageDataFactory.Create(@"Imagenes/LogoAlvarez.png");

                iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imageData);

                pdfImg.SetHeight(100);
                pdfImg.SetFixedPosition(480, 480);

                document.Add(pdfImg);

                pdfImg.SetFixedPosition(480, 150);

                document.Add(pdfImg);

                */

                document.Add(table);

                document.Close();

                Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = rutaPDF;
                prc.Start();

                /*
                byte[] content = Pdf
                    .From(html)
                    .OfSize(PaperSize.Letter)
                    .Content();
                String rutaPDF = @"C:\OpcyonApp\Cotizacion_" + cotizacion.IdCotizacion + ".pdf";

                File.WriteAllBytes(rutaPDF, content);

                Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = rutaPDF;
                prc.Start();
                */
            }
            else if (Tipo == "LONA/VINIL")/*TIPO DE ORDE LONA VINIL*/
            {                
                Document document = null;

                System.IO.Directory.CreateDirectory(@"C:\Impresos\Ordenes");
                String rutaPDF = @"C:\Impresos\Ordenes\OrdenLV_" + NumeroOrden + ".pdf";

                //PdfDocument pdf = new PdfDocument(new PdfReader(@"AlvarezCotizacionL.pdf"), new PdfWriter(rutaPDF));
                PdfDocument pdf = new PdfDocument(new PdfWriter(rutaPDF));
                document = new Document(pdf, PageSize.LETTER);

                document.SetMargins(5, 5, 5, 5);

                float[] columnWidths = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
                PdfFont f = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                float fs = 9;

                ImageData imgLogo = ImageDataFactory.Create(@"C:\Impresos\orden2File.png");

                iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imgLogo);

                pdfImg.SetHeight(350);

                table.AddCell(new Cell(1, 10)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(pdfImg));

                table.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 10)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(pdfImg));               
                

                //PRIMER RENGLON
                /*
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden de trabajo")));
                */
                document.Add(table);

                float bot = 745;

                Paragraph para = new Paragraph(NumeroOrden);
                para.SetFont(f);
                para.SetFontSize(fs);
                para.SetBold();
                para.SetFixedPosition(190, bot, 50);
                document.Add(para);

                para = new Paragraph(tbRecibe.Text);
                para.SetFont(f);
                para.SetFontSize(fs);
                para.SetBold();
                para.SetFixedPosition(425, bot, 150);
                document.Add(para);

                para = new Paragraph(tbClientes.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(130, bot - 15, 250);
                document.Add(para);

                para = new Paragraph(tbSolicita.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(375, bot - 15, 250);
                document.Add(para);

                para = new Paragraph(tbTelefono.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(80, bot - 35, 250);
                document.Add(para);

                if (cbEnvioA.Text == "WHATS")
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(456, bot - 38, 250);
                    document.Add(para);
                }
                else if (cbEnvioA.Text == "CORREO")
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(516, bot - 38, 250);
                    document.Add(para);
                }
                else if (cbEnvioA.Text == "LO TRAJO")
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 38, 250);
                    document.Add(para);
                }

                para = new Paragraph(dtpFecha.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(100, bot - 55, 250);
                document.Add(para);


                para = new Paragraph(tbLonaMedida.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(100, bot - 78, 250);
                document.Add(para);

                if (chbLonaNormal.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(346, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaTraslucida.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(414, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaUv.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(506, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaEcosolvente.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoBYO.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(138, bot - 105, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoBastilla.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(208, bot - 105, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoSobrante.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(297, bot - 105, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoBolsa.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(369, bot - 105, 250);
                    document.Add(para);
                }

                para = new Paragraph(tbLonaAcabadoOtro.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(400, bot - 115, 250);
                document.Add(para);

                para = new Paragraph(tbLonaAcabadoOtro.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(35, bot - 135, 450);
                document.Add(para);

                /*VINIL*/

                para = new Paragraph(tbVinilMedida.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(100, bot - 182, 250);
                document.Add(para);

                if (chbVinilBrillante.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(338, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilMate.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(390, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilUv.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(497, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilEcosolvente.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilEcono.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(65, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilEcoGris.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(123, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilAlta.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(215, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilMicro.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(309, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilTrans.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(360, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilEstBlanco.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(432, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilEstTrans.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(500, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilReflejante.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 204, 250);
                    document.Add(para);
                }

                para = new Paragraph(tbVinilObservaciones.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(55, bot - 234, 450);
                document.Add(para);

                para = new Paragraph(tbOtroMaterial.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(115, bot - 284, 450);
                document.Add(para);

                /*SEGUNDA IMPRESIÓN*/

                bot = bot - 370;

                para = new Paragraph(NumeroOrden);
                para.SetFont(f);
                para.SetFontSize(fs);
                para.SetBold();
                para.SetFixedPosition(190, bot, 50);
                document.Add(para);

                para = new Paragraph(tbRecibe.Text);
                para.SetFont(f);
                para.SetFontSize(fs);
                para.SetBold();
                para.SetFixedPosition(425, bot, 150);
                document.Add(para);

                para = new Paragraph(tbClientes.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(130, bot - 15, 250);
                document.Add(para);

                para = new Paragraph(tbSolicita.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(375, bot - 15, 250);
                document.Add(para);

                para = new Paragraph(tbTelefono.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(80, bot - 35, 250);
                document.Add(para);

                if (cbEnvioA.Text == "WHATS")
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(456, bot - 38, 250);
                    document.Add(para);
                }
                else if (cbEnvioA.Text == "CORREO")
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(516, bot - 38, 250);
                    document.Add(para);
                }
                else if (cbEnvioA.Text == "LO TRAJO")
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 38, 250);
                    document.Add(para);
                }

                para = new Paragraph(dtpFecha.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(100, bot - 55, 250);
                document.Add(para);


                para = new Paragraph(tbLonaMedida.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(100, bot - 78, 250);
                document.Add(para);

                if (chbLonaNormal.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(346, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaTraslucida.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(414, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaUv.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(506, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaEcosolvente.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 81, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoBYO.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(138, bot - 105, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoBastilla.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(208, bot - 105, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoSobrante.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(297, bot - 105, 250);
                    document.Add(para);
                }

                if (chbLonaAcabadoBolsa.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 3);
                    para.SetBold();
                    para.SetFixedPosition(369, bot - 105, 250);
                    document.Add(para);
                }

                para = new Paragraph(tbLonaAcabadoOtro.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(400, bot - 115, 250);
                document.Add(para);

                para = new Paragraph(tbLonaObservaciones.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(35, bot - 135, 450);
                document.Add(para);

                /*VINIL*/

                para = new Paragraph(tbVinilMedida.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(100, bot - 182, 250);
                document.Add(para);

                if (chbVinilBrillante.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(338, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilMate.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(390, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilUv.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(497, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilEcosolvente.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 184, 250);
                    document.Add(para);
                }

                if (chbVinilEcono.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(65, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilEcoGris.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(123, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilAlta.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(215, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilMicro.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(309, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilTrans.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(360, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilEstBlanco.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(432, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilEstTrans.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(500, bot - 204, 250);
                    document.Add(para);
                }

                if (chbVinilReflejante.IsChecked == true)
                {
                    para = new Paragraph("X");
                    para.SetFont(f);
                    para.SetFontSize(fs + 2);
                    para.SetBold();
                    para.SetFixedPosition(578, bot - 204, 250);
                    document.Add(para);
                }

                para = new Paragraph(tbVinilObservaciones.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(55, bot - 234, 450);
                document.Add(para);

                para = new Paragraph(tbOtroMaterial.Text);
                para.SetFont(f);
                para.SetFontSize(fs - 2);
                para.SetBold();
                para.SetFixedPosition(115, bot - 284, 450);
                document.Add(para);


                document.Close();

                Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = rutaPDF;
                prc.Start();
            }
        }

        private void dgOrdenesAnteriores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void tbDelNumero_KeyUp(object sender, KeyEventArgs e)
        {
            if (tbCantidad.Text.Length > 0 && tbDelNumero.Text.Length > 0)
            {
                int Cantidad = int.Parse(tbCantidad.Text);
                int Del = int.Parse(tbDelNumero.Text);
                int Al = Del + Cantidad - 1;

                tbAlNumero.Text = Al.ToString();
            }
        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {
            if (tbCantidad.Text.Length > 0 && tbDelNumero.Text.Length > 0)
            {
                int Cantidad = int.Parse(tbCantidad.Text);
                int Del = int.Parse(tbDelNumero.Text);
                int Al = Del + Cantidad - 1;

                tbAlNumero.Text = Al.ToString();
            }
        }

        private void cbConFolio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (tbDelNumero != null && tbAlNumero != null)
                {
                    if (cbConFolio.SelectedIndex == 0)
                    {
                        tbDelNumero.IsEnabled = true;
                        tbAlNumero.IsEnabled = true;
                    }
                    else
                    {
                        tbDelNumero.IsEnabled = false;
                        tbAlNumero.IsEnabled = false;
                    }
                }
            }
            catch (Exception exc)
            {

            }
        }

        private void lblFolioOrden_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                lblFolioOrden.Content = dbContext.Valores.First().numero_orden;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            String NumeroOrden = "12345";
            Document document = null;

            System.IO.Directory.CreateDirectory(@"C:\Impresos\Ordenes");
            String rutaPDF = @"C:\Impresos\Ordenes\OrdenT_" + NumeroOrden + ".pdf";

            //PdfDocument pdf = new PdfDocument(new PdfReader(@"AlvarezCotizacionL.pdf"), new PdfWriter(rutaPDF));
            PdfDocument pdf = new PdfDocument(new PdfWriter(rutaPDF));
            document = new Document(pdf, PageSize.LETTER);

            document.SetMargins(5, 5, 5, 5);

            float[] columnWidths = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
            PdfFont f = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            float fs = 9;

            ImageData imgLogo = ImageDataFactory.Create(@"C:\Impresos\orden2File.png");

            iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imgLogo);

            pdfImg.SetHeight(350);

            table.AddCell(new Cell(1, 10)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(pdfImg));

            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 10)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(pdfImg));

            //PRIMER RENGLON
            /*
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Orden de trabajo")));
            */
            document.Add(table);

            document.Close();

            Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = rutaPDF;
            prc.Start();
        }

        private void cbTipoOrden_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTipoOrden.SelectedIndex < 0)
            {

            }
            else if (cbTipoOrden.SelectedIndex == 0)
            {
                if (gTipo1 != null)
                {
                    gTipo1.Visibility = Visibility.Visible;
                    gTipo2.Visibility = Visibility.Hidden;
                }                
            }
            else if (cbTipoOrden.SelectedIndex == 1)
            {
                if (gTipo2 != null)
                {
                    gTipo1.Visibility = Visibility.Hidden;
                    gTipo2.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
