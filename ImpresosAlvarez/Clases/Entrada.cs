using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class Entrada
    {
        public DateTime Fecha { get; set; }
        public String Proveedor { get; set; }
        public String Factura { get; set; }
        public String Cantidad { get; set; }
        public String NombreInsumo { get; set; }
        public int IdInsumo { get; set; }

        public Entrada()
        { 

        }

    }
}
