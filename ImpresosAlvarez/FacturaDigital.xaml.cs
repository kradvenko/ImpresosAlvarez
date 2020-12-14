using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Data.Entity;
using System.Data;
using ImpresosAlvarez.mx.facturacfdi.v33;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para FacturaDigital.xaml
    /// </summary>
    public partial class FacturaDigital : Window
    {
        List<Clientes> _clientes;
        Clientes _clienteElegido;
        List<ConceptoFactura> _conceptos;
        List<Contribuyentes> _contribuyentes;

        public float Subtotal;
        public float Total;
        public float RetencionIva;
        public float RetencionIsr;
        public float RetencionCedular;

        X509Certificate certificado;

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

        private String rutaPDF;
        private String rutaXML;

        private String fechaFactura;

        private Factura datosFacturaElectronica;

        String XML;
        String XMLTimbrado;
        String CadenaOriginal;

        int FolioActual;

        private bool timbreValido;

        List<FormasPago> _FormasPago;
        List<UsosCFDI> _UsosCFDI;

        public FacturaDigital()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = _clientes;

                _contribuyentes = dbContext.Contribuyentes.ToList();
                cbContribuyentes.ItemsSource = _contribuyentes;
                cbContribuyentes.SelectedValuePath = "id_contribuyente";
                cbContribuyentes.DisplayMemberPath = "nombre";
                cbContribuyentes.SelectedIndex = 0;
            }

            _conceptos = new List<ConceptoFactura>();
            datosFacturaElectronica = new Factura();

            datosFacturaElectronica.derechosRPP = "0";
            datosFacturaElectronica.otrosDerechos = "0";
            datosFacturaElectronica.adquisicion = "0";
            datosFacturaElectronica.iva = "0";
            datosFacturaElectronica.retencionIva = "0";
            datosFacturaElectronica.retencionIsr = "0";
            datosFacturaElectronica.honorarios = "0";
            datosFacturaElectronica.retencionCedular = "0";
            //datosFacturaElectronica.totalImpuestos = "0";
            datosFacturaElectronica.usoCFDI = "P01";
            datosFacturaElectronica.usoCFDITexto = "P01 Por definir";
            datosFacturaElectronica.formaPago = "99";
            datosFacturaElectronica.formaPagoTexto = "99 Por definir";
            datosFacturaElectronica.fechaDocumento = DateTime.Now;
            datosFacturaElectronica.metodoPago = "PUE";
            datosFacturaElectronica.metodoPagoTexto = "Pago en una sola exhibición";
            datosFacturaElectronica.escritura = "";
            datosFacturaElectronica.predial = "";

            datosFacturaElectronica.numeroCertificadoSAT = "CSD";
            datosFacturaElectronica.numeroCertificado = "NC";
            datosFacturaElectronica.fechaTimbrado = "FECHATIMBRADO";
            datosFacturaElectronica.uuid = "UUID";

            timbreValido = false;

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _FormasPago = dbContext.FormasPago.ToList();
                _UsosCFDI = dbContext.UsosCFDI.ToList();
                cbFormasPago.ItemsSource = _FormasPago;
                cbFormasPago.SelectedValuePath = "Clave";
                cbFormasPago.DisplayMemberPath = "FormaPago";
                cbUsosCFDI.ItemsSource = _UsosCFDI;
                cbUsosCFDI.SelectedValuePath = "Clave";
                cbUsosCFDI.DisplayMemberPath = "Uso";
            }
            
            cbUsosCFDI.SelectedIndex = 0;
            cbFormasPago.SelectedIndex = cbFormasPago.Items.Count - 1;
            
            if (datosFacturaElectronica.metodoPago == "PUE")
            {
                cbMetodoPago.SelectedIndex = 0;
            }
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;
                lblNombre.Content = _clienteElegido.nombre;
                lblRFC.Content = _clienteElegido.rfc;
            }
        }

        public void CalcularTotal()
        {
            Subtotal = 0;
            Total = 0;
            RetencionIva = 0;
            RetencionIsr = 0;
            RetencionCedular = 0;

            foreach (ConceptoFactura item in _conceptos)
            {
                Subtotal += item.Importe;
                /*
                if (cbRetencionIVA.IsChecked.Value)
                {
                    RetencionIva += item.Importe * 0.106667f;
                    RetencionIva = float.Parse(Math.Round(RetencionIva, 2).ToString());
                }
                if (cbRetencionISR.IsChecked.Value)
                {
                    RetencionIsr += item.Importe * 0.100000f;
                    RetencionIsr = float.Parse(Math.Round(RetencionIsr, 2).ToString());
                }
                if (cbRetencionCedular.IsChecked.Value)
                {
                    RetencionCedular += item.Importe * 0.015000f;
                    RetencionCedular = float.Parse(Math.Round(RetencionCedular, 2).ToString());
                }
                */
            }

            float iva = Subtotal * 0.16f;
            iva = float.Parse((Math.Round(iva, 2)).ToString());
            datosFacturaElectronica.iva = iva.ToString();

            //datosFacturaElectronica.totalImpuestos = datosFacturaElectronica.CalcularTotalImpuestos().ToString();

            RetencionIsr = float.Parse(Math.Round(RetencionIsr, 2).ToString());
            RetencionIva = float.Parse(Math.Round(RetencionIva, 2).ToString());
            RetencionCedular = float.Parse(Math.Round(RetencionCedular, 2).ToString());

            iva = float.Parse((Math.Round(iva, 2)).ToString());

            //Total = Subtotal + float.Parse(datosFacturaElectronica.totalImpuestos);

            datosFacturaElectronica.retencionIsr = RetencionIsr.ToString();
            datosFacturaElectronica.retencionIva = RetencionIva.ToString();
            datosFacturaElectronica.retencionCedular = RetencionCedular.ToString();

            Total = Subtotal + iva - RetencionIsr - RetencionIva - RetencionCedular;

            Total = float.Parse(Math.Round(Total, 2).ToString());

            //lblTotalImpuestos.Content = "$ " + datosFacturaElectronica.totalImpuestos;
            lblTotal.Content = "$ " + Total.ToString();
        }

        private void btnAgregarConcepto_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteElegido == null)
            {
                MessageBox.Show("No ha elegido un cliente para facturar.");
                return;
            }
            else
            {
                ControlConceptoFactura concepto = new ControlConceptoFactura(this, "NUEVO", null);
                concepto.ShowDialog();
            }
        }

        private void btnModificarConcepto_Click(object sender, RoutedEventArgs e)
        {
            if (dgConceptos.SelectedItem == null)
            {
                MessageBox.Show("No ha elegido un concepto.");
                return;
            }

            ConceptoFactura concepto = (ConceptoFactura)dgConceptos.SelectedItem;
            ControlConceptoFactura control = new ControlConceptoFactura(this, "MODIFICAR", concepto);
            control.ShowDialog();
        }

        private void btnPrefacturar_Click(object sender, RoutedEventArgs e)
        {
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
                contraseñaLlave = "ALVA7209";
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

            CargarXMLTemplate();
            FacturacionElectronica(false);
            ImprimirPDF(false);
        }

        private void btnFacturar_Click(object sender, RoutedEventArgs e)
        {
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
                contraseñaLlave = "ALVA7209";
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

            CargarXMLTemplate();
            FacturacionElectronica(true);
            ImprimirPDF(true);
            if (timbreValido)
            {
                GuardarFactura();
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void AgregarConcepto(ConceptoFactura concepto)
        {
            _conceptos.Add(concepto);
            dgConceptos.ItemsSource = null;
            dgConceptos.ItemsSource = _conceptos;
            CalcularTotal();
        }

        public void ActualizarConceptos()
        {
            dgConceptos.ItemsSource = null;
            dgConceptos.ItemsSource = _conceptos;
            CalcularTotal();
        }

        private void CargarXMLTemplate()
        {
            XML = File.ReadAllText(@"C:\Impresos\Facturacion\XML_3_3_Template.xml");
        }

        private void FacturacionElectronica(bool bTimbrar)
        {
            try
            {
                string strSello = string.Empty;
                string strPathLlave = rutaLlave;
                string strLlavePwd = contraseñaLlave;

                X509Certificate certificado = new X509Certificate(rutaCertificado);
                String certificadoStr = Convert.ToBase64String(certificado.GetRawCertData());
                String numCertificado = Encoding.ASCII.GetString(HexToString(certificado.GetSerialNumberString()));

                XML = XML.Replace("NoCertificado=\"\"", "NoCertificado=\"" + numCertificado + "\"");
                XML = XML.Replace("Certificado=\"\"", "Certificado=\"" + certificadoStr + "\"");

                File.WriteAllText(@"C:\Impresos\Facturacion\XML_3_3.xml", XML);

                GenerarXML();

                System.Security.SecureString passwordSeguro = new System.Security.SecureString();
                passwordSeguro.Clear();
                foreach (char c in strLlavePwd.ToCharArray())
                    passwordSeguro.AppendChar(c);
                byte[] llavePrivadaBytes = File.ReadAllBytes(strPathLlave);
                RSACryptoServiceProvider rsa = opensslkey.DecodeEncryptedPrivateKeyInfo(llavePrivadaBytes, passwordSeguro);
                SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();
                byte[] bytesFirmados = rsa.SignData(Encoding.UTF8.GetBytes(GenerarCadenaOriginal()), hasher);
                CadenaOriginal = GenerarCadenaOriginal();
                strSello = Convert.ToBase64String(bytesFirmados);

                XML = XML.Replace("Sello=\"\"", "Sello=\"" + strSello + "\"");

                File.WriteAllText(@"C:\Impresos\XML_3_3.xml", XML);

                Directory.CreateDirectory(@"C:\Impresos\Facturas\" + _clienteElegido.nombre);
                string fileName = @"C:\Impresos\Facturas\" + _clienteElegido.nombre + @"\pre_" + cbContribuyentes.Text + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";

                File.WriteAllText(fileName, XML);

            }
            catch (Exception exc)
            {
                MessageBox.Show("Error: " + exc.Message);
            }

            if (bTimbrar)
            {
                Timbrar();
            }
        }

        private void Timbrar()
        {
            string usuario = usuarioFacturacion;
            string pass = contraseñaFacturacion;
            string cfdixml = XML;

            WSForcogsaService serv = new WSForcogsaService();
            wsTimbradoResponse respTimbre = new wsTimbradoResponse();

            try
            {
                // obtiene el token
                wsAutenticarResponse resp = serv.Autenticar(usuario, pass);

                // seccion timbre
                respTimbre = serv.Timbrar(cfdixml, resp.token);
                //Console.Write(respTimbre.cfdi);
                if (respTimbre.cfdi != null && respTimbre.cfdi.Trim().Length > 0)
                {
                    XMLTimbrado = respTimbre.cfdi;
                    string fileName = @"C:\Impresos\Facturas\" + _clienteElegido.nombre + @"\fac_" + cbContribuyentes.Text + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";
                    File.WriteAllText(fileName, XMLTimbrado);
                    rutaXML = fileName;
                    GenerarComplemento();
                    timbreValido = true;
                }
                else
                {
                    XMLTimbrado = respTimbre.mensaje;
                    MessageBox.Show(XMLTimbrado);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ha ocurrido un error inesperado.");
            }
            
        }

        public static byte[] HexToString(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        private void GenerarXML()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(XML);

            XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);
            nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
            /*
            if (cbRetencionCedular.IsChecked.Value)
            {
                nms.AddNamespace("implocal", "http://www.sat.gob.mx/implocal");
            }
            */
            XmlAttribute xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Serie", nms);
            xAttrib.Value = "-";
            datosFacturaElectronica.serie = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Folio", nms);
            xAttrib.Value = FolioActual.ToString();
            datosFacturaElectronica.folio = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Fecha", nms);
            fechaFactura = DateTime.UtcNow.AddHours(-7).ToString("s");
            xAttrib.Value = fechaFactura;
            datosFacturaElectronica.fechaExpedicion = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@FormaPago", nms);
            xAttrib.Value = datosFacturaElectronica.formaPago;
            datosFacturaElectronica.formaPago = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@MetodoPago", nms);
            xAttrib.Value = datosFacturaElectronica.metodoPago;
            datosFacturaElectronica.metodoPago = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@SubTotal", nms);
            xAttrib.Value = AddDecimals(Subtotal.ToString());
            datosFacturaElectronica.subTotal = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Total", nms);
            xAttrib.Value = AddDecimals(Total.ToString());
            datosFacturaElectronica.total = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@LugarExpedicion", nms);
            xAttrib.Value = datosFacturaElectronica.domicilioEmisorCodigoPostal;
            datosFacturaElectronica.lugarExpedicion = "Tepic, Nay.";

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@NoCertificado", nms);
            datosFacturaElectronica.numeroCertificado = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Rfc", nms);
            xAttrib.Value = rfcEmisor;
            datosFacturaElectronica.rfcEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Nombre", nms);
            xAttrib.Value = nombreEmisor;
            datosFacturaElectronica.nombreEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@RegimenFiscal", nms);
            xAttrib.Value = "612";

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@Rfc", nms);
            xAttrib.Value = _clienteElegido.rfc.Replace("-", "");
            datosFacturaElectronica.rfcReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@Nombre", nms);
            xAttrib.Value = _clienteElegido.nombre;
            datosFacturaElectronica.nombreReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@UsoCFDI", nms);
            xAttrib.Value = datosFacturaElectronica.usoCFDI;

            datosFacturaElectronica.domicilioReceptorCalle = _clienteElegido.domicilio;
            datosFacturaElectronica.domicilioReceptorNumeroExterior = _clienteElegido.numero_exterior;
            datosFacturaElectronica.domicilioReceptorColonia = _clienteElegido.colonia;
            datosFacturaElectronica.domicilioReceptorEstado = _clienteElegido.estado;
            datosFacturaElectronica.domicilioReceptorMunicipio = _clienteElegido.ciudad;
            datosFacturaElectronica.domicilioReceptorCodigoPostal = _clienteElegido.codigo_postal;

            XmlNode xConceptos = xDoc.SelectSingleNode("//cfdi:Conceptos", nms);
            XmlNode xImpuestosNodo = xDoc.SelectSingleNode("//cfdi:Impuestos", nms);

            float impuesto = 0;
            float impuestoTotal = 0;

            datosFacturaElectronica.conceptos = new List<ConceptoFactura>();

            XmlAttribute xa;

            foreach (ConceptoFactura item in _conceptos)
            {
                if (item != null)
                {
                    ConceptoFactura cConcepto = new ConceptoFactura();

                    XmlNode xConcepto = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Concepto", "http://www.sat.gob.mx/cfd/3");

                    xa = xDoc.CreateAttribute("ClaveProdServ");
                    xa.Value = item.Clave;
                    xConcepto.Attributes.Append(xa);
                    cConcepto.descripcionConcepto = xa.Value;

                    xa = xDoc.CreateAttribute("ClaveUnidad");
                    xa.Value = item.claveUnidad;
                    xConcepto.Attributes.Append(xa);
                    cConcepto.claveUnidad = xa.Value;

                    xa = xDoc.CreateAttribute("NoIdentificacion");
                    xa.Value = "-";
                    xConcepto.Attributes.Append(xa);
                    cConcepto.noIdentificacion = xa.Value;

                    xa = xDoc.CreateAttribute("Cantidad");
                    xa.Value = item.Cantidad.ToString();
                    xConcepto.Attributes.Append(xa);
                    cConcepto.cantidad = xa.Value;

                    xa = xDoc.CreateAttribute("Descripcion");
                    xa.Value = item.Descripcion;
                    xConcepto.Attributes.Append(xa);
                    cConcepto.claveProductoServicio = xa.Value;
                    cConcepto.descripcionProducto = item.Descripcion;

                    xa = xDoc.CreateAttribute("ValorUnitario");
                    xa.Value = item.PrecioUnitario.ToString();
                    xConcepto.Attributes.Append(xa);
                    cConcepto.valorUnitario = xa.Value;

                    xa = xDoc.CreateAttribute("Importe");
                    xa.Value = AddDecimals(item.Importe.ToString());
                    xConcepto.Attributes.Append(xa);
                    cConcepto.importe = xa.Value;
                    /*
                    xa = xDoc.CreateAttribute("Descuento");
                    xa.Value = "0.00";
                    xConcepto.Attributes.Append(xa);
                    cConcepto.descuento = xa.Value;
                    */
                    /*
                    datosFacturaElectronica.conceptos[conceptosFacturaIndex] = cConcepto;
                    conceptosFacturaIndex++;
                    */
                    datosFacturaElectronica.conceptos.Add(cConcepto);

                    impuesto = item.Cantidad * item.PrecioUnitario;
                    impuesto = impuesto * 0.16f;
                    impuesto = float.Parse((Math.Round(impuesto, 2)).ToString());
                    impuestoTotal = impuestoTotal + impuesto;

                    XmlNode xImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Impuestos", "http://www.sat.gob.mx/cfd/3");

                    XmlNode xTraslados = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslados", "http://www.sat.gob.mx/cfd/3");

                    XmlNode xTraslado = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslado", "http://www.sat.gob.mx/cfd/3");

                    XmlNode xRetenciones = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/3");

                    xa = xDoc.CreateAttribute("Base");
                    xa.Value = item.Importe.ToString();
                    xTraslado.Attributes.Append(xa);

                    xa = xDoc.CreateAttribute("Impuesto");
                    xa.Value = "002";
                    xTraslado.Attributes.Append(xa);

                    xa = xDoc.CreateAttribute("TipoFactor");
                    xa.Value = "Tasa";
                    xTraslado.Attributes.Append(xa);

                    xa = xDoc.CreateAttribute("TasaOCuota");
                    xa.Value = "0.160000";
                    xTraslado.Attributes.Append(xa);

                    xa = xDoc.CreateAttribute("Importe");
                    xa.Value = AddDecimals(impuesto.ToString());
                    xTraslado.Attributes.Append(xa);

                    xTraslados.AppendChild(xTraslado);
                    xImpuestos.AppendChild(xTraslados);
                    
                    xConcepto.AppendChild(xImpuestos);
                    xConceptos.AppendChild(xConcepto);
                }
                    
            }
            
            XmlNode xTrasladosImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslados", "http://www.sat.gob.mx/cfd/3");

            XmlNode xTrasladoImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslado", "http://www.sat.gob.mx/cfd/3");

            xa = xDoc.CreateAttribute("Impuesto");
            xa.Value = "002";
            xTrasladoImpuestos.Attributes.Append(xa);
            xa = xDoc.CreateAttribute("TipoFactor");
            xa.Value = "Tasa";
            xTrasladoImpuestos.Attributes.Append(xa);
            xa = xDoc.CreateAttribute("TasaOCuota");
            xa.Value = "0.160000";
            xTrasladoImpuestos.Attributes.Append(xa);
            xa = xDoc.CreateAttribute("Importe");
            impuestoTotal = float.Parse((Math.Round(impuestoTotal, 2)).ToString());
            xa.Value = AddDecimals(impuestoTotal.ToString());
            xTrasladoImpuestos.Attributes.Append(xa);

            xTrasladosImpuestos.AppendChild(xTrasladoImpuestos);

            xImpuestosNodo.AppendChild(xTrasladosImpuestos);

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Impuestos//@TotalImpuestosTrasladados", nms);
            xAttrib.Value = AddDecimals(impuestoTotal.ToString());

            XML = xDoc.InnerXml;

            File.WriteAllText(@"C:\Impresos\Facturacion\XML_3_3.xml", XML);
        }

        private String GenerarCadenaOriginal()
        {
            String cadenaOriginal = "";

            try
            {
                //Cargar el XML
                StreamReader reader = new StreamReader(@"C:\Impresos\Facturacion\XML_3_3.xml");
                XPathDocument myXPathDoc = new XPathDocument(reader);

                //Cargando el XSLT
                XslCompiledTransform myXslTrans = new XslCompiledTransform();
                myXslTrans.Load(@"C:\Impresos\Facturacion\XLS_3_3.xslt");
                //myXslTrans.Load(@"C:\Impresos\cadenaoriginal_3_2.xslt");


                StringWriter str = new StringWriter();
                XmlTextWriter myWriter = new XmlTextWriter(str);

                //Aplicando transformacion
                myXslTrans.Transform(myXPathDoc, null, myWriter);

                //Resultado
                cadenaOriginal = str.ToString();

            }
            catch (Exception exc)
            {
                cadenaOriginal = exc.Message;
            }

            return cadenaOriginal;
        }

        private String AddDecimals(String o)
        {
            String n = "";

            if (o.Contains("."))
            {
                n = o;
            }
            else
            {
                n = o + ".00";
            }

            return n;
        }

        void ImprimirPDF(Boolean SAT)
        {

            System.IO.Directory.CreateDirectory(@"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor);
            String Directorio = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor;
            string fileName;
            if (SAT)
            {
                fileName = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor + "\\fac_" + cbContribuyentes.Text + "_" + FolioActual + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
            }
            else
            {
                fileName = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor + "\\pre_" + cbContribuyentes.Text + "_" + FolioActual + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
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
            pdfImg.SetHeight(90);

            table.AddCell(new Cell(4, 4)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
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
                .Add(new Paragraph("Serie y folio: " + FolioActual.ToString())));

            //Renglon
            /*
            pdfImg.SetHeight(150);
            pdfImg.SetFixedPosition(50, 50);
            */
            pdfImg.SetHeight(120);

            
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
                .Add(new Paragraph("Fecha: " + DateTime.Today.ToShortDateString())));

            //Renglon
            
            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.domicilioEmisorColonia + " CP " + datosFacturaElectronica.domicilioEmisorCodigoPostal + " " + datosFacturaElectronica.domicilioEmisorMunicipio + " " + datosFacturaElectronica.domicilioEmisorEstado + "\nTel 212-66-46 Fax 216-64-22")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Regimen fiscal: Persona física con actividad empresarial y profesional")));

            /*
            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("No. serie CSD")));
            */

            //Renglon 4
            /*
            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.domicilioEmisorColonia)));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.numeroCertificadoSAT)));

            //Renglon 5
            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.domicilioEmisorMunicipio)));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Folio fiscal")));

            //Renglon 6
            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.domicilioEmisorEstado + "C.P. " + datosFacturaElectronica.domicilioEmisorCodigoPostal)));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.uuid)));

            //Renglon 7
            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("PERSONAS FISICAS CON ACTIVIDADES EMPRESARIALES Y PROFESIONALES")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Fecha y hora de certificación")));

            //Renglon 8
            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Teléfonos: ")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.fechaTimbrado)));

            //Renglon 9
            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));
            */
            //Renglon 9
            /*
            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Tipo de comprobante: I")));
            */
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
                .Add(new Paragraph(_clienteElegido.rfc)));

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
                .Add(new Paragraph(_clienteElegido.nombre)));

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
                .Add(new Paragraph(_clienteElegido.domicilio + " " + _clienteElegido.numero_exterior)));

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
                .Add(new Paragraph(_clienteElegido.colonia)));

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
                .Add(new Paragraph(_clienteElegido.ciudad + ", " + _clienteElegido.estado)));

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
                .Add(new Paragraph(_clienteElegido.codigo_postal)));

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
            /*
            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Escritura")));

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.escritura)));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Predial")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(datosFacturaElectronica.predial)));
            */
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

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("Clave Unidad")));

            table.AddCell(new Cell(1, 4)
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

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.Unidad)));

                table.AddCell(new Cell(1, 4)
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
                .Add(new Paragraph("$ " + AddDecimals(Subtotal.ToString()))));

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

            if (datosFacturaElectronica.retencionIva != "0")
            {
                table.AddCell(new Cell(1, 9)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Retención IVA: ")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.retencionIva))));
            }

            if (datosFacturaElectronica.retencionIsr != "0")
            {
                table.AddCell(new Cell(1, 9)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Retención ISR: ")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.retencionIsr))));
            }

            if (datosFacturaElectronica.retencionCedular != "0")
            {
                table.AddCell(new Cell(1, 9)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Retención Cedular: ")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.retencionCedular))));
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

            if (datosFacturaElectronica.derechosRPP != "0")
            {
                table.AddCell(new Cell(1, 7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Cobro derechos RPP: ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.derechosRPP))));
            }

            if (datosFacturaElectronica.otrosDerechos != "0")
            {
                table.AddCell(new Cell(1, 7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Otros derechos: ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.otrosDerechos))));
            }

            if (datosFacturaElectronica.derechosRPP != "0")
            {
                table.AddCell(new Cell(1, 7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Impuesto por adquisición: ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.adquisicion))));
            }

            if (datosFacturaElectronica.honorarios != "0")
            {
                table.AddCell(new Cell(1, 7)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Honorarios: ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(datosFacturaElectronica.honorarios))));
            }

            table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .Add(new Paragraph("Este documento es una representación impresa de un CFDI")));

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

                if (!timbreValido)
                {
                    return;
                }

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
                /*
                rutaPDF = fileName;
                Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = fileName;
                prc.Start();
                */
            }

        }

        private void GenerarComplemento()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(XMLTimbrado);

            XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);
            nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
            nms.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

            XmlAttribute xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@Version", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "||" + xAttrib.Value + "|";

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@UUID", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "|" + xAttrib.Value + "|";
            datosFacturaElectronica.uuid = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@FechaTimbrado", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "|" + xAttrib.Value + "|";
            datosFacturaElectronica.fechaTimbrado = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@RfcProvCertif", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "|" + xAttrib.Value + "|";
            /*
            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@Leyenda", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "|" + xAttrib.Value + "|";
            */
            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@SelloCFD", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "|" + xAttrib.Value + "|";
            datosFacturaElectronica.selloCFD = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@NoCertificadoSAT", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "|" + xAttrib.Value + "|";
            datosFacturaElectronica.numeroCertificadoSAT = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//tfd:TimbreFiscalDigital//@SelloSAT", nms);
            datosFacturaElectronica.cadenaOriginalSAT = datosFacturaElectronica.cadenaOriginalSAT + "|" + xAttrib.Value + "||";
            datosFacturaElectronica.selloSAT = xAttrib.Value;
        }

        private void btnImpuestos_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteElegido == null)
            {
                MessageBox.Show("No ha elegido un cliente.");
                return;
            }
            /*ControlImpuestos impuestos = new ControlImpuestos(this, datosFacturaElectronica);
            impuestos.ShowDialog();*/
        }

        private void btnEliminarConcepto_Click(object sender, RoutedEventArgs e)
        {
            if (dgConceptos.SelectedItem != null)
            {
                _conceptos.Remove((ConceptoFactura)dgConceptos.SelectedItem);
                dgConceptos.ItemsSource = null;
                dgConceptos.ItemsSource = _conceptos;
                CalcularTotal();
            }
        }

        private void GuardarFactura()
        {
            /*
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                using (DbContextTransaction transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        Facturas factura = new Facturas();
                        DetalleFacturas detalle = new DetalleFacturas();

                        factura.IdCliente = _clienteElegido.IdCliente;
                        factura.IdContribuyente = ((Contribuyentes)cbContribuyentes.SelectedItem).IdContribuyente;
                        factura.Subtotal = Subtotal;
                        factura.Total = Total;
                        factura.Estado = "ACTIVO";
                        factura.Fecha = DateTime.Now.ToString();
                        factura.Numero = FolioActual.ToString();
                        factura.XML = XML;
                        factura.Escritura = datosFacturaElectronica.escritura;
                        factura.Predial = datosFacturaElectronica.predial;
                        factura.SelloCFDI = datosFacturaElectronica.selloCFD;
                        factura.SelloSAT = datosFacturaElectronica.selloSAT;
                        factura.CadenaOriginal = datosFacturaElectronica.cadenaOriginalSAT;
                        factura.FormaPago = datosFacturaElectronica.formaPago;
                        factura.UsoCFDI = datosFacturaElectronica.usoCFDI;
                        factura.NumeroDocumento = datosFacturaElectronica.numeroDocumento;
                        factura.FechaDocumento = datosFacturaElectronica.fechaDocumento;
                        factura.MontoDocumento = datosFacturaElectronica.montoDocumento;
                        factura.TotalDocumento = datosFacturaElectronica.totalDocumento;
                        factura.ImpuestosRPP = double.Parse(datosFacturaElectronica.derechosRPP);
                        factura.ImpuestosOtros = double.Parse(datosFacturaElectronica.otrosDerechos);
                        factura.ImpuestosAdquisiciones = double.Parse(datosFacturaElectronica.adquisicion);
                        factura.ImpuestosIVA = double.Parse(datosFacturaElectronica.iva);
                        factura.ImpuestosIVARetencion = double.Parse(datosFacturaElectronica.retencionIva);
                        factura.ImpuestosISRRetencion = double.Parse(datosFacturaElectronica.retencionIsr);
                        factura.Honorarios = double.Parse(datosFacturaElectronica.honorarios);
                        factura.ImpuestosRetencionCedular = double.Parse(datosFacturaElectronica.retencionCedular);

                        dbContext.Facturas.Add(factura);

                        dbContext.SaveChanges();

                        foreach (ConceptoFactura item in _conceptos)
                        {
                            detalle = new DetalleFacturas();

                            detalle.IdFactura = factura.IdFactura;
                            detalle.Cantidad = item.Cantidad;
                            detalle.Descripcion = item.Descripcion;
                            detalle.Importe = item.Importe;
                            detalle.PrecioUnitario = item.PrecioUnitario;
                            detalle.ClaveServicio = item.Clave;
                            detalle.Unidad = item.Unidad;

                            dbContext.DetalleFacturas.Add(detalle);

                            dbContext.SaveChanges();
                        }

                        int IdContribuyente = int.Parse(cbContribuyentes.SelectedValue.ToString());
                        FoliosFacturas Fol = dbContext.FoliosFacturas.Where(F => F.IdContribuyente == IdContribuyente).FirstOrDefault();
                        Fol.Folio = Fol.Folio + 1;

                        dbContext.SaveChanges();

                        transaction.Commit();

                        this.Close();
                    }
                    catch (Exception exc)
                    {
                        transaction.Rollback();
                        MessageBox.Show(exc.Message);
                    }
                }
            }
            */
        }

        private void btnInfoFactura_Click(object sender, RoutedEventArgs e)
        {
            /*
            InformacionFactura informacion = new InformacionFactura(this, datosFacturaElectronica);
            informacion.ShowDialog();*/
        }

        private void btnComplementoEnajenacion_Click(object sender, RoutedEventArgs e)
        {
            /*ComplementoEnajenacion complemento = new ComplementoEnajenacion(this, datosFacturaElectronica);
            complemento.ShowDialog();*/
        }

        private void cbContribuyentes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbContribuyentes.SelectedItem == null)
            {
                return;
            }
            int IdContribuyente = int.Parse(cbContribuyentes.SelectedValue.ToString());
            Contribuyentes c = (Contribuyentes)cbContribuyentes.SelectedItem;
            FolioActual = int.Parse(c.numero_factura);
            lblFolio.Content = FolioActual.ToString();
            /*
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                FolioActual = int.Parse(dbContext.FoliosFacturas.Where(F => F.IdContribuyente == IdContribuyente).First().Folio.ToString());
                lblFolio.Content = FolioActual.ToString();
            }
            */
        }

        private void cbRetencionIVA_Click(object sender, RoutedEventArgs e)
        {
            CalcularTotal();
        }

        private void cbRetencionISR_Click(object sender, RoutedEventArgs e)
        {
            CalcularTotal();
        }

        private void cbRetencionCedular_Click(object sender, RoutedEventArgs e)
        {
            CalcularTotal();
        }
    }
}
