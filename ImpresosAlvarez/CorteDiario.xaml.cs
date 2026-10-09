using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para CorteDiario.xaml
    /// </summary>
    public partial class CorteDiario : System.Windows.Window
    {
        String Fecha;
        float TotalEfectivo = 0;
        float TotalCheque = 0;
        float TotalTransferencia = 0;
        float TotalEfectivoFacturas = 0;
        float TotalEfectivoNotas = 0;

        List<Clientes> _clientes;
        Clientes _clienteElegido;
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
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = _clientes;                
            }
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
                        factura.aplicado = corte.aplicado;
                    }
                    else
                    {
                        List<Pagos> pagos = dbContext.Pagos.Where(P => P.id_factura == factura.id_factura && P.fecha == Fecha).ToList();
                        if (pagos.Count > 0)
                        {
                            factura.total_pagado = (decimal)pagos.Sum(P => P.cantidad);
                        }
                    }
                }

                List<Entity.CorteDiario> corteDiario = dbContext.CorteDiario.Where(c => c.fecha_pago == Fecha && c.tipo == "FACTURA" && c.primer_pago == "NO").ToList();

                foreach (var corte in corteDiario)
                {
                    var factura = new FacturaViewModel();
                    if (factura != null)
                    {
                        factura.id_corte_diario = corte.id_corte_diario;
                        factura.id_factura = corte.id_factura ?? 0;
                        factura.id_nota = corte.id_nota ?? 0;
                        factura.id_cliente = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.id_cliente).FirstOrDefault() : 0;
                        factura.id_contribuyente = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.id_contribuyente).FirstOrDefault() : 0;
                        factura.subtotal = corte.id_factura.HasValue ? (decimal)dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.subtotal).FirstOrDefault() : 0;
                        factura.total = corte.id_factura.HasValue ? (decimal)dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.total).FirstOrDefault() : 0;
                        factura.pagada = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.pagada).FirstOrDefault() : "";
                        factura.estado = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.estado).FirstOrDefault() : "";
                        factura.fecha = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.fecha).FirstOrDefault() : "";
                        factura.numero = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.numero).FirstOrDefault() : "";
                        factura.nombre = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.razon_cancelado).FirstOrDefault() : "";
                        factura.NombreContribuyente = corte.id_factura.HasValue ? dbContext.Contribuyentes.Where(co => co.id_contribuyente == factura.id_contribuyente).Select(co => co.nombre.Substring(0, co.nombre.IndexOf(" "))).FirstOrDefault() : "";
                        factura.id_entrega = corte.id_entrega ?? 0;
                        factura.entrego = corte.entrega;
                        factura.referencia = corte.referencia;
                        factura.observaciones = corte.observaciones;
                        factura.total_pagado = (decimal)(corte.total_pagado ?? 0);
                        factura.aplicado = corte.aplicado;
                        facturas.Add(factura);
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
                TotalEfectivoFacturas = totalEfectivo;
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
                           nombre = f.NombreUnificado,
                           NombreContribuyente = "",
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
                        cotizacion.aplicado = corte.aplicado;
                    }
                    else
                    {
                        List<PagosNotas> pagosNotas = dbContext.PagosNotas.Where(PN => PN.id_nota == cotizacion.id_nota && PN.fecha == Fecha).ToList();
                        if (pagosNotas.Count > 0)
                        {                            
                            cotizacion.total_pagado = (decimal)pagosNotas.Sum(PN => PN.cantidad);
                            if (dbContext.PagosNotas.Where(PN => PN.id_nota == cotizacion.id_nota).ToList().Count() == 1)
                            {
                                cotizacion.primer_pago = "SI";
                            }
                        }
                    }
                }

                List<Entity.CorteDiario> corteDiario = dbContext.CorteDiario.Where(c => c.fecha_pago == Fecha && c.tipo == "COTIZACION" && c.primer_pago == "NO").ToList();

                foreach (var corte in corteDiario)
                {
                    var cotizacion = new FacturaViewModel();
                    if (cotizacion != null)
                    {
                        cotizacion.id_corte_diario = corte.id_corte_diario;
                        cotizacion.id_factura = corte.id_factura ?? 0;
                        cotizacion.id_nota = corte.id_nota ?? 0;
                        cotizacion.id_cliente = (int)(corte.id_nota.HasValue ? dbContext.Notas.Where(n => n.id_nota == corte.id_nota).Select(n => n.id_cliente).FirstOrDefault() : 0);
                        cotizacion.id_contribuyente = 0;
                        cotizacion.subtotal = 0;
                        cotizacion.total = corte.id_nota.HasValue ? (decimal)dbContext.Notas.Where(n => n.id_nota == corte.id_nota).Select(n => n.total).FirstOrDefault() : 0;
                        cotizacion.pagada = corte.id_nota.HasValue ? dbContext.Notas.Where(n => n.id_nota == corte.id_nota).Select(n => n.pagada).FirstOrDefault() : "";
                        cotizacion.estado = corte.id_nota.HasValue ? dbContext.Notas.Where(n => n.id_nota == corte.id_nota).Select(n => n.estado).FirstOrDefault() : "";
                        cotizacion.fecha = corte.id_nota.HasValue ? dbContext.Notas.Where(n => n.id_nota == corte.id_nota).Select(n => n.fecha).FirstOrDefault() : "";
                        cotizacion.numero = corte.id_nota.HasValue ? dbContext.Notas.Where(n => n.id_nota == corte.id_nota).Select(n => n.numero).FirstOrDefault() : "";
                        cotizacion.nombre = corte.id_nota.HasValue ? dbContext.Clientes.Where(c => c.id_cliente == cotizacion.id_cliente).Select(c => c.nombre).FirstOrDefault() : "NADA";
                        cotizacion.NombreContribuyente = "";
                        cotizacion.id_entrega = corte.id_entrega ?? 0;
                        cotizacion.entrego = corte.entrega;
                        cotizacion.referencia = corte.referencia;
                        cotizacion.observaciones = corte.observaciones;
                        cotizacion.total_pagado = (decimal)(corte.total_pagado ?? 0);
                        cotizacion.aplicado = corte.aplicado;
                        cotizaciones.Add(cotizacion);
                    }
                }

                float totalCotizaciones = (float)cotizaciones.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                float totalEfectivo = (float)cotizaciones.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                TotalEfectivo += totalEfectivo;
                lblTotalCotizaciones.Content = $"Total Cotizaciones: {totalCotizaciones}";

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";

                TotalEfectivoNotas = totalEfectivo;

                dgCotizaciones.ItemsSource = cotizaciones;
            }

            lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivoFacturas + TotalEfectivoNotas}";
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
                int index = facturas.FindIndex(f => f.id_factura == facturaActualizada.id_factura && f.id_corte_diario == facturaActualizada.id_corte_diario);
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

                TotalEfectivoFacturas = totalEfectivo;

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivoFacturas + TotalEfectivoNotas}";
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
                        corte.primer_pago = factura.primer_pago;
                        corte.total_abonado = (double?)factura.total_abonado + (double?)factura.total_pagado;
                        if (factura.referencia == "FIRMO" || factura.referencia == "")
                        {
                            corte.aplicado = "NO";
                            corte.fecha_aplicado = "";
                        }
                        else
                        {
                            corte.aplicado = "SI";
                            corte.fecha_aplicado = Fecha;

                            Pagos pago = dbContext.Pagos.Where(P => P.id_factura == factura.id_factura && P.fecha == Fecha).FirstOrDefault();
                            if (pago != null)
                            {
                                pago.tipo = corte.referencia;
                                pago.cantidad = (double)factura.total_pagado;
                                pago.fecha = Fecha;
                                pago.notas = factura.observaciones;
                                pago.numero_cheque = "";
                                pago.banco = "";
                                pago.numero_recibo = "";
                            }
                            else
                            {
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
                        }
                        corte.id_factura = factura.id_factura;
                        corte.id_nota = 0;

                        if (factura.total_pagado + factura.total_abonado == factura.total)
                        {
                            Facturas facturaElegida = dbContext.Facturas.FirstOrDefault(f => f.id_factura == factura.id_factura);
                            if (facturaElegida != null)
                            {
                                facturaElegida.pagada = "SI";
                            }
                        }

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
                            corteExistente.total_abonado = (double?)factura.total_abonado + (double?)factura.total_pagado;
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

                                    Pagos pago = dbContext.Pagos.Where(P => P.id_factura == factura.id_factura && P.fecha == Fecha).FirstOrDefault();
                                    if (pago != null)
                                    {
                                        pago.tipo = corteExistente.referencia;
                                        pago.cantidad = (double)factura.total_pagado;
                                        pago.fecha = Fecha;
                                        pago.notas = factura.observaciones;
                                        pago.numero_cheque = "";
                                        pago.banco = "";
                                        pago.numero_recibo = "";
                                    }
                                    else
                                    {
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
                                    }

                                    if (factura.total_pagado == factura.total)
                                    {
                                        Facturas facturaElegida = dbContext.Facturas.FirstOrDefault(f => f.id_factura == factura.id_factura);
                                        if (facturaElegida != null)
                                        {
                                            facturaElegida.pagada = "SI";
                                        }
                                    }
                                    else
                                    {
                                        Facturas facturaElegida = dbContext.Facturas.FirstOrDefault(f => f.id_factura == factura.id_factura);
                                        if (facturaElegida != null)
                                        {
                                            facturaElegida.pagada = "NO";
                                        }
                                    }
                                }
                                else
                                {
                                    Pagos pago = dbContext.Pagos.Where(P => P.id_factura == factura.id_factura && P.fecha == Fecha).FirstOrDefault();
                                    if (pago != null)
                                    {
                                        pago.tipo = corteExistente.referencia;
                                        pago.cantidad = (double)factura.total_pagado;
                                        pago.fecha = Fecha;
                                        pago.notas = factura.observaciones;
                                        pago.numero_cheque = "";
                                        pago.banco = "";
                                        pago.numero_recibo = "";
                                    }
                                    if (factura.total_pagado == factura.total)
                                    {
                                        Facturas facturaElegida = dbContext.Facturas.FirstOrDefault(f => f.id_factura == factura.id_factura);
                                        if (facturaElegida != null)
                                        {
                                            facturaElegida.pagada = "SI";
                                        }
                                    }
                                    else
                                    {
                                        Facturas facturaElegida = dbContext.Facturas.FirstOrDefault(f => f.id_factura == factura.id_factura);
                                        if (facturaElegida != null)
                                        {
                                            facturaElegida.pagada = "NO";
                                        }
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
                        corte.primer_pago = cotizacion.primer_pago;
                        corte.total_abonado = (double?)cotizacion.total_abonado + (double?)cotizacion.total_pagado;
                        if (cotizacion.referencia == "FIRMO" || cotizacion.referencia == "")
                        {
                            corte.aplicado = "NO";
                            corte.fecha_aplicado = "";
                        }
                        else
                        {
                            corte.aplicado = "SI";
                            corte.fecha_aplicado = Fecha;
                            
                            PagosNotas pago = dbContext.PagosNotas.Where(PN => PN.id_pagonota == cotizacion.id_pago).FirstOrDefault();

                            if (pago != null)
                            {
                                pago.tipo = corte.referencia;
                                pago.cantidad = (double)cotizacion.total_pagado;
                                pago.fecha = Fecha;
                                pago.notas = cotizacion.observaciones;
                                pago.numero_cheque = "";
                                pago.banco = "";
                                pago.numero_recibo = "";
                                corte.id_pago = pago.id_pagonota;
                            }
                            else
                            {
                                using (ImpresosBDEntities dbContextPago = new ImpresosBDEntities())
                                {
                                    PagosNotas pagosNotas = new PagosNotas();
                                    pagosNotas.id_nota = cotizacion.id_nota;
                                    pagosNotas.tipo = corte.referencia;
                                    pagosNotas.cantidad = (double)cotizacion.total_pagado;
                                    pagosNotas.fecha = Fecha;
                                    pagosNotas.notas = cotizacion.observaciones;
                                    pagosNotas.numero_cheque = "";
                                    pagosNotas.banco = "";
                                    pagosNotas.numero_recibo = "";
                                    dbContextPago.PagosNotas.Add(pagosNotas);
                                    dbContextPago.SaveChanges();

                                    corte.id_pago = pagosNotas.id_pagonota;
                                }                            }                            
                        }
                        corte.id_factura = 0;
                        corte.id_nota = cotizacion.id_nota;
                        dbContext.CorteDiario.Add(corte);

                        if (cotizacion.total_pagado + cotizacion.total_abonado == cotizacion.total)
                        {
                            Notas notaElegida = dbContext.Notas.FirstOrDefault(n => n.id_nota == cotizacion.id_nota);
                            if (notaElegida != null)
                            {
                                notaElegida.pagada = "SI";
                            }
                        }

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
                            corteExistente.total_abonado = (double?)cotizacion.total_abonado + (double?)cotizacion.total_pagado;
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
                                    /*
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
                                    */
                                    PagosNotas pago = dbContext.PagosNotas.Where(PN => PN.id_nota == cotizacion.id_nota && PN.fecha == Fecha).FirstOrDefault();

                                    if (pago != null)
                                    {
                                        pago.tipo = corteExistente.referencia;
                                        pago.cantidad = (double)cotizacion.total_pagado;
                                        pago.fecha = Fecha;
                                        pago.notas = cotizacion.observaciones;
                                        pago.numero_cheque = "";
                                        pago.banco = "";
                                        pago.numero_recibo = "";
                                    }
                                    else
                                    {
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

                                    if (cotizacion.total_pagado == cotizacion.total)
                                    {
                                        Notas notaElegida = dbContext.Notas.FirstOrDefault(n => n.id_nota == cotizacion.id_nota);
                                        if (notaElegida != null)
                                        {
                                            notaElegida.pagada = "SI";
                                        }
                                    }
                                    else
                                    {
                                        Notas notaElegida = dbContext.Notas.FirstOrDefault(n => n.id_nota == cotizacion.id_nota);
                                        if (notaElegida != null)
                                        {
                                            notaElegida.pagada = "NO";
                                        }
                                    }
                                }
                                else
                                {
                                    PagosNotas pago = dbContext.PagosNotas.Where(PN => PN.id_nota == cotizacion.id_nota && PN.fecha == Fecha).FirstOrDefault();
                                    if (pago != null)
                                    {
                                        pago.tipo = corteExistente.referencia;
                                        pago.cantidad = (double)cotizacion.total_pagado;
                                        pago.fecha = Fecha;
                                        pago.notas = cotizacion.observaciones;
                                        pago.numero_cheque = "";
                                        pago.banco = "";
                                        pago.numero_recibo = "";
                                    }

                                    if (cotizacion.total_pagado == cotizacion.total)
                                    {
                                        Notas notaElegida = dbContext.Notas.FirstOrDefault(n => n.id_nota == cotizacion.id_nota);
                                        if (notaElegida != null)
                                        {
                                            notaElegida.pagada = "SI";
                                        }
                                    }
                                    else
                                    {
                                        Notas notaElegida = dbContext.Notas.FirstOrDefault(n => n.id_nota == cotizacion.id_nota);
                                        if (notaElegida != null)
                                        {
                                            notaElegida.pagada = "NO";
                                        }
                                    }
                                }
                            }

                            if (cotizacion.total_pagado == cotizacion.total)
                            {
                                Notas notaElegida = dbContext.Notas.FirstOrDefault(n => n.id_nota == cotizacion.id_nota);
                                if (notaElegida != null)
                                {
                                    notaElegida.pagada = "SI";
                                }
                            }

                            dbContext.SaveChanges();
                        }
                    }
                }
            }

            CargarFacturas();
            CargarCotizaciones();
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
                int index = notas.FindIndex(f => f.id_nota == notaActualizada.id_nota && f.id_corte_diario == notaActualizada.id_corte_diario);
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

                TotalEfectivoNotas = totalEfectivo;

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivoFacturas + TotalEfectivoNotas}";
            }
        }

        public void AgregarCotizacionPasada(int IdNota)
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
                       .Where(F => F.id_nota == IdNota)
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
                           nombre = f.NombreUnificado,
                           NombreContribuyente = f.NombreUnificado,
                           id_entrega = f.id_entrega,
                           entrego = f.entrego,
                           referencia = f.referencia,
                           observaciones = f.observaciones                          
                       })
                       .ToList();

                var cotizacionesOriginales = dgCotizaciones.ItemsSource as List<FacturaViewModel>;

                var cotizacionExistente = cotizacionesOriginales.FirstOrDefault(c => c.id_nota == IdNota && c.aplicado == "NO");
                if (cotizacionExistente != null)
                {
                    MessageBox.Show("Hay una cotización pendiente de aplicar.");
                    return;
                }

                foreach (FacturaViewModel cotizacion in cotizaciones)
                {
                    var corte = dbContext.CorteDiario.FirstOrDefault(c => c.id_nota == cotizacion.id_nota);

                    cotizacion.id_corte_diario = 0;
                    cotizacion.entrego = "";
                    cotizacion.referencia = "";
                    cotizacion.observaciones = "";
                    cotizacion.total_pagado = 0;
                    cotizacion.aplicado = "NO";
                    cotizacion.primer_pago = "NO";
                    if (corte != null)
                    {
                        cotizacion.total_abonado = (decimal)(corte.total_abonado ?? 0);
                    }
                    else
                    {
                        cotizacion.total_abonado = 0;
                    }   
                    cotizacion.total_pagado = 0;

                    List<PagosNotas> pagosNotas = dbContext.PagosNotas.Where(PN => PN.id_nota == cotizacion.id_nota).ToList();
                    if (pagosNotas.Count > 0)
                    {                        
                        cotizacion.total_abonado = (decimal)pagosNotas.Sum(PN => PN.cantidad);
                    }
                    else
                    {
                        cotizacion.total_abonado = 0;
                    }

                    cotizacionesOriginales.Add(cotizacion);
                }

                float totalCotizaciones = (float)cotizacionesOriginales.Where(f => f.referencia != "").Sum(f => f.total_pagado);
                float totalEfectivo = (float)cotizacionesOriginales.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                TotalEfectivo += totalEfectivo;
                lblTotalCotizaciones.Content = $"Total Cotizaciones: {totalCotizaciones}";

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";

                TotalEfectivoNotas = totalEfectivo;

                dgCotizaciones.ItemsSource = null;
                dgCotizaciones.ItemsSource = cotizacionesOriginales;
            }

            lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivoFacturas + TotalEfectivoNotas}";
        }

        public void AgregarFacturaPasada(int IdFactura)
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
                       .Where(F => F.id_factura == IdFactura)
                       .ToList()
                       .Select(f => new FacturaViewModel
                       {
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

                    factura.id_corte_diario = 0;
                    factura.entrego = "";
                    factura.referencia = "";
                    factura.observaciones = "";
                    factura.total_pagado = 0;
                    factura.aplicado = "NO";
                    factura.total_pagado = 0;
                    factura.primer_pago = "NO";
                    List<Pagos> pagos = dbContext.Pagos.Where(P => P.id_factura == factura.id_factura && P.fecha == Fecha).ToList();
                    if (pagos.Count > 0)
                    {
                        factura.total_abonado = (decimal)pagos.Sum(P => P.cantidad);
                    }
                    else
                    {
                        factura.total_abonado = 0;
                    }
                }

                List<Entity.CorteDiario> corteDiario = dbContext.CorteDiario.Where(c => c.fecha_pago == Fecha && c.tipo == "FACTURA" && c.primer_pago == "NO").ToList();

                foreach (var corte in corteDiario)
                {
                    var factura = new FacturaViewModel();
                    if (factura != null)
                    {
                        factura.id_corte_diario = corte.id_corte_diario;
                        factura.id_factura = corte.id_factura ?? 0;
                        factura.id_nota = corte.id_nota ?? 0;
                        factura.id_cliente = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.id_cliente).FirstOrDefault() : 0;
                        factura.id_contribuyente = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.id_contribuyente).FirstOrDefault() : 0;
                        factura.subtotal = corte.id_factura.HasValue ? (decimal)dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.subtotal).FirstOrDefault() : 0;
                        factura.total = corte.id_factura.HasValue ? (decimal)dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.total).FirstOrDefault() : 0;
                        factura.pagada = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.pagada).FirstOrDefault() : "";
                        factura.estado = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.estado).FirstOrDefault() : "";
                        factura.fecha = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.fecha).FirstOrDefault() : "";
                        factura.numero = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.numero).FirstOrDefault() : "";
                        factura.nombre = corte.id_factura.HasValue ? dbContext.Facturas.Where(f => f.id_factura == corte.id_factura).Select(f => f.razon_cancelado).FirstOrDefault() : "";
                        factura.NombreContribuyente = corte.id_factura.HasValue ? dbContext.Contribuyentes.Where(co => co.id_contribuyente == factura.id_contribuyente).Select(co => co.nombre.Substring(0, co.nombre.IndexOf(" "))).FirstOrDefault() : "";
                        factura.id_entrega = corte.id_entrega ?? 0;
                        factura.entrego = corte.entrega;
                        factura.referencia = corte.referencia;
                        factura.observaciones = corte.observaciones;
                        factura.total_pagado = (decimal)(corte.total_pagado ?? 0);
                        factura.aplicado = corte.aplicado;
                        facturas.Add(factura);
                    }
                }

                float totalFacturas = (float)facturas.Sum(f => f.total_pagado);
                float totalEfectivo = (float)facturas.Where(f => f.referencia == "Efectivo").Sum(f => f.total_pagado);
                TotalEfectivo = totalEfectivo;
                TotalCheque = (float)facturas.Where(f => f.referencia == "Cheque").Sum(f => f.total_pagado);
                TotalTransferencia = (float)facturas.Where(f => f.referencia == "Transferencia").Sum(f => f.total_pagado);
                lblTotalFacturas.Content = $"Total Facturas: {totalFacturas}";

                lblTotalEfectivo.Content = $"Total Efectivo: {TotalEfectivo}";

                dgFacturas.ItemsSource = null;
                dgFacturas.ItemsSource = facturas;
                //TotalEfectivo = totalFacturas;
                TotalEfectivoFacturas = totalEfectivo;
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            string rutaPlantilla = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "TemplateDailySales.xlsx");

            if (!System.IO.File.Exists(rutaPlantilla))
            {
                MessageBox.Show("No se encontró el archivo de plantilla en: " + rutaPlantilla, "ATENCION", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivo de Excel (*.xlsx)|*.xlsx",
                FileName = "CorteDiario_" + Fecha.Replace("/", "-") + ".xlsx",
                Title = "Guardar Corte Diario"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            string rutaDestino = saveFileDialog.FileName;

            List<FacturaViewModel> facturas = (dgFacturas.ItemsSource as IEnumerable<FacturaViewModel>)?.ToList() ?? new List<FacturaViewModel>();
            List<FacturaViewModel> cotizaciones = (dgCotizaciones.ItemsSource as IEnumerable<FacturaViewModel>)?.ToList() ?? new List<FacturaViewModel>();

            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Open(rutaPlantilla);

                Excel.Worksheet hoja = (Excel.Worksheet)workbook.Sheets[1];

                int fila = 5;
                foreach (FacturaViewModel factura in facturas)
                {
                    hoja.Cells[fila, 2] = factura.nombre;
                    hoja.Cells[fila, 3] = factura.NombreContribuyente;
                    hoja.Cells[fila, 4] = factura.numero;
                    hoja.Cells[fila, 5] = factura.total_pagado;
                    hoja.Cells[fila, 6] = factura.fecha;
                    hoja.Cells[fila, 7] = factura.entrego;
                    hoja.Cells[fila, 8] = factura.referencia;
                    hoja.Cells[fila, 9] = factura.observaciones;
                    fila++;
                }
                // Las cotizaciones se insertan a partir de la celda B27
                int filaCotizaciones = 27;
                const int columnaInicioCotizaciones = 2; // Columna B

                foreach (FacturaViewModel cotizacion in cotizaciones)
                {
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones] = cotizacion.nombre;
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones + 1] = "NOTA";
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones + 2] = cotizacion.numero;
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones + 3] = cotizacion.total_pagado;
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones + 4] = cotizacion.fecha;
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones + 5] = cotizacion.entrego;
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones + 6] = cotizacion.referencia;
                    hoja.Cells[filaCotizaciones, columnaInicioCotizaciones + 7] = cotizacion.observaciones;
                    filaCotizaciones++;
                }

                workbook.SaveAs(rutaDestino);

                MessageBox.Show("El archivo se generó correctamente en: " + rutaDestino, "ATENCION", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al generar el archivo de Excel: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }

                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }
            }
        }





        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;
                PagosPendientesCliente pagosPendientes = new PagosPendientesCliente(_clienteElegido, this);
                pagosPendientes.ShowDialog();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgCotizaciones.SelectedItem != null)
            {
                FacturaViewModel selectedCotizacion = dgCotizaciones.SelectedItem as FacturaViewModel;
                if (selectedCotizacion != null)
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        var nota = dbContext.Notas.FirstOrDefault(n => n.id_nota == selectedCotizacion.id_nota);
                        var pagos = dbContext.PagosNotas.Where(p => p.id_nota == selectedCotizacion.id_nota).ToList();
                        float totalAbonado = (float)pagos.Sum(p => p.cantidad);
                        if (nota != null)
                        {
                            ImprimirPDF(nota, totalAbonado);
                        }
                    }
                }
            }
        }

        public void ImprimirPDF(Notas NotaElegida, float totalAbonado)
        {
            if (_clienteElegido == null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    _clienteElegido = dbContext.Clientes.FirstOrDefault(c => c.id_cliente == NotaElegida.id_cliente);
                }
            }
            String rutaPDF = "";

            Document document = null;

            rutaPDF = @"C:\Impresos\Cotizaciones\CotizacionReimpresion_" + NotaElegida.numero + ".pdf";

            //PdfDocument pdf = new PdfDocument(new PdfReader(@"AlvarezCotizacionL.pdf"), new PdfWriter(rutaPDF));
            PdfDocument pdf = new PdfDocument(new PdfWriter(rutaPDF));
            document = new Document(pdf, PageSize.LETTER.Rotate());

            document.SetMargins(10, 10, 10, 10);

            float[] columnWidths = { 1, 5, 1, 1, 1, 1, 5, 1, 1 };
            Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
            PdfFont f = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            /*
            Cell cell = new Cell(1, 5)
                .Add(new Paragraph("This is a header"))
                .SetFont(f)
                .SetFontSize(13)
                .SetFontColor(DeviceGray.WHITE)
                .SetBackgroundColor(DeviceGray.BLACK)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            */
            float fs = 9;

            //1er renglón

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("PRESUPUESTO")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FOLIO: " + NotaElegida.numero)));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("PRESUPUESTO")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FOLIO: " + NotaElegida.numero)));

            //2do Renglón

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FECHA")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(DateTime.Now.Date.ToShortDateString())));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("FECHA")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(DateTime.Now.Date.ToShortDateString())));

            //3er Renglón

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("NOMBRE")));

            if (_clienteElegido.pseudonimo.Contains("VARIOS"))
            {
                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(NotaElegida.solicita)));
            }
            else
            {
                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(tbClientes.Text)));
            }


            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("NOMBRE")));

            if (_clienteElegido.pseudonimo.Contains("VARIOS"))
            {
                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(NotaElegida.solicita)));
            }
            else
            {
                table.AddCell(new Cell(1, 3)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(tbClientes.Text)));
            }

            //4to Renglón

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("DIRECCIÓN")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("DIRECCIÓN")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            //5to Renglón

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("CIUDAD")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("CIUDAD")));

            table.AddCell(new Cell(1, 3)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(fs)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph("")));

            //to renglón Separador

            table.AddCell(new Cell(1, 9)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            ImageData imageData = ImageDataFactory.Create(@"Imagenes/LogoAlvarez.png");

            iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imageData);
            pdfImg.SetHeight(250);
            pdfImg.SetFixedPosition(50, 200);

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("CANTIDAD")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)

                .Add(new Paragraph("DESCRIPCIÓN")));

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)

                .Add(new Paragraph("TOTAL")));


            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("CANTIDAD")));

            table.AddCell(new Cell(1, 2)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("DESCRIPCIÓN")));

            table.AddCell(new Cell()
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceGray(0.75f))
                .SetFont(f)
                .SetFontSize(fs)
                .Add(new Paragraph("TOTAL")));

            List<DetalleNota> _cotizacion = new List<DetalleNota>();
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _cotizacion = dbContext.DetalleNota.Where(c => c.id_nota == NotaElegida.id_nota).ToList();
            }

            foreach (DetalleNota item in _cotizacion)
            {
                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.descripcion)));

                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.importe.ToString())));

                table.AddCell(new Cell()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.cantidad.ToString())));

                table.AddCell(new Cell(1, 2)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.descripcion)));

                table.AddCell(new Cell()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(fs)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(item.importe.ToString())));
            }

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(10, 10, 0)
                .Add(new Paragraph("ESTOS PRECIO SON MÁS IVA")));

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(420, 10, 0)
                .Add(new Paragraph("ESTOS PRECIO SON MÁS IVA")));

            if (NotaElegida.total - totalAbonado == 0)
            {
                table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(250, 60, 0)
                .Add(new Paragraph(
                "TOTAL: " + NotaElegida.total.ToString()
                + "\nPAGADO")));
            }
            else
            {
                table.AddCell(new Cell(1, 4)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFont(f)
                    .SetFontSize(13)
                    .SetBold()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetFixedPosition(250, 60, 0)
                    .Add(new Paragraph("ABONADO: " + totalAbonado +
                    "\nRESTAN " + (NotaElegida.total - totalAbonado) +
                    "\nTOTAL: " + NotaElegida.total.ToString())));
            }

            table.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                );

            if (NotaElegida.total - totalAbonado == 0)
            {
                table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(650, 60, 0)
                .Add(new Paragraph(
                "TOTAL: " + NotaElegida.total.ToString()
                + "\nPAGADO")));
            }
            else
            {
                table.AddCell(new Cell(1, 4)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                .SetFont(f)
                .SetFontSize(13)
                .SetBold()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFixedPosition(650, 60, 0)
                .Add(new Paragraph("ABONADO: " + totalAbonado +
                "\nRESTAN " + (NotaElegida.total - totalAbonado) +
                "\nTOTAL: " + NotaElegida.total.ToString())));
            }

            document.Add(pdfImg);
            pdfImg.SetFixedPosition(480, 200);
            document.Add(pdfImg);

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
    }
}
