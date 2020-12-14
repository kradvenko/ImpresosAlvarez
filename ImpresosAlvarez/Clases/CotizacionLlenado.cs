using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class CotizacionLlenado
    {
        public int IdOrden { get; set; }
        public int Cantidad { get; set; }
        public String Descripcion { get; set; }
        public double PrecioUnitario { get; set; }
        public double Importe { get; set; }

        public CotizacionLlenado()
        {

        }
    }
}
