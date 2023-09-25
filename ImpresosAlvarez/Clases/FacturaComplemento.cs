using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class FacturaComplemento
    {
        public int IdFactura { get; set; }
        public int IdCliente { get; set; }
        public int IdContribuyente { get; set; }
        public String Numero { get; set; }
        public String Estado { get; set; }
        public String XML { get; set; }
        public float SubTotal { get; set; }
        public float Total { get; set; }
        public String Pagada { get; set; }
        public float IVA { get; set; }
        public String IdFormaPagoStr { get; set; }
        public int IdFormaPago { get; set; }
        public String FormaPago { get; set; }

        public FacturaComplemento()
        {

        }
        public FacturaComplemento(int IdFactura, int IdCliente, int IdContribuyente, String Numero, String Estado, String XML, float SubTotal, float Total, String Pagada, float IVA, String IdFormaPagoStr, int IdFormaPago, String FormaPago)
        {
            this.IdFactura = IdFactura;
            this.IdCliente = IdCliente;
            this.IdContribuyente = IdContribuyente;
            this.Numero = Numero;
            this.Estado = Estado;
            this.XML = XML;
            this.SubTotal = SubTotal;
            this.Total = Total;
            this.Pagada = Pagada;
            this.IVA = IVA;
            this.IdFormaPagoStr = IdFormaPagoStr;
            this.IdFormaPago = int.Parse(IdFormaPagoStr);
            this.FormaPago = FormaPago;
        }

    }
}
