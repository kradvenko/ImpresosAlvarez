using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class ProductoServicioAutocomplete
    {
        public int IdServicio { get; set; }
        public String Clave { get; set; }
        public String Descripcion { get; set; }
        public String AutoCompleteText { get; set; }
    }
}
