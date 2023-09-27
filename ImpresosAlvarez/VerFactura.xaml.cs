using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using ImpresosAlvarez.mx.facturacfdiCancelacion.v40;
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
using System.Diagnostics;
using System.IO;
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
using System.Xml;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para VerFactura.xaml
    /// </summary>
    public partial class VerFactura : Window
    {
        Facturas _factura;
        ControlFacturas _parent;

        List<ConceptoFactura> _conceptos;
        List<Contribuyentes> _contribuyentes;
        List<DetalleFactura> _detalle;

        private Factura datosFacturaElectronica;
        private ImpresosAlvarez.Entity.FacturaDigital datosFacturaDigital;

        Clientes _cliente;

        private String rutaCertificado;
        private String rutaLlave;
        private String contraseñaLlave;
        private String nombreEmisor;
        private String rfcEmisor;
        private String serie;
        private String usuarioFacturacion;
        private String contraseñaFacturacion;
        private String curp;
        private String regimen;

        String UUIDSustituye;
        Entity.FacturaDigital FacturaElegida;
        String Motivo;

        String rutaPDF;
        public VerFactura(ControlFacturas Parent, Facturas Factura)
        {
            InitializeComponent();
            this._parent = Parent;
            this._factura = Factura;
        }

        public VerFactura(Facturas Factura)
        {
            InitializeComponent();
            this._parent = null;
            this._factura = Factura;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                lblFolio.Content = _factura.numero;

                datosFacturaElectronica = new Factura();

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    _contribuyentes = dbContext.Contribuyentes.ToList();
                    cbContribuyentes.ItemsSource = _contribuyentes;
                    cbContribuyentes.SelectedValuePath = "id_contribuyente";
                    cbContribuyentes.DisplayMemberPath = "nombre";
                    cbContribuyentes.SelectedValue = _factura.id_contribuyente;

                    _cliente = dbContext.Clientes.Where(C => C.id_cliente == _factura.id_cliente).First();

                    lblNombre.Content = _cliente.nombre;
                    lblRFC.Content = _cliente.rfc;

                    _detalle = dbContext.DetalleFactura.Where(D => D.id_factura == _factura.id_factura).ToList();

                    _conceptos = new List<ConceptoFactura>();

                    foreach (DetalleFactura item in _detalle)
                    {
                        ConceptoFactura c = new ConceptoFactura();
                        c.Cantidad = (int)item.cantidad;
                        c.Descripcion = item.descripcion;
                        c.Importe = (float)item.importe;
                        if (item.precio_unitario != null)
                        {
                            c.PrecioUnitario = (float)item.precio_unitario;
                            c.Unidad = item.unidad;
                            c.Clave = item.clave_servicio;
                        }
                        else
                        {
                            c.PrecioUnitario = c.Importe / c.Cantidad;
                            c.Unidad = "";
                            c.Clave = "";
                        }
                        _conceptos.Add(c);
                    }
                    dgConceptos.ItemsSource = _conceptos;
                    regimen = "Regimen de incorporación fiscal";

                    lblTotal.Content = "$ " + Math.Round((double)_factura.total, 2).ToString();
                    lblEstado.Content = _factura.estado;

                    datosFacturaDigital = dbContext.FacturaDigital.Where(FD => FD.id_factura == _factura.id_factura).FirstOrDefault();
                    lblMetodoPago.Content = datosFacturaDigital.forma_pago;

                    CargarDatosFactura();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR: " + exc.Message);
            }
        }

        private void bntCancelarFactura_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Desea cancelar la factura?", "ATENCIÓN", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MotivosCancelacionFactura motivos = new MotivosCancelacionFactura(this);
                motivos.ShowDialog();
            }
        }

        public void ElegirFacturaRelacionada(String UUID, Entity.FacturaDigital FacturaElegida)
        {
            this.UUIDSustituye = UUID;
            this.FacturaElegida = FacturaElegida;
        }

        public void Cancelacion(String Motivo)
        {
            this.Motivo = Motivo;
            if (cbContribuyentes.Text.Contains("JOSE"))
            {
                rutaCertificado = @"C:\Impresos\Jose\Certificado.cer";
                rutaLlave = @"C:\Impresos\Jose\Llave.key";
                contraseñaLlave = "Musica47";
                nombreEmisor = "JOSE ALVAREZ JIMENEZ";
                rfcEmisor = "AAJJ470205DH1";
                serie = "-";
                usuarioFacturacion = "JoseAlvarezJi";
                contraseñaFacturacion = "oF5r1o6S3";
                curp = "AAJJ470205HNTLMS00";
                regimen = "Regimen de incorporación fiscal";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }
            else if (cbContribuyentes.Text.Contains("MARIA"))
            {
                rutaCertificado = @"C:\Impresos\Maria\Certificado.cer";
                rutaLlave = @"C:\Impresos\Maria\Llave.key";
                contraseñaLlave = "M1945luz";
                nombreEmisor = "MARIA DE LA LUZ RAMIREZ GALVAN";
                rfcEmisor = "RAGL450530F25";
                serie = "-";
                usuarioFacturacion = "VicMar";
                contraseñaFacturacion = "773C8*8F1";
                curp = "RAGL450530MDFMLZ00";
                regimen = "Regimen de incorporación fiscal";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }
            else if (cbContribuyentes.Text.Contains("VICTOR"))
            {
                rutaCertificado = @"C:\Impresos\Victor\Certificado.cer";
                rutaLlave = @"C:\Impresos\Victor\Llave.key";
                contraseñaLlave = "ALVA7209E51";
                nombreEmisor = "VICTOR MANUEL ALVAREZ RAMIREZ";
                rfcEmisor = "AARV720921E51";
                serie = "-";
                usuarioFacturacion = "VictorAlvarez";
                contraseñaFacturacion = "g8r.83*.5";
                curp = "AARV720921HDFLMC04";
                regimen = "Regimen de incorporación fiscal";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 619 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }

            string usuario = usuarioFacturacion;
            string pass = contraseñaFacturacion;
            string strPathLlave = rutaLlave;
            string strPathCert = rutaCertificado;
            //string[] folios = new string[1];
            wsFolios40[] folios = new wsFolios40[1];
            
            wsCancelacionResponse resCancelacion = new wsCancelacionResponse();
            //WSCancelacionService serCancel = new WSCancelacionService();
            WSCancelacion40Service serCancel = new WSCancelacion40Service();

            try
            {
                accesos acc = new accesos();
                acc.usuario = usuario;
                acc.password = pass;

                byte[] llavePublicaBytes = File.ReadAllBytes(rutaCertificado);
                byte[] llavePrivadaBytes = File.ReadAllBytes(strPathLlave);

                string[] cadenaOriginal = datosFacturaDigital.cadena_original.Split('|');

                wsFolios40 fol = new wsFolios40();
                fol.folio = new wsFolio();

                if (UUIDSustituye != null)
                {
                    fol.folio.uuid = cadenaOriginal[4];
                    fol.folio.folioSustitucion = UUIDSustituye;
                    fol.folio.motivo = Motivo;
                    folios[0] = fol;
                }
                else
                {
                    fol.folio.uuid = cadenaOriginal[4];
                    fol.folio.folioSustitucion = "";
                    fol.folio.motivo = Motivo;
                    folios[0] = fol;
                }

                string fecha = DateTime.UtcNow.AddHours(-7).ToString("s");

                resCancelacion = serCancel.Cancelacion40_1(rfcEmisor, fecha, folios, llavePublicaBytes, llavePrivadaBytes, contraseñaLlave, acc);

                File.WriteAllText(@"C:\Impresos\Facturacion\Acuse.xml", resCancelacion.acuse);

                switch (resCancelacion.folios[0].estatusUUID)
                {
                    case "201":
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            Entity.FacturaDigital f = dbContext.FacturaDigital.Where(F => F.id_factura == _factura.id_factura).FirstOrDefault();
                            f.acuse = resCancelacion.acuse;
                            f.fecha_cancelado = DateTime.UtcNow.AddHours(-7);

                            Facturas fac = dbContext.Facturas.Where(F => F.id_factura == _factura.id_factura).FirstOrDefault();
                            fac.estado = "CANCELADO";

                            List<FacturaOrden> facturaOrdenes = dbContext.FacturaOrden.Where(FO => FO.id_factura == _factura.id_factura).ToList();

                            foreach (FacturaOrden item in facturaOrdenes)
                            {
                                dbContext.Modificar_Tipo_Orden_Grupo(_factura.id_factura, "COTIZACION");
                                dbContext.Borrar_FacturaOrden(_factura.id_factura);
                                dbContext.Cambiar_Estado_Factura(_factura.id_factura, "CANCELADO");
                                dbContext.Factura_Razon_Cancelado(_factura.id_factura, "");
                            }

                            dbContext.SaveChanges();
                            _parent.ActualizarLista();
                            MessageBox.Show("Se ha cancelado la factura.");
                            this.Close();
                        }
                        break;
                    case "202":
                        MessageBox.Show("La factura ya ha sido cancelada anteriormente.");
                        break;
                    case "203":
                        MessageBox.Show("La factura no se ha encontrado.");
                        break;
                    case "204":
                        MessageBox.Show("La factura no se puede cancelar.");
                        break;
                    case "205":
                        MessageBox.Show("La factura no existe.");
                        break;
                    default:
                        MessageBox.Show("Clave de resultado " + resCancelacion.folios[0].estatusUUID);
                        break;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CargarDatosFactura()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(datosFacturaDigital.xml);

            XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);

            nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
            nms.AddNamespace("tfd", "http://www.sat.gob.mx/cfd/4");

            XmlAttribute xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//@UUID", nms);
            datosFacturaElectronica.uuid = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//@NoCertificadoSAT", nms);
            datosFacturaElectronica.numeroCertificadoSAT = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//@FechaTimbrado", nms);
            datosFacturaElectronica.fechaTimbrado = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Folio", nms);
            if (xAttrib != null)
            {
                datosFacturaElectronica.folio = xAttrib.Value;
            }
            else
            {
                MessageBox.Show("Error. No se encontró el folio.");
                return;
            }

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Fecha", nms);
            datosFacturaElectronica.fechaExpedicion = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@FormaPago", nms);
            datosFacturaElectronica.formaPago = xAttrib.Value;

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                datosFacturaElectronica.formaPagoTexto = dbContext.FormasPago.Where(C => C.Clave == datosFacturaElectronica.formaPago).First().FormaPago;
            }

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@NoCertificado", nms);
            datosFacturaElectronica.numeroCertificado = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@LugarExpedicion", nms);
            datosFacturaElectronica.lugarExpedicion = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@MetodoPago", nms);
            datosFacturaElectronica.metodoPago = xAttrib.Value;

            datosFacturaElectronica.metodoPagoTexto = datosFacturaElectronica.metodoPago;
            /*
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                //datosFacturaElectronica.metodoPagoTexto = dbContext.FormasPago.Where(C => C.Clave == datosFacturaElectronica.metodoPago).First().FormaPago;
            }
            */

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Serie", nms);
            datosFacturaElectronica.serie = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@SubTotal", nms);
            datosFacturaElectronica.subTotal = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Total", nms);
            datosFacturaElectronica.total = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Nombre", nms);
            datosFacturaElectronica.nombreEmisor = xAttrib.Value;            

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Rfc", nms);
            datosFacturaElectronica.rfcEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@DomicilioFiscalReceptor", nms);
            datosFacturaElectronica.domicilioReceptorCodigoPostal = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@DomicilioFiscalReceptor", nms);
            datosFacturaElectronica.domicilioEmisorCodigoPostal = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@Nombre", nms);
            datosFacturaElectronica.nombreReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@RegimenFiscalReceptor", nms);
            datosFacturaElectronica.regimenFiscalReceptor = xAttrib.Value;

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                datosFacturaElectronica.regimenFiscalReceptor = dbContext.RegimenFiscal.Where(C => C.Clave == datosFacturaElectronica.regimenFiscalReceptor).First().Descripcion;
            }

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@Rfc", nms);
            datosFacturaElectronica.rfcReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@UsoCFDI", nms);
            datosFacturaElectronica.usoCFDI = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Impuestos//@TotalImpuestosTrasladados", nms);
            datosFacturaElectronica.iva = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Impuestos//@TotalImpuestosRetenidos", nms);
            datosFacturaElectronica.retencionIsr = xAttrib.Value;

            datosFacturaElectronica.selloSAT = datosFacturaDigital.sello_sat;

            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaDigital.cadena_original;

            datosFacturaElectronica.selloCFD = datosFacturaDigital.sello_cfdi;

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                datosFacturaElectronica.usoCFDITexto = dbContext.UsosCFDI.Where(C => C.Clave == datosFacturaElectronica.usoCFDI).First().Uso;
            }

            if (cbContribuyentes.Text.Contains("JOSE"))
            {
                rutaCertificado = @"C:\Impresos\Jose\Certificado.cer";
                rutaLlave = @"C:\Impresos\Jose\Llave.key";
                contraseñaLlave = "Musica47";
                nombreEmisor = "JOSE ALVAREZ JIMENEZ";
                rfcEmisor = "AAJJ470205DH1";
                serie = "-";
                usuarioFacturacion = "JoseAlvarezJi";
                contraseñaFacturacion = "oF5r1o6S3";
                curp = "AAJJ470205HNTLMS00";
                regimen = "Regimen de incorporación fiscal";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }
            else if (cbContribuyentes.Text.Contains("MARIA"))
            {
                rutaCertificado = @"C:\Impresos\Maria\Certificado.cer";
                rutaLlave = @"C:\Impresos\Maria\Llave.key";
                contraseñaLlave = "M1945luz";
                nombreEmisor = "MARIA DE LA LUZ RAMIREZ GALVAN";
                rfcEmisor = "RAGL450530F25";
                serie = "-";
                usuarioFacturacion = "VicMar";
                contraseñaFacturacion = "773C8*8F1";
                curp = "RAGL450530MDFMLZ00";
                regimen = "Regimen de incorporación fiscal";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }
            else if (cbContribuyentes.Text.Contains("VICTOR"))
            {
                rutaCertificado = @"C:\Impresos\Victor\Certificado.cer";
                rutaLlave = @"C:\Impresos\Victor\Llave.key";
                contraseñaLlave = "ALVA7209E51";
                nombreEmisor = "VICTOR MANUEL ALVAREZ RAMIREZ";
                rfcEmisor = "AARV720921E51";
                serie = "-";
                usuarioFacturacion = "VictorAlvarez";
                contraseñaFacturacion = "g8r.83*.5";
                curp = "AARV720921HDFLMC04";
                regimen = "Regimen de incorporación fiscal";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 619 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }
        }
        void ImprimirPDF(Boolean SAT)
        {

            System.IO.Directory.CreateDirectory(@"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", ""));
            String Directorio = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", "");
            string fileName;
            if (SAT)
            {
                fileName = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", "") + @"\fac_" + cbContribuyentes.SelectedValue.ToString() + "_" + _factura.numero + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
            }
            else
            {
                fileName = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", "") + @"\pre_" + cbContribuyentes.SelectedValue.ToString() + "_" + _factura.numero + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
            }

            Document document = null;

            PdfDocument pdf = new PdfDocument(new PdfWriter(fileName));
            document = new Document(pdf, PageSize.LETTER);

            document.SetMargins(10, 10, 10, 10);

            float[] columnWidths = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
            Table tableSAT = new Table(UnitValue.CreatePercentArray(columnWidths));

            PdfFont f = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont fb = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            float fs = 9;
            float fxs = 4;

            ImageData imgLogo = ImageDataFactory.Create(@"C:\Impresos\Logo\Logo.png");

            iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imgLogo);

            //Renglon 1
            pdfImg.SetHeight(70);

            table.AddCell(new Cell(4, 4)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(pdfImg));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.nombreEmisor)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Factura")));

            //Renglon           

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("RFC " + datosFacturaElectronica.rfcEmisor)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Serie y folio: " + _factura.numero)));

            //Renglon
            /*
            pdfImg.SetHeight(150);
            pdfImg.SetFixedPosition(50, 50);
            */

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.domicilioEmisorCalle)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                //.Add(new Paragraph("Fecha: " + DateTime.Today.ToShortDateString())));
                .Add(new Paragraph("Fecha: " + datosFacturaElectronica.fechaExpedicion.Substring(0, 10))));

            //Renglon

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.domicilioEmisorColonia + " CP " + datosFacturaElectronica.domicilioEmisorCodigoPostal + " " + datosFacturaElectronica.domicilioEmisorMunicipio + " " + datosFacturaElectronica.domicilioEmisorEstado + "\nTel 311-217-13-35 311-217-85-17")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(regimen)));

            //Datos cliente
            //Renglon
            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .Add(new Paragraph("Datos del cliente")));
            //Renglón 1
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("RFC")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(_cliente.rfc)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Clave moneda")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("MXN")));

            //Renglón 2
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Nombre")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(_cliente.nombre)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Uso CFDI")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.usoCFDITexto)));

            //Renglón 3
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Calle")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(_cliente.domicilio + " " + _cliente.numero_exterior)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Colonia")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(_cliente.colonia)));

            //Renglon
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Ciudad, Estado")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(_cliente.ciudad + ", " + _cliente.estado)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("C. P. ")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(_cliente.codigo_postal)));

            //Renglón
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Metodo de pago")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.metodoPagoTexto)));

            table.AddCell(new Cell(1, 2)
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
               .SetFont(fb)
               .SetFontSize(fs)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .Add(new Paragraph("Forma de pago")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.formaPagoTexto)));

            //Renglón
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Régimen")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.regimenFiscalReceptor)));

            table.AddCell(new Cell(1, 2)
               .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
               .SetFont(fb)
               .SetFontSize(fs)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph()));

            //Conceptos
            //Renglón 1
            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("ProdServ")));

            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Cantidad")));

            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Clave Unidad")));

            table.AddCell(new Cell(1, 5)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Descripción")));

            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Precio Unitario")));

            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Importe")));

            foreach (ConceptoFactura item in _conceptos)
            {
                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Clave)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Cantidad.ToString())));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Unidad)));

                table.AddCell(new Cell(1, 5)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Descripcion)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(item.PrecioUnitario.ToString()))));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(item.Importe.ToString()))));
            }

            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(" ")));


            //Totales
            //Renglón 1
            table.AddCell(new Cell(1, 9)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Subtotal ")));

            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("$ " + AddDecimals(_factura.subtotal.ToString()))));

            table.AddCell(new Cell(1, 9)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("16% IVA: ")));

            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.iva))));

            if (_cliente.aplica_retencion == "SI")
            {
                table.AddCell(new Cell(1, 9)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("1.25% RETENCIÓN ISR: ")));

                table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.retencionIsr))));
            }

            table.AddCell(new Cell(1, 9)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Total: ")));

            table.AddCell(new Cell(1, 1)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.total))));

            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(ConvertirALetra(datosFacturaElectronica.total))));
            //PARA CFDI 4.0 o 3.3
            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .Add(new Paragraph("Este documento es una representación impresa de un CFDI 4.0")));


            if (SAT)
            {
                /*
                datosFacturaElectronica.cadenaOriginalSAT = "||1.0|912B245D-275F-4F52-8E4A-7FBFCDBFD302|2017-04-26T14: 07:40 | acnCGJZrkojvGs8MxWaoEAmgVBu9t + r + Dp26DzDUcu7lu8 / 0LplKVlq1DnYDVCd6KxLnoCJ8amuhnCkUcoK68j0tKlBelwOLSFEsYAQepsT7ZJkhdJZ27Sz2nkFuLm5EKbRTZIPItN30DaKMAs8VcJe17QIp9vj / 3xddZoFD9fl0qsAYJTl22CkdWcuNoVz2WgAwOgDL + U2qHNuczvEk8ckoFM0om5D1PjSAGUM1hyXCxn8 + MMz4EfTT9X837ldQNRqXMSzfuhFbdMxOTCN9eUCjb4EIJgvE37LzGsokDW / tMMrus9jzVuEQFhZ5OFfOmVM / Q5sQQLpEquqvsL / G + Q ==| 00001000000405607284 || ";
                datosFacturaElectronica.selloSAT = "V2XaPk+SsLYu4boX2rWS1vCJWuJUDx/1F9cPyTvJXcsEGESH84BS5nIIvb2aDqFhYoy1HLiTqUGRT0GbnnGTS1ZElonGA9AmCVeHshgEJypIY2s0WS0OKblUwgCQZWi2n6uMG2bvMPC3I9ChHH21c44Q6ZgV/y5W5A8Uawb/+jx2hVj9pnWTta0AD7yVfoiTLwdlpvoeqWlamBX2reAW7NIDaz8KVEesAdd9bTDqjsw1dFCjbYXmhubqtD2WMnsKb3aKPfD4aHnINNscm15nkgn1qMqnbFWLFm3kFU9fAVmQbf7eSolKKLsKVuvRw7keJeJgWzs5e0lLXnt+JUfDWg==";
                datosFacturaElectronica.selloCFD = "acnCGJZrkojvGs8MxWaoEAmgVBu9t+r+Dp26DzDUcu7lu8/0LplKVlq1DnYDVCd6KxLnoCJ8amuhnCkUcoK68j0tKlBelwOLSFEsYAQepsT7ZJkhdJZ27Sz2nkFuLm5EKbRTZIPItN30DaKMAs8VcJe17QIp9vj / 3xddZoFD9fl0qsAYJTl22CkdWcuNoVz2WgAwOgDL + U2qHNuczvEk8ckoFM0om5D1PjSAGUM1hyXCxn8 + MMz4EfTT9X837ldQNRqXMSzfuhFbdMxOTCN9eUCjb4EIJgvE37LzGsokDW / tMMrus9jzVuEQFhZ5OFfOmVM / Q5sQQLpEquqvsL / G + Q == ";
                datosFacturaElectronica.uuid = "B8023C9E-1205-4824-ACE6-96B08E134BBA";
                datosFacturaElectronica.numeroCertificado = "00001000000408531476";
                datosFacturaElectronica.numeroCertificadoSAT = "00001000000412961981";
                datosFacturaElectronica.fechaTimbrado = "2020-11-04T17:22:20";
                */

                datosFacturaElectronica.selloSAT = datosFacturaElectronica.selloSAT.Insert(200, Environment.NewLine);

                datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT.Insert(200, Environment.NewLine);

                datosFacturaElectronica.selloCFD = datosFacturaElectronica.selloCFD.Insert(200, Environment.NewLine);


                var qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
                var qrCode = qrEncoder.Encode(datosFacturaElectronica.rfcEmisor + "&" + datosFacturaElectronica.rfcReceptor + "&" + datosFacturaElectronica.total + "&" + datosFacturaElectronica.uuid);

                var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);
                //renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
                var stream = new FileStream(@"C:\Impresos\qrcode.png", FileMode.Create);
                renderer.WriteToStream(qrCode.Matrix, System.Drawing.Imaging.ImageFormat.Png, stream);
                stream.Close();

                ImageData imageData = ImageDataFactory.Create(@"C:\Impresos\qrcode.png");

                iText.Layout.Element.Image qcode = new iText.Layout.Element.Image(imageData);
                qcode.ScaleAbsolute(100, 100);
                //
                tableSAT.AddCell(new Cell(8, 2)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(qcode));

                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Número de certificado del emisor")));
                //
                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.numeroCertificado)));

                //
                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Folio fiscal")));
                //
                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.uuid)));

                //
                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("No. Serie del Certificado del SAT")));
                //
                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.numeroCertificadoSAT)));

                //
                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha y hora certificación")));
                //
                tableSAT.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.fechaTimbrado)));

                //
                tableSAT.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Sello digital del CFDI")));
                //
                tableSAT.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fxs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.selloCFD)));
                //

                tableSAT.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Sello del SAT")));
                //
                tableSAT.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fxs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.selloSAT)));
                //
                tableSAT.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Cadena original del complemento de certificación digital del SAT")));
                //
                tableSAT.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fxs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.cadenaOriginalSAT)));
            }


            document.Add(table);
            if (SAT)
            {
                document.Add(tableSAT);
            }

            document.Close();

            if (MessageBox.Show("Desea abrir el PDF generado?", "Atención", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process.Start(Directorio);
                rutaPDF = fileName;
                Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = fileName;
                while (!File.Exists(fileName))
                {

                }
                prc.Start();
            }
        }

        private String AddDecimals(String o)
        {
            String n = "";
            if (o != null)
            {
                if (o.Contains("."))
                {
                    int found = o.IndexOf(".");
                    if (o.Substring(found).Length == 3)
                    {
                        n = o;
                    }
                    else
                    {
                        n = o + "0";
                    }
                }
                else
                {
                    n = o + ".00";
                }
            }
            else
            {
                MessageBox.Show("Error");
            }

            return n;
        }

        private string ConvertirALetra(string numero)
        {
            int longitud;
            int pos;
            string sinpunto;
            string aLetra;
            string tempL;
            string conpunto;
            tempL = "";
            if (numero.Contains('.') == true)
            {
                sinpunto = numero.Remove(numero.IndexOf('.'));
                conpunto = numero.Substring(numero.IndexOf('.') + 1);
                if (conpunto.Length == 1)
                {
                    conpunto = conpunto + "0";
                }
                if (conpunto.Length > 2)
                {
                    conpunto = conpunto.Substring(0, 2);
                }
            }
            else
            {
                sinpunto = numero;
                conpunto = "00";
            }

            longitud = sinpunto.Length;

            aLetra = "";

            for (int i = 0; i < longitud; i++)
            {
                pos = longitud - i;
                switch (sinpunto[i])
                {
                    case '0':
                        tempL = "";
                        break;
                    case '1':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "UN CIENTO";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                switch (sinpunto[i + 1])
                                {
                                    case '1':
                                        tempL = "ONCE";
                                        break;
                                    case '2':
                                        tempL = "DOCE";
                                        break;
                                    case '3':
                                        tempL = "TRECE";
                                        break;
                                    case '4':
                                        tempL = "CATORCE";
                                        break;
                                    case '5':
                                        tempL = "QUINCE";
                                        break;
                                    default:
                                        tempL = "DIEZ";
                                        break;
                                }
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "UN";
                                }
                                else
                                {
                                    switch (sinpunto[i - 1])
                                    {
                                        case '1':
                                            tempL = "";
                                            break;
                                        default:
                                            tempL = "Y UN";
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                    case '2':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "DOS CIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "VEINTE";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "DOS";
                                }
                                else
                                {
                                    switch (sinpunto[i - 1])
                                    {
                                        case '1':
                                            tempL = "";
                                            break;
                                        default:
                                            tempL = "Y DOS";
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                    case '3':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "TRES CIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "TREINTA";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "TRES";
                                }
                                else
                                {
                                    switch (sinpunto[i - 1])
                                    {
                                        case '1':
                                            tempL = "";
                                            break;
                                        default:
                                            tempL = "Y TRES";
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                    case '4':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "CUATRO CIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "CUARENTA";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "CUATRO";
                                }
                                else
                                {
                                    switch (sinpunto[i - 1])
                                    {
                                        case '1':
                                            tempL = "";
                                            break;
                                        default:
                                            tempL = "Y CUATRO";
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                    case '5':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "QUINIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "CINCUENTA";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "CINCO";
                                }
                                else
                                {
                                    switch (sinpunto[i - 1])
                                    {
                                        case '1':
                                            tempL = "";
                                            break;
                                        default:
                                            tempL = "Y CINCO";
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                    case '6':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "SEIS CIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "SESENTA";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "SEIS";
                                }
                                else
                                {
                                    tempL = "Y SEIS";
                                }
                                break;
                        }
                        break;
                    case '7':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "SIETE CIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "SETENTA";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "SIETE";
                                }
                                else
                                {
                                    tempL = "Y SIETE";
                                }
                                break;
                        }
                        break;
                    case '8':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "OCHO CIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "OCHENTA";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "OCHO";
                                }
                                else
                                {
                                    tempL = "Y OCHO";
                                }
                                break;
                        }
                        break;
                    case '9':
                        switch (pos)
                        {
                            case 9:
                            case 6:
                            case 3:
                                tempL = "NOVECIENTOS";
                                break;
                            case 8:
                            case 5:
                            case 2:
                                tempL = "NOVENTA";
                                break;
                            case 7:
                            case 4:
                            case 1:
                                if (i - 1 < 0)
                                {
                                    tempL = "NUEVE";
                                }
                                else
                                {
                                    tempL = "Y NUEVE";
                                }
                                break;
                        }
                        break;
                }

                if (pos == 4)
                {
                    tempL = tempL + " MIL";
                }
                if (tempL != "")
                {
                    aLetra = aLetra + " " + tempL;
                }
            }

            return (aLetra + " PESOS " + conpunto + "/100 M.N.");
        }

        private void btnVerPDF_Click(object sender, RoutedEventArgs e)
        {
            ImprimirPDF(true);
        }
    }
}
