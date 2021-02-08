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
    /// Lógica de interacción para VerComplemento.xaml
    /// </summary>
    public partial class VerComplemento : Window
    {
        Facturas _factura;
        ControlComplementos _parent;

        List<Parcialidades> _conceptos;
        List<Contribuyentes> _contribuyentes;
        List<Parcialidades> _detalle;

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
        public VerComplemento(ControlComplementos Parent, Facturas Factura)
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

                _detalle = dbContext.Parcialidades.Where(D => D.id_factura == _factura.id_factura).ToList();

                _conceptos = new List<Parcialidades>();
                /*
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
                */
                dgConceptos.ItemsSource = _detalle;
                lblTotal.Content = "$ " + Math.Round((double)_factura.total, 2).ToString();
                lblEstado.Content = _factura.estado;

                datosFacturaElectronica.metodoPago = "PUE";

                datosFacturaDigital = dbContext.FacturaDigital.Where(FD => FD.id_factura == _factura.id_factura).FirstOrDefault();
            }
        }

        private void btnCancelarComplemento_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Desea cancelar el complemento?", "ATENCIÓN", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                    Parcialidades comp = _detalle[0];

                    accesos acc = new accesos();
                    acc.usuario = usuario;
                    acc.password = pass;

                    byte[] llavePublicaBytes = File.ReadAllBytes(rutaCertificado);
                    byte[] llavePrivadaBytes = File.ReadAllBytes(strPathLlave);

                    if (comp.cadena_original == null)
                    {
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            Parcialidades del = dbContext.Parcialidades.Where(P => P.id_parcialidad == comp.id_parcialidad).FirstOrDefault();

                            dbContext.Parcialidades.Remove(del);

                            dbContext.SaveChanges();

                            _parent.ActualizarLista();
                            MessageBox.Show("Se ha eliminado el complemento.");
                            this.Close();
                            return;
                        }
                    }
                    string[] cadenaOriginal = comp.cadena_original.Split('|');

                    folios[0] = cadenaOriginal[4];

                    string fecha = DateTime.UtcNow.AddHours(-7).ToString("s");

                    resCancelacion = serCancel.Cancelacion_1(rfcEmisor, fecha, folios, llavePublicaBytes, llavePrivadaBytes, contraseñaLlave, acc);

                    File.WriteAllText(@"C:\Impresos\Complementos\Acuse.xml", resCancelacion.acuse);

                    switch (resCancelacion.folios[0].estatusUUID)
                    {
                        case "201":
                            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                            {
                                foreach (Parcialidades item in _detalle)
                                {
                                    Parcialidades p = dbContext.Parcialidades.Where(P => P.id_parcialidad == item.id_parcialidad).FirstOrDefault();
                                    p.acuse = resCancelacion.acuse;
                                    p.fecha_cancelado = DateTime.UtcNow.AddHours(-7);

                                    p.estado = "CANCELADO";
                                }

                                dbContext.SaveChanges();
                                _parent.ActualizarLista();
                                MessageBox.Show("Se ha cancelado el complemento.");
                                this.Close();
                            }
                            break;
                        case "202":
                            MessageBox.Show("El complemento ya ha sido cancelado anteriormente.");
                            break;
                        case "203":
                            MessageBox.Show("El complemento no se ha encontrado.");
                            break;
                        case "204":
                            MessageBox.Show("El complemento no se puede cancelar.");
                            break;
                        case "205":
                            MessageBox.Show("El complemento no existe.");
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
