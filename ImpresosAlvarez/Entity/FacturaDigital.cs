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
    
    public partial class FacturaDigital
    {
        public int id_factura_digital { get; set; }
        public int id_factura { get; set; }
        public string xml { get; set; }
        public string para_recibo { get; set; }
        public string acuse { get; set; }
        public Nullable<System.DateTime> fecha_cancelado { get; set; }
        public string sello_cfdi { get; set; }
        public string sello_sat { get; set; }
        public string cadena_original { get; set; }
        public string forma_pago { get; set; }
        public string uso_cfdi { get; set; }
        public string ine_tipo_proceso { get; set; }
        public string ine_tipo_comite { get; set; }
        public string ine_clave_contabilidad { get; set; }
        public string ine_entidad { get; set; }
        public string ine_ambito { get; set; }
        public string ine_id_contabilidad { get; set; }
    }
}
