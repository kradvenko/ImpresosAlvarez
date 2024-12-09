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
//using ImpresosAlvarez.mx.facturacfdi.dev33;
using System.Net.Mail;
using System.Net;

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

        Entity.FacturaDigital FacturaCancelada;
        String UUIDCancelado;

        bool Es40 = false;
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
            datosFacturaElectronica.metodoPago = "PUE";
            datosFacturaElectronica.metodoPagoTexto = "Pago en una sola exhibición";

            datosFacturaElectronica.numeroCertificadoSAT = "CSD";
            datosFacturaElectronica.numeroCertificado = "NC";
            datosFacturaElectronica.fechaTimbrado = "FECHATIMBRADO";
            datosFacturaElectronica.uuid = "UUID";

            //Para complemento INE
            datosFacturaElectronica.IneTipoProceso = "";
            datosFacturaElectronica.IneTipoComite = "";
            datosFacturaElectronica.IneIdContabilidad = "";
            datosFacturaElectronica.IneEntidad = "";
            datosFacturaElectronica.IneClaveContabilidad = "";
            datosFacturaElectronica.IneAmbito = "";

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

            //cbUsosCFDI.SelectedIndex = cbUsosCFDI.Items.Count - 1;
            cbUsosCFDI.SelectedIndex = 12;
            datosFacturaElectronica.usoCFDITexto = cbUsosCFDI.Text;
            cbFormasPago.SelectedIndex = cbFormasPago.Items.Count - 1;
            datosFacturaElectronica.formaPagoTexto = cbFormasPago.Text;
            
            if (datosFacturaElectronica.metodoPago == "PUE")
            {
                cbMetodoPago.SelectedIndex = 0;
            }

            FacturaCancelada = null;

            rutaPDF = "";
            rutaXML = "";
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;
                lblNombre.Content = _clienteElegido.nombre;
                lblRFC.Content = _clienteElegido.rfc;
                lblDomicilio.Content = _clienteElegido.domicilio + " " + _clienteElegido.colonia + " " + _clienteElegido.ciudad;
                lblNombreConstancia.Content = _clienteElegido.nombre_constancia;
                lblRegimenFiscalReceptor.Content = _clienteElegido.regimen_fiscal;
                lblCodigoPostal.Content = _clienteElegido.codigo_postal;

                if (_clienteElegido.aplica_retencion == "SI")
                {
                    cbAplicaRetencion.IsChecked = true;
                }
                else
                {
                    cbAplicaRetencion.IsChecked = false;
                }
            }
        }

        public void CalcularTotal()
        {
            Subtotal = 0;
            Total = 0;
            RetencionIva = 0;
            RetencionIsr = 0;
            RetencionCedular = 0;
            float iva = 0;            

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
                float temp = item.Importe * 0.16f;
                iva += float.Parse(Math.Round(temp, 2).ToString());

                if (_clienteElegido.aplica_retencion == "SI")
                {
                    //RetencionIsr += item.Importe * 0.012500f;
                    //RetencionIsr += Redondear(item.Importe * 0.012500f);
                    //RetencionIsr += float.Parse((Math.Ceiling(item.Importe * 0.012500f)).ToString());
                    float temp2 = item.Importe * 0.012500f;
                    RetencionIsr += float.Parse(Math.Round(temp2, 2).ToString());
                    //RetencionIsr += float.Parse(Math.Round((item.Importe * 0.012500f), 2).ToString());
                }
            }

            
            iva = float.Parse((Math.Round(iva, 2)).ToString());
            //iva = Redondear(iva);

            //RetencionIsr = Redondear(RetencionIsr);
            RetencionIsr = float.Parse(Math.Round(RetencionIsr, 2).ToString());

            datosFacturaElectronica.iva = iva.ToString();
            datosFacturaElectronica.retencionIsr = RetencionIsr.ToString();

            //datosFacturaElectronica.totalImpuestos = datosFacturaElectronica.CalcularTotalImpuestos().ToString();

            //RetencionIsr = float.Parse(Math.Round(RetencionIsr, 2).ToString());
            //RetencionIva = float.Parse(Math.Round(RetencionIva, 2).ToString());
            //RetencionCedular = float.Parse(Math.Round(RetencionCedular, 2).ToString());

            //iva = float.Parse((Math.Round(iva, 2)).ToString());
            /*
            if (tbIVAMod.Text.Length > 3)
            {
                iva = float.Parse(tbIVAMod.Text);
            }
            */
            //Total = Subtotal + float.Parse(datosFacturaElectronica.totalImpuestos);
            /*
            datosFacturaElectronica.retencionIsr = RetencionIsr.ToString();
            datosFacturaElectronica.retencionIva = RetencionIva.ToString();
            datosFacturaElectronica.retencionCedular = RetencionCedular.ToString();
            */

            Total = Subtotal + iva - RetencionIsr - RetencionIva - RetencionCedular;

            Total = float.Parse(Math.Round(Total, 2).ToString());

            //lblTotalImpuestos.Content = "$ " + datosFacturaElectronica.totalImpuestos;
            lblSubtotal.Content = "$ " + Subtotal.ToString();
            lblISR.Content = "$ " + RetencionIsr.ToString();
            lblIVA.Content = "$ " + iva.ToString();
            lblTotal.Content = "$ " + Math.Round(Total, 2).ToString() + " (" + ConvertirALetra(Total.ToString()) + ") ";
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
                contraseñaLlave = "Musica47**";
                nombreEmisor = "JOSE ALVAREZ JIMENEZ";
                rfcEmisor = "AAJJ470205DH1";
                serie = "-";
                usuarioFacturacion = "JoseAlvarezJi";
                contraseñaFacturacion = "oF5r1o6S3";
                curp = "AAJJ470205HNTLMS00";
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 619 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
            }
            
            CargarXML4Template();
            
            FacturacionElectronica40(false);
            ImprimirPDF(false);
            //EnviarPorCorreo();
        }

        private void btnFacturar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Desea facturar?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }
            if (cbContribuyentes.Text.Contains("JOSE"))
            {
                rutaCertificado = @"C:\Impresos\Jose\Certificado.cer";
                rutaLlave = @"C:\Impresos\Jose\Llave.key";
                contraseñaLlave = "Musica47*";
                nombreEmisor = "JOSE ALVAREZ JIMENEZ";
                rfcEmisor = "AAJJ470205DH1";
                serie = "-";
                usuarioFacturacion = "JoseAlvarezJi";
                contraseñaFacturacion = "oF5r1o6S3";
                curp = "AAJJ470205HNTLMS00";
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 619 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
            regimen = "Regimen de incorporación fiscal";

            datosFacturaElectronica.domicilioEmisorCalle = "";
            datosFacturaElectronica.domicilioEmisorColonia = "";
            datosFacturaElectronica.domicilioEmisorMunicipio = " ";
            datosFacturaElectronica.domicilioEmisorEstado = "";
            datosFacturaElectronica.domicilioEmisorCodigoPostal = "44970";
            */

            CargarXMLTemplate();
            FacturacionElectronica(true);
            if (timbreValido)
            {
                ImprimirPDF(true);
                GuardarFactura();
                EnviarPorCorreo();
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
            if (FacturaCancelada == null)
            {
                XML = File.ReadAllText(@"C:\Impresos\Facturacion\XML_3_3_Template.xml");
            }
            else
            {
                XML = File.ReadAllText(@"C:\Impresos\Facturacion\XML_3_3_Template_Cancelacion.xml");
            }
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
                string fileName = @"C:\Impresos\Facturas\" + _clienteElegido.nombre + @"\pre_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";

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
                    string fileName = @"C:\Impresos\Facturas\" + _clienteElegido.nombre.Replace("\"", "") + @"\fac_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";
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
                MessageBox.Show("Ha ocurrido un error inesperado. " + exc.Message);
            }
            
        }

        private void Timbrar40()
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
                    string fileName = @"C:\Impresos\Facturas\" + _clienteElegido.nombre.Replace("\"", "") + @"\fac_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";
                    File.WriteAllText(fileName, XMLTimbrado);
                    rutaXML = fileName;
                    GenerarComplemento40();
                    timbreValido = true;
                }
                else
                {
                    //timbreValido = true;
                    XMLTimbrado = respTimbre.mensaje;
                    MessageBox.Show(XMLTimbrado);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ha ocurrido un error inesperado. " + exc.Message);
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
            if (cbMetodoPago.SelectedIndex == 0)
            {
                datosFacturaElectronica.metodoPago = "PUE";
            }
            else
            {
                datosFacturaElectronica.metodoPago = "PPD";
            }
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

            if (FacturaCancelada != null)
            {
                XmlNode xCfdiRelacionados = xDoc.SelectSingleNode("//cfdi:CfdiRelacionados", nms);
                XmlNode xCfdiRelacionado = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "CfdiRelacionado", "http://www.sat.gob.mx/cfd/3");

                XmlAttribute xR = xDoc.CreateAttribute("UUID");
                xR.Value = UUIDCancelado;
                xCfdiRelacionado.Attributes.Append(xR);

                xCfdiRelacionados.AppendChild(xCfdiRelacionado);
            }

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Rfc", nms);
            xAttrib.Value = rfcEmisor;
            datosFacturaElectronica.rfcEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Nombre", nms);
            xAttrib.Value = nombreEmisor;
            datosFacturaElectronica.nombreEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@RegimenFiscal", nms);
            xAttrib.Value = "626";

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
            float baseTotal = 0;
            float impuestoRetencion = 0;
            float impuestoRetencionTotal = 0;
            float baseTotalRetencion = 0;

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

                    //IVA
                    impuesto = item.Cantidad * item.PrecioUnitario;
                    impuesto = impuesto * 0.16f;
                    impuesto = float.Parse((Math.Round(impuesto, 2)).ToString());
                    //impuesto = Redondear(impuesto);
                    impuestoTotal = impuestoTotal + impuesto;
                    baseTotal = baseTotal + item.Importe;

                    //RETENCION
                    impuestoRetencion = item.Cantidad * item.PrecioUnitario;
                    impuestoRetencion = impuestoRetencion * 0.012500f;
                    impuestoRetencion = float.Parse((Math.Round(impuestoRetencion, 2)).ToString());
                    //impuestoRetencion = Redondear(impuestoRetencion);
                    impuestoRetencionTotal = impuestoRetencionTotal + impuestoRetencion;
                    baseTotalRetencion = baseTotalRetencion + item.Importe;

                    XmlNode xImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Impuestos", "http://www.sat.gob.mx/cfd/3");

                    XmlNode xTraslados = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslados", "http://www.sat.gob.mx/cfd/3");

                    XmlNode xTraslado = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslado", "http://www.sat.gob.mx/cfd/3");

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

                    if (_clienteElegido.aplica_retencion == "SI")
                    {
                        XmlNode xRetenciones = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/3");

                        XmlNode xRetencion = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retencion", "http://www.sat.gob.mx/cfd/3");

                        xa = xDoc.CreateAttribute("Base");
                        xa.Value = item.Importe.ToString();
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("Impuesto");
                        xa.Value = "001";
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("TipoFactor");
                        xa.Value = "Tasa";
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("TasaOCuota");
                        xa.Value = "0.012500";
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("Importe");
                        xa.Value = AddDecimals(impuestoRetencion.ToString());
                        xRetencion.Attributes.Append(xa);

                        xRetenciones.AppendChild(xRetencion);
                        xImpuestos.AppendChild(xRetenciones);
                    }

                    xConcepto.AppendChild(xImpuestos);
                    xConceptos.AppendChild(xConcepto);
                }
                    
            }
            
            XmlNode xTrasladosImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslados", "http://www.sat.gob.mx/cfd/3");

            XmlNode xTrasladoImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslado", "http://www.sat.gob.mx/cfd/3");

            XmlNode xRetencionesImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/3");

            XmlNode xRetencionImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retencion", "http://www.sat.gob.mx/cfd/3");

            if (_clienteElegido.aplica_retencion == "SI")
            {
                /*
                xa = xDoc.CreateAttribute("Base");
                xa.Value = baseTotal.ToString();
                xRetencionImpuestos.Attributes.Append(xa);
                */
                xa = xDoc.CreateAttribute("Impuesto");
                xa.Value = "001";
                xRetencionImpuestos.Attributes.Append(xa);
                /*
                xa = xDoc.CreateAttribute("TipoFactor");
                xa.Value = "Tasa";
                xTrasladoImpuestos.Attributes.Append(xa);
                
                xa = xDoc.CreateAttribute("TasaOCuota");
                xa.Value = "0.012500";
                xRetencionImpuestos.Attributes.Append(xa);
                */

                xa = xDoc.CreateAttribute("Importe");
                impuestoRetencionTotal = float.Parse((Math.Round(impuestoRetencionTotal, 2)).ToString());
                xa.Value = AddDecimals(impuestoRetencionTotal.ToString());
                xRetencionImpuestos.Attributes.Append(xa);

                xRetencionesImpuestos.AppendChild(xRetencionImpuestos);

                xImpuestosNodo.AppendChild(xRetencionesImpuestos);

                xa = xDoc.CreateAttribute("TotalImpuestosRetenidos");
                xa.Value = AddDecimals(impuestoRetencionTotal.ToString());
                xImpuestosNodo.Attributes.Append(xa);

                //RetencionIsr = impuestoRetencionTotal;
            }
            /*
            xa = xDoc.CreateAttribute("Base");
            xa.Value = baseTotal.ToString();
            xTrasladoImpuestos.Attributes.Append(xa);
            */
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
            /*
            if (tbIVAMod.Text.Length > 3)
            {
                xa.Value = tbIVAMod.Text;
            }
            else
            {
                impuestoTotal = float.Parse((Math.Round(impuestoTotal, 2)).ToString());
                xa.Value = AddDecimals(impuestoTotal.ToString());
            }
            */
            impuestoTotal = float.Parse((Math.Round(impuestoTotal, 2)).ToString());
            xa.Value = AddDecimals(impuestoTotal.ToString());
            xTrasladoImpuestos.Attributes.Append(xa);

            xTrasladosImpuestos.AppendChild(xTrasladoImpuestos);

            xImpuestosNodo.AppendChild(xTrasladosImpuestos);

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Impuestos//@TotalImpuestosTrasladados", nms);
            xAttrib.Value = AddDecimals(impuestoTotal.ToString());

            if (datosFacturaElectronica.IneTipoProceso.Length > 0)
            {
                XmlNode xComprobante = xDoc.SelectSingleNode("//cfdi:Comprobante", nms);
                XmlNode xComplemento;

                xComplemento = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Complemento", "http://www.sat.gob.mx/cfd/3");

                XmlNode xComplementoINE = xDoc.CreateNode(XmlNodeType.Element, "ine", "INE", "http://www.sat.gob.mx/ine");

                xa = xDoc.CreateAttribute("Version");
                xa.Value = "1.1";
                xComplementoINE.Attributes.Append(xa);

                xa = xDoc.CreateAttribute("TipoProceso");
                xa.Value = datosFacturaElectronica.IneTipoProceso;
                xComplementoINE.Attributes.Append(xa);

                if (datosFacturaElectronica.IneTipoProceso == "Ordinario")
                {
                    xa = xDoc.CreateAttribute("TipoComite");
                    xa.Value = datosFacturaElectronica.IneTipoComite;
                    xComplementoINE.Attributes.Append(xa);

                    if (datosFacturaElectronica.IneTipoComite == "Ejecutivo Nacional")
                    {
                        xa = xDoc.CreateAttribute("IdContabilidad");
                        xa.Value = datosFacturaElectronica.IneClaveContabilidad;
                        xComplementoINE.Attributes.Append(xa);
                    }

                    xComplemento.AppendChild(xComplementoINE);
                    xComprobante.AppendChild(xComplemento);
                }
                else
                {
                    XmlNode xNodoEntidad = xDoc.CreateNode(XmlNodeType.Element, "ine", "Entidad", "http://www.sat.gob.mx/ine");

                    xa = xDoc.CreateAttribute("ClaveEntidad");
                    xa.Value = datosFacturaElectronica.IneEntidad;
                    xNodoEntidad.Attributes.Append(xa);

                    xa = xDoc.CreateAttribute("Ambito");
                    xa.Value = datosFacturaElectronica.IneAmbito;
                    xNodoEntidad.Attributes.Append(xa);

                    if (datosFacturaElectronica.IneIdContabilidad.Length > 0)
                    {
                        XmlNode xNodoIdContabilidad = xDoc.CreateNode(XmlNodeType.Element, "ine", "Contabilidad", "http://www.sat.gob.mx/ine");

                        xa = xDoc.CreateAttribute("IdContabilidad");
                        xa.Value = datosFacturaElectronica.IneIdContabilidad;
                        xNodoIdContabilidad.Attributes.Append(xa);

                        xNodoEntidad.AppendChild(xNodoIdContabilidad);
                    }

                    xComplementoINE.AppendChild(xNodoEntidad);
                    xComplemento.AppendChild(xComplementoINE);
                    xComprobante.AppendChild(xComplemento);
                }

            }

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

        void ImprimirPDF(Boolean SAT)
        {

            System.IO.Directory.CreateDirectory(@"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", ""));
            String Directorio = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", "");
            string fileName;
            if (SAT)
            {
                fileName = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", "") + @"\fac_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
            }
            else
            {
                fileName = @"C:\Impresos\Facturas\" + datosFacturaElectronica.nombreReceptor.Replace("\"", "") + @"\pre_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + datosFacturaElectronica.fechaExpedicion.Replace("/", "-").Replace(":", "-") + ".pdf";
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
                .Add(new Paragraph("Serie y folio: " + FolioActual.ToString())));

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

            if (FacturaCancelada != null)
            {
                //Renglón especial
                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("UUID factura relacionada por cancelación")));

                table.AddCell(new Cell(1, 6)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(UUIDCancelado)));
            }
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

            if (_clienteElegido.aplica_retencion == "SI")
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

            if (datosFacturaElectronica.IneTipoProceso.Length > 0)
            {
                table.AddCell(new Cell(1, 9)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Complemento INE Tipo Proceso: ")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(fb)
                    .SetFontSize(fs)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(datosFacturaElectronica.IneTipoProceso)));
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
            if (Es40)
            {
                table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .Add(new Paragraph("Este documento es una representación impresa de un CFDI 4.0")));
            }
            else
            {
                table.AddCell(new Cell(1, 10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                .SetFontColor(new DeviceRgb(255, 255, 255))
                .SetFont(fb)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .Add(new Paragraph("Este documento es una representación impresa de un CFDI 3.3")));
            }
            

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

        private void GenerarComplemento40()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(XMLTimbrado);

            XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);
            nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
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
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                using (DbContextTransaction transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        Facturas factura = new Facturas();
                        DetalleFactura detalle = new DetalleFactura();

                        factura.id_cliente = _clienteElegido.id_cliente;
                        factura.id_contribuyente = ((Contribuyentes)cbContribuyentes.SelectedItem).id_contribuyente;
                        factura.subtotal = Subtotal;
                        factura.total = Math.Round(Total, 2);
                        factura.estado = "ACTIVO";
                        factura.fecha = DateTime.Now.ToString().Substring(0, 10);
                        factura.numero = FolioActual.ToString();
                        factura.pagada = "NO";
                        factura.estado = "ACTIVO";
                        factura.razon_cancelado = "";
                        factura.amparada_por = "";
                        factura.usuario = "";

                        dbContext.Facturas.Add(factura);

                        dbContext.SaveChanges();

                        foreach (ConceptoFactura item in _conceptos)
                        {
                            detalle = new DetalleFactura();

                            detalle.id_factura = factura.id_factura;
                            detalle.cantidad = item.Cantidad;
                            detalle.descripcion = item.Descripcion;
                            detalle.importe = item.Importe;                            
                            detalle.precio_unitario = item.PrecioUnitario;
                            detalle.clave_servicio = item.Clave;
                            detalle.unidad = item.Unidad;
                            
                            dbContext.DetalleFactura.Add(detalle);

                            if (item.IdOrden > 0)
                            {
                                dbContext.Adjuntar_Orden_Factura(item.IdOrden, factura.id_factura);
                                dbContext.Modificar_Tipo_Orden(item.IdOrden, "FACTURA");
                            }

                            dbContext.SaveChanges();

                            if (item.IdInsumo > 0)
                            {
                                SalidasInventario nueva = new SalidasInventario();
                                nueva.fecha = DateTime.Now.Date;
                                nueva.presupuesto = 0;
                                nueva.orden_trabajo = "";
                                nueva.nota = "";
                                nueva.cantidad = item.Cantidad;
                                nueva.id_insumo = item.IdInsumo;
                                nueva.descripcion = item.DescripcionInsumo;
                                nueva.factura = factura.id_factura.ToString();

                                dbContext.SalidasInventario.Add(nueva);

                                Insumos insumo = dbContext.Insumos.Where(I => I.id_insumo == item.IdInsumo).First();
                                insumo.stock = insumo.stock - nueva.cantidad;

                                detalle.id_articulo = item.IdInsumo;
                            }
                        }

                        dbContext.SaveChanges();

                        Contribuyentes cont = new Contribuyentes();
                        int idContribuyente = int.Parse(cbContribuyentes.SelectedValue.ToString());

                        cont = dbContext.Contribuyentes.Where(C => C.id_contribuyente == idContribuyente).FirstOrDefault();
                        cont.numero_factura = (int.Parse(cont.numero_factura) + 1).ToString();

                        dbContext.SaveChanges();

                        Entity.FacturaDigital fdigital = new Entity.FacturaDigital();
                        fdigital.id_factura = factura.id_factura;
                        fdigital.xml = XMLTimbrado;
                        if (datosFacturaElectronica.metodoPago == "PUE")
                        {
                            fdigital.para_recibo = "SI";
                        }
                        else
                        {
                            fdigital.para_recibo = "NO";
                        }
                        if (FacturaCancelada == null)
                        {
                            fdigital.para_cancelacion = "NO";
                        }
                        else
                        {
                            fdigital.para_cancelacion = "SI";
                        }
                        fdigital.sello_cfdi = datosFacturaElectronica.selloCFD;
                        fdigital.sello_sat = datosFacturaElectronica.selloSAT;
                        fdigital.cadena_original = datosFacturaElectronica.cadenaOriginalSAT;
                        fdigital.uso_cfdi = datosFacturaElectronica.usoCFDI;
                        fdigital.forma_pago = datosFacturaElectronica.formaPago;

                        dbContext.FacturaDigital.Add(fdigital);

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

        private void cbFormasPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            datosFacturaElectronica.formaPago = cbFormasPago.SelectedValue.ToString();
            datosFacturaElectronica.formaPagoTexto = (e.AddedItems[0] as FormasPago).FormaPago;
        }

        private void cbUsosCFDI_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            datosFacturaElectronica.usoCFDI = cbUsosCFDI.SelectedValue.ToString();
            datosFacturaElectronica.usoCFDITexto = (e.AddedItems[0] as UsosCFDI).Uso;
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
            /*
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
                        //SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                        SmtpClient SmtpServer = new SmtpClient("smtp-mail.outlook.com");
                        //mail.From = new MailAddress("alvarezimpresores_16@hotmail.com");
                        mail.From = new MailAddress(config.correo);

                        mail.To.Add(item.correo);
                        mail.Subject = "Envío de información de facturas - Alvarez Impresores";
                        mail.Body = "Saludos, envío la información de las facturas. Recuerde: después de 72 horas no se pueden cancelar.";

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
                        //SmtpServer.Port = 25;
                        SmtpServer.UseDefaultCredentials = false;
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
            */
            String Respuesta = "";
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    List<Correos> correos = dbContext.Correos.Where(C => C.id_cliente == _clienteElegido.id_cliente).ToList();

                    foreach (Correos item in correos)
                    {
                        var fromAddress = new MailAddress("alvarezimpresoresfacturacion@gmail.com", "Ticket");
                        var toAddress = new MailAddress(item.correo, "-");
                        string fromPassword = "pwiz gpht qqpj pjso ";
                        string subject = "Envío de información de facturas - Alvarez Impresores";
                        string body = "Saludos, envío la información de las facturas. Recuerde: después de 72 horas no se pueden cancelar.";

                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                        };                        
                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = body,
                            IsBodyHtml = false
                        })
                        {
                            if (rutaPDF.Length > 0)
                            {
                                System.Net.Mail.Attachment attachment;
                                attachment = new System.Net.Mail.Attachment(@rutaPDF);
                                message.Attachments.Add(attachment);
                            }
                            if (rutaXML.Length > 0)
                            {
                                System.Net.Mail.Attachment attachment;
                                attachment = new System.Net.Mail.Attachment(@rutaXML);
                                message.Attachments.Add(attachment);
                            }
                            //message.Attachments.Add(new Attachment(@"C:\Motela\reportecancelados.pdf"));
                            smtp.Send(message);

                            MessageBox.Show("Correo enviado a " + item.correo);
                        }
                    }
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        
        }

        private void EnviarPorCorreoTEST()
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Configuracion config = dbContext.Configuracion.Single();
                    MailMessage mail = new MailMessage();
                    //SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
                    SmtpClient SmtpServer = new SmtpClient("smtp-mail.outlook.com");
                    //SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                    //mail.From = new MailAddress("alvarezimpresores_16@hotmail.com");
                    //mail.From = new MailAddress("kradvenko@outlook.com");
                    mail.From = new MailAddress(config.correo);

                    mail.To.Add("kradvenko@gmail.com");
                    mail.Subject = "Envío de información de facturas - Alvarez Impresores";
                    mail.Body = "Saludos, envío la información de las facturas. Recuerde: después de 72 horas no se pueden cancelar.";

                    System.Net.Mail.Attachment attachment;
                    attachment = new System.Net.Mail.Attachment(@"C:\Impresos\nota.xml");
                    mail.Attachments.Add(attachment);

                    SmtpServer.Port = 587;
                    //SmtpServer.Port = 25;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(config.usuario_correo, config.password_correo);
                    //SmtpServer.Credentials = new System.Net.NetworkCredential("kradvenko@outlook.com", "R4t6u7i9");
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mail);
                    MessageBox.Show("Correo enviado.");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnBuscarOrdenes_Click(object sender, RoutedEventArgs e)
        {
            BuscarOrdenes ordenes = new BuscarOrdenes(_clienteElegido.id_cliente, this);
            ordenes.ShowDialog();
        }

        public bool OrdenAgregada(int IdOrden)
        {
            if (dgConceptos.ItemsSource is null)
            {
                return false;
            }
            foreach (ConceptoFactura item in dgConceptos.ItemsSource)
            {
                if (item.IdOrden != null)
                {
                    if (IdOrden == item.IdOrden)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void AgregarOrden(ConceptoFactura nuevaOrden)
        {
            _conceptos.Add(nuevaOrden);
            dgConceptos.ItemsSource = null;
            dgConceptos.ItemsSource = _conceptos;
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

        private void btnComplementoINE_Click(object sender, RoutedEventArgs e)
        {
            ControlComplementoINE ine = new ControlComplementoINE(this, datosFacturaElectronica);
            ine.ShowDialog();
        }

        private void btnCancelacionFactura_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteElegido is null)
            {
                return;
            }
            BuscarFacturaCancelada Buscar = new BuscarFacturaCancelada(this, _clienteElegido);
            Buscar.ShowDialog();
        }

        public void ElegirFacturaCancelada(Entity.FacturaDigital FacturaElegida)
        {
            string[] parts = FacturaElegida.cadena_original.Split('|');
            lblUUIDCancelada.Content = parts[4];
            UUIDCancelado = parts[4];
            FacturaCancelada = FacturaElegida;
        }

        private void btnLimpiarFacturaRelacionada_Click(object sender, RoutedEventArgs e)
        {
            FacturaCancelada = null;
            lblUUIDCancelada.Content = "-";
        }

        private void btnFacturar4_Click(object sender, RoutedEventArgs e)
        {            
            if (MessageBox.Show("Desea facturar en versión 4.0?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            Es40 = true;

            if (cbContribuyentes.Text.Contains("JOSE"))
            {
                rutaCertificado = @"C:\Impresos\Jose\Certificado.cer";
                rutaLlave = @"C:\Impresos\Jose\Llave.key";
                contraseñaLlave = "Musica47*";
                nombreEmisor = "JOSE ALVAREZ JIMENEZ";
                rfcEmisor = "AAJJ470205DH1";
                serie = "-";
                usuarioFacturacion = "JoseAlvarezJi";
                contraseñaFacturacion = "oF5r1o6S3";
                curp = "AAJJ470205HNTLMS00";
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 625 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
                regimen = "Regimen fiscal simplificado de confianza";

                datosFacturaElectronica.domicilioEmisorCalle = "MORELOS 619 PTE";
                datosFacturaElectronica.domicilioEmisorColonia = "HERIBERTO CASAS";
                datosFacturaElectronica.domicilioEmisorMunicipio = "TEPIC ";
                datosFacturaElectronica.domicilioEmisorEstado = "NAY";
                datosFacturaElectronica.domicilioEmisorCodigoPostal = "63080";
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
            regimen = "Regimen fiscal simplificado de confianza";

            datosFacturaElectronica.domicilioEmisorCalle = "";
            datosFacturaElectronica.domicilioEmisorColonia = "";
            datosFacturaElectronica.domicilioEmisorMunicipio = " ";
            datosFacturaElectronica.domicilioEmisorEstado = "";
            datosFacturaElectronica.domicilioEmisorCodigoPostal = "44970";
            */
            if (_clienteElegido.regimen_fiscal == "")
            {
                MessageBox.Show("No se ha registrado el regimen fiscal del cliente.");
                return;
            }

            if (_clienteElegido.nombre_constancia == "")
            {
                MessageBox.Show("No se ha registrado el nombre de la constancia fiscal del cliente.");
                return;
            }

            CargarXML4Template();
            FacturacionElectronica40(true);
            if (timbreValido)
            {
                ImprimirPDF(true);
                GuardarFactura();
                EnviarPorCorreo();
            }
        }

        private void CargarXML4Template()
        {
            if (FacturaCancelada == null)
            {
                XML = File.ReadAllText(@"C:\Impresos\Facturacion\XML_4_0_Template.xml");
            }
            else
            {
                XML = File.ReadAllText(@"C:\Impresos\Facturacion\XML_4_0_Template_Cancelacion.xml");
            }
        }

        private void btnLlenadoCliente4_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteElegido != null)
            {
                Llenado4 llenado = new Llenado4(this, _clienteElegido);
                llenado.ShowDialog();
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

                File.WriteAllText(@"C:\Impresos\Facturacion\XML_4_0.xml", XML);
                
                GenerarXML40();
                
                System.Security.SecureString passwordSeguro = new System.Security.SecureString();
                passwordSeguro.Clear();
                foreach (char c in strLlavePwd.ToCharArray())
                    passwordSeguro.AppendChar(c);
                byte[] llavePrivadaBytes = File.ReadAllBytes(strPathLlave);
                RSACryptoServiceProvider rsa = opensslkey.DecodeEncryptedPrivateKeyInfo(llavePrivadaBytes, passwordSeguro);
                SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();
                
                CadenaOriginal = GenerarCadenaOriginal40();
                
                byte[] bytesFirmados = rsa.SignData(Encoding.UTF8.GetBytes(CadenaOriginal), hasher);
                strSello = Convert.ToBase64String(bytesFirmados);
                
                XML = XML.Replace("Sello=\"\"", "Sello=\"" + strSello + "\"");

                File.WriteAllText(@"C:\Impresos\XML_4_0.xml", XML);

                Directory.CreateDirectory(@"C:\Impresos\Facturas\" + _clienteElegido.nombre.Replace("\"", ""));
                string fileName = @"C:\Impresos\Facturas\" + _clienteElegido.nombre.Replace("\"", "") + @"\pre_" + cbContribuyentes.SelectedValue.ToString() + "_" + FolioActual + "_" + fechaFactura.Replace("/", "-").Replace(":", "-") + ".xml";

                File.WriteAllText(fileName, XML);

            }
            catch (Exception exc)
            {
                MessageBox.Show("Error: " + exc.Message);
            }

            if (bTimbrar)
            {
                Timbrar40();
            }
        }

        private void GenerarXML40()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(XML);

            XmlNamespaceManager nms = new XmlNamespaceManager(xDoc.NameTable);
            nms.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
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
            if (cbMetodoPago.SelectedIndex == 0)
            {
                datosFacturaElectronica.metodoPago = "PUE";
            }
            else
            {
                datosFacturaElectronica.metodoPago = "PPD";
            }
            xAttrib.Value = datosFacturaElectronica.metodoPago;
            datosFacturaElectronica.metodoPago = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@SubTotal", nms);
            xAttrib.Value = AddDecimals(Subtotal.ToString());
            datosFacturaElectronica.subTotal = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@Total", nms);
            xAttrib.Value = AddDecimals(Math.Round(Total, 2).ToString());
            datosFacturaElectronica.total = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@LugarExpedicion", nms);
            xAttrib.Value = datosFacturaElectronica.domicilioEmisorCodigoPostal;
            datosFacturaElectronica.lugarExpedicion = "Tepic, Nay.";

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Comprobante//@NoCertificado", nms);
            datosFacturaElectronica.numeroCertificado = xAttrib.Value;

            if (FacturaCancelada != null)
            {
                XmlNode xCfdiRelacionados = xDoc.SelectSingleNode("//cfdi:CfdiRelacionados", nms);
                XmlNode xCfdiRelacionado = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "CfdiRelacionado", "http://www.sat.gob.mx/cfd/4");

                XmlAttribute xR = xDoc.CreateAttribute("UUID");
                xR.Value = UUIDCancelado;
                xCfdiRelacionado.Attributes.Append(xR);

                xCfdiRelacionados.AppendChild(xCfdiRelacionado);
            }

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Rfc", nms);
            xAttrib.Value = rfcEmisor;
            datosFacturaElectronica.rfcEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@Nombre", nms);
            xAttrib.Value = nombreEmisor;
            datosFacturaElectronica.nombreEmisor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Emisor//@RegimenFiscal", nms);
            xAttrib.Value = "626";

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@Rfc", nms);
            xAttrib.Value = _clienteElegido.rfc.Replace("-", "");
            datosFacturaElectronica.rfcReceptor = xAttrib.Value;
            //PRUEBAS
            //xAttrib.Value = "FUNK671228PH6";
            //datosFacturaElectronica.rfcReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@Nombre", nms);
            //xAttrib.Value = _clienteElegido.nombre;
            xAttrib.Value = _clienteElegido.nombre_constancia;
            datosFacturaElectronica.nombreReceptor = xAttrib.Value;
            //PRUEBAS
            //xAttrib.Value = "KARLA FUENTE NOLASCO";
            //datosFacturaElectronica.nombreReceptor = xAttrib.Value;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@UsoCFDI", nms);
            xAttrib.Value = datosFacturaElectronica.usoCFDI;

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@RegimenFiscalReceptor", nms);
            xAttrib.Value = _clienteElegido.regimen_fiscal;

            datosFacturaElectronica.regimenFiscalReceptor = xAttrib.Value;

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                datosFacturaElectronica.regimenFiscalReceptor = dbContext.RegimenFiscal.Where(C => C.Clave == datosFacturaElectronica.regimenFiscalReceptor).First().Descripcion;
            }
            //PRUEBAS
            //xAttrib.Value = "612";

            xAttrib = (XmlAttribute)xDoc.SelectSingleNode("//cfdi:Receptor//@DomicilioFiscalReceptor", nms);
            xAttrib.Value = _clienteElegido.codigo_postal;
            //PRUEBAS
            //xAttrib.Value = "44970";

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
            float baseTotal = 0;
            float impuestoRetencion = 0;
            float impuestoRetencionTotal = 0;
            float baseTotalRetencion = 0; 

            datosFacturaElectronica.conceptos = new List<ConceptoFactura>();

            XmlAttribute xa;

            foreach (ConceptoFactura item in _conceptos)
            {
                if (item != null)
                {
                    ConceptoFactura cConcepto = new ConceptoFactura();

                    XmlNode xConcepto = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Concepto", "http://www.sat.gob.mx/cfd/4");

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

                    xa = xDoc.CreateAttribute("ObjetoImp");
                    xa.Value = "02";
                    xConcepto.Attributes.Append(xa);
                    cConcepto.ObjetoImp = xa.Value;

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
                    //float.Parse((Math.Round(impuesto, 2)).ToString());
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

                    //IVA
                    impuesto = item.Cantidad * item.PrecioUnitario;
                    impuesto = impuesto * 0.16f;
                    impuesto = float.Parse((Math.Round(impuesto, 2)).ToString());
                    impuestoTotal = impuestoTotal + impuesto;
                    baseTotal = baseTotal + item.Importe;

                    //RETENCION
                    impuestoRetencion = item.Cantidad * item.PrecioUnitario;
                    impuestoRetencion = impuestoRetencion * 0.012500f;
                    impuestoRetencion = float.Parse((Math.Round(impuestoRetencion, 2)).ToString());
                    impuestoRetencionTotal = impuestoRetencionTotal + impuestoRetencion;
                    baseTotalRetencion = baseTotalRetencion + item.Importe;

                    XmlNode xImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Impuestos", "http://www.sat.gob.mx/cfd/4");

                    XmlNode xTraslados = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslados", "http://www.sat.gob.mx/cfd/4");

                    XmlNode xTraslado = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslado", "http://www.sat.gob.mx/cfd/4");                    

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

                    if (_clienteElegido.aplica_retencion == "SI")
                    {
                        XmlNode xRetenciones = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/4");

                        XmlNode xRetencion = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retencion", "http://www.sat.gob.mx/cfd/4");

                        xa = xDoc.CreateAttribute("Base");
                        xa.Value = item.Importe.ToString();
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("Impuesto");
                        xa.Value = "001";
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("TipoFactor");
                        xa.Value = "Tasa";
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("TasaOCuota");
                        xa.Value = "0.012500";
                        xRetencion.Attributes.Append(xa);

                        xa = xDoc.CreateAttribute("Importe");
                        xa.Value = AddDecimals(impuestoRetencion.ToString());
                        xRetencion.Attributes.Append(xa);

                        xRetenciones.AppendChild(xRetencion);
                        xImpuestos.AppendChild(xRetenciones);
                    }

                    xConcepto.AppendChild(xImpuestos);
                    xConceptos.AppendChild(xConcepto);

                }

            }

            XmlNode xTrasladosImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslados", "http://www.sat.gob.mx/cfd/4");

            XmlNode xTrasladoImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Traslado", "http://www.sat.gob.mx/cfd/4");

            XmlNode xRetencionesImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/4");

            XmlNode xRetencionImpuestos = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Retencion", "http://www.sat.gob.mx/cfd/4");

            if (_clienteElegido.aplica_retencion == "SI")
            {
                /*
                xa = xDoc.CreateAttribute("Base");
                xa.Value = baseTotal.ToString();
                xRetencionImpuestos.Attributes.Append(xa);
                */
                xa = xDoc.CreateAttribute("Impuesto");
                xa.Value = "001";
                /*
                xRetencionImpuestos.Attributes.Append(xa);
                xa = xDoc.CreateAttribute("TipoFactor");
                xa.Value = "Tasa";
                */
                /*
                xTrasladoImpuestos.Attributes.Append(xa);
                xa = xDoc.CreateAttribute("TasaOCuota");
                xa.Value = "0.012500";
                */
                xRetencionImpuestos.Attributes.Append(xa);
                xa = xDoc.CreateAttribute("Importe");
                impuestoRetencionTotal = float.Parse((Math.Round(impuestoRetencionTotal, 2)).ToString());
                xa.Value = AddDecimals(impuestoRetencionTotal.ToString());
                xRetencionImpuestos.Attributes.Append(xa);

                xRetencionesImpuestos.AppendChild(xRetencionImpuestos);

                xImpuestosNodo.AppendChild(xRetencionesImpuestos);

                xa = xDoc.CreateAttribute("TotalImpuestosRetenidos");
                xa.Value = AddDecimals(impuestoRetencionTotal.ToString());
                xImpuestosNodo.Attributes.Append(xa);

                //RetencionIsr = impuestoRetencionTotal;
            }
            
            xa = xDoc.CreateAttribute("Base");
            xa.Value = baseTotal.ToString();
            xTrasladoImpuestos.Attributes.Append(xa);
            
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

            if (datosFacturaElectronica.IneTipoProceso.Length > 0)
            {
                XmlNode xINEschema = xDoc.SelectSingleNode("//cfdi:Comprobante", nms);
                xa = xINEschema.Attributes["xsi:schemaLocation"];
                xa.Value = xa.Value + " http://www.sat.gob.mx/ine http://www.sat.gob.mx/sitio_internet/cfd/ine/ine11.xsd";

                xa = xDoc.CreateAttribute("xmlns:ine");
                xa.Value = "http://www.sat.gob.mx/ine";
                xINEschema.Attributes.Append(xa);

                XmlNode xComprobante = xDoc.SelectSingleNode("//cfdi:Comprobante", nms);
                XmlNode xComplemento;

                xComplemento = xDoc.CreateNode(XmlNodeType.Element, "cfdi", "Complemento", "http://www.sat.gob.mx/cfd/4");

                XmlNode xComplementoINE = xDoc.CreateNode(XmlNodeType.Element, "ine", "INE", "http://www.sat.gob.mx/ine");

                xa = xDoc.CreateAttribute("Version");
                xa.Value = "1.1";
                xComplementoINE.Attributes.Append(xa);

                xa = xDoc.CreateAttribute("TipoProceso");
                xa.Value = datosFacturaElectronica.IneTipoProceso;
                xComplementoINE.Attributes.Append(xa);

                if (datosFacturaElectronica.IneTipoProceso == "Ordinario")
                {
                    xa = xDoc.CreateAttribute("TipoComite");
                    xa.Value = datosFacturaElectronica.IneTipoComite;
                    xComplementoINE.Attributes.Append(xa);

                    if (datosFacturaElectronica.IneTipoComite == "Ejecutivo Nacional")
                    {
                        xa = xDoc.CreateAttribute("IdContabilidad");
                        xa.Value = datosFacturaElectronica.IneClaveContabilidad;
                        xComplementoINE.Attributes.Append(xa);
                    }

                    xComplemento.AppendChild(xComplementoINE);
                    xComprobante.AppendChild(xComplemento);
                }
                else
                {
                    XmlNode xNodoEntidad = xDoc.CreateNode(XmlNodeType.Element, "ine", "Entidad", "http://www.sat.gob.mx/ine");

                    xa = xDoc.CreateAttribute("ClaveEntidad");
                    xa.Value = datosFacturaElectronica.IneEntidad;
                    xNodoEntidad.Attributes.Append(xa);

                    xa = xDoc.CreateAttribute("Ambito");
                    xa.Value = datosFacturaElectronica.IneAmbito;
                    xNodoEntidad.Attributes.Append(xa);

                    if (datosFacturaElectronica.IneIdContabilidad.Length > 0)
                    {
                        XmlNode xNodoIdContabilidad = xDoc.CreateNode(XmlNodeType.Element, "ine", "Contabilidad", "http://www.sat.gob.mx/ine");

                        xa = xDoc.CreateAttribute("IdContabilidad");
                        xa.Value = datosFacturaElectronica.IneIdContabilidad;
                        xNodoIdContabilidad.Attributes.Append(xa);

                        xNodoEntidad.AppendChild(xNodoIdContabilidad);
                    }

                    xComplementoINE.AppendChild(xNodoEntidad);
                    xComplemento.AppendChild(xComplementoINE);
                    xComprobante.AppendChild(xComplemento);
                }

            }

            XML = xDoc.InnerXml;
            //XML = XML.Replace("&amp;", "&");

            File.WriteAllText(@"C:\Impresos\Facturacion\XML_4_0.xml", XML);
        }

        private String GenerarCadenaOriginal40()
        {
            String cadenaOriginal = "";

            try
            {
                //Cargar el XML
                StreamReader reader = new StreamReader(@"C:\Impresos\Facturacion\XML_4_0.xml");
                XPathDocument myXPathDoc = new XPathDocument(reader);
                
                //Cargando el XSLT
                XslCompiledTransform myXslTrans = new XslCompiledTransform();
                myXslTrans.Load(@"C:\Impresos\Facturacion\XLS_4_0.xslt");
                //myXslTrans.Load(@"C:\Impresos\cadenaoriginal_3_2.xslt");
                
                

                StringWriter str = new StringWriter();
                XmlTextWriter myWriter = new XmlTextWriter(str);

                //Aplicando transformacion
                myXslTrans.Transform(myXPathDoc, null, myWriter);
                
                //Resultado
                cadenaOriginal = str.ToString();
                cadenaOriginal = cadenaOriginal.Replace("\n", "").Replace("\r", "");
                cadenaOriginal = cadenaOriginal.Replace("&amp;", "&");

            }
            catch (Exception exc)
            {
                cadenaOriginal = exc.Message;
                MessageBox.Show(exc.InnerException.Message + " - " + exc.Message);
            }

            return cadenaOriginal;
        }

        public void CargarDatosCliente()
        {
            try
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    _clientes = dbContext.Clientes.ToList();
                    _clienteElegido = dbContext.Clientes.Where(T => T.id_cliente == _clienteElegido.id_cliente).First();
                    tbClientes.AutoCompleteSource = _clientes;
                    tbClientes.SelectedItem = _clienteElegido;
                }
                lblNombre.Content = _clienteElegido.nombre;
                lblRFC.Content = _clienteElegido.rfc;
                lblDomicilio.Content = _clienteElegido.domicilio + " " + _clienteElegido.colonia + " " + _clienteElegido.ciudad;
                lblNombreConstancia.Content = _clienteElegido.nombre_constancia;
                lblRegimenFiscalReceptor.Content = _clienteElegido.regimen_fiscal;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void cbAplicaRetencion_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteElegido != null)
            {
                if (_clienteElegido.aplica_retencion == "SI")
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        Clientes c = dbContext.Clientes.Where(T => T.id_cliente == _clienteElegido.id_cliente).First();
                        c.aplica_retencion = "NO";
                        dbContext.SaveChanges();

                        _clienteElegido.aplica_retencion = "NO";
                    }
                }
                else
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        Clientes c = dbContext.Clientes.Where(T => T.id_cliente == _clienteElegido.id_cliente).First();
                        c.aplica_retencion = "SI";
                        dbContext.SaveChanges();

                        _clienteElegido.aplica_retencion = "SI";
                    }
                }
            }
        }

        private void tbIVAMod_KeyUp(object sender, KeyEventArgs e)
        {
            /*
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (tbIVAMod.Text.Length > 3)
                    {
                        CalcularTotal();
                    }
                    else
                    {
                        tbIVAMod.Text = "";
                        CalcularTotal();
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
            */
        }

        private float Redondear(float Numero)
        {
            String NString = Numero.ToString();
            String n = "";
            float NuevoNumero = Numero;

            if (NString.Contains("."))
            {
                int found = NString.IndexOf(".");
                if (NString.Substring(found).Length > 2)
                {
                    String DString = NString.Substring(found + 1);
                    String Nueva = "";
                    bool SumaUno = false;
                    int VActual = 0;
                    for (int i = DString.Length - 1; i >= 0; i--)
                    {
                        VActual = int.Parse(DString.Substring(i, 1));
                        if (SumaUno)
                        {
                            VActual++;
                            SumaUno = false;
                        }
                        if (VActual >= 5 && i > 1)
                        {
                            VActual = 0;
                            SumaUno = true;
                        }
                        Nueva = Nueva.Insert(0, VActual.ToString());
                    }
                    if (int.Parse(Nueva) == 0)
                    {
                        NuevoNumero = Numero + 1;
                    }
                    else
                    {
                        Nueva = Nueva.Substring(0, 2);
                        NuevoNumero = float.Parse(String.Concat(NString.Substring(0, found), ".", Nueva));
                    }
                }
            }
            return NuevoNumero;
        }

        private void tbFacturaCancelada_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int f;
                if (int.TryParse(tbFacturaCancelada.Text, out f))
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        try
                        {
                            String nofactura = tbFacturaCancelada.Text;

                            if (cbContribuyentes.SelectedItem == null)
                            {
                                MessageBox.Show("No ha elegido un contribuyente.");
                                return;
                            }
                            Contribuyentes con = (Contribuyentes)cbContribuyentes.SelectedItem;

                            Entity.Facturas fact = dbContext.Facturas.Where(F => F.numero == nofactura && F.id_contribuyente == con.id_contribuyente).First();


                            Entity.FacturaDigital fac = dbContext.FacturaDigital.Where(F => F.id_factura == fact.id_factura).First();

                            if (fac != null)
                            {
                                ElegirFacturaCancelada(fac);
                            }
                            else
                            {
                                MessageBox.Show("No se ha encontrado la factura.");
                            }
                            
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.Message);
                        }
                    }
                }
            }
        }
    }
}
