//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImpresosAlvarez.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class Terminado
    {
        public int id_terminado { get; set; }
        public int id_orden { get; set; }
        public int id_usuario { get; set; }
        public string fecha_inicio { get; set; }
        public string hora_inicio { get; set; }
        public string fecha_fin { get; set; }
        public string hora_fin { get; set; }
        public string tipo_terminado { get; set; }
        public string descripcion { get; set; }
    
        public virtual Usuarios Usuarios { get; set; }
        public virtual Ordenes Ordenes { get; set; }
    }
}
