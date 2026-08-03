using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class VentaDirectaItem
    {
        public int id_insumo { get; set; }
        public string descripcion { get; set; }
        public double precio { get; set; }
        public int cantidad { get; set; }
        public double total { get; set; }

        public VentaDirectaItem(int id_insumo, string descripcion, double precio, int cantidad)
        {
            this.id_insumo = id_insumo;
            this.descripcion = descripcion;
            this.precio = precio;
            this.cantidad = cantidad;
            this.total = precio * cantidad;
        }
    }
}
