using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para CorteDiario.xaml
    /// </summary>
    public partial class CorteDiario : Window
    {
        String Fecha;        
        public CorteDiario()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFecha.SelectedDate = DateTime.Now;
            Fecha = dpFecha.SelectedDate.Value.ToShortDateString();
            CargarFacturas();
            CargarCotizaciones();
        }

        private void CargarFacturas()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var facturas = dbContext.Facturas
                        .Join(
                            dbContext.Clientes,
                            f => f.id_cliente,
                            c => c.id_cliente,
                            (f, c) => new
                            {
                                f.id_factura,
                                f.id_cliente,
                                f.id_contribuyente,
                                f.subtotal,
                                f.total,
                                f.pagada,
                                f.estado,
                                f.fecha,
                                f.numero,
                                f.razon_cancelado,
                                f.amparada_por,
                                c.nombre
                            }
                        )
                        .Join(
                            dbContext.Contribuyentes,
                            f => f.id_contribuyente,
                            co => co.id_contribuyente,
                            (f, co) => new
                            {
                                f.id_factura,
                                f.id_cliente,
                                f.id_contribuyente,
                                f.subtotal,
                                f.total,
                                f.pagada,
                                f.estado,
                                f.fecha,
                                f.numero,
                                f.razon_cancelado,
                                f.amparada_por,
                                f.nombre,
                                NombreContribuyente = co.nombre.Substring(0, co.nombre.IndexOf(" ")),
                                id_entrega = 0,
                                entrego = "",
                                referencia = "",
                                observaciones = ""
                            }
                        )
                       .Where(F => F.fecha == Fecha)
                       .ToList()
                       .Select(f => new FacturaViewModel {
                           id_corte_diario = 0,
                           id_factura = f.id_factura,
                           id_nota = 0,
                           id_cliente = f.id_cliente,
                           id_contribuyente = f.id_contribuyente,
                           subtotal = (decimal)f.subtotal,
                           total = (decimal)f.total,
                           pagada = f.pagada,
                           estado = f.estado,
                           fecha = f.fecha,
                           numero = f.numero,
                           nombre = f.nombre,
                           NombreContribuyente = f.NombreContribuyente,
                           id_entrega = f.id_entrega,
                           entrego = f.entrego,
                           referencia = f.referencia,
                           observaciones = f.observaciones
                       })
                       .ToList();

                float totalFacturas = (float)facturas.Sum(f => f.total);
                lblTotalFacturas.Content = $"Total Facturas: {totalFacturas}";

                foreach (FacturaViewModel factura in facturas)
                {
                    var corte = dbContext.CorteDiario.FirstOrDefault(c => c.id_factura == factura.id_factura);
                    if (corte != null)
                    {
                        factura.id_corte_diario = corte.id_corte_diario;
                        factura.entrego = corte.entrega;
                        factura.referencia = corte.referencia;
                        factura.observaciones = corte.observaciones;
                    }
                }

                dgFacturas.ItemsSource = facturas;
            }
        }

        private void CargarCotizaciones()
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var cotizaciones = dbContext.Notas
                        .Join(
                            dbContext.Clientes,
                            f => f.id_cliente,
                            c => c.id_cliente,
                            (f, c) => new
                            {
                                f.id_nota,
                                f.id_cliente,
                                f.total,
                                f.pagada,
                                f.estado,
                                f.fecha,
                                f.numero,
                                f.solicita,
                                c.nombre,
                                NombreUnificado = c.nombre.Contains("VARIOS") ? (c.nombre + " / " + f.solicita) : c.nombre,
                                id_entrega = 0,
                                entrego = "",
                                referencia = "",
                                observaciones = ""
                            }
                        )
                       .Where(F => F.fecha == Fecha)
                       .ToList()
                       .Select(f => new FacturaViewModel
                       {
                           id_corte_diario = 0,
                           id_factura = 0,
                           id_nota = f.id_nota,
                           id_cliente = (int)f.id_cliente,
                           id_contribuyente = 0,
                           subtotal = 0,
                           total = (decimal)f.total,
                           pagada = f.pagada,
                           estado = f.estado,
                           fecha = f.fecha,
                           numero = f.numero,
                           nombre = f.nombre,
                           NombreContribuyente = f.NombreUnificado,
                           id_entrega = f.id_entrega,
                           entrego = f.entrego,
                           referencia = f.referencia,
                           observaciones = f.observaciones
                       })
                       .ToList();
                float totalCotizaciones = (float)cotizaciones.Sum(f => f.total);
                lblTotalCotizaciones.Content = $"Total Cotizaciones: {totalCotizaciones}";

                foreach (FacturaViewModel cotizacion in cotizaciones)
                {
                    var corte = dbContext.CorteDiario.FirstOrDefault(c => c.id_nota == cotizacion.id_nota);
                    if (corte != null)
                    {
                        cotizacion.id_corte_diario = corte.id_corte_diario;
                        cotizacion.entrego = corte.entrega;
                        cotizacion.referencia = corte.referencia;
                        cotizacion.observaciones = corte.observaciones;
                    }
                }

                dgCotizaciones.ItemsSource = cotizaciones;
            }
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFecha.SelectedDate.HasValue)
            {
                Fecha = dpFecha.SelectedDate.Value.ToShortDateString();
                CargarFacturas();
                CargarCotizaciones();
            }

        }

        private void dgFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgFacturas.SelectedItem != null)
            {
                FacturaViewModel selectedFactura = dgFacturas.SelectedItem as FacturaViewModel;
                if (selectedFactura.entrego == null)
                {
                    selectedFactura.entrego = "";
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedFactura, "Factura", this, "");
                    detalleWindow.ShowDialog();
                }
                else
                {
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedFactura, "Factura", this, "EDITAR");
                    detalleWindow.ShowDialog();
                }                
            }
        }
        public void ActualizarFactura(FacturaViewModel facturaActualizada)
        {
            var facturas = dgFacturas.ItemsSource as List<FacturaViewModel>;
            if (facturas != null)
            {
                int index = facturas.FindIndex(f => f.id_factura == facturaActualizada.id_factura);
                if (index >= 0)
                {
                    facturas[index] = facturaActualizada;
                    dgFacturas.ItemsSource = null;
                    dgFacturas.ItemsSource = facturas;
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            foreach (FacturaViewModel factura in dgFacturas.ItemsSource)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    if (factura.id_corte_diario == 0)
                    {
                        Entity.CorteDiario corte = new Entity.CorteDiario();

                        corte.numero = factura.numero;
                        corte.cliente = factura.nombre;
                        corte.contribuyente = factura.NombreContribuyente;
                        corte.subtotal = (double?)factura.subtotal;
                        corte.total = (double?)factura.total;
                        corte.referencia = factura.referencia;
                        corte.id_entrega = factura.id_entrega;
                        corte.entrega = factura.entrego;
                        corte.observaciones = factura.observaciones;
                        corte.tipo = "FACTURA";
                        corte.fecha_pago = Fecha;
                        if (factura.referencia == "FIRMO" || factura.referencia == "")
                        {
                            corte.aplicado = "NO";
                            corte.fecha_aplicado = "";
                        }
                        else
                        {
                            corte.aplicado = "SI";
                            corte.fecha_aplicado = Fecha;

                            Pagos pagos = new Pagos();
                            pagos.id_factura = factura.id_factura;
                            pagos.tipo = corte.referencia;
                            pagos.cantidad = (double)factura.total;
                            pagos.fecha = Fecha;
                            pagos.notas = factura.observaciones;
                            pagos.numero_cheque = "";
                            pagos.banco = "";
                            pagos.numero_recibo = "";
                            dbContext.Pagos.Add(pagos);
                        }
                        corte.id_factura = factura.id_factura;
                        corte.id_nota = 0;

                        dbContext.CorteDiario.Add(corte);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        Entity.CorteDiario corteExistente = dbContext.CorteDiario.FirstOrDefault(c => c.id_corte_diario == factura.id_corte_diario);
                        if (corteExistente == null)
                        {

                        }
                        else
                        {
                            corteExistente.referencia = factura.referencia;
                            corteExistente.id_entrega = factura.id_entrega;
                            corteExistente.entrega = factura.entrego;
                            corteExistente.observaciones = factura.observaciones;
                            if (factura.referencia == "FIRMO" || factura.referencia == "")
                            {
                                corteExistente.aplicado = "NO";
                                corteExistente.fecha_aplicado = "";
                            }
                            else
                            {
                                if (corteExistente.aplicado == "NO")
                                {
                                    corteExistente.aplicado = "SI";
                                    corteExistente.fecha_aplicado = Fecha;

                                    Pagos pagos = new Pagos();
                                    pagos.id_factura = factura.id_factura;
                                    pagos.tipo = corteExistente.referencia;
                                    pagos.cantidad = (double)factura.total;
                                    pagos.fecha = Fecha;
                                    pagos.notas = factura.observaciones;
                                    pagos.numero_cheque = "";
                                    pagos.banco = "";
                                    pagos.numero_recibo = "";
                                    dbContext.Pagos.Add(pagos);
                                }
                            }
                            dbContext.SaveChanges();
                        }
                    }
                }
            }

            foreach (FacturaViewModel cotizacion in dgCotizaciones.ItemsSource)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    if (cotizacion.id_corte_diario == 0)
                    {
                        Entity.CorteDiario corte = new Entity.CorteDiario();
                        corte.numero = cotizacion.numero;
                        corte.cliente = cotizacion.nombre;
                        corte.contribuyente = "";
                        corte.subtotal = (double?)cotizacion.subtotal;
                        corte.total = (double?)cotizacion.total;
                        corte.referencia = cotizacion.referencia;
                        corte.id_entrega = cotizacion.id_entrega;
                        corte.entrega = cotizacion.entrego;
                        corte.observaciones = cotizacion.observaciones;
                        corte.tipo = "COTIZACION";
                        corte.fecha_pago = Fecha;
                        if (cotizacion.referencia == "FIRMO" || cotizacion.referencia == "")
                        {
                            corte.aplicado = "NO";
                            corte.fecha_aplicado = "";
                        }
                        else
                        {
                            corte.aplicado = "SI";
                            corte.fecha_aplicado = Fecha;

                            PagosNotas pagosNotas = new PagosNotas();
                            pagosNotas.id_nota = cotizacion.id_nota;
                            pagosNotas.tipo = corte.referencia;
                            pagosNotas.cantidad = (double)cotizacion.total;
                            pagosNotas.fecha = Fecha;
                            pagosNotas.notas = cotizacion.observaciones;
                            pagosNotas.numero_cheque = "";
                            pagosNotas.banco = "";
                            pagosNotas.numero_recibo = "";
                            dbContext.PagosNotas.Add(pagosNotas);
                        }
                        corte.id_factura = 0;
                        corte.id_nota = cotizacion.id_nota;
                        dbContext.CorteDiario.Add(corte);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        Entity.CorteDiario corteExistente = dbContext.CorteDiario.FirstOrDefault(c => c.id_corte_diario == cotizacion.id_corte_diario);
                        if (corteExistente == null)
                        {
                        }
                        else
                        {
                            corteExistente.referencia = cotizacion.referencia;
                            corteExistente.id_entrega = cotizacion.id_entrega;
                            corteExistente.entrega = cotizacion.entrego;
                            corteExistente.observaciones = cotizacion.observaciones;
                            if (cotizacion.referencia == "FIRMO" || cotizacion.referencia == "")
                            {
                                corteExistente.aplicado = "NO";
                                corteExistente.fecha_aplicado = "";
                            }
                            else
                            {
                                if (corteExistente.aplicado == "NO")
                                {
                                    corteExistente.aplicado = "SI";
                                    corteExistente.fecha_aplicado = Fecha;

                                    PagosNotas pagosNotas = new PagosNotas();
                                    pagosNotas.id_nota = cotizacion.id_nota;
                                    pagosNotas.tipo = corteExistente.referencia;
                                    pagosNotas.cantidad = (double)cotizacion.total;
                                    pagosNotas.fecha = Fecha;
                                    pagosNotas.notas = cotizacion.observaciones;
                                    pagosNotas.numero_cheque = "";
                                    pagosNotas.banco = "";
                                    pagosNotas.numero_recibo = "";
                                    dbContext.PagosNotas.Add(pagosNotas);
                                }
                            }
                            dbContext.SaveChanges();
                        }
                    }
                }
            }
        }

        private void dgCotizaciones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCotizaciones.SelectedItem != null)
            {
                FacturaViewModel selectedCotizacion = dgCotizaciones.SelectedItem as FacturaViewModel;
                if (selectedCotizacion.entrego == null)
                {
                    selectedCotizacion.entrego = "";
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedCotizacion, "Cotizacion", this, "");
                    detalleWindow.ShowDialog();
                }
                else
                {
                    CorteDiarioDetalle detalleWindow = new CorteDiarioDetalle(selectedCotizacion, "Cotizacion", this, "EDITAR");
                    detalleWindow.ShowDialog();
                }
            }
        }
        public void ActualizarNota(FacturaViewModel notaActualizada)
        {
            var notas = dgCotizaciones.ItemsSource as List<FacturaViewModel>;
            if (notas != null)
            {
                int index = notas.FindIndex(f => f.id_nota == notaActualizada.id_nota);
                if (index >= 0)
                {
                    notas[index] = notaActualizada;
                    dgCotizaciones.ItemsSource = null;
                    dgCotizaciones.ItemsSource = notas;
                }
            }
        }

        // Botón o invocador público para exportar ambos grids
        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                FileName = $"Corte_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ExportGridsToExcel(dlg.FileName);
                    MessageBox.Show("Exportación finalizada.", "Exportar a Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exportando a Excel: {ex.Message}", "Exportar a Excel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Exporta ambos grids en la misma hoja: primero Facturas, luego Cotizaciones
        // Ignora propiedades que empiezan por "id".
        public void ExportGridsToExcel(string filePath)
        {
            Excel.Application xlApp = null;
            Excel.Workbook wb = null;
            Excel.Worksheet ws = null;
            try
            {
                xlApp = new Excel.Application
                {
                    Visible = false,
                    DisplayAlerts = false
                };
                wb = xlApp.Workbooks.Add();

                ws = wb.Worksheets[1] as Excel.Worksheet;
                try
                {
                    ws.Name = "Facturas y Cotizaciones";
                }
                catch
                {
                    // Ignorar si falla el renombrado
                }

                int currentRow = 1;

                // Escribir sección de Facturas con orden específico
                WriteSectionToWorksheet(ws, dgFacturas.ItemsSource as IEnumerable, "Facturas", ref currentRow, SectionKind.Facturas);

                // Fila en blanco entre secciones
                currentRow += 1;

                // Escribir sección de Cotizaciones con orden específico
                WriteSectionToWorksheet(ws, dgCotizaciones.ItemsSource as IEnumerable, "Cotizaciones", ref currentRow, SectionKind.Cotizaciones);

                // Ajustar columnas
                var usedRange = ws.UsedRange;
                usedRange.Columns.AutoFit();
                Marshal.ReleaseComObject(usedRange);

                // Guardar libro
                wb.SaveAs(filePath);
            }
            finally
            {
                if (ws != null) Marshal.ReleaseComObject(ws);
                if (wb != null)
                {
                    wb.Close(false);
                    Marshal.ReleaseComObject(wb);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private enum SectionKind { Facturas, Cotizaciones }

        // Escribe una sección (título + tabla) en la hoja en la posición indicada por currentRow (por referencia).
        // Filtra propiedades cuyo nombre empiece por "id" (case-insensitive).
        // Ordena columnas según especificado y añade total para columna "total".
        private void WriteSectionToWorksheet(Excel.Worksheet ws, IEnumerable items, string sectionTitle, ref int currentRow, SectionKind kind)
        {
            // Título de sección
            ws.Cells[currentRow, 1] = sectionTitle;
            var titleRange = ws.Range[ws.Cells[currentRow, 1], ws.Cells[currentRow, 1]];
            titleRange.Font.Bold = true;
            currentRow++;

            if (items == null)
            {
                ws.Cells[currentRow, 1] = "No hay datos";
                currentRow++;
                Marshal.ReleaseComObject(titleRange);
                return;
            }

            // Obtener primer elemento para descubrir propiedades
            object first = null;
            foreach (var it in items)
            {
                first = it;
                break;
            }

            if (first == null)
            {
                ws.Cells[currentRow, 1] = "No hay datos";
                currentRow++;
                Marshal.ReleaseComObject(titleRange);
                return;
            }

            // Obtener propiedades y filtrar las que empiezan por "id" (case-insensitive)
            var allProps = TypeDescriptor.GetProperties(first)
                .Cast<PropertyDescriptor>()
                .Where(p => p != null && !p.Name.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Definir orden deseado (misma para Facturas y Cotizaciones según solicitud)
            // Cada entrada tiene posibles aliases para mapear a propiedades reales
            var desiredOrder = new[]
            {
                new { Label = "Cliente", Aliases = new[] { "nombre", "NombreUnificado", "Nombre", "cliente" } },
                new { Label = "Contribuyente", Aliases = new[] { "NombreContribuyente", "nombrecontribuyente", "contribuyente" } },
                new { Label = "Numero", Aliases = new[] { "numero" } },
                new { Label = "Total", Aliases = new[] { "total" } },
                new { Label = "Fecha", Aliases = new[] { "fecha" } },
                new { Label = "Entrega", Aliases = new[] { "entrego", "Entrega", "entrega" } },
                new { Label = "Referencia", Aliases = new[] { "referencia" } },
                new { Label = "Observaciones", Aliases = new[] { "observaciones" } }
            };

            // Construir lista de PropertyDescriptor por el orden deseado (null si no existe)
            var orderedProps = new PropertyDescriptor[desiredOrder.Length];
            for (int i = 0; i < desiredOrder.Length; i++)
            {
                var aliases = desiredOrder[i].Aliases;
                PropertyDescriptor found = null;
                foreach (var a in aliases)
                {
                    found = allProps.FirstOrDefault(p => string.Equals(p.Name, a, StringComparison.OrdinalIgnoreCase));
                    if (found != null) break;
                }
                orderedProps[i] = found; // puede ser null, se rellenará en blanco luego
            }

            // Encabezados (usar labels solicitados)
            for (int c = 0; c < desiredOrder.Length; c++)
            {
                ws.Cells[currentRow, c + 1] = desiredOrder[c].Label;
                var headerCellRange = ws.Range[ws.Cells[currentRow, c + 1], ws.Cells[currentRow, c + 1]];
                headerCellRange.Font.Bold = true;
                Marshal.ReleaseComObject(headerCellRange);
            }
            currentRow++;

            // Índice de la columna "Total" dentro del orden (1-based) si existe en orderedProps
            int totalColumnIndex = -1;
            for (int i = 0; i < orderedProps.Length; i++)
            {
                if (orderedProps[i] != null && string.Equals(orderedProps[i].Name, "total", StringComparison.OrdinalIgnoreCase))
                {
                    totalColumnIndex = i + 1;
                    break;
                }
            }

            // Filas y acumulador del total
            decimal acumuladoTotal = 0m;
            foreach (var item in items)
            {
                for (int c = 0; c < orderedProps.Length; c++)
                {
                    var prop = orderedProps[c];
                    object val = null;
                    if (prop != null)
                    {
                        try { val = prop.GetValue(item); }
                        catch { val = null; }
                    }
                    ws.Cells[currentRow, c + 1] = val ?? "";

                    // Si esta columna es "total", intentar sumar
                    if (totalColumnIndex != -1 && c + 1 == totalColumnIndex)
                    {
                        if (val != null)
                        {
                            try
                            {
                                decimal d;
                                if (val is decimal dec) d = dec;
                                else if (val is double db) d = Convert.ToDecimal(db);
                                else if (val is float f) d = Convert.ToDecimal(f);
                                else if (val is int iVal) d = Convert.ToDecimal(iVal);
                                else if (val is long lVal) d = Convert.ToDecimal(lVal);
                                else
                                {
                                    decimal.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
                                }
                                acumuladoTotal += d;
                            }
                            catch
                            {
                                // Ignorar valores no convertibles
                            }
                        }
                    }
                }
                currentRow++;
            }

            // Escribir fila de total si se detectó columna "total"
            if (totalColumnIndex != -1)
            {
                // Celda para etiqueta de total en la primera columna
                ws.Cells[currentRow, 1] = $"Total {sectionTitle}";
                var labelRange = ws.Range[ws.Cells[currentRow, 1], ws.Cells[currentRow, 1]];
                labelRange.Font.Bold = true;
                Marshal.ReleaseComObject(labelRange);

                // Celda para el valor acumulado en la columna correspondiente
                var totalCell = ws.Cells[currentRow, totalColumnIndex];
                totalCell.NumberFormat = "#,##0.00";
                ws.Cells[currentRow, totalColumnIndex] = acumuladoTotal;
                var totalCellRange = ws.Range[ws.Cells[currentRow, totalColumnIndex], ws.Cells[currentRow, totalColumnIndex]];
                totalCellRange.Font.Bold = true;
                Marshal.ReleaseComObject(totalCellRange);

                currentRow++;
            }

            Marshal.ReleaseComObject(titleRange);
        }
    }
}
