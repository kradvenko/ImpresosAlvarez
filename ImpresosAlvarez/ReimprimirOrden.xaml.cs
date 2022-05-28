using ImpresosAlvarez.Entity;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para ReimprimirOrden.xaml
    /// </summary>
    public partial class ReimprimirOrden : Window
    {
        public ReimprimirOrden()
        {
            InitializeComponent();
        }

        private void btnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (tbNumeroOrden.Text.Length > 0)
            {
                ImprimirPDF(tbNumeroOrden.Text);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void ImprimirPDF(String NumeroOrden)
        {
            int PExc = 1;
            try
            {
                Ordenes OrdenElegida;
                Clientes ClienteOrden;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    OrdenElegida = dbContext.Ordenes.Where(O => O.numero.ToString() == NumeroOrden).First();

                    if (OrdenElegida == null)
                    {
                        return;
                    }
                    else
                    {
                        ClienteOrden = dbContext.Clientes.Where(C => C.id_cliente == OrdenElegida.id_cliente).First();
                    }
                }

                String rutaPDF = "";

                Document document = null;

                System.IO.Directory.CreateDirectory(@"C:\Impresos\Ordenes");
                rutaPDF = @"C:\Impresos\Ordenes\Orden_" + NumeroOrden + ".pdf";

                //PdfDocument pdf = new PdfDocument(new PdfReader(@"AlvarezCotizacionL.pdf"), new PdfWriter(rutaPDF));
                PdfDocument pdf = new PdfDocument(new PdfWriter(rutaPDF));
                document = new Document(pdf, PageSize.LETTER);

                document.SetMargins(10, 10, 10, 10);

                float[] columnWidths = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
                PdfFont f = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                float fs = 9;
                PExc++;
                //PRIMER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden de trabajo")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Recibe")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.quien_recibio)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.prioridad)));

                //SEGUNDO RENGLON
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Nombre de la empresa")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ClienteOrden.pseudonimo)));
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Teléfono")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.telefono)));
                PExc++;
                //TERCER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Solicitante")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.solicitante)));
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha solicita")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_solicita)));
                PExc++;
                //CUARTO RENGLON                
                PExc++;
                table.AddCell(new Cell(1, 5)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                PExc++;
                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //QUINTO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Descripción")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.nombre_trabajo)));

                PExc++;
                //SEXTO RENGLON
                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Cantidad")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Color de tintas")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.color_tintas)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Papel")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tipo_papel)));

                PExc++;
                //SEPTIMO RENGLON                
                
                PExc++;
                //OCTAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Con folio")));

                String ConFolio = "Si No [X]";
                if (OrdenElegida.con_folio == "SI")
                {
                    ConFolio = "Si [X] No";
                }

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ConFolio)));

                String Folios = "";
                if (OrdenElegida.con_folio == "SI")
                {
                    Folios = "Del " + OrdenElegida.del_numero + " Al " + OrdenElegida.al_numero;
                }
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Folios)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden anterior")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.orden_anterior)));
                /*
                table.AddCell(new Cell(2, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));
                */

                //NOVENO RENGLON
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tamaño del papel")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tamano)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha negativo ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_negativo)));

                /*
                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()                
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                */

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DECIMO RENGLON
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Copias")));

                String Copias = "";

                if (OrdenElegida.copia_1.Length > 0)
                {
                    Copias = " Copia 1 " + OrdenElegida.copia_1;
                }

                if (OrdenElegida.copia_2.Length > 0)
                {
                    Copias = Copias + " Copia 2 " + OrdenElegida.copia_2;
                }

                if (OrdenElegida.copia_3.Length > 0)
                {
                    Copias = Copias + " Copia 3 " + OrdenElegida.copia_3;
                }

                if (OrdenElegida.copia_4.Length > 0)
                {
                    Copias = Copias + " Copia 4 " + OrdenElegida.copia_4;
                }

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Copias)));
                PExc++;
                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DUODECIMO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Acabado")));

                String Acabado = "";

                if (OrdenElegida.pegado == "SI")
                {
                    Acabado = " PEGADO ";
                }

                if (OrdenElegida.engrapado == "SI")
                {
                    Acabado = Acabado + " ACABADO ";
                }

                if (OrdenElegida.perforacion == "SI")
                {
                    Acabado = Acabado + " PERFORACION ";
                }

                if (OrdenElegida.rojo == "SI")
                {
                    Acabado = Acabado + " ROJO ";
                }

                if (OrdenElegida.blanco == "SI")
                {
                    Acabado = Acabado + " BLANCO ";
                }
                PExc++;
                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Acabado)));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //CATORCEAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Especificaciones")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("NOMBRE DEL DISEÑADOR: _________________________________")));
                PExc++;
                //QUINCEAVO RENGLON

                if (OrdenElegida.ruta != null)
                {

                    table.AddCell(new Cell(1, 2)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("Ruta")));

                    table.AddCell(new Cell(1, 8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(OrdenElegida.ruta)));
                }
                
                PExc++;
                //DIECISEISAVO RENGLON

                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                   .SetFont(f)
                   .SetFontSize(fs)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .Add(new Paragraph(OrdenElegida.especificaciones)));

                // 19 RENGLON

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Negativo nuevo [  ]")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Ya existe negativo [  ]")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tirar placas y negativos [  ]")));
                PExc++;
                // 19

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Tipo de máquina " + OrdenElegida.tipo_maquina)));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 6)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                PExc++;
                //////////////////////////////////////////
                ///
                table.AddCell(new Cell(8, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(20)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));

                /////////////////////////////////// SEGUNDA IMPRESION
                //PRIMER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden de trabajo")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Recibe")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.quien_recibio)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.prioridad)));

                //SEGUNDO RENGLON
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Nombre de la empresa")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ClienteOrden.pseudonimo)));
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Teléfono")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.telefono)));
                PExc++;
                //TERCER RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Solicitante")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.solicitante)));
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha solicita")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_solicita)));
                PExc++;
                //CUARTO RENGLON                
                PExc++;
                table.AddCell(new Cell(1, 5)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                PExc++;
                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //QUINTO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Descripción")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.nombre_trabajo)));

                PExc++;
                //SEXTO RENGLON
                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Cantidad")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Color de tintas")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.color_tintas)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Papel")));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tipo_papel)));

                PExc++;
                //SEPTIMO RENGLON                

                PExc++;
                //OCTAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Con folio")));

                ConFolio = "Si No [X]";
                if (OrdenElegida.con_folio == "SI")
                {
                    ConFolio = "Si [X] No";
                }

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(ConFolio)));

                Folios = "";
                if (OrdenElegida.con_folio == "SI")
                {
                    Folios = "Del " + OrdenElegida.del_numero + " Al " + OrdenElegida.al_numero;
                }
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Folios)));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Orden anterior")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.orden_anterior)));
                /*
                table.AddCell(new Cell(2, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.numero.ToString())));
                */

                //NOVENO RENGLON
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tamaño del papel")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.tamano)));

                table.AddCell(new Cell(1, 1)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Fecha negativo ")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(OrdenElegida.fecha_negativo)));

                /*
                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(20)
                    .SetBold()                
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                */

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DECIMO RENGLON
                PExc++;
                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Copias")));

                Copias = "";

                if (OrdenElegida.copia_1.Length > 0)
                {
                    Copias = " Copia 1 " + OrdenElegida.copia_1;
                }

                if (OrdenElegida.copia_2.Length > 0)
                {
                    Copias = Copias + " Copia 2 " + OrdenElegida.copia_2;
                }

                if (OrdenElegida.copia_3.Length > 0)
                {
                    Copias = Copias + " Copia 3 " + OrdenElegida.copia_3;
                }

                if (OrdenElegida.copia_4.Length > 0)
                {
                    Copias = Copias + " Copia 4 " + OrdenElegida.copia_4;
                }

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Copias)));
                PExc++;
                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //DUODECIMO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Acabado")));

                Acabado = "";

                if (OrdenElegida.pegado == "SI")
                {
                    Acabado = " PEGADO ";
                }

                if (OrdenElegida.engrapado == "SI")
                {
                    Acabado = Acabado + " ACABADO ";
                }

                if (OrdenElegida.perforacion == "SI")
                {
                    Acabado = Acabado + " PERFORACION ";
                }

                if (OrdenElegida.rojo == "SI")
                {
                    Acabado = Acabado + " ROJO ";
                }

                if (OrdenElegida.blanco == "SI")
                {
                    Acabado = Acabado + " BLANCO ";
                }
                PExc++;
                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(Acabado)));

                //SEPARADOR
                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                   .SetFont(f)
                   .SetFontSize(3)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetFontColor(new DeviceRgb(255, 255, 255))
                   .Add(new Paragraph("SEPARADOR")));
                //SEPARADOR

                //CATORCEAVO RENGLON

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(0, 0, 0))
                    .SetFontColor(new DeviceRgb(255, 255, 255))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Especificaciones")));

                table.AddCell(new Cell(1, 8)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("NOMBRE DEL DISEÑADOR: _________________________________")));
                PExc++;
                //QUINCEAVO RENGLON

                if (OrdenElegida.ruta != null)
                {

                    table.AddCell(new Cell(1, 2)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("Ruta")));

                    table.AddCell(new Cell(1, 8)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFont(f)
                        .SetFontSize(fs)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(OrdenElegida.ruta)));
                }

                PExc++;
                //DIECISEISAVO RENGLON

                table.AddCell(new Cell(1, 10)
                   .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                   .SetFont(f)
                   .SetFontSize(fs)
                   .SetBold()
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .Add(new Paragraph(OrdenElegida.especificaciones)));

                // 19 RENGLON

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Negativo nuevo [  ]")));

                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Ya existe negativo [  ]")));

                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("Tirar placas y negativos [  ]")));
                PExc++;
                // 19

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Tipo de máquina " + OrdenElegida.tipo_maquina)));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    //.Add(new Paragraph("Ryobi [  ]    Printmaster [  ]")));
                    .Add(new Paragraph("")));

                table.AddCell(new Cell(1, 6)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("")));
                PExc++;

                //////////////////////////////////////////


                /*

                ImageData imageData = ImageDataFactory.Create(@"Imagenes/LogoAlvarez.png");

                iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imageData);

                pdfImg.SetHeight(100);
                pdfImg.SetFixedPosition(480, 480);

                document.Add(pdfImg);

                pdfImg.SetFixedPosition(480, 150);

                document.Add(pdfImg);

                */

                document.Add(table);

                document.Close();

                Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = rutaPDF;
                prc.Start();

                /*
                byte[] content = Pdf
                    .From(html)
                    .OfSize(PaperSize.Letter)
                    .Content();
                String rutaPDF = @"C:\OpcyonApp\Cotizacion_" + cotizacion.IdCotizacion + ".pdf";

                File.WriteAllBytes(rutaPDF, content);

                Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = rutaPDF;
                prc.Start();
                */
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message + " -- " + PExc.ToString());
            }
        }
    }
}
