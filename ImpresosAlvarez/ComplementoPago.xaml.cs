using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using ImpresosAlvarez.mx.facturacfdi.v33;
//using ImpresosAlvarez.mx.facturacfdi.dev33;
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
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para ComplementoPago.xaml
    /// </summary>
    public partial class ComplementoPago : Window
    {
        List<Clientes> _clientes;
        Clientes _clienteElegido;
        List<Contribuyentes> _contribuyentes;
        List<ComplementoPagoData> _complementos;
        List<Parcialidades> _parcialidades;

        public float Subtotal;
        public float Total;

        List<FormasPago> _FormasPago;

        List<Facturas> _facturas;

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

        private Factura datosFacturaElectronica;

        String XML;
        String XMLTimbrado;
        String CadenaOriginal;

        private int idParcialidad;

        int FolioActual;

        private String fechaFactura;

        private bool timbreValido;

        private String rutaPDF;
        private String rutaXML;

        float IvaDRTotal = 0;
        float ISRTotal = 0;
        
        public ComplementoPago()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            datosFacturaElectronica = new Factura();
            datosFacturaElectronica.pagos = new List<ComplementoPagoData>();
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

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _FormasPago = dbContext.FormasPago.ToList();
                cbFormasPago.ItemsSource = _FormasPago;
                cbFormasPago.SelectedValuePath = "Clave";
                cbFormasPago.DisplayMemberPath = "FormaPago";
            }

            cbMetodoPago.SelectedIndex = 0;
            dtpFechaPagoComplemento.SelectedDate = DateTime.Now;
            cbFormasPago.SelectedIndex = 0;

            _complementos = new List<ComplementoPagoData>();

            
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;
                ObtenerFacturas();
            }
        }

        public void ObtenerFacturas()
        {
            if (tbClientes.SelectedItem != null)
            {
                Clientes c = (Clientes)tbClientes.SelectedItem;
                Contribuyentes cb = (Contribuyentes)cbContribuyentes.SelectedItem;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    /*
                    _facturas = dbContext.Facturas
                        .Where(F => F.id_cliente == c.id_cliente && F.estado == "ACTIVO" && F.id_contribuyente == cb.id_contribuyente)
                        .ToList();
                    */

                    List<FacturaComplemento> facts = dbContext.Facturas.Join(
                            dbContext.FacturaDigital,
                            f => f.id_factura,
                            fd => fd.id_factura,
                            (f, fd) => new FacturaComplemento {
                                IdFactura = f.id_factura,
                                IdCliente = f.id_cliente,
                                IdContribuyente = f.id_contribuyente,
                                Numero = f.numero,
                                Estado = f.estado,
                                XML = fd.xml,
                                SubTotal = f.subtotal.ToString(),
                                Total = f.total.ToString(),
                                Pagada = f.pagada
                            }
                        )
                        //.Where(F => F.IdCliente == c.id_cliente && F.Estado == "ACTIVO" && F.IdContribuyente == cb.id_contribuyente && F.Pagada == "NO")
                        .Where(F => F.IdCliente == c.id_cliente && F.Estado == "ACTIVO" && F.IdContribuyente == cb.id_contribuyente)
                        .ToList();

                    dgFacturas.ItemsSource = facts;
                }
            }
        }

        private void cbContribuyentes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ObtenerFacturas();
        }

        private void btnCrearComplemento_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Desea generar el complemento?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            foreach (ComplementoPagoData item in dgComplemento.Items)
            {
                if (item.Pagado == "0")
                {
                    MessageBox.Show("No ha ingresado un monto mayor a 0 para el pago.");
                    return;
                }
            }

            CargarXMLTemplate();
            InsertarParcialidad();

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
                regimen = "Regimen simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC";
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
                regimen = "Regimen simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC";
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
                regimen = "Regimen simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 619 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }
            /*
            rutaCertificado = @"C:\Impresos\Pruebas\Certificado.cer";
            rutaLlave = @"C:\Impresos\Pruebas\Llave.key";
            contraseñaLlave = "12345678a";
            nombreEmisor = "XOCHILT CASAS CHAVEZ";
            rfcEmisor = "CACX7605101P8";
            serie = "-";
            usuarioFacturacion = "pruebasWS";
            contraseñaFacturacion = "pruebasWS";
            curp = "-";
            regimen = "Regimen simplificado de confianza";

            datosFacturaElectronica.domicilioEmisorCalle = "";
            datosFacturaElectronica.domicilioEmisorColonia = "";
            datosFacturaElectronica.domicilioEmisorMunicipio = " ";
            datosFacturaElectronica.domicilioEmisorEstado = "";
            datosFacturaElectronica.domicilioEmisorCodigoPostal = "44970";
            */
            FacturacionElectronica(true);
            ImprimirPDF(true);
            if (timbreValido)
            {
                ActualizarParcialidad();
                Thread email = new Thread(EnviarPorCorreo);
                email.Start();
                this.Close();
                //EnviarPorCorreo();
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
        private void CargarXMLTemplate()
        {
            XML = File.ReadAllText(@"C:\Impresos\Complemento_Template.xml");
        }

        private void CargarXMLTemplate20()
        {
            XML = File.ReadAllText(@"C:\Impresos\Complemento_Template_2_0.xml");
        }
        private String GenerarCadenaOriginal()
        {
            String cadenaOriginal = "";

            try
            {
                //Cargar el XML
                StreamReader reader = new StreamReader(@"C:\Impresos\COMP_1_0.xml");
                XPathDocument myXPathDoc = new XPathDocument(reader);

                //Cargando el XSLT
                XslCompiledTransform myXslTrans = new XslCompiledTransform();
                myXslTrans.Load(@"C:\Impresos\XLS_3_3.xslt");
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

        private String GenerarCadenaOriginal20()
        {
            String cadenaOriginal = "";

            try
            {
                //Cargar el XML
                StreamReader reader = new StreamReader(@"C:\Impresos\COMP_2_0.xml");
                XPathDocument myXPathDoc = new XPathDocument(reader);

                //Cargando el XSLT
                XslCompiledTransform myXslTrans = new XslCompiledTransform();
                myXslTrans.Load(@"C:\Impresos\XLS_4_0.xslt");
                //myXslTrans.Load(@"C:\Impresos\cadenaoriginal_3_2.xslt");


                StringWriter str = new StringWriter();
                XmlTextWriter myWriter = new XmlTextWriter(str);

                //Aplicando transformacion
                myXslTrans.Transform(myXPathDoc, null, myWriter);

                //Resultado
                cadenaOriginal = str.ToString();
                cadenaOriginal = cadenaOriginal.Replace("\n", "").Replace("\r", "");

            }
            catch (Exception exc)
            {
                cadenaOriginal = exc.Message;
            }

            return cadenaOriginal;
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

                datosFacturaElectronica.numeroCertificado = numCertificado;

                File.WriteAllText(@"C:\Impresos\COMP_1_0.xml", XML);

                CrearComplemento(); //!!

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

                Directory.CreateDirectory(@"C:\Impresos\Complementos\" + _clienteElegido.nombre);
                string fileName = @"C:\Impresos\Complementos\" + _clienteElegido.nombre + @"\c_pre_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";

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
        private void FacturacionElectronica40(bool bTimbrar)
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

                datosFacturaElectronica.numeroCertificado = numCertificado;

                File.WriteAllText(@"C:\Impresos\COMP_2_0.xml", XML);

                CrearComplemento20(); //!!

                System.Security.SecureString passwordSeguro = new System.Security.SecureString();
                passwordSeguro.Clear();
                foreach (char c in strLlavePwd.ToCharArray())
                    passwordSeguro.AppendChar(c);
                byte[] llavePrivadaBytes = File.ReadAllBytes(strPathLlave);
                RSACryptoServiceProvider rsa = opensslkey.DecodeEncryptedPrivateKeyInfo(llavePrivadaBytes, passwordSeguro);
                SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();
                byte[] bytesFirmados = rsa.SignData(Encoding.UTF8.GetBytes(GenerarCadenaOriginal20()), hasher);
                CadenaOriginal = GenerarCadenaOriginal20();
                strSello = Convert.ToBase64String(bytesFirmados);

                XML = XML.Replace("Sello=\"\"", "Sello=\"" + strSello + "\"");

                File.WriteAllText(@"C:\Impresos\XML_4_0.xml", XML);

                Directory.CreateDirectory(@"C:\Impresos\Complementos\" + datosFacturaElectronica.nombreReceptor);
                string fileName = @"C:\Impresos\Complementos\" + datosFacturaElectronica.nombreReceptor + @"\c_pre_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";

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
                    string fileName = @"C:\Impresos\Complementos\" + datosFacturaElectronica.nombreReceptor + @"\fac_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";
                    File.WriteAllText(fileName, XMLTimbrado);
                    rutaXML = fileName;
                    XML = XMLTimbrado;
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
        private void CrearComplemento()
        {
            CalcularTotal();

            XmlDocument xCom = new XmlDocument();
            xCom.LoadXml(XML);

            XmlNamespaceManager comNms = new XmlNamespaceManager(xCom.NameTable);
            if (XML.Contains("cfd/3"))
            {
                comNms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                comNms.AddNamespace("pago10", "http://www.sat.gob.mx/Pagos");
            }
            else
            {
                comNms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
                comNms.AddNamespace("pago20", "http://www.sat.gob.mx/Pagos20");

            }
            

            XmlAttribute xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@Serie", comNms);
            xAttrib.Value = "-";
            datosFacturaElectronica.serie = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@Folio", comNms);
            if (idParcialidad > 0)
            {
                xAttrib.Value = idParcialidad.ToString();
            }
            else
            {
                xAttrib.Value = "-";
            }
            datosFacturaElectronica.folio = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@Fecha", comNms);
            fechaFactura = DateTime.UtcNow.AddHours(-7).ToString("s");
            //fechaFactura = dtpFechaComplemento.Value.ToUniversalTime().AddHours(-7).ToString("s");
            xAttrib.Value = fechaFactura;
            datosFacturaElectronica.fechaExpedicion = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@LugarExpedicion", comNms);
            xAttrib.Value = "63080";
            datosFacturaElectronica.domicilioEmisorCodigoPostal = xAttrib.Value;
            datosFacturaElectronica.lugarExpedicion = "Tepic, Nay.";

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Emisor//@Rfc", comNms);
            xAttrib.Value = rfcEmisor;
            datosFacturaElectronica.rfcEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Emisor//@Nombre", comNms);
            xAttrib.Value = nombreEmisor;
            datosFacturaElectronica.nombreEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Emisor//@RegimenFiscal", comNms);
            xAttrib.Value = "626";

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@Rfc", comNms);
            xAttrib.Value = _clienteElegido.rfc.Replace("-", ""); ;
            datosFacturaElectronica.rfcReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@Nombre", comNms);
            xAttrib.Value = _clienteElegido.nombre;
            datosFacturaElectronica.nombreReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@UsoCFDI", comNms);
            datosFacturaElectronica.usoCFDI = "P01";
            xAttrib.Value = datosFacturaElectronica.usoCFDI;

            XmlNode xPagos = xCom.SelectSingleNode("//cfdi:Comprobante//pago10:Pagos", comNms);

            XmlAttribute xa;

            datosFacturaElectronica.conceptos = new List<ConceptoFactura>();
            int conceptosFacturaIndex = 0;

            XmlNode xPago = xCom.CreateNode(XmlNodeType.Element, "pago10", "Pago", "http://www.sat.gob.mx/Pagos");

            xa = xCom.CreateAttribute("FechaPago");
            string fechaFacturaP = dtpFechaPagoComplemento.SelectedDate.Value.ToUniversalTime().AddHours(-7).ToString("s");
            xa.Value = fechaFacturaP;
            xPago.Attributes.Append(xa);

            xa = xCom.CreateAttribute("FormaDePagoP");
            datosFacturaElectronica.formaPago = cbFormasPago.SelectedValue.ToString();
            datosFacturaElectronica.formaPagoTexto = cbFormasPago.Text;
            xa.Value = datosFacturaElectronica.formaPago;
            xPago.Attributes.Append(xa);

            xa = xCom.CreateAttribute("MonedaP");
            xa.Value = "MXN";
            xPago.Attributes.Append(xa);

            xa = xCom.CreateAttribute("Monto");
            xa.Value = Math.Round(Total, 2).ToString();
            xPago.Attributes.Append(xa);

            foreach (ComplementoPagoData item in dgComplemento.Items)
            {
                XmlNode xDocto = xCom.CreateNode(XmlNodeType.Element, "pago10", "DoctoRelacionado", "http://www.sat.gob.mx/Pagos");

                item.FechaPago = xa.Value;
                item.FormaPago = xa.Value;
                item.MonedaPago = xa.Value;
                item.MontoPago = xa.Value;

                xa = xCom.CreateAttribute("IdDocumento");
                xa.Value = item.UUID;
                xDocto.Attributes.Append(xa);
                item.IdDocto = xa.Value;

                xa = xCom.CreateAttribute("Serie");
                xa.Value = item.Serie;
                xDocto.Attributes.Append(xa);
                item.Serie = xa.Value;

                xa = xCom.CreateAttribute("Folio");
                xa.Value = item.Folio;
                xDocto.Attributes.Append(xa);
                item.Folio = xa.Value;

                xa = xCom.CreateAttribute("MonedaDR");
                xa.Value = "MXN";
                xDocto.Attributes.Append(xa);

                xa = xCom.CreateAttribute("MetodoDePagoDR");
                if (cbMetodoPago.SelectedIndex == 0)
                {
                    datosFacturaElectronica.metodoPago = "PUE";
                }
                else
                {
                    datosFacturaElectronica.metodoPago = "PPD";
                }
                datosFacturaElectronica.metodoPagoTexto = cbMetodoPago.Text;
                xa.Value = datosFacturaElectronica.metodoPago;
                xDocto.Attributes.Append(xa);
                item.MetodoPagoDR = xa.Value;

                xa = xCom.CreateAttribute("NumParcialidad");
                xa.Value = item.Parcialidad;
                xDocto.Attributes.Append(xa);
                item.Parcialidad = xa.Value;

                xa = xCom.CreateAttribute("ImpSaldoAnt");
                xa.Value = item.SaldoAnterior;
                xDocto.Attributes.Append(xa);
                item.SaldoAnterior = xa.Value;

                xa = xCom.CreateAttribute("ImpPagado");
                xa.Value = item.Pagado;
                xDocto.Attributes.Append(xa);
                item.ImportePagado = xa.Value;

                xa = xCom.CreateAttribute("ImpSaldoInsoluto");
                xa.Value = item.SaldoInsoluto;
                xDocto.Attributes.Append(xa);
                item.SaldoInsoluto = xa.Value;

                //datosFacturaElectronica.conceptos[conceptosFacturaIndex] = item;
                datosFacturaElectronica.pagos.Add(item);
                conceptosFacturaIndex++;

                xPago.AppendChild(xDocto);
            }

            xPagos.AppendChild(xPago);

            XML = xCom.InnerXml;

            File.WriteAllText(@"C:\Impresos\COMP_1_0.xml", XML);
        }

        private void CrearComplemento20()
        {
            CalcularTotal();

            XmlDocument xCom = new XmlDocument();
            xCom.LoadXml(XML);

            XmlNamespaceManager comNms = new XmlNamespaceManager(xCom.NameTable);
            comNms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
            comNms.AddNamespace("pago20", "http://www.sat.gob.mx/Pagos20");

            XmlAttribute xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@Serie", comNms);
            xAttrib.Value = "-";
            datosFacturaElectronica.serie = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@Folio", comNms);
            if (idParcialidad > 0)
            {
                xAttrib.Value = idParcialidad.ToString();
            }
            else
            {
                xAttrib.Value = "-";
            }
            datosFacturaElectronica.folio = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@Fecha", comNms);
            fechaFactura = DateTime.UtcNow.AddHours(-7).ToString("s");
            //fechaFactura = dtpFechaComplemento.Value.ToUniversalTime().AddHours(-7).ToString("s");
            xAttrib.Value = fechaFactura;
            datosFacturaElectronica.fechaExpedicion = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Comprobante//@LugarExpedicion", comNms);
            xAttrib.Value = "63080";
            datosFacturaElectronica.domicilioEmisorCodigoPostal = xAttrib.Value;
            datosFacturaElectronica.lugarExpedicion = "Tepic, Nay.";

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Emisor//@Rfc", comNms);
            xAttrib.Value = rfcEmisor;
            datosFacturaElectronica.rfcEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Emisor//@Nombre", comNms);
            xAttrib.Value = nombreEmisor;
            datosFacturaElectronica.nombreEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Emisor//@RegimenFiscal", comNms);
            xAttrib.Value = "626";

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@Rfc", comNms);
            xAttrib.Value = _clienteElegido.rfc.Replace("-", ""); ;
            datosFacturaElectronica.rfcReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@Nombre", comNms);
            xAttrib.Value = _clienteElegido.nombre_constancia;
            datosFacturaElectronica.nombreReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@UsoCFDI", comNms);
            datosFacturaElectronica.usoCFDI = "CP01";
            xAttrib.Value = datosFacturaElectronica.usoCFDI;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@DomicilioFiscalReceptor", comNms);
            xAttrib.Value = _clienteElegido.codigo_postal;
            datosFacturaElectronica.domicilioReceptorCodigoPostal = xAttrib.Value;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Receptor//@RegimenFiscalReceptor", comNms);
            xAttrib.Value = _clienteElegido.regimen_fiscal;

            XmlNode xPagos = xCom.SelectSingleNode("//cfdi:Comprobante//pago20:Pagos", comNms);

            XmlAttribute xa;

            datosFacturaElectronica.conceptos = new List<ConceptoFactura>();
            int conceptosFacturaIndex = 0;

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Complemento//pago20:Pagos//@MontoTotalPagos", comNms);
            xAttrib.Value = Math.Round(Total, 2).ToString();
            xAttrib.Value = AddDecimals(xAttrib.Value);

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Complemento//pago20:Pagos//@TotalTrasladosBaseIVA16", comNms);
            //xAttrib.Value = (Total - IvaDRTotal + ISRTotal).ToString();
            xAttrib.Value = Math.Round(Total - IvaDRTotal + ISRTotal, 2).ToString();
            xAttrib.Value = AddDecimals(xAttrib.Value);

            xAttrib = (XmlAttribute)xCom.SelectSingleNode("//cfdi:Complemento//pago20:Pagos//@TotalTrasladosImpuestoIVA16", comNms);
            //xAttrib.Value = IvaDRTotal.ToString();
            xAttrib.Value = Math.Round(IvaDRTotal, 2).ToString();
            xAttrib.Value = AddDecimals(xAttrib.Value);

            XmlNode xPagosTotales = xCom.SelectSingleNode("//cfdi:Comprobante//pago20:Pagos//pago20:Totales", comNms);

            if (ISRTotal > 0)
            {
                xa = xCom.CreateAttribute("TotalRetencionesISR");
                //xa.Value = ISRTotal.ToString();
                xa.Value = Math.Round(ISRTotal, 2).ToString();
                xPagosTotales.Attributes.Append(xa);
            }

            XmlNode xPago = xCom.CreateNode(XmlNodeType.Element, "pago20", "Pago", "http://www.sat.gob.mx/Pagos20");

            xa = xCom.CreateAttribute("FechaPago");
            string fechaFacturaP = dtpFechaPagoComplemento.SelectedDate.Value.ToUniversalTime().AddHours(-7).ToString("s");
            xa.Value = fechaFacturaP;
            xPago.Attributes.Append(xa);

            xa = xCom.CreateAttribute("FormaDePagoP");
            datosFacturaElectronica.formaPago = cbFormasPago.SelectedValue.ToString();
            datosFacturaElectronica.formaPagoTexto = cbFormasPago.Text;
            xa.Value = datosFacturaElectronica.formaPago;
            xPago.Attributes.Append(xa);

            xa = xCom.CreateAttribute("MonedaP");
            xa.Value = "MXN";
            xPago.Attributes.Append(xa);

            xa = xCom.CreateAttribute("Monto");
            xa.Value = Math.Round(Total, 2).ToString();
            xa.Value = AddDecimals(xa.Value);
            xPago.Attributes.Append(xa);

            xa = xCom.CreateAttribute("TipoCambioP");
            xa.Value = "1";
            xPago.Attributes.Append(xa);

            foreach (ComplementoPagoData item in dgComplemento.Items)
            {
                XmlNode xDocto = xCom.CreateNode(XmlNodeType.Element, "pago20", "DoctoRelacionado", "http://www.sat.gob.mx/Pagos20");

                item.FechaPago = xa.Value;
                item.FormaPago = xa.Value;
                item.MonedaPago = xa.Value;
                item.MontoPago = xa.Value;

                xa = xCom.CreateAttribute("IdDocumento");
                xa.Value = item.UUID;
                xDocto.Attributes.Append(xa);
                item.IdDocto = xa.Value;

                xa = xCom.CreateAttribute("Serie");
                xa.Value = item.Serie;
                xDocto.Attributes.Append(xa);
                item.Serie = xa.Value;

                xa = xCom.CreateAttribute("Folio");
                xa.Value = item.Folio;
                xDocto.Attributes.Append(xa);
                item.Folio = xa.Value;

                xa = xCom.CreateAttribute("MonedaDR");
                xa.Value = "MXN";
                xDocto.Attributes.Append(xa);

                /*
                xa = xCom.CreateAttribute("MetodoDePagoDR");
                if (cbMetodoPago.SelectedIndex == 0)
                {
                    datosFacturaElectronica.metodoPago = "PUE";
                }
                else
                {
                    datosFacturaElectronica.metodoPago = "PPD";
                }
               
                datosFacturaElectronica.metodoPagoTexto = cbMetodoPago.Text;
                xa.Value = datosFacturaElectronica.metodoPago;
                xDocto.Attributes.Append(xa);
                item.MetodoPagoDR = xa.Value;
                */
                xa = xCom.CreateAttribute("NumParcialidad");
                xa.Value = item.Parcialidad;
                xDocto.Attributes.Append(xa);
                item.Parcialidad = xa.Value;

                xa = xCom.CreateAttribute("ImpSaldoAnt");
                xa.Value = item.SaldoAnterior;
                xDocto.Attributes.Append(xa);
                item.SaldoAnterior = xa.Value;

                xa = xCom.CreateAttribute("ImpPagado");
                xa.Value = item.Pagado;
                xDocto.Attributes.Append(xa);
                item.ImportePagado = xa.Value;

                xa = xCom.CreateAttribute("ImpSaldoInsoluto");
                xa.Value = item.SaldoInsoluto;
                xDocto.Attributes.Append(xa);
                item.SaldoInsoluto = xa.Value;

                xa = xCom.CreateAttribute("ObjetoImpDR");
                xa.Value = "02";
                xDocto.Attributes.Append(xa);

                xa = xCom.CreateAttribute("EquivalenciaDR");
                xa.Value = "1";
                xDocto.Attributes.Append(xa);

                XmlNode xImpuestosDR = xCom.CreateNode(XmlNodeType.Element, "pago20", "ImpuestosDR", "http://www.sat.gob.mx/Pagos20");

                if (ISRTotal > 0)
                {
                    XmlNode xRetencionesDR = xCom.CreateNode(XmlNodeType.Element, "pago20", "RetencionesDR", "http://www.sat.gob.mx/Pagos20");
                    XmlNode xRetencionDR = xCom.CreateNode(XmlNodeType.Element, "pago20", "RetencionDR", "http://www.sat.gob.mx/Pagos20");

                    xa = xCom.CreateAttribute("BaseDR");
                    //xa.Value = (Total - IvaDRTotal + ISRTotal).ToString();
                    //xa.Value = Math.Round(Total - IvaDRTotal + ISRTotal, 2).ToString();
                    xa.Value = Math.Round(float.Parse(item.Pagado) - float.Parse(item.IvaDR) + float.Parse(item.ISR), 2).ToString();
                    xa.Value = AddDecimals(xa.Value);
                    xRetencionDR.Attributes.Append(xa);

                    xa = xCom.CreateAttribute("ImpuestoDR");
                    xa.Value = "001";
                    xRetencionDR.Attributes.Append(xa);

                    xa = xCom.CreateAttribute("TipoFactorDR");
                    xa.Value = "Tasa";
                    xRetencionDR.Attributes.Append(xa);

                    xa = xCom.CreateAttribute("TasaOCuotaDR");
                    xa.Value = "0.012500";
                    xRetencionDR.Attributes.Append(xa);

                    xa = xCom.CreateAttribute("ImporteDR");
                    //xa.Value = Math.Round(IvaDRTotal, 2).ToString();
                    //xa.Value = Math.Round(ISRTotal, 2).ToString();
                    xa.Value = Math.Round(float.Parse(item.ISR), 2).ToString();
                    xa.Value = AddDecimals(xa.Value);
                    xRetencionDR.Attributes.Append(xa);

                    xRetencionesDR.AppendChild(xRetencionDR);
                    xImpuestosDR.AppendChild(xRetencionesDR);
                }

                XmlNode xTrasladosDR = xCom.CreateNode(XmlNodeType.Element, "pago20", "TrasladosDR", "http://www.sat.gob.mx/Pagos20");
                XmlNode xTrasladoDR = xCom.CreateNode(XmlNodeType.Element, "pago20", "TrasladoDR", "http://www.sat.gob.mx/Pagos20");

                xa = xCom.CreateAttribute("BaseDR");
                //xa.Value = (Total - IvaDRTotal + ISRTotal).ToString();
                //xa.Value = Math.Round(Total - IvaDRTotal + ISRTotal, 2).ToString();
                xa.Value = Math.Round(float.Parse(item.Pagado) - float.Parse(item.IvaDR) + float.Parse(item.ISR), 2).ToString();
                xa.Value = AddDecimals(xa.Value);
                xTrasladoDR.Attributes.Append(xa);

                xa = xCom.CreateAttribute("ImpuestoDR");
                xa.Value = "002";
                xTrasladoDR.Attributes.Append(xa);

                xa = xCom.CreateAttribute("TipoFactorDR");
                xa.Value = "Tasa";
                xTrasladoDR.Attributes.Append(xa);

                xa = xCom.CreateAttribute("TasaOCuotaDR");
                xa.Value = "0.160000";
                xTrasladoDR.Attributes.Append(xa);

                xa = xCom.CreateAttribute("ImporteDR");
                //xa.Value = Math.Round(IvaDRTotal, 2).ToString();
                //xa.Value = IvaDRTotal.ToString();
                xa.Value = Math.Round(float.Parse(item.IvaDR), 2).ToString();
                xa.Value = AddDecimals(xa.Value);
                xTrasladoDR.Attributes.Append(xa);

                xTrasladosDR.AppendChild(xTrasladoDR);
                xImpuestosDR.AppendChild(xTrasladosDR);
                
                xDocto.AppendChild(xImpuestosDR);

                //datosFacturaElectronica.conceptos[conceptosFacturaIndex] = item;
                datosFacturaElectronica.pagos.Add(item);
                conceptosFacturaIndex++;

                xPago.AppendChild(xDocto);
            }
            /*
            foreach (ComplementoPagoData item in dgComplemento.Items)
            {
                item.FechaPago = xa.Value;
                item.FormaPago = xa.Value;
                item.MonedaPago = xa.Value;
                item.MontoPago = xa.Value;

                XmlNode xImpuestosP = xCom.CreateNode(XmlNodeType.Element, "pago20", "ImpuestosP", "http://www.sat.gob.mx/Pagos20");

                if (ISRTotal > 0)
                {
                    XmlNode xRetencionesP = xCom.CreateNode(XmlNodeType.Element, "pago20", "RetencionesP", "http://www.sat.gob.mx/Pagos20");
                    XmlNode xRetencionP = xCom.CreateNode(XmlNodeType.Element, "pago20", "RetencionP", "http://www.sat.gob.mx/Pagos20");

                    xa = xCom.CreateAttribute("ImpuestoP");
                    xa.Value = "001";
                    xRetencionP.Attributes.Append(xa);

                    xa = xCom.CreateAttribute("ImporteP");
                    //xa.Value = Math.Round(IvaDRTotal, 2).ToString();
                    xa.Value = Math.Round(ISRTotal, 2).ToString();
                    xa.Value = AddDecimals(xa.Value);
                    xRetencionP.Attributes.Append(xa);

                    xRetencionesP.AppendChild(xRetencionP);
                    xImpuestosP.AppendChild(xRetencionesP);
                }

                XmlNode xTrasladosP = xCom.CreateNode(XmlNodeType.Element, "pago20", "TrasladosP", "http://www.sat.gob.mx/Pagos20");
                XmlNode xTrasladoP = xCom.CreateNode(XmlNodeType.Element, "pago20", "TrasladoP", "http://www.sat.gob.mx/Pagos20");

                xa = xCom.CreateAttribute("BaseP");
                xa.Value = Math.Round(Total - IvaDRTotal + ISRTotal, 2).ToString();
                //xa.Value = (Total - IvaDRTotal + ISRTotal).ToString();
                xa.Value = AddDecimals(xa.Value);
                xTrasladoP.Attributes.Append(xa);

                xa = xCom.CreateAttribute("ImpuestoP");
                xa.Value = "002";
                xTrasladoP.Attributes.Append(xa);

                xa = xCom.CreateAttribute("TipoFactorP");
                xa.Value = "Tasa";
                xTrasladoP.Attributes.Append(xa);

                xa = xCom.CreateAttribute("TasaOCuotaP");
                xa.Value = "0.160000";
                xTrasladoP.Attributes.Append(xa);

                xa = xCom.CreateAttribute("ImporteP");
                xa.Value = Math.Round(IvaDRTotal, 2).ToString();
                //xa.Value = IvaDRTotal.ToString();
                xa.Value = AddDecimals(xa.Value);
                xTrasladoP.Attributes.Append(xa);

                xTrasladosP.AppendChild(xTrasladoP);
                xImpuestosP.AppendChild(xTrasladosP);

                xPago.AppendChild(xImpuestosP);
            }
            */

            XmlNode xImpuestosP = xCom.CreateNode(XmlNodeType.Element, "pago20", "ImpuestosP", "http://www.sat.gob.mx/Pagos20");

            if (ISRTotal > 0)
            {
                XmlNode xRetencionesP = xCom.CreateNode(XmlNodeType.Element, "pago20", "RetencionesP", "http://www.sat.gob.mx/Pagos20");
                XmlNode xRetencionP = xCom.CreateNode(XmlNodeType.Element, "pago20", "RetencionP", "http://www.sat.gob.mx/Pagos20");

                xa = xCom.CreateAttribute("ImpuestoP");
                xa.Value = "001";
                xRetencionP.Attributes.Append(xa);

                xa = xCom.CreateAttribute("ImporteP");
                //xa.Value = Math.Round(IvaDRTotal, 2).ToString();
                xa.Value = Math.Round(ISRTotal, 2).ToString();
                xa.Value = AddDecimals(xa.Value);
                xRetencionP.Attributes.Append(xa);

                xRetencionesP.AppendChild(xRetencionP);
                xImpuestosP.AppendChild(xRetencionesP);
            }

            XmlNode xTrasladosP = xCom.CreateNode(XmlNodeType.Element, "pago20", "TrasladosP", "http://www.sat.gob.mx/Pagos20");
            XmlNode xTrasladoP = xCom.CreateNode(XmlNodeType.Element, "pago20", "TrasladoP", "http://www.sat.gob.mx/Pagos20");

            xa = xCom.CreateAttribute("BaseP");
            xa.Value = Math.Round(Total - IvaDRTotal + ISRTotal, 2).ToString();
            //xa.Value = (Total - IvaDRTotal + ISRTotal).ToString();
            xa.Value = AddDecimals(xa.Value);
            xTrasladoP.Attributes.Append(xa);

            xa = xCom.CreateAttribute("ImpuestoP");
            xa.Value = "002";
            xTrasladoP.Attributes.Append(xa);

            xa = xCom.CreateAttribute("TipoFactorP");
            xa.Value = "Tasa";
            xTrasladoP.Attributes.Append(xa);

            xa = xCom.CreateAttribute("TasaOCuotaP");
            xa.Value = "0.160000";
            xTrasladoP.Attributes.Append(xa);

            xa = xCom.CreateAttribute("ImporteP");
            xa.Value = Math.Round(IvaDRTotal, 2).ToString();
            //xa.Value = IvaDRTotal.ToString();
            xa.Value = AddDecimals(xa.Value);
            xTrasladoP.Attributes.Append(xa);

            xTrasladosP.AppendChild(xTrasladoP);
            xImpuestosP.AppendChild(xTrasladosP);

            xPago.AppendChild(xImpuestosP);

            xPagos.AppendChild(xPago);

            XML = xCom.InnerXml;

            File.WriteAllText(@"C:\Impresos\COMP_2_0.xml", XML);
        }

        private void GenerarComplemento()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(XML);

            XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);

            if (XML.Contains("cfd/3"))
            {
                nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                nms.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
            }
            else
            {
                nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
                nms.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
            }

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

            XML = xDoc.InnerXml;
        }
        private void CalcularTotal()
        {
            float total = 0;
            float totalIva = 0;
            float totalIsr = 0;

            foreach (ComplementoPagoData item in dgComplemento.Items)
            {
                total = total + float.Parse(item.Pagado);
                totalIva = totalIva + float.Parse(item.IvaDR);
                if (item.ISR != "")
                {
                    totalIsr = totalIsr + float.Parse(item.ISR);
                }
            }
            lblTotal.Content = total.ToString("0.00");

            IvaDRTotal = totalIva;
            ISRTotal = totalIsr;
            Total = total;
        }
        private void InsertarParcialidad()
        {
            idParcialidad = 0;
            try
            {

                foreach (ComplementoPagoData item in dgComplemento.Items)
                {
                    int idFactura = int.Parse(item.IdFactura);
                    String xml = XML;
                    float anterior = float.Parse(item.SaldoAnterior);
                    float pagado = float.Parse(item.Pagado);
                    float insoluto = float.Parse(item.SaldoInsoluto);
                    int parcialidad = int.Parse(item.Parcialidad);

                    int idContribuyente = int.Parse(cbContribuyentes.SelectedValue.ToString());

                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        Parcialidades parce = new Parcialidades();

                        parce.id_factura = idFactura;
                        parce.xml = xml;
                        parce.anterior = anterior;
                        parce.pagado = pagado;
                        parce.insoluto = insoluto;
                        parce.parcialidad = parcialidad;
                        parce.fecha = DateTime.Now;
                        parce.estado = "ACTIVO";

                        dbContext.Parcialidades.Add(parce);

                        dbContext.SaveChanges();

                        //idParcialidad = parce.id_parcialidad;

                        Contribuyentes cont = new Contribuyentes();
                        cont = dbContext.Contribuyentes.Where(C => C.id_contribuyente == idContribuyente).FirstOrDefault();
                        FolioActual = int.Parse(cont.num_complemento);
                        cont.num_complemento = (int.Parse(cont.num_complemento) + 1).ToString();

                        parce.folio = FolioActual.ToString();

                        dbContext.SaveChanges();

                        idParcialidad = int.Parse(cont.num_complemento);
                        datosFacturaElectronica.folio = idParcialidad.ToString();

                        xml = xml.Replace("Folio=\"-\"", "Folio=\"" + idParcialidad.ToString() + "\"");

                        parce.xml = xml;
                        XML = xml;

                        dbContext.SaveChanges();

                        item.IdParcialidad = parce.id_parcialidad.ToString();
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message + " " + exc.InnerException.Message);
            }
        }
        private void ActualizarParcialidad()
        {
            foreach (ComplementoPagoData item in dgComplemento.Items)
            {
                int idParcialidad = int.Parse(item.IdParcialidad);
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Parcialidades parce = dbContext.Parcialidades.Where(P => P.id_parcialidad == idParcialidad).FirstOrDefault();

                    parce.xml = XML;
                    parce.sello_cfdi = datosFacturaElectronica.selloCFD;
                    parce.sello_sat = datosFacturaElectronica.selloSAT;
                    parce.cadena_original = datosFacturaElectronica.cadenaOriginalSAT;

                    dbContext.SaveChanges();
                }
            }
        }
        void ImprimirPDF(Boolean SAT)
        {
            try
            {
                System.IO.Directory.CreateDirectory(@"C:\Impresos\Complementos\" + datosFacturaElectronica.nombreReceptor);
                String Directorio = @"C:\Impresos\Complementos\" + datosFacturaElectronica.nombreReceptor;
                string fileName;
                if (SAT)
                {
                    fileName = @"C:\Impresos\Complementos\" + datosFacturaElectronica.nombreReceptor + @"\com_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
                }
                else
                {
                    fileName = @"C:\Impresos\Complementos\" + datosFacturaElectronica.nombreReceptor + @"\pre_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
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
                    .Add(new Paragraph("Complemento")));

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
                    .Add(new Paragraph(datosFacturaElectronica.usoCFDI)));

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
                //Conceptos
                //Renglón 1
                table.AddCell(new Cell(1, 5)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("UUID")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Saldo Anterior")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Monto pagado")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Saldo insoluto")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Parcialidad")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Folio")));

                foreach (ComplementoPagoData item in _complementos)
                {
                    table.AddCell(new Cell(1, 5)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(item.IdDocto)));

                    table.AddCell(new Cell(1, 1)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(item.SaldoAnterior.ToString())));

                    table.AddCell(new Cell(1, 1)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(item.ImportePagado)));

                    table.AddCell(new Cell(1, 1)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(item.SaldoInsoluto)));

                    table.AddCell(new Cell(1, 1)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(item.Parcialidad.ToString())));

                    table.AddCell(new Cell(1, 1)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(item.Folio.ToString())));
                }

                table.AddCell(new Cell(1, 9)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Total ")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("$ " + AddDecimals(Total.ToString()))));

                table.AddCell(new Cell(1, 10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(" ")));

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
                    rutaPDF = fileName;
                    Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = fileName;
                    while (!File.Exists(fileName))
                    {

                    }
                    prc.Start();

                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR: " + exc.Message);
            }
        }

        private void dgFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgFacturas.SelectedItem != null)
                {
                    String uuid = "";
                    String serie = "-";
                    String folio = "";
                    String parcialidad = "1";
                    String saldoAnterior = "0";
                    String pagado = "0";
                    String saldoInsoluto = "0";
                    String IdFactura = "0";
                    String IvaDR = "0";

                    FacturaComplemento fact = (FacturaComplemento)dgFacturas.SelectedItem;

                    if (fact.Estado == "CANCELADO")
                    {
                        MessageBox.Show("La factura esta cancelada. No se puede utilizar.", "ATENCION", MessageBoxButton.OK);
                        return;
                    }

                    IdFactura = fact.IdFactura.ToString();

                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(fact.XML);

                    XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);
                    nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                    nms.AddNamespace("tfd", "http://www.sat.gob.mx/cfd/3");

                    XmlAttribute xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//@UUID", nms);
                    uuid = xAttrib.Value;

                    xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Folio", nms);
                    if (xAttrib != null)
                    {
                        folio = xAttrib.Value;
                    }
                    else
                    {
                        MessageBox.Show("Error. No se encontró el folio.");
                        return;
                    }

                    xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Impuestos//@TotalImpuestosTrasladados", nms);
                    IvaDR = xAttrib.Value;

                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        Parcialidades parce = dbContext.Parcialidades.Where(P => P.id_factura == fact.IdFactura).OrderByDescending(P => P.id_parcialidad).FirstOrDefault();

                        if (parce != null)
                        {
                            parcialidad = (parce.parcialidad + 1).ToString();
                            saldoAnterior = parce.insoluto.ToString();
                        }
                        else
                        {
                            saldoAnterior = "0";
                            saldoInsoluto = fact.Total;
                        }

                        ComplementoPagoData comp = new ComplementoPagoData { UUID = uuid, Serie = serie, Folio = folio, Parcialidad = parcialidad, SaldoAnterior = saldoAnterior, Pagado = pagado, SaldoInsoluto = saldoInsoluto, IdFactura = IdFactura, IvaDR = IvaDR };
                        _complementos.Add(comp);
                        dgComplemento.ItemsSource = null;
                        dgComplemento.ItemsSource = _complementos;
                    }
                    CalcularTotal();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("No se puede agregar la factura.");
            }
        }

        private void dgComplemento_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgComplemento.SelectedItem != null)
            {
                ComplementoPagoData data = (ComplementoPagoData)dgComplemento.SelectedItem;
                ComplementoPagoControl control = new ComplementoPagoControl(data, this);
                control.ShowDialog();
            }
        }
        public void ActualizarComplemento()
        {
            dgComplemento.ItemsSource = null;
            dgComplemento.ItemsSource = _complementos;
            CalcularTotal();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private String AddDecimals(String o)
        {
            String n = "";

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

            return n;
        }

        private void cbFormasPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            datosFacturaElectronica.formaPago = cbFormasPago.SelectedValue.ToString();
            datosFacturaElectronica.formaPagoTexto = (e.AddedItems[0] as FormasPago).FormaPago;
        }

        private void cbMetodoPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMetodoPago.SelectedIndex == 0)
            {
                datosFacturaElectronica.metodoPago = "PUE";
                datosFacturaElectronica.metodoPagoTexto = "Pago en una sola exhibición";
            }
            else
            {
                datosFacturaElectronica.metodoPago = "PPD";
                datosFacturaElectronica.metodoPagoTexto = "Pago en parcialidades o diferido";
            }
        }
        private void EnviarPorCorreo()
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    List<Correos> correos = dbContext.Correos.Where(C => C.id_cliente == _clienteElegido.id_cliente).ToList();

                    foreach (Correos item in correos)
                    {
                        Configuracion config = dbContext.Configuracion.Single();
                        MailMessage mail = new MailMessage();
                        //SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
                        SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                        //mail.From = new MailAddress("alvarezimpresores_16@hotmail.com");
                        mail.From = new MailAddress(config.correo);

                        mail.To.Add(item.correo);
                        mail.Subject = "Envío de información de complementos - Alvarez Impresores";
                        mail.Body = "Saludos, envío la información de los complementos. Recuerde: después de 72 horas no se pueden cancelar.";

                        if (rutaPDF.Length > 0)
                        {
                            System.Net.Mail.Attachment attachment;
                            attachment = new System.Net.Mail.Attachment(@rutaPDF);
                            mail.Attachments.Add(attachment);
                        }
                        if (rutaXML.Length > 0)
                        {
                            System.Net.Mail.Attachment attachment;
                            attachment = new System.Net.Mail.Attachment(@rutaXML);
                            mail.Attachments.Add(attachment);
                        }

                        SmtpServer.Port = 587;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(config.usuario_correo, config.password_correo);
                        SmtpServer.EnableSsl = true;

                        SmtpServer.Send(mail);
                        MessageBox.Show("Correo enviado a " + item.correo);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnPagarTodos_Click(object sender, RoutedEventArgs e)
        {
            foreach (ComplementoPagoData item in _complementos)
            {
                item.Pagado = item.SaldoInsoluto;
                item.SaldoInsoluto = "0";
            }

            dgComplemento.ItemsSource = null;
            dgComplemento.ItemsSource = _complementos;

            CalcularTotal();
        }

        private void dgComplemento_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (dgComplemento.SelectedItem != null)
                {
                    ComplementoPagoData rem = (ComplementoPagoData)dgComplemento.SelectedItem;
                    _complementos.Remove(rem);
                    dgComplemento.ItemsSource = null;
                    dgComplemento.ItemsSource = _complementos;
                    CalcularTotal();
                }
            }
        }

        private void btnAgregarTodos_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in dgFacturas.SelectedItems)
            {
                FacturaComplemento fact = (FacturaComplemento)item;

                try
                {
                    if (dgFacturas.SelectedItem != null)
                    {
                        String uuid = "";
                        String serie = "-";
                        String folio = "";
                        String parcialidad = "1";
                        String saldoAnterior = "0";
                        String pagado = "0";
                        String saldoInsoluto = "0";
                        String IdFactura = "0";
                        String IvaDR = "0";
                        String ISR = "0";

                        if (fact.Estado == "CANCELADO")
                        {
                            MessageBox.Show("La factura esta cancelada. No se puede utilizar.", "ATENCION", MessageBoxButton.OK);
                            return;
                        }

                        IdFactura = fact.IdFactura.ToString();

                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(fact.XML);

                        XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);

                        if (fact.XML.Contains("cfd/3"))
                        {
                            nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                            nms.AddNamespace("tfd", "http://www.sat.gob.mx/cfd/3");
                        }
                        else
                        {
                            nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
                            nms.AddNamespace("tfd", "http://www.sat.gob.mx/cfd/4");

                        }

                        XmlAttribute xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Complemento//@UUID", nms);
                        uuid = xAttrib.Value;

                        xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Folio", nms);
                        if (xAttrib != null)
                        {
                            folio = xAttrib.Value;
                        }
                        else
                        {
                            MessageBox.Show("Error. No se encontró el folio.");
                            return;
                        }

                        xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Impuestos//@TotalImpuestosTrasladados", nms);
                        IvaDR = xAttrib.Value;
                        try
                        {
                            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Impuestos//@TotalImpuestosRetenidos", nms);
                            if (xAttrib != null)
                            {
                                ISR = xAttrib.Value;
                            }
                            else
                            {
                                ISR = "0";
                            }
                        }
                        catch (Exception exc)
                        {
                            ISR = "";
                        }

                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            Parcialidades parce = dbContext.Parcialidades.Where(P => P.id_factura == fact.IdFactura && P.estado == "ACTIVO").OrderByDescending(P => P.id_parcialidad).FirstOrDefault();

                            if (parce != null)
                            {
                                parcialidad = (parce.parcialidad + 1).ToString();
                                //parcialidad = AddDecimals(parcialidad);
                                saldoAnterior = parce.insoluto.ToString();
                                saldoAnterior = AddDecimals(saldoAnterior);
                            }
                            else
                            {
                                saldoAnterior = fact.Total;
                                saldoAnterior = AddDecimals(saldoAnterior);
                                saldoInsoluto = fact.Total;
                                saldoInsoluto = AddDecimals(saldoInsoluto);
                            }

                            ComplementoPagoData comp = new ComplementoPagoData { UUID = uuid, Serie = serie, Folio = folio, Parcialidad = parcialidad, SaldoAnterior = saldoAnterior, Pagado = AddDecimals(pagado), SaldoInsoluto = saldoInsoluto, IdFactura = IdFactura, IvaDR = IvaDR, ISR = ISR };
                            _complementos.Add(comp);
                            dgComplemento.ItemsSource = null;
                            dgComplemento.ItemsSource = _complementos;
                        }
                        CalcularTotal();
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("No se puede agregar la factura." + exc.Message);
                }
            }
        }

        private void dgFacturas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FacturaComplemento f = (FacturaComplemento)dgFacturas.SelectedItem;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    _parcialidades = dbContext.Parcialidades.Where(P => P.id_factura == f.IdFactura).ToList();
                    dgParcialidades.ItemsSource = null;
                    dgParcialidades.ItemsSource = _parcialidades;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error: " + exc.Message);
            }
        }

        private void btnCrearComplemento40_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Desea generar el complemento 2.0?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            foreach (ComplementoPagoData item in dgComplemento.Items)
            {
                if (item.Pagado == "0")
                {
                    MessageBox.Show("No ha ingresado un monto mayor a 0 para el pago.");
                    return;
                }
            }

            CargarXMLTemplate20();
            InsertarParcialidad();

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
                regimen = "Regimen simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC";
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
                regimen = "Regimen simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC";
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
                regimen = "Regimen simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 619 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63000";
            }
            /*
            rutaCertificado = @"C:\Impresos\Pruebas\Certificado.cer";
            rutaLlave = @"C:\Impresos\Pruebas\Llave.key";
            contraseñaLlave = "12345678a";
            nombreEmisor = "XOCHILT CASAS CHAVEZ";
            rfcEmisor = "CACX7605101P8";
            serie = "-";
            usuarioFacturacion = "pruebasWS";
            contraseñaFacturacion = "pruebasWS";
            curp = "-";
            regimen = "Regimen simplificado de confianza";

            datosFacturaElectronica.domicilioEmisorCalle = "";
            datosFacturaElectronica.domicilioEmisorColonia = "";
            datosFacturaElectronica.domicilioEmisorMunicipio = " ";
            datosFacturaElectronica.domicilioEmisorEstado = "";
            datosFacturaElectronica.domicilioEmisorCodigoPostal = "44970";
            */

            FacturacionElectronica40(true);
            ImprimirPDF(true);
            if (timbreValido)
            {
                ActualizarParcialidad();
                Thread email = new Thread(EnviarPorCorreo);
                email.Start();
                this.Close();
                //EnviarPorCorreo();
            }
        }

        private void btnBorrarComplemento_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgParcialidades.SelectedItem != null)
                {
                    if (MessageBox.Show("Desea eliminar la parcialidad?", "Atención", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Parcialidades p = (Parcialidades)dgParcialidades.SelectedItem;
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            Parcialidades Par = dbContext.Parcialidades.Where(P => P.id_parcialidad == p.id_parcialidad).First();
                            dbContext.Parcialidades.Remove(Par);
                            dbContext.SaveChanges();
                        }
                        FacturaComplemento f = (FacturaComplemento)dgFacturas.SelectedItem;

                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            _parcialidades = dbContext.Parcialidades.Where(P => P.id_factura == f.IdFactura).ToList();
                            dgParcialidades.ItemsSource = null;
                            dgParcialidades.ItemsSource = _parcialidades;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR " + exc.Message);
            }
        }
    }
}
