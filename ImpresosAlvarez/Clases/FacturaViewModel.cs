using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{    
    public class FacturaViewModel
    {
        public int id_factura { get; set; }
        public int id_cliente { get; set; }
        public int id_contribuyente { get; set; }
        public decimal subtotal { get; set; }
        public decimal total { get; set; }
        public string pagada { get; set; }
        public string estado { get; set; }
        public string fecha { get; set; }
        public string numero { get; set; }
        public string nombre { get; set; }
        public string NombreContribuyente { get; set; }
        public int id_entrega { get; set; }
        public string entrego { get; set; }
        public string referencia { get; set; }
        public string observaciones { get; set; }
    }
}
