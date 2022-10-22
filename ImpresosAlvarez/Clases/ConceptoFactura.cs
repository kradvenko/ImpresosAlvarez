using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class ConceptoFactura
    {
        public int IdServicio { get; set; }
        public int Cantidad { get; set; }
        public String Descripcion { get; set; }
        public float PrecioUnitario { get; set; }
        public float Importe { get; set; }
        public String Unidad { get; set; }
        public String Clave { get; set; }
        public ProductosServicios Servicio { get; set; }
        public  int IdOrden { get; set; }
        public int IdInsumo { get; set; }
        public String DescripcionInsumo { get; set; }
        public String ObjetoImp { get; set; }


        public String cantidad;
        public String unidad;
        public String descripcionProducto;
        public String valorUnitario;
        public String importe;
        public String claveProductoServicio;
        public String claveUnidad;
        public String noIdentificacion;
        public String descripcionConcepto;
        public String descuento;
        //Para el complemento
        public String fechaPago;
        public String formaPago;
        public String monedaPago;
        public String montoPago;
        public String idDocto;
        public String serie;
        public String folio;
        public String metodoPagoDR;
        public String parcialidad;
        public String saldoAnterior;
        public String importePagado;
        public String saldoInsoluto;
        public String idFactura;

        public ConceptoFactura()
        {

        }
    }
}
