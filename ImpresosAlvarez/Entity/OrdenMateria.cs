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
    
    public partial class OrdenMateria
    {
        public int id_ordenmateria { get; set; }
        public int id_orden { get; set; }
        public int id_materia { get; set; }
        public Nullable<double> cantidad { get; set; }
        public string unidad { get; set; }
        public string fecha_solicitado { get; set; }
        public string hora_solicitado { get; set; }
        public string notas { get; set; }
    }
}
