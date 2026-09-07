using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;
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
        float TotalEfectivo = 0;
        float TotalCheque = 0;
        float TotalTransferencia = 0;
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
            lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";
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

                foreach (FacturaViewModel factura in facturas)
                {
                    var corte = dbContext.CorteDiario.FirstOrDefault(c => c.id_factura == factura.id_factura);
                    if (corte != null)
                    {
                        factura.id_corte_diario = corte.id_corte_diario;
                        factura.entrego = corte.entrega;
                        factura.referencia = corte.referencia;
                        factura.observaciones = corte.observaciones;
                        factura.total_pagado = (decimal)(corte.total_pagado ?? 0);
                    }
                }

                float totalFacturas = (float)facturas.Sum(f => f.total_pagado);
                float totalEfectivo = (float)facturas.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                TotalEfectivo = totalEfectivo;
                TotalCheque = (float)facturas.Where(f => f.referencia == "Cheque").Sum(f => f.total_pagado);
                TotalTransferencia = (float)facturas.Where(f => f.referencia == "Transferencia").Sum(f => f.total_pagado);
                lblTotalFacturas.Content = $"Total Facturas: {totalFacturas}";

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";

                dgFacturas.ItemsSource = facturas;
                //TotalEfectivo = totalFacturas;
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

                foreach (FacturaViewModel cotizacion in cotizaciones)
                {
                    var corte = dbContext.CorteDiario.FirstOrDefault(c => c.id_nota == cotizacion.id_nota);
                    if (corte != null)
                    {
                        cotizacion.id_corte_diario = corte.id_corte_diario;
                        cotizacion.entrego = corte.entrega;
                        cotizacion.referencia = corte.referencia;
                        cotizacion.observaciones = corte.observaciones;
                        cotizacion.total_pagado = (decimal)(corte.total_pagado ?? 0);
                    }
                }

                float totalCotizaciones = (float)cotizaciones.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                float totalEfectivo = (float)cotizaciones.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                TotalEfectivo += totalEfectivo;
                lblTotalCotizaciones.Content = $"Total Cotizaciones: {totalCotizaciones}";

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";

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
                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";
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

                float totalFacturas = (float)facturas.Sum(f => f.total_pagado);
                float totalEfectivo = (float)facturas.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                TotalEfectivo = totalEfectivo;
                TotalCheque = (float)facturas.Where(f => f.referencia == "Cheque").Sum(f => f.total_pagado);
                TotalTransferencia = (float)facturas.Where(f => f.referencia == "Transferencia").Sum(f => f.total_pagado);
                lblTotalFacturas.Content = $"Total Facturas: {totalFacturas}";

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";
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
                        corte.total_pagado = (double?)factura.total_pagado;
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
                            pagos.cantidad = (double)factura.total_pagado;
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
                            corteExistente.total_pagado = (double?)factura.total_pagado;
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
                                    pagos.cantidad = (double)factura.total_pagado;
                                    pagos.fecha = Fecha;
                                    pagos.notas = factura.observaciones;
                                    pagos.numero_cheque = "";
                                    pagos.banco = "";
                                    pagos.numero_recibo = "";
                                    dbContext.Pagos.Add(pagos);

                                    if (pagos.cantidad == corteExistente.total)
                                    {
                                        
                                    }
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
                        corte.total_pagado = (double?)cotizacion.total_pagado;
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
                            pagosNotas.cantidad = (double)cotizacion.total_pagado;
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
                            corteExistente.total_pagado = (double?)cotizacion.total_pagado;
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
                                    pagosNotas.cantidad = (double)cotizacion.total_pagado;
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
                float totalCotizaciones = (float)notas.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                float totalEfectivo = (float)notas.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                TotalEfectivo += totalEfectivo;
                lblTotalCotizaciones.Content = $"Total Cotizaciones: {totalCotizaciones}";

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";
            }
        }

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

        private enum SectionKind { Facturas, Cotizaciones }

        // Exporta ambos grids en la misma hoja: primero Facturas, luego Cotizaciones.
        // Ignora propiedades que empiezan por "id". Sólo añade filas con valor en "Entrega".
        // Añade columna "Pagado" que usa el campo `total_pagado` (si existe).
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
                try { ws.Name = "Facturas y Cotizaciones"; } catch { /* ignorar */ }

                int currentRow = 1;

                WriteSectionToWorksheet(ws, dgFacturas.ItemsSource as IEnumerable, "Facturas", ref currentRow, SectionKind.Facturas);

                currentRow += 1; // fila en blanco

                WriteSectionToWorksheet(ws, dgCotizaciones.ItemsSource as IEnumerable, "Cotizaciones", ref currentRow, SectionKind.Cotizaciones);

                var usedRange = ws.UsedRange as Excel.Range;
                usedRange.Columns.AutoFit();
                Marshal.ReleaseComObject(usedRange);

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

        // Escritura de sección con la columna "Pagado" (campo total_pagado)
        private void WriteSectionToWorksheet(Excel.Worksheet ws, IEnumerable items, string sectionTitle, ref int currentRow, SectionKind kind)
        {
            // Título de sección
            Excel.Range titleRange = ws.Cells[currentRow, 1] as Excel.Range;
            titleRange.Value = sectionTitle;
            titleRange.Font.Bold = true;
            Marshal.ReleaseComObject(titleRange);
            currentRow++;

            if (items == null)
            {
                var r = ws.Cells[currentRow, 1] as Excel.Range;
                r.Value = "No hay datos";
                Marshal.ReleaseComObject(r);
                currentRow++;
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
                var r = ws.Cells[currentRow, 1] as Excel.Range;
                r.Value = "No hay datos";
                Marshal.ReleaseComObject(r);
                currentRow++;
                return;
            }

            // Obtener propiedades y filtrar las que empiezan por "id"
            var allProps = TypeDescriptor.GetProperties(first)
                .Cast<PropertyDescriptor>()
                .Where(p => p != null && !p.Name.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Orden solicitado: Cliente, Contribuyente, Numero, Pagado(total_pagado), Fecha, Entrega, Referencia, Observaciones
            var desiredOrder = new[]
            {
                new { Label = "Cliente", Aliases = new[] { "nombre", "NombreUnificado", "Nombre", "cliente" } },
                new { Label = "Contribuyente", Aliases = new[] { "NombreContribuyente", "nombrecontribuyente", "contribuyente" } },
                new { Label = "Numero", Aliases = new[] { "numero" } },
                new { Label = "Pagado", Aliases = new[] { "total_pagado", "total" } }, // mostramos total_pagado aquí
                new { Label = "Fecha", Aliases = new[] { "fecha" } },
                new { Label = "Entrega", Aliases = new[] { "entrego", "Entrega", "entrega" } },
                new { Label = "Referencia", Aliases = new[] { "referencia" } },
                new { Label = "Observaciones", Aliases = new[] { "observaciones" } }
            };

            // Mapear propiedades en orden deseado
            var orderedProps = new PropertyDescriptor[desiredOrder.Length];
            for (int i = 0; i < desiredOrder.Length; i++)
            {
                foreach (var a in desiredOrder[i].Aliases)
                {
                    var found = allProps.FirstOrDefault(p => string.Equals(p.Name, a, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        orderedProps[i] = found;
                        break;
                    }
                }
            }

            // Encabezados
            int headerRow = currentRow;
            for (int c = 0; c < desiredOrder.Length; c++)
            {
                var hr = ws.Cells[currentRow, c + 1] as Excel.Range;
                hr.Value = desiredOrder[c].Label;
                hr.Font.Bold = true;
                Marshal.ReleaseComObject(hr);
            }
            currentRow++;

            // Índices relevantes (1-based): Pagado (total_pagado preferred), Entrega, Referencia
            int pagadoColumnIndex = -1;
            int entregaColumnIndex = -1;
            int referenciaColumnIndex = -1;
            for (int i = 0; i < orderedProps.Length; i++)
            {
                var pd = orderedProps[i];
                if (pd == null) continue;
                if (string.Equals(pd.Name, "total_pagado", StringComparison.OrdinalIgnoreCase) || string.Equals(pd.Name, "total", StringComparison.OrdinalIgnoreCase))
                    pagadoColumnIndex = i + 1;
                if (string.Equals(pd.Name, "entrego", StringComparison.OrdinalIgnoreCase) || string.Equals(pd.Name, "entrega", StringComparison.OrdinalIgnoreCase))
                    entregaColumnIndex = i + 1;
                if (string.Equals(pd.Name, "referencia", StringComparison.OrdinalIgnoreCase))
                    referenciaColumnIndex = i + 1;
            }

            decimal acumuladoPagado = 0m;
            var referenciaSums = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            // Recorrer elementos y escribir sólo los que cumplan filtro de Entrega (si existe)
            foreach (var item in items)
            {
                // Si existe la columna Entrega, comprobar valor
                if (entregaColumnIndex != -1)
                {
                    var entregaProp = orderedProps[entregaColumnIndex - 1];
                    object entregaVal = null;
                    try { entregaVal = entregaProp?.GetValue(item); } catch { entregaVal = null; }
                    if (entregaVal == null || string.IsNullOrWhiteSpace(entregaVal.ToString()))
                    {
                        continue; // omitir fila
                    }
                }

                object referenciaValObj = null;
                decimal thisRowPagado = 0m;
                bool thisRowHasPagado = false;

                // Escribir columnas en el orden solicitado
                for (int c = 0; c < orderedProps.Length; c++)
                {
                    var prop = orderedProps[c];
                    object val = null;
                    try { val = prop?.GetValue(item); } catch { val = null; }

                    var cell = ws.Cells[currentRow, c + 1] as Excel.Range;
                    cell.Value = val ?? "";
                    Marshal.ReleaseComObject(cell);

                    // Capturar referencia y pagado para agregados
                    if (referenciaColumnIndex != -1 && c + 1 == referenciaColumnIndex)
                    {
                        referenciaValObj = val;
                    }

                    if (pagadoColumnIndex != -1 && c + 1 == pagadoColumnIndex && val != null)
                    {
                        try
                        {
                            decimal d;
                            if (val is decimal dec) d = dec;
                            else if (val is double db) d = Convert.ToDecimal(db);
                            else if (val is float f) d = Convert.ToDecimal(f);
                            else if (val is int iVal) d = Convert.ToDecimal(iVal);
                            else if (val is long lVal) d = Convert.ToDecimal(lVal);
                            else decimal.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d);

                            acumuladoPagado += d;
                            thisRowPagado = d;
                            thisRowHasPagado = true;
                        }
                        catch { /* ignorar conversión */ }
                    }
                }

                // Acumular por referencia para Facturas (usar Pagado)
                if (kind == SectionKind.Facturas && referenciaColumnIndex != -1 && referenciaValObj != null && thisRowHasPagado)
                {
                    var key = referenciaValObj.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        if (!referenciaSums.TryGetValue(key, out var existing)) referenciaSums[key] = thisRowPagado;
                        else referenciaSums[key] = existing + thisRowPagado;
                    }
                }

                currentRow++;
            }

            // Escribir total general de la sección (si existe columna Pagado)
            if (pagadoColumnIndex != -1)
            {
                var labelCell = ws.Cells[currentRow, 1] as Excel.Range;
                labelCell.Value = $"Total {sectionTitle}";
                labelCell.Font.Bold = true;
                Marshal.ReleaseComObject(labelCell);

                var totalCell = ws.Cells[currentRow, pagadoColumnIndex] as Excel.Range;
                totalCell.NumberFormat = "#,##0.00";
                totalCell.Value = acumuladoPagado;
                totalCell.Font.Bold = true;
                Marshal.ReleaseComObject(totalCell);

                currentRow++;
            }

            // Para Facturas: escribir totales por Referencia a la derecha de la tabla y totales especiales basados en Pagado
            if (kind == SectionKind.Facturas)
            {
                int startCol = orderedProps.Length + 2; // columna vacía entre tablas

                // Tabla de referencias si hay datos
                int refRow = headerRow + 1;
                if (referenciaSums.Count > 0)
                {
                    var hdrRef1 = ws.Cells[headerRow, startCol] as Excel.Range;
                    hdrRef1.Value = "Referencia";
                    hdrRef1.Font.Bold = true;
                    Marshal.ReleaseComObject(hdrRef1);

                    var hdrRef2 = ws.Cells[headerRow, startCol + 1] as Excel.Range;
                    hdrRef2.Value = "Total Referencia";
                    hdrRef2.Font.Bold = true;
                    Marshal.ReleaseComObject(hdrRef2);

                    foreach (var kvp in referenciaSums.OrderBy(k => k.Key))
                    {
                        var r1 = ws.Cells[refRow, startCol] as Excel.Range;
                        r1.Value = kvp.Key;
                        Marshal.ReleaseComObject(r1);

                        var r2 = ws.Cells[refRow, startCol + 1] as Excel.Range;
                        r2.NumberFormat = "#,##0.00";
                        r2.Value = kvp.Value;
                        Marshal.ReleaseComObject(r2);

                        refRow++;
                    }

                    // Total general de referencias
                    var lblTotalRefs = ws.Cells[refRow, startCol] as Excel.Range;
                    lblTotalRefs.Value = "Total general";
                    lblTotalRefs.Font.Bold = true;
                    Marshal.ReleaseComObject(lblTotalRefs);

                    var valTotalRefs = ws.Cells[refRow, startCol + 1] as Excel.Range;
                    valTotalRefs.NumberFormat = "#,##0.00";
                    valTotalRefs.Value = referenciaSums.Values.Sum();
                    valTotalRefs.Font.Bold = true;
                    Marshal.ReleaseComObject(valTotalRefs);
                }

                // --- Totales especiales: Efectivo, Cheque, Transferencia (usando referenciaSums acumuladas sobre Pagado) ---
                /*
                int totalsStartCol = startCol + (referenciaSums.Count > 0 ? 3 : 0);

                var hdrSpecial = ws.Cells[headerRow, totalsStartCol] as Excel.Range;
                hdrSpecial.Value = "Totales por Medio";
                hdrSpecial.Font.Bold = true;
                Marshal.ReleaseComObject(hdrSpecial);

                referenciaSums.TryGetValue("Efectivo", out var totalEfectivo);
                referenciaSums.TryGetValue("Cheque", out var totalCheque);
                referenciaSums.TryGetValue("Transferencia", out var totalTransferencia);

                int specialRow = headerRow + 1;

                var lblEf = ws.Cells[specialRow, totalsStartCol] as Excel.Range;
                lblEf.Value = "Total Efectivo";
                Marshal.ReleaseComObject(lblEf);

                var valEf = ws.Cells[specialRow, totalsStartCol + 1] as Excel.Range;
                valEf.NumberFormat = "#,##0.00";
                valEf.Value = totalEfectivo;
                Marshal.ReleaseComObject(valEf);
                specialRow++;

                var lblCh = ws.Cells[specialRow, totalsStartCol] as Excel.Range;
                lblCh.Value = "Total Cheque";
                Marshal.ReleaseComObject(lblCh);

                var valCh = ws.Cells[specialRow, totalsStartCol + 1] as Excel.Range;
                valCh.NumberFormat = "#,##0.00";
                valCh.Value = totalCheque;
                Marshal.ReleaseComObject(valCh);
                specialRow++;

                var lblTr = ws.Cells[specialRow, totalsStartCol] as Excel.Range;
                lblTr.Value = "Total Transferencia";
                Marshal.ReleaseComObject(lblTr);

                var valTr = ws.Cells[specialRow, totalsStartCol + 1] as Excel.Range;
                valTr.NumberFormat = "#,##0.00";
                valTr.Value = totalTransferencia;
                Marshal.ReleaseComObject(valTr);
                specialRow++;
                */
            }
        }
    }
}
