using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class ComplementoPagoData
    {
        public String IdParcialidad { get; set; }
        public String UUID { get; set; }
        public String Serie { get; set; }
        public String Folio { get; set; }
        public String Parcialidad { get; set; }
        public String SaldoAnterior { get; set; }
        public String Pagado { get; set; }
        public String SaldoInsoluto { get; set; }
        public String IdFactura { get; set; }
        public String ImportePagado { get; set; }
        //
        public String FechaPago { get; set; }
        public String FormaPago { get; set; }
        public String MonedaPago { get; set; }
        public String MontoPago { get; set; }
        public String IdDocto { get; set; }
        public String MetodoPagoDR { get; set; }
        public String IvaDR { get; set; }
        public String ISR { get; set; }
        public ComplementoPagoData()
        {

        }
        public ComplementoPagoData(String UUID, String Serie, String Folio, String Parcialidad, String SaldoAnterior, String Pagado, String SaldoInsoluto, String IdFactura, String IvaDR, String ISR)
        {
            this.UUID = UUID;
            this.Serie = Serie;
            this.Folio = Folio;
            this.Parcialidad = Parcialidad;
            this.SaldoAnterior = SaldoAnterior;
            this.Pagado = Pagado;
            this.SaldoInsoluto = SaldoInsoluto;
            this.IdFactura = IdFactura;
            this.IvaDR = IvaDR;
            this.ISR = ISR;
        }
    }
}
