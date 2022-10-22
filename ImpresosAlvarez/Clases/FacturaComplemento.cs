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
        public String SubTotal { get; set; }
        public String Total { get; set; }
        public String Pagada { get; set; }
        public String IVA { get; set; }

        public FacturaComplemento()
        {

        }
        public FacturaComplemento(int IdFactura, int IdCliente, int IdContribuyente, String Numero, String Estado, String XML, String SubTotal, String Total, String Pagada, String IVA)
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
        }

    }
}
