using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para VerCotizaciones.xaml
    /// </summary>
    public partial class VerCotizaciones : Window
    {
        ObservableCollection<Clientes> _clientes;
        Clientes _clienteElegido;

        ObservableCollection<Obtener_Cotizaciones_Fecha_Result> _ordenesAnteriores;
        List<Ordenes> OrdenesAnteriores;

        ObservableCollection<CotizacionLlenado> _cotizacion;

        float _TotalNota = 0;

        public VerCotizaciones()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFechaNota.SelectedDate = DateTime.Now;
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = new ObservableCollection<Clientes>(dbContext.Clientes);
                tbClientes.AutoCompleteSource = _clientes;
                tbNumero.Text = dbContext.NumeroNota.Where(N => N.id_numeronota == 1).FirstOrDefault().numero;
            }

            _cotizacion = new ObservableCollection<CotizacionLlenado>();
            dgCotizacion.ItemsSource = null;
            dgCotizacion.ItemsSource = _cotizacion;

            dpFechaRecepcion.SelectedDate = DateTime.Now;
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;
                lblNombreCliente.Content = _clienteElegido.nombre;
                lblDireccionCliente.Content = _clienteElegido.domicilio + " " + _clienteElegido.colonia + " CP " + _clienteElegido.codigo_postal;
                lblLugar.Content = _clienteElegido.ciudad + " " + _clienteElegido.estado;
                lblRFC.Content = _clienteElegido.rfc;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    /*
                    _ordenesAnteriores = new ObservableCollection<Obtener_Cotizaciones_Fecha_Result>(dbContext.Obtener_Cotizaciones_Fecha(dpFechaRecepcion.SelectedDate.Value.ToShortDateString(), _clienteElegido.id_cliente).ToList());
                    dgNotasPasadas.ItemsSource = null;
                    dgNotasPasadas.ItemsSource = _ordenesAnteriores;
                    */
                    OrdenesAnteriores = dbContext.Ordenes.Where(O => O.id_cliente == _clienteElegido.id_cliente).ToList();
                    dgNotasPasadas.ItemsSource = null;
                    dgNotasPasadas.ItemsSource = OrdenesAnteriores;
                }
            }
        }

        private void CalcularTotales()
        {
            double total = 0;
            foreach (CotizacionLlenado item in _cotizacion)
            {
                total = total + item.Importe;
            }
            lblTotal.Content = "$ " + total.ToString();
            _TotalNota = float.Parse(total.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VerCotizacionesArticuloInfo articulo = new VerCotizacionesArticuloInfo(this, null);
            articulo.ShowDialog();
        }

        public void AgregarArticulo(CotizacionLlenado articulo)
        {
            _cotizacion.Add(articulo);
            dgCotizacion.ItemsSource = null;
            dgCotizacion.ItemsSource = _cotizacion;
            CalcularTotales();
        }

        public void ActualizarArticulo(CotizacionLlenado articulo)
        {
            if (dgCotizacion.SelectedItem != null)
            {
                CotizacionLlenado i = (CotizacionLlenado)dgCotizacion.SelectedItem;
                i.Cantidad = articulo.Cantidad;
                i.Descripcion = articulo.Descripcion;
                i.PrecioUnitario = articulo.PrecioUnitario;
                i.Importe = articulo.Importe;
                dgCotizacion.ItemsSource = null;
                dgCotizacion.ItemsSource = _cotizacion;
                CalcularTotales();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (dgCotizacion.SelectedItem != null)
            {
                CotizacionLlenado i = (CotizacionLlenado)dgCotizacion.SelectedItem;
                VerCotizacionesArticuloInfo articulo = new VerCotizacionesArticuloInfo(this, i);
                articulo.ShowDialog();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (dpFechaNota.SelectedDate == null)
            {
                MessageBox.Show("No ha elegido una fecha para la nota.");
                return;
            }
            if (dgCotizacion.Items.Count == 0)
            {
                MessageBox.Show("No hay conceptos en la cotización.");
                return;
            }
            if (_clienteElegido == null)
            {
                MessageBox.Show("No ha elegido un cliente.");
                return;
            }
            else
            {
                if (MessageBox.Show("¿Desea crear la nota?", "Atención", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            CalcularTotales();

                            var NumeroNota = dbContext.NumeroNota.Where(N => N.id_numeronota == 1).FirstOrDefault();
                            NumeroNota.numero = (int.Parse(NumeroNota.numero) + 1).ToString();
                            dbContext.SaveChanges();

                            var Nota = new Notas();

                            Nota.id_cliente = _clienteElegido.id_cliente;
                            Nota.total = _TotalNota;
                            Nota.pagada = "NO";
                            Nota.estado = "ACTIVO";
                            Nota.fecha = dpFechaNota.SelectedDate.Value.ToShortDateString();
                            Nota.numero = tbNumero.Text;
                            Nota.solicita = tbSolicita.Text;

                            dbContext.Notas.Add(Nota);

                            dbContext.SaveChanges();

                            DetalleNota Detalle = new DetalleNota();

                            foreach (CotizacionLlenado item in _cotizacion)
                            {
                                Detalle = new DetalleNota();

                                Detalle.cantidad = item.Cantidad;
                                Detalle.descripcion = item.Descripcion;
                                Detalle.id_nota = Nota.id_nota;
                                Detalle.importe = item.Importe;

                                dbContext.DetalleNota.Add(Detalle);

                                if (item.IdOrden > 0)
                                {
                                    dbContext.Adjuntar_Orden_Nota(Nota.id_nota, item.IdOrden);
                                    dbContext.Modificar_Tipo_Orden(item.IdOrden, "NOTA");

                                    NotaOrden nd = new NotaOrden();

                                    nd.id_nota = Nota.id_nota;
                                    nd.id_orden = item.IdOrden;

                                    dbContext.NotaOrden.Add(nd);
                                }

                                if (item.IdInsumo > 0)
                                {
                                    SalidasInventario nueva = new SalidasInventario();
                                    nueva.fecha = dpFechaNota.SelectedDate.Value;
                                    nueva.presupuesto = 0;
                                    nueva.orden_trabajo = "";
                                    nueva.factura = "";
                                    nueva.cantidad = item.Cantidad;
                                    nueva.id_insumo = item.IdInsumo;
                                    nueva.descripcion = item.DescripcionInsumo;
                                    nueva.nota = Nota.id_nota.ToString();

                                    dbContext.SalidasInventario.Add(nueva);

                                    Insumos insumo = dbContext.Insumos.Where(I => I.id_insumo == item.IdInsumo).First();
                                    insumo.stock = insumo.stock - nueva.cantidad;

                                    Detalle.id_articulo = item.IdInsumo;
                                }
                            }

                            dbContext.SaveChanges();

                            ImprimirPDF();

                            this.Close();
                        }
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("Error: " + exc.Message);
                    }
                }
            }
        }

        public void ImprimirPDF()
        {
            String rutaPDF = "";

            Document document = null;

            rutaPDF = @"C:\Impresos\Cotizaciones\Cotizacion_" + tbNumero.Text + ".pdf";

            //PdfDocument pdf = new PdfDocument(new PdfReader(@"AlvarezCotizacionL.pdf"), new PdfWriter(rutaPDF));
            PdfDocument pdf = new PdfDocument(new PdfWriter(rutaPDF));
            document = new Document(pdf, PageSize.LETTER.Rotate());

            document.SetMargins(10, 10, 10, 10);

            float[] columnWidths = { 1, 5, 1, 1, 1, 1, 5, 1, 1 };
            Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
            PdfFont f = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            /*
            Cell cell = new Cell(1, 5)
                .Add(new Paragraph("This is a header"))
                .SetFont(f)
                .SetFontSize(13)
                .SetFontColor(DeviceGray.WHITE)
                .SetBackgroundColor(DeviceGray.BLACK)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            */
            float fs = 9;

            //1er renglón

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("PRESUPUESTO")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FOLIO: " + tbNumero.Text)));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("PRESUPUESTO")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FOLIO: " + tbNumero.Text)));

            //2do Renglón

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FECHA")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(DateTime.Now.Date.ToShortDateString())));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FECHA")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(DateTime.Now.Date.ToShortDateString())));

            //3er Renglón

            table.AddCell(new Cell()                
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("NOMBRE")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(lblNombreCliente.Content.ToString())));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("NOMBRE")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(lblNombreCliente.Content.ToString())));

            //4to Renglón

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("DIRECCIÓN")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(lblDireccionCliente.Content.ToString())));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("DIRECCIÓN")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(lblDireccionCliente.Content.ToString())));

            //5to Renglón

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("CIUDAD")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(lblLugar.Content.ToString())));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("CIUDAD")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(lblLugar.Content.ToString())));

            //to renglón Separador

            table.AddCell(new Cell(1, 9)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            ImageData imageData = ImageDataFactory.Create(@"Imagenes/LogoAlvarez.png");

            iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imageData);
            pdfImg.SetHeight(250);
            pdfImg.SetFixedPosition(50, 200);

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("CANTIDAD")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                
                .Add(new Paragraph("DESCRIPCIÓN")));            

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                
                .Add(new Paragraph("TOTAL")));


            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("CANTIDAD")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("DESCRIPCIÓN")));            

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("TOTAL")));

            foreach (CotizacionLlenado item in _cotizacion)
            {                
                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)                        
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Descripcion)));                

                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Importe.ToString())));

                table.AddCell(new Cell()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Descripcion)));                

                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Importe.ToString())));
            }

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(10, 10, 0)
                .Add(new Paragraph("ESTOS PRECIO SON MÁS IVA")));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(420, 10, 0)
                .Add(new Paragraph("ESTOS PRECIO SON MÁS IVA")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(250, 10, 0)
                .Add(new Paragraph("TOTAL: " + lblTotal.Content.ToString())));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );            

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(650, 10, 0)                
                .Add(new Paragraph("TOTAL: " + lblTotal.Content.ToString())));


            document.Add(pdfImg);
            pdfImg.SetFixedPosition(480, 200);
            document.Add(pdfImg);

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

        private void dgNotasPasadas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void dpFechaRecepcion_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;
                lblNombreCliente.Content = _clienteElegido.nombre;
                lblDireccionCliente.Content = _clienteElegido.domicilio + " " + _clienteElegido.colonia + " CP " + _clienteElegido.codigo_postal;
                lblLugar.Content = _clienteElegido.ciudad + " " + _clienteElegido.estado;
                lblRFC.Content = _clienteElegido.rfc;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    _ordenesAnteriores = new ObservableCollection<Obtener_Cotizaciones_Fecha_Result>(dbContext.Obtener_Cotizaciones_Fecha(dpFechaRecepcion.SelectedDate.Value.ToShortDateString(), _clienteElegido.id_cliente).ToList());
                    dgNotasPasadas.ItemsSource = null;
                    dgNotasPasadas.ItemsSource = _ordenesAnteriores;
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (dgCotizacion.SelectedItem != null)
            {
                CotizacionLlenado c = (CotizacionLlenado)dgCotizacion.SelectedItem;
                _cotizacion.Remove(c);
                dgCotizacion.ItemsSource = null;
                dgCotizacion.ItemsSource = _cotizacion;
                CalcularTotales();
            }
            else
            {
                MessageBox.Show("No ha elegido un articulo.");
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (dgNotasPasadas.SelectedItems.Count > 0)
            {
                /*
                foreach (Obtener_Cotizaciones_Fecha_Result item in dgNotasPasadas.SelectedItems)
                {
                    CotizacionLlenado _articulo = new CotizacionLlenado();
                    Obtener_Cotizaciones_Fecha_Result a = new Obtener_Cotizaciones_Fecha_Result();

                    a = (Obtener_Cotizaciones_Fecha_Result)item;

                    _articulo.IdOrden = a.id_orden;
                    _articulo.Cantidad = (int)a.cantidad;
                    _articulo.Descripcion = a.nombre_trabajo + " TAMAÑO: " + a.tamano + " COLOR: " + a.color_tintas + " PAPEL: " + a.tipo_papel;
                    _articulo.PrecioUnitario = (double)a.total / (int)a.cantidad;
                    _articulo.Importe = (double)a.total;

                    _cotizacion.Add(_articulo);
                }
                */

                foreach (Ordenes item in dgNotasPasadas.SelectedItems)
                {
                    CotizacionLlenado _articulo = new CotizacionLlenado();

                    _articulo.IdOrden = item.id_orden;
                    _articulo.Cantidad = (int)item.cantidad;
                    _articulo.Descripcion = item.nombre_trabajo + " TAMAÑO: " + item.tamano + " COLOR: " + item.color_tintas + " PAPEL: " + item.tipo_papel;
                    _articulo.PrecioUnitario = (double)item.total / (int)item.cantidad;
                    _articulo.Importe = (double)item.total;

                    _cotizacion.Add(_articulo);
                }
                CalcularTotales();
            }
        }
    }
}
