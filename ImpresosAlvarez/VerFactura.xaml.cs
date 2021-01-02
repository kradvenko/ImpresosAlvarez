using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using ImpresosAlvarez.mx.facturacfdi.v331;
using System;
using System.Collections.Generic;
using System.IO;
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
        public VerFactura(ControlFacturas Parent, Facturas Factura)
        {
            InitializeComponent();
            this._parent = Parent;
            this._factura = Factura;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
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
                    c.PrecioUnitario = c.Importe / c.Cantidad;
                    /*
                    c.Unidad = item.Unidad;
                    c.Clave = item.ClaveServicio;
                    */
                    _conceptos.Add(c);
                }
                dgConceptos.ItemsSource = _conceptos;

                lblTotal.Content = "$ " + Math.Round((double)_factura.total, 2).ToString();
                lblEstado.Content = _factura.estado;

                datosFacturaElectronica.metodoPago = "PUE";

                datosFacturaDigital = dbContext.FacturaDigital.Where(FD => FD.id_factura == _factura.id_factura).FirstOrDefault();
            }
        }

        private void bntCancelarFactura_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Desea cancelar la factura?", "ATENCIÓN", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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

                string usuario = usuarioFacturacion;
                string pass = contraseñaFacturacion;
                string strPathLlave = rutaLlave;
                string strPathCert = rutaCertificado;
                string[] folios = new string[1];

                wsCancelacionResponse resCancelacion = new wsCancelacionResponse();
                WSCancelacionService serCancel = new WSCancelacionService();

                try
                {
                    accesos acc = new accesos();
                    acc.usuario = usuario;
                    acc.password = pass;

                    byte[] llavePublicaBytes = File.ReadAllBytes(rutaCertificado);
                    byte[] llavePrivadaBytes = File.ReadAllBytes(strPathLlave);

                    string[] cadenaOriginal = datosFacturaDigital.cadena_original.Split('|');

                    folios[0] = cadenaOriginal[4];

                    string fecha = DateTime.UtcNow.AddHours(-7).ToString("s");

                    resCancelacion = serCancel.Cancelacion_1(rfcEmisor, fecha, folios, llavePublicaBytes, llavePrivadaBytes, contraseñaLlave, acc);

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

                                dbContext.SaveChanges();
                                _parent.ActualizarLista();
                                this.Close();
                            }
                            MessageBox.Show("Se ha cancelado la factura.");
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
                            break;
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
