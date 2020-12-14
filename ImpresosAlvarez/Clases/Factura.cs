using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class Factura
    {
        //Datos del Emisor de la factura
        public String nombreEmisor;
        public String domicilioEmisorCalle;
        public String domicilioEmisorNumeroExterior;
        public String domicilioEmisorColonia;
        public String domicilioEmisorMunicipio;
        public String domicilioEmisorEstado;
        public String domicilioEmisorCodigoPostal;
        public String rfcEmisor;
        //Datos del Receptor de la factura
        public String nombreReceptor;
        public String domicilioReceptorCalle;
        public String domicilioReceptorNumeroExterior;
        public String domicilioReceptorColonia;
        public String domicilioReceptorMunicipio;
        public String domicilioReceptorEstado;
        public String domicilioReceptorCodigoPostal;
        public String rfcReceptor;
        //Datos de la Factura Electrónica
        public String fechaExpedicion;
        public String serie;
        public String folio;
        public String lugarExpedicion;
        public String formaPago;
        public String metodoPago;
        public String subTotal;
        public String total;
        public String uuid;
        public String fechaTimbrado;
        public String selloCFD;
        public String selloSAT;
        public String numeroCertificado;
        public String numeroCertificadoSAT;
        public String cadenaOriginalSAT;
        public String usoCFDI;
        //Conceptos de la Factura Electrónica
        public List<ConceptoFactura> conceptos;
        //Impuestos
        public String derechosRPP;
        public String otrosDerechos;
        public String adquisicion;
        public String iva;
        public String retencionIva;
        public String retencionIsr;
        public String retencionCedular;
        public String honorarios;
        //Más datos
        public String escritura;
        public String predial;
        public String usoCFDITexto;
        public String formaPagoTexto;
        public String metodoPagoTexto;

        //public String totalImpuestos;
        //Complemento enajenación
        public String numeroDocumento;
        public DateTime fechaDocumento;
        public String montoDocumento;
        public String totalDocumento;

        public Factura()
        {

        }
    }
}
